#nullable disable
namespace PolarisDisplay.Server;

partial class BroadcastPage
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel _rootLayout;
    private NeonPanel _settingsCard;
    private Label _settingsTitle;
    private Panel _settingsBody;
    private TableLayoutPanel _settingsGrid;
    private Panel _portBox, _fpsBox, _qualityBox, _resolutionBox, _bandBox, _mbpsBox, _screenBox;
    private Label _portLabel, _fpsLabel, _qualityLabel, _resolutionLabel, _bandLabel, _mbpsLabel, _screenLabel;
    private NumericUpDown _port, _fps, _quality, _bitrate;
    private NeonComboBox _resolution, _bitrateMode, _screen;
    private Button _refresh;
    private Label _status;
    private NeonButton _start;
    private NeonPanel _statsPanel;
    private Label _statsTitle;
    private TableLayoutPanel _statsGrid;
    private Label _m1Title, _m1Value, _m2Title, _m2Value, _m3Title, _m3Value, _m4Title, _m4Value, _m5Title, _m5Value;
    private NeonPanel _screenOverview, _screen1, _screen2, _screen3;
    private TableLayoutPanel _screenGrid;
    private Label _screenOverviewTitle, _screen1Title, _screen1Sub, _screen2Title, _screen2Sub, _screen3Title, _screen3Sub;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _rootLayout = new TableLayoutPanel();
        _screenOverview = new NeonPanel();
        _screenGrid = new TableLayoutPanel();
        _screen1 = new NeonPanel();
        _screen1Sub = new Label();
        _screen1Title = new Label();
        _screen2 = new NeonPanel();
        _screen2Sub = new Label();
        _screen2Title = new Label();
        _screen3 = new NeonPanel();
        _screen3Sub = new Label();
        _screen3Title = new Label();
        _screenOverviewTitle = new Label();
        _settingsCard = new NeonPanel();
        _settingsBody = new Panel();
        _settingsGrid = new TableLayoutPanel();
        _portBox = new Panel();
        _port = new NumericUpDown();
        _portLabel = new Label();
        _fpsBox = new Panel();
        _fps = new NumericUpDown();
        _fpsLabel = new Label();
        _qualityBox = new Panel();
        _quality = new NumericUpDown();
        _qualityLabel = new Label();
        _resolutionBox = new Panel();
        _resolution = new NeonComboBox();
        _resolutionLabel = new Label();
        _bandBox = new Panel();
        _bitrateMode = new NeonComboBox();
        _bandLabel = new Label();
        _mbpsBox = new Panel();
        _bitrate = new NumericUpDown();
        _mbpsLabel = new Label();
        _screenBox = new Panel();
        _screen = new NeonComboBox();
        _screenLabel = new Label();
        _refresh = new Button();
        _status = new Label();
        _start = new NeonButton();
        _settingsTitle = new Label();
        _statsPanel = new NeonPanel();
        _statsGrid = new TableLayoutPanel();
        _m1Title = new Label();
        _m2Title = new Label();
        _m3Title = new Label();
        _m4Title = new Label();
        _m5Title = new Label();
        _m1Value = new Label();
        _m2Value = new Label();
        _m3Value = new Label();
        _m4Value = new Label();
        _m5Value = new Label();
        _statsTitle = new Label();
        _rootLayout.SuspendLayout();
        _screenOverview.SuspendLayout();
        _screenGrid.SuspendLayout();
        _screen1.SuspendLayout();
        _screen2.SuspendLayout();
        _screen3.SuspendLayout();
        _settingsCard.SuspendLayout();
        _settingsBody.SuspendLayout();
        _settingsGrid.SuspendLayout();
        _portBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_port).BeginInit();
        _fpsBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_fps).BeginInit();
        _qualityBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_quality).BeginInit();
        _resolutionBox.SuspendLayout();
        _bandBox.SuspendLayout();
        _mbpsBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_bitrate).BeginInit();
        _screenBox.SuspendLayout();
        _statsPanel.SuspendLayout();
        _statsGrid.SuspendLayout();
        SuspendLayout();
        // 
        // _rootLayout
        // 
        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(_screenOverview, 0, 0);
        _rootLayout.Controls.Add(_settingsCard, 0, 1);
        _rootLayout.Controls.Add(_statsPanel, 0, 2);
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.Location = new Point(14, 14);
        _rootLayout.Name = "_rootLayout";
        _rootLayout.RowCount = 3;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 294F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
        _rootLayout.Size = new Size(985, 520);
        _rootLayout.TabIndex = 0;
        // 
        // _screenOverview
        // 
        _screenOverview.BackColor = Color.FromArgb(7, 18, 31);
        _screenOverview.BorderColor = Color.FromArgb(31, 158, 225);
        _screenOverview.BorderRadius = 10;
        _screenOverview.BorderThickness = 1;
        _screenOverview.Controls.Add(_screenGrid);
        _screenOverview.Controls.Add(_screenOverviewTitle);
        _screenOverview.Dock = DockStyle.Fill;
        _screenOverview.Gradient = true;
        _screenOverview.GradientEnd = Color.FromArgb(11, 14, 28);
        _screenOverview.GradientStart = Color.FromArgb(7, 25, 45);
        _screenOverview.Location = new Point(3, 3);
        _screenOverview.Name = "_screenOverview";
        _screenOverview.Padding = new Padding(12, 8, 12, 8);
        _screenOverview.Size = new Size(979, 102);
        _screenOverview.TabIndex = 0;
        // 
        // _screenGrid
        // 
        _screenGrid.ColumnCount = 3;
        _screenGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        _screenGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        _screenGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
        _screenGrid.Controls.Add(_screen1, 0, 0);
        _screenGrid.Controls.Add(_screen2, 1, 0);
        _screenGrid.Controls.Add(_screen3, 2, 0);
        _screenGrid.Dock = DockStyle.Fill;
        _screenGrid.Location = new Point(12, 30);
        _screenGrid.Name = "_screenGrid";
        _screenGrid.RowCount = 1;
        _screenGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _screenGrid.Size = new Size(955, 64);
        _screenGrid.TabIndex = 0;
        // 
        // _screen1
        // 
        _screen1.BackColor = Color.FromArgb(6, 20, 34);
        _screen1.BorderColor = Color.FromArgb(31, 158, 225);
        _screen1.BorderRadius = 10;
        _screen1.BorderThickness = 1;
        _screen1.Controls.Add(_screen1Sub);
        _screen1.Controls.Add(_screen1Title);
        _screen1.Dock = DockStyle.Fill;
        _screen1.Gradient = true;
        _screen1.GradientEnd = Color.FromArgb(8, 17, 29);
        _screen1.GradientStart = Color.FromArgb(7, 28, 48);
        _screen1.Location = new Point(4, 4);
        _screen1.Margin = new Padding(4);
        _screen1.Name = "_screen1";
        _screen1.Padding = new Padding(10, 6, 10, 6);
        _screen1.Size = new Size(310, 56);
        _screen1.TabIndex = 0;
        // 
        // _screen1Sub
        // 
        _screen1Sub.Dock = DockStyle.Fill;
        _screen1Sub.Font = new Font("Segoe UI", 8.5F);
        _screen1Sub.ForeColor = Color.FromArgb(181, 211, 228);
        _screen1Sub.Location = new Point(10, 30);
        _screen1Sub.Name = "_screen1Sub";
        _screen1Sub.Size = new Size(290, 20);
        _screen1Sub.TabIndex = 0;
        _screen1Sub.Text = "1920 × 1080   •   60 Hz";
        _screen1Sub.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _screen1Title
        // 
        _screen1Title.Dock = DockStyle.Top;
        _screen1Title.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        _screen1Title.ForeColor = Color.FromArgb(91, 222, 255);
        _screen1Title.Location = new Point(10, 6);
        _screen1Title.Name = "_screen1Title";
        _screen1Title.Size = new Size(290, 24);
        _screen1Title.TabIndex = 1;
        _screen1Title.Text = "POLARIS-1";
        _screen1Title.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _screen2
        // 
        _screen2.BackColor = Color.FromArgb(6, 20, 34);
        _screen2.BorderColor = Color.FromArgb(31, 158, 225);
        _screen2.BorderRadius = 10;
        _screen2.BorderThickness = 1;
        _screen2.Controls.Add(_screen2Sub);
        _screen2.Controls.Add(_screen2Title);
        _screen2.Dock = DockStyle.Fill;
        _screen2.Gradient = true;
        _screen2.GradientEnd = Color.FromArgb(8, 17, 29);
        _screen2.GradientStart = Color.FromArgb(7, 28, 48);
        _screen2.Location = new Point(322, 4);
        _screen2.Margin = new Padding(4);
        _screen2.Name = "_screen2";
        _screen2.Padding = new Padding(10, 6, 10, 6);
        _screen2.Size = new Size(310, 56);
        _screen2.TabIndex = 1;
        // 
        // _screen2Sub
        // 
        _screen2Sub.Dock = DockStyle.Fill;
        _screen2Sub.Font = new Font("Segoe UI", 8.5F);
        _screen2Sub.ForeColor = Color.FromArgb(181, 211, 228);
        _screen2Sub.Location = new Point(10, 30);
        _screen2Sub.Name = "_screen2Sub";
        _screen2Sub.Size = new Size(290, 20);
        _screen2Sub.TabIndex = 0;
        _screen2Sub.Text = "1920 × 1080   •   60 Hz";
        _screen2Sub.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _screen2Title
        // 
        _screen2Title.Dock = DockStyle.Top;
        _screen2Title.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        _screen2Title.ForeColor = Color.FromArgb(91, 222, 255);
        _screen2Title.Location = new Point(10, 6);
        _screen2Title.Name = "_screen2Title";
        _screen2Title.Size = new Size(290, 24);
        _screen2Title.TabIndex = 1;
        _screen2Title.Text = "POLARIS-2";
        _screen2Title.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _screen3
        // 
        _screen3.BackColor = Color.FromArgb(6, 20, 34);
        _screen3.BorderColor = Color.FromArgb(31, 158, 225);
        _screen3.BorderRadius = 10;
        _screen3.BorderThickness = 1;
        _screen3.Controls.Add(_screen3Sub);
        _screen3.Controls.Add(_screen3Title);
        _screen3.Dock = DockStyle.Fill;
        _screen3.Gradient = true;
        _screen3.GradientEnd = Color.FromArgb(8, 17, 29);
        _screen3.GradientStart = Color.FromArgb(7, 28, 48);
        _screen3.Location = new Point(640, 4);
        _screen3.Margin = new Padding(4);
        _screen3.Name = "_screen3";
        _screen3.Padding = new Padding(10, 6, 10, 6);
        _screen3.Size = new Size(311, 56);
        _screen3.TabIndex = 2;
        // 
        // _screen3Sub
        // 
        _screen3Sub.Dock = DockStyle.Fill;
        _screen3Sub.Font = new Font("Segoe UI", 8.5F);
        _screen3Sub.ForeColor = Color.FromArgb(181, 211, 228);
        _screen3Sub.Location = new Point(10, 30);
        _screen3Sub.Name = "_screen3Sub";
        _screen3Sub.Size = new Size(291, 20);
        _screen3Sub.TabIndex = 0;
        _screen3Sub.Text = "1920 × 1080   •   60 Hz";
        _screen3Sub.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _screen3Title
        // 
        _screen3Title.Dock = DockStyle.Top;
        _screen3Title.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        _screen3Title.ForeColor = Color.FromArgb(91, 222, 255);
        _screen3Title.Location = new Point(10, 6);
        _screen3Title.Name = "_screen3Title";
        _screen3Title.Size = new Size(291, 24);
        _screen3Title.TabIndex = 1;
        _screen3Title.Text = "POLARIS-3";
        _screen3Title.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _screenOverviewTitle
        // 
        _screenOverviewTitle.Dock = DockStyle.Top;
        _screenOverviewTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _screenOverviewTitle.ForeColor = Color.White;
        _screenOverviewTitle.Location = new Point(12, 8);
        _screenOverviewTitle.Name = "_screenOverviewTitle";
        _screenOverviewTitle.Size = new Size(955, 22);
        _screenOverviewTitle.TabIndex = 1;
        _screenOverviewTitle.Text = "SANAL EKRANLAR";
        _screenOverviewTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _settingsCard
        // 
        _settingsCard.BackColor = Color.FromArgb(8, 18, 34);
        _settingsCard.BorderColor = Color.FromArgb(0, 145, 240);
        _settingsCard.BorderRadius = 12;
        _settingsCard.BorderThickness = 1;
        _settingsCard.Controls.Add(_settingsBody);
        _settingsCard.Controls.Add(_settingsTitle);
        _settingsCard.Dock = DockStyle.Fill;
        _settingsCard.Gradient = true;
        _settingsCard.GradientEnd = Color.FromArgb(25, 14, 54);
        _settingsCard.GradientStart = Color.FromArgb(7, 25, 49);
        _settingsCard.Location = new Point(0, 104);
        _settingsCard.Margin = new Padding(0, 0, 0, 10);
        _settingsCard.Name = "_settingsCard";
        _settingsCard.Padding = new Padding(14, 10, 14, 10);
        _settingsCard.Size = new Size(985, 294);
        _settingsCard.TabIndex = 0;
        // 
        // _settingsBody
        // 
        _settingsBody.BackColor = Color.Transparent;
        _settingsBody.Controls.Add(_settingsGrid);
        _settingsBody.Controls.Add(_status);
        _settingsBody.Controls.Add(_start);
        _settingsBody.Dock = DockStyle.Fill;
        _settingsBody.Location = new Point(14, 44);
        _settingsBody.Name = "_settingsBody";
        _settingsBody.Size = new Size(957, 240);
        _settingsBody.TabIndex = 0;
        // 
        // _settingsGrid
        // 
        _settingsGrid.BackColor = Color.Transparent;
        _settingsGrid.ColumnCount = 4;
        _settingsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        _settingsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        _settingsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        _settingsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        _settingsGrid.Controls.Add(_portBox, 0, 0);
        _settingsGrid.Controls.Add(_fpsBox, 1, 0);
        _settingsGrid.Controls.Add(_qualityBox, 2, 0);
        _settingsGrid.Controls.Add(_resolutionBox, 3, 0);
        _settingsGrid.Controls.Add(_bandBox, 0, 1);
        _settingsGrid.Controls.Add(_mbpsBox, 1, 1);
        _settingsGrid.Controls.Add(_screenBox, 2, 1);
        _settingsGrid.Dock = DockStyle.Top;
        _settingsGrid.Location = new Point(0, 0);
        _settingsGrid.Name = "_settingsGrid";
        _settingsGrid.RowCount = 2;
        _settingsGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
        _settingsGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _settingsGrid.Size = new Size(957, 145);
        _settingsGrid.TabIndex = 0;
        // 
        // _portBox
        // 
        _portBox.BackColor = Color.Transparent;
        _portBox.Controls.Add(_port);
        _portBox.Controls.Add(_portLabel);
        _portBox.Dock = DockStyle.Fill;
        _portBox.Location = new Point(3, 3);
        _portBox.Name = "_portBox";
        _portBox.Padding = new Padding(3, 2, 6, 2);
        _portBox.Size = new Size(233, 70);
        _portBox.TabIndex = 0;
        // 
        // _port
        // 
        _port.BackColor = Color.FromArgb(7, 18, 31);
        _port.Dock = DockStyle.Fill;
        _port.Font = new Font("Segoe UI", 10F);
        _port.ForeColor = Color.White;
        _port.Location = new Point(3, 22);
        _port.Margin = new Padding(0);
        _port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        _port.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
        _port.Name = "_port";
        _port.Size = new Size(224, 25);
        _port.TabIndex = 0;
        _port.Value = new decimal(new int[] { 50505, 0, 0, 0 });
        // 
        // _portLabel
        // 
        _portLabel.Dock = DockStyle.Top;
        _portLabel.Font = new Font("Segoe UI", 9F);
        _portLabel.ForeColor = Color.FromArgb(175, 215, 240);
        _portLabel.Location = new Point(3, 2);
        _portLabel.Name = "_portLabel";
        _portLabel.Size = new Size(224, 20);
        _portLabel.TabIndex = 1;
        _portLabel.Text = "Port (Client)";
        // 
        // _fpsBox
        // 
        _fpsBox.BackColor = Color.Transparent;
        _fpsBox.Controls.Add(_fps);
        _fpsBox.Controls.Add(_fpsLabel);
        _fpsBox.Dock = DockStyle.Fill;
        _fpsBox.Location = new Point(242, 3);
        _fpsBox.Name = "_fpsBox";
        _fpsBox.Padding = new Padding(3, 2, 6, 2);
        _fpsBox.Size = new Size(233, 70);
        _fpsBox.TabIndex = 1;
        // 
        // _fps
        // 
        _fps.BackColor = Color.FromArgb(7, 18, 31);
        _fps.Dock = DockStyle.Fill;
        _fps.Font = new Font("Segoe UI", 10F);
        _fps.ForeColor = Color.White;
        _fps.Location = new Point(3, 22);
        _fps.Margin = new Padding(0);
        _fps.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        _fps.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
        _fps.Name = "_fps";
        _fps.Size = new Size(224, 25);
        _fps.TabIndex = 0;
        _fps.Value = new decimal(new int[] { 60, 0, 0, 0 });
        // 
        // _fpsLabel
        // 
        _fpsLabel.Dock = DockStyle.Top;
        _fpsLabel.Font = new Font("Segoe UI", 9F);
        _fpsLabel.ForeColor = Color.FromArgb(175, 215, 240);
        _fpsLabel.Location = new Point(3, 2);
        _fpsLabel.Name = "_fpsLabel";
        _fpsLabel.Size = new Size(224, 20);
        _fpsLabel.TabIndex = 1;
        _fpsLabel.Text = "▣  FPS";
        // 
        // _qualityBox
        // 
        _qualityBox.BackColor = Color.Transparent;
        _qualityBox.Controls.Add(_quality);
        _qualityBox.Controls.Add(_qualityLabel);
        _qualityBox.Dock = DockStyle.Fill;
        _qualityBox.Location = new Point(481, 3);
        _qualityBox.Name = "_qualityBox";
        _qualityBox.Padding = new Padding(3, 2, 6, 2);
        _qualityBox.Size = new Size(233, 70);
        _qualityBox.TabIndex = 2;
        // 
        // _quality
        // 
        _quality.BackColor = Color.FromArgb(7, 18, 31);
        _quality.Dock = DockStyle.Fill;
        _quality.Font = new Font("Segoe UI", 10F);
        _quality.ForeColor = Color.White;
        _quality.Location = new Point(3, 22);
        _quality.Margin = new Padding(0);
        _quality.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
        _quality.Name = "_quality";
        _quality.Size = new Size(224, 25);
        _quality.TabIndex = 0;
        _quality.Value = new decimal(new int[] { 85, 0, 0, 0 });
        // 
        // _qualityLabel
        // 
        _qualityLabel.Dock = DockStyle.Top;
        _qualityLabel.Font = new Font("Segoe UI", 9F);
        _qualityLabel.ForeColor = Color.FromArgb(175, 215, 240);
        _qualityLabel.Location = new Point(3, 2);
        _qualityLabel.Name = "_qualityLabel";
        _qualityLabel.Size = new Size(224, 20);
        _qualityLabel.TabIndex = 1;
        _qualityLabel.Text = "▣  Kalite";
        // 
        // _resolutionBox
        // 
        _resolutionBox.BackColor = Color.Transparent;
        _resolutionBox.Controls.Add(_resolution);
        _resolutionBox.Controls.Add(_resolutionLabel);
        _resolutionBox.Dock = DockStyle.Fill;
        _resolutionBox.Location = new Point(720, 3);
        _resolutionBox.Name = "_resolutionBox";
        _resolutionBox.Padding = new Padding(3, 2, 6, 2);
        _resolutionBox.Size = new Size(234, 70);
        _resolutionBox.TabIndex = 3;
        // 
        // _resolution
        // 
        _resolution.BackColor = Color.FromArgb(7, 18, 31);
        _resolution.Dock = DockStyle.Fill;
        _resolution.DrawMode = DrawMode.OwnerDrawFixed;
        _resolution.DropDownHeight = 260;
        _resolution.DropDownStyle = ComboBoxStyle.DropDownList;
        _resolution.FlatStyle = FlatStyle.Flat;
        _resolution.Font = new Font("Segoe UI", 10F);
        _resolution.ForeColor = Color.White;
        _resolution.IntegralHeight = false;
        _resolution.ItemHeight = 28;
        _resolution.Items.AddRange(new object[] { "1920×1080 (FHD)", "2560×1440 (QHD)", "3840×2160 (4K)", "1600×900", "1366×768", "1280×720" });
        _resolution.Location = new Point(3, 22);
        _resolution.Margin = new Padding(0);
        _resolution.Name = "_resolution";
        _resolution.Size = new Size(225, 34);
        _resolution.TabIndex = 0;
        // 
        // _resolutionLabel
        // 
        _resolutionLabel.Dock = DockStyle.Top;
        _resolutionLabel.Font = new Font("Segoe UI", 9F);
        _resolutionLabel.ForeColor = Color.FromArgb(175, 215, 240);
        _resolutionLabel.Location = new Point(3, 2);
        _resolutionLabel.Name = "_resolutionLabel";
        _resolutionLabel.Size = new Size(225, 20);
        _resolutionLabel.TabIndex = 1;
        _resolutionLabel.Text = "▣  Çözünürlük";
        // 
        // _bandBox
        // 
        _bandBox.BackColor = Color.Transparent;
        _bandBox.Controls.Add(_bitrateMode);
        _bandBox.Controls.Add(_bandLabel);
        _bandBox.Dock = DockStyle.Fill;
        _bandBox.Location = new Point(3, 79);
        _bandBox.Name = "_bandBox";
        _bandBox.Padding = new Padding(3, 2, 6, 2);
        _bandBox.Size = new Size(233, 63);
        _bandBox.TabIndex = 4;
        // 
        // _bitrateMode
        // 
        _bitrateMode.BackColor = Color.FromArgb(7, 18, 31);
        _bitrateMode.Dock = DockStyle.Fill;
        _bitrateMode.DrawMode = DrawMode.OwnerDrawFixed;
        _bitrateMode.DropDownHeight = 260;
        _bitrateMode.DropDownStyle = ComboBoxStyle.DropDownList;
        _bitrateMode.FlatStyle = FlatStyle.Flat;
        _bitrateMode.Font = new Font("Segoe UI", 10F);
        _bitrateMode.ForeColor = Color.White;
        _bitrateMode.IntegralHeight = false;
        _bitrateMode.ItemHeight = 28;
        _bitrateMode.Items.AddRange(new object[] { "Otomatik", "Manuel" });
        _bitrateMode.Location = new Point(3, 22);
        _bitrateMode.Margin = new Padding(0);
        _bitrateMode.Name = "_bitrateMode";
        _bitrateMode.Size = new Size(224, 34);
        _bitrateMode.TabIndex = 0;
        // 
        // _bandLabel
        // 
        _bandLabel.Dock = DockStyle.Top;
        _bandLabel.Font = new Font("Segoe UI", 9F);
        _bandLabel.ForeColor = Color.FromArgb(175, 215, 240);
        _bandLabel.Location = new Point(3, 2);
        _bandLabel.Name = "_bandLabel";
        _bandLabel.Size = new Size(224, 20);
        _bandLabel.TabIndex = 1;
        _bandLabel.Text = "▦  Bant";
        // 
        // _mbpsBox
        // 
        _mbpsBox.BackColor = Color.Transparent;
        _mbpsBox.Controls.Add(_bitrate);
        _mbpsBox.Controls.Add(_mbpsLabel);
        _mbpsBox.Dock = DockStyle.Fill;
        _mbpsBox.Location = new Point(242, 79);
        _mbpsBox.Name = "_mbpsBox";
        _mbpsBox.Padding = new Padding(3, 2, 6, 2);
        _mbpsBox.Size = new Size(233, 63);
        _mbpsBox.TabIndex = 5;
        // 
        // _bitrate
        // 
        _bitrate.BackColor = Color.FromArgb(7, 18, 31);
        _bitrate.Dock = DockStyle.Fill;
        _bitrate.Enabled = false;
        _bitrate.Font = new Font("Segoe UI", 10F);
        _bitrate.ForeColor = Color.White;
        _bitrate.Location = new Point(3, 22);
        _bitrate.Margin = new Padding(0);
        _bitrate.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
        _bitrate.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
        _bitrate.Name = "_bitrate";
        _bitrate.Size = new Size(224, 25);
        _bitrate.TabIndex = 0;
        _bitrate.Value = new decimal(new int[] { 50, 0, 0, 0 });
        // 
        // _mbpsLabel
        // 
        _mbpsLabel.Dock = DockStyle.Top;
        _mbpsLabel.Font = new Font("Segoe UI", 9F);
        _mbpsLabel.ForeColor = Color.FromArgb(175, 215, 240);
        _mbpsLabel.Location = new Point(3, 2);
        _mbpsLabel.Name = "_mbpsLabel";
        _mbpsLabel.Size = new Size(224, 20);
        _mbpsLabel.TabIndex = 1;
        _mbpsLabel.Text = "ϟ  Mbps";
        // 
        // _screenBox
        // 
        _screenBox.BackColor = Color.Transparent;
        _screenBox.Controls.Add(_screen);
        _screenBox.Controls.Add(_screenLabel);
        _screenBox.Controls.Add(_refresh);
        _screenBox.Dock = DockStyle.Fill;
        _screenBox.Location = new Point(481, 79);
        _screenBox.Name = "_screenBox";
        _screenBox.Padding = new Padding(3, 2, 42, 2);
        _screenBox.Size = new Size(233, 63);
        _screenBox.TabIndex = 6;
        // 
        // _screen
        // 
        _screen.BackColor = Color.FromArgb(7, 18, 31);
        _screen.Dock = DockStyle.Fill;
        _screen.DrawMode = DrawMode.OwnerDrawFixed;
        _screen.DropDownHeight = 260;
        _screen.DropDownStyle = ComboBoxStyle.DropDownList;
        _screen.DropDownWidth = 320;
        _screen.FlatStyle = FlatStyle.Flat;
        _screen.Font = new Font("Segoe UI", 10F);
        _screen.ForeColor = Color.White;
        _screen.IntegralHeight = false;
        _screen.ItemHeight = 28;
        _screen.Items.AddRange(new object[] { "Ekran 1" });
        _screen.Location = new Point(3, 22);
        _screen.Margin = new Padding(0);
        _screen.Name = "_screen";
        _screen.Size = new Size(188, 34);
        _screen.TabIndex = 0;
        // 
        // _screenLabel
        // 
        _screenLabel.Dock = DockStyle.Top;
        _screenLabel.Font = new Font("Segoe UI", 9F);
        _screenLabel.ForeColor = Color.FromArgb(175, 215, 240);
        _screenLabel.Location = new Point(3, 2);
        _screenLabel.Name = "_screenLabel";
        _screenLabel.Size = new Size(190, 20);
        _screenLabel.TabIndex = 1;
        _screenLabel.Text = "▣  Ekran";
        // 
        // _refresh
        // 
        _refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _refresh.BackColor = Color.Transparent;
        _refresh.FlatAppearance.BorderSize = 0;
        _refresh.FlatStyle = FlatStyle.Flat;
        _refresh.Font = new Font("Segoe UI", 13F);
        _refresh.ForeColor = Color.FromArgb(100, 220, 255);
        _refresh.Location = new Point(192, 20);
        _refresh.Name = "_refresh";
        _refresh.Size = new Size(34, 30);
        _refresh.TabIndex = 2;
        _refresh.Text = "⟳";
        _refresh.UseVisualStyleBackColor = false;
        // 
        // _status
        // 
        _status.Dock = DockStyle.Bottom;
        _status.Font = new Font("Segoe UI", 9.5F);
        _status.ForeColor = Color.FromArgb(45, 238, 130);
        _status.Location = new Point(0, 156);
        _status.Name = "_status";
        _status.Size = new Size(957, 32);
        _status.TabIndex = 1;
        _status.Text = "●  Hazır — Yayın bekleniyor...";
        _status.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _start
        // 
        _start.BorderRadius = 9;
        _start.Dock = DockStyle.Bottom;
        _start.FlatStyle = FlatStyle.Flat;
        _start.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        _start.ForeColor = Color.White;
        _start.GradientEnd = Color.FromArgb(30, 78, 190);
        _start.GradientStart = Color.FromArgb(9, 132, 220);
        _start.HoverEnd = Color.FromArgb(75, 104, 230);
        _start.HoverStart = Color.FromArgb(35, 181, 255);
        _start.Location = new Point(0, 202);
        _start.Name = "_start";
        _start.Size = new Size(957, 38);
        _start.TabIndex = 2;
        _start.Text = "◉   YAYINI BAŞLAT";
        _start.UseBackColorFill = false;
        // 
        // _settingsTitle
        // 
        _settingsTitle.Dock = DockStyle.Top;
        _settingsTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _settingsTitle.ForeColor = Color.White;
        _settingsTitle.Location = new Point(14, 10);
        _settingsTitle.Name = "_settingsTitle";
        _settingsTitle.Padding = new Padding(2, 5, 0, 0);
        _settingsTitle.Size = new Size(957, 34);
        _settingsTitle.TabIndex = 1;
        _settingsTitle.Text = "⚙  Yayın Ayarları   •   WEB PORT: 8765";
        // 
        // _statsPanel
        // 
        _statsPanel.BackColor = Color.FromArgb(8, 18, 34);
        _statsPanel.BorderColor = Color.FromArgb(0, 125, 235);
        _statsPanel.BorderRadius = 12;
        _statsPanel.BorderThickness = 1;
        _statsPanel.Controls.Add(_statsGrid);
        _statsPanel.Controls.Add(_statsTitle);
        _statsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        _statsPanel.Gradient = true;
        _statsPanel.GradientEnd = Color.FromArgb(19, 17, 45);
        _statsPanel.GradientStart = Color.FromArgb(6, 24, 43);
        _statsPanel.Location = new Point(3, 398);
        _statsPanel.Name = "_statsPanel";
        _statsPanel.Padding = new Padding(12, 7, 12, 6);
        _statsPanel.Size = new Size(760, 82);
        _statsPanel.TabIndex = 1;
        // 
        // _statsGrid
        // 
        _statsGrid.BackColor = Color.Transparent;
        _statsGrid.ColumnCount = 5;
        _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        _statsGrid.Controls.Add(_m1Title, 0, 0);
        _statsGrid.Controls.Add(_m2Title, 1, 0);
        _statsGrid.Controls.Add(_m3Title, 2, 0);
        _statsGrid.Controls.Add(_m4Title, 3, 0);
        _statsGrid.Controls.Add(_m5Title, 4, 0);
        _statsGrid.Controls.Add(_m1Value, 0, 1);
        _statsGrid.Controls.Add(_m2Value, 1, 1);
        _statsGrid.Controls.Add(_m3Value, 2, 1);
        _statsGrid.Controls.Add(_m4Value, 3, 1);
        _statsGrid.Controls.Add(_m5Value, 4, 1);
        _statsGrid.Dock = DockStyle.Fill;
        _statsGrid.Location = new Point(12, 35);
        _statsGrid.Name = "_statsGrid";
        _statsGrid.RowCount = 2;
        _statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        _statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        _statsGrid.Size = new Size(736, 41);
        _statsGrid.TabIndex = 0;
        // 
        // _m1Title
        // 
        _m1Title.Dock = DockStyle.Fill;
        _m1Title.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _m1Title.ForeColor = Color.FromArgb(135, 190, 235);
        _m1Title.Location = new Point(3, 0);
        _m1Title.Name = "_m1Title";
        _m1Title.Size = new Size(185, 23);
        _m1Title.TabIndex = 0;
        _m1Title.Text = "Bağlantı";
        _m1Title.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _m2Title
        // 
        _m2Title.Dock = DockStyle.Fill;
        _m2Title.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _m2Title.ForeColor = Color.FromArgb(135, 190, 235);
        _m2Title.Location = new Point(194, 0);
        _m2Title.Name = "_m2Title";
        _m2Title.Size = new Size(185, 23);
        _m2Title.TabIndex = 1;
        _m2Title.Text = "Giden Veri";
        _m2Title.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _m3Title
        // 
        _m3Title.Dock = DockStyle.Fill;
        _m3Title.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _m3Title.ForeColor = Color.FromArgb(135, 190, 235);
        _m3Title.Location = new Point(385, 0);
        _m3Title.Name = "_m3Title";
        _m3Title.Size = new Size(185, 23);
        _m3Title.TabIndex = 2;
        _m3Title.Text = "Süre";
        _m3Title.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _m4Title
        // 
        _m4Title.Dock = DockStyle.Fill;
        _m4Title.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _m4Title.ForeColor = Color.FromArgb(135, 190, 235);
        _m4Title.Location = new Point(576, 0);
        _m4Title.Name = "_m4Title";
        _m4Title.Size = new Size(185, 23);
        _m4Title.TabIndex = 3;
        _m4Title.Text = "CPU";
        _m4Title.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _m5Title
        // 
        _m5Title.Dock = DockStyle.Fill;
        _m5Title.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _m5Title.ForeColor = Color.FromArgb(135, 190, 235);
        _m5Title.Location = new Point(767, 0);
        _m5Title.Name = "_m5Title";
        _m5Title.Size = new Size(185, 23);
        _m5Title.TabIndex = 4;
        _m5Title.Text = "Bellek";
        _m5Title.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _m1Value
        // 
        _m1Value.Dock = DockStyle.Fill;
        _m1Value.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _m1Value.ForeColor = Color.White;
        _m1Value.Location = new Point(3, 23);
        _m1Value.Name = "_m1Value";
        _m1Value.Size = new Size(185, 32);
        _m1Value.TabIndex = 5;
        _m1Value.Text = "0";
        _m1Value.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _m2Value
        // 
        _m2Value.Dock = DockStyle.Fill;
        _m2Value.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _m2Value.ForeColor = Color.White;
        _m2Value.Location = new Point(194, 23);
        _m2Value.Name = "_m2Value";
        _m2Value.Size = new Size(185, 32);
        _m2Value.TabIndex = 6;
        _m2Value.Text = "0 Mbps";
        _m2Value.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _m3Value
        // 
        _m3Value.Dock = DockStyle.Fill;
        _m3Value.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _m3Value.ForeColor = Color.White;
        _m3Value.Location = new Point(385, 23);
        _m3Value.Name = "_m3Value";
        _m3Value.Size = new Size(185, 32);
        _m3Value.TabIndex = 7;
        _m3Value.Text = "00:00:00";
        _m3Value.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _m4Value
        // 
        _m4Value.Dock = DockStyle.Fill;
        _m4Value.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _m4Value.ForeColor = Color.White;
        _m4Value.Location = new Point(576, 23);
        _m4Value.Name = "_m4Value";
        _m4Value.Size = new Size(185, 32);
        _m4Value.TabIndex = 8;
        _m4Value.Text = "0%";
        _m4Value.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _m5Value
        // 
        _m5Value.Dock = DockStyle.Fill;
        _m5Value.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _m5Value.ForeColor = Color.White;
        _m5Value.Location = new Point(767, 23);
        _m5Value.Name = "_m5Value";
        _m5Value.Size = new Size(185, 32);
        _m5Value.TabIndex = 9;
        _m5Value.Text = "0 MB";
        _m5Value.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _statsTitle
        // 
        _statsTitle.Dock = DockStyle.Top;
        _statsTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        _statsTitle.ForeColor = Color.White;
        _statsTitle.Location = new Point(12, 7);
        _statsTitle.Name = "_statsTitle";
        _statsTitle.Size = new Size(736, 25);
        _statsTitle.TabIndex = 1;
        _statsTitle.Text = "▥  İstatistikler";
        // 
        // BroadcastPage
        // 
        AutoScaleMode = AutoScaleMode.Inherit;
        BackColor = Color.FromArgb(3, 8, 18);
        Controls.Add(_rootLayout);
        Name = "BroadcastPage";
        Padding = new Padding(14);
        Size = new Size(1013, 548);
        _rootLayout.ResumeLayout(false);
        _screenOverview.ResumeLayout(false);
        _screenGrid.ResumeLayout(false);
        _screen1.ResumeLayout(false);
        _screen2.ResumeLayout(false);
        _screen3.ResumeLayout(false);
        _settingsCard.ResumeLayout(false);
        _settingsBody.ResumeLayout(false);
        _settingsGrid.ResumeLayout(false);
        _portBox.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_port).EndInit();
        _fpsBox.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_fps).EndInit();
        _qualityBox.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_quality).EndInit();
        _resolutionBox.ResumeLayout(false);
        _bandBox.ResumeLayout(false);
        _mbpsBox.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_bitrate).EndInit();
        _screenBox.ResumeLayout(false);
        _statsPanel.ResumeLayout(false);
        _statsGrid.ResumeLayout(false);
        ResumeLayout(false);
    }

}
