using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;

namespace PolarisDisplay.Client;

internal sealed class ClientTelemetry
{
    public double DecodeFps { get; set; }
    public double Mbps { get; set; }
    public int PingMs { get; set; }
    public int Quality { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FrameId { get; set; }
    public string Mode { get; set; } = "Düşük Gecikme";
}

internal static class ClientUpdateService
{
    public const string Version = "1.0.0";
    public static readonly string DefaultManifestUrl = "";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(6) };

    public sealed record Manifest(string Version, string Url, string Notes);

    public static async Task<Manifest?> CheckAsync(string url, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            return await Http.GetFromJsonAsync<Manifest>(url, cancellationToken: token);
        }
        catch { return null; }
    }

    public static bool IsNewer(string current, string candidate)
    {
        if (System.Version.TryParse(current, out var a) && System.Version.TryParse(candidate, out var b))
            return b > a;
        return !string.Equals(current, candidate, StringComparison.OrdinalIgnoreCase);
    }
}

internal static class QrLinks
{
    public static string BuildViewerUrl(string host, int port) => $"http://{host}:{port}/";
    public static string BuildClientUrl(string host, int port, string pin)
        => $"polaris://connect?host={Uri.EscapeDataString(host)}&port={port}&pin={Uri.EscapeDataString(pin ?? string.Empty)}";
}
