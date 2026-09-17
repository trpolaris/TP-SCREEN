using System.Runtime.InteropServices;
using System.Text.Json;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;

namespace PolarisDisplay.Server;

internal static class ServerUpdateService
{
    public const string Version = "1.0.0";
    public static readonly string DefaultManifestUrl = "";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(6) };
    public sealed record Manifest(string Version, string Url, string Notes);

    public static async Task<Manifest?> CheckAsync(string url, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try { return await Http.GetFromJsonAsync<Manifest>(url, cancellationToken: token); }
        catch { return null; }
    }

    public static bool IsNewer(string current, string candidate)
    {
        if (System.Version.TryParse(current, out var a) && System.Version.TryParse(candidate, out var b)) return b > a;
        return !string.Equals(current, candidate, StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed record DisplayModeChoice(int Width, int Height, int Hz, string Orientation)
{
    public override string ToString() => $"{Width}×{Height} • {Hz} Hz • {Orientation}";
}

internal static class DisplayModeService
{
    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int DM_PELSWIDTH = 0x80000;
    private const int DM_PELSHEIGHT = 0x100000;
    private const int DM_DISPLAYFREQUENCY = 0x400000;
    private const int DM_DISPLAYORIENTATION = 0x80;
    private const int CDS_TEST = 0x2;
    private const int CDS_UPDATEREGISTRY = 0x1;
    private const int CDS_RESET = 0x40000000;
    private const int DISP_CHANGE_SUCCESSFUL = 0;
    private const int DMDO_DEFAULT = 0;
    private const int DMDO_90 = 1;
    private const int DMDO_180 = 2;
    private const int DMDO_270 = 3;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DEVMODE
    {
        private const int CCHDEVICENAME = 32;
        private const int CCHFORMNAME = 32;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)] public string dmDeviceName;
        public short dmSpecVersion, dmDriverVersion, dmSize, dmDriverExtra;
        public int dmFields, dmPositionX, dmPositionY, dmDisplayOrientation, dmDisplayFixedOutput;
        public short dmColor, dmDuplex, dmYResolution, dmTTOption, dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)] public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel, dmPelsWidth, dmPelsHeight, dmDisplayFlags, dmDisplayFrequency;
        public int dmICMMethod, dmICMIntent, dmMediaType, dmDitherType, dmReserved1, dmReserved2;
        public int dmPanningWidth, dmPanningHeight;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int EnumDisplaySettings(string? lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, int dwflags, IntPtr lParam);

    public static List<DisplayModeChoice> GetModes(string deviceName)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<DisplayModeChoice>();

        for (int i = 0; ; i++)
        {
            var d = NewMode();
            if (EnumDisplaySettings(deviceName, i, ref d) == 0)
                break;

            int hz = Math.Max(1, d.dmDisplayFrequency);
            int w = Math.Max(1, d.dmPelsWidth);
            int h = Math.Max(1, d.dmPelsHeight);
            AddModeVariants(list, set, w, h, hz);
        }

        return list
            .OrderByDescending(x => x.Width * x.Height)
            .ThenByDescending(x => x.Hz)
            .ThenBy(x => x.Orientation)
            .ToList();
    }

    public static DisplayModeChoice? GetCurrent(string deviceName)
    {
        var d = NewMode();
        if (EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref d) == 0)
            return null;

        return new DisplayModeChoice(
            d.dmPelsWidth,
            d.dmPelsHeight,
            Math.Max(1, d.dmDisplayFrequency),
            OrientationName(d.dmDisplayOrientation));
    }

    public static bool Apply(string deviceName, DisplayModeChoice choice)
    {
        if (string.IsNullOrWhiteSpace(deviceName) || choice.Width < 16 || choice.Height < 16)
            return false;

        var desiredRotation = OrientationToCcd(choice.Orientation);

        // First commit the rotation against the already-valid CCD path. This is
        // the most reliable operation for an IddCx virtual monitor because it
        // does not manufacture a second source/target mode pair. If Windows
        // immediately reports the requested portrait desktop dimensions, the
        // job is complete. Otherwise the full source/target mode transaction
        // below applies the requested resolution as well.
        if (desiredRotation is DisplayConfigInterop.CcdRotation.Rotate90 or DisplayConfigInterop.CcdRotation.Rotate180 or DisplayConfigInterop.CcdRotation.Rotate270)
        {
            if (DisplayConfigInterop.TrySetTargetRotation(deviceName, desiredRotation))
            {
                if (desiredRotation == DisplayConfigInterop.CcdRotation.Rotate180)
                {
                    if (DisplayConfigInterop.TryGetCurrentRotation(deviceName, out var actual) && actual == desiredRotation)
                        return true;
                }
                else if (MatchesRequestedMode(deviceName, choice))
                {
                    return true;
                }
            }
        }

        // The CCD path is authoritative for IddCx displays. Rotation, desktop
        // resolution and target signal are committed together when the simple
        // rotation transaction above was not enough.
        if (DisplayConfigInterop.TryApplyDisplayConfig(deviceName, desiredRotation, choice.Width, choice.Height, choice.Hz) &&
            WaitForRequestedMode(deviceName, choice))
            return true;

        // Fallback for Windows builds where CCD rejects a virtual topology change.
        // This is deliberately second so it cannot overwrite a successful CCD mode.
        if (TryApplyDevMode(deviceName, choice, useOrientation: desiredRotation != DisplayConfigInterop.CcdRotation.Identity, testOnly: false) &&
            WaitForRequestedMode(deviceName, choice))
            return true;

        return false;
    }

    private static bool WaitForRequestedMode(string deviceName, DisplayModeChoice choice)
    {
        for (int i = 0; i < 20; i++)
        {
            if (MatchesRequestedMode(deviceName, choice)) return true;
            Thread.Sleep(25);
        }
        return MatchesRequestedMode(deviceName, choice);
    }

    private static bool MatchesRequestedMode(string deviceName, DisplayModeChoice choice)
    {
        var current = GetCurrent(deviceName);
        if (current is null) return false;

        bool quarterTurn = choice.Orientation is "Dikey 90°" or "Dikey 270°";
        int expectedWidth = quarterTurn ? Math.Min(choice.Width, choice.Height) : choice.Width;
        int expectedHeight = quarterTurn ? Math.Max(choice.Width, choice.Height) : choice.Height;
        bool dimensionsMatch = current.Width == expectedWidth && current.Height == expectedHeight;
        if (quarterTurn)
            dimensionsMatch |= current.Width == choice.Width && current.Height == choice.Height;
        if (!dimensionsMatch) return false;

        var wanted = OrientationToCcd(choice.Orientation);
        if (DisplayConfigInterop.TryGetCurrentRotation(deviceName, out var actualRotation))
            return actualRotation == wanted;

        return string.Equals(current.Orientation, choice.Orientation, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryApplyDevMode(string deviceName, DisplayModeChoice choice, bool useOrientation, bool testOnly)
    {
        var d = NewMode();
        if (EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref d) == 0)
            return false;

        bool rotate90 = choice.Orientation is "Dikey 90°" or "Dikey 270°";
        int width = choice.Width;
        int height = choice.Height;
        if (rotate90 && width > height) (width, height) = (height, width);
        else if (!rotate90 && height > width) (width, height) = (height, width);

        d.dmPelsWidth = width;
        d.dmPelsHeight = height;
        d.dmDisplayFrequency = choice.Hz;
        d.dmFields |= DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY;
        if (useOrientation)
        {
            d.dmDisplayOrientation = OrientationValue(choice.Orientation);
            d.dmFields |= DM_DISPLAYORIENTATION;
        }

        int flags = testOnly ? CDS_TEST : CDS_UPDATEREGISTRY | CDS_RESET;
        if (ChangeDisplaySettingsEx(deviceName, ref d, IntPtr.Zero, CDS_TEST, IntPtr.Zero) != DISP_CHANGE_SUCCESSFUL)
            return false;

        if (testOnly)
            return true;

        return ChangeDisplaySettingsEx(deviceName, ref d, IntPtr.Zero, flags, IntPtr.Zero) == DISP_CHANGE_SUCCESSFUL;
    }

    private static void AddModeVariants(List<DisplayModeChoice> list, HashSet<string> set, int width, int height, int hz)
    {
        int landscapeWidth = Math.Max(width, height);
        int landscapeHeight = Math.Min(width, height);
        Add(new DisplayModeChoice(landscapeWidth, landscapeHeight, hz, "Yatay"));
        Add(new DisplayModeChoice(landscapeWidth, landscapeHeight, hz, "Ters"));
        Add(new DisplayModeChoice(landscapeHeight, landscapeWidth, hz, "Dikey 90°"));
        Add(new DisplayModeChoice(landscapeHeight, landscapeWidth, hz, "Dikey 270°"));

        void Add(DisplayModeChoice value)
        {
            string key = $"{value.Width}x{value.Height}@{value.Hz}:{value.Orientation}";
            if (set.Add(key)) list.Add(value);
        }
    }

    private static DisplayConfigInterop.CcdRotation OrientationToCcd(string value) => value switch
    {
        "Dikey 90°" => DisplayConfigInterop.CcdRotation.Rotate90,
        "Ters" => DisplayConfigInterop.CcdRotation.Rotate180,
        "Dikey 270°" => DisplayConfigInterop.CcdRotation.Rotate270,
        _ => DisplayConfigInterop.CcdRotation.Identity
    };

    private static DEVMODE NewMode() => new()
    {
        dmDeviceName = string.Empty,
        dmFormName = string.Empty,
        dmSize = (short)Marshal.SizeOf<DEVMODE>()
    };

    private static string OrientationName(int value) => value switch
    {
        DMDO_90 => "Dikey 90°",
        DMDO_180 => "Ters",
        DMDO_270 => "Dikey 270°",
        _ => "Yatay"
    };

    private static int OrientationValue(string value) => value switch
    {
        "Dikey 90°" => DMDO_90,
        "Ters" => DMDO_180,
        "Dikey 270°" => DMDO_270,
        _ => DMDO_DEFAULT
    };

    private static bool TryApplyCcdRotation(string deviceName, DisplayConfigInterop.CcdRotation rotation) =>
        DisplayConfigInterop.TrySetTargetRotation(deviceName, rotation);
}

internal static class QrPayloadBuilder
{
    public static string ViewerUrl(string host, int webPort) => $"http://{host}:{webPort}/";
    public static string ClientPayload(string host, int port, string pin) =>
        $"POLARIS_CONNECT|host={Uri.EscapeDataString(host)}&port={port}&pin={Uri.EscapeDataString(pin ?? string.Empty)}";
}
