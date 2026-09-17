
using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace PolarisDisplay.Client;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
        {
            try { MessageBox.Show(e.Exception.Message, "PolarisDisplay Client", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                if (e.ExceptionObject is Exception ex)
                    MessageBox.Show(ex.Message, "PolarisDisplay Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } catch { }
        };
        Application.Run(new ClientForm());
    }
}

internal static class Protocol
{
    public const byte Version = 9;
    public const int HeaderSize = 32;
    public const int MaxPayload = 32 * 1024 * 1024;

    public static bool Valid(ReadOnlySpan<byte> b)
        => b.Length >= HeaderSize &&
           b[0] == 'P' && b[1] == 'J' && b[2] == 'P' && b[3] == '9' &&
           b[4] == Version &&
           (b[5] == 1 || b[5] == 2);

    public static int I32(byte[] b, int o)
        => System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(b.AsSpan(o, 4));

    public static long I64(byte[] b, int o)
        => System.Buffers.Binary.BinaryPrimitives.ReadInt64BigEndian(b.AsSpan(o, 8));

    public static byte Kind(byte[] b) => b[5];

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
}

internal sealed class DecodedFrame : IDisposable
{
    private byte[]? _buffer;
    private MemoryStream? _stream;
    private Image? _image;

    public DecodedFrame(byte[] buffer, MemoryStream stream, Image image)
    {
        _buffer = buffer;
        _stream = stream;
        _image = image;
    }

    public Image Image => _image ?? throw new ObjectDisposedException(nameof(DecodedFrame));

    public int Width => Image.Width;
    public int Height => Image.Height;

    public void Dispose()
    {
        Image? image = Interlocked.Exchange(ref _image, null);
        MemoryStream? stream = Interlocked.Exchange(ref _stream, null);
        byte[]? buffer = Interlocked.Exchange(ref _buffer, null);
        try { image?.Dispose(); } catch { }
        try { stream?.Dispose(); } catch { }
        if (buffer is not null)
        {
            try { ArrayPool<byte>.Shared.Return(buffer); } catch { }
        }
    }
}

internal sealed class VideoView : Control
{
    private readonly object _frameSync = new();
    private DecodedFrame? _frame;
    private readonly List<DecodedFrame> _retiredFrames = new();
    private int _paintPosted;
    private float _cursorX = -1, _cursorY = -1;
    private bool _cursorVisible;
    private int _cursorRotation;

    public VideoView()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(3, 8, 18);
        Cursor = Cursors.Default;

        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
    }

    public void SetFrame(DecodedFrame frame)
    {
        if (IsDisposed)
        {
            frame.Dispose();
            return;
        }

        lock (_frameSync)
        {
            var old = _frame;
            _frame = frame;
            if (old is not null) _retiredFrames.Add(old);
        }

        if (Interlocked.Exchange(ref _paintPosted, 1) == 0)
        {
            try
            {
                BeginInvoke((Action)(() =>
                {
                    try { Invalidate(); }
                    finally { Interlocked.Exchange(ref _paintPosted, 0); }
                }));
            }
            catch
            {
                Interlocked.Exchange(ref _paintPosted, 0);
                DisposeRetiredFrames();
            }
        }
    }

    public void ClearFrame()
    {
        lock (_frameSync)
        {
            var old = _frame;
            _frame = null;
            if (old is not null) _retiredFrames.Add(old);
            _cursorVisible = false;
            _cursorX = -1f;
            _cursorY = -1f;
        }
        DisposeRetiredFrames();
        try { Invalidate(); } catch { }
    }

    public void SetRemoteCursor(float normalizedX, float normalizedY, bool visible, int rotation = 0)
    {
        normalizedX = Math.Clamp(normalizedX, 0f, 1f);
        normalizedY = Math.Clamp(normalizedY, 0f, 1f);
        rotation = ((rotation % 360) + 360) % 360;
        // The server reports cursor coordinates in the already-rotated desktop
        // coordinate space. The client now applies the rotation at the Windows CCD
        // topology level, so rotating these normalized coordinates again would make
        // the cursor appear mirrored/offset (especially on 90/270 degree displays).
        _cursorX = normalizedX;
        _cursorY = normalizedY;
        _cursorRotation = rotation;
        _cursorVisible = visible;
        try { Invalidate(); } catch { }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        DecodedFrame? frame;
        float cursorX;
        float cursorY;
        bool cursorVisible;

        lock (_frameSync)
        {
            frame = _frame;
            cursorX = _cursorX;
            cursorY = _cursorY;
            cursorVisible = _cursorVisible;
        }

        if (frame is null)
        {
            DisposeRetiredFrames();
            return;
        }

        e.Graphics.CompositingMode = CompositingMode.SourceCopy;
        e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
        e.Graphics.InterpolationMode = frame.Width == ClientSize.Width && frame.Height == ClientSize.Height
            ? InterpolationMode.NearestNeighbor
            : InterpolationMode.Bilinear;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
        e.Graphics.SetClip(ClientRectangle);

        int viewWidth = ClientSize.Width;
        int viewHeight = ClientSize.Height;
        if (viewWidth > 0 && viewHeight > 0)
        {
            if (viewWidth == frame.Width && viewHeight == frame.Height)
            {
                e.Graphics.DrawImageUnscaled(frame.Image, 0, 0);
            }
            else
            {
                e.Graphics.DrawImage(
                    frame.Image,
                    new Rectangle(0, 0, viewWidth, viewHeight),
                    0, 0, frame.Width, frame.Height,
                    GraphicsUnit.Pixel);
            }

            if (cursorVisible && cursorX >= 0f && cursorY >= 0f)
            {
                Cursor systemCursor = Cursors.Default;
                Size cursorSize = SystemInformation.CursorSize;
                if (cursorSize.Width <= 0 || cursorSize.Height <= 0)
                    cursorSize = systemCursor.Size;

                float x = cursorX * viewWidth;
                float y = cursorY * viewHeight;
                var cursorRect = new Rectangle(
                    (int)Math.Round(x),
                    (int)Math.Round(y),
                    Math.Max(1, cursorSize.Width),
                    Math.Max(1, cursorSize.Height));

                systemCursor.Draw(e.Graphics, cursorRect);
            }
        }

        DisposeRetiredFrames();
    }

    private void DisposeRetiredFrames()
    {
        DecodedFrame[] retired;
        lock (_frameSync)
        {
            if (_retiredFrames.Count == 0) return;
            retired = _retiredFrames.ToArray();
            _retiredFrames.Clear();
        }

        foreach (var frame in retired)
            frame.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DecodedFrame[] retired;
            lock (_frameSync)
            {
                if (_frame is not null) _retiredFrames.Add(_frame);
                _frame = null;
                retired = _retiredFrames.ToArray();
                _retiredFrames.Clear();
            }
            foreach (var frame in retired)
                frame.Dispose();
        }

        base.Dispose(disposing);
    }
}
