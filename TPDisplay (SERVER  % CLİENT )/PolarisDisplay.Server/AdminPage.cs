namespace PolarisDisplay.Server;

public sealed partial class AdminPage : UserControl
{
    public event EventHandler? SaveSettings;
    public event EventHandler? RefreshQr;
    public event EventHandler? CheckUpdate;
    public AdminPage() => InitializeComponent();
    public string Pin => _pin.Text;
    public bool PinEnabled => _pinEnabled.Checked;
    public string UpdateUrl => _updateUrl.Text;
    internal void SetSettings(ServerSettings s)
    {
        _pinEnabled.Checked=!string.IsNullOrWhiteSpace(s.Pin); _pin.Text=s.Pin; _updateUrl.Text=s.UpdateManifestUrl;
    }
    public void SetStatus(string text)=>_status.Text=text;
    public void SetUpdateStatus(string text)=>_updateStatus.Text=text;
    public void SetQr(string url, Bitmap image){var old=_qr.Image;_qr.Image=image;old?.Dispose();_qrUrl.Text=url;}
    private void SaveSettingsClick(object? s,EventArgs e)=>SaveSettings?.Invoke(this,EventArgs.Empty);
    private void RefreshQrClick(object? s,EventArgs e)=>RefreshQr?.Invoke(this,EventArgs.Empty);
    private void CheckUpdateClick(object? s,EventArgs e)=>CheckUpdate?.Invoke(this,EventArgs.Empty);
}
