#nullable disable
namespace PolarisDisplay.Server;
partial class ServerSettingsDialogForm
{
    private System.ComponentModel.IContainer components = null;
    private NumericUpDown _port, _fps, _quality, _bitrate;
    private TextBox _pin, _updateUrl;
    private CheckBox _auto;
    private Button _ok, _cancel;
    private Label _portLabel, _fpsLabel, _qualityLabel, _bitrateLabel, _pinLabel, _updateUrlLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _port = new NumericUpDown(); _fps = new NumericUpDown(); _quality = new NumericUpDown(); _bitrate = new NumericUpDown();
        _pin = new TextBox(); _updateUrl = new TextBox(); _auto = new CheckBox(); _ok = new Button(); _cancel = new Button();
        _portLabel = new Label(); _fpsLabel = new Label(); _qualityLabel = new Label(); _bitrateLabel = new Label(); _pinLabel = new Label(); _updateUrlLabel = new Label();
        ((System.ComponentModel.ISupportInitialize)_port).BeginInit(); ((System.ComponentModel.ISupportInitialize)_fps).BeginInit(); ((System.ComponentModel.ISupportInitialize)_quality).BeginInit(); ((System.ComponentModel.ISupportInitialize)_bitrate).BeginInit();
        SuspendLayout();
        Text = "TRPOLARIS Server • Ayarlar"; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(600, 510);
        MinimizeBox = false; MaximizeBox = false; FormBorderStyle = FormBorderStyle.FixedDialog; BackColor = Color.FromArgb(7,18,31); ForeColor = Color.White;
        Load += ServerSettingsDialogForm_Load;
        _portLabel.Left=24; _portLabel.Top=27; _portLabel.AutoSize=true; _portLabel.Text="Port"; _portLabel.ForeColor=Color.FromArgb(160,195,220);
        _fpsLabel.Left=24; _fpsLabel.Top=67; _fpsLabel.AutoSize=true; _fpsLabel.Text="FPS"; _fpsLabel.ForeColor=Color.FromArgb(160,195,220);
        _qualityLabel.Left=24; _qualityLabel.Top=107; _qualityLabel.AutoSize=true; _qualityLabel.Text="JPEG kalite"; _qualityLabel.ForeColor=Color.FromArgb(160,195,220);
        _bitrateLabel.Left=24; _bitrateLabel.Top=147; _bitrateLabel.AutoSize=true; _bitrateLabel.Text="Bitrate Mbps"; _bitrateLabel.ForeColor=Color.FromArgb(160,195,220);
        _pinLabel.Left=24; _pinLabel.Top=187; _pinLabel.AutoSize=true; _pinLabel.Text="PIN"; _pinLabel.ForeColor=Color.FromArgb(160,195,220);
        _updateUrlLabel.Left=24; _updateUrlLabel.Top=247; _updateUrlLabel.AutoSize=true; _updateUrlLabel.Text="Güncelleme URL"; _updateUrlLabel.ForeColor=Color.FromArgb(160,195,220);
        _port.Left=150; _port.Top=24; _port.Width=180; _port.Minimum=1; _port.Maximum=65535;
        _fps.Left=150; _fps.Top=64; _fps.Width=180; _fps.Minimum=1; _fps.Maximum=240;
        _quality.Left=150; _quality.Top=104; _quality.Width=180; _quality.Minimum=50; _quality.Maximum=100;
        _bitrate.Left=150; _bitrate.Top=144; _bitrate.Width=180; _bitrate.Minimum=2; _bitrate.Maximum=200;
        _pin.Left = 150; _pin.Top = 184; _pin.Width = 220; _pin.UseSystemPasswordChar = true;
        _updateUrl.Left = 150; _updateUrl.Top = 244; _updateUrl.Width = 420; _updateUrl.PlaceholderText = "Güncelleme manifest URL (opsiyonel)";
        _auto.Left = 150; _auto.Top = 214; _auto.Width = 220; _auto.Text = "Otomatik bitrate"; _auto.ForeColor = Color.White;
        _ok.Left = 330; _ok.Top = 455; _ok.Width = 110; _ok.Text = "Kaydet"; _ok.Click += _ok_Click;
        _cancel.Left = 460; _cancel.Top = 455; _cancel.Width = 110; _cancel.Text = "İptal"; _cancel.DialogResult = DialogResult.Cancel;
        AcceptButton = _ok; CancelButton = _cancel;
        Controls.AddRange(new Control[] { _portLabel, _fpsLabel, _qualityLabel, _bitrateLabel, _pinLabel, _updateUrlLabel, _port, _fps, _quality, _bitrate, _pin, _updateUrl, _auto, _ok, _cancel });
        ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_port).EndInit(); ((System.ComponentModel.ISupportInitialize)_fps).EndInit(); ((System.ComponentModel.ISupportInitialize)_quality).EndInit(); ((System.ComponentModel.ISupportInitialize)_bitrate).EndInit();
    }

}
