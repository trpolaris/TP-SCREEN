using System.Drawing;
using System.Windows.Forms;
namespace PolarisDisplay.Server;
internal sealed partial class ServerSettingsDialogForm : Form
{
    private readonly ServerSettings _settings;
    public ServerSettingsDialogForm(ServerSettings settings){ _settings=settings; InitializeComponent(); PolarisTheme.Apply(this); }
    private void ServerSettingsDialogForm_Load(object? sender, EventArgs e){
        _port.Value=Math.Clamp(_settings.Port,(int)_port.Minimum,(int)_port.Maximum); _fps.Value=Math.Clamp(_settings.Fps,(int)_fps.Minimum,(int)_fps.Maximum);
        _quality.Value=Math.Clamp(_settings.Quality,(int)_quality.Minimum,(int)_quality.Maximum); _bitrate.Value=Math.Clamp(_settings.BitrateMbps,(int)_bitrate.Minimum,(int)_bitrate.Maximum);
        _pin.Text=_settings.Pin; _updateUrl.Text=_settings.UpdateManifestUrl; _auto.Checked=_settings.AutoBitrate;
    }
    private void _ok_Click(object? sender, EventArgs e){ _settings.Port=(int)_port.Value; _settings.Fps=(int)_fps.Value; _settings.Quality=(int)_quality.Value; _settings.BitrateMbps=(int)_bitrate.Value; _settings.AutoBitrate=_auto.Checked; _settings.Pin=_pin.Text; _settings.UpdateManifestUrl=_updateUrl.Text.Trim(); ServerSettingsStore.Save(_settings); DialogResult=DialogResult.OK; }
}
