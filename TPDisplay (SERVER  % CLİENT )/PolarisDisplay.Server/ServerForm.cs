using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;

namespace PolarisDisplay.Server;

public sealed partial class ServerForm : Form
{
    // Cached once per ServerForm type; used by the JPEG encoder without
    // crossing partial-class/type boundaries.
    private static readonly ImageCodecInfo? s_jpegCodec = ImageCodecInfo.GetImageEncoders()
        .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
    // Page controls are Designer-owned. Business logic references them through
    // these aliases and never recreates persistent UI at runtime.
    // Designer-owned page controls. These are assigned only after InitializeComponent()
    // so no property/field access can occur while the page fields are still null.
    private NeonPanel _settingsCard = null!;
    private Label _settingsTitle = null!;
    private NumericUpDown _port = null!;
    private NumericUpDown _fps = null!;
    private NumericUpDown _quality = null!;
    private NumericUpDown _bitrate = null!;
    private NeonComboBox _resolution = null!;
    private NeonComboBox _bitrateMode = null!;
    private NeonComboBox _screen = null!;
    private Button _refresh = null!;
    private NeonButton _start = null!;
    private Label _status = null!;
    private NeonPanel _statsPanel = null!;
    private Label _m1Value = null!;
    private Label _m2Value = null!;
    private Label _m3Value = null!;
    private Label _m4Value = null!;
    private Label _m5Value = null!;
    private FlowLayoutPanel _connectionsFlow = null!;
    private Label _connectionsEmpty = null!;
    private Label _categoryTitle = null!;
    private Label _categoryText = null!;
    private RichTextBox _log = null!;
    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HTCAPTION = 0x2;
    private readonly ConcurrentDictionary<string, byte> _activeClients = new();
    private readonly object _assignmentSync = new();
    private readonly Dictionary<string, string> _clientScreenAssignments = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _clientDisplayNames = new(StringComparer.OrdinalIgnoreCase);
    private const string ConnectionHistoryPathName = "ServerConnections.json";
    private readonly Dictionary<string, DateTime> _clientConnectedAt = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ConnectionHistoryItem> _connectionHistory = new();
    private string _lastEndpoint = "—";
    private DateTime _lastEndpointTime;
    private long _totalBytesSent;
    private long _lastStatsBytes;
    private DateTime _lastStatsUtc = DateTime.UtcNow;
    private TimeSpan _lastCpuTime;
    private DateTime _broadcastStartedUtc;
    private Process? _statsProcess;
    private System.Windows.Forms.Timer? _statsTimer;
    private System.Windows.Forms.Timer? _screenMonitorTimer;
    private string _screenSignature = string.Empty;
    private BrowserViewerServer? _browserViewer;
    private UdpClient? _discoveryUdp;
    private readonly ServerSettings _settings = ServerSettingsStore.Load();
    private int _activeTargetMbps = 8;
    private bool _allowClose;
    private readonly CancellationTokenSource _shutdownCts = new();
    private NotifyIcon? _trayIcon;
    private ContextMenuStrip? _trayMenu;
    private sealed record ConnectionHistoryItem(string Endpoint, DateTime ConnectedAt, bool IsBrowser);

    private bool _runtimeInitialized;

    public ServerForm()
    {
        // InitializeComponent owns the complete static UI. Runtime services are
        // intentionally initialized later so Visual Studio Designer can safely
        // construct and modify the form without depending on page aliases.
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (_runtimeInitialized || IsDesignMode())
            return;

        // Mark the runtime as initialized only after all Designer-owned controls
        // have been resolved successfully. This prevents a partial initialization
        // from permanently blocking a later retry.
        if (!InitializeRuntime())
            return;

        _runtimeInitialized = true;
    }

    private bool InitializeRuntime()
    {
        // InitializeComponent is the source of truth for the static UI.
        // A Designer edit can temporarily leave a page field out of the generated
        // method, especially while the form is being regenerated. Recover only the
        // missing page here; existing Designer instances and their geometry are never
        // replaced or repositioned.
        // All persistent pages and navigation controls are created by the
        // WinForms Designer. Runtime must never manufacture or reposition them.
        if (_broadcastPage is null || _connectionsPage is null ||
            _statisticsPage is null || _adminPage is null ||
            _aboutPage is null || _logPage is null ||
            _tabBroadcast is null || _tabConnections is null ||
            _tabStats is null || _tabAdmin is null ||
            _tabAbout is null || _tabLog is null)
        {
            Debug.WriteLine("ServerForm Designer UI is incomplete; runtime initialization skipped.");
            return false;
        }

        try
        {
            _settingsCard = _broadcastPage.SettingsCard;
            _settingsTitle = _broadcastPage.SettingsTitle;
            _port = _broadcastPage.Port;
            _fps = _broadcastPage.Fps;
            _quality = _broadcastPage.Quality;
            _bitrate = _broadcastPage.Bitrate;
            _resolution = _broadcastPage.Resolution;
            _bitrateMode = _broadcastPage.BitrateMode;
            _screen = _broadcastPage.Screen;
            _refresh = _broadcastPage.RefreshButton;
            _start = _broadcastPage.StartButton;
            _status = _broadcastPage.Status;
            _statsPanel = _broadcastPage.StatsPanel;
            _m1Value = _broadcastPage.M1Value;
            _m2Value = _broadcastPage.M2Value;
            _m3Value = _broadcastPage.M3Value;
            _m4Value = _broadcastPage.M4Value;
            _m5Value = _broadcastPage.M5Value;
            _connectionsFlow = _connectionsPage.ConnectionsFlow;
            _connectionsEmpty = _connectionsPage.ConnectionsEmpty;
            _categoryTitle = _aboutPage.CategoryTitle;
            _categoryText = _aboutPage.CategoryText;
            _log = _logPage.Log;
        }
        catch (NullReferenceException)
        {
            // A partially regenerated Designer page is not allowed to crash the
            // application. The next load pass can rebuild only the missing page.
            return false;
        }

        ApplySavedSettings();
        PolarisTheme.Apply(this);
        KeyPreview = true;
        KeyDown += (_, e) => { if (e.KeyCode == Keys.F10) ShowSettings(); };
        _tabBroadcast.Click += (_, _) => SelectTab(0);
        _tabConnections.Click += (_, _) => SelectTab(1);
        _tabStats.Click += (_, _) => SelectTab(2);
        _tabAdmin.Click += (_, _) => SelectTab(3);
        _tabAbout.Click += (_, _) => SelectTab(4);
        _tabLog.Click += (_, _) => SelectTab(5);
        SelectTab(0);
        _bitrateMode.SelectedIndexChanged += (_, _) => _bitrate.Enabled = _bitrateMode.SelectedIndex == 1;
        _refresh.Click += (_, _) => RefreshScreens();
        _screen.MouseUp += (_, e) =>
        {
            if (e.Button != MouseButtons.Right || _screen.SelectedItem is not ScreenItem si) return;
            var menu = new ContextMenuStrip();
            menu.Items.Add("Çözünürlük / Hz / Yön...", null, (_, _) => ShowDisplayModeDialog(si.Screen));
            menu.Show(_screen, e.Location);
        };
        _start.Click += async (_, _) => { SaveUiSettings(); await ToggleServerAsync(); };
        _resolution.SelectedIndexChanged += (_, _) => UpdateQualityRecommendation();
        _screen.SelectedIndexChanged += (_, _) => BuildResolutions();
        _titlePanel.DoubleClick += (_, _) => ToggleMaximize();
        _titlePanel.MouseDown += DragWindow;
        _logoBox.MouseDown += DragWindow;
        _mainHeader.MouseDown += DragWindow;
        _mainTitle.MouseDown += DragWindow;
        _mainSubtitle.MouseDown += DragWindow;
        _lastClientLabel.MouseDown += DragWindow;
        _statusBadge.MouseDown += DragWindow;
        InitializeTraySupport();
        FormClosing += ServerForm_FormClosing;
        _adminPage.SaveSettings += (_, _) => SaveAdminPageSettings();
        _adminPage.RefreshQr += (_, _) => RefreshAdminQr();
        _adminPage.CheckUpdate += async (_, _) => await CheckForServerUpdateFromAdminAsync();
        _connectionsPage.ClearHistoryRequested += (_, _) => ClearConnectionHistory();
        Shown += ServerForm_Shown;
        return true;
    }



    private void MinimizeButton_Click(object? sender, EventArgs e) => MinimizeToTray();
    private void MaximizeButton_Click(object? sender, EventArgs e) => ToggleMaximize();
    private void CloseButton_Click(object? sender, EventArgs e) => Close();

    private void SocialOpen(object? sender, EventArgs e)
    {
        if (sender is PictureBox { Tag: string url } && Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            try
            {
                Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
            }
            catch
            {
                // Ignore browser launch failures.
            }
        }
    }

    private void SocialEnter(object? sender, EventArgs e)
    {
        if (sender is PictureBox icon)
            icon.BackColor = Color.FromArgb(28, 0, 180, 255);
    }

    private void SocialLeave(object? sender, EventArgs e)
    {
        if (sender is PictureBox icon)
            icon.BackColor = Color.Transparent;
    }

    private void ServerForm_Shown(object? sender, EventArgs e)
    {
        Shown -= ServerForm_Shown;
        // Let the first frame of the window paint before starting I/O, discovery,
        // browser and telemetry services. This keeps cold-start responsive.
        BeginInvoke((Action)(() =>
        {
            if (IsDisposed || Disposing) return;
            LoadConnectionHistory();
            RefreshScreens();
            _browserViewer = new BrowserViewerServer(GetBrowserScreens, ResolveBrowserScreen, Math.Clamp(_settings.WebPort, 1024, 65535), RegisterBrowserClient, UnregisterBrowserClient, HandleRemoteClientCommand, () => _listener is not null, UpdateBrowserTelemetry, CreateBrowserFrameSubscription);
            try
            {
                _browserViewer.Start();
                _settingsTitle.Text = $"⚙  Yayın   •   Web: {_browserViewer.Port}";
                Log($"Web görüntüleyici: http://localhost:{_browserViewer.Port}/");
            }
            catch (Exception ex)
            {
                Log("Browser görüntüleyici başlatılamadı: " + ex.Message);
            }
            StartLanDiscovery();
            _statsProcess = Process.GetCurrentProcess();
            _lastCpuTime = _statsProcess.TotalProcessorTime;
            _lastStatsUtc = DateTime.UtcNow;
            _statsTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _statsTimer.Tick += (_, _) => UpdateRealtimeStats();
            _statsTimer.Start();
            _screenMonitorTimer = new System.Windows.Forms.Timer { Interval = 400 };
            _screenMonitorTimer.Tick += (_, _) => RefreshScreensIfChanged();
            _screenMonitorTimer.Start();
            RefreshAdminPage();
        }));
    }


    private void RefreshScreensIfChanged()
    {
        if (IsDisposed || Disposing) return;

        var currentSignature = string.Join("|", Screen.AllScreens
            .Where(IsPolarisVirtualScreen)
            .OrderBy(s => s.DeviceName, StringComparer.OrdinalIgnoreCase)
            .Select(s => $"{s.DeviceName}:{s.Bounds.X},{s.Bounds.Y},{s.Bounds.Width},{s.Bounds.Height}:{s.Primary}"));

        if (string.Equals(currentSignature, _screenSignature, StringComparison.Ordinal) &&
            _screen.Items.Count == Screen.AllScreens.Count(IsPolarisVirtualScreen))
            return;

        _screenSignature = currentSignature;
        RefreshScreens();
        RefreshAdminPage();
    }

    internal void RefreshScreensAfterDriverBootstrap()
    {
        if (IsDisposed || Disposing) return;
        RefreshScreens();
        RefreshAdminPage();
    }

    private void InitializeTraySupport()
    {
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Server'ı Aç", null, (_, _) => RestoreFromTray());
        _trayMenu.Items.Add("Kapat", null, (_, _) =>
        {
            _allowClose = true;
            Close();
        });

        _trayIcon = new NotifyIcon
        {
            Text = "TRPOLARIS Server",
            ContextMenuStrip = _trayMenu
        };
        _trayIcon.Icon = Icon ?? SystemIcons.Application;
        _trayIcon.Visible = true;
        _trayIcon.DoubleClick += (_, _) => RestoreFromTray();
    }

    private void MinimizeToTray()
    {
        ShowInTaskbar = false;
        Hide();
    }

    private void RestoreFromTray()
    {
        ShowInTaskbar = true;
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();
    }

    private void ServerForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            var result = ShowCloseChoiceDialog();
            if (result == DialogResult.No)
            {
                e.Cancel = true;
                MinimizeToTray();
                return;
            }
            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            _allowClose = true;
        }

        _trayIcon?.Dispose();
        _trayMenu?.Dispose();
        _statsTimer?.Stop();
        _statsTimer?.Dispose();
        _screenMonitorTimer?.Stop();
        _screenMonitorTimer?.Dispose();
        StopServer();
        _browserViewer?.Dispose();
        _discoveryUdp?.Dispose();
        try { _shutdownCts.Cancel(); } catch { }
        try { _shutdownCts.Dispose(); } catch { }
        DriverBootstrap.Shutdown();
        Application.ExitThread();
    }

    private DialogResult ShowCloseChoiceDialog()
    {
        using var dialog = new CloseChoiceDialogForm();
        return dialog.ShowDialog(this);
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    private void ApplySavedSettings()
    {
        _port.Value = Math.Clamp(_settings.Port, (int)_port.Minimum, (int)_port.Maximum);
        _fps.Value = Math.Clamp(_settings.Fps, (int)_fps.Minimum, (int)_fps.Maximum);
        _quality.Value = Math.Clamp(_settings.Quality, (int)_quality.Minimum, (int)_quality.Maximum);
        _bitrate.Value = Math.Clamp(_settings.BitrateMbps, (int)_bitrate.Minimum, (int)_bitrate.Maximum);
        _bitrateMode.SelectedIndex = _settings.AutoBitrate ? 0 : 1;
        ServerAuth.SetPin(_settings.Pin);
    }

    private void SaveUiSettings()
    {
        _settings.Port = (int)_port.Value; _settings.Fps = (int)_fps.Value; _settings.Quality = (int)_quality.Value;
        _settings.BitrateMbps = (int)_bitrate.Value; _settings.AutoBitrate = _bitrateMode.SelectedIndex == 0;
        ServerSettingsStore.Save(_settings); ServerAuth.SetPin(_settings.Pin);
    }

    private void RefreshAdminPage()
    {
        _adminPage.SetSettings(_settings);
        RefreshAdminQr();
    }

    private void SaveAdminPageSettings()
    {
        _settings.Pin = _adminPage.PinEnabled ? _adminPage.Pin.Trim() : string.Empty;
        _settings.UpdateManifestUrl = _adminPage.UpdateUrl.Trim();
        ServerSettingsStore.Save(_settings);
        ServerAuth.SetPin(_settings.Pin);
        _adminPage.SetStatus(_settings.Pin.Length == 0 ? "PIN koruması kapalı" : "PIN koruması aktif");
    }


    private void RefreshAdminQr()
    {
        try
        {
            string host = GetBestLanAddress();
            string url = QrPayloadBuilder.ViewerUrl(host, _browserViewer?.Port ?? _settings.WebPort);
            _adminPage.SetQr(url, CreateQrBitmap(url));
        }
        catch (Exception ex) { _adminPage.SetStatus("QR oluşturulamadı: " + ex.Message); }
    }

    private static string GetBestLanAddress()
    {
        try
        {
            var local = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a));
            return local?.ToString() ?? "127.0.0.1";
        }
        catch { return "127.0.0.1"; }
    }

    private static Bitmap CreateQrBitmap(string payload)
    {
        using var generator = new QRCoder.QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCoder.QRCodeGenerator.ECCLevel.M);
        using var code = new QRCoder.PngByteQRCode(data);
        byte[] png = code.GetGraphic(8);
        using var ms = new MemoryStream(png);
        using var temp = new Bitmap(ms);
        return new Bitmap(temp);
    }

    private async Task CheckForServerUpdateFromAdminAsync()
    {
        _settings.UpdateManifestUrl = _adminPage.UpdateUrl.Trim();
        ServerSettingsStore.Save(_settings);
        if (string.IsNullOrWhiteSpace(_settings.UpdateManifestUrl))
        {
            _adminPage.SetUpdateStatus("Manifest URL girilmedi.");
            return;
        }
        var m = await ServerUpdateService.CheckAsync(_settings.UpdateManifestUrl);
        if (m is null) { _adminPage.SetUpdateStatus("Güncelleme kontrolü başarısız."); return; }
        _adminPage.SetUpdateStatus(ServerUpdateService.IsNewer(ServerUpdateService.Version, m.Version) ? $"Yeni sürüm: {m.Version}" : $"Güncel: {ServerUpdateService.Version}");
        if (ServerUpdateService.IsNewer(ServerUpdateService.Version, m.Version) && Uri.TryCreate(m.Url, UriKind.Absolute, out var uri))
        {
            var r = MessageBox.Show(this, $"Yeni sürüm bulundu: {m.Version}\n\n{m.Notes}\n\nİndirme sayfası açılsın mı?", "PolarisDisplay Güncelleme", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (r == DialogResult.Yes) Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
        }
    }

    private void ShowSettings()
    {
        if (_listener is not null) return;
        if (!ServerSettingsDialog.Show(this, _settings)) return;
        ApplySavedSettings();
        ServerAuth.SetPin(_settings.Pin);
        _settings.WebPort = Math.Clamp(_settings.WebPort, 1024, 65535);
        _settings.WebPort = _browserViewer?.Port ?? _settings.WebPort;
        if (!string.IsNullOrWhiteSpace(_settings.UpdateManifestUrl))
            _ = CheckForServerUpdateAsync();
    }

    private async Task CheckForServerUpdateAsync()
    {
        var m = await ServerUpdateService.CheckAsync(_settings.UpdateManifestUrl);
        if (m is null) return;
        if (ServerUpdateService.IsNewer(ServerUpdateService.Version, m.Version))
        {
            if (InvokeRequired) { BeginInvoke(new Action(async () => await CheckForServerUpdateAsync())); return; }
            var r = MessageBox.Show(this, $"Yeni PolarisDisplay sürümü bulundu: {m.Version}\n\n{m.Notes}\n\nİndirme sayfasını açmak ister misiniz?", "PolarisDisplay Güncelleme", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (r == DialogResult.Yes && Uri.TryCreate(m.Url, UriKind.Absolute, out var uri)) Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
        }
    }

    private void StartLanDiscovery()
    {
        try
        {
            _discoveryUdp ??= ServerDiscovery.CreateListener();
            _ = Task.Run(() => ServerDiscovery.RunAsync(_discoveryUdp, () => (int)_port.Value, () => _browserViewer?.Port ?? 8765, () => _listener is not null, _shutdownCts.Token));
            Log($"LAN keşfi aktif • UDP {ServerDiscovery.DiscoveryPort}");
        }
        catch (Exception ex) { Log("LAN keşfi başlatılamadı: " + ex.Message); }
    }

    private void UpdateRealtimeStats()
    {
        if (IsDisposed) return;
        var now = DateTime.UtcNow;
        double seconds = Math.Max(0.001, (now - _lastStatsUtc).TotalSeconds);
        long bytes = Interlocked.Read(ref _totalBytesSent);
        double mbps = (bytes - _lastStatsBytes) * 8d / seconds / 1_000_000d;
        _lastStatsBytes = bytes;
        _lastStatsUtc = now;

        double cpu = 0;
        try
        {
            var p = _statsProcess ??= Process.GetCurrentProcess();
            var current = p.TotalProcessorTime;
            cpu = Math.Clamp((current - _lastCpuTime).TotalSeconds / seconds / Math.Max(1, Environment.ProcessorCount) * 100d, 0, 100);
            _lastCpuTime = current;
        }
        catch { }

        long memMb = 0;
        try { memMb = ((_statsProcess ?? Process.GetCurrentProcess()).WorkingSet64 + 524_288) / 1_048_576; } catch { }
        TimeSpan uptime = _broadcastStartedUtc == default ? TimeSpan.Zero : now - _broadcastStartedUtc;

        _m1Value.Text = _activeClients.Count.ToString();
        _m2Value.Text = $"{mbps:0.0} Mbps";
        _m3Value.Text = uptime.ToString(@"hh\:mm\:ss");
        _m4Value.Text = $"{cpu:0.0}%";
        _m5Value.Text = $"{memMb} MB";
        _statisticsPage.SetServerStats(_activeClients.Count, mbps, uptime, cpu, memMb, _clientTelemetry);
    }

    private void UpdateBrowserTelemetry(string clientId, double fps, double mbps, int quality, int width, int height)
    {
        string key = "WEB • " + clientId;
        _clientTelemetry.AddOrUpdate(key,
            new ClientTelemetryState(-1, "WEB", fps, mbps, quality, width, height),
            (_, old) => old with { Fps = fps, Mbps = mbps, Quality = quality, Width = width, Height = height, Mode = "WEB" });
    }

    private void RegisterBrowserClient(string clientId, string endpoint, string deviceName)
    {
        string key = "WEB • " + clientId;
        lock (_assignmentSync)
            _clientDisplayNames[key] = endpoint;
        _clientTelemetry[key] = new ClientTelemetryState(-1, "WEB", 0, 0, 0, 0, 0);
        RegisterClient(key, endpoint);
        SetClientScreen(key, deviceName);
    }

    private void UnregisterBrowserClient(string clientId)
    {
        string key = "WEB • " + clientId;
        _clientTelemetry.TryRemove(key, out _);
        UnregisterClient(key);
    }

    private void RegisterClient(string endpoint, string? displayName = null)
    {
        DateTime now = DateTime.Now;
        bool wasPresent = _activeClients.ContainsKey(endpoint);
        _activeClients[endpoint] = 0;
        lock (_assignmentSync)
        {
            _clientConnectedAt[endpoint] = now;
            if (!string.IsNullOrWhiteSpace(displayName)) _clientDisplayNames[endpoint] = displayName!;
            _connectionHistory.RemoveAll(x => string.Equals(x.Endpoint, endpoint, StringComparison.OrdinalIgnoreCase));
            _connectionHistory.Insert(0, new ConnectionHistoryItem(endpoint, now, endpoint.StartsWith("WEB • ", StringComparison.OrdinalIgnoreCase)));
            if (_connectionHistory.Count > 50) _connectionHistory.RemoveRange(50, _connectionHistory.Count - 50);
            SaveConnectionHistoryUnsafe();
        }
        if (!wasPresent) Log($"Yeni bağlantı: {GetConnectionLabel(endpoint)}");
        SetStatus($"{_activeClients.Count} uzak ekran bağlı");
        _lastEndpoint = endpoint;
        _lastEndpointTime = now;
        RefreshHeaderConnectionSummary();
        RebuildConnectionsView();
    }

    private string GetConnectionLabel(string endpoint)
    {
        lock (_assignmentSync)
        {
            if (endpoint.StartsWith("WEB • ", StringComparison.OrdinalIgnoreCase))
            {
                if (_clientDisplayNames.TryGetValue(endpoint, out var browserEndpoint) && !string.IsNullOrWhiteSpace(browserEndpoint))
                    return $"WEB • {browserEndpoint}";
                return $"WEB • {endpoint[6..]}";
            }
            if (_clientDisplayNames.TryGetValue(endpoint, out var name) && !string.IsNullOrWhiteSpace(name))
                return $"PC • {name} • {endpoint}";
        }
        return endpoint;
    }

    private void RefreshHeaderConnectionSummary()
    {
        if (_lastClientLabel is null) return;
        int count = _activeClients.Count;
        string text;
        if (count > 0)
        {
            text = count == 1
                ? $"Bağlı: {GetConnectionLabel(_activeClients.Keys.First())}"
                : $"Bağlı bilgisayar: {count}  •  Son: {GetConnectionLabel(_lastEndpoint)}";
        }
        else
        {
            text = _lastEndpoint == "—"
                ? "Bağlantı bekleniyor"
                : $"Son: {GetConnectionLabel(_lastEndpoint)} • {_lastEndpointTime:HH:mm:ss}";
        }

        // Keep the header summary inside its Designer-owned slot; never let
        // a long endpoint string visually overlap the status badge.
        using var font = _lastClientLabel.Font;
        int maxWidth = Math.Max(40, _lastClientLabel.ClientSize.Width - 4);
        if (TextRenderer.MeasureText(text, font).Width > maxWidth)
        {
            const string ellipsis = "…";
            while (text.Length > 1 && TextRenderer.MeasureText(text + ellipsis, font).Width > maxWidth)
                text = text[..^1];
            text += ellipsis;
        }
        _lastClientLabel.Text = text;
    }

    private void UnregisterClient(string endpoint)
    {
        _activeClients.TryRemove(endpoint, out _);
        lock (_assignmentSync) { _clientConnectedAt.Remove(endpoint); _clientScreenAssignments.Remove(endpoint); }
        SetStatus(_activeClients.Count > 0 ? $"{_activeClients.Count} uzak ekran bağlı" : "Bağlantı bekleniyor...");
        RefreshHeaderConnectionSummary();
        RebuildConnectionsView();
    }

    private string ConnectionHistoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TRPolaris", ConnectionHistoryPathName);


    private void LoadConnectionHistory()
    {
        try
        {
            if (!File.Exists(ConnectionHistoryPath)) return;
            var list = JsonSerializer.Deserialize<List<ConnectionHistoryItem>>(File.ReadAllText(ConnectionHistoryPath));
            if (list is null) return;
            lock (_assignmentSync)
            {
                _connectionHistory.Clear();
                _connectionHistory.AddRange(list.Take(50));
                if (_connectionHistory.Count > 0) { _lastEndpoint = _connectionHistory[0].Endpoint; _lastEndpointTime = _connectionHistory[0].ConnectedAt; }
            }
        }
        catch { }
    }

    private void SaveConnectionHistoryUnsafe()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConnectionHistoryPath)!);
            File.WriteAllText(ConnectionHistoryPath, JsonSerializer.Serialize(_connectionHistory));
        }
        catch { }
    }

    private void ClearConnectionHistory()
    {
        lock (_assignmentSync) _connectionHistory.Clear();
        SaveConnectionHistoryUnsafe();
        RebuildConnectionsView();
        RefreshHeaderConnectionSummary();
    }

    private void HandleRemoteClientCommand(string endpoint, string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return;
        try
        {
            var p = command.Trim().Split('|');
            if (p.Length < 2) return;
            if (p[0].Equals("POLARIS_AUTH", StringComparison.OrdinalIgnoreCase))
            {
                string supplied = p.Length > 1 ? p[1].Trim() : string.Empty;
                if (ServerAuth.RequiresPin && supplied.Length == 0)
                {
                    _pendingControlCommands[endpoint] = JsonSerializer.Serialize(new
                    {
                        type = "authRequired",
                        message = "PIN gerekli"
                    });
                    return;
                }

                bool ok = ServerAuth.ValidatePin(supplied);
                if (ok && _clientConnectionGenerations.TryGetValue(endpoint, out var authGeneration))
                    _authenticatedClients[endpoint] = authGeneration;
                else if (!ok)
                    _authenticatedClients.TryRemove(endpoint, out _);
                if (_clientAuthWaiters.TryGetValue(endpoint, out var waiter)) waiter.TrySetResult(ok);
                _pendingControlCommands[endpoint] = JsonSerializer.Serialize(new
                {
                    type = "authResult",
                    success = ok,
                    message = ok ? "PIN doğrulandı" : "PIN doğrulaması başarısız"
                });
                return;
            }

            // Authentication is connection-scoped. No HELLO/MODE/SELECT/PING command
            // is accepted until the PIN handshake has succeeded.
            if (!_clientConnectionGenerations.TryGetValue(endpoint, out var generation) ||
                !_authenticatedClients.TryGetValue(endpoint, out var authenticatedGeneration) ||
                authenticatedGeneration != generation)
                return;

            if (p[0].Equals("POLARIS_HELLO", StringComparison.OrdinalIgnoreCase))
            {
                string name = p.Length > 1 && !string.IsNullOrWhiteSpace(p[1]) ? p[1] : "PC";
                lock (_assignmentSync) _clientDisplayNames[endpoint] = name.Trim();
                RebuildConnectionsView(); RefreshHeaderConnectionSummary();
                return;
            }

            if (p[0].Equals("POLARIS_PING", StringComparison.OrdinalIgnoreCase) && p.Length >= 2)
            {
                _pendingControlCommands[endpoint] = JsonSerializer.Serialize(new { type = "pong", sentTicks = p[1] });
                return;
            }

            if (p[0].Equals("POLARIS_PING_RESULT", StringComparison.OrdinalIgnoreCase) && p.Length >= 2 && int.TryParse(p[1], out var reportedPing))
            {
                int ping = Math.Clamp(reportedPing, 0, 60_000);
                _clientTelemetry.AddOrUpdate(endpoint, new ClientTelemetryState(ping, "Düşük Gecikme", 0, 0, 0, 0, 0), (_, old) => old with { PingMs = ping });
                return;
            }

            if (p[0].Equals("POLARIS_MODE", StringComparison.OrdinalIgnoreCase))
            {
                string mode = p.Length > 1 ? Uri.UnescapeDataString(p[1]) : "Düşük Gecikme";
                _clientTelemetry.AddOrUpdate(endpoint, new ClientTelemetryState(-1, mode, 0, 0, 0, 0, 0), (_, old) => old with { Mode = mode });
                return;
            }

            if (p[0].Equals("POLARIS_SELECT", StringComparison.OrdinalIgnoreCase) && p.Length >= 2)
            {
                var result = TrySetClientScreen(endpoint, p[1].Trim());
                _pendingControlCommands[endpoint] = JsonSerializer.Serialize(new
                {
                    type = "screenResult",
                    success = result.Success,
                    message = result.Message,
                    deviceName = result.DeviceName,
                    rotation = GetScreenRotationByDeviceName(result.DeviceName)
                });
                return;
            }

        }
        catch (Exception ex) { Log("Uzak giriş hatası: " + ex.Message); }
    }

    private void RegisterBytes(long bytes) => Interlocked.Add(ref _totalBytesSent, bytes);

    private async Task<List<Screen>> WaitForRemoteScreensAsync(CancellationToken token)
    {
        // The IddCx monitors can appear in the Windows display topology a moment
        // after the Server process starts. Poll the topology before assigning a
        // Client so the first clients do not accidentally receive the primary
        // physical monitor.
        for (int attempt = 0; attempt < 30 && !token.IsCancellationRequested; attempt++)
        {
            var screens = GetRemoteScreens();
            if (screens.Count > 0)
            {
                Log($"Polaris sanal ekranları algılandı: {string.Join(", ", screens.Select((x, i) => $"Ekran {i + 1}={x.DeviceName} {x.Bounds.Width}×{x.Bounds.Height}"))}");
                return screens;
            }
            await Task.Delay(100, token);
        }
        return GetRemoteScreens();
    }

    private async Task<Screen?> SelectRemoteScreenAsync(string endpoint, Screen fallback, CancellationToken token)
    {
        // Extended-display mode is mandatory for multi-client operation.
        // Never silently fall back to the same physical monitor for every client.
        var remotes = await WaitForRemoteScreensAsync(token);

        lock (_assignmentSync)
        {
            if (remotes.Count > 0)
            {
                // Only reuse a previous assignment when it is still one of the
                // current Polaris virtual outputs. Old versions could have
                // stored the physical primary monitor here; never let that
                // stale assignment override automatic virtual-screen routing.
                if (_clientScreenAssignments.TryGetValue(endpoint, out string? oldDevice) &&
                    remotes.Any(s => string.Equals(s.DeviceName, oldDevice, StringComparison.OrdinalIgnoreCase)))
                {
                    var old = remotes.First(s => string.Equals(s.DeviceName, oldDevice, StringComparison.OrdinalIgnoreCase));
                    return old;
                }
                var activeDevices = _activeClients.Keys
                    .Where(k => !string.Equals(k, endpoint, StringComparison.OrdinalIgnoreCase))
                    .Select(k => _clientScreenAssignments.TryGetValue(k, out var d) ? d : null)
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var selected = remotes.FirstOrDefault(s => !activeDevices.Contains(s.DeviceName));
                if (selected is not null)
                {
                    _clientScreenAssignments[endpoint] = selected.DeviceName;
                    Log($"{endpoint} → {GetScreenDisplayName(selected)} ({selected.Bounds.Width}×{selected.Bounds.Height})");
                    RebuildConnectionsView();
                    return selected;
                }

                Log($"{endpoint} bağlantısı reddedildi: tüm Polaris uzak ekranları kullanımda.");
                return null;
            }

            // Do not silently stream the primary/physical screen when the
            // virtual topology is unavailable. That was the old behaviour that
            // made every Client show the main desktop. The caller rejects the
            // connection and the log tells the user exactly why.
            Log($"{endpoint} bağlantısı reddedildi: Polaris sanal ekranları Windows görüntü topolojisinde bulunamadı.");
            return null;
        }
    }

    private Screen? ResolveClientScreen(string endpoint, Screen fallback)
    {
        lock (_assignmentSync)
        {
            if (_clientScreenAssignments.TryGetValue(endpoint, out string? deviceName))
            {
                var assigned = Screen.AllScreens.FirstOrDefault(s => string.Equals(s.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));
                if (assigned is not null) return assigned;
            }
        }
        return fallback;
    }

    private (bool Success, string Message, string DeviceName) TrySetClientScreen(string endpoint, string deviceName)
    {
        var screen = Screen.AllScreens.FirstOrDefault(s =>
            string.Equals(s.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));

        if (screen is null || !IsPolarisVirtualScreen(screen))
            return (false, "Ekran bulunamadı.", string.Empty);

        lock (_assignmentSync)
        {
            bool occupied = _activeClients.Keys.Any(other =>
                !string.Equals(other, endpoint, StringComparison.OrdinalIgnoreCase) &&
                _clientScreenAssignments.TryGetValue(other, out var assigned) &&
                string.Equals(assigned, screen.DeviceName, StringComparison.OrdinalIgnoreCase));

            if (occupied)
            {
                string label = GetScreenDisplayName(screen);
                Log($"{endpoint} → {label} zaten başka bağlantıda.");
                return (false, $"{label} şu anda kullanılıyor", screen.DeviceName);
            }

            _clientScreenAssignments[endpoint] = screen.DeviceName;
        }

        string name = GetScreenDisplayName(screen);
        Log($"{endpoint} → {name} ({screen.Bounds.Width}×{screen.Bounds.Height})");

        if (endpoint.StartsWith("WEB • ", StringComparison.OrdinalIgnoreCase))
        {
            _browserViewer?.SetScreen(endpoint[6..], screen.DeviceName);
        }
        // TCP Clients consume the authoritative screenResult above. Their stream
        // loop watches _clientScreenAssignments and swaps the shared producer without
        // injecting a duplicate screen command into the same socket.
        else
        {
            _pendingScreenCommands.TryRemove(endpoint, out _);
        }

        try { if (!IsDisposed) BeginInvoke((Action)RebuildConnectionsView); } catch { }
        return (true, $"{name} ekranına geçildi", screen.DeviceName);
    }

    private void SetClientScreen(string endpoint, string deviceName)
    {
        TrySetClientScreen(endpoint, deviceName);
    }

    private static string GetScreenDisplayName(Screen screen)
    {
        // Polaris screens are a logical set of the driver's three virtual
        // outputs. Do not expose Windows DISPLAYxx numbering here: Windows
        // can assign large/reused numbers such as DISPLAY45, DISPLAY46...
        // Keep the UI stable as Ekran 1/2/3 and never show physical monitors.
        if (!IsPolarisVirtualScreen(screen))
            return "Polaris Ekran";

        var remotes = GetRemoteScreens();
        int index = remotes.FindIndex(s => string.Equals(s.DeviceName, screen.DeviceName, StringComparison.OrdinalIgnoreCase));
        return index >= 0 ? $"Ekran {index + 1}" : "Polaris Ekran";
    }

    private static int GetWindowsDisplayNumber(string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName)) return 0;
        int marker = deviceName.LastIndexOf("DISPLAY", StringComparison.OrdinalIgnoreCase);
        if (marker < 0) return 0;
        string suffix = deviceName[(marker + 7)..].Trim();
        return int.TryParse(suffix, out int number) && number > 0 ? number : 0;
    }

    private void RebuildConnectionsView()
    {
        if (_connectionsFlow is null || _connectionsEmpty is null || IsDisposed) return;
        if (InvokeRequired) { try { BeginInvoke((Action)RebuildConnectionsView); } catch { } return; }

        _connectionsPage.ClearHistoryButton.Visible = true;
        _connectionsPage.ClearHistoryButton.BringToFront();
        _connectionsFlow.SuspendLayout();
        _connectionsFlow.Controls.Clear();

        List<ConnectionHistoryItem> history;
        lock (_assignmentSync)
            history = _connectionHistory.Where(x => !_activeClients.ContainsKey(x.Endpoint)).Take(10).ToList();

        // Active connections are intentionally rendered first. History is always
        // below them and active endpoints are filtered out of history.
        if (!_activeClients.IsEmpty)
        {
            var activeTitle = new Label { Text = "AKTİF BAĞLANTILAR", ForeColor = Color.FromArgb(85, 210, 255), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Margin = new Padding(2, 2, 2, 6) };
            _connectionsFlow.Controls.Add(activeTitle);
        }
        else
        {
            var activeTitle = new Label { Text = "AKTİF BAĞLANTILAR", ForeColor = Color.FromArgb(85, 210, 255), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Margin = new Padding(2, 2, 2, 6) };
            _connectionsFlow.Controls.Add(activeTitle);
            var noActive = new Label { Text = "Şu anda aktif bağlantı yok.", ForeColor = Color.FromArgb(120, 155, 180), Font = new Font("Segoe UI", 9F), AutoSize = true, Margin = new Padding(2, 0, 2, 10) };
            _connectionsFlow.Controls.Add(noActive);
        }

        foreach (var endpoint in _activeClients.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            string assignedName; DateTime connectedAt;
            lock (_assignmentSync)
            {
                assignedName = _clientScreenAssignments.TryGetValue(endpoint, out var d) ? d : string.Empty;
                connectedAt = _clientConnectedAt.TryGetValue(endpoint, out var t) ? t : DateTime.Now;
            }

            var row = new Panel { Width = Math.Max(420, _connectionsFlow.ClientSize.Width - 20), Height = 82, Margin = new Padding(2, 2, 2, 8), BackColor = Color.FromArgb(7, 22, 38) };
            var name = new Label { Text = $"●  {GetConnectionLabel(endpoint)}", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold), Location = new Point(12, 8), AutoSize = true };
            var time = new Label { Text = $"Bağlandı: {connectedAt:HH:mm:ss}   •   Canlı", ForeColor = Color.FromArgb(80, 230, 150), Font = new Font("Segoe UI", 8.5F), Location = new Point(12, 37), AutoSize = true };
            var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(7, 18, 31), ForeColor = Color.White, Font = new Font("Segoe UI", 9F), Width = 220, Height = 28, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            var screens = GetRemoteScreens().Select(s => new ScreenChoice(s.DeviceName, GetScreenDisplayName(s), s.Bounds.Width, s.Bounds.Height)).ToList();
            foreach (var sc in screens) combo.Items.Add(sc);
            int selected = screens.FindIndex(x => string.Equals(x.DeviceName, assignedName, StringComparison.OrdinalIgnoreCase));
            if (selected >= 0) combo.SelectedIndex = selected; else if (combo.Items.Count > 0) combo.SelectedIndex = 0;
            combo.Location = new Point(Math.Max(230, row.ClientSize.Width - combo.Width - 12), 25);
            combo.SelectedIndexChanged += (_, _) => { if (combo.SelectedItem is ScreenChoice sc) SetClientScreen(endpoint, sc.DeviceName); };
            row.Resize += (_, _) => combo.Left = Math.Max(230, row.ClientSize.Width - combo.Width - 12);
            row.Controls.Add(combo); row.Controls.Add(time); row.Controls.Add(name); _connectionsFlow.Controls.Add(row);
        }

        var historyTitle = new Label { Text = "SON BAĞLANTILAR", ForeColor = Color.FromArgb(165, 190, 220), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), AutoSize = true, Margin = new Padding(2, 12, 2, 6) };
        _connectionsFlow.Controls.Add(historyTitle);

        foreach (var item in history)
        {
            var card = new Panel
            { Width = Math.Max(420, _connectionsFlow.ClientSize.Width - 20), Height = 54, Margin = new Padding(2, 2, 2, 6), BackColor = Color.FromArgb(5, 18, 32) };
            var dot = new Label { Text = "○", ForeColor = Color.FromArgb(105, 135, 160), Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(10, 8), AutoSize = true };
            var name = new Label { Text = GetConnectionLabel(item.Endpoint), ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), Location = new Point(30, 6), AutoSize = true, MaximumSize = new Size(Math.Max(280, card.Width - 240), 20), AutoEllipsis = true };
            var sub = new Label { Text = $"{item.ConnectedAt:dd.MM.yyyy HH:mm:ss}  •  Sonlandırıldı", ForeColor = Color.FromArgb(120, 155, 180), Font = new Font("Segoe UI", 8F), Location = new Point(30, 29), AutoSize = true };
            card.Controls.Add(sub); card.Controls.Add(name); card.Controls.Add(dot); _connectionsFlow.Controls.Add(card);
        }

        if (history.Count == 0)
        {
            var emptyHistory = new Label { Text = "Henüz sonlandırılmış bağlantı yok.", ForeColor = Color.FromArgb(120, 155, 180), Font = new Font("Segoe UI", 9F), AutoSize = true, Margin = new Padding(2, 0, 2, 6) };
            _connectionsFlow.Controls.Add(emptyHistory);
        }

        _connectionsEmpty.Visible = false;
        _connectionsFlow.ResumeLayout();
    }

    private sealed record ScreenChoice(string DeviceName, string Name, int Width, int Height)
    {
        public override string ToString() => $"{Name} • {Width}×{Height}";
    }

    private BrowserFrameSubscription? CreateBrowserFrameSubscription(string deviceName)
    {
        if (_listener is null || string.IsNullOrWhiteSpace(deviceName)) return null;
        Screen? screen = Screen.AllScreens.FirstOrDefault(s => string.Equals(s.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));
        if (screen is null) return null;

        // Use the same shared producer registry as Client streaming. This method
        // is only the ServerForm bridge used by BrowserViewerServer; it must never
        // create its own capture/encoder pipeline.
        return CreateBrowserFrameSubscriptionInternal(
            deviceName,
            screen.Bounds.Width,
            screen.Bounds.Height,
            (int)_fps.Value,
            Math.Clamp(_activeTargetMbps, 1, 500),
            _settings.AutoBitrate,
            (int)_quality.Value);
    }

    private IReadOnlyList<BrowserViewerServer.BrowserScreenInfo> GetBrowserScreens()
    {
        return GetRemoteScreens()
            .Select(s => new BrowserViewerServer.BrowserScreenInfo(
                s.DeviceName,
                $"Ekran {GetPolarisOrdinal(s)}",
                s.Bounds.Width,
                s.Bounds.Height))
            .ToList();
    }

    private static Screen? ResolveBrowserScreen(string deviceName)
    {
        var all = Screen.AllScreens.ToList();
        if (!string.IsNullOrWhiteSpace(deviceName))
        {
            var exact = all.FirstOrDefault(s => string.Equals(s.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));
            if (exact is not null) return exact;
        }

        var remote = all.Where(IsPolarisVirtualScreen).OrderBy(s => s.DeviceName, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        return remote ?? all.FirstOrDefault();
    }

    private static int GetScreenRotation(Screen screen)
    {
        try
        {
            if (DisplayConfigInterop.TryGetCurrentRotation(screen.DeviceName, out var rotation))
            {
                return rotation switch
                {
                    DisplayConfigInterop.CcdRotation.Rotate90 => 90,
                    DisplayConfigInterop.CcdRotation.Rotate180 => 180,
                    DisplayConfigInterop.CcdRotation.Rotate270 => 270,
                    _ => 0
                };
            }
        }
        catch { }
        return 0;
    }

    private static int GetScreenRotationByDeviceName(string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName)) return 0;
        var screen = Screen.AllScreens.FirstOrDefault(x => string.Equals(x.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));
        return screen is null ? 0 : GetScreenRotation(screen);
    }

    private static int GetPolarisOrdinal(Screen screen)
    {
        var remotes = Screen.AllScreens.Where(IsPolarisVirtualScreen).OrderBy(s => s.DeviceName, StringComparer.OrdinalIgnoreCase).ToList();
        int index = remotes.FindIndex(s => string.Equals(s.DeviceName, screen.DeviceName, StringComparison.OrdinalIgnoreCase));
        return index >= 0 ? index + 1 : 0;
    }

    private static List<Screen> GetRemoteScreens()
    {
        return Screen.AllScreens.Where(IsPolarisVirtualScreen).OrderBy(s => s.DeviceName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool IsPolarisVirtualScreen(Screen screen)
    {
        try
        {
            // DisplayInterop.EnumDisplayDevices(screen.DeviceName, 0, ...) enumerates the
            // monitor attached to DISPLAYx. Our IddCx driver exposes the
            // adapter identity at the top level, so looking only at the EDID
            // monitor string can make Polaris screens look like normal
            // physical displays. Enumerate adapters first and match DISPLAYx.
            for (uint i = 0; ; i++)
            {
                var adapter = new DisplayInterop.DISPLAY_DEVICE { cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>() };
                if (!DisplayInterop.EnumDisplayDevices(null, i, ref adapter, 0)) break;
                if (!string.Equals(adapter.DeviceName, screen.DeviceName, StringComparison.OrdinalIgnoreCase))
                    continue;

                string adapterText = $"{adapter.DeviceName} {adapter.DeviceString} {adapter.DeviceID} {adapter.DeviceKey}";
                if (adapterText.Contains("IddSample", StringComparison.OrdinalIgnoreCase) ||
                    adapterText.Contains("Polaris", StringComparison.OrdinalIgnoreCase) ||
                    adapterText.Contains("Indirect Display", StringComparison.OrdinalIgnoreCase) ||
                    adapterText.Contains("INDIRECTDISPLAY", StringComparison.OrdinalIgnoreCase) ||
                    adapterText.Contains("Microsoft Indirect Display Adapter", StringComparison.OrdinalIgnoreCase))
                    return true;

                // Some Windows versions expose the indirect adapter with a
                // generic adapter string but a distinctive monitor DeviceID.
                var monitor = new DisplayInterop.DISPLAY_DEVICE { cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>() };
                for (uint m = 0; DisplayInterop.EnumDisplayDevices(screen.DeviceName, m, ref monitor, 0); m++)
                {
                    string monitorText = $"{monitor.DeviceName} {monitor.DeviceString} {monitor.DeviceID} {monitor.DeviceKey}";
                    if (monitorText.Contains("IddSample", StringComparison.OrdinalIgnoreCase) ||
                        monitorText.Contains("Polaris", StringComparison.OrdinalIgnoreCase) ||
                        monitorText.Contains("Indirect Display", StringComparison.OrdinalIgnoreCase) ||
                        monitorText.Contains("INDIRECTDISPLAY", StringComparison.OrdinalIgnoreCase))
                        return true;
                    monitor = new DisplayInterop.DISPLAY_DEVICE { cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>() };
                }
            }
            return false;
        }
        catch { return false; }
    }


    private void ShowDisplayModeDialog(Screen screen)
    {
        using var dialog = new DisplayModeDialogForm(screen, RefreshScreens);
        dialog.ShowDialog(this);
    }

    private void SelectTab(int index)
    {
        _tabBroadcast.Selected = index == 0; _tabConnections.Selected = index == 1; _tabStats.Selected = index == 2; _tabAdmin.Selected = index == 3; _tabAbout.Selected = index == 4; _tabLog.Selected = index == 5;
        _tabBroadcast.Invalidate(); _tabConnections.Invalidate(); _tabStats.Invalidate(); _tabAdmin.Invalidate(); _tabAbout.Invalidate(); _tabLog.Invalidate();
        _broadcastPage.Visible = index == 0;
        _connectionsPage.Visible = index == 1;
        _statisticsPage.Visible = index == 2;
        _adminPage.Visible = index == 3;
        _aboutPage.Visible = index == 4;
        _logPage.Visible = index == 5;
        if (index == 1) RebuildConnectionsView();

    }

    private void DragWindow(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        ReleaseCapture(); SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
    }

    private void ToggleMaximize() => WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;

    private bool IsDesignMode() => LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode;



    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color.FromArgb(0, 150, 255), 2);
        e.Graphics.DrawRectangle(pen, new Rectangle(2, 2, Width - 5, Height - 5));
        using var glow = new Pen(Color.FromArgb(120, 220, 255), 1);
        e.Graphics.DrawRectangle(glow, new Rectangle(5, 5, Width - 11, Height - 11));
    }


}
