#nullable disable
namespace PolarisDisplay.Server;

partial class LogPage
{
    private System.ComponentModel.IContainer components = null;
    private NeonPanel _logPanel;
    private Label _logTitle;
    private RichTextBox _log;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _logPanel = new NeonPanel();
        _log = new RichTextBox();
        _logTitle = new Label();
        _logPanel.SuspendLayout();
        SuspendLayout();
        // 
        // _logPanel
        // 
        _logPanel.BackColor = Color.FromArgb(8, 18, 34);
        _logPanel.BorderColor = Color.FromArgb(0, 105, 190);
        _logPanel.BorderRadius = 12;
        _logPanel.BorderThickness = 1;
        _logPanel.Controls.Add(_log);
        _logPanel.Controls.Add(_logTitle);
        _logPanel.Dock = DockStyle.Fill;
        _logPanel.Gradient = true;
        _logPanel.GradientEnd = Color.FromArgb(14, 15, 34);
        _logPanel.GradientStart = Color.FromArgb(5, 18, 34);
        _logPanel.Location = new Point(14, 14);
        _logPanel.Name = "_logPanel";
        _logPanel.Padding = new Padding(18, 12, 18, 14);
        _logPanel.Size = new Size(1180, 648);
        _logPanel.TabIndex = 0;
        // 
        // _log
        // 
        _log.BackColor = Color.FromArgb(2, 11, 22);
        _log.BorderStyle = BorderStyle.None;
        _log.Dock = DockStyle.Fill;
        _log.Font = new Font("Cascadia Mono", 9F);
        _log.ForeColor = Color.FromArgb(125, 190, 230);
        _log.Location = new Point(18, 42);
        _log.Name = "_log";
        _log.ReadOnly = true;
        _log.Size = new Size(1140, 580);
        _log.TabIndex = 0;
        _log.Text = "";
        // 
        // _logTitle
        // 
        _logTitle.Dock = DockStyle.Top;
        _logTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        _logTitle.ForeColor = Color.White;
        _logTitle.Location = new Point(18, 12);
        _logTitle.Name = "_logTitle";
        _logTitle.Size = new Size(1538, 30);
        _logTitle.TabIndex = 1;
        _logTitle.Text = "☷  Log";
        // 
        // LogPage
        // 
        BackColor = Color.FromArgb(3, 8, 18);
        Controls.Add(_logPanel);
        Name = "LogPage";
        Padding = new Padding(14);
        Size = new Size(1236, 704);
        _logPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
