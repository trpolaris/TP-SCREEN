#nullable disable
namespace PolarisDisplay.Client;

partial class ConnectionPage
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel _rootLayout;
    private NeonPanel _connectPanel;
    private Label _connectTitle;
    private Panel _connectionBody;
    private TableLayoutPanel _connectionGrid;
    private Label _ipLabel;
    private Label _portLabel;
    private NeonTextBox _host;
    private NumericUpDown _port;
    private Label _cardTitle;
    private Label _cardSub;
    private NeonButton _cardConnect;
    private NeonPanel _quickPanel;
    private Panel _quickBody;
    private TableLayoutPanel _quickCommands;
    private Button _quickRecent;
    private Button _quickFavorites;
    private Button _quickClear;
    private Panel _recentListPanel;
    private Panel _favoritesListPanel;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _rootLayout = new TableLayoutPanel();
        _connectPanel = new NeonPanel();
        _connectionBody = new Panel();
        _connectionGrid = new TableLayoutPanel();
        _ipLabel = new Label();
        _portLabel = new Label();
        _port = new NumericUpDown();
        _cardConnect = new NeonButton();
        _host = new NeonTextBox();
        _cardTitle = new Label();
        _cardSub = new Label();
        _connectTitle = new Label();
        _quickPanel = new NeonPanel();
        _quickBody = new Panel();
        _favoritesListPanel = new Panel();
        _recentListPanel = new Panel();
        _quickCommands = new TableLayoutPanel();
        _quickRecent = new Button();
        _quickFavorites = new Button();
        _quickClear = new Button();
        _rootLayout.SuspendLayout();
        _connectPanel.SuspendLayout();
        _connectionBody.SuspendLayout();
        _connectionGrid.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_port).BeginInit();
        _quickPanel.SuspendLayout();
        _quickBody.SuspendLayout();
        _quickCommands.SuspendLayout();
        SuspendLayout();
        // 
        // _rootLayout
        // 
        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(_connectPanel, 0, 0);
        _rootLayout.Controls.Add(_quickPanel, 0, 1);
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.Location = new Point(14, 14);
        _rootLayout.Name = "_rootLayout";
        _rootLayout.RowCount = 2;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _rootLayout.Size = new Size(768, 485);
        _rootLayout.TabIndex = 0;
        // 
        // _connectPanel
        // 
        _connectPanel.BackColor = Color.FromArgb(8, 18, 34);
        _connectPanel.BorderColor = Color.FromArgb(0, 150, 255);
        _connectPanel.BorderRadius = 12;
        _connectPanel.BorderThickness = 1;
        _connectPanel.Controls.Add(_connectionBody);
        _connectPanel.Controls.Add(_connectTitle);
        _connectPanel.Dock = DockStyle.Fill;
        _connectPanel.Gradient = true;
        _connectPanel.GradientEnd = Color.FromArgb(25, 14, 54);
        _connectPanel.GradientStart = Color.FromArgb(7, 25, 49);
        _connectPanel.Location = new Point(0, 0);
        _connectPanel.Margin = new Padding(0, 0, 0, 10);
        _connectPanel.Name = "_connectPanel";
        _connectPanel.Padding = new Padding(16, 12, 16, 12);
        _connectPanel.Size = new Size(768, 180);
        _connectPanel.TabIndex = 0;
        // 
        // _connectionBody
        // 
        _connectionBody.BackColor = Color.Transparent;
        _connectionBody.Controls.Add(_connectionGrid);
        _connectionBody.Location = new Point(18, 54);
        _connectionBody.Name = "_connectionBody";
        _connectionBody.Size = new Size(748, 112);
        _connectionBody.TabIndex = 0;
        // 
        // _connectionGrid
        // 
        _connectionGrid.ColumnCount = 2;
        _connectionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 69.2513351F));
        _connectionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30.7486629F));
        _connectionGrid.Controls.Add(_ipLabel, 0, 0);
        _connectionGrid.Controls.Add(_portLabel, 1, 0);
        _connectionGrid.Controls.Add(_port, 1, 1);
        _connectionGrid.Controls.Add(_cardConnect, 1, 2);
        _connectionGrid.Controls.Add(_host, 0, 1);
        _connectionGrid.Controls.Add(_cardTitle, 0, 2);
        _connectionGrid.Controls.Add(_cardSub, 0, 3);
        _connectionGrid.Dock = DockStyle.Fill;
        _connectionGrid.Location = new Point(0, 0);
        _connectionGrid.Name = "_connectionGrid";
        _connectionGrid.RowCount = 4;
        _connectionGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        _connectionGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        _connectionGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        _connectionGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _connectionGrid.Size = new Size(748, 112);
        _connectionGrid.TabIndex = 0;
        // 
        // _ipLabel
        // 
        _ipLabel.Dock = DockStyle.Fill;
        _ipLabel.Font = new Font("Segoe UI", 9F);
        _ipLabel.ForeColor = Color.FromArgb(165, 205, 235);
        _ipLabel.Location = new Point(3, 0);
        _ipLabel.Name = "_ipLabel";
        _ipLabel.Size = new Size(512, 20);
        _ipLabel.TabIndex = 0;
        _ipLabel.Text = "IP / Sunucu";
        _ipLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _portLabel
        // 
        _portLabel.Dock = DockStyle.Fill;
        _portLabel.Font = new Font("Segoe UI", 9F);
        _portLabel.ForeColor = Color.FromArgb(165, 205, 235);
        _portLabel.Location = new Point(521, 0);
        _portLabel.Name = "_portLabel";
        _portLabel.Size = new Size(224, 20);
        _portLabel.TabIndex = 1;
        _portLabel.Text = "Port";
        _portLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _port
        // 
        _port.BackColor = Color.FromArgb(7, 18, 31);
        _port.BorderStyle = BorderStyle.None;
        _port.Dock = DockStyle.Fill;
        _port.Font = new Font("Segoe UI", 10F);
        _port.ForeColor = Color.White;
        _port.Location = new Point(518, 21);
        _port.Margin = new Padding(0, 1, 0, 5);
        _port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        _port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _port.Name = "_port";
        _port.Size = new Size(230, 21);
        _port.TabIndex = 3;
        _port.TextAlign = HorizontalAlignment.Center;
        _port.Value = new decimal(new int[] { 50505, 0, 0, 0 });
        // 
        // _cardConnect
        // 
        _cardConnect.BorderRadius = 9;
        _cardConnect.FlatStyle = FlatStyle.Flat;
        _cardConnect.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        _cardConnect.ForeColor = Color.White;
        _cardConnect.GradientEnd = Color.FromArgb(30, 78, 190);
        _cardConnect.GradientStart = Color.FromArgb(9, 132, 220);
        _cardConnect.HoverEnd = Color.FromArgb(75, 104, 230);
        _cardConnect.HoverStart = Color.FromArgb(35, 181, 255);
        _cardConnect.Location = new Point(526, 49);
        _cardConnect.Margin = new Padding(8, 5, 0, 5);
        _cardConnect.Name = "_cardConnect";
        _connectionGrid.SetRowSpan(_cardConnect, 2);
        _cardConnect.Size = new Size(140, 40);
        _cardConnect.TabIndex = 6;
        _cardConnect.Text = "BAĞLAN";
        _cardConnect.UseBackColorFill = false;
        // 
        // _host
        // 
        _host.BackColor = Color.Transparent;
        _host.BorderColor = Color.FromArgb(0, 190, 255);
        _host.BorderRadius = 9;
        _host.BorderStyle = BorderStyle.None;
        _host.BorderThickness = 1;
        _host.Dock = DockStyle.Fill;
        _host.FocusBorderColor = Color.FromArgb(225, 20, 220);
        _host.Font = new Font("Segoe UI", 10.5F);
        _host.ForeColor = SystemColors.WindowText;
        _host.Location = new Point(0, 21);
        _host.Margin = new Padding(0, 1, 12, 5);
        _host.Name = "_host";
        _host.RightToLeft = RightToLeft.No;
        _host.Size = new Size(506, 19);
        _host.TabIndex = 2;
        _host.Text = "127.0.0.1";
        _host.TextAlign = HorizontalAlignment.Center;
        // 
        // _cardTitle
        // 
        _cardTitle.Dock = DockStyle.Fill;
        _cardTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _cardTitle.ForeColor = Color.White;
        _cardTitle.Location = new Point(3, 44);
        _cardTitle.Name = "_cardTitle";
        _cardTitle.Size = new Size(512, 25);
        _cardTitle.TabIndex = 4;
        _cardTitle.Text = "PolarisDisplay Server";
        _cardTitle.TextAlign = ContentAlignment.BottomLeft;
        // 
        // _cardSub
        // 
        _cardSub.Dock = DockStyle.Fill;
        _cardSub.Font = new Font("Segoe UI", 9F);
        _cardSub.ForeColor = Color.FromArgb(155, 200, 235);
        _cardSub.Location = new Point(3, 69);
        _cardSub.Name = "_cardSub";
        _cardSub.Size = new Size(512, 43);
        _cardSub.TabIndex = 5;
        _cardSub.Text = "Sunucu adresini girip bağlanın.";
        // 
        // _connectTitle
        // 
        _connectTitle.Dock = DockStyle.Top;
        _connectTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        _connectTitle.ForeColor = Color.White;
        _connectTitle.Location = new Point(16, 12);
        _connectTitle.Name = "_connectTitle";
        _connectTitle.Padding = new Padding(2, 5, 0, 0);
        _connectTitle.Size = new Size(736, 40);
        _connectTitle.TabIndex = 1;
        _connectTitle.Text = "▣  Sunucu Bağlantısı";
        // 
        // _quickPanel
        // 
        _quickPanel.BackColor = Color.FromArgb(8, 18, 34);
        _quickPanel.BorderColor = Color.FromArgb(0, 115, 225);
        _quickPanel.BorderRadius = 12;
        _quickPanel.BorderThickness = 1;
        _quickPanel.Controls.Add(_quickBody);
        _quickPanel.Dock = DockStyle.Fill;
        _quickPanel.Gradient = true;
        _quickPanel.GradientEnd = Color.FromArgb(20, 15, 45);
        _quickPanel.GradientStart = Color.FromArgb(5, 20, 39);
        _quickPanel.Location = new Point(3, 193);
        _quickPanel.Name = "_quickPanel";
        _quickPanel.Padding = new Padding(14);
        _quickPanel.Size = new Size(762, 289);
        _quickPanel.TabIndex = 1;
        // 
        // _quickBody
        // 
        _quickBody.BackColor = Color.Transparent;
        _quickBody.Controls.Add(_favoritesListPanel);
        _quickBody.Controls.Add(_recentListPanel);
        _quickBody.Controls.Add(_quickCommands);
        _quickBody.Dock = DockStyle.Fill;
        _quickBody.Location = new Point(14, 14);
        _quickBody.Name = "_quickBody";
        _quickBody.Size = new Size(734, 261);
        _quickBody.TabIndex = 0;
        // 
        // _favoritesListPanel
        // 
        _favoritesListPanel.AutoScroll = true;
        _favoritesListPanel.BackColor = Color.Transparent;
        _favoritesListPanel.Dock = DockStyle.Fill;
        _favoritesListPanel.Location = new Point(0, 48);
        _favoritesListPanel.Name = "_favoritesListPanel";
        _favoritesListPanel.Size = new Size(734, 213);
        _favoritesListPanel.TabIndex = 2;
        _favoritesListPanel.Visible = false;
        // 
        // _recentListPanel
        // 
        _recentListPanel.AutoScroll = true;
        _recentListPanel.BackColor = Color.Transparent;
        _recentListPanel.Dock = DockStyle.Fill;
        _recentListPanel.Location = new Point(0, 48);
        _recentListPanel.Name = "_recentListPanel";
        _recentListPanel.Size = new Size(734, 213);
        _recentListPanel.TabIndex = 1;
        _recentListPanel.Visible = false;
        // 
        // _quickCommands
        // 
        _quickCommands.ColumnCount = 3;
        _quickCommands.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        _quickCommands.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        _quickCommands.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
        _quickCommands.Controls.Add(_quickRecent, 0, 0);
        _quickCommands.Controls.Add(_quickFavorites, 1, 0);
        _quickCommands.Controls.Add(_quickClear, 2, 0);
        _quickCommands.Dock = DockStyle.Top;
        _quickCommands.Location = new Point(0, 0);
        _quickCommands.Name = "_quickCommands";
        _quickCommands.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        _quickCommands.Size = new Size(734, 48);
        _quickCommands.TabIndex = 0;
        // 
        // _quickRecent
        // 
        _quickRecent.BackColor = Color.FromArgb(8, 24, 43);
        _quickRecent.Dock = DockStyle.Fill;
        _quickRecent.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 220);
        _quickRecent.FlatStyle = FlatStyle.Flat;
        _quickRecent.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        _quickRecent.ForeColor = Color.FromArgb(200, 230, 255);
        _quickRecent.Location = new Point(0, 0);
        _quickRecent.Margin = new Padding(0, 0, 5, 0);
        _quickRecent.Name = "_quickRecent";
        _quickRecent.Size = new Size(239, 48);
        _quickRecent.TabIndex = 0;
        _quickRecent.Text = "Son Bağlantılar";
        _quickRecent.UseVisualStyleBackColor = false;
        // 
        // _quickFavorites
        // 
        _quickFavorites.BackColor = Color.FromArgb(8, 24, 43);
        _quickFavorites.Dock = DockStyle.Fill;
        _quickFavorites.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 220);
        _quickFavorites.FlatStyle = FlatStyle.Flat;
        _quickFavorites.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        _quickFavorites.ForeColor = Color.FromArgb(200, 230, 255);
        _quickFavorites.Location = new Point(249, 0);
        _quickFavorites.Margin = new Padding(5, 0, 5, 0);
        _quickFavorites.Name = "_quickFavorites";
        _quickFavorites.Size = new Size(234, 48);
        _quickFavorites.TabIndex = 1;
        _quickFavorites.Text = "Favoriler";
        _quickFavorites.UseVisualStyleBackColor = false;
        // 
        // _quickClear
        // 
        _quickClear.BackColor = Color.FromArgb(8, 24, 43);
        _quickClear.Dock = DockStyle.Fill;
        _quickClear.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 220);
        _quickClear.FlatStyle = FlatStyle.Flat;
        _quickClear.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        _quickClear.ForeColor = Color.FromArgb(200, 230, 255);
        _quickClear.Location = new Point(493, 0);
        _quickClear.Margin = new Padding(5, 0, 0, 0);
        _quickClear.Name = "_quickClear";
        _quickClear.Size = new Size(241, 48);
        _quickClear.TabIndex = 2;
        _quickClear.Text = "Temizle";
        _quickClear.UseVisualStyleBackColor = false;
        // 
        // ConnectionPage
        // 
        BackColor = Color.FromArgb(3, 8, 18);
        Controls.Add(_rootLayout);
        Name = "ConnectionPage";
        Padding = new Padding(14);
        Size = new Size(796, 513);
        _rootLayout.ResumeLayout(false);
        _connectPanel.ResumeLayout(false);
        _connectionBody.ResumeLayout(false);
        _connectionGrid.ResumeLayout(false);
        _connectionGrid.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_port).EndInit();
        _quickPanel.ResumeLayout(false);
        _quickBody.ResumeLayout(false);
        _quickCommands.ResumeLayout(false);
        ResumeLayout(false);
    }
}
