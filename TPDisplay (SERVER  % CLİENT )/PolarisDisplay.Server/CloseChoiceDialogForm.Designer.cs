#nullable disable
namespace PolarisDisplay.Server;

partial class CloseChoiceDialogForm
{
    private System.ComponentModel.IContainer components = null;
    private NeonPanel _shell;
    private Panel _header;
    private Label _title, _question, _info;
    private ChromeButton _close;
    private NeonButton _yes, _no, _cancel;

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
        _question = new Label();
        _info = new Label();
        _yes = new NeonButton();
        _no = new NeonButton();
        _cancel = new NeonButton();
        SuspendLayout();
        _shell.SuspendLayout();
        _header.SuspendLayout();

        // CloseChoiceDialogForm
        Text = "TRPOLARIS Server";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        ClientSize = new Size(500, 215);
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
        _title.Text = "TRPOLARIS Server";
        _title.ForeColor = Color.White;
        _title.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        _title.AutoSize = true;
        _title.Location = new Point(16, 12);

        // _close
        _close.Glyph = "×";
        _close.Size = new Size(40, 40);
        _close.Location = new Point(459, 1);
        _close.ForeColor = Color.FromArgb(255, 125, 170);
        _close.Click += _close_Click;

        // _question
        _question.Text = "TRPOLARIS Server'ı kapatmak istiyor musun?";
        _question.Left = 22;
        _question.Top = 64;
        _question.AutoSize = true;
        _question.ForeColor = Color.White;
        _question.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

        // _info
        _info.Text = "Evet: Server'ı kapat\nHayır: Bildirim alanında çalışmaya devam et\nİptal: Pencereyi açık bırak";
        _info.Left = 22;
        _info.Top = 94;
        _info.AutoSize = true;
        _info.ForeColor = Color.FromArgb(160, 205, 235);
        _info.Font = new Font("Segoe UI", 8.5F);

        // _yes
        _yes.Text = "EVET";
        _yes.Left = 270;
        _yes.Top = 158;
        _yes.Width = 68;
        _yes.Height = 34;
        _yes.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _yes.BorderRadius = 7;
        _yes.GradientStart = Color.FromArgb(45, 52, 70);
        _yes.GradientEnd = Color.FromArgb(80, 35, 80);
        _yes.HoverStart = Color.FromArgb(60, 80, 105);
        _yes.HoverEnd = Color.FromArgb(110, 45, 100);
        _yes.Click += _yes_Click;

        // _no
        _no.Text = "HAYIR";
        _no.Left = 344;
        _no.Top = 158;
        _no.Width = 68;
        _no.Height = 34;
        _no.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _no.BorderRadius = 7;
        _no.GradientStart = Color.FromArgb(45, 52, 70);
        _no.GradientEnd = Color.FromArgb(80, 35, 80);
        _no.HoverStart = Color.FromArgb(60, 80, 105);
        _no.HoverEnd = Color.FromArgb(110, 45, 100);
        _no.Click += _no_Click;

        // _cancel
        _cancel.Text = "İPTAL";
        _cancel.Left = 418;
        _cancel.Top = 158;
        _cancel.Width = 68;
        _cancel.Height = 34;
        _cancel.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _cancel.BorderRadius = 7;
        _cancel.GradientStart = Color.FromArgb(45, 52, 70);
        _cancel.GradientEnd = Color.FromArgb(80, 35, 80);
        _cancel.HoverStart = Color.FromArgb(60, 80, 105);
        _cancel.HoverEnd = Color.FromArgb(110, 45, 100);
        _cancel.Click += _close_Click;

        _header.Controls.Add(_title);
        _header.Controls.Add(_close);
        _shell.Controls.Add(_header);
        _shell.Controls.Add(_question);
        _shell.Controls.Add(_info);
        _shell.Controls.Add(_yes);
        _shell.Controls.Add(_no);
        _shell.Controls.Add(_cancel);
        Controls.Add(_shell);
        AcceptButton = _yes;
        CancelButton = _cancel;

        _header.ResumeLayout(false);
        _header.PerformLayout();
        _shell.ResumeLayout(false);
        ResumeLayout(false);
    }
}
