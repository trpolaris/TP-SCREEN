using System.Drawing;
using System.Runtime.InteropServices;

namespace PolarisDisplay.Client;

internal static class RemoteDisplayOrientation
{
    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int DM_PELSWIDTH = 0x80000;
    private const int DM_PELSHEIGHT = 0x100000;
    private const int DM_DISPLAYORIENTATION = 0x80;
    private const int CDS_UPDATEREGISTRY = 0x1;
    private const int CDS_RESET = 0x40000000;
    private const int CDS_TEST = 0x2;
    private const int DISP_CHANGE_SUCCESSFUL = 0;
    private const int DMDO_DEFAULT = 0;
    private const int DMDO_90 = 1;
    private const int DMDO_180 = 2;
    private const int DMDO_270 = 3;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
        public short dmSpecVersion, dmDriverVersion, dmSize, dmDriverExtra;
        public int dmFields, dmPositionX, dmPositionY, dmDisplayOrientation, dmDisplayFixedOutput;
        public short dmColor, dmDuplex, dmYResolution, dmTTOption, dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel, dmPelsWidth, dmPelsHeight, dmDisplayFlags, dmDisplayFrequency;
        public int dmICMMethod, dmICMIntent, dmMediaType, dmDitherType, dmReserved1, dmReserved2;
        public int dmPanningWidth, dmPanningHeight;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ChangeDisplaySettingsEx(string deviceName, ref DEVMODE devMode, IntPtr hwnd, int flags, IntPtr param);

    private static readonly object Sync = new();
    private static string? _deviceName;
    private static DisplayConfigInterop.CcdRotation _originalCcdRotation;
    private static bool _ccdCaptured;
    private static DEVMODE? _originalMode;
    private static bool _fallbackChanged;

    public static bool Apply(Form owner, int rotation)
    {
        rotation = Normalize(rotation);
        string device = Screen.FromControl(owner).DeviceName;

        lock (Sync)
        {
            if (_deviceName is not null && !string.Equals(_deviceName, device, StringComparison.OrdinalIgnoreCase))
                RestoreLocked();

            if (_deviceName is null)
            {
                _deviceName = device;
                _ccdCaptured = DisplayConfigInterop.TryGetCurrentRotation(device, out _originalCcdRotation);
                if (!_ccdCaptured)
                    _originalCcdRotation = DisplayConfigInterop.CcdRotation.Identity;
            }

            var target = rotation switch
            {
                90 => DisplayConfigInterop.CcdRotation.Rotate90,
                180 => DisplayConfigInterop.CcdRotation.Rotate180,
                270 => DisplayConfigInterop.CcdRotation.Rotate270,
                _ => DisplayConfigInterop.CcdRotation.Identity
            };

            // Use CCD first. Unlike ChangeDisplaySettingsEx/DEVMODE, this changes
            // the Windows display topology itself, so the shell, hit testing and
            // mouse coordinate space all agree with the rotated virtual monitor.
            if (DisplayConfigInterop.TrySetTargetRotation(device, target))
            {
                _fallbackChanged = false;
                ResizeFullscreen(owner);
                return true;
            }

            // Keep the old DEVMODE path only as a fallback for systems where CCD
            // cannot address the virtual display.
            return ApplyDevModeFallback(owner, device, rotation);
        }
    }

    public static void Restore()
    {
        lock (Sync) RestoreLocked();
    }

    private static void RestoreLocked()
    {
        string? device = _deviceName;
        if (string.IsNullOrWhiteSpace(device)) return;

        try
        {
            if (_ccdCaptured)
                DisplayConfigInterop.TrySetTargetRotation(device, _originalCcdRotation);
            else if (_fallbackChanged && _originalMode is not null)
            {
                var mode = _originalMode.Value;
                ChangeDisplaySettingsEx(device, ref mode, IntPtr.Zero, CDS_UPDATEREGISTRY | CDS_RESET, IntPtr.Zero);
            }
        }
        catch { }

        _deviceName = null;
        _originalMode = null;
        _ccdCaptured = false;
        _fallbackChanged = false;
    }

    private static bool ApplyDevModeFallback(Form owner, string device, int rotation)
    {
        var mode = NewMode();
        if (EnumDisplaySettings(device, ENUM_CURRENT_SETTINGS, ref mode) == 0)
            return false;

        if (!_fallbackChanged)
            _originalMode = mode;

        int dmdo = rotation switch { 90 => DMDO_90, 180 => DMDO_180, 270 => DMDO_270, _ => DMDO_DEFAULT };
        bool quarter = rotation == 90 || rotation == 270;
        if (quarter && (mode.dmDisplayOrientation == DMDO_DEFAULT || mode.dmDisplayOrientation == DMDO_180))
            (mode.dmPelsWidth, mode.dmPelsHeight) = (mode.dmPelsHeight, mode.dmPelsWidth);
        else if (!quarter && (mode.dmDisplayOrientation == DMDO_90 || mode.dmDisplayOrientation == DMDO_270))
            (mode.dmPelsWidth, mode.dmPelsHeight) = (mode.dmPelsHeight, mode.dmPelsWidth);

        mode.dmDisplayOrientation = dmdo;
        mode.dmFields |= DM_DISPLAYORIENTATION | DM_PELSWIDTH | DM_PELSHEIGHT;

        if (ChangeDisplaySettingsEx(device, ref mode, IntPtr.Zero, CDS_TEST, IntPtr.Zero) != DISP_CHANGE_SUCCESSFUL)
            return false;
        if (ChangeDisplaySettingsEx(device, ref mode, IntPtr.Zero, CDS_UPDATEREGISTRY | CDS_RESET, IntPtr.Zero) != DISP_CHANGE_SUCCESSFUL)
            return false;

        _fallbackChanged = true;
        ResizeFullscreen(owner);
        return true;
    }

    private static void ResizeFullscreen(Form owner)
    {
        // Orientation changes must never resize the normal Viewer window.
        // The old implementation treated every borderless form as fullscreen,
        // which is why a normal 800x560 Viewer could suddenly become screen-sized.
        if (owner is ClientForm client && !client.IsViewerFullscreen)
            return;

        try
        {
            if (owner.IsDisposed || !owner.IsHandleCreated) return;
            owner.BeginInvoke((Action)(() =>
            {
                try
                {
                    if (owner is ClientForm viewer && !viewer.IsViewerFullscreen)
                        return;
                    var screen = Screen.FromControl(owner);
                    if (owner.FormBorderStyle == FormBorderStyle.None)
                        owner.Bounds = screen.Bounds;
                }
                catch { }
            }));
        }
        catch { }
    }

    private static DEVMODE NewMode() => new()
    {
        dmDeviceName = string.Empty,
        dmFormName = string.Empty,
        dmSize = (short)Marshal.SizeOf<DEVMODE>()
    };

    private static int Normalize(int value) => value switch
    {
        90 => 90,
        180 => 180,
        270 => 270,
        _ => 0
    };
}
