using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Drawing;

namespace PolarisDisplay.Server;

internal sealed class ServerSettings
{
    public int Port { get; set; } = 50505;
    public int Fps { get; set; } = 60;
    public int Quality { get; set; } = 85;
    public int BitrateMbps { get; set; } = 50;
    public bool AutoBitrate { get; set; } = true;
    public string Pin { get; set; } = "";
    public int WebPort { get; set; } = 8765;
    public string UpdateManifestUrl { get; set; } = "";
}

internal static class ServerSettingsStore
{
    private static readonly string Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TRPolaris");
    private static readonly string SettingsPath = Path.Combine(Root, "ServerSettings.json");

    public static ServerSettings Load()
    {
        try { if (File.Exists(SettingsPath)) return JsonSerializer.Deserialize<ServerSettings>(File.ReadAllText(SettingsPath)) ?? new(); } catch { }
        return new();
    }
    public static void Save(ServerSettings value)
    {
        try { Directory.CreateDirectory(Root); File.WriteAllText(SettingsPath, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented=true })); } catch { }
    }
}

internal static class ServerSettingsDialog
{
    public static bool Show(Form owner, ServerSettings settings)
    {
        using var f = new ServerSettingsDialogForm(settings);
        return f.ShowDialog(owner) == DialogResult.OK;
    }
}

internal static class ServerDiscovery
{
    public const int DiscoveryPort=50504;
    public static UdpClient CreateListener()=>new(new IPEndPoint(IPAddress.Any,DiscoveryPort));
    public static async Task RunAsync(UdpClient udp,Func<int> portProvider,Func<int> webPortProvider,Func<bool> isServerRunning,CancellationToken token)
    {
        using(udp)
        {
            while(!token.IsCancellationRequested)
            {
                UdpReceiveResult r; try{r=await udp.ReceiveAsync(token);}catch{break;}
                string req=Encoding.UTF8.GetString(r.Buffer); if(!req.StartsWith("POLARIS_DISCOVER|",StringComparison.OrdinalIgnoreCase))continue;
                if (!isServerRunning()) continue;
                string payload=JsonSerializer.Serialize(new{type="polaris-server",name=Environment.MachineName,port=portProvider(),webPort=webPortProvider()});
                byte[] bytes=Encoding.UTF8.GetBytes(payload); try{await udp.SendAsync(bytes,bytes.Length,r.RemoteEndPoint);}catch{}
            }
        }
    }
}
