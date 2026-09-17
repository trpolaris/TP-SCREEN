namespace PolarisDisplay.Server;
public sealed partial class StatisticsPage : UserControl
{
    public StatisticsPage() { InitializeComponent(); }

    internal void SetServerStats(int clients, double mbps, TimeSpan uptime, double cpu, long mem, System.Collections.Concurrent.ConcurrentDictionary<string, ServerForm.ClientTelemetryState> telemetry)
    {
        if (IsDisposed) return;
        _v1.Text = clients.ToString();
        _v2.Text = $"{mbps:0.0} Mbps";
        _v3.Text = uptime.ToString(@"hh\:mm\:ss");
        _v4.Text = $"{cpu:0.0}%";
        _v5.Text = $"{mem} MB";

        var entries = telemetry
            .Where(x => x.Value.Fps > 0 || string.Equals(x.Value.Mode, "WEB", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Value.Fps)
            .Take(6)
            .Select(x =>
            {
                string prefix = x.Key.StartsWith("WEB • ", StringComparison.OrdinalIgnoreCase) ? "WEB" : "CLIENT";
                return $"{prefix} • {x.Value.Fps:0} FPS • {x.Value.Mbps:0.0} Mbps • Q{x.Value.Quality}";
            })
            .ToArray();

        _v6.Text = entries.Length == 0 ? "Bağlantı verisi bekleniyor" : string.Join("\n", entries);

        var pings = telemetry.Values.Where(x => x.PingMs >= 0 && x.PingMs <= 6000).Select(x => x.PingMs).ToArray();
        _v7.Text = pings.Length == 0 ? "—" : $"{pings.Average():0} ms";
        _v8.Text = telemetry.Values.Any(x => x.Width > 0 && x.Height > 0)
            ? $"{telemetry.Values.Where(x => x.Width > 0 && x.Height > 0).Max(x => x.Width)}×{telemetry.Values.Where(x => x.Width > 0 && x.Height > 0).Max(x => x.Height)}"
            : "—";
        _v9.Text = telemetry.Count == 0 ? "Bağlı değil" : $"{telemetry.Count} aktif • JPEG / düşük gecikme";
    }
}
