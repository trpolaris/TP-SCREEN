using System.Collections.Concurrent;
using System.IO;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QRCoder;

namespace PolarisDisplay.Server;

internal sealed class BrowserViewerServer : IDisposable
{
    private sealed class BrowserConnection
    {
        public required string ClientId { get; init; }
        public required string Endpoint { get; init; }
        public required long Generation { get; init; }
        public string ScreenName = string.Empty;
    }

    // The browser tab owns a stable id in sessionStorage. TCP endpoints are not
    // stable across reconnects and therefore are never used as the browser key.
    private readonly ConcurrentDictionary<string, BrowserConnection> _browserConnections =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _browserGenerations = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _browserLifetimes = new(StringComparer.OrdinalIgnoreCase);
    private const int DefaultPort = 8765;
    private readonly Func<IReadOnlyList<BrowserScreenInfo>> _screenProvider;
    private readonly Func<string, Screen?> _screenResolver;
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private readonly Action<string, string, string>? _onClientConnected;
    private readonly Action<string>? _onClientDisconnected;
    private readonly Func<bool>? _isBroadcastEnabled;
    private readonly Action<string, double, double, int, int, int>? _onTelemetry;
    private readonly Func<string, BrowserFrameSubscription?>? _frameSubscriptionFactory;
    private volatile bool _broadcastEnabled = true;
    private CancellationTokenSource _broadcastStop = new();

    public int Port { get; }

    public BrowserViewerServer(
        Func<IReadOnlyList<BrowserScreenInfo>> screenProvider,
        Func<string, Screen?> screenResolver,
        int port = DefaultPort,
        Action<string, string, string>? onClientConnected = null,
        Action<string>? onClientDisconnected = null,
        Action<string, string>? onClientCommand = null,
        Func<bool>? isBroadcastEnabled = null,
        Action<string, double, double, int, int, int>? onTelemetry = null,
        Func<string, BrowserFrameSubscription?>? frameSubscriptionFactory = null)
    {
        _screenProvider = screenProvider;
        _screenResolver = screenResolver;
        Port = port;
        _onClientConnected = onClientConnected;
        _onClientDisconnected = onClientDisconnected;
        _isBroadcastEnabled=isBroadcastEnabled;
        _onTelemetry = onTelemetry;
        _frameSubscriptionFactory = frameSubscriptionFactory;
        _broadcastEnabled=isBroadcastEnabled?.Invoke() ?? true;
    }

    public void Start()
    {
        if (_listener is not null)
            return;

        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, Port);
        _listener.Start();
        _ = AcceptLoopAsync(_cts.Token);
    }

    public void SetBroadcastEnabled(bool enabled)
    {
        _broadcastEnabled = enabled;
        if (!enabled)
        {
            try { _broadcastStop.Cancel(); } catch { }
        }
        else
        {
            if (_broadcastStop.IsCancellationRequested)
            {
                _broadcastStop.Dispose();
                _broadcastStop = new CancellationTokenSource();
            }
        }
    }

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { }
        try { _listener?.Stop(); } catch { }
        _listener = null;
        foreach (var lifetime in _browserLifetimes.Values)
        {
            try { lifetime.Cancel(); lifetime.Dispose(); } catch { }
        }
        _browserLifetimes.Clear();
        _browserConnections.Clear();
        _cts?.Dispose();
        _cts = null;
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = await _listener!.AcceptTcpClientAsync(token);
                client.NoDelay = true;
                _ = Task.Run(() => HandleClientAsync(client, token), token);
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        catch { }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                string request = await ReadHttpHeadersAsync(stream, token);
                if (string.IsNullOrWhiteSpace(request)) return;

                string firstLine = request.Split("\r\n", 2, StringSplitOptions.None)[0];
                string[] parts = firstLine.Split(' ', 3);
                string method = parts.Length > 0 ? parts[0] : "GET";
                string target = parts.Length > 1 ? parts[1] : "/";
                var headers = ParseHeaders(request);

                if (method != "GET")
                {
                    await WriteResponseAsync(stream, 405, "Method Not Allowed", "text/plain; charset=utf-8", Encoding.UTF8.GetBytes("GET only"), token);
                    return;
                }

                if (headers.TryGetValue("upgrade", out string? upgrade) &&
                    upgrade.Contains("websocket", StringComparison.OrdinalIgnoreCase))
                {
                    string endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Bilinmeyen";
                    await HandleWebSocketAsync(stream, target, headers, endpoint, token);
                    return;
                }

                await HandleHttpAsync(stream, target, token);
            }
        }
        catch { }
    }

    private async Task HandleHttpAsync(NetworkStream stream, string target, CancellationToken token)
    {
        string path = target;
        int q = target.IndexOf('?');
        string query = q >= 0 ? target[(q + 1)..] : string.Empty;
        if (q >= 0) path = target[..q];

        switch (path)
        {
            case "/qr":
            {
                string host = "127.0.0.1";
                try
                {
                    if (_listener?.LocalEndpoint is IPEndPoint ep && ep.AddressFamily == AddressFamily.InterNetwork && !IPAddress.Any.Equals(ep.Address))
                        host = ep.Address.ToString();
                    else
                    {
                        var local = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a));
                        if (local is not null) host = local.ToString();
                    }
                } catch { }
                using var generator = new QRCodeGenerator();
                using var data = generator.CreateQrCode($"http://{host}:{Port}/", QRCodeGenerator.ECCLevel.M);
                var png = new PngByteQRCode(data).GetGraphic(8);
                await WriteResponseAsync(stream, 200, "OK", "image/png", png, token);
                return;
            }
            case "/":
            case "/index.html":
                await WriteResponseAsync(stream, 200, "OK", "text/html; charset=utf-8", Encoding.UTF8.GetBytes(BrowserHtml), token);
                return;
            case "/api/screens":
                var json = JsonSerializer.Serialize(_screenProvider());
                await WriteResponseAsync(stream, 200, "OK", "application/json; charset=utf-8", Encoding.UTF8.GetBytes(json), token);
                return;
            case "/api/auth-state":
                var authJson = JsonSerializer.Serialize(new { requiresPin = ServerAuth.RequiresPin });
                await WriteResponseAsync(stream, 200, "OK", "application/json; charset=utf-8", Encoding.UTF8.GetBytes(authJson), token);
                return;
            case "/favicon.ico":
                await WriteResponseAsync(stream, 204, "No Content", "text/plain", Array.Empty<byte>(), token);
                return;
            default:
                await WriteResponseAsync(stream, 404, "Not Found", "text/plain; charset=utf-8", Encoding.UTF8.GetBytes("Not found"), token);
                return;
        }
    }

    private async Task HandleWebSocketAsync(NetworkStream stream, string target, Dictionary<string, string> headers, string endpoint, CancellationToken token)
    {
        if(!_broadcastEnabled || (_isBroadcastEnabled is not null && !_isBroadcastEnabled())){await WriteResponseAsync(stream,503,"Service Unavailable","text/plain; charset=utf-8",Encoding.UTF8.GetBytes("PolarisDisplay yayını şu anda durduruldu."),token);return;}
        if (!headers.TryGetValue("sec-websocket-key", out string? key) || string.IsNullOrWhiteSpace(key))
        {
            await WriteResponseAsync(stream, 400, "Bad Request", "text/plain; charset=utf-8", Encoding.UTF8.GetBytes("Missing Sec-WebSocket-Key"), token);
            return;
        }

        string accept = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(key.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        string handshake =
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            $"Sec-WebSocket-Accept: {accept}\r\n\r\n";
        byte[] handshakeBytes = Encoding.ASCII.GetBytes(handshake);
        await stream.WriteAsync(handshakeBytes, token);

        string screenName = "";
        string clientId = string.Empty;
        int qi = target.IndexOf('?');
        if (qi >= 0)
        {
            foreach (string pair in target[(qi + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = pair.IndexOf('=');
                if (eq <= 0) continue;
                string keyName = Uri.UnescapeDataString(pair[..eq]);
                string value = Uri.UnescapeDataString(pair[(eq + 1)..]);
                if (string.Equals(keyName, "screen", StringComparison.OrdinalIgnoreCase))
                    screenName = value;
                else if (string.Equals(keyName, "cid", StringComparison.OrdinalIgnoreCase))
                    clientId = value;
            }
        }

        if (string.IsNullOrWhiteSpace(clientId))
            clientId = "legacy-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(endpoint))).ToLowerInvariant();
        clientId = clientId.Trim();
        if (clientId.Length > 128) clientId = clientId[..128];

        Screen? screen = _screenResolver(screenName);
        if (screen is null)
        {
            await SendTextFrameAsync(stream, "{\"type\":\"error\",\"message\":\"Ekran bulunamadı\"}", token);
            await SendCloseFrameAsync(stream, token);
            return;
        }

        string? authCommand = await ReadWebSocketTextFrameAsync(stream, token);
        if (authCommand is null || !authCommand.StartsWith("POLARIS_AUTH|", StringComparison.OrdinalIgnoreCase))
        {
            await SendTextFrameAsync(stream, JsonSerializer.Serialize(new { type = "authRequired", message = "PIN gerekli" }), token);
            await SendCloseFrameAsync(stream, token);
            return;
        }

        string suppliedPin = authCommand["POLARIS_AUTH|".Length..].Trim();
        if (ServerAuth.RequiresPin && suppliedPin.Length == 0)
        {
            await SendTextFrameAsync(stream, JsonSerializer.Serialize(new { type = "authRequired", message = "PIN gerekli" }), token);
            authCommand = await ReadWebSocketTextFrameAsync(stream, token);
            suppliedPin = authCommand is not null && authCommand.StartsWith("POLARIS_AUTH|", StringComparison.OrdinalIgnoreCase)
                ? authCommand["POLARIS_AUTH|".Length..].Trim() : string.Empty;
        }

        bool authOk = ServerAuth.ValidatePin(suppliedPin);
        await SendTextFrameAsync(stream, JsonSerializer.Serialize(new { type = "authResult", success = authOk, message = authOk ? "PIN doğrulandı" : "PIN doğrulaması başarısız" }), token);
        if (!authOk)
        {
            await SendCloseFrameAsync(stream, token);
            return;
        }

        long generation = _browserGenerations.AddOrUpdate(clientId, 1, static (_, current) => current + 1);
        var connectionCts = CancellationTokenSource.CreateLinkedTokenSource(token, _broadcastStop.Token);
        if (_browserLifetimes.TryGetValue(clientId, out var previousLifetime))
        {
            try { previousLifetime.Cancel(); } catch { }
        }
        _browserLifetimes[clientId] = connectionCts;

        var connection = new BrowserConnection
        {
            ClientId = clientId,
            Endpoint = endpoint,
            Generation = generation,
            ScreenName = screen.DeviceName
        };
        _browserConnections[clientId] = connection;

        _onClientConnected?.Invoke(clientId, endpoint, screen.DeviceName);

        try
        {
            await SendScreenHelloAsync(stream, screen, connectionCts.Token);
            await SendScreenFramesAsync(stream, connection, connectionCts.Token);
        }
        finally
        {
            if (_browserConnections.TryGetValue(clientId, out var currentConnection) && currentConnection.Generation == generation)
            {
                _browserConnections.TryRemove(clientId, out _);
                if (_browserLifetimes.TryRemove(clientId, out var lifetime))
                {
                    try { lifetime.Dispose(); } catch { }
                }
                _onClientDisconnected?.Invoke(clientId);
            }
            else
            {
                try { connectionCts.Dispose(); } catch { }
            }
        }
    }

    public void SetScreen(string clientId, string deviceName)
    {
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(deviceName))
            return;

        if (_browserConnections.TryGetValue(clientId, out var connection))
            connection.ScreenName = deviceName;
    }

    private static async Task SendScreenHelloAsync(
        NetworkStream stream,
        Screen screen,
        CancellationToken token)
    {
        await SendTextFrameAsync(stream, JsonSerializer.Serialize(new
        {
            type = "hello",
            screen = new
            {
                deviceName = screen.DeviceName,
                width = screen.Bounds.Width,
                height = screen.Bounds.Height
            }
        }), token);
    }

    private async Task SendScreenFramesAsync(NetworkStream stream, BrowserConnection connection, CancellationToken token)
    {
        BrowserFrameSubscription? subscription = null;
        string activeScreen = string.Empty;
        long telemetryStart = Stopwatch.GetTimestamp();
        long telemetryFrames = 0;
        long telemetryBytes = 0;
        // WEB is an independent encoded stream. The subscription already provides
        // the browser-specific FPS/bitrate budget, so do not throttle the socket
        // here. A slow browser only stalls its own connection; native Client
        // producers and their queues are completely independent.
        long lastSentFrameId = -1;

        try
        {
            while (!token.IsCancellationRequested && _broadcastEnabled && (_isBroadcastEnabled?.Invoke() ?? true))
            {
                string desiredName = connection.ScreenName;
                Screen? current = _screenResolver(desiredName);
                if (current is null)
                {
                    await Task.Delay(30, token);
                    continue;
                }

                if (!string.Equals(current.DeviceName, activeScreen, StringComparison.OrdinalIgnoreCase))
                {
                    subscription?.Dispose();
                    subscription = _frameSubscriptionFactory?.Invoke(current.DeviceName);
                    activeScreen = current.DeviceName;
                    await SendScreenHelloAsync(stream, current, token);
                }

                if (subscription is null)
                {
                    await Task.Delay(100, token);
                    continue;
                }

                BrowserFrameData? frame = await subscription.WaitNextAsync(token).ConfigureAwait(false);
                if (frame is null) continue;

                try
                {
                    if (frame.FrameId == lastSentFrameId)
                    {
                        frame.Dispose();
                        await Task.Delay(2, token).ConfigureAwait(false);
                        continue;
                    }
                    lastSentFrameId = frame.FrameId;
                    await SendBinaryFrameAsync(stream, frame.Width, frame.Height, frame.Payload, token);
                    try
                    {
                        Point cursorPos = System.Windows.Forms.Cursor.Position;
                        Rectangle bounds = current.Bounds;
                        bool cursorVisible = bounds.Contains(cursorPos);
                        double cx = cursorVisible && bounds.Width > 0 ? Math.Clamp((cursorPos.X - bounds.Left) / (double)bounds.Width, 0d, 1d) : 0d;
                        double cy = cursorVisible && bounds.Height > 0 ? Math.Clamp((cursorPos.Y - bounds.Top) / (double)bounds.Height, 0d, 1d) : 0d;
                        await SendTextFrameAsync(stream, JsonSerializer.Serialize(new { type = "cursor", x = cx, y = cy, visible = cursorVisible }), token);
                    }
                    catch { }

                    telemetryFrames++;
                    telemetryBytes += 8 + frame.Payload.Length;
                    if (Stopwatch.GetElapsedTime(telemetryStart).TotalSeconds >= 1.0)
                    {
                        double elapsed = Math.Max(0.001, Stopwatch.GetElapsedTime(telemetryStart).TotalSeconds);
                        _onTelemetry?.Invoke(connection.ClientId, telemetryFrames / elapsed, telemetryBytes * 8d / elapsed / 1_000_000d, frame.Quality, frame.Width, frame.Height);
                        telemetryStart = Stopwatch.GetTimestamp();
                        telemetryFrames = 0;
                        telemetryBytes = 0;
                    }
                }
                finally
                {
                    frame.Dispose();
                }
            }
        }
        catch (OperationCanceledException) { }
        catch { }
        finally
        {
            subscription?.Dispose();
        }
    }



    private static Dictionary<string, string> ParseHeaders(string request)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string[] lines = request.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines.Skip(1))
        {
            int i = line.IndexOf(':');
            if (i <= 0) continue;
            result[line[..i].Trim()] = line[(i + 1)..].Trim();
        }
        return result;
    }

    private static async Task<string> ReadHttpHeadersAsync(NetworkStream stream, CancellationToken token)
    {
        using var ms = new MemoryStream();
        byte[] one = new byte[1];
        while (ms.Length < 32 * 1024)
        {
            int n = await stream.ReadAsync(one, token);
            if (n == 0) return string.Empty;
            ms.WriteByte(one[0]);
            if (ms.Length >= 4)
            {
                byte[] a = ms.GetBuffer();
                int l = (int)ms.Length;
                if (a[l - 4] == '\r' && a[l - 3] == '\n' && a[l - 2] == '\r' && a[l - 1] == '\n')
                    return Encoding.ASCII.GetString(a, 0, l);
            }
        }
        return string.Empty;
    }

    private static async Task WriteResponseAsync(NetworkStream stream, int code, string text, string contentType, byte[] body, CancellationToken token)
    {
        string header =
            $"HTTP/1.1 {code} {text}\r\n" +
            $"Content-Type: {contentType}\r\n" +
            $"Content-Length: {body.Length}\r\n" +
            "Cache-Control: no-store\r\n" +
            "Connection: close\r\n\r\n";
        byte[] bytes = Encoding.ASCII.GetBytes(header);
        await stream.WriteAsync(bytes, token);
        if (body.Length > 0) await stream.WriteAsync(body, token);
    }

    private static async Task<string?> ReadWebSocketTextFrameAsync(NetworkStream stream, CancellationToken token)
    {
        byte[] h = new byte[2];
        if (!await ReadExactAsync(stream, h, token)) return null;
        byte opcode = (byte)(h[0] & 0x0F);
        bool masked = (h[1] & 0x80) != 0;
        ulong length = (ulong)(h[1] & 0x7F);
        if (length == 126)
        {
            byte[] b = new byte[2]; if (!await ReadExactAsync(stream,b,token)) return null;
            length = BinaryPrimitives.ReadUInt16BigEndian(b);
        }
        else if (length == 127)
        {
            byte[] b = new byte[8]; if (!await ReadExactAsync(stream,b,token)) return null;
            length = BinaryPrimitives.ReadUInt64BigEndian(b);
        }
        if (length > 64 * 1024 || !masked) return null;
        byte[] mask = new byte[4]; if (!await ReadExactAsync(stream,mask,token)) return null;
        byte[] payload = new byte[(int)length]; if (length > 0 && !await ReadExactAsync(stream,payload,token)) return null;
        for (int i=0;i<payload.Length;i++) payload[i] = (byte)(payload[i] ^ mask[i&3]);
        if (opcode == 0x8) return null;
        return opcode == 0x1 ? Encoding.UTF8.GetString(payload) : null;
    }

    private static async Task SendTextFrameAsync(NetworkStream stream, string text, CancellationToken token) =>
        await SendFrameAsync(stream, 0x1, Encoding.UTF8.GetBytes(text), token);

    private static async Task SendBinaryFrameAsync(NetworkStream stream, int width, int height, byte[] payload, CancellationToken token)
    {
        int payloadLength = checked(8 + payload.Length);
        byte[] header = CreateWebSocketHeader(0x2, payloadLength);
        await stream.WriteAsync(header, token);
        byte[] dimensions = new byte[8];
        BinaryPrimitives.WriteInt32BigEndian(dimensions.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(dimensions.AsSpan(4, 4), height);
        await stream.WriteAsync(dimensions, token);
        await stream.WriteAsync(payload, token);
    }

    private static async Task SendCloseFrameAsync(NetworkStream stream, CancellationToken token) =>
        await SendFrameAsync(stream, 0x8, Array.Empty<byte>(), token);

    private static byte[] CreateWebSocketHeader(byte opcode, int payloadLength)
    {
        using var header = new MemoryStream(14);
        header.WriteByte((byte)(0x80 | opcode));
        if (payloadLength < 126)
        {
            header.WriteByte((byte)payloadLength);
        }
        else if (payloadLength <= ushort.MaxValue)
        {
            header.WriteByte(126);
            byte[] e = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(e, (ushort)payloadLength);
            header.Write(e, 0, e.Length);
        }
        else
        {
            header.WriteByte(127);
            byte[] e = new byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(e, (ulong)payloadLength);
            header.Write(e, 0, e.Length);
        }
        return header.ToArray();
    }

    private static async Task SendFrameAsync(NetworkStream stream, byte opcode, byte[] payload, CancellationToken token)
    {
        var header = new MemoryStream();
        header.WriteByte((byte)(0x80 | opcode));
        if (payload.Length < 126)
        {
            header.WriteByte((byte)payload.Length);
        }
        else if (payload.Length <= ushort.MaxValue)
        {
            header.WriteByte(126);
            byte[] e = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(e, (ushort)payload.Length);
            header.Write(e, 0, e.Length);
        }
        else
        {
            header.WriteByte(127);
            byte[] e = new byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(e, (ulong)payload.Length);
            header.Write(e, 0, e.Length);
        }

        await stream.WriteAsync(header.GetBuffer().AsMemory(0, (int)header.Length), token);
        if (payload.Length > 0) await stream.WriteAsync(payload, token);
    }

    private static async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken token)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int n = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), token);
            if (n <= 0) return false;
            offset += n;
        }
        return true;
    }

    public sealed record BrowserScreenInfo(string deviceName, string name, int width, int height);

    private const string BrowserHtml = """
<!doctype html>
<html lang="tr">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1,maximum-scale=1,user-scalable=no">
<title>PolarisDisplay Browser</title>
<style>
*{box-sizing:border-box}html,body{margin:0;width:100%;height:100%;background:#000;color:#d8f4ff;font-family:Segoe UI,Arial,sans-serif;overflow:hidden;-webkit-tap-highlight-color:transparent}#stage{position:fixed;inset:0;background:#000;display:flex;align-items:center;justify-content:center}#view{width:100%;height:100%;max-width:none;max-height:none;display:none;user-select:none;-webkit-user-drag:none;object-fit:fill;object-position:center center}#bar{position:fixed;z-index:20;top:0;left:0;right:0;height:54px;display:flex;align-items:center;gap:8px;padding:0 12px;background:rgba(5,16,29,.96);border-bottom:1px solid #114a73;transform:translateY(-100%);transition:transform .16s ease}#bar.show{transform:translateY(0)}#brand{font-weight:800;letter-spacing:.3px;margin-right:auto}.tag{font-size:12px;color:#8eb4cd;white-space:nowrap}select,button{height:34px;background:#081b2c;color:#d8f4ff;border:1px solid #1d5f8d;border-radius:7px;padding:0 10px;font-size:13px}button{cursor:pointer}#disconnect{display:none}#status{min-width:84px;text-align:right;font-size:12px;color:#7fe6ff;white-space:nowrap}#remoteCursor{position:fixed;z-index:15;width:0;height:0;pointer-events:none;display:none;filter:drop-shadow(0 1px 1px rgba(0,0,0,.85))}#remoteCursor:before{content:"";position:absolute;left:0;top:0;width:0;height:0;border-left:11px solid #fff;border-top:18px solid transparent;border-bottom:4px solid transparent;transform:rotate(-12deg);transform-origin:0 0}.qrbox{position:fixed;z-index:50;right:18px;top:68px;width:240px;background:rgba(5,16,29,.97);border:1px solid #1d5f8d;border-radius:12px;padding:14px;text-align:center;display:none;box-shadow:0 14px 40px rgba(0,0,0,.45)}.qrbox.show{display:block}.qrbox img{width:190px;height:190px;background:#fff}.qrtitle{font-weight:800;margin-bottom:10px;color:#d8f4ff}.qrbox button{margin-top:10px;width:100%}#pinbox{position:fixed;z-index:100;left:50%;top:50%;transform:translate(-50%,-50%);width:340px;background:rgba(5,16,29,.98);border:1px solid #1d5f8d;border-radius:12px;padding:18px;box-shadow:0 20px 60px rgba(0,0,0,.6);display:none}.pintitle{font-weight:800;color:#d8f4ff;margin-bottom:8px}.pintext{font-size:13px;color:#9dc0d6;margin-bottom:12px}#pin{width:100%;height:38px;background:#081b2c;color:#fff;border:1px solid #1d5f8d;border-radius:7px;padding:0 10px}.pinbuttons{display:flex;justify-content:flex-end;gap:8px;margin-top:12px}@media(max-width:700px){#bar{height:auto;min-height:54px;flex-wrap:wrap;padding:7px}.tag{display:none}#brand{width:100%;margin-right:0}select,button{height:32px}#status{position:absolute;right:8px;top:8px}}
</style>
</head>
<body>
<div id="pinbox"><div class="pintitle">POLARISDISPLAY • PIN</div><div class="pintext">Sunucu PIN kodunu girin</div><input id="pin" type="password" autocomplete="off" maxlength="64" placeholder="PIN"><div class="pinbuttons"><button id="pincancel">İPTAL</button><button id="pinok">BAĞLAN</button></div></div>
<div id="qrbox" class="qrbox"><div class="qrtitle">WEB VIEWER QR</div><img src="/qr" alt="QR"><button id="qrclose">KAPAT</button></div>
<div id="bar"><div id="brand">POLARISDISPLAY • BROWSER</div><span class="tag">Ekran</span><select id="screen"></select><button id="connect">BAĞLAN</button><button id="disconnect">BAĞLANTIYI KES</button><button id="full">TAM EKRAN</button><button id="qrbtn">QR</button><span id="status">Hazır</span></div>
<div id="stage"><img id="view" alt="PolarisDisplay"><div id="remoteCursor"></div></div>
<script>
(function(){
var screenEl=document.getElementById('screen'),view=document.getElementById('view'),statusEl=document.getElementById('status'),bar=document.getElementById('bar'),remoteCursor=document.getElementById('remoteCursor'),connectBtn=document.getElementById('connect'),disconnectBtn=document.getElementById('disconnect');
var ws=null,blobUrl=null,selected='',hideTimer=null,reconnectTimer=null,reconnectDelay=1000,connectGeneration=0,manualDisconnected=false,requiresPin=false,browserPin='',pinCallback=null;
var urlApi=window.URL||window.webkitURL;
function makeId(){return 'pd-'+new Date().getTime().toString(36)+'-'+Math.floor(Math.random()*2147483647).toString(36)}
var clientId='';try{clientId=sessionStorage.getItem('polarisDisplayClientId')||'';if(!clientId){clientId=makeId();sessionStorage.setItem('polarisDisplayClientId',clientId)}}catch(e){clientId=makeId()}
function showMenu(){bar.className='show';if(hideTimer)clearTimeout(hideTimer);hideTimer=setTimeout(function(){bar.className=''},1800)}
function showMenuTouch(){showMenu()}
document.addEventListener('mousemove',function(e){if(e.clientY<70)showMenu()});document.addEventListener('touchstart',showMenuTouch,false);bar.addEventListener('mouseenter',showMenu,false);bar.addEventListener('touchstart',showMenuTouch,false);
function xhr(url,callback){var x=new XMLHttpRequest();x.open('GET',url,true);x.onreadystatechange=function(){if(x.readyState!==4)return;if(x.status>=200&&x.status<300){try{callback(null,JSON.parse(x.responseText))}catch(e){callback(e,null)}}else callback(new Error('HTTP '+x.status),null)};x.onerror=function(){callback(new Error('network'),null)};try{x.send(null)}catch(e){callback(e,null)}}
function loadScreens(){xhr('/api/screens?ts='+new Date().getTime(),function(err,list){if(err){if(!ws||ws.readyState!==1)statusEl.innerHTML='Ekran listesi yok';return}var keep=selected||screenEl.value;while(screenEl.firstChild)screenEl.removeChild(screenEl.firstChild);var i,o,s;for(i=0;i<list.length;i++){s=list[i];o=document.createElement('option');o.value=s.deviceName;o.text=s.name+' • '+s.width+'×'+s.height;screenEl.appendChild(o)}for(i=0;i<screenEl.options.length;i++){if(screenEl.options[i].value===keep){screenEl.selectedIndex=i;break}}if(screenEl.options.length&&screenEl.selectedIndex<0)screenEl.selectedIndex=0;selected=screenEl.value;if(!ws||ws.readyState!==1)statusEl.innerHTML=list.length?'Hazır':'Ekran yok'})}
function loadAuthState(callback){xhr('/api/auth-state?ts='+new Date().getTime(),function(err,m){requiresPin=!err&&m&&!!m.requiresPin;if(callback)callback()})}
function askPin(callback){pinCallback=callback;var box=document.getElementById('pinbox'),input=document.getElementById('pin');box.style.display='block';input.value='';setTimeout(function(){try{input.focus()}catch(e){}},30)}
function finishPin(ok){var input=document.getElementById('pin'),box=document.getElementById('pinbox'),cb=pinCallback;pinCallback=null;box.style.display='none';if(cb)cb(ok?(input.value||'').replace(/^\s+|\s+$/g,''):null)}
document.getElementById('pinok').onclick=function(){finishPin(true)};document.getElementById('pincancel').onclick=function(){finishPin(false)};document.getElementById('pin').onkeydown=function(e){e=e||window.event;var k=e.keyCode||e.which;if(k===13)finishPin(true);else if(k===27)finishPin(false)};
function setConnectedUi(connected){connectBtn.style.display=connected?'none':'inline-block';disconnectBtn.style.display=connected?'inline-block':'none'}
function clearReconnect(){if(reconnectTimer){clearTimeout(reconnectTimer);reconnectTimer=null}}
function scheduleReconnect(){if(manualDisconnected||reconnectTimer||document.hidden)return;var wait=reconnectDelay;reconnectDelay=Math.min(10000,Math.max(1000,reconnectDelay*2));reconnectTimer=setTimeout(function(){reconnectTimer=null;connect(false)},wait);statusEl.innerHTML='Yeniden bağlanıyor…'}
function disconnect(){manualDisconnected=true;connectGeneration++;clearReconnect();if(ws){try{ws.onclose=null;ws.close(1000,'user-disconnect')}catch(e){}ws=null}remoteCursor.style.display='none';setConnectedUi(false);statusEl.innerHTML='Bağlantı kesildi'}
function connect(manual){if(manual===undefined)manual=true;if(manual){manualDisconnected=false;reconnectDelay=1000;connectGeneration++}var generation=connectGeneration;clearReconnect();if(ws){try{ws.onclose=null;ws.close(1000,'reconnect')}catch(e){}ws=null}selected=screenEl.value||selected;loadAuthState(function(){if(generation!==connectGeneration)return;if(requiresPin&&!browserPin){askPin(function(pin){if(generation!==connectGeneration)return;if(pin===null){statusEl.innerHTML='PIN iptal edildi';manualDisconnected=true;return}browserPin=pin;openSocket(generation)})}else openSocket(generation)})}
function openSocket(generation){var proto=location.protocol==='https:'?'wss':'ws',socket;statusEl.innerHTML='Bağlanıyor…';try{socket=new WebSocket(proto+'://'+location.host+'/ws?screen='+encodeURIComponent(selected)+'&cid='+encodeURIComponent(clientId))}catch(e){statusEl.innerHTML='WebSocket desteklenmiyor';return}ws=socket;socket.binaryType='arraybuffer';var authOk=false;socket.onopen=function(){if(generation!==connectGeneration)return;statusEl.innerHTML='Doğrulanıyor…';try{socket.send('POLARIS_AUTH|'+(requiresPin?browserPin:''))}catch(e){}};
socket.onmessage=function(e){if(generation!==connectGeneration)return;if(typeof e.data==='string'){var msg;try{msg=JSON.parse(e.data)}catch(err){return}if(msg.type==='authRequired'){askPin(function(pin){if(generation!==connectGeneration||pin===null){try{socket.close()}catch(e){}return}browserPin=pin;try{socket.send('POLARIS_AUTH|'+pin)}catch(e){}})}else if(msg.type==='authResult'){if(!msg.success){browserPin='';statusEl.innerHTML=msg.message||'PIN doğrulaması başarısız';manualDisconnected=true;try{socket.close()}catch(e){}return}authOk=true;setConnectedUi(true);statusEl.innerHTML='Bağlandı';try{socket.send('POLARIS_HELLO|WEB')}catch(e){}}else if(msg.type==='hello'){if(msg.screen&&msg.screen.deviceName){selected=msg.screen.deviceName;var i;for(i=0;i<screenEl.options.length;i++){if(screenEl.options[i].value===selected){screenEl.selectedIndex=i;break}}}statusEl.innerHTML='Bağlı'}else if(msg.type==='cursor'){if(msg.visible&&isFinite(msg.x)&&isFinite(msg.y)){remoteCursor.style.display='block';remoteCursor.style.left=(Math.max(0,Math.min(1,msg.x))*window.innerWidth)+'px';remoteCursor.style.top=(Math.max(0,Math.min(1,msg.y))*window.innerHeight)+'px'}else remoteCursor.style.display='none'}else if(msg.type==='error'){statusEl.innerHTML=msg.message||'Hata'}}else{if(!authOk)return;handleBinary(e.data)}};
socket.onclose=function(){if(generation!==connectGeneration)return;ws=null;remoteCursor.style.display='none';setConnectedUi(false);if(!manualDisconnected){statusEl.innerHTML='Bağlantı koptu';scheduleReconnect()}else statusEl.innerHTML='Bağlantı kesildi'};socket.onerror=function(){if(generation!==connectGeneration)return;statusEl.innerHTML='Bağlantı hatası'};
}
function handleBinary(data){if(!data)return;if(data instanceof ArrayBuffer){showJpeg(new Uint8Array(data),8);return}if(window.FileReader&&data instanceof Blob){var r=new FileReader();r.onload=function(){showJpeg(new Uint8Array(r.result),8)};r.readAsArrayBuffer(data)}}
function showJpeg(bytes,offset){if(!bytes||bytes.length<=offset)return;var blob=new Blob([bytes.subarray(offset)],{type:'image/jpeg'});if(blobUrl&&urlApi){try{urlApi.revokeObjectURL(blobUrl)}catch(e){}}if(urlApi&&urlApi.createObjectURL)blobUrl=urlApi.createObjectURL(blob);if(blobUrl){view.src=blobUrl;view.style.display='block'}}
screenEl.onchange=function(){selected=screenEl.value;connect(true)};connectBtn.onclick=function(){connect(true)};disconnectBtn.onclick=disconnect;
document.getElementById('full').onclick=function(){try{if(document.fullscreenElement){document.exitFullscreen()}else if(document.documentElement.requestFullscreen){document.documentElement.requestFullscreen()}else if(document.documentElement.webkitRequestFullscreen){document.documentElement.webkitRequestFullscreen()}}catch(e){}};
var qrbox=document.getElementById('qrbox');document.getElementById('qrbtn').onclick=function(){qrbox.className='qrbox show';showMenu()};document.getElementById('qrclose').onclick=function(){qrbox.className='qrbox'};
loadAuthState(function(){loadScreens()});setInterval(loadScreens,3000);
})();
</script>
</body>
</html>
""";
}
