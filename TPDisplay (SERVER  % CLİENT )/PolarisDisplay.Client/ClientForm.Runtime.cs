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

public sealed partial class ClientForm : Form
{
    private TcpClient? _client;
    private CancellationTokenSource? _cts;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private bool _autoReconnect;
    private int _connectionBusy;
    private readonly System.Windows.Forms.Timer _reconnectTimer = new() { Interval = 500 };
    private readonly System.Windows.Forms.Timer _pingTimer = new() { Interval = 1200 };
    private readonly ClientSettings _settings = ClientSettingsStore.Load();
    private long _lastPingSentTicks;
    private long _lastBytesReceived;
    private long _lastNetworkBytesSample;
    private DateTime _lastNetworkStatUtc = DateTime.UtcNow;
    private int _serverQuality;
    private int _serverRotation;
    private double _serverFps;
    private double _serverMbps;
    private string _confirmedServerScreen = string.Empty;
    private int _screenSelectionGeneration;
    private ushort _expectedScreenToken;
    private ushort _pendingScreenToken;
    private readonly System.Windows.Forms.Timer _screenNoticeTimer = new() { Interval = 2200 };
    private readonly ToolTip _screenToast = new();
    private double _networkMbps;
    private int _lastPingMs = -1;
    private string _streamMode = "Düşük Gecikme";
    private TaskCompletionSource<bool>? _authWaiter;
    private volatile bool _serverRejected;
    private volatile bool _pinPromptCancelled;
    private string _serverRejectReason = string.Empty;


    private long _frameCount;
    private long _shown;
    private readonly Stopwatch _statClock = Stopwatch.StartNew();

    private bool _isFullscreen;
    private long _overlayLastMoveTick;
    private FormBorderStyle _oldBorder;
    private Padding _oldFormPadding;
    private Padding _oldContentPadding;
    private FormWindowState _oldState;
    private Rectangle _oldBounds;
    private bool _normalBoundsCaptured;
    internal bool IsViewerFullscreen => _isFullscreen;


    private void ApplySavedSettings()
    {
        _host.Text = string.Empty;
        _port.Value = Math.Clamp(_settings.Port, (int)_port.Minimum, (int)_port.Maximum);
        if (!_settings.AutoDiscover) _host.Text = _settings.Host;
        _streamMode = string.IsNullOrWhiteSpace(_settings.StreamMode) ? "Düşük Gecikme" : _settings.StreamMode;
        _autoReconnect = _settings.AutoReconnect;
    }

    private void ShowSettings()
    {
        if (ClientSettingsDialog.Show(this, _settings))
        {
            ApplySavedSettings();
            _ = CheckForClientUpdateAsync();
            if (_client is not null)
            {
                // A PIN change takes effect on the next authenticated connection.
                _ = SendRemoteCommandAsync($"POLARIS_MODE|{Uri.EscapeDataString(_streamMode)}\n");
            }
        }
    }

    private async Task CheckForClientUpdateAsync()
    {
        var m = await ClientUpdateService.CheckAsync(_settings.UpdateManifestUrl);
        if (m is null || !ClientUpdateService.IsNewer(ClientUpdateService.Version, m.Version)) return;
        if (InvokeRequired) { BeginInvoke(new Action(async () => await CheckForClientUpdateAsync())); return; }
        var r = MessageBox.Show(this, $"Yeni PolarisDisplay Client sürümü bulundu: {m.Version}\n\n{m.Notes}\n\nİndirme sayfasını açmak ister misiniz?", "PolarisDisplay Güncelleme", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (r == DialogResult.Yes && Uri.TryCreate(m.Url, UriKind.Absolute, out var uri)) Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute=true });
    }

    private async Task DiscoverServerAsync()
    {
        if (!_settings.AutoDiscover) return;
        var found = await LanDiscovery.FindServerAsync(CancellationToken.None);
        if (found is null)
        {
            _host.Text = string.Empty;
            _status.Text = "Sunucu bulunamadı";
            return;
        }
        _host.Text = found.Host;
        _port.Value = Math.Clamp(found.Port, (int)_port.Minimum, (int)_port.Maximum);
        _status.Text = $"Bulundu: {found.Name} • {found.Host}:{found.Port}";
        await LoadServerScreensAsync();
    }

    private async Task SendPingAsync()
    {
        if (_client is null) return;
        _lastPingSentTicks = Stopwatch.GetTimestamp();
        await SendRemoteCommandAsync($"POLARIS_PING|{_lastPingSentTicks}\n");
        long bytes = Interlocked.Read(ref _lastBytesReceived);
        double seconds = Math.Max(0.001, (DateTime.UtcNow - _lastNetworkStatUtc).TotalSeconds);
        _networkMbps = Math.Max(0, (bytes - _lastNetworkBytesSample) * 8d / seconds / 1_000_000d);
        _lastNetworkBytesSample = bytes;
        _lastNetworkStatUtc = DateTime.UtcNow;
    }

    private async Task ToggleConnectionAsync()
    {
        // Disconnect is always allowed immediately. ReceiveLoop owns the
        // connection while connected, so waiting on _connectionBusy here would
        // make the "BAĞLANTIYI KES" button permanently unresponsive.
        if (_client is not null)
        {
            _autoReconnect = false;
            Disconnect();
            return;
        }

        if (Interlocked.Exchange(ref _connectionBusy, 1) != 0) return;

        // Remove any stale transport state left by a just-closed connection before
        // starting a new authentication handshake. This prevents the old receive
        // loop from racing the new PIN waiter when the same server endpoint reconnects quickly.
        try { _cts?.Cancel(); } catch { }
        try { _client?.Close(); } catch { }
        _client = null;
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var client = new TcpClient
        {
            NoDelay = true,
            ReceiveBufferSize = 96 * 1024,
            SendBufferSize = 96 * 1024
        };

        try
        {
            _pinPromptCancelled = false;
            _status.Text = "Bağlanıyor...";
            if (_settings.AutoDiscover && string.IsNullOrWhiteSpace(_host.Text))
                await DiscoverServerAsync();

            string host = _host.Text.Trim();
            if (string.IsNullOrWhiteSpace(host))
                throw new SocketException((int)SocketError.HostNotFound);

            await LoadServerScreensAsync();

            await client.ConnectAsync(
                host,
                (int)_port.Value,
                _cts.Token);

            _client = client;
            _autoReconnect = _settings.AutoReconnect;
            _cardConnect.Text = "BAĞLANTIYI KES";
            _cardConnect.ForeColor = Color.White;
            _cardConnect.GradientStart = Color.FromArgb(165, 35, 70);
            _cardConnect.GradientEnd = Color.FromArgb(225, 20, 155);
            _status.Text = "Bağlantı doğrulanıyor...";

            _serverRejected = false;
            _serverRejectReason = string.Empty;
            _authWaiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Task receiveTask = ReceiveLoopAsync(client, _cts.Token);
            string configuredPin = _settings.Pin?.Trim() ?? string.Empty;
            await SendRemoteCommandAsync($"POLARIS_AUTH|{configuredPin}\n");

            Task authTimeout = Task.Delay(TimeSpan.FromSeconds(4), _cts.Token);
            Task completed = await Task.WhenAny(_authWaiter.Task, authTimeout, receiveTask);
            if (_serverRejected)
            {
                _autoReconnect = false;
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(_serverRejectReason) ? "Sunucu bağlantıyı reddetti." : _serverRejectReason);
            }
            if (completed == receiveTask && !_authWaiter.Task.IsCompleted)
            {
                _autoReconnect = false;
                throw new IOException("Server doğrulama sırasında bağlantıyı kapattı.");
            }

            bool authenticated = completed == _authWaiter.Task && await _authWaiter.Task;
            if (!authenticated)
            {
                _autoReconnect = false;
                if (_pinPromptCancelled)
                    throw new OperationCanceledException("PIN girişi kullanıcı tarafından iptal edildi.");
                throw new UnauthorizedAccessException("PIN doğrulaması başarısız.");
            }

            _viewPanel.Visible = true;
            _quickPanel.Visible = false;
            SelectTab(1);
            await SendRemoteCommandAsync($"POLARIS_HELLO|{Environment.MachineName}\n");
            await SendRemoteCommandAsync($"POLARIS_MODE|{Uri.EscapeDataString(_streamMode)}\n");
            RememberConnection();
            UpdateConnectionHeader(true);

            // Send the selected server display only after authentication and HELLO.
            if (!string.IsNullOrWhiteSpace(_selectedServerScreen))
                await SendScreenSelectionAsync(_selectedServerScreen);

            await receiveTask;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (_autoReconnect)
                _status.Text = "Bağlantı yok • yeniden deneniyor";
            else
            {
                try { MessageBox.Show(this, ex.Message, "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
            }
        }
        finally
        {
            _authWaiter = null;
            Disconnect();
            if (_autoReconnect && !IsDisposed) _status.Text = "Bağlantı koptu • yeniden deneniyor";
            Interlocked.Exchange(ref _connectionBusy, 0);
        }
    }

    private string? PromptForPin()
    {
        using var dialog = new PinDialogForm();
        return dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.Pin) ? dialog.Pin.Trim() : null;
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    private void HandleServerControlMessage(string message)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(message);
            if (!doc.RootElement.TryGetProperty("type", out var type)) return;
            string kind = type.GetString() ?? string.Empty;
            if (string.Equals(kind, "authRequired", StringComparison.OrdinalIgnoreCase))
            {
                // A PIN saved in Client Settings is the connection credential.
                // Never interrupt an automatic connection with a dialog when it exists.
                string configuredPin = _settings.Pin?.Trim() ?? string.Empty;
                string? pin = string.IsNullOrWhiteSpace(configuredPin) ? null : configuredPin;
                if (pin is null)
                {
                    try
                    {
                        if (IsHandleCreated && !IsDisposed)
                            pin = (string?)Invoke(new Func<string?>(PromptForPin));
                    }
                    catch { }
                }

                if (pin is null)
                {
                    _pinPromptCancelled = true;
                    _autoReconnect = false;
                    _authWaiter?.TrySetResult(false);
                    try { _cts?.Cancel(); } catch { }
                    SetUiText(_status, "PIN girişi iptal edildi • otomatik bağlantı kapatıldı");
                    return;
                }

                _ = SendRemoteCommandAsync($"POLARIS_AUTH|{pin}\n");
                return;
            }

            if (string.Equals(kind, "connectionRejected", StringComparison.OrdinalIgnoreCase))
            {
                _serverRejected = true;
                _serverRejectReason = doc.RootElement.TryGetProperty("message", out var rm)
                    ? rm.GetString() ?? "Sunucu bağlantıyı reddetti."
                    : "Sunucu bağlantıyı reddetti.";
                _autoReconnect = false;
                _authWaiter?.TrySetResult(false);
                SetUiText(_status, _serverRejectReason);
                return;
            }

            if (string.Equals(kind, "authResult", StringComparison.OrdinalIgnoreCase))
            {
                bool success = doc.RootElement.TryGetProperty("success", out var authOk) && authOk.GetBoolean();
                string authMessage = doc.RootElement.TryGetProperty("message", out var authMsg) ? authMsg.GetString() ?? string.Empty : string.Empty;
                if (!success)
                {
                    // One explicit authentication rejection is enough to stop the
                    // automatic reconnect loop. The user can press BAĞLAN manually.
                    _autoReconnect = false;
                    SetUiText(_status, string.IsNullOrWhiteSpace(authMessage) ? "PIN doğrulaması başarısız • otomatik bağlantı kapatıldı" : authMessage + " • otomatik bağlantı kapatıldı");
                    try { _cts?.Cancel(); } catch { }
                }
                else
                {
                    // Keep the credential that just authenticated the connection so
                    // subsequent reconnects never need an interactive PIN prompt.
                    string configuredPin = _settings.Pin?.Trim() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(configuredPin))
                        ClientSettingsStore.Save(_settings);
                }
                _authWaiter?.TrySetResult(success);
                return;
            }

            if (string.Equals(kind, "pong", StringComparison.OrdinalIgnoreCase))
            {
                if (doc.RootElement.TryGetProperty("sentTicks", out var sent))
                {
                    long sentTicks = sent.GetInt64();
                    _lastPingMs = (int)Math.Clamp(Stopwatch.GetElapsedTime(sentTicks).TotalMilliseconds, 0, 60000);
                    _ = SendRemoteCommandAsync($"POLARIS_PING_RESULT|{_lastPingMs}\n");
                }
                return;
            }
            if (string.Equals(kind, "screenResult", StringComparison.OrdinalIgnoreCase))
            {
                bool success = doc.RootElement.TryGetProperty("success", out var ok) && ok.GetBoolean();
                string msg = doc.RootElement.TryGetProperty("message", out var mm) ? mm.GetString() ?? string.Empty : string.Empty;
                string resultDeviceName = doc.RootElement.TryGetProperty("deviceName", out var dn) ? dn.GetString() ?? string.Empty : string.Empty;
                int resultRotation = doc.RootElement.TryGetProperty("rotation", out var rr) && rr.TryGetInt32(out var rrv) ? rrv : 0;
                _serverRotation = ((resultRotation % 360) + 360) % 360;
                try { RemoteDisplayOrientation.Apply(this, _serverRotation); } catch { }
                void ApplyResult()
                {
                    _serverScreenSelect.Enabled = true;
                    if (success && !string.IsNullOrWhiteSpace(resultDeviceName))
                    {
                        _selectedServerScreen = resultDeviceName;
                        _confirmedServerScreen = resultDeviceName;
                        // The TCP stream is ordered and the server serializes the screenResult
                        // with frame writes. Once the server acknowledges the switch, all
                        // subsequent JPEG frames belong to the acknowledged screen. Switch the
                        // accepted token now, but keep the last rendered frame alive until the
                        // first frame from the new screen arrives. This prevents black flashes.
                        _expectedScreenToken = Protocol.ScreenToken(resultDeviceName);
                        _pendingScreenToken = 0;
                        _screenSelectionGeneration++;
                        _applyingServerScreenSelection = true;
                        try
                        {
                            for (int i = 0; i < _serverScreenSelect.Items.Count; i++)
                            {
                                if (_serverScreenSelect.Items[i] is RemoteScreen rr &&
                                    string.Equals(rr.DeviceName, resultDeviceName, StringComparison.OrdinalIgnoreCase))
                                {
                                    _serverScreenSelect.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                        finally { _applyingServerScreenSelection = false; }
                        _status.Text = msg;
                    }
                    else
                    {
                        _pendingScreenToken = 0;
                        _expectedScreenToken = string.IsNullOrWhiteSpace(_confirmedServerScreen)
                            ? (ushort)0
                            : Protocol.ScreenToken(_confirmedServerScreen);
                        _status.Text = string.IsNullOrWhiteSpace(msg) ? "Bu ekran şu anda kullanılıyor" : msg;
                        _loadingServerScreens = true;
                        try
                        {
                            for (int i=0;i<_serverScreenSelect.Items.Count;i++)
                                if (_serverScreenSelect.Items[i] is RemoteScreen rr && string.Equals(rr.DeviceName,_confirmedServerScreen,StringComparison.OrdinalIgnoreCase)) { _serverScreenSelect.SelectedIndex=i; break; }
                        }
                        finally { _loadingServerScreens=false; }
                    }
                    _screenToast.Show(msg, _serverScreenSelect, 1500);
                    _screenNoticeTimer.Stop(); _screenNoticeTimer.Start();
                }
                if (IsHandleCreated && InvokeRequired) BeginInvoke((Action)ApplyResult); else ApplyResult();
                return;
            }

            if (string.Equals(kind, "telemetry", StringComparison.OrdinalIgnoreCase))
            {
                if (doc.RootElement.TryGetProperty("fps", out var tfps)) _serverFps = tfps.GetDouble();
                if (doc.RootElement.TryGetProperty("mbps", out var tmp)) _serverMbps = tmp.GetDouble();
                if (doc.RootElement.TryGetProperty("quality", out var tq)) _serverQuality = tq.GetInt32();
                return;
            }

            if (string.Equals(kind, "cursor", StringComparison.OrdinalIgnoreCase))
            {
                float x = doc.RootElement.TryGetProperty("x", out var cx) ? cx.GetSingle() : -1f;
                float y = doc.RootElement.TryGetProperty("y", out var cy) ? cy.GetSingle() : -1f;
                bool visible = !doc.RootElement.TryGetProperty("visible", out var cv) || cv.GetBoolean();
                if (IsHandleCreated && InvokeRequired) BeginInvoke((Action)(() => _view.SetRemoteCursor(x, y, visible, _serverRotation)));
                else _view.SetRemoteCursor(x, y, visible, _serverRotation);
                return;
            }

            if (!string.Equals(kind, "screen", StringComparison.OrdinalIgnoreCase))
                return;

            string requestedDeviceName = doc.RootElement.TryGetProperty("deviceName", out var d)
                ? d.GetString() ?? string.Empty
                : string.Empty;
            int requestedRotation = doc.RootElement.TryGetProperty("rotation", out var rot) && rot.TryGetInt32(out var rv) ? rv : 0;
            _serverRotation = ((requestedRotation % 360) + 360) % 360;

            if (string.IsNullOrWhiteSpace(requestedDeviceName))
                return;

            try { RemoteDisplayOrientation.Apply(this, requestedRotation); } catch { }

            void Apply()
            {
                _selectedServerScreen = requestedDeviceName;
                _confirmedServerScreen = requestedDeviceName;
                _expectedScreenToken = Protocol.ScreenToken(requestedDeviceName);
                _pendingScreenToken = 0;
                _serverScreenSelect.Enabled = true;
                if (_serverScreenSelect is null) return;

                try
                {
                    _loadingServerScreens = true;
                    int match = -1;
                    for (int i = 0; i < _serverScreenSelect.Items.Count; i++)
                    {
                        if (_serverScreenSelect.Items[i] is RemoteScreen item &&
                            string.Equals(item.DeviceName, requestedDeviceName, StringComparison.OrdinalIgnoreCase))
                        {
                            match = i;
                            break;
                        }
                    }
                    if (match >= 0)
                        _serverScreenSelect.SelectedIndex = match;
                }
                finally
                {
                    _loadingServerScreens = false;
                }
            }

            if (IsHandleCreated && InvokeRequired)
                BeginInvoke((Action)Apply);
            else
                Apply();
        }
        catch { }
    }

    private async Task ReceiveLoopAsync(
        TcpClient client,
        CancellationToken token)
    {
        using var stream = client.GetStream();
        stream.ReadTimeout = Timeout.Infinite;
        using var loopCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        var loopToken = loopCts.Token;

        // Network I/O and JPEG decoding are deliberately separated. A bounded
        // latest-frame queue prevents TCP/decoder backlog from turning into
        // seconds of latency when the client cannot decode every frame.
        var gate = new SemaphoreSlim(0);
        var sync = new object();
        FramePacket? latest = null;
        Exception? readerError = null;

        async Task ReaderAsync()
        {
            var header = new byte[Protocol.HeaderSize];
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    await ReadExactlyAsync(stream, header, loopToken);

                    if (!Protocol.Valid(header))
                    {
                        var found = await ResyncHeaderAsync(stream, header, loopToken);
                        if (!found)
                            throw new InvalidDataException(
                                $"JPEG v9 header geçersiz: {BitConverter.ToString(header, 0, 8)}");
                    }

                    int payloadLength = Protocol.I32(header, 28);

                    if (payloadLength <= 0 || payloadLength > Protocol.MaxPayload)
                        throw new InvalidDataException("Frame payload bilgileri geçersiz.");

                    if (Protocol.Kind(header) == 2)
                    {
                        byte[] control = ArrayPool<byte>.Shared.Rent(payloadLength);
                        try
                        {
                            await ReadExactlyAsync(stream, control.AsMemory(0, payloadLength), loopToken);
                            string message = Encoding.UTF8.GetString(control, 0, payloadLength);
                            HandleServerControlMessage(message);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(control);
                        }
                        continue;
                    }

                    ushort screenToken = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(6, 2));
                    int width = Protocol.I32(header, 8);
                    int height = Protocol.I32(header, 12);
                    int fps = Protocol.I32(header, 16);
                    long frameId = Protocol.I64(header, 20);

                    if (width <= 0 || height <= 0 || width > 8192 || height > 8192)
                        throw new InvalidDataException("JPEG frame bilgileri geçersiz.");

                    byte[] payload = ArrayPool<byte>.Shared.Rent(payloadLength);
                    try
                    {
                        await ReadExactlyAsync(stream, payload.AsMemory(0, payloadLength), loopToken);
                        Interlocked.Add(ref _lastBytesReceived, payloadLength + Protocol.HeaderSize);
                    }
                    catch
                    {
                        ArrayPool<byte>.Shared.Return(payload);
                        throw;
                    }

                    var packet = new FramePacket(payload, payloadLength, width, height, fps, frameId, screenToken);
                    FramePacket? old;
                    lock (sync)
                    {
                        old = latest;
                        latest = packet;
                    }

                    if (old is not null)
                        ArrayPool<byte>.Shared.Return(old.Buffer);

                    gate.Release();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                readerError = ex;
                try { loopCts.Cancel(); } catch { }
                try { gate.Release(); } catch { }
            }
        }

        async Task DecoderAsync()
        {
            long lastStatusUiTick = 0;
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    await gate.WaitAsync(loopToken);

                    FramePacket? packet;
                    lock (sync)
                    {
                        packet = latest;
                        latest = null;
                    }

                    if (packet is null)
                        continue;

                    // The TCP control channel is authoritative for screen selection.
                    // Do not discard otherwise valid JPEG frames solely because a
                    // transition frame still carries the previous screen token; doing
                    // so can leave the Client permanently black during a screen switch.
                    ushort pendingToken = Volatile.Read(ref _pendingScreenToken);
                    if (pendingToken != 0 && packet.ScreenToken == pendingToken)
                    {
                        Volatile.Write(ref _expectedScreenToken, pendingToken);
                        Volatile.Write(ref _pendingScreenToken, (ushort)0);
                    }

                    bool transferred = false;
                    try
                    {
                        var ms = new MemoryStream(
                            packet.Buffer, 0, packet.Length, writable: false, publiclyVisible: true);
                        Image image;
                        try
                        {
                            image = Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: false);
                        }
                        catch
                        {
                            ms.Dispose();
                            throw;
                        }

                        var decoded = new DecodedFrame(packet.Buffer, ms, image);
                        transferred = true;
                        ShowFrame(decoded);

                        _frameCount++;
                        _shown++;

                        if (_statClock.ElapsedMilliseconds >= 1000)
                        {
                            double nowMbps = _networkMbps;
                            int ping = _lastPingMs;
                            double shownFps = _serverFps > 0 ? _serverFps : _shown;
                            double shownMbps = _serverMbps > 0 ? _serverMbps : nowMbps;
                            string q = _serverQuality > 0 ? _serverQuality.ToString() : "—";
                            SetUiText(_stats, $"FPS {shownFps:0} • {shownMbps:0.0} Mbps • Ping {(ping < 0 ? "—" : ping.ToString())} ms • Q{q} • {packet.Width}×{packet.Height}");
                            _shown = 0;
                            _statClock.Restart();
                        }

                        long statusTick = Environment.TickCount64;
                        if (statusTick - lastStatusUiTick >= 500)
                        {
                            lastStatusUiTick = statusTick;
                            SetUiText(_status, $"JPEG • {packet.Fps} FPS hedef • Frame {packet.FrameId}");
                        }
                    }
                    finally
                    {
                        if (!transferred)
                            ArrayPool<byte>.Shared.Return(packet.Buffer);
                    }

                    if (readerError is not null)
                        throw readerError;
                }
            }
            catch (OperationCanceledException) { }
        }

        var reader = ReaderAsync();
        var decoder = DecoderAsync();
        await Task.WhenAny(reader, decoder);

        if (readerError is not null && !token.IsCancellationRequested)
            throw readerError;

        loopCts.Cancel();
        try { await reader; } catch (OperationCanceledException) { }
        try { await decoder; } catch (OperationCanceledException) { }

        lock (sync)
        {
            if (latest is not null)
            {
                ArrayPool<byte>.Shared.Return(latest.Buffer);
                latest = null;
            }
        }

        gate.Dispose();
    }

    private sealed record FramePacket(
        byte[] Buffer,
        int Length,
        int Width,
        int Height,
        int Fps,
        long FrameId,
        ushort ScreenToken);

    private static async Task<bool> ResyncHeaderAsync(
        NetworkStream stream,
        byte[] current,
        CancellationToken token)
    {
        // Search for "PJP9" in the byte stream. This makes accidental stale
        // bytes recoverable, while v9 itself never emits ambiguous probe frames.
        var window = new byte[Protocol.HeaderSize];

        Buffer.BlockCopy(current, 0, window, 0, current.Length);

        for (int i = 1; i < window.Length; i++)
        {
            Array.Copy(window, i, window, 0, window.Length - i);

            int need = i;
            await ReadExactlyAsync(
                stream,
                window.AsMemory(window.Length - need, need),
                token);

            if (Protocol.Valid(window))
            {
                Buffer.BlockCopy(window, 0, current, 0, Protocol.HeaderSize);
                return true;
            }
        }

        return false;
    }

    private static async Task ReadExactlyAsync(
        NetworkStream stream,
        Memory<byte> buffer,
        CancellationToken token)
    {
        int offset = 0;

        while (offset < buffer.Length)
        {
            int n = await stream.ReadAsync(
                buffer.Slice(offset),
                token);

            if (n == 0)
                throw new IOException("Server bağlantıyı kapattı.");

            offset += n;
        }
    }

    private void SetUiText(Control control, string text)
    {
        if (control.IsDisposed) return;
        try
        {
            if (control.InvokeRequired)
                control.BeginInvoke((Action)(() => control.Text = text));
            else
                control.Text = text;
        }
        catch { }
    }

    private void ShowFrame(DecodedFrame frame)
        => _view.SetFrame(frame);

    private async Task SendRemoteCommandAsync(string message)
    {
        TcpClient? client = _client;
        if (client is null) return;
        await _sendLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!ReferenceEquals(_client, client) || !client.Connected) return;
            byte[] data = Encoding.UTF8.GetBytes(message);
            await client.GetStream().WriteAsync(data).ConfigureAwait(false);
        }
        catch { }
        finally { _sendLock.Release(); }
    }

    private void Disconnect()
    {
        // Restore the normal viewer window BEFORE clearing the connection state.
        // This guarantees disconnecting from fullscreen returns to the exact
        // pre-fullscreen rectangle instead of leaving the borderless form maximized.
        if (_isFullscreen)
            ExitFullscreen();

        try { RemoteDisplayOrientation.Restore(); } catch { }
        _cts?.Cancel();
        try { _client?.Close(); } catch { }

        _client = null;
        _fullscreenDisconnectButton.Visible = false;
        _fullscreenExitButton.Visible = false;

        _cts?.Dispose();
        _cts = null;

        _cardConnect.Text = "BAĞLAN";
        _cardConnect.ForeColor = Color.White;
        _cardConnect.GradientStart = Color.FromArgb(0, 120, 255);
        _cardConnect.GradientEnd = Color.FromArgb(225, 0, 220);
        _status.Text = "Bağlantı yok";
        _stats.Text = "Bağlantı yok";
        UpdateConnectionHeader(false);

        // Auto-discovery owns the host field. Never keep a stale IP after a disconnect.
        if (_settings.AutoDiscover)
        {
            _host.Text = string.Empty;
            _selectedServerScreen = string.Empty;
            _confirmedServerScreen = string.Empty;
            _expectedScreenToken = 0;
            _pendingScreenToken = 0;
            Interlocked.Increment(ref _screenSelectionGeneration);
            _ = RefreshDiscoveryAfterDisconnectAsync();
        }

        // The connection quick-access panel is hidden while streaming; always restore it
        // when returning to the connection page so Recent/Favorites do not disappear.
        _quickPanel.Visible = true;
        RefreshQuickAccessPanels();

        if (IsHandleCreated && !IsDisposed) SelectTab(0);
    }

    private async Task RefreshDiscoveryAfterDisconnectAsync()
    {
        if (!_settings.AutoDiscover || IsDisposed) return;
        try
        {
            await Task.Delay(60);
            if (_client is not null || IsDisposed) return;
            await DiscoverServerAsync();
        }
        catch { }
    }

    private void PositionConnectionCard()
    {
        // ConnectionPage.Designer.cs owns the connection layout.
    }

    private void HandleViewerMouseMove(Point location)
    {
        if (!_isFullscreen)
            return;

        Point p = PointToClient(Cursor.Position);

        if (p.Y <= 72)
        {
            _overlayLastMoveTick = Environment.TickCount64;
            LayoutFullscreenToolbar();
            _fullscreenDisconnectButton.Visible = true;
            _fullscreenExitButton.Visible = true;
        }
        else if (_fullscreenDisconnectButton.Visible)
        {
            _overlayLastMoveTick = Environment.TickCount64;
        }
    }

    private void LayoutFullscreenToolbar()
    {
        if (!_isFullscreen) return;
        int gap = 8;
        int w1 = 96;
        int w2 = 104;
        int total = w1 + gap + w2;
        int left = Math.Max(8, (_viewPanel.ClientSize.Width - total) / 2);
        _fullscreenDisconnectButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        _fullscreenExitButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        _fullscreenDisconnectButton.Location = new Point(left, 18);
        _fullscreenDisconnectButton.Size = new Size(w1, 30);
        _fullscreenExitButton.Location = new Point(left + w1 + gap, 18);
        _fullscreenExitButton.Size = new Size(w2, 30);
    }

    private void EnterFullscreen()
    {
        if (_isFullscreen || _client is null) return;

        // ClientForm is intentionally borderless even in normal mode, so
        // FormBorderStyle cannot be used to distinguish normal/fullscreen. Save
        // the actual normal Bounds and WindowState instead.
        if (WindowState == FormWindowState.Normal && Bounds.Width > 0 && Bounds.Height > 0)
        {
            _oldBounds = Bounds;
            _normalBoundsCaptured = true;
        }
        _oldState = WindowState;
        _oldBorder = FormBorderStyle;
        _oldFormPadding = Padding;
        _oldContentPadding = _contentPanel.Padding;

        var targetScreen = Screen.FromControl(this);
        WindowState = FormWindowState.Normal;
        FormBorderStyle = FormBorderStyle.None;
        // Fullscreen viewer must use the complete monitor surface; the normal shell padding
        // is intentionally removed only for fullscreen and restored on exit.
        Padding = Padding.Empty;
        _contentPanel.Padding = Padding.Empty;
        StartPosition = FormStartPosition.Manual;
        Bounds = targetScreen.Bounds;
        _isFullscreen = true;

        _headerPanel.Visible = false;
        _mainHeader.Visible = false;
        _footerBar.Visible = false;
        _connectionPage.Visible = false;
        _settingsPage.Visible = false;
        _aboutPage.Visible = false;
        _displayPage.Visible = true;
        _viewPanel.Visible = true;
        _statusPanel.Visible = false;
        _displayPanel.Visible = false;
        _fullscreenDisconnectButton.Visible = true;
        _fullscreenExitButton.Visible = true;

        _viewPanel.PerformLayout();
        _displayPage.PerformLayout();
        _contentPanel.PerformLayout();
        _view.BringToFront();
        _fullscreenDisconnectButton.BringToFront();
        _fullscreenExitButton.BringToFront();
        LayoutFullscreenToolbar();

        _overlayLastMoveTick = Environment.TickCount64;
        _view.Invalidate();
        _view.Focus();
    }

    private void ExitFullscreen()
    {
        if (!_isFullscreen) return;

        _isFullscreen = false;
        _fullscreenDisconnectButton.Visible = false;
        _fullscreenExitButton.Visible = false;

        SuspendLayout();
        try
        {
            WindowState = FormWindowState.Normal;
            Padding = _oldFormPadding;
            _contentPanel.Padding = _oldContentPadding;
            FormBorderStyle = _oldBorder;
            StartPosition = FormStartPosition.Manual;

            // Borderless normal mode still needs an explicit Bounds restore.
            // Restore the exact pre-fullscreen rectangle before selecting the
            // page so page layout cannot overwrite it.
            if (_normalBoundsCaptured && _oldBounds.Width > 0 && _oldBounds.Height > 0)
                Bounds = _oldBounds;
            else
                Bounds = new Rectangle(Location, new Size(800, 560));
            _oldBounds = Bounds;
            _normalBoundsCaptured = true;

            _headerPanel.Visible = true;
            _mainHeader.Visible = true;
            _footerBar.Visible = true;
            _contentPanel.Visible = true;

            SelectTab(_client is null ? 0 : 1);

            if (_oldState == FormWindowState.Maximized)
                WindowState = FormWindowState.Maximized;
        }
        finally
        {
            ResumeLayout(true);
        }

        UpdateFullscreenChromeVisibility();
        PerformLayout();
        _contentPanel.PerformLayout();
        _displayPage.PerformLayout();
        _viewPanel.PerformLayout();
        Invalidate(true);
        Update();
    }

    private void ToggleFullscreen()
    {
        if (_isFullscreen) ExitFullscreen();
        else EnterFullscreen();
    }

}
