#nullable disable
namespace PolarisDisplay.Server;

partial class AdminPage
{
    private System.ComponentModel.IContainer components = null;
    private Label _title, _status, _qrUrl, _updateStatus;
    private Label _qrTitle, _updateTitle;
    private CheckBox _pinEnabled;
    private TextBox _pin, _updateUrl;
    private Button _saveSettings, _refreshQr, _checkUpdate;
    private PictureBox _qr;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _title = new Label();
        _status = new Label();
        _qrUrl = new Label();
        _updateStatus = new Label();
        _qrTitle = new Label();
        _updateTitle = new Label();
        _pinEnabled = new CheckBox();
        _pin = new TextBox();
        _updateUrl = new TextBox();
        _saveSettings = new Button();
        _refreshQr = new Button();
        _checkUpdate = new Button();
        _qr = new PictureBox();
        ((System.ComponentModel.ISupportInitialize)_qr).BeginInit();
        SuspendLayout();

        // AdminPage
        BackColor = Color.FromArgb(3, 8, 18);
        Name = "AdminPage";
        Padding = new Padding(24);
        AutoScroll = true;

        // _title
        _title.Text = "⚙  Yönetim • Güvenlik • QR • Güncelleme";
        _title.Dock = DockStyle.Top;
        _title.Height = 40;
        _title.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold);
        _title.ForeColor = Color.White;

        // _pinEnabled
        _pinEnabled.Text = "PIN güvenliğini etkinleştir";
        _pinEnabled.Left = 24;
        _pinEnabled.Top = 52;
        _pinEnabled.AutoSize = true;
        _pinEnabled.ForeColor = Color.White;

        // _pin
        _pin.Left = 24;
        _pin.Top = 80;
        _pin.Width = 260;
        _pin.UseSystemPasswordChar = true;
        _pin.PlaceholderText = "PIN";

        // _updateUrl
        _updateUrl.Left = 24;
        _updateUrl.Top = 118;
        _updateUrl.Width = 420;
        _updateUrl.PlaceholderText = "Güncelleme manifest URL";

        // _saveSettings
        _saveSettings.Text = "GÜVENLİĞİ / URL'Yİ KAYDET";
        _saveSettings.Left = 460;
        _saveSettings.Top = 116;
        _saveSettings.Width = 210;
        _saveSettings.Click += SaveSettingsClick;

        // _status
        _status.Left = 24;
        _status.Top = 154;
        _status.Width = 650;
        _status.ForeColor = Color.FromArgb(105, 215, 160);
        _status.AutoSize = true;
        _status.Text = "Hazır";

        // _qrTitle
        _qrTitle.Text = "QR WEB VIEWER";
        _qrTitle.Left = 24;
        _qrTitle.Top = 310;
        _qrTitle.AutoSize = true;
        _qrTitle.ForeColor = Color.FromArgb(95, 210, 255);
        _qrTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

        // _qr
        _qr.Left = 24;
        _qr.Top = 335;
        _qr.Size = new Size(130, 130);
        _qr.SizeMode = PictureBoxSizeMode.Zoom;
        _qr.BackColor = Color.White;

        // _qrUrl
        _qrUrl.Left = 170;
        _qrUrl.Top = 350;
        _qrUrl.Width = 450;
        _qrUrl.AutoSize = false;
        _qrUrl.Height = 40;
        _qrUrl.ForeColor = Color.FromArgb(165, 205, 235);

        // _refreshQr
        _refreshQr.Text = "QR YENİLE";
        _refreshQr.Left = 170;
        _refreshQr.Top = 400;
        _refreshQr.Width = 150;
        _refreshQr.Click += RefreshQrClick;

        // _updateTitle
        _updateTitle.Text = "GÜNCELLEME";
        _updateTitle.Left = 480;
        _updateTitle.Top = 310;
        _updateTitle.AutoSize = true;
        _updateTitle.ForeColor = Color.FromArgb(95, 210, 255);
        _updateTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

        // _updateStatus
        _updateStatus.Left = 480;
        _updateStatus.Top = 360;
        _updateStatus.Width = 340;
        _updateStatus.Height = 45;
        _updateStatus.ForeColor = Color.FromArgb(185, 220, 245);
        _updateStatus.Text = "Manifest URL girip kontrol edin.";

        // _checkUpdate
        _checkUpdate.Text = "GÜNCELLEMEYİ KONTROL ET";
        _checkUpdate.Left = 480;
        _checkUpdate.Top = 420;
        _checkUpdate.Width = 250;
        _checkUpdate.Click += CheckUpdateClick;

        Controls.Add(_checkUpdate);
        Controls.Add(_updateStatus);
        Controls.Add(_updateTitle);
        Controls.Add(_refreshQr);
        Controls.Add(_qrUrl);
        Controls.Add(_qr);
        Controls.Add(_qrTitle);
        Controls.Add(_saveSettings);
        Controls.Add(_updateUrl);
        Controls.Add(_pin);
        Controls.Add(_pinEnabled);
        Controls.Add(_status);
        Controls.Add(_title);

        ((System.ComponentModel.ISupportInitialize)_qr).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
