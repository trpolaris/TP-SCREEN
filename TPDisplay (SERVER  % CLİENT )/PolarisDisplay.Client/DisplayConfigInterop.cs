using System.Runtime.InteropServices;

namespace PolarisDisplay.Client;

internal static class DisplayConfigInterop
{
    internal enum CcdRotation : uint
    {
        Identity = 1,
        Rotate90 = 2,
        Rotate180 = 3,
        Rotate270 = 4
    }

    private const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;
    private const uint QDC_VIRTUAL_MODE_AWARE = 0x00000010;
    private const uint QDC_VIRTUAL_REFRESH_RATE_AWARE = 0x00000040;
    private const uint DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE = 0x00000008;
    private const uint SDC_USE_SUPPLIED_DISPLAY_CONFIG = 0x00000020;
    private const uint SDC_APPLY = 0x00000080;
    private const uint SDC_SAVE_TO_DATABASE = 0x00000200;
    private const uint SDC_ALLOW_CHANGES = 0x00000400;
    private const uint SDC_NO_OPTIMIZATION = 0x00000100;
    private const uint SDC_FORCE_MODE_ENUMERATION = 0x00001000;
    private const uint SDC_VIRTUAL_MODE_AWARE = 0x00008000;
    private const uint SDC_VIRTUAL_REFRESH_RATE_AWARE = 0x00020000;

    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private const uint DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct DISPLAYCONFIG_RATIONAL
    {
        public uint Numerator;
        public uint Denominator;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct POINTL
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct DISPLAYCONFIG_2DREGION
    {
        public uint cx;
        public uint cy;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
    {
        public ulong pixelRate;
        public DISPLAYCONFIG_RATIONAL hSyncFreq;
        public DISPLAYCONFIG_RATIONAL vSyncFreq;
        public DISPLAYCONFIG_2DREGION activeSize;
        public DISPLAYCONFIG_2DREGION totalSize;
        public uint videoStandard;
        public uint scanLineOrdering;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct SOURCE_VIRTUAL_MODE_INFO
    {
        public ushort cloneGroupId;
        public ushort sourceModeInfoIdx;
    }

    [StructLayout(LayoutKind.Explicit, Size = 4, Pack = 4)]
    private struct SOURCE_MODE_UNION
    {
        [FieldOffset(0)] public uint modeInfoIdx;
        [FieldOffset(0)] public SOURCE_VIRTUAL_MODE_INFO virtualMode;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct DISPLAYCONFIG_PATH_SOURCE_INFO
    {
        public LUID adapterId;
        public uint id;
        public SOURCE_MODE_UNION mode;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct TARGET_VIRTUAL_MODE_INFO
    {
        public ushort desktopModeInfoIdx;
        public ushort targetModeInfoIdx;
    }

    [StructLayout(LayoutKind.Explicit, Size = 4, Pack = 4)]
    private struct TARGET_MODE_UNION
    {
        [FieldOffset(0)] public uint modeInfoIdx;
        [FieldOffset(0)] public TARGET_VIRTUAL_MODE_INFO virtualMode;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct DISPLAYCONFIG_PATH_TARGET_INFO
    {
        public LUID adapterId;
        public uint id;
        public TARGET_MODE_UNION mode;
        public uint outputTechnology;
        public CcdRotation rotation;
        public uint scaling;
        public DISPLAYCONFIG_RATIONAL refreshRate;
        public uint scanLineOrdering;
        [MarshalAs(UnmanagedType.Bool)] public bool targetAvailable;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct DISPLAYCONFIG_PATH_INFO
    {
        public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
        public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
        public uint flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct DISPLAYCONFIG_TARGET_MODE
    {
        public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct DISPLAYCONFIG_SOURCE_MODE
    {
        public uint width;
        public uint height;
        public uint pixelFormat;
        public POINTL position;
    }

    [StructLayout(LayoutKind.Explicit, Size = 64, Pack = 4)]
    private struct DISPLAYCONFIG_MODE_INFO
    {
        [FieldOffset(0)] public uint infoType;
        [FieldOffset(4)] public uint id;
        [FieldOffset(8)] public LUID adapterId;
        [FieldOffset(16)] public DISPLAYCONFIG_TARGET_MODE targetMode;
        [FieldOffset(16)] public DISPLAYCONFIG_SOURCE_MODE sourceMode;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DISPLAYCONFIG_DEVICE_INFO_HEADER
    {
        public uint type;
        public uint size;
        public LUID adapterId;
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string viewGdiDeviceName;
    }

    [DllImport("user32.dll", SetLastError = false)]
    private static extern int GetDisplayConfigBufferSizes(
        uint flags,
        out uint numPathArrayElements,
        out uint numModeInfoArrayElements);

    [DllImport("user32.dll", SetLastError = false)]
    private static extern int QueryDisplayConfig(
        uint flags,
        ref uint numPathArrayElements,
        [Out] DISPLAYCONFIG_PATH_INFO[] pathInfoArray,
        ref uint numModeInfoArrayElements,
        [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
        IntPtr currentTopologyId);

    [DllImport("user32.dll", SetLastError = false)]
    private static extern int SetDisplayConfig(
        uint numPathArrayElements,
        [In] DISPLAYCONFIG_PATH_INFO[] pathInfoArray,
        uint numModeInfoArrayElements,
        [In] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
        uint flags);

    [DllImport("user32.dll", SetLastError = false)]
    private static extern int DisplayConfigGetDeviceInfo(IntPtr requestPacket);

    internal static bool TryApplyDisplayConfig(string deviceName, CcdRotation rotation, int requestedWidth, int requestedHeight, int refreshHz)
    {
        if (string.IsNullOrWhiteSpace(deviceName)) return false;

        const uint queryFlags = QDC_ONLY_ACTIVE_PATHS | QDC_VIRTUAL_MODE_AWARE | QDC_VIRTUAL_REFRESH_RATE_AWARE;
        const uint setFlags = SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_APPLY | SDC_SAVE_TO_DATABASE |
                              SDC_ALLOW_CHANGES | SDC_NO_OPTIMIZATION | SDC_FORCE_MODE_ENUMERATION | SDC_VIRTUAL_MODE_AWARE | SDC_VIRTUAL_REFRESH_RATE_AWARE;

        for (int attempt = 0; attempt < 3; attempt++)
        {
            int sizeResult = GetDisplayConfigBufferSizes(queryFlags, out uint pathCount, out uint modeCount);
            if (sizeResult != 0 || pathCount == 0) return false;

            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            // QueryDisplayConfig can under-report the mode array after a topology
            // change. Keep spare entries so we can append the requested source and
            // target mode without relying on a mode that is currently active only.
            int initialModeCapacity = Math.Max(16, checked((int)modeCount + 8));
            var modes = new DISPLAYCONFIG_MODE_INFO[initialModeCapacity];
            uint pathsInOut = pathCount;
            uint modesInOut = (uint)modes.Length;

            int queryResult = QueryDisplayConfig(queryFlags, ref pathsInOut, paths, ref modesInOut, modes, IntPtr.Zero);
            if (queryResult == ERROR_INSUFFICIENT_BUFFER && attempt < 2) continue;
            if (queryResult != 0) return false;

            bool matched = false;
            bool changed = false;

            for (int i = 0; i < pathsInOut; i++)
            {
                if (!TryGetSourceDeviceName(paths[i].sourceInfo, out string gdiName) ||
                    !string.Equals(gdiName, deviceName, StringComparison.OrdinalIgnoreCase))
                    continue;

                matched = true;
                if (paths[i].targetInfo.rotation != rotation)
                {
                    paths[i].targetInfo.rotation = rotation;
                    changed = true;
                }

                if (requestedWidth > 0 && requestedHeight > 0)
                {
                    bool quarterTurn = rotation is CcdRotation.Rotate90 or CcdRotation.Rotate270;
                    // For a 90/270 degree path the desktop/source mode is portrait,
                    // while the target video signal remains the driver's landscape
                    // mode. Windows applies targetInfo.rotation between those two
                    // spaces. Supplying portrait dimensions to both source and target
                    // makes SetDisplayConfig reject the topology on IddCx displays.
                    uint sourceWidth = (uint)Math.Max(16, requestedWidth);
                    uint sourceHeight = (uint)Math.Max(16, requestedHeight);
                    uint targetWidth = (uint)(quarterTurn ? Math.Max(requestedWidth, requestedHeight) : requestedWidth);
                    uint targetHeight = (uint)(quarterTurn ? Math.Min(requestedWidth, requestedHeight) : requestedHeight);
                    bool virtualMode = (paths[i].flags & DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) != 0;

                    int sourceCurrentIndex = GetPathSourceModeIndex(paths[i]);
                    int targetCurrentIndex = GetPathTargetModeIndex(paths[i]);

                    DISPLAYCONFIG_MODE_INFO sourceMode;
                    if (sourceCurrentIndex >= 0 && sourceCurrentIndex < modesInOut)
                    {
                        sourceMode = modes[sourceCurrentIndex];
                    }
                    else
                    {
                        sourceMode = new DISPLAYCONFIG_MODE_INFO
                        {
                            infoType = 1,
                            id = paths[i].sourceInfo.id,
                            adapterId = paths[i].sourceInfo.adapterId
                        };
                    }
                    sourceMode.infoType = 1;
                    sourceMode.id = paths[i].sourceInfo.id;
                    sourceMode.adapterId = paths[i].sourceInfo.adapterId;
                    sourceMode.sourceMode.width = sourceWidth;
                    sourceMode.sourceMode.height = sourceHeight;
                    if (sourceMode.sourceMode.pixelFormat == 0)
                        sourceMode.sourceMode.pixelFormat = 1; // DISPLAYCONFIG_PIXELFORMAT_32BPP
                    int sourceIndex = AppendOrReplaceSourceMode(ref modes, ref modesInOut, sourceMode);

                    DISPLAYCONFIG_MODE_INFO targetMode;
                    if (targetCurrentIndex >= 0 && targetCurrentIndex < modesInOut)
                    {
                        targetMode = modes[targetCurrentIndex];
                    }
                    else
                    {
                        targetMode = new DISPLAYCONFIG_MODE_INFO
                        {
                            infoType = 2,
                            id = paths[i].targetInfo.id,
                            adapterId = paths[i].targetInfo.adapterId
                        };
                    }
                    targetMode.infoType = 2;
                    targetMode.id = paths[i].targetInfo.id;
                    targetMode.adapterId = paths[i].targetInfo.adapterId;
                    var signal = targetMode.targetMode.targetVideoSignalInfo;
                    signal.activeSize.cx = targetWidth;
                    signal.activeSize.cy = targetHeight;
                    signal.totalSize.cx = targetWidth;
                    signal.totalSize.cy = targetHeight;
                    if (refreshHz > 0)
                    {
                        signal.vSyncFreq = new DISPLAYCONFIG_RATIONAL { Numerator = (uint)refreshHz, Denominator = 1 };
                        signal.hSyncFreq = new DISPLAYCONFIG_RATIONAL
                        {
                            Numerator = checked((uint)Math.Max(1L, (long)refreshHz * targetHeight)),
                            Denominator = 1
                        };
                    }
                    if (signal.pixelRate == 0)
                    {
                        uint hz = refreshHz > 0 ? (uint)refreshHz : 60u;
                        signal.pixelRate = (ulong)targetWidth * targetHeight * hz;
                    }
                    targetMode.targetMode.targetVideoSignalInfo = signal;
                    int targetIndex = AppendOrReplaceTargetMode(ref modes, ref modesInOut, targetMode, targetWidth, targetHeight, refreshHz);

                    if (virtualMode)
                    {
                        paths[i].sourceInfo.mode.virtualMode.sourceModeInfoIdx = checked((ushort)sourceIndex);
                        paths[i].targetInfo.mode.virtualMode.targetModeInfoIdx = checked((ushort)targetIndex);
                        paths[i].targetInfo.mode.virtualMode.desktopModeInfoIdx = checked((ushort)sourceIndex);
                    }
                    else
                    {
                        paths[i].sourceInfo.mode.modeInfoIdx = (uint)sourceIndex;
                        paths[i].targetInfo.mode.modeInfoIdx = (uint)targetIndex;
                    }

                    var selectedSignal = modes[targetIndex].targetMode.targetVideoSignalInfo;
                    if (selectedSignal.vSyncFreq.Denominator == 0)
                        selectedSignal.vSyncFreq = new DISPLAYCONFIG_RATIONAL { Numerator = (uint)Math.Max(1, refreshHz), Denominator = 1 };
                    paths[i].targetInfo.refreshRate = selectedSignal.vSyncFreq;
                    changed = true;
                }
                break;
            }

            if (!matched) return false;
            if (!changed) return true;

            int setResult = SetDisplayConfig(pathsInOut, paths, modesInOut, modes, setFlags);
            if (setResult == 0) return true;
            if (setResult == ERROR_INSUFFICIENT_BUFFER && attempt < 2) continue;
            return false;
        }

        return false;
    }

    private static bool SameLuid(LUID left, LUID right) =>
        left.LowPart == right.LowPart && left.HighPart == right.HighPart;

    private static void EnsureModeCapacity(ref DISPLAYCONFIG_MODE_INFO[] modes, uint requiredCount)
    {
        if (requiredCount <= (uint)modes.Length) return;

        int currentLength = modes.Length;
        int requiredLength = checked((int)requiredCount);
        int newLength = Math.Max(requiredLength, Math.Max(currentLength * 2, 16));
        Array.Resize(ref modes, newLength);
    }

    private static int GetPathSourceModeIndex(DISPLAYCONFIG_PATH_INFO path)
    {
        bool virtualMode = (path.flags & DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) != 0;
        return virtualMode ? path.sourceInfo.mode.virtualMode.sourceModeInfoIdx : checked((int)path.sourceInfo.mode.modeInfoIdx);
    }

    private static int GetPathTargetModeIndex(DISPLAYCONFIG_PATH_INFO path)
    {
        bool virtualMode = (path.flags & DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) != 0;
        return virtualMode ? path.targetInfo.mode.virtualMode.targetModeInfoIdx : checked((int)path.targetInfo.mode.modeInfoIdx);
    }

    private static int AppendOrReplaceSourceMode(
        ref DISPLAYCONFIG_MODE_INFO[] modes,
        ref uint count,
        DISPLAYCONFIG_MODE_INFO mode)
    {
        for (int i = 0; i < (int)count; i++)
        {
            if (modes[i].infoType == 1 && SameLuid(modes[i].adapterId, mode.adapterId) && modes[i].id == mode.id)
            {
                modes[i] = mode;
                return i;
            }
        }

        EnsureModeCapacity(ref modes, count + 1);
        int index = checked((int)count++);
        modes[index] = mode;
        return index;
    }

    private static int AppendOrReplaceTargetMode(
        ref DISPLAYCONFIG_MODE_INFO[] modes,
        ref uint count,
        DISPLAYCONFIG_MODE_INFO mode,
        uint width,
        uint height,
        int refreshHz)
    {
        for (int i = 0; i < (int)count; i++)
        {
            if (modes[i].infoType != 2 || !SameLuid(modes[i].adapterId, mode.adapterId) || modes[i].id != mode.id)
                continue;
            modes[i] = mode;
            return i;
        }

        EnsureModeCapacity(ref modes, count + 1);
        int index = checked((int)count++);
        modes[index] = mode;
        return index;
    }

    internal static bool TrySetTargetRotation(string deviceName, CcdRotation rotation)
    {
        if (string.IsNullOrWhiteSpace(deviceName)) return false;

        const uint queryFlags = QDC_ONLY_ACTIVE_PATHS | QDC_VIRTUAL_MODE_AWARE | QDC_VIRTUAL_REFRESH_RATE_AWARE;
        const uint baseSetFlags = SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_APPLY | SDC_SAVE_TO_DATABASE |
                                   SDC_ALLOW_CHANGES | SDC_VIRTUAL_MODE_AWARE;

        for (int attempt = 0; attempt < 3; attempt++)
        {
            int sizeResult = GetDisplayConfigBufferSizes(queryFlags, out uint pathCount, out uint modeCount);
            if (sizeResult != 0 || pathCount == 0) return false;

            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[Math.Max(8, checked((int)modeCount))];
            uint pathCountInOut = pathCount;
            uint modeCountInOut = (uint)modes.Length;

            int queryResult = QueryDisplayConfig(queryFlags, ref pathCountInOut, paths, ref modeCountInOut, modes, IntPtr.Zero);
            if (queryResult == ERROR_INSUFFICIENT_BUFFER && attempt < 2) continue;
            if (queryResult != 0) return false;

            bool found = false;
            for (int i = 0; i < pathCountInOut; i++)
            {
                if (!TryGetSourceDeviceName(paths[i].sourceInfo, out string gdiName) ||
                    !string.Equals(gdiName, deviceName, StringComparison.OrdinalIgnoreCase))
                    continue;

                paths[i].targetInfo.rotation = rotation;
                found = true;
                break;
            }

            if (!found) return false;

            uint[] flagVariants =
            {
                baseSetFlags | SDC_VIRTUAL_REFRESH_RATE_AWARE,
                baseSetFlags
            };

            foreach (uint setFlags in flagVariants)
            {
                int setResult = SetDisplayConfig(pathCountInOut, paths, modeCountInOut, modes, setFlags);
                if (setResult == 0) return true;
                if (setResult == ERROR_INSUFFICIENT_BUFFER) break;
            }
        }

        return false;
    }

    internal static bool TryGetCurrentRotation(string deviceName, out CcdRotation rotation)
    {
        rotation = CcdRotation.Identity;
        if (string.IsNullOrWhiteSpace(deviceName)) return false;
        const uint flags = QDC_ONLY_ACTIVE_PATHS | QDC_VIRTUAL_MODE_AWARE | QDC_VIRTUAL_REFRESH_RATE_AWARE;
        int sizeResult = GetDisplayConfigBufferSizes(flags, out uint pathCount, out uint modeCount);
        if (sizeResult != 0 || pathCount == 0) return false;
        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new DISPLAYCONFIG_MODE_INFO[Math.Max(modeCount, 8)];
        uint pCount = pathCount, mCount = modeCount;
        int result = QueryDisplayConfig(flags, ref pCount, paths, ref mCount, modes, IntPtr.Zero);
        if (result != 0) return false;
        for (int i = 0; i < pCount; i++)
        {
            if (!TryGetSourceDeviceName(paths[i].sourceInfo, out string gdiName)) continue;
            if (!string.Equals(gdiName, deviceName, StringComparison.OrdinalIgnoreCase)) continue;
            rotation = paths[i].targetInfo.rotation;
            return true;
        }
        return false;
    }

    private static bool TryGetSourceDeviceName(DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo, out string name)
    {
        name = string.Empty;
        var info = new DISPLAYCONFIG_SOURCE_DEVICE_NAME
        {
            header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
            {
                type = DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME,
                size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DEVICE_NAME>(),
                adapterId = sourceInfo.adapterId,
                id = sourceInfo.id
            },
            viewGdiDeviceName = string.Empty
        };

        int size = Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DEVICE_NAME>();
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(info, buffer, false);
            int result = DisplayConfigGetDeviceInfo(buffer);
            if (result != 0) return false;
            info = Marshal.PtrToStructure<DISPLAYCONFIG_SOURCE_DEVICE_NAME>(buffer);
            name = info.viewGdiDeviceName ?? string.Empty;
            return !string.IsNullOrWhiteSpace(name);
        }
        finally
        {
            Marshal.DestroyStructure<DISPLAYCONFIG_SOURCE_DEVICE_NAME>(buffer);
            Marshal.FreeHGlobal(buffer);
        }
    }
}
