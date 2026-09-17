using System.Drawing;
using System.Windows.Forms;
namespace PolarisDisplay.Client;
internal sealed partial class ClientSettingsDialogForm:Form
{
 private readonly ClientSettings _settings;
 public ClientSettingsDialogForm(ClientSettings settings){_settings=settings;InitializeComponent(); PolarisTheme.Apply(this);}
 private void LoadSettings(object? s,EventArgs e){_host.Text=_settings.Host;_port.Value=Math.Clamp(_settings.Port,(int)_port.Minimum,(int)_port.Maximum);_pin.Text=_settings.Pin;_quality.SelectedItem=_settings.QualityMode;if(_quality.SelectedIndex<0)_quality.SelectedIndex=0;_autoReconnect.Checked=_settings.AutoReconnect;_discover.Checked=_settings.AutoDiscover;_mode.SelectedItem=_settings.StreamMode;if(_mode.SelectedIndex<0)_mode.SelectedIndex=0;_updateUrl.Text=_settings.UpdateManifestUrl;}
 private void SaveSettings(object? s,EventArgs e){_settings.Host=_host.Text.Trim();_settings.Port=(int)_port.Value;_settings.Pin=_pin.Text;_settings.QualityMode=_quality.Text;_settings.AutoReconnect=_autoReconnect.Checked;_settings.AutoDiscover=_discover.Checked;_settings.StreamMode=_mode.Text;_settings.UpdateManifestUrl=_updateUrl.Text.Trim();ClientSettingsStore.Save(_settings);DialogResult=DialogResult.OK;}
}
