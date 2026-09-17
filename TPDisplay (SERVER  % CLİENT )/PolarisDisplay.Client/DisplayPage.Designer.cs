#nullable disable
namespace PolarisDisplay.Client;

partial class DisplayPage
{
    private System.ComponentModel.IContainer components = null;
    private Panel _viewPanel;
    private VideoView _view;
    private NeonButton _fullscreenDisconnectButton;
    private NeonButton _fullscreenExitButton;
    private NeonPanel _displayPanel;
    private Label _displayTitle;
    private TableLayoutPanel _displayGrid;
    private Label _serverScreenLabel;
    private NeonComboBox _serverScreenSelect;
    private NeonComboBox _displayMode;
    private NeonComboBox _displayQuality;
    private NeonPanel _statusPanel;
    private Label _statusTitle;
    private Label _status;
    private Label _stats;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _viewPanel = new Panel();
        _fullscreenExitButton = new NeonButton();
        _fullscreenDisconnectButton = new NeonButton();
        _displayPanel = new NeonPanel();
        _displayGrid = new TableLayoutPanel();
        _serverScreenLabel = new Label();
        _serverScreenSelect = new NeonComboBox();
        _displayMode = new NeonComboBox();
        _displayQuality = new NeonComboBox();
        _displayTitle = new Label();
        _view = new VideoView();
        _statusPanel = new NeonPanel();
        _stats = new Label();
        _status = new Label();
        _statusTitle = new Label();
        _viewPanel.SuspendLayout();
        _displayPanel.SuspendLayout();
        _displayGrid.SuspendLayout();
        _statusPanel.SuspendLayout();
        SuspendLayout();
        // 
        // _viewPanel
        // 
        _viewPanel.BackColor = Color.FromArgb(3, 8, 18);
        _viewPanel.Controls.Add(_fullscreenExitButton);
        _viewPanel.Controls.Add(_fullscreenDisconnectButton);
        _viewPanel.Controls.Add(_displayPanel);
        _viewPanel.Controls.Add(_view);
        _viewPanel.Dock = DockStyle.Fill;
        _viewPanel.Location = new Point(0, 0);
        _viewPanel.Name = "_viewPanel";
        _viewPanel.Size = new Size(940, 582);
        _viewPanel.TabIndex = 0;
        // 
        // _fullscreenExitButton
        // 
        _fullscreenExitButton.Anchor = AnchorStyles.Top;
        _fullscreenExitButton.BorderRadius = 15;
        _fullscreenExitButton.FlatStyle = FlatStyle.Flat;
        _fullscreenExitButton.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
        _fullscreenExitButton.ForeColor = Color.White;
        _fullscreenExitButton.GradientEnd = Color.FromArgb(235, 0, 220);
        _fullscreenExitButton.GradientStart = Color.FromArgb(0, 111, 255);
        _fullscreenExitButton.HoverEnd = Color.FromArgb(255, 30, 225);
        _fullscreenExitButton.HoverStart = Color.FromArgb(35, 140, 255);
        _fullscreenExitButton.Location = new Point(736, 22);
        _fullscreenExitButton.Name = "_fullscreenExitButton";
        _fullscreenExitButton.Size = new Size(104, 30);
        _fullscreenExitButton.TabIndex = 1;
        _fullscreenExitButton.Text = "TAM EKRANDAN ÇIK";
        _fullscreenExitButton.UseBackColorFill = false;
        _fullscreenExitButton.Visible = false;
        // 
        // _fullscreenDisconnectButton
        // 
        _fullscreenDisconnectButton.Anchor = AnchorStyles.Top;
        _fullscreenDisconnectButton.BorderRadius = 15;
        _fullscreenDisconnectButton.FlatStyle = FlatStyle.Flat;
        _fullscreenDisconnectButton.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
        _fullscreenDisconnectButton.ForeColor = Color.White;
        _fullscreenDisconnectButton.GradientEnd = Color.FromArgb(225, 20, 155);
        _fullscreenDisconnectButton.GradientStart = Color.FromArgb(165, 35, 70);
        _fullscreenDisconnectButton.HoverEnd = Color.FromArgb(255, 30, 225);
        _fullscreenDisconnectButton.HoverStart = Color.FromArgb(35, 140, 255);
        _fullscreenDisconnectButton.Location = new Point(558, 22);
        _fullscreenDisconnectButton.Name = "_fullscreenDisconnectButton";
        _fullscreenDisconnectButton.Size = new Size(96, 30);
        _fullscreenDisconnectButton.TabIndex = 0;
        _fullscreenDisconnectButton.Text = "BAĞLANTIYI KES";
        _fullscreenDisconnectButton.UseBackColorFill = false;
        _fullscreenDisconnectButton.Visible = false;
        // 
        // _displayPanel
        // 
        _displayPanel.BackColor = Color.FromArgb(8, 18, 34);
        _displayPanel.BorderColor = Color.FromArgb(0, 125, 235);
        _displayPanel.BorderRadius = 12;
        _displayPanel.BorderThickness = 1;
        _displayPanel.Controls.Add(_displayGrid);
        _displayPanel.Controls.Add(_displayTitle);
        _displayPanel.Gradient = true;
        _displayPanel.GradientEnd = Color.FromArgb(19, 17, 45);
        _displayPanel.GradientStart = Color.FromArgb(6, 24, 43);
        _displayPanel.Anchor = AnchorStyles.Bottom;
        _displayPanel.Location = new Point(320, 496);
        _displayPanel.Name = "_displayPanel";
        _displayPanel.Padding = new Padding(10, 10, 10, 10);
        _displayPanel.Size = new Size(300, 60);
        _displayPanel.TabIndex = 1;
        // 
        // _displayGrid
        // 
        _displayGrid.ColumnCount = 1;
        _displayGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _displayGrid.Controls.Add(_serverScreenSelect, 0, 0);
        _displayGrid.Dock = DockStyle.Fill;
        _displayGrid.Location = new Point(10, 10);
        _displayGrid.Name = "_displayGrid";
        _displayGrid.RowCount = 1;
        _displayGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _displayGrid.Size = new Size(280, 40);
        _displayGrid.TabIndex = 0;
        // 
        // _serverScreenLabel
        // 
        _serverScreenLabel.Dock = DockStyle.Fill;
        _serverScreenLabel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        _serverScreenLabel.ForeColor = Color.FromArgb(135, 190, 235);
        _serverScreenLabel.Location = new Point(3, 0);
        _serverScreenLabel.Name = "_serverScreenLabel";
        _serverScreenLabel.Size = new Size(230, 48);
        _serverScreenLabel.TabIndex = 0;
        _serverScreenLabel.Text = "Sunucu ekranı";
        _serverScreenLabel.TextAlign = ContentAlignment.MiddleLeft;
        _serverScreenLabel.Visible = false;
        // 
        // _serverScreenSelect
        // 
        _serverScreenSelect.BackColor = Color.FromArgb(7, 18, 31);
        _serverScreenSelect.Dock = DockStyle.Fill;
        _serverScreenSelect.DrawMode = DrawMode.OwnerDrawFixed;
        _serverScreenSelect.DropDownHeight = 260;
        _serverScreenSelect.DropDownStyle = ComboBoxStyle.DropDownList;
        _serverScreenSelect.DropDownWidth = 360;
        _serverScreenSelect.FlatStyle = FlatStyle.Flat;
        _serverScreenSelect.Font = new Font("Segoe UI", 10.5F);
        _serverScreenSelect.ForeColor = Color.White;
        _serverScreenSelect.IntegralHeight = false;
        _serverScreenSelect.ItemHeight = 34;
        _serverScreenSelect.Location = new Point(0, 0);
        _serverScreenSelect.Margin = new Padding(0);
        _serverScreenSelect.Name = "_serverScreenSelect";
        _serverScreenSelect.Size = new Size(280, 40);
        _serverScreenSelect.TabIndex = 1;
        // 
        // _displayMode
        // 
        _displayMode.BackColor = Color.FromArgb(7, 18, 31);
        _displayMode.Dock = DockStyle.Fill;
        _displayMode.DrawMode = DrawMode.OwnerDrawFixed;
        _displayMode.DropDownHeight = 260;
        _displayMode.DropDownStyle = ComboBoxStyle.DropDownList;
        _displayMode.FlatStyle = FlatStyle.Flat;
        _displayMode.Font = new Font("Segoe UI", 10.5F);
        _displayMode.ForeColor = Color.White;
        _displayMode.IntegralHeight = false;
        _displayMode.ItemHeight = 34;
        _displayMode.Items.AddRange(new object[] { "Pencere", "Tam Ekran" });
        _displayMode.Location = new Point(0, 51);
        _displayMode.Margin = new Padding(0, 3, 8, 0);
        _displayMode.Name = "_displayMode";
        _displayMode.Size = new Size(228, 40);
        _displayMode.TabIndex = 2;
        _displayMode.Visible = false;
        // 
        // _displayQuality
        // 
        _displayQuality.BackColor = Color.FromArgb(7, 18, 31);
        _displayQuality.Dock = DockStyle.Fill;
        _displayQuality.DrawMode = DrawMode.OwnerDrawFixed;
        _displayQuality.DropDownHeight = 260;
        _displayQuality.DropDownStyle = ComboBoxStyle.DropDownList;
        _displayQuality.FlatStyle = FlatStyle.Flat;
        _displayQuality.Font = new Font("Segoe UI", 10.5F);
        _displayQuality.ForeColor = Color.White;
        _displayQuality.IntegralHeight = false;
        _displayQuality.ItemHeight = 34;
        _displayQuality.Items.AddRange(new object[] { "Otomatik", "Yüksek", "Dengeli" });
        _displayQuality.Location = new Point(244, 51);
        _displayQuality.Margin = new Padding(8, 3, 0, 0);
        _displayQuality.Name = "_displayQuality";
        _displayQuality.Size = new Size(229, 40);
        _displayQuality.TabIndex = 3;
        _displayQuality.Visible = false;
        // 
        // _displayTitle
        // 
        _displayTitle.Dock = DockStyle.Top;
        _displayTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _displayTitle.ForeColor = Color.White;
        _displayTitle.Location = new Point(14, 10);
        _displayTitle.Name = "_displayTitle";
        _displayTitle.Size = new Size(473, 28);
        _displayTitle.TabIndex = 1;
        _displayTitle.Text = "▣  Görüntü Ayarları";
        _displayTitle.Visible = false;
        // 
        // _view
        // 
        _view.BackColor = Color.Black;
        _view.Dock = DockStyle.Fill;
        _view.Location = new Point(0, 0);
        _view.Name = "_view";
        _view.Size = new Size(940, 582);
        _view.TabIndex = 0;
        // 
        // _statusPanel
        // 
        _statusPanel.BackColor = Color.FromArgb(8, 18, 34);
        _statusPanel.BorderColor = Color.FromArgb(0, 105, 190);
        _statusPanel.BorderRadius = 12;
        _statusPanel.BorderThickness = 1;
        _statusPanel.Controls.Add(_stats);
        _statusPanel.Controls.Add(_status);
        _statusPanel.Controls.Add(_statusTitle);
        _statusPanel.Dock = DockStyle.Bottom;
        _statusPanel.Gradient = true;
        _statusPanel.GradientEnd = Color.FromArgb(14, 15, 34);
        _statusPanel.GradientStart = Color.FromArgb(5, 18, 34);
        _statusPanel.Location = new Point(0, 582);
        _statusPanel.Name = "_statusPanel";
        _statusPanel.Padding = new Padding(14, 8, 14, 8);
        _statusPanel.Size = new Size(940, 58);
        _statusPanel.TabIndex = 2;
        // 
        // _stats
        // 
        _stats.Dock = DockStyle.Right;
        _stats.Font = new Font("Segoe UI", 9F);
        _stats.ForeColor = Color.FromArgb(135, 190, 235);
        _stats.Location = new Point(746, 8);
        _stats.Name = "_stats";
        _stats.Size = new Size(180, 42);
        _stats.TabIndex = 0;
        _stats.Text = "Bağlantı yok";
        _stats.TextAlign = ContentAlignment.MiddleRight;
        // 
        // _status
        // 
        _status.Dock = DockStyle.Fill;
        _status.Font = new Font("Segoe UI", 9F);
        _status.ForeColor = Color.White;
        _status.Location = new Point(104, 8);
        _status.Name = "_status";
        _status.Size = new Size(822, 42);
        _status.TabIndex = 1;
        _status.Text = "Bağlantı yok";
        _status.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _statusTitle
        // 
        _statusTitle.Dock = DockStyle.Left;
        _statusTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        _statusTitle.ForeColor = Color.FromArgb(165, 205, 235);
        _statusTitle.Location = new Point(14, 8);
        _statusTitle.Name = "_statusTitle";
        _statusTitle.Size = new Size(90, 42);
        _statusTitle.TabIndex = 2;
        _statusTitle.Text = "Durum";
        _statusTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // DisplayPage
        // 
        BackColor = Color.FromArgb(3, 8, 18);
        Controls.Add(_viewPanel);
        Controls.Add(_statusPanel);
        Name = "DisplayPage";
        Size = new Size(940, 640);
        _viewPanel.ResumeLayout(false);
        _displayPanel.ResumeLayout(false);
        _displayGrid.ResumeLayout(false);
        _statusPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
