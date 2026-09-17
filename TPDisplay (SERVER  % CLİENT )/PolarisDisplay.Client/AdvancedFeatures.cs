using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace PolarisDisplay.Client;

internal sealed class ClientSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 50505;
    public string Pin { get; set; } = "";
    public string QualityMode { get; set; } = "Otomatik";
    public bool AutoReconnect { get; set; } = true;
    public bool AutoDiscover { get; set; } = true;
    public int PreferredFps { get; set; } = 60;
    public string StreamMode { get; set; } = "Düşük Gecikme";
    public string UpdateManifestUrl { get; set; } = "";
}


internal static class ClientSettingsStore
{
    private static readonly string Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TRPolaris");
    private static readonly string SettingsPath = Path.Combine(Root, "ClientSettings.json");

    public static ClientSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
                return JsonSerializer.Deserialize<ClientSettings>(File.ReadAllText(SettingsPath)) ?? new ClientSettings();
        }
        catch { }
        return new ClientSettings();
    }

    public static void Save(ClientSettings value)
    {
        try
        {
            Directory.CreateDirectory(Root);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

}

internal sealed record DiscoveredServer(string Host, int Port, int WebPort, string Name);

internal static class LanDiscovery
{
    public const int DiscoveryPort = 50504;
    private const string Request = "POLARIS_DISCOVER|1";

    public static async Task<DiscoveredServer?> FindServerAsync(CancellationToken token)
    {
        using var udp = new UdpClient(AddressFamily.InterNetwork) { EnableBroadcast = true };
        udp.Client.ReceiveTimeout = 900;
        byte[] data = Encoding.UTF8.GetBytes(Request);
        try
        {
            await udp.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, DiscoveryPort));
            var receive = udp.ReceiveAsync(token).AsTask();
            var done = await Task.WhenAny(receive, Task.Delay(850, token));
            if (done != receive) return null;
            var result = await receive;
            string text = Encoding.UTF8.GetString(result.Buffer);
            using var doc = JsonDocument.Parse(text);
            var r = doc.RootElement;
            return new DiscoveredServer(
                result.RemoteEndPoint.Address.ToString(),
                r.TryGetProperty("port", out var p) ? p.GetInt32() : 50505,
                r.TryGetProperty("webPort", out var w) ? w.GetInt32() : 8765,
                r.TryGetProperty("name", out var n) ? n.GetString() ?? "POLARIS-SERVER" : "POLARIS-SERVER");
        }
        catch { return null; }
    }

    public static UdpClient CreateListener() => new(new IPEndPoint(IPAddress.Any, DiscoveryPort));

    public static async Task RunResponderAsync(UdpClient udp, Func<int> portProvider, Func<int> webPortProvider, CancellationToken token)
    {
        using (udp)
        {
            while (!token.IsCancellationRequested)
            {
                UdpReceiveResult r;
                try { r = await udp.ReceiveAsync(token); }
                catch (OperationCanceledException) { break; }
                catch { break; }

                if (!Encoding.UTF8.GetString(r.Buffer).StartsWith("POLARIS_DISCOVER|", StringComparison.OrdinalIgnoreCase))
                    continue;

                string payload = JsonSerializer.Serialize(new
                {
                    type = "polaris-server",
                    name = Environment.MachineName,
                    port = portProvider(),
                    webPort = webPortProvider()
                });
                byte[] bytes = Encoding.UTF8.GetBytes(payload);
                try { await udp.SendAsync(bytes, bytes.Length, r.RemoteEndPoint); } catch { }
            }
        }
    }
}

internal static class ClientSettingsDialog
{
    public static bool Show(Form owner, ClientSettings settings)
    {
        using var f = new ClientSettingsDialogForm(settings);
        return f.ShowDialog(owner) == DialogResult.OK;
    }
}
