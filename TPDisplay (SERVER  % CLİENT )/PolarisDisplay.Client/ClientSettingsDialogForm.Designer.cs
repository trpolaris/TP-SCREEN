#nullable disable
namespace PolarisDisplay.Client;
partial class ClientSettingsDialogForm
{
    private System.ComponentModel.IContainer components = null;
    private TextBox _host, _pin, _updateUrl; private NumericUpDown _port; private ComboBox _quality, _mode; private CheckBox _autoReconnect, _discover; private Button _ok, _cancel;
    private Label _hostLabel, _portLabel, _pinLabel, _qualityLabel, _modeLabel, _updateUrlLabel;
    protected override void Dispose(bool disposing){ if(disposing) components?.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        components=new System.ComponentModel.Container(); _host=new TextBox(); _port=new NumericUpDown(); _pin=new TextBox(); _quality=new ComboBox(); _autoReconnect=new CheckBox(); _discover=new CheckBox(); _mode=new ComboBox(); _updateUrl=new TextBox(); _ok=new Button(); _cancel=new Button();
        _hostLabel=new Label(); _portLabel=new Label(); _pinLabel=new Label(); _qualityLabel=new Label(); _modeLabel=new Label(); _updateUrlLabel=new Label();
        ((System.ComponentModel.ISupportInitialize)_port).BeginInit(); SuspendLayout();
        Text="TRPOLARIS Viewer • Ayarlar"; StartPosition=FormStartPosition.CenterParent; ClientSize=new Size(560,475); MinimizeBox=false; MaximizeBox=false; FormBorderStyle=FormBorderStyle.FixedDialog; BackColor=Color.FromArgb(7,18,31); ForeColor=Color.White; Load+=LoadSettings;
        _hostLabel.Left=24; _hostLabel.Top=27; _hostLabel.AutoSize=true; _hostLabel.Text="Sunucu IP / Host"; _hostLabel.ForeColor=Color.FromArgb(160,195,220);
        _portLabel.Left=24; _portLabel.Top=67; _portLabel.AutoSize=true; _portLabel.Text="Port"; _portLabel.ForeColor=Color.FromArgb(160,195,220);
        _pinLabel.Left=24; _pinLabel.Top=107; _pinLabel.AutoSize=true; _pinLabel.Text="PIN"; _pinLabel.ForeColor=Color.FromArgb(160,195,220);
        _qualityLabel.Left=24; _qualityLabel.Top=147; _qualityLabel.AutoSize=true; _qualityLabel.Text="Kalite"; _qualityLabel.ForeColor=Color.FromArgb(160,195,220);
        _modeLabel.Left=24; _modeLabel.Top=247; _modeLabel.AutoSize=true; _modeLabel.Text="Mod"; _modeLabel.ForeColor=Color.FromArgb(160,195,220);
        _updateUrlLabel.Left=24; _updateUrlLabel.Top=277; _updateUrlLabel.AutoSize=true; _updateUrlLabel.Text="Güncelleme URL"; _updateUrlLabel.ForeColor=Color.FromArgb(160,195,220);
        _host.Left=150; _host.Top=24; _host.Width=365;
        _pin.Left=150; _pin.Top=104; _pin.Width=200; _pin.UseSystemPasswordChar=true;
        _updateUrl.Left=150; _updateUrl.Top=274; _updateUrl.Width=365; _updateUrl.PlaceholderText="Güncelleme manifest URL (opsiyonel)";
        _port.Left=150; _port.Top=64; _port.Width=120; _port.Minimum=1; _port.Maximum=65535;
        _quality.Left=150; _quality.Top=144; _quality.Width=200; _quality.DropDownStyle=ComboBoxStyle.DropDownList; _quality.Items.AddRange(new object[]{"Otomatik","Yüksek","Dengeli"});
        _mode.Left=150; _mode.Top=244; _mode.Width=240; _mode.DropDownStyle=ComboBoxStyle.DropDownList; _mode.Items.AddRange(new object[]{"Düşük Gecikme","Kalite Öncelikli"});
        _autoReconnect.Left=150; _autoReconnect.Top=184; _autoReconnect.Width=240; _autoReconnect.Text="Otomatik yeniden bağlan"; _autoReconnect.ForeColor=Color.White;
        _discover.Left=150; _discover.Top=214; _discover.Width=240; _discover.Text="LAN sunucu keşfi"; _discover.ForeColor=Color.White;
        _ok.Left=280; _ok.Top=435; _ok.Width=110; _ok.Text="Kaydet"; _ok.Click+=SaveSettings; _cancel.Left=400; _cancel.Top=435; _cancel.Width=110; _cancel.Text="İptal"; _cancel.DialogResult=DialogResult.Cancel; AcceptButton=_ok; CancelButton=_cancel;
        Controls.AddRange(new Control[]{_hostLabel,_portLabel,_pinLabel,_qualityLabel,_modeLabel,_updateUrlLabel,_host,_port,_pin,_quality,_autoReconnect,_discover,_mode,_updateUrl,_ok,_cancel});
        ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)_port).EndInit();
    }
}
