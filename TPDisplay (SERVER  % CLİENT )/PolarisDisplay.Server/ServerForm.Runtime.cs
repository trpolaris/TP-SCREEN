using System.Buffers;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace PolarisDisplay.Server;

public sealed partial class ServerForm : Form
{

    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private readonly ConcurrentDictionary<string, string> _pendingScreenCommands = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _pendingControlCommands = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _clientAuthWaiters = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _clientConnectionGenerations = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _authenticatedClients = new(StringComparer.OrdinalIgnoreCase);
    private long _connectionGenerationSeed;
    private readonly ConcurrentDictionary<string, ClientTelemetryState> _clientTelemetry = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, TcpClient> _clientTransports = new(StringComparer.OrdinalIgnoreCase);
    internal sealed record ClientTelemetryState(int PingMs,string Mode,double Fps,double Mbps,int Quality,int Width,int Height);

    private sealed class ScreenItem
    {
        public required Screen Screen { get; init; }
        public required string Name { get; init; }
        public bool IsRemote { get; init; }
        public override string ToString() =>
            $"{Name} • {Screen.Bounds.Width}×{Screen.Bounds.Height}";
    }

    private sealed class ResolutionItem
    {
        public required int Width { get; init; }
        public required int Height { get; init; }
        public string Label => $"{Width}×{Height}";
        public override string ToString() => Label;
    }

    private void RefreshScreens()
    {
        var previousDevice = (_screen.SelectedItem as ScreenItem)?.Screen.DeviceName;
        _screen.Items.Clear();

        // Only Polaris virtual outputs belong in the Server screen selector.
        // Never mix the host's physical monitors into the remote-display list.
        var remote = Screen.AllScreens
            .Where(IsPolarisVirtualScreen)
            .OrderBy(s => s.DeviceName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (int i = 0; i < remote.Count; i++)
        {
            var screen = remote[i];
            _screen.Items.Add(new ScreenItem
            {
                Screen = screen,
                Name = $"Ekran {i + 1}",
                IsRemote = true
            });
        }

        if (_screen.Items.Count > 0)
        {
            int selected = 0;
            if (!string.IsNullOrWhiteSpace(previousDevice))
            {
                for (int i = 0; i < _screen.Items.Count; i++)
                {
                    if (_screen.Items[i] is ScreenItem item &&
                        string.Equals(item.Screen.DeviceName, previousDevice, StringComparison.OrdinalIgnoreCase))
                    {
                        selected = i;
                        break;
                    }
                }
            }
            _screen.SelectedIndex = selected;
        }

        _broadcastPage.SetActiveScreens(remote);
        BuildResolutions();
    }

    private void BuildResolutions()
    {
        _resolution.Items.Clear();

        var selected = _screen.SelectedItem as ScreenItem;
        if (selected is null)
            return;

        int sw = selected.Screen.Bounds.Width;
        int sh = selected.Screen.Bounds.Height;

        AddResolution(sw, sh);

        foreach (var item in new[]
        {
            (3840,2160), (2560,1440), (2560,1600), (1920,1200),
            (1920,1080), (1600,900), (1366,768), (1280,720),
            (1152,648), (1024,576), (960,540), (854,480)
        })
        {
            if (item.Item1 <= sw && item.Item2 <= sh)
                AddResolution(item.Item1, item.Item2);
        }

        if (_resolution.Items.Count > 0)
            _resolution.SelectedIndex = 0;

        UpdateQualityRecommendation();
    }

    private void AddResolution(int width, int height)
    {
        if (_resolution.Items
            .OfType<ResolutionItem>()
            .Any(x => x.Width == width && x.Height == height))
            return;

        _resolution.Items.Add(new ResolutionItem { Width = width, Height = height });
    }

    private void UpdateQualityRecommendation()
    {
        if (_resolution.SelectedItem is not ResolutionItem r)
            return;

        // JPEG encoding is CPU-bound. Lower starting quality at high resolutions
        // keeps 60 FPS viable while remaining visually sharp on a LAN.
        // Keep JPEG quality high enough to avoid blockiness. The bitrate
        // controller is allowed to lower this only when the selected network
        // budget is actually exceeded.
        _quality.Value = r.Width >= 2560 ? 82 :
                         r.Width >= 1920 ? 86 :
                         r.Width >= 1600 ? 88 :
                         r.Width >= 1280 ? 90 : 92;
    }

    private async Task ToggleServerAsync()
    {
        if (_listener is not null)
        {
            StopServer();
            return;
        }

        if (_screen.SelectedItem is not ScreenItem screenItem ||
            _resolution.SelectedItem is not ResolutionItem res)
        {
            MessageBox.Show(this, "Ekran ve çözünürlük seçin.");
            return;
        }

        int fps = (int)_fps.Value;
        int quality = (int)_quality.Value;
        int manualMbps = (int)_bitrate.Value;
        int targetMbps = manualMbps;

        if (_bitrateMode.SelectedIndex == 0)
        {
            var link = GetBestLinkSpeedMbps();
            // Conservative LAN budget: reserve headroom for TCP/UI traffic.
            // JPEG gets enough bandwidth for low-latency 60 FPS without blindly
            // selecting 90+ Mbps on a gigabit NIC.
            double pixelsPerSecond = Math.Max(1, res.Width * (double)res.Height * Math.Max(15, fps));
            int resolutionBudget = pixelsPerSecond >= 500_000_000 ? 100 :
                                   pixelsPerSecond >= 250_000_000 ? 65 :
                                   pixelsPerSecond >= 120_000_000 ? 42 :
                                   pixelsPerSecond >= 60_000_000 ? 28 : 16;
            int linkBudget = link >= 2500 ? 120 : link >= 1000 ? 80 : link >= 500 ? 45 : link >= 100 ? 20 : 8;
            targetMbps = Math.Clamp(Math.Min(resolutionBudget, linkBudget), 8, 120);
            Log($"Otomatik bant: NIC {link} Mbps → JPEG hedef {targetMbps} Mbps");
        }

        // Browser subscriptions must use the same encoder budget as Clients.
        _activeTargetMbps = targetMbps;
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, (int)_port.Value);
        _listener.Start();

        _broadcastStartedUtc = DateTime.UtcNow;
        Interlocked.Exchange(ref _totalBytesSent, 0);
        _lastStatsBytes = 0;
        _lastStatsUtc = DateTime.UtcNow;
        _lastCpuTime = _statsProcess?.TotalProcessorTime ?? TimeSpan.Zero;
        SetRunning(true);
        _browserViewer?.SetBroadcastEnabled(true);

        Log($"SERVER 0.0.0.0:{_port.Value} • JPEG • {fps} FPS");
        Log($"Kaynak {screenItem.Name} / {screenItem.Screen.DeviceName} • {screenItem.Screen.Bounds.Width}×{screenItem.Screen.Bounds.Height}");
        if (GetRemoteScreens().Count > 0)
            Log($"Çoklu uzak ekran modu aktif • {GetRemoteScreens().Count} sanal ekran bulundu.");
        Log($"Çıkış {res.Width}×{res.Height} • Q{quality} • hedef {targetMbps} Mbps");

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                client.NoDelay = true;
                client.SendBufferSize = 96 * 1024;
                client.ReceiveBufferSize = 96 * 1024;
                string endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Bilinmeyen";
                CloseDuplicateClientConnections(endpoint);
                // Register the transport immediately so two near-simultaneous
                // reconnects from the same PC cannot both pass the dedupe check
                // before their StreamClientAsync workers start.
                _clientTransports[endpoint] = client;
                var assignedScreen = await SelectRemoteScreenAsync(endpoint, screenItem.Screen, _cts.Token);
                if (assignedScreen is null)
                {
                    _clientTransports.TryRemove(endpoint, out _);
                    const string rejection = "Boş Polaris ekranı yok. Bağlantı reddedildi.";
                    Log($"{endpoint} bağlantısı reddedildi: boş uzak ekran yok.");
                    await SendConnectionRejectedAsync(client, rejection, _cts.Token);
                    client.Dispose();
                    continue;
                }
                _ = Task.Run(() => StreamClientAsync(
                    client, assignedScreen, res.Width, res.Height,
                    fps, quality, targetMbps, _bitrateMode.SelectedIndex == 0, _cts.Token, endpoint));
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        catch (Exception ex)
        {
            Log("Server hatası: " + ex.Message);
        }
    }


    private sealed class SharedEncodedFrame : IDisposable
    {
        private byte[]? _buffer;
        private int _refs = 1;

        public int Length { get; }
        public int Width { get; }
        public int Height { get; }
        public int Fps { get; }
        public int Quality { get; }
        public long FrameId { get; }
        public byte[] Buffer => _buffer ?? throw new ObjectDisposedException(nameof(SharedEncodedFrame));

        public SharedEncodedFrame(byte[] buffer, int length, int width, int height, int fps, int quality, long frameId)
        {
            _buffer = buffer;
            Length = length;
            Width = width;
            Height = height;
            Fps = fps;
            Quality = quality;
            FrameId = frameId;
        }

        public bool TryAddRef()
        {
            while (true)
            {
                int current = Volatile.Read(ref _refs);
                if (current <= 0 || Volatile.Read(ref _buffer) is null) return false;
                if (Interlocked.CompareExchange(ref _refs, current + 1, current) == current) return true;
            }
        }

        public void Release()
        {
            if (Interlocked.Decrement(ref _refs) != 0) return;
            byte[]? buffer = Interlocked.Exchange(ref _buffer, null);
            if (buffer is not null) ReturnPooled(buffer);
        }

        public void Dispose() => Release();

        private static void ReturnPooled(byte[] buffer) => ArrayPool<byte>.Shared.Return(buffer);
    }

    private sealed record EncodedSnapshot(SharedEncodedFrame Frame)
    {
        public byte[] Payload => Frame.Buffer;
        public int Length => Frame.Length;
        public int Width => Frame.Width;
        public int Height => Frame.Height;
        public int Fps => Frame.Fps;
        public int Quality => Frame.Quality;
        public long FrameId => Frame.FrameId;
        public void Release() => Frame.Release();
    }

    /// <summary>
    /// Growable stream backed by ArrayPool<byte>. It eliminates the extra
    /// MemoryStream -> ArrayPool copy on every JPEG frame and keeps the pooled
    /// buffer alive until the shared frame is released by all clients.
    /// </summary>
    private sealed class PooledBufferStream : Stream
    {
        private byte[]? _buffer;
        private int _position;
        private int _length;
        private bool _detached;

        public PooledBufferStream(int initialCapacity)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(16 * 1024, initialCapacity));
        }

        public override bool CanRead => false;
        public override bool CanSeek => true;
        public override bool CanWrite => _buffer is not null;
        public override long Length => _length;
        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(value));
                EnsureNotDisposed();
                _position = (int)value;
            }
        }

        private void EnsureNotDisposed()
        {
            if (_buffer is null) throw new ObjectDisposedException(nameof(PooledBufferStream));
        }

        private void EnsureCapacity(int required)
        {
            EnsureNotDisposed();
            if (required <= _buffer!.Length) return;
            int next = _buffer.Length;
            while (next < required)
            {
                int grown = next <= 16 * 1024 * 1024 ? next * 2 : next + 16 * 1024 * 1024;
                if (grown <= next) { next = required; break; }
                next = grown;
            }
            byte[] replacement = ArrayPool<byte>.Shared.Rent(next);
            Buffer.BlockCopy(_buffer, 0, replacement, 0, _length);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = replacement;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer is null) throw new ArgumentNullException(nameof(buffer));
            if ((uint)offset > (uint)buffer.Length || (uint)count > (uint)(buffer.Length - offset))
                throw new ArgumentOutOfRangeException();
            EnsureNotDisposed();
            int end = checked(_position + count);
            EnsureCapacity(end);
            Buffer.BlockCopy(buffer, offset, _buffer!, _position, count);
            _position = end;
            if (_position > _length) _length = _position;
        }

        public override void WriteByte(byte value)
        {
            EnsureNotDisposed();
            int end = checked(_position + 1);
            EnsureCapacity(end);
            _buffer![_position++] = value;
            if (_position > _length) _length = _position;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureNotDisposed();
            long target = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            if (target < 0 || target > int.MaxValue) throw new IOException("Geçersiz stream konumu.");
            _position = (int)target;
            return _position;
        }

        public override void SetLength(long value)
        {
            EnsureNotDisposed();
            if (value < 0 || value > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(value));
            EnsureCapacity((int)value);
            _length = (int)value;
            if (_position > _length) _position = _length;
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() { }
        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public (byte[] Buffer, int Length) Detach()
        {
            EnsureNotDisposed();
            byte[] buffer = _buffer!;
            int length = _length;
            _buffer = null;
            _detached = true;
            _position = 0;
            _length = 0;
            return (buffer, length);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_detached)
            {
                byte[]? buffer = Interlocked.Exchange(ref _buffer, null);
                if (buffer is not null) ArrayPool<byte>.Shared.Return(buffer);
            }
            base.Dispose(disposing);
        }
    }

    private sealed class LatestEncodedFrameSlot : IDisposable
    {
        private readonly object _sync = new();
        private SharedEncodedFrame? _frame;
        private int _width;
        private int _height;
        private int _fps;
        private int _quality;
        private long _frameId;
        private readonly SemaphoreSlim _signal = new(0, 1);
        private bool _disposed;

        public void PublishShared(SharedEncodedFrame frame)
        {
            if (!frame.TryAddRef()) return;
            SharedEncodedFrame? replaced = null;
            bool accepted = false;
            lock (_sync)
            {
                if (!_disposed)
                {
                    replaced = _frame;
                    _frame = frame;
                    _width = frame.Width;
                    _height = frame.Height;
                    _fps = frame.Fps;
                    _quality = frame.Quality;
                    _frameId = frame.FrameId;
                    accepted = true;
                }
            }
            if (replaced is not null) replaced.Release();
            if (!accepted)
            {
                frame.Release();
                return;
            }
            try { _signal.Release(); } catch (SemaphoreFullException) { } catch (ObjectDisposedException) { }
        }

        public async ValueTask<EncodedSnapshot?> WaitAndTakeAsync(CancellationToken token, int timeoutMilliseconds = Timeout.Infinite)
        {
            try
            {
                if (timeoutMilliseconds == Timeout.Infinite)
                {
                    await _signal.WaitAsync(token).ConfigureAwait(false);
                }
                else
                {
                    bool signalled = await _signal.WaitAsync(timeoutMilliseconds, token).ConfigureAwait(false);
                    if (!signalled) return null;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
            lock (_sync)
            {
                if (_disposed || _frame is null) return null;
                var result = new EncodedSnapshot(_frame);
                _frame = null;
                return result;
            }
        }

        public void Dispose()
        {
            SharedEncodedFrame? frame;
            lock (_sync)
            {
                if (_disposed) return;
                _disposed = true;
                frame = _frame;
                _frame = null;
            }
            frame?.Release();
            try { _signal.Dispose(); } catch { }
        }
    }

    private readonly record struct FrameProducerKey(
        string DeviceName,
        int Width,
        int Height,
        int Fps,
        int TargetMbps,
        bool AutoAdapt,
        int InitialQuality);

    private sealed class SharedFrameProducer : IDisposable
    {
        private readonly FrameProducerKey _key;
        private readonly ConcurrentDictionary<string, LatestEncodedFrameSlot> _subscribers = new(StringComparer.OrdinalIgnoreCase);
        // Browser is a non-blocking read-only tap. It must never participate in the
        // client delivery queue because a slow browser must not affect Client FPS.
        private readonly object _browserLatestSync = new();
        private SharedEncodedFrame? _browserLatest;
        private int _browserConsumers;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _worker;
        private int _disposed;

        public SharedFrameProducer(FrameProducerKey key)
        {
            _key = key;
            _worker = Task.Factory.StartNew(RunAsync, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }

        public LatestEncodedFrameSlot Subscribe(string endpoint)
        {
            ThrowIfDisposed();
            return _subscribers.GetOrAdd(endpoint, _ => new LatestEncodedFrameSlot());
        }

        public void Unsubscribe(string endpoint)
        {
            if (_subscribers.TryRemove(endpoint, out var slot)) slot.Dispose();
        }

        public bool HasSubscribers => !_subscribers.IsEmpty;
        public bool HasBrowserConsumer => Volatile.Read(ref _browserConsumers) > 0;
        public void SetBrowserConsumer(bool active)
        {
            if (active) Interlocked.Increment(ref _browserConsumers);
            else if (Interlocked.Decrement(ref _browserConsumers) < 0) Interlocked.Exchange(ref _browserConsumers, 0);
        }

        public EncodedSnapshot? TryGetLatestForBrowser()
        {
            lock (_browserLatestSync)
            {
                if (_browserLatest is null || !_browserLatest.TryAddRef()) return null;
                return new EncodedSnapshot(_browserLatest);
            }
        }

        private void PublishLatestForBrowser(SharedEncodedFrame frame)
        {
            if (!frame.TryAddRef()) return;
            SharedEncodedFrame? old;
            lock (_browserLatestSync)
            {
                old = _browserLatest;
                _browserLatest = frame;
            }
            old?.Release();
        }

        private sealed record EncodedJpegResult(byte[] Buffer, int Length, int Quality);

        private async Task RunAsync()
        {
            int quality = Math.Clamp(_key.InitialQuality, 40, 95);
            long frameId = 0;
            long deadline = Stopwatch.GetTimestamp();
            Bitmap?[] frames = new Bitmap?[2];
            Graphics?[] graphics = new Graphics?[2];
            Bitmap? sourceBitmap = null;
            Graphics? sourceGraphics = null;
            Rectangle sourceBounds = Rectangle.Empty;
            string sourceDeviceName = _key.DeviceName;
            long statStart = Stopwatch.GetTimestamp();
            long statFrames = 0;
            long statBytes = 0;
            double encodeMsTotal = 0;
            int encodeSamples = 0;
            Task<EncodedJpegResult>? pendingEncode = null;
            int pendingIndex = -1;
            void RecreateBuffers(Rectangle bounds)
            {
                sourceGraphics?.Dispose();
                sourceBitmap?.Dispose();
                sourceBitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb);
                sourceGraphics = Graphics.FromImage(sourceBitmap);
                sourceGraphics.CompositingMode = CompositingMode.SourceCopy;
                sourceGraphics.SmoothingMode = SmoothingMode.HighSpeed;
                sourceGraphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                for (int i = 0; i < 2; i++)
                {
                    graphics[i]?.Dispose();
                    frames[i]?.Dispose();
                    var bitmap = new Bitmap(_key.Width, _key.Height, PixelFormat.Format24bppRgb);
                    frames[i] = bitmap;
                    var graphicsContext = Graphics.FromImage(bitmap);
                    graphics[i] = graphicsContext;
                    graphicsContext.CompositingMode = CompositingMode.SourceCopy;
                    graphicsContext.SmoothingMode = SmoothingMode.HighSpeed;
                    graphicsContext.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                }
                sourceBounds = bounds;
            }

            Task<EncodedJpegResult> EncodeAsync(int index, int requestedQuality)
            {
                return Task.Run(() =>
                {
                    Bitmap frame = frames[index] ?? throw new InvalidOperationException("Encode yüzeyi hazır değil.");
                    using var output = new PooledBufferStream(Math.Max(256 * 1024, _key.Width * _key.Height / 5));
                    long encodeStart = Stopwatch.GetTimestamp();
                    int effectiveQuality = EncodeAdaptiveJpeg(frame, output, requestedQuality, _key.Fps, _key.TargetMbps, _key.AutoAdapt);
                    double encodeMs = Stopwatch.GetElapsedTime(encodeStart).TotalMilliseconds;
                    (byte[] rented, int length) = output.Detach();
                    encodeMsTotal += encodeMs;
                    encodeSamples++;
                    return new EncodedJpegResult(rented, length, effectiveQuality);
                }, _cts.Token);
            }

            void PublishResult(EncodedJpegResult result)
            {
                var shared = new SharedEncodedFrame(
                    result.Buffer,
                    result.Length,
                    _key.Width,
                    _key.Height,
                    _key.Fps,
                    result.Quality,
                    ++frameId);

                // Keep a single latest encoded frame for the browser as a read-only tap.
                // This is deliberately outside the client subscriber queue.
                PublishLatestForBrowser(shared);
                foreach (var subscriber in _subscribers)
                    subscriber.Value.PublishShared(shared);
                shared.Release();

                statFrames++;
                statBytes += Protocol.HeaderSize + result.Length;
                quality = Math.Clamp(result.Quality, 28, 95);
            }

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    if (_subscribers.IsEmpty && !HasBrowserConsumer)
                    {
                        if (pendingEncode is not null)
                        {
                            var idleResult = await pendingEncode.ConfigureAwait(false);
                            ArrayPool<byte>.Shared.Return(idleResult.Buffer);
                            pendingEncode = null;
                            pendingIndex = -1;
                        }
                        await Task.Delay(15, _cts.Token).ConfigureAwait(false);
                        continue;
                    }

                    Screen? currentScreen = Screen.AllScreens.FirstOrDefault(s =>
                        string.Equals(s.DeviceName, sourceDeviceName, StringComparison.OrdinalIgnoreCase));
                    if (currentScreen is null)
                    {
                        await Task.Delay(10, _cts.Token).ConfigureAwait(false);
                        continue;
                    }

                    Rectangle bounds = currentScreen.Bounds;
                    if (bounds.Width < 16 || bounds.Height < 16)
                    {
                        await Task.Delay(10, _cts.Token).ConfigureAwait(false);
                        continue;
                    }

                    if (sourceBounds != bounds || frames[0] is null || frames[1] is null)
                    {
                        if (pendingEncode is not null)
                        {
                            var modeResult = await pendingEncode.ConfigureAwait(false);
                            ArrayPool<byte>.Shared.Return(modeResult.Buffer);
                            pendingEncode = null;
                            pendingIndex = -1;
                        }
                        RecreateBuffers(bounds);
                    }

                    int captureIndex;
                    if (pendingEncode is null)
                    {
                        captureIndex = 0;
                    }
                    else
                    {
                        captureIndex = pendingIndex == 0 ? 1 : 0;
                    }

                    try
                    {
                        CaptureDesktopScaledIntoBitmap(frames[captureIndex]!, graphics[captureIndex]!, bounds, sourceBitmap!, sourceGraphics!);
                    }
                    catch (ArgumentException)
                    {
                        // Windows can transiently invalidate the desktop surface
                        // during a display-mode/topology transition. Keep the
                        // producer alive and retry instead of killing the stream.
                        await Task.Delay(20, _cts.Token).ConfigureAwait(false);
                        deadline = Stopwatch.GetTimestamp();
                        continue;
                    }
                    catch (ExternalException)
                    {
                        await Task.Delay(20, _cts.Token).ConfigureAwait(false);
                        deadline = Stopwatch.GetTimestamp();
                        continue;
                    }

                    if (pendingEncode is null)
                    {
                        pendingIndex = captureIndex;
                        pendingEncode = EncodeAsync(captureIndex, quality);
                    }
                    else
                    {
                        // The previous JPEG is encoded on another thread while this
                        // thread captures the next frame. This removes capture+encode
                        // serialization and prevents a second Client from creating a
                        // second encoder pipeline.
                        Task<EncodedJpegResult> previous = pendingEncode;
                        pendingIndex = captureIndex;
                        pendingEncode = null;

                        var result = await previous.ConfigureAwait(false);
                        PublishResult(result);

                        double frameBudget = 1000d / Math.Max(1, _key.Fps);
                        if (Stopwatch.GetElapsedTime(statStart).TotalSeconds >= 1.0)
                        {
                            double elapsed = Math.Max(0.001, Stopwatch.GetElapsedTime(statStart).TotalSeconds);
                            if (_key.AutoAdapt && encodeSamples > 0)
                            {
                                double producedMbps = statBytes * 8d / elapsed / 1_000_000d;
                                double avgEncodeMs = encodeMsTotal / encodeSamples;
                                if (avgEncodeMs > frameBudget * 0.60)
                                    quality = Math.Max(28, quality - 5);
                                else if (avgEncodeMs < frameBudget * 0.24 && producedMbps < _key.TargetMbps * 0.82)
                                    quality = Math.Min(95, quality + 2);
                            }
                            statStart = Stopwatch.GetTimestamp();
                            statFrames = 0;
                            statBytes = 0;
                            encodeMsTotal = 0;
                            encodeSamples = 0;
                        }

                        pendingEncode = EncodeAsync(captureIndex, quality);
                    }

                    double frameSeconds = 1d / Math.Max(1, _key.Fps);
                    long step = Math.Max(1, (long)(Stopwatch.Frequency * frameSeconds));
                    deadline = Math.Max(deadline + step, Stopwatch.GetTimestamp());
                    long remaining = deadline - Stopwatch.GetTimestamp();
                    if (remaining > 0)
                    {
                        int delayMs = (int)(remaining * 1000 / Stopwatch.Frequency);
                        if (delayMs > 0) await Task.Delay(delayMs, _cts.Token).ConfigureAwait(false);
                        else await Task.Yield();
                    }
                    else
                    {
                        deadline = Stopwatch.GetTimestamp();
                        await Task.Yield();
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { LogStatic($"Paylaşımlı frame üretici hatası ({_key.DeviceName}): {ex.Message}"); }
            finally
            {
                if (pendingEncode is not null)
                {
                    try
                    {
                        var result = await pendingEncode.ConfigureAwait(false);
                        ArrayPool<byte>.Shared.Return(result.Buffer);
                    }
                    catch { }
                }
                sourceGraphics?.Dispose();
                sourceBitmap?.Dispose();
                for (int i = 0; i < 2; i++)
                {
                    graphics[i]?.Dispose();
                    frames[i]?.Dispose();
                }
            }
        }

        private static void LogStatic(string message) => Debug.WriteLine(message);

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
                throw new ObjectDisposedException(nameof(SharedFrameProducer));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            _cts.Cancel();
            foreach (var kv in _subscribers)
                if (_subscribers.TryRemove(kv.Key, out var slot)) slot.Dispose();
            lock (_browserLatestSync)
            {
                _browserLatest?.Release();
                _browserLatest = null;
            }
            try { _worker.Wait(250); } catch { }
            _cts.Dispose();
        }
    }

    private static class SharedFrameProducerRegistry
    {
        private static readonly ConcurrentDictionary<FrameProducerKey, SharedFrameProducer> Producers = new();

        public static SharedFrameProducer Get(FrameProducerKey key)
        {
            while (true)
            {
                if (Producers.TryGetValue(key, out var existing)) return existing;
                var created = new SharedFrameProducer(key);
                if (Producers.TryAdd(key, created)) return created;
                created.Dispose();
            }
        }

        public static bool TryGetForBrowser(string deviceName, int width, int height, out FrameProducerKey key, out SharedFrameProducer producer)
        {
            key = default;
            producer = null!;
            SharedFrameProducer? best = null;
            FrameProducerKey bestKey = default;

            foreach (var pair in Producers)
            {
                var candidate = pair.Key;
                if (!string.Equals(candidate.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!pair.Value.HasSubscribers && !pair.Value.HasBrowserConsumer) continue;

                bool exactSize = candidate.Width == width && candidate.Height == height;
                if (best is null ||
                    (exactSize && !(bestKey.Width == width && bestKey.Height == height)) ||
                    (exactSize == (bestKey.Width == width && bestKey.Height == height) && candidate.Fps > bestKey.Fps))
                {
                    best = pair.Value;
                    bestKey = candidate;
                }
            }

            if (best is null) return false;
            key = bestKey;
            producer = best;
            return true;
        }

        public static void Release(FrameProducerKey key, SharedFrameProducer producer)
        {
            if (producer.HasSubscribers || producer.HasBrowserConsumer) return;
            if (Producers.TryRemove(new KeyValuePair<FrameProducerKey, SharedFrameProducer>(key, producer)))
                producer.Dispose();
        }
    }

    internal static BrowserFrameSubscription CreateBrowserFrameSubscriptionInternal(
        string deviceName, int width, int height, int fps, int targetMbps, bool autoAdapt, int quality)
    {
        // Browser has its OWN producer. It never consumes a Client producer
        // because that would inherit the Client's bitrate/FPS and a slow browser
        // socket would make the Web telemetry look artificially low. The producer
        // registry still shares this browser encoder between multiple browser tabs
        // viewing the same screen, so opening another tab does not create another
        // capture/encode pipeline. Client producers remain completely independent.
        int browserWidth = Math.Clamp(width, 16, 8192);
        int browserHeight = Math.Clamp(height, 16, 8192);
        const int BrowserFps = 30;
        const int BrowserMbps = 10;
        const int BrowserQuality = 55;

        var browserKey = new FrameProducerKey(
            deviceName, browserWidth, browserHeight, BrowserFps, BrowserMbps, true, BrowserQuality);
        SharedFrameProducer browserProducer = SharedFrameProducerRegistry.Get(browserKey);
        browserProducer.SetBrowserConsumer(true);

        return new BrowserFrameSubscription(
            async token =>
            {
                while (!token.IsCancellationRequested)
                {
                    EncodedSnapshot? snapshot = browserProducer.TryGetLatestForBrowser();
                    if (snapshot is not null)
                    {
                        return new BrowserFrameData(
                            snapshot.Payload, snapshot.Width, snapshot.Height,
                            snapshot.Fps, snapshot.Quality, snapshot.FrameId, snapshot.Release);
                    }
                    await Task.Delay(2, token).ConfigureAwait(false);
                }
                return null;
            },
            () =>
            {
                browserProducer.SetBrowserConsumer(false);
                SharedFrameProducerRegistry.Release(browserKey, browserProducer);
            });
    }

    private static int GetCurrentOutputWidth(Screen screen, int fallback)
    {
        int width = screen.Bounds.Width;
        return width >= 16 && width <= 8192 ? width : Math.Clamp(fallback, 16, 8192);
    }

    private static int GetCurrentOutputHeight(Screen screen, int fallback)
    {
        int height = screen.Bounds.Height;
        return height >= 16 && height <= 8192 ? height : Math.Clamp(fallback, 16, 8192);
    }

    private static int GetServerScreenRotation(Screen screen)
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

    private async Task StreamClientAsync(
        TcpClient client,
        Screen screen,
        int outWidth,
        int outHeight,
        int fps,
        int initialQuality,
        int targetMbps,
        bool autoAdapt,
        CancellationToken token,
        string endpoint)
    {
        using var stream = client.GetStream();
        using var writeLock = new SemaphoreSlim(1, 1);
        int targetFps = Math.Clamp(fps, 15, 120);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token);
        var workerToken = linked.Token;
        long connectionGeneration = Interlocked.Increment(ref _connectionGenerationSeed);
        _clientConnectionGenerations[endpoint] = connectionGeneration;
        var authWaiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _clientAuthWaiters[endpoint] = authWaiter;

        if (outWidth < 16 || outHeight < 16 || outWidth > 8192 || outHeight > 8192)
            throw new ArgumentOutOfRangeException(nameof(outWidth), "Geçersiz çıkış çözünürlüğü.");

        SharedFrameProducer? producer = null;
        FrameProducerKey producerKey = default;
        LatestEncodedFrameSlot? latest = null;
        try
        {
            Log($"Client {endpoint} • bağlantı kuruluyor");
            _clientTransports[endpoint] = client;
            _ = ReceiveClientCommandsAsync(stream, endpoint, workerToken, linked, writeLock);
            var authCompleted = await Task.WhenAny(authWaiter.Task, Task.Delay(TimeSpan.FromSeconds(4), workerToken)).ConfigureAwait(false);
            if (authCompleted != authWaiter.Task || !await authWaiter.Task.ConfigureAwait(false))
                throw new UnauthorizedAccessException("İstemci doğrulaması başarısız.");
            if (_clientConnectionGenerations.TryGetValue(endpoint, out var currentGeneration) && currentGeneration == connectionGeneration)
                _authenticatedClients[endpoint] = connectionGeneration;
            RegisterClient(endpoint);

            // Keep one encoder pipeline per screen/resolution/FPS/bitrate budget.
            // Per-client display quality is reported as telemetry, but must not
            // fork the capture/encode producer and halve FPS when a second client joins.

            _pendingScreenCommands[endpoint] = JsonSerializer.Serialize(new
            {
                type = "screen", deviceName = screen.DeviceName, name = GetScreenDisplayName(screen),
                width = screen.Bounds.Width, height = screen.Bounds.Height, rotation = GetServerScreenRotation(screen)
            });
            Log($"Client {endpoint} • JPEG yayın başladı • hedef {targetFps} FPS");

            int producerWidth = GetCurrentOutputWidth(screen, outWidth);
            int producerHeight = GetCurrentOutputHeight(screen, outHeight);
            producerKey = new FrameProducerKey(screen.DeviceName, producerWidth, producerHeight, targetFps, targetMbps, autoAdapt, Math.Clamp(initialQuality, 40, 95));
            producer = SharedFrameProducerRegistry.Get(producerKey);
            latest = producer.Subscribe(endpoint);
            string activeProducerDevice = screen.DeviceName;
            int lastSentRotation = -1;

            async Task SendAsync()
            {
                long sentFrames = 0;
                long statStart = Stopwatch.GetTimestamp();
                long statBytes = 0;
                byte[] frameHeader = new byte[Protocol.HeaderSize];
                int lastCursorX = int.MinValue, lastCursorY = int.MinValue;
                bool lastCursorVisible = false;

                while (!workerToken.IsCancellationRequested)
                {
                    var assignedNow = ResolveClientScreen(endpoint, screen);
                    if (assignedNow is not null)
                    {
                        int currentWidth = GetCurrentOutputWidth(assignedNow, outWidth);
                        int currentHeight = GetCurrentOutputHeight(assignedNow, outHeight);
                        bool deviceChanged = !string.Equals(activeProducerDevice, assignedNow.DeviceName, StringComparison.OrdinalIgnoreCase);
                        bool sizeChanged = producerKey.Width != currentWidth || producerKey.Height != currentHeight;
                        if (deviceChanged || sizeChanged)
                        {
                            producer.Unsubscribe(endpoint);
                            SharedFrameProducerRegistry.Release(producerKey, producer);
                            producerKey = new FrameProducerKey(assignedNow.DeviceName, currentWidth, currentHeight, targetFps, targetMbps, autoAdapt, Math.Clamp(initialQuality, 40, 95));
                            producer = SharedFrameProducerRegistry.Get(producerKey);
                            latest = producer.Subscribe(endpoint);
                            activeProducerDevice = assignedNow.DeviceName;
                            screen = assignedNow;
                        }
                    }

                    int currentRotation = GetServerScreenRotation(screen);
                    if (currentRotation != lastSentRotation)
                    {
                        _pendingScreenCommands[endpoint] = JsonSerializer.Serialize(new
                        {
                            type = "screen", deviceName = screen.DeviceName, name = GetScreenDisplayName(screen),
                            width = screen.Bounds.Width, height = screen.Bounds.Height, rotation = currentRotation
                        });
                        lastSentRotation = currentRotation;
                    }

                    if (_pendingScreenCommands.TryRemove(endpoint, out string? controlJson))
                    {
                        byte[] controlPayload = Encoding.UTF8.GetBytes(controlJson);
                        byte[] controlHeader = new byte[Protocol.HeaderSize];
                        Protocol.WriteControlHeader(controlHeader, controlPayload.Length);
                        await writeLock.WaitAsync(workerToken).ConfigureAwait(false);
                        try
                        {
                            await stream.WriteAsync(controlHeader, workerToken).ConfigureAwait(false);
                            await stream.WriteAsync(controlPayload, workerToken).ConfigureAwait(false);
                        }
                        finally { writeLock.Release(); }
                    }
                    if (_pendingControlCommands.TryRemove(endpoint, out string? responseJson))
                    {
                        byte[] responsePayload = Encoding.UTF8.GetBytes(responseJson);
                        byte[] responseHeader = new byte[Protocol.HeaderSize];
                        Protocol.WriteControlHeader(responseHeader, responsePayload.Length);
                        await writeLock.WaitAsync(workerToken).ConfigureAwait(false);
                        try
                        {
                            await stream.WriteAsync(responseHeader, workerToken).ConfigureAwait(false);
                            await stream.WriteAsync(responsePayload, workerToken).ConfigureAwait(false);
                        }
                        finally { writeLock.Release(); }
                    }

                    EncodedSnapshot? snapshot = latest is null
                        ? null
                        : await latest.WaitAndTakeAsync(workerToken, 10).ConfigureAwait(false);
                    if (snapshot is null) continue;

                    int sentLength = snapshot.Length;
                    Protocol.WriteHeader(frameHeader, snapshot.Width, snapshot.Height, snapshot.Fps, snapshot.FrameId, sentLength, activeProducerDevice);
                    try
                    {
                        await writeLock.WaitAsync(workerToken).ConfigureAwait(false);
                        try
                        {
                            await stream.WriteAsync(frameHeader, workerToken).ConfigureAwait(false);
                            await stream.WriteAsync(snapshot.Payload.AsMemory(0, sentLength), workerToken).ConfigureAwait(false);
                        }
                        finally { writeLock.Release(); }
                    }
                    finally
                    {
                        snapshot.Release();
                    }
                    sentFrames++;
                    statBytes += Protocol.HeaderSize + sentLength;
                    RegisterBytes(Protocol.HeaderSize + sentLength);

                    try
                    {
                        Point cursorPos = System.Windows.Forms.Cursor.Position;
                        bool visible = Screen.AllScreens.Any(s => s.DeviceName.Equals(screen.DeviceName, StringComparison.OrdinalIgnoreCase) && s.Bounds.Contains(cursorPos));
                        Rectangle cb = screen.Bounds;
                        float nx = cb.Width > 0 ? Math.Clamp((cursorPos.X - cb.Left) / (float)cb.Width, 0f, 1f) : -1f;
                        float ny = cb.Height > 0 ? Math.Clamp((cursorPos.Y - cb.Top) / (float)cb.Height, 0f, 1f) : -1f;
                        int ix = (int)(nx * 100000f), iy = (int)(ny * 100000f);
                        if (ix != lastCursorX || iy != lastCursorY || visible != lastCursorVisible)
                        {
                            _pendingControlCommands[endpoint] = JsonSerializer.Serialize(new { type = "cursor", x = nx, y = ny, visible });
                            lastCursorX = ix; lastCursorY = iy; lastCursorVisible = visible;
                        }
                    }
                    catch { }

                    if (Stopwatch.GetElapsedTime(statStart).TotalSeconds >= 1.0)
                    {
                        double elapsed = Math.Max(0.001, Stopwatch.GetElapsedTime(statStart).TotalSeconds);
                        double sentFps = sentFrames / elapsed;
                        double sentMbps = statBytes * 8d / elapsed / 1_000_000d;
                        if (_clientTelemetry.TryGetValue(endpoint, out var old))
                            _clientTelemetry[endpoint] = old with { Fps = sentFps, Mbps = sentMbps, Quality = snapshot.Quality, Width = snapshot.Width, Height = snapshot.Height };
                        else
                            _clientTelemetry[endpoint] = new ClientTelemetryState(-1, "Düşük Gecikme", sentFps, sentMbps, snapshot.Quality, snapshot.Width, snapshot.Height);
                        statStart = Stopwatch.GetTimestamp();
                        sentFrames = 0;
                        statBytes = 0;
                    }
                }
            }

            Task sender = Task.Factory.StartNew(
                () => SendAsync(), workerToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

            await sender.ConfigureAwait(false);
            linked.Cancel();
            try { await sender; } catch { }
        }
        catch (OperationCanceledException) { }
        catch (IOException) { }
        catch (Exception ex) { Log($"Client yayın hatası: {ex.Message}"); }
        finally
        {
            linked.Cancel();
            if (producer is not null)
            {
                producer.Unsubscribe(endpoint);
                SharedFrameProducerRegistry.Release(producerKey, producer);
            }
            latest?.Dispose();
            _pendingScreenCommands.TryRemove(endpoint, out _);
            _pendingControlCommands.TryRemove(endpoint, out _);
            if (_clientConnectionGenerations.TryGetValue(endpoint, out var currentGeneration) && currentGeneration == connectionGeneration)
            {
                _clientConnectionGenerations.TryRemove(endpoint, out _);
                _authenticatedClients.TryRemove(endpoint, out _);
            }
            if (_clientAuthWaiters.TryGetValue(endpoint, out var activeWaiter) && ReferenceEquals(activeWaiter, authWaiter))
                _clientAuthWaiters.TryRemove(endpoint, out _);
            _clientTelemetry.TryRemove(endpoint, out _);
            _clientTransports.TryRemove(endpoint, out _);
            UnregisterClient(endpoint);
            client.Dispose();
            Log($"Client ayrıldı: {endpoint}.");
        }
    }

    private void CloseDuplicateClientConnections(string endpoint)
    {
        string address = GetEndpointAddress(endpoint);
        if (string.IsNullOrWhiteSpace(address) || string.Equals(address, "Bilinmeyen", StringComparison.OrdinalIgnoreCase))
            return;

        foreach (var kv in _clientTransports.ToArray())
        {
            if (kv.Key.StartsWith("WEB • ", StringComparison.OrdinalIgnoreCase))
                continue;
            if (string.Equals(kv.Key, endpoint, StringComparison.OrdinalIgnoreCase))
                continue;
            if (!string.Equals(GetEndpointAddress(kv.Key), address, StringComparison.OrdinalIgnoreCase))
                continue;

            try { kv.Value.Close(); } catch { }
            UnregisterClient(kv.Key);
            _clientTelemetry.TryRemove(kv.Key, out _);
            _pendingScreenCommands.TryRemove(kv.Key, out _);
            _pendingControlCommands.TryRemove(kv.Key, out _);
        }
    }

    private static string GetEndpointAddress(string endpoint)
    {
        try
        {
            if (endpoint.StartsWith("WEB • ", StringComparison.OrdinalIgnoreCase))
                endpoint = endpoint[6..];
            if (endpoint.Contains('/')) endpoint = endpoint[..endpoint.IndexOf('/')];
            int separator = endpoint.LastIndexOf(':');
            return separator > 0 ? endpoint[..separator] : endpoint;
        }
        catch { return string.Empty; }
    }

    private static async Task SendConnectionRejectedAsync(TcpClient client, string message, CancellationToken token)
    {
        try
        {
            string json = JsonSerializer.Serialize(new { type = "connectionRejected", message });
            byte[] payload = Encoding.UTF8.GetBytes(json);
            byte[] header = new byte[Protocol.HeaderSize];
            Protocol.WriteControlHeader(header, payload.Length);
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(header, token).ConfigureAwait(false);
            await stream.WriteAsync(payload, token).ConfigureAwait(false);
        }
        catch { }
    }

    private async Task ReceiveClientCommandsAsync(NetworkStream stream, string endpoint, CancellationToken token, CancellationTokenSource connectionLifetime, SemaphoreSlim writeLock)
    {
        byte[] buffer = new byte[2048];
        var pending = new StringBuilder();

        try
        {
            while (!token.IsCancellationRequested)
            {
                int count = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);
                if (count <= 0)
                {
                    connectionLifetime.Cancel();
                    break;
                }

                pending.Append(Encoding.UTF8.GetString(buffer, 0, count));

                while (true)
                {
                    string all = pending.ToString();
                    int newline = all.IndexOf('\n');
                    if (newline < 0) break;

                    string line = all[..newline].Trim();
                    pending.Remove(0, newline + 1);

                    if (line.Length == 0) continue;
                    HandleRemoteClientCommand(endpoint, line);
                    bool immediateResponse = line.StartsWith("POLARIS_AUTH|", StringComparison.OrdinalIgnoreCase) ||
                                             line.StartsWith("POLARIS_SELECT|", StringComparison.OrdinalIgnoreCase);
                    if (immediateResponse && _pendingControlCommands.TryRemove(endpoint, out string? controlResponse))
                    {
                        byte[] payload = Encoding.UTF8.GetBytes(controlResponse);
                        var controlHeader = new byte[Protocol.HeaderSize];
                        Protocol.WriteControlHeader(controlHeader, payload.Length);
                        await writeLock.WaitAsync(token);
                        try
                        {
                            await stream.WriteAsync(controlHeader, token);
                            await stream.WriteAsync(payload, token);
                        }
                        finally { writeLock.Release(); }
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (IOException) { connectionLifetime.Cancel(); }
        catch (Exception ex)
        {
            connectionLifetime.Cancel();
            Log($"Uzak komut hatası ({endpoint}): {ex.Message}");
        }
    }

    private void ApplyClientScreenSelection(string endpoint, string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName)) return;
        var target = GetRemoteScreens().FirstOrDefault(s => string.Equals(s.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));
        if (target is null)
        {
            Log($"{endpoint} → geçersiz Polaris ekranı: {deviceName}");
            return;
        }

        lock (_assignmentSync)
        {
            bool occupied = _activeClients.Keys.Any(other =>
                !string.Equals(other, endpoint, StringComparison.OrdinalIgnoreCase) &&
                _clientScreenAssignments.TryGetValue(other, out var assigned) &&
                string.Equals(assigned, target.DeviceName, StringComparison.OrdinalIgnoreCase));

            if (occupied)
            {
                Log($"{endpoint} → {GetScreenDisplayName(target)} zaten başka bağlantıda.");
                return;
            }

            _clientScreenAssignments[endpoint] = target.DeviceName;
        }

        Log($"{endpoint} → {GetScreenDisplayName(target)} ({target.Bounds.Width}×{target.Bounds.Height})");
        try
        {
            if (!IsDisposed) BeginInvoke((Action)RebuildConnectionsView);
        }
        catch { }
    }

    private static void CaptureDesktopScaledIntoBitmap(
        Bitmap bitmap,
        Graphics graphics,
        Rectangle bounds,
        Bitmap sourceBitmap,
        Graphics sourceGraphics)
    {
        if (bounds.Width < 16 || bounds.Height < 16)
            throw new ArgumentOutOfRangeException(nameof(bounds));

        // Use the WinForms/GDI capture path that the original working build used.
        // It is reliable for the current Windows desktop topology, including
        // indirect/virtual monitors, whereas capturing the desktop through a
        // process-global HDC can return a black surface on display transitions.
        sourceGraphics.CopyFromScreen(
            bounds.Left,
            bounds.Top,
            0,
            0,
            bounds.Size,
            CopyPixelOperation.SourceCopy);

        graphics.CompositingMode = CompositingMode.SourceCopy;
        if (bitmap.Width == bounds.Width && bitmap.Height == bounds.Height)
        {
            graphics.DrawImageUnscaled(sourceBitmap, 0, 0);
            return;
        }

        graphics.InterpolationMode = InterpolationMode.Bilinear;
        graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
        graphics.SmoothingMode = SmoothingMode.HighSpeed;
        graphics.DrawImage(
            sourceBitmap,
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            0,
            0,
            bounds.Width,
            bounds.Height,
            GraphicsUnit.Pixel);
    }

    private static int EncodeAdaptiveJpeg(
        Bitmap frame,
        Stream output,
        int quality,
        int fps,
        int targetMbps,
        bool autoAdapt)
    {
        int q = Math.Clamp(quality, 25, 95);
        long targetBytesPerFrame = Math.Max(1024L, targetMbps * 1_000_000L / 8L / Math.Max(1, fps));
        if (!autoAdapt)
        {
            double pixels = Math.Max(1d, frame.Width * (double)frame.Height);
            double bitsPerPixel = targetBytesPerFrame * 8d / pixels;
            int bitrateQuality = bitsPerPixel switch
            {
                < 0.06 => 25,
                < 0.10 => 32,
                < 0.16 => 40,
                < 0.24 => 48,
                < 0.36 => 58,
                < 0.52 => 68,
                < 0.72 => 76,
                < 0.95 => 84,
                < 1.30 => 90,
                _ => 95
            };
            q = Math.Min(q, bitrateQuality);
        }

        output.SetLength(0);
        output.Position = 0;
        if (s_jpegCodec is null)
            throw new InvalidOperationException("Windows JPEG codec bulunamadı.");

        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)q);
        frame.Save(output, s_jpegCodec, parameters);

        if (!autoAdapt) return q;

        double ratio = output.Length / (double)targetBytesPerFrame;
        if (ratio > 1.45) return Math.Max(28, q - 4);
        if (ratio > 1.15) return Math.Max(32, q - 2);
        if (ratio < 0.55) return Math.Min(95, q + 2);
        return q;
    }

    private static int GetBestLinkSpeedMbps()
    {
        long best = 0;

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up)
                continue;

            if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            if (nic.Speed > best)
                best = nic.Speed;
        }

        if (best <= 0)
            return 100;

        return (int)Math.Clamp(best / 1_000_000L, 1L, 10_000L);
    }



    private static string? GetDisplayString(string deviceName)
    {
        var dd = new DisplayInterop.DISPLAY_DEVICE { cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>() };

        for (uint i = 0; DisplayInterop.EnumDisplayDevices(deviceName, i, ref dd, 0); i++)
        {
            if (!string.IsNullOrWhiteSpace(dd.DeviceString))
                return dd.DeviceString;

            dd = new DisplayInterop.DISPLAY_DEVICE { cb = Marshal.SizeOf<DisplayInterop.DISPLAY_DEVICE>() };
        }

        return null;
    }

    private void SetRunning(bool running)
    {
        _port.Enabled = !running;
        _fps.Enabled = !running;
        _quality.Enabled = !running;
        _resolution.Enabled = !running;
        _bitrateMode.Enabled = !running;
        _bitrate.Enabled = !running && _bitrateMode.SelectedIndex == 1;
        _screen.Enabled = !running;
        _refresh.Enabled = !running;

        _start.Text = running ? "DURDUR" : "BAŞLAT";
        _start.BackColor = running
            ? Color.FromArgb(190, 70, 70)
            : Color.FromArgb(40, 166, 105);
    }

    private void SetStatus(string text)
    {
        if (InvokeRequired)
        {
            try { BeginInvoke(() => _status.Text = text); } catch { }
            return;
        }

        _status.Text = text;
    }

    private void SetStats(string text)
    {
        if (InvokeRequired)
        {
            try { BeginInvoke(() => _status.Text = text); } catch { }
            return;
        }

        _status.Text = text;
    }

    private void Log(string text)
    {
        if (InvokeRequired)
        {
            try { BeginInvoke(() => Log(text)); } catch { }
            return;
        }

        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}\r\n");
        _log.SelectionStart = _log.TextLength;
        _log.ScrollToCaret();
    }

    private void StopServer()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _listener = null;
        _cts?.Dispose();
        _cts = null;
        SetRunning(false);
        _broadcastStartedUtc = default;
        _browserViewer?.SetBroadcastEnabled(false);
        foreach (var transport in _clientTransports.Values.ToArray())
        {
            try { transport.Close(); } catch { }
        }
        _clientTransports.Clear();
        _pendingScreenCommands.Clear();
        _pendingControlCommands.Clear();
        _clientTelemetry.Clear();
        _clientAuthWaiters.Clear();
        _authenticatedClients.Clear();
        _clientConnectionGenerations.Clear();
        lock (_assignmentSync)
        {
            _clientConnectedAt.Clear();
            _clientScreenAssignments.Clear();
            _clientDisplayNames.Clear();
        }
        _activeClients.Clear();
        _status.Text = "Durduruldu";
    }

}
