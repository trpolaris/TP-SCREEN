#nullable disable
namespace PolarisDisplay.Client;

partial class PinDialogForm
{
    private System.ComponentModel.IContainer components = null;
    private NeonPanel _shell;
    private Panel _header;
    private Label _title, _label;
    private ChromeButton _close;
    private TextBox _box;
    private NeonButton _ok, _cancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _shell = new NeonPanel();
        _header = new Panel();
        _title = new Label();
        _close = new ChromeButton();
        _label = new Label();
        _box = new TextBox();
        _ok = new NeonButton();
        _cancel = new NeonButton();
        SuspendLayout();
        _shell.SuspendLayout();
        _header.SuspendLayout();

        // PinDialogForm
        Text = "PolarisDisplay PIN";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(430, 190);
        BackColor = Color.FromArgb(3, 10, 22);
        ForeColor = Color.White;

        // _shell
        _shell.Dock = DockStyle.Fill;
        _shell.BorderRadius = 12;
        _shell.BorderColor = Color.FromArgb(0, 150, 255);
        _shell.GradientStart = Color.FromArgb(6, 20, 38);
        _shell.GradientEnd = Color.FromArgb(25, 12, 48);

        // _header
        _header.Dock = DockStyle.Top;
        _header.Height = 42;
        _header.BackColor = Color.FromArgb(5, 12, 25);

        // _title
        _title.Text = "PolarisDisplay PIN";
        _title.ForeColor = Color.White;
        _title.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        _title.AutoSize = true;
        _title.Location = new Point(16, 12);

        // _close
        _close.Glyph = "×";
        _close.Size = new Size(40, 40);
        _close.Location = new Point(389, 1);
        _close.ForeColor = Color.FromArgb(255, 125, 170);
        _close.Click += _close_Click;

        // _label
        _label.Text = "Sunucunun PIN kodunu girin";
        _label.Left = 22;
        _label.Top = 58;
        _label.AutoSize = true;
        _label.ForeColor = Color.FromArgb(175, 210, 235);
        _label.Font = new Font("Segoe UI", 9.5F);

        // _box
        _box.Left = 22;
        _box.Top = 88;
        _box.Width = 386;
        _box.Height = 30;
        _box.UseSystemPasswordChar = true;
        _box.MaxLength = 64;
        _box.BackColor = Color.FromArgb(7, 18, 31);
        _box.ForeColor = Color.White;
        _box.BorderStyle = BorderStyle.FixedSingle;
        _box.Font = new Font("Segoe UI", 10.5F);

        // _ok
        _ok.Text = "BAĞLAN";
        _ok.Left = 218;
        _ok.Top = 135;
        _ok.Width = 90;
        _ok.Height = 38;
        _ok.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        _ok.BorderRadius = 8;
        _ok.Click += _ok_Click;

        // _cancel
        _cancel.Text = "İPTAL";
        _cancel.Left = 318;
        _cancel.Top = 135;
        _cancel.Width = 90;
        _cancel.Height = 38;
        _cancel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        _cancel.BorderRadius = 8;
        _cancel.GradientStart = Color.FromArgb(45, 52, 70);
        _cancel.GradientEnd = Color.FromArgb(80, 35, 80);
        _cancel.HoverStart = Color.FromArgb(60, 80, 105);
        _cancel.HoverEnd = Color.FromArgb(110, 45, 100);
        _cancel.Click += _cancel_Click;

        _header.Controls.Add(_title);
        _header.Controls.Add(_close);
        _shell.Controls.Add(_header);
        _shell.Controls.Add(_label);
        _shell.Controls.Add(_box);
        _shell.Controls.Add(_ok);
        _shell.Controls.Add(_cancel);
        Controls.Add(_shell);
        AcceptButton = _ok;
        CancelButton = _cancel;

        _header.ResumeLayout(false);
        _header.PerformLayout();
        _shell.ResumeLayout(false);
        ResumeLayout(false);
    }
}
