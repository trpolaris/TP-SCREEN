#nullable disable
namespace PolarisDisplay.Server;

partial class ConnectionsPage
{
    private System.ComponentModel.IContainer components = null;
    private NeonPanel _connectionsPanel;
    private Label _connectionsTitle;
    private FlowLayoutPanel _connectionsFlow;
    private Label _connectionsEmpty;
    private NeonButton _clearHistory;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _connectionsPanel = new NeonPanel();
        _connectionsFlow = new FlowLayoutPanel();
        _connectionsEmpty = new Label();
        _connectionsTitle = new Label();
        _clearHistory = new NeonButton();
        _connectionsPanel.SuspendLayout();
        _connectionsFlow.SuspendLayout();
        SuspendLayout();
        // 
        // _connectionsPanel
        // 
        _connectionsPanel.BackColor = Color.FromArgb(8, 18, 34);
        _connectionsPanel.BorderColor = Color.FromArgb(0, 145, 235);
        _connectionsPanel.BorderRadius = 12;
        _connectionsPanel.BorderThickness = 1;
        _connectionsPanel.Controls.Add(_connectionsFlow);
        _connectionsPanel.Controls.Add(_clearHistory);
        _connectionsPanel.Controls.Add(_connectionsTitle);
        _connectionsPanel.Dock = DockStyle.Fill;
        _connectionsPanel.Gradient = true;
        _connectionsPanel.GradientEnd = Color.FromArgb(25, 14, 54);
        _connectionsPanel.GradientStart = Color.FromArgb(7, 25, 49);
        _connectionsPanel.Location = new Point(14, 14);
        _connectionsPanel.Name = "_connectionsPanel";
        _connectionsPanel.Padding = new Padding(18, 14, 18, 14);
        _connectionsPanel.Size = new Size(1180, 648);
        _connectionsPanel.TabIndex = 0;
        // 
        // _connectionsFlow
        // 
        _connectionsFlow.AutoScroll = true;
        _connectionsFlow.BackColor = Color.Transparent;
        _connectionsFlow.Controls.Add(_connectionsEmpty);
        _connectionsFlow.Dock = DockStyle.Fill;
        _connectionsFlow.FlowDirection = FlowDirection.TopDown;
        _connectionsFlow.Location = new Point(18, 52);
        _connectionsFlow.Name = "_connectionsFlow";
        _connectionsFlow.Padding = new Padding(0, 8, 0, 0);
        _connectionsFlow.Size = new Size(1140, 580);
        _connectionsFlow.TabIndex = 0;
        _connectionsFlow.WrapContents = false;
        // 
        // _connectionsEmpty
        // 
        _connectionsEmpty.Font = new Font("Segoe UI", 10F);
        _connectionsEmpty.ForeColor = Color.FromArgb(145, 185, 215);
        _connectionsEmpty.Location = new Point(3, 8);
        _connectionsEmpty.Name = "_connectionsEmpty";
        _connectionsEmpty.Size = new Size(520, 40);
        _connectionsEmpty.TabIndex = 0;
        _connectionsEmpty.Text = "Henüz bağlı bilgisayar yok.";
        _connectionsEmpty.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _connectionsTitle
        // 
        _connectionsTitle.Dock = DockStyle.Top;
        _connectionsTitle.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
        _connectionsTitle.ForeColor = Color.White;
        _connectionsTitle.Location = new Point(18, 14);
        _connectionsTitle.Name = "_connectionsTitle";
        _connectionsTitle.Size = new Size(990, 38);
        _connectionsTitle.TabIndex = 1;
        _connectionsTitle.Text = "♧  Bağlantılar";
        // 
        // _clearHistory
        // 
        _clearHistory.BackColor = Color.FromArgb(12, 30, 48);
        _clearHistory.BorderRadius = 8;
        _clearHistory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _clearHistory.Dock = DockStyle.None;
        _clearHistory.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _clearHistory.ForeColor = Color.White;
        _clearHistory.Name = "_clearHistory";
        _clearHistory.Location = new Point(994, 16);
        _clearHistory.Size = new Size(150, 34);
        _clearHistory.TabIndex = 2;
        _clearHistory.Text = "GEÇMİŞİ TEMİZLE";
        _clearHistory.Click += ClearHistoryClick;
        // 
        // ConnectionsPage
        // 
        BackColor = Color.FromArgb(3, 8, 18);
        Controls.Add(_connectionsPanel);
        Name = "ConnectionsPage";
        Padding = new Padding(14);
        Size = new Size(1236, 704);
        _connectionsPanel.ResumeLayout(false);
        _connectionsFlow.ResumeLayout(false);
        ResumeLayout(false);
    }
}
