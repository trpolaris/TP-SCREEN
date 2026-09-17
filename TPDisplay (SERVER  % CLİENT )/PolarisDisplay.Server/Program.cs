using System.Buffers;
using System.Collections.Concurrent;
using System.ComponentModel;

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace PolarisDisplay.Server;

internal sealed class BrowserFrameData : IDisposable
{
    public byte[] Payload { get; }
    public int Width { get; }
    public int Height { get; }
    public int Fps { get; }
    public int Quality { get; }
    public long FrameId { get; }
    private Action? _release;

    public BrowserFrameData(byte[] payload, int width, int height, int fps, int quality, long frameId, Action? release = null)
    {
        Payload = payload;
        Width = width;
        Height = height;
        Fps = fps;
        Quality = quality;
        FrameId = frameId;
        _release = release;
    }

    public void Dispose() => Interlocked.Exchange(ref _release, null)?.Invoke();
}

internal sealed class BrowserFrameSubscription : IDisposable
{
    private readonly Func<CancellationToken, ValueTask<BrowserFrameData?>> _wait;
    private readonly Action _dispose;
    private int _disposed;

    public BrowserFrameSubscription(Func<CancellationToken, ValueTask<BrowserFrameData?>> wait, Action dispose)
    {
        _wait = wait;
        _dispose = dispose;
    }

    public ValueTask<BrowserFrameData?> WaitNextAsync(CancellationToken token) =>
        Volatile.Read(ref _disposed) != 0 ? ValueTask.FromResult<BrowserFrameData?>(null) : _wait(token);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            _dispose();
    }
}

internal static class Program
{

    [STAThread]
    [DllImport("user32.dll")]
    private static extern IntPtr SetProcessDpiAwarenessContext(IntPtr dpiContext);

    private static readonly IntPtr DpiAwarenessContextPerMonitorV2 = new(-4);

    static void Main()
    {
        using var serverMutex = new Mutex(true, "Global\\TRPOLARIS.PolarisDisplay.Server", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("PolarisDisplay Server zaten çalışıyor.", "PolarisDisplay", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Screen.Bounds/CopyFromScreen must use physical pixels. Without this,
        // a Windows display-mode/DPI transition can leave the capture surface
        // out of sync with the actual monitor.
        try { SetProcessDpiAwarenessContext(DpiAwarenessContextPerMonitorV2); } catch { }

        ApplicationConfiguration.Initialize();

        // Show the Server UI immediately. Driver/PnP initialization can take
        // several seconds on a cold start, so it runs after the window is ready
        // instead of blocking the WinForms message loop.
        var form = new ServerForm();
        form.Shown += (_, _) =>
        {
            _ = Task.Run(() => DriverBootstrap.EnsureInstalled())
                .ContinueWith(_ =>
                {
                    if (form.IsDisposed || form.Disposing) return;
                    try
                    {
                        form.BeginInvoke(new Action(form.RefreshScreensAfterDriverBootstrap));
                    }
                    catch { }
                }, TaskScheduler.Default);
        };

        Application.Run(form);
    }
}



internal static class DriverBootstrap
{
    private static readonly string[] DriverMarkers =
    {
        "Polaris", "IddSample", "Indirect Display", "INDIRECTDISPLAY",
        "Microsoft Indirect Display Adapter"
    };

    private static Process? _deviceAppProcess;
    private static bool _startedDeviceApp;
    private static string? _embeddedRuntimeDirectory;

    private static readonly (string FileName, string ResourceName)[] EmbeddedDriverFiles =
    {
        ("IddSampleDriver.inf", "PolarisDisplay.DriverRuntime.IddSampleDriver.inf"),
        ("IddSampleDriver.cat", "PolarisDisplay.DriverRuntime.IddSampleDriver.cat"),
        ("IddSampleDriver.dll", "PolarisDisplay.DriverRuntime.IddSampleDriver.dll"),
        ("IddSampleDriver.cer", "PolarisDisplay.DriverRuntime.IddSampleDriver.cer"),
        ("IddSampleApp.exe", "PolarisDisplay.DriverRuntime.IddSampleApp.exe")
    };

    public static void EnsureInstalled()
    {
        // Prefer the embedded runtime, but always fall back to the staged Driver\Runtime
        // tree. This is important during Visual Studio Debug runs and after a previous
        // build has left an older single-file resource set behind.
        string? embeddedRuntime = ExtractEmbeddedDriverRuntime();
        string? inf = embeddedRuntime is null ? null : Path.Combine(embeddedRuntime, "IddSampleDriver.inf");
        string? app = embeddedRuntime is null ? null : Path.Combine(embeddedRuntime, "IddSampleApp.exe");

        if (string.IsNullOrWhiteSpace(inf) || !File.Exists(inf))
            inf = FindBundledInf();
        if (string.IsNullOrWhiteSpace(app) || !File.Exists(app))
            app = FindBundledDeviceApp();

        if (inf is null || app is null)
        {
            MessageBox.Show(
                "PolarisDisplay driver runtime bulunamadı.\n\nDriver\\Runtime klasöründeki veya uygulamaya gömülü IddSampleDriver runtime dosyaları okunamadı.",
                "PolarisDisplay Driver",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        int displayCountBefore = GetPolarisDisplayCount();
        bool packageInstalled = IsDriverPackageInstalled();
        bool runtimeChanged = !IsRuntimeFingerprintCurrent(inf);
        bool needInstall = !packageInstalled || (runtimeChanged && displayCountBefore == 0);

        // Do not tear down an already healthy virtual-display stack merely because
        // Server was restarted. Reinstalling the IddCx package while Explorer/DWM is
        // using the virtual topology can transiently break Explorer. Only install
        // when the package is actually missing, or when no Polaris outputs exist.
        if (needInstall)
        {
            string? certificate = embeddedRuntime is null ? null : Path.Combine(embeddedRuntime, "IddSampleDriver.cer");
            if (string.IsNullOrWhiteSpace(certificate) || !File.Exists(certificate))
                certificate = FindBundledCertificate();

            if (!EnsureDriverCertificateTrusted(certificate))
            {
                MessageBox.Show(
                    "PolarisDisplay driver sertifikası Windows tarafından güvenilir hale getirilemedi.\n\nDriver kurulumu durduruldu.",
                    "PolarisDisplay Driver",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!TryInstallDriver(inf))
            {
                MessageBox.Show(
                    "PolarisDisplay driver kurulumu tamamlanamadı.\n\nSanal ekranlar başlatılmayacak.",
                    "PolarisDisplay Driver",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            SaveRuntimeFingerprint(inf);
            TryPnPUtil("/scan-devices");
        }

        // If a valid IddSampleApp is already running and the virtual outputs are
        // present, leave it alone. Otherwise start the bundled owner process.
        if (!HasDeviceAppRunning())
        {
            EnsureDeviceAppRunning(app);
            TryPnPUtil("/scan-devices");
        }
        WaitForPolarisDisplays();

        if (GetPolarisDisplayCount() == 0 && HasDeviceAppRunning())
        {
            TryPnPUtil("/scan-devices");
            WaitForPolarisDisplays();
        }
    }

    public static void Shutdown()
    {
        var p = _deviceAppProcess;
        _deviceAppProcess = null;

        if (!_startedDeviceApp || p is null)
        {
            try
            {
                foreach (var orphan in Process.GetProcessesByName("IddSampleApp"))
                {
                    try
                    {
                        if (!orphan.HasExited)
                        {
                            orphan.Kill(entireProcessTree: true);
                            orphan.WaitForExit(3000);
                        }
                    }
                    catch { }
                    finally { orphan.Dispose(); }
                }
            }
            catch { }
            CleanupEmbeddedDriverRuntime();
            return;
        }

        _startedDeviceApp = false;
        try
        {
            if (!p.HasExited)
            {
                p.Kill(entireProcessTree: true);
                p.WaitForExit(5000);
            }
        }
        catch { }
        finally
        {
            p.Dispose();
        }

        try
        {
            foreach (var orphan in Process.GetProcessesByName("IddSampleApp"))
            {
                try
                {
                    if (!orphan.HasExited)
                    {
                        orphan.Kill(entireProcessTree: true);
                        orphan.WaitForExit(3000);
                    }
                }
                catch { }
                finally { orphan.Dispose(); }
            }
        }
        catch { }

        CleanupEmbeddedDriverRuntime();
    }

    private static string? ExtractEmbeddedDriverRuntime()
    {
        try
        {
            var assembly = typeof(DriverBootstrap).Assembly;
            string root = Path.Combine(Path.GetTempPath(), "TRPOLARIS", "DriverRuntime", Environment.ProcessId.ToString());
            Directory.CreateDirectory(root);

            foreach (var (fileName, resourceName) in EmbeddedDriverFiles)
            {
                Stream? source = assembly.GetManifestResourceStream(resourceName);
                if (source is null)
                {
                    string? actualName = assembly.GetManifestResourceNames()
                        .FirstOrDefault(n => n.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase) ||
                                             n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrWhiteSpace(actualName))
                        source = assembly.GetManifestResourceStream(actualName);
                }

                if (source is null)
                    throw new FileNotFoundException("Gömülü driver kaynağı bulunamadı.", resourceName);

                using (source)
                {
                    string destination = Path.Combine(root, fileName);
                    using var target = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.Read);
                    source.CopyTo(target);
                }
            }

            _embeddedRuntimeDirectory = root;
            return root;
        }
        catch
        {
            _embeddedRuntimeDirectory = null;
            return null;
        }
    }

    private static void CleanupEmbeddedDriverRuntime()
    {
        string? directory = _embeddedRuntimeDirectory;
        _embeddedRuntimeDirectory = null;
        if (string.IsNullOrWhiteSpace(directory))
            return;

        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);

            string? parent = Path.GetDirectoryName(directory);
            if (!string.IsNullOrWhiteSpace(parent) && Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any())
                Directory.Delete(parent, recursive: false);
        }
        catch { }
    }

    private static void WaitForPolarisDisplays()
    {
        // Polaris is a three-output virtual topology. Give PnP/IddCx a few
        // seconds to expose all outputs before Server starts routing clients.
        for (int attempt = 0; attempt < 60 && GetPolarisDisplayCount() < 3; attempt++)
            Thread.Sleep(100);
    }

    private const string CodeSigningEkuOid = "1.3.6.1.5.5.7.3.3";

    private static bool EnsureDriverCertificateTrusted(string? certificatePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(certificatePath) || !File.Exists(certificatePath))
                throw new FileNotFoundException("IddSampleDriver.cer bulunamadı.", certificatePath);

            string? catalogPath = Path.Combine(Path.GetDirectoryName(certificatePath)!, "IddSampleDriver.cat");
            using var certificate = new X509Certificate2(certificatePath);

            // The driver is currently test-signed. Do not pin a hard-coded SHA-256
            // fingerprint here: rebuilding the driver can legitimately create a new
            // test certificate. Instead, verify the CER is self-signed, intended for
            // code signing, valid now, and (when possible) is actually embedded in
            // the matching catalog as a signer certificate.
            if (!string.Equals(certificate.Subject, certificate.Issuer, StringComparison.OrdinalIgnoreCase))
                throw new CryptographicException("Driver sertifikası self-signed değil.");

            if (DateTime.UtcNow < certificate.NotBefore.ToUniversalTime() ||
                DateTime.UtcNow > certificate.NotAfter.ToUniversalTime())
                throw new CryptographicException("Driver sertifikasının geçerlilik süresi dışında.");

            bool hasCodeSigningEku = certificate.Extensions
                .OfType<X509EnhancedKeyUsageExtension>()
                .SelectMany(e => e.EnhancedKeyUsages.Cast<Oid>())
                .Any(oid => string.Equals(oid.Value, CodeSigningEkuOid, StringComparison.Ordinal));

            if (!hasCodeSigningEku)
                throw new CryptographicException("Driver sertifikasında Code Signing yetkisi bulunmuyor.");

            if (File.Exists(catalogPath) && !CatalogContainsCertificate(catalogPath, certificate))
                throw new CryptographicException("IddSampleDriver.cer ile IddSampleDriver.cat içindeki imzacı sertifika eşleşmiyor. Driver/Runtime dosyalarını aynı build'den kullanın.");

            // Setup already imports the certificate, but doing it here as well makes
            // the Server package self-contained when the EXE is started directly.
            if (!AddCertificateToStore(certificatePath, "Root") ||
                !AddCertificateToStore(certificatePath, "TrustedPublisher"))
                return false;

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Driver sertifikası doğrulanamadı.\n\n" + ex.Message,
                "PolarisDisplay Driver",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }
    }

    private static bool CatalogContainsCertificate(string catalogPath, X509Certificate2 expected)
    {
        try
        {
            byte[] catalogBytes = File.ReadAllBytes(catalogPath);
            var certificates = new X509Certificate2Collection();
            certificates.Import(catalogBytes);

            string expectedThumbprint = expected.GetCertHashString(HashAlgorithmName.SHA256);
            return certificates.Cast<X509Certificate2>().Any(c =>
                string.Equals(c.GetCertHashString(HashAlgorithmName.SHA256), expectedThumbprint, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            // Some Windows/.NET combinations do not expose certificates from a
            // catalog through X509Certificate2Collection.Import. The structural
            // checks above are still valid, so do not make a good test package fail
            // solely because the catalog parser is unavailable.
            Debug.WriteLine("CAT sertifika listesi okunamadı: " + ex.Message);
            return true;
        }
    }

    private static bool AddCertificateToStore(string certificatePath, string storeName)
    {
        string certutil = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "certutil.exe");
        var psi = new ProcessStartInfo
        {
            FileName = certutil,
            Arguments = $"-addstore -f {storeName} \"{certificatePath}\"",
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(certificatePath)!,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.Unicode,
            StandardErrorEncoding = Encoding.Unicode
        };

        using var p = Process.Start(psi);
        if (p is null)
            return false;

        string output = p.StandardOutput.ReadToEnd();
        string error = p.StandardError.ReadToEnd();
        p.WaitForExit(15000);

        if (!p.HasExited || p.ExitCode != 0)
        {
            string details = string.IsNullOrWhiteSpace(error) ? output : error;
            Debug.WriteLine($"certutil -addstore {storeName} failed: {details}");
            return false;
        }

        return true;
    }

    private static bool TryInstallDriver(string inf)
    {
        try
        {
            string pnputil = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "pnputil.exe");
            var psi = new ProcessStartInfo
            {
                FileName = pnputil,
                Arguments = $"/add-driver \"{inf}\" /install",
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(inf)!,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            if (p is null)
                throw new InvalidOperationException("pnputil başlatılamadı.");

            p.WaitForExit(30000);

            if (!p.HasExited || p.ExitCode != 0)
                throw new InvalidOperationException($"Driver kurulumu başarısız oldu. pnputil exit code: {(p.HasExited ? p.ExitCode : -1)}");
            return true;
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            MessageBox.Show(
                "PolarisDisplay driver kurulumu için yönetici izni gerekiyor.\n\nUAC isteği iptal edildi.",
                "PolarisDisplay",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "PolarisDisplay driver otomatik kurulamadı.\n\n" + ex.Message,
                "PolarisDisplay",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }
    }

    private static string RuntimeFingerprintPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TRPolaris", "DriverRuntime.sha256");

    private static string CalculateRuntimeFingerprint(string inf)
    {
        using var sha = SHA256.Create();
        string directory = Path.GetDirectoryName(inf)!;
        var files = new[]
        {
            inf,
            Path.Combine(directory, "IddSampleDriver.dll"),
            Path.Combine(directory, "IddSampleDriver.cer"),
            Path.Combine(directory, "IddSampleDriver.cat")
        };
        using var ms = new MemoryStream();
        foreach (string file in files)
        {
            if (!File.Exists(file)) continue;
            byte[] bytes = File.ReadAllBytes(file);
            ms.Write(bytes, 0, bytes.Length);
        }
        return Convert.ToHexString(sha.ComputeHash(ms));
    }

    private static bool IsRuntimeFingerprintCurrent(string inf)
    {
        try
        {
            if (!File.Exists(RuntimeFingerprintPath)) return false;
            string directory = Path.GetDirectoryName(inf)!;
            string[] required =
            {
                inf,
                Path.Combine(directory, "IddSampleDriver.dll"),
                Path.Combine(directory, "IddSampleDriver.inf"),
                Path.Combine(directory, "IddSampleDriver.cer"),
                Path.Combine(directory, "IddSampleDriver.cat"),
                Path.Combine(directory, "IddSampleApp.exe")
            };
            if (required.Any(path => !File.Exists(path))) return false;

            string current = File.ReadAllText(RuntimeFingerprintPath).Trim();
            return current.Length == 64 && string.Equals(current, CalculateRuntimeFingerprint(inf), StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static void SaveRuntimeFingerprint(string inf)
    {
        try
        {
            string path = RuntimeFingerprintPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, CalculateRuntimeFingerprint(inf));
        }
        catch { }
    }

    private static bool IsDriverPackageInstalled()
    {
        try
        {
            string pnputil = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "pnputil.exe");
            var psi = new ProcessStartInfo
            {
                FileName = pnputil,
                Arguments = "/enum-drivers",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.Unicode
            };

            using var p = Process.Start(psi);
            if (p is null) return false;
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(10000);

            // pnputil output formatting/encoding can vary by Windows build.
            // Remove NULs so both UTF-16 and UTF-8 decoded output remains searchable.
            output = output.Replace("\0", string.Empty);

            return output.Contains("iddsampledriver.inf", StringComparison.OrdinalIgnoreCase)
                || output.Contains("TRPOLARIS", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static string? FindBundledCertificate()
    {
        try
        {
            string baseDir = AppContext.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(baseDir, "Driver", "Runtime", "IddSampleDriver.cer"),
                Path.Combine(baseDir, "IddSampleDriver.cer"),
                Path.Combine(baseDir, "..", "..", "..", "Driver", "Runtime", "IddSampleDriver.cer")
            };
            return candidates.FirstOrDefault(File.Exists);
        }
        catch { return null; }
    }

    private static string? FindBundledInf()
    {
        string baseDir = AppContext.BaseDirectory;

        // Release/Debug output keeps the Driver\Runtime tree. In our package the
        // actual INF lives one level deeper:
        // Driver\Runtime\IddSampleDriver\IddSampleDriver.inf
        string[] candidates =
        {
            Path.Combine(baseDir, "Driver", "Runtime", "IddSampleDriver.inf"),
            Path.Combine(baseDir, "Driver", "Runtime", "IddSampleDriver", "IddSampleDriver.inf"),
            Path.Combine(baseDir, "Driver", "IddSampleDriver", "IddSampleDriver.inf"),
            Path.Combine(baseDir, "Driver", "IddSampleDriver.inf"),
            Path.Combine(baseDir, "Driver", "PolarisDisplay.inf"),
            Path.Combine(baseDir, "PolarisDisplay.inf"),
            Path.Combine(baseDir, "IddSampleDriver.inf")
        };

        string? direct = candidates.FirstOrDefault(File.Exists);
        if (direct is not null)
            return direct;

        // Development fallback: locate the INF anywhere under Driver\Runtime.
        // This makes the bootstrap resilient to Visual Studio's LinkBase/output layout.
        try
        {
            string runtimeRoot = Path.Combine(baseDir, "Driver", "Runtime");
            if (Directory.Exists(runtimeRoot))
            {
                string? staged = Directory
                    .EnumerateFiles(runtimeRoot, "IddSampleDriver.inf", SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (staged is not null) return staged;
            }
        }
        catch { }

        // Visual Studio Debug output may not copy Driver\Runtime next to the EXE.
        // Resolve the same staged files from the solution root as a development fallback.
        string? solutionRoot = FindSolutionRoot(baseDir);
        if (solutionRoot is not null)
        {
            string[] devCandidates =
            {
                Path.Combine(solutionRoot, "Driver", "Runtime", "IddSampleDriver.inf"),
                Path.Combine(solutionRoot, "Driver", "IddSampleDriver", "IddSampleDriver.inf")
            };
            string? dev = devCandidates.FirstOrDefault(File.Exists);
            if (dev is not null) return dev;
        }

        return null;
    }

    private static string? FindBundledDeviceApp()
    {
        string baseDir = AppContext.BaseDirectory;
        string[] candidates =
        {
            Path.Combine(baseDir, "Driver", "Runtime", "IddSampleApp.exe"),
            Path.Combine(baseDir, "Driver", "IddSampleApp.exe"),
            Path.Combine(baseDir, "Driver", "Polaris-IDD-Project", "x64", "Debug", "IddSampleApp.exe"),
            Path.Combine(baseDir, "IddSampleApp.exe")
        };

        string? direct = candidates.FirstOrDefault(File.Exists);
        if (direct is not null)
            return direct;

        string? solutionRoot = FindSolutionRoot(baseDir);
        if (solutionRoot is not null)
        {
            string[] devCandidates =
            {
                Path.Combine(solutionRoot, "Driver", "Runtime", "IddSampleApp.exe"),
                Path.Combine(solutionRoot, "Driver", "Polaris-IDD-Project", "x64", "Debug", "IddSampleApp.exe"),
                Path.Combine(solutionRoot, "POLARIS-IDD", "IndirectDisplay", "x64", "Debug", "IddSampleApp.exe"),
                Path.Combine(solutionRoot, "IndirectDisplay", "x64", "Debug", "IddSampleApp.exe")
            };
            return devCandidates.FirstOrDefault(File.Exists);
        }

        return null;
    }

    private static string? FindSolutionRoot(string start)
    {
        try
        {
            DirectoryInfo? dir = new DirectoryInfo(start);
            for (int i = 0; i < 8 && dir is not null; i++, dir = dir.Parent)
            {
                if (File.Exists(Path.Combine(dir.FullName, "PolarisDisplay.sln")))
                    return dir.FullName;
            }
        }
        catch { }
        return null;
    }

    private static void StopStaleDeviceApps()
    {
        try
        {
            foreach (var process in Process.GetProcessesByName("IddSampleApp"))
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(5000);
                    }
                }
                catch { }
                finally { process.Dispose(); }
            }
        }
        catch { }
    }

    private static bool HasDeviceAppRunning()
    {
        try { return Process.GetProcessesByName("IddSampleApp").Any(p => !p.HasExited); }
        catch { return false; }
    }

    private static void EnsureDeviceAppRunning(string? appPath)
    {
        if (HasDeviceAppRunning())
            return;

        if (string.IsNullOrWhiteSpace(appPath) || !File.Exists(appPath))
            return;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = appPath,
                WorkingDirectory = Path.GetDirectoryName(appPath)!,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            _deviceAppProcess = Process.Start(psi);
            _startedDeviceApp = _deviceAppProcess is not null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            try
            {
                _deviceAppProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = appPath,
                    WorkingDirectory = Path.GetDirectoryName(appPath)!,
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                _startedDeviceApp = _deviceAppProcess is not null;
            }
            catch { }
        }
        catch { }
    }

    private static int GetPolarisDisplayCount()
    {
        int count = 0;
        try
        {
            foreach (var screen in Screen.AllScreens)
            {
                string text = screen.DeviceName;
                if (DriverMarkers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase)))
                {
                    count++;
                    continue;
                }

                var dd = new DisplayInterop.DISPLAY_DEVICE { cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>() };
                if (!DisplayInterop.EnumDisplayDevices(null, 0, ref dd, 0)) continue;
            }
        }
        catch { }

        // Use the same robust adapter/monitor test as HasPolarisDisplay, but
        // count distinct DISPLAYx devices rather than returning on the first.
        try
        {
            count = Screen.AllScreens.Count(IsPolarisDisplay);
        }
        catch { }
        return count;
    }

    internal static bool IsPolarisDisplay(Screen screen)
    {
        try
        {
            for (uint i = 0; ; i++)
            {
                var adapter = new DisplayInterop.DISPLAY_DEVICE { cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>() };
                if (!DisplayInterop.EnumDisplayDevices(null, i, ref adapter, 0)) break;
                if (!string.Equals(adapter.DeviceName, screen.DeviceName, StringComparison.OrdinalIgnoreCase)) continue;
                string adapterText = $"{adapter.DeviceName} {adapter.DeviceString} {adapter.DeviceID} {adapter.DeviceKey}";
                if (DriverMarkers.Any(m => adapterText.Contains(m, StringComparison.OrdinalIgnoreCase))) return true;

                var monitor = new DisplayInterop.DISPLAY_DEVICE { cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>() };
                for (uint m = 0; DisplayInterop.EnumDisplayDevices(screen.DeviceName, m, ref monitor, 0); m++)
                {
                    string monitorText = $"{monitor.DeviceName} {monitor.DeviceString} {monitor.DeviceID} {monitor.DeviceKey}";
                    if (DriverMarkers.Any(x => monitorText.Contains(x, StringComparison.OrdinalIgnoreCase))) return true;
                    monitor = new DisplayInterop.DISPLAY_DEVICE { cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>() };
                }
                return false;
            }
        }
        catch { }
        return false;
    }

    private static bool HasPolarisDisplay()
    {
        try
        {
            for (uint i = 0; i < 32; i++)
            {
                DisplayInterop.DISPLAY_DEVICE dd = new();
                dd.cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>();
                if (!DisplayInterop.EnumDisplayDevices(null, i, ref dd, 0)) break;
                string text = $"{dd.DeviceName} {dd.DeviceString} {dd.DeviceID}";
                if (DriverMarkers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            foreach (var screen in Screen.AllScreens)
            {
                string text = screen.DeviceName;
                if (DriverMarkers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
        }
        catch { }
        return false;
    }

    private static void TryPnPUtil(string args)
    {
        try
        {
            string pnputil = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "pnputil.exe");
            using var p = Process.Start(new ProcessStartInfo
            {
                FileName = pnputil,
                Arguments = args,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            });
            p?.WaitForExit(10000);
        }
        catch { }
    }
}

internal static class ServerAuth
{
    private static readonly object Sync = new();
    private static string _pin = string.Empty;
    public static void SetPin(string? pin) { lock (Sync) _pin = pin?.Trim() ?? string.Empty; }
    public static bool RequiresPin { get { lock (Sync) return _pin.Length > 0; } }
    public static bool ValidatePin(string supplied) { lock (Sync) return _pin.Length == 0 || string.Equals(_pin, supplied ?? string.Empty, StringComparison.Ordinal); }
}

internal static class Protocol
{
    public const byte Version = 9;
    public const int HeaderSize = 32;
    public const int MaxPayload = 32 * 1024 * 1024;
    public const byte KindJpeg = 1;
    public const byte KindControl = 2;

    public static void WriteHeader(
        Span<byte> b,
        int width,
        int height,
        int fps,
        long frameId,
        int payloadLength,
        string deviceName)
    {
        b.Clear();
        b[0] = (byte)'P'; b[1] = (byte)'J'; b[2] = (byte)'P'; b[3] = (byte)'9';
        b[4] = Version;
        b[5] = 1; // JPEG
        ushort token = ScreenToken(deviceName);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(b[6..8], token);

        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(b[8..12], width);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(b[12..16], height);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(b[16..20], fps);
        System.Buffers.Binary.BinaryPrimitives.WriteInt64BigEndian(b[20..28], frameId);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(b[28..32], payloadLength);
    }

    public static ushort ScreenToken(string deviceName)
    {
        unchecked
        {
            uint hash = 2166136261u;
            foreach (byte b in Encoding.UTF8.GetBytes(deviceName ?? string.Empty))
            {
                hash ^= b;
                hash *= 16777619u;
            }
            ushort token = (ushort)((hash ^ (hash >> 16)) & 0xFFFF);
            return token == 0 ? (ushort)1 : token;
        }
    }

    public static void WriteControlHeader(Span<byte> b, int payloadLength)
    {
        b.Clear();
        b[0] = (byte)'P'; b[1] = (byte)'J'; b[2] = (byte)'P'; b[3] = (byte)'9';
        b[4] = Version;
        b[5] = KindControl;
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(b[28..32], payloadLength);
    }
}
