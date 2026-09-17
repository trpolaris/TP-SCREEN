#nullable disable
namespace PolarisDisplay.Server;

partial class ServerForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel _topPanel, _titlePanel, _chromePanel, _navPanel, _contentPanel, _footer;
    private TableLayoutPanel _navLayout;
    private PictureBox _logoBox;
    private Label _lastClientLabel, _footerLeft, _footerRight;
    private Panel _mainHeader;
    private Label _mainTitle, _mainSubtitle, _statusBadge, _sideBrand, _sideCaption;
    private FlowLayoutPanel _socialLinksPanel;
    private PictureBox _facebookIcon, _instagramIcon, _twitterIcon, _githubIcon, _youtubeIcon;
    private ChromeButton _minimizeButton, _maximizeButton, _closeButton;
    private NeonTabButton _tabBroadcast, _tabConnections, _tabStats, _tabAdmin, _tabAbout, _tabLog;
    private StatisticsPage _statisticsPage;
    private AdminPage _adminPage;
    private BroadcastPage _broadcastPage;
    private ConnectionsPage _connectionsPage;
    private AboutPage _aboutPage;
    private LogPage _logPage;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _tabBroadcast = new NeonTabButton();
        _tabConnections = new NeonTabButton();
        _tabStats = new NeonTabButton();
        _tabAdmin = new NeonTabButton();
        _tabAbout = new NeonTabButton();
        _tabLog = new NeonTabButton();
        _statisticsPage = new StatisticsPage();
        _adminPage = new AdminPage();
        _broadcastPage = new BroadcastPage();
        _connectionsPage = new ConnectionsPage();
        _aboutPage = new AboutPage();
        _logPage = new LogPage();
        _topPanel = new Panel();
        _navPanel = new Panel();
        _navLayout = new TableLayoutPanel();
        _titlePanel = new Panel();
        _sideCaption = new Label();
        _sideBrand = new Label();
        _logoBox = new PictureBox();
        _lastClientLabel = new Label();
        _chromePanel = new Panel();
        _minimizeButton = new ChromeButton();
        _maximizeButton = new ChromeButton();
        _closeButton = new ChromeButton();
        _contentPanel = new Panel();
        _footer = new Panel();
        _socialLinksPanel = new FlowLayoutPanel();
        _youtubeIcon = new PictureBox();
        _githubIcon = new PictureBox();
        _twitterIcon = new PictureBox();
        _instagramIcon = new PictureBox();
        _facebookIcon = new PictureBox();
        _footerRight = new Label();
        _footerLeft = new Label();
        _mainHeader = new Panel();
        _statusBadge = new Label();
        _mainSubtitle = new Label();
        _mainTitle = new Label();
        _topPanel.SuspendLayout();
        _navPanel.SuspendLayout();
        _titlePanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_logoBox).BeginInit();
        _footer.SuspendLayout();
        _socialLinksPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_youtubeIcon).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_githubIcon).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_twitterIcon).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_instagramIcon).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_facebookIcon).BeginInit();
        _mainHeader.SuspendLayout();
        SuspendLayout();
        // 
        // _topPanel
        // 
        _topPanel.BackColor = Color.FromArgb(4, 12, 24);
        _topPanel.Controls.Add(_navPanel);
        _topPanel.Controls.Add(_titlePanel);
        _topPanel.Dock = DockStyle.Left;
        _topPanel.Location = new Point(4, 4);
        _topPanel.Name = "_topPanel";
        _topPanel.Padding = new Padding(8);
        _topPanel.Size = new Size(188, 692);
        _topPanel.TabIndex = 2;
        // 
        // _navPanel
        // 
        _navPanel.BackColor = Color.Transparent;
        _navPanel.Controls.Add(_navLayout);
        _navPanel.Dock = DockStyle.Fill;
        _navPanel.Location = new Point(8, 112);
        _navPanel.Name = "_navPanel";
        _navPanel.Padding = new Padding(0, 8, 0, 8);
        _navPanel.Size = new Size(172, 572);
        _navPanel.TabIndex = 1;
        // 
        // _navLayout
        // 
        _navLayout.BackColor = Color.Transparent;
        _navLayout.ColumnCount = 1;
        _navLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _navLayout.Dock = DockStyle.Top;
        _navLayout.Location = new Point(0, 8);
        _navLayout.Name = "_navLayout";
        _navLayout.RowCount = 6;
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        _navLayout.Size = new Size(172, 264);
        _navLayout.TabIndex = 0;
        _navLayout.Controls.Add(_tabBroadcast, 0, 0);
        _navLayout.Controls.Add(_tabConnections, 0, 1);
        _navLayout.Controls.Add(_tabStats, 0, 2);
        _navLayout.Controls.Add(_tabAdmin, 0, 3);
        _navLayout.Controls.Add(_tabAbout, 0, 4);
        _navLayout.Controls.Add(_tabLog, 0, 5);
        // 
        // _tabBroadcast
        // 
        _tabBroadcast.Name = "_tabBroadcast";
        _tabBroadcast.Size = new Size(172, 44);
        _tabBroadcast.Text = "▣  Yayın";
        _tabBroadcast.Selected = true;
        _tabBroadcast.TabIndex = 0;
        // 
        // _tabConnections
        // 
        _tabConnections.Name = "_tabConnections";
        _tabConnections.Size = new Size(172, 44);
        _tabConnections.Text = "♧  Bağlantılar";
        _tabConnections.TabIndex = 1;
        // 
        // _tabStats
        // 
        _tabStats.Name = "_tabStats";
        _tabStats.Size = new Size(172, 44);
        _tabStats.Text = "◌  İstatistikler";
        _tabStats.TabIndex = 2;
        // 
        // _tabAdmin
        // 
        _tabAdmin.Name = "_tabAdmin";
        _tabAdmin.Size = new Size(172, 44);
        _tabAdmin.Text = "⚙  Yönetim";
        _tabAdmin.TabIndex = 3;
        // 
        // _tabAbout
        // 
        _tabAbout.Name = "_tabAbout";
        _tabAbout.Size = new Size(172, 44);
        _tabAbout.Text = "ⓘ  Hakkında";
        _tabAbout.TabIndex = 4;
        // 
        // _tabLog
        // 
        _tabLog.Name = "_tabLog";
        _tabLog.Size = new Size(172, 44);
        _tabLog.Text = "☷  Log";
        _tabLog.TabIndex = 5;
        // 
        // _titlePanel
        // 
        _titlePanel.BackColor = Color.FromArgb(5, 16, 29);
        _titlePanel.Controls.Add(_sideCaption);
        _titlePanel.Controls.Add(_sideBrand);
        _titlePanel.Controls.Add(_logoBox);
        _titlePanel.Dock = DockStyle.Top;
        _titlePanel.Location = new Point(8, 8);
        _titlePanel.Name = "_titlePanel";
        _titlePanel.Size = new Size(172, 104);
        _titlePanel.TabIndex = 0;
        // 
        // _sideCaption
        // 
        _sideCaption.Font = new Font("Segoe UI", 7.5F);
        _sideCaption.ForeColor = Color.FromArgb(73, 204, 255);
        _sideCaption.Location = new Point(4, 74);
        _sideCaption.Name = "_sideCaption";
        _sideCaption.Size = new Size(164, 16);
        _sideCaption.TabIndex = 0;
        _sideCaption.Text = "VIRTUAL DISPLAY SERVER";
        _sideCaption.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _sideBrand
        // 
        _sideBrand.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        _sideBrand.ForeColor = Color.White;
        _sideBrand.Location = new Point(4, 50);
        _sideBrand.Name = "_sideBrand";
        _sideBrand.Size = new Size(164, 24);
        _sideBrand.TabIndex = 1;
        _sideBrand.Text = "TRPOLARIS";
        _sideBrand.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _logoBox
        // 
        _logoBox.BackColor = Color.Transparent;
        _logoBox.Image = Properties.Resources.Logo;
        _logoBox.Location = new Point(21, 4);
        _logoBox.Name = "_logoBox";
        _logoBox.Size = new Size(130, 44);
        _logoBox.SizeMode = PictureBoxSizeMode.Zoom;
        _logoBox.TabIndex = 2;
        _logoBox.TabStop = false;
        // 
        // _lastClientLabel
        // 
        _lastClientLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _lastClientLabel.Font = new Font("Segoe UI", 8F);
        _lastClientLabel.ForeColor = Color.FromArgb(111, 151, 177);
        _lastClientLabel.Location = new Point(252, 0);
        _lastClientLabel.Name = "_lastClientLabel";
        _lastClientLabel.Size = new Size(336, 34);
        _lastClientLabel.TabIndex = 2;
        _lastClientLabel.Text = "Bağlantı bekleniyor";
        _lastClientLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _chromePanel
        // 
        _chromePanel.BackColor = Color.FromArgb(5, 14, 27);
        _chromePanel.Controls.Add(_closeButton);
        _chromePanel.Controls.Add(_maximizeButton);
        _chromePanel.Controls.Add(_minimizeButton);
        _chromePanel.Dock = DockStyle.Right;
        _chromePanel.Location = new Point(852, 0);
        _chromePanel.Name = "_chromePanel";
        _chromePanel.Size = new Size(126, 60);
        _chromePanel.TabIndex = 10;
        // 
        // _minimizeButton
        // 
        _minimizeButton.BackColor = Color.FromArgb(5, 14, 27);
        _minimizeButton.FlatStyle = FlatStyle.Flat;
        _minimizeButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _minimizeButton.ForeColor = Color.White;
        _minimizeButton.Glyph = "—";
        _minimizeButton.HoverBackColor = Color.FromArgb(34, 42, 58);
        _minimizeButton.Location = new Point(0, 0);
        _minimizeButton.Name = "_minimizeButton";
        _minimizeButton.Size = new Size(42, 60);
        _minimizeButton.TabIndex = 0;
        _minimizeButton.TabStop = false;
        _minimizeButton.Visible = true;
        _minimizeButton.Click += MinimizeButton_Click;
        // 
        // _maximizeButton
        // 
        _maximizeButton.BackColor = Color.FromArgb(5, 14, 27);
        _maximizeButton.FlatStyle = FlatStyle.Flat;
        _maximizeButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _maximizeButton.ForeColor = Color.White;
        _maximizeButton.Glyph = "□";
        _maximizeButton.HoverBackColor = Color.FromArgb(34, 42, 58);
        _maximizeButton.Location = new Point(42, 0);
        _maximizeButton.Name = "_maximizeButton";
        _maximizeButton.Size = new Size(42, 60);
        _maximizeButton.TabIndex = 1;
        _maximizeButton.TabStop = false;
        _maximizeButton.Visible = true;
        _maximizeButton.Click += MaximizeButton_Click;
        // 
        // _closeButton
        // 
        _closeButton.BackColor = Color.FromArgb(5, 14, 27);
        _closeButton.FlatStyle = FlatStyle.Flat;
        _closeButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _closeButton.ForeColor = Color.White;
        _closeButton.Glyph = "×";
        _closeButton.HoverBackColor = Color.FromArgb(34, 42, 58);
        _closeButton.Location = new Point(84, 0);
        _closeButton.Name = "_closeButton";
        _closeButton.Size = new Size(42, 60);
        _closeButton.TabIndex = 2;
        _closeButton.TabStop = false;
        _closeButton.Visible = true;
        _closeButton.Click += CloseButton_Click;
        // 
        _chromePanel.BackColor = Color.FromArgb(5, 14, 27);
        _chromePanel.Controls.Add(_closeButton);
        _chromePanel.Controls.Add(_maximizeButton);
        _chromePanel.Controls.Add(_minimizeButton);
        _chromePanel.Location = new Point(852, 0);
        _chromePanel.Name = "_chromePanel";
        _chromePanel.Size = new Size(126, 60);
        _chromePanel.TabIndex = 10;
        // 
        // _minimizeButton
        // 
        _minimizeButton.BackColor = Color.FromArgb(5, 14, 27);
        _minimizeButton.FlatStyle = FlatStyle.Flat;
        _minimizeButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _minimizeButton.ForeColor = Color.White;
        _minimizeButton.Glyph = "—";
        _minimizeButton.HoverBackColor = Color.FromArgb(34, 42, 58);
        _minimizeButton.Location = new Point(0, 0);
        _minimizeButton.Name = "_minimizeButton";
        _minimizeButton.Size = new Size(42, 60);
        _minimizeButton.TabIndex = 0;
        _minimizeButton.TabStop = false;
        _minimizeButton.UseVisualStyleBackColor = false;
        _minimizeButton.Visible = true;
        _minimizeButton.Click += MinimizeButton_Click;
        // 
        // _maximizeButton
        // 
        _maximizeButton.BackColor = Color.FromArgb(5, 14, 27);
        _maximizeButton.FlatStyle = FlatStyle.Flat;
        _maximizeButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _maximizeButton.ForeColor = Color.White;
        _maximizeButton.Glyph = "□";
        _maximizeButton.HoverBackColor = Color.FromArgb(34, 42, 58);
        _maximizeButton.Location = new Point(42, 0);
        _maximizeButton.Name = "_maximizeButton";
        _maximizeButton.Size = new Size(42, 60);
        _maximizeButton.TabIndex = 1;
        _maximizeButton.TabStop = false;
        _maximizeButton.UseVisualStyleBackColor = false;
        _maximizeButton.Visible = true;
        _maximizeButton.Click += MaximizeButton_Click;
        // 
        // _closeButton
        // 
        _closeButton.BackColor = Color.FromArgb(5, 14, 27);
        _closeButton.FlatStyle = FlatStyle.Flat;
        _closeButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _closeButton.ForeColor = Color.White;
        _closeButton.Glyph = "×";
        _closeButton.HoverBackColor = Color.FromArgb(34, 42, 58);
        _closeButton.Location = new Point(84, 0);
        _closeButton.Name = "_closeButton";
        _closeButton.Size = new Size(42, 60);
        _closeButton.TabIndex = 2;
        _closeButton.TabStop = false;
        _closeButton.UseVisualStyleBackColor = false;
        _closeButton.Visible = true;
        _closeButton.Click += CloseButton_Click;
        // 
        _chromePanel.BackColor = Color.FromArgb(5, 14, 27);
        _chromePanel.Dock = DockStyle.Right;
        _chromePanel.Location = new Point(852, 0);
        _chromePanel.Name = "_chromePanel";
        _chromePanel.Size = new Size(126, 60);
        _chromePanel.TabIndex = 10;
        // 
        // _contentPanel
        // 
        _contentPanel.BackColor = Color.FromArgb(3, 8, 18);
        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.Location = new Point(192, 64);
        _contentPanel.Name = "_contentPanel";
        _contentPanel.Padding = new Padding(14);
        _contentPanel.Size = new Size(984, 596);
        _contentPanel.TabIndex = 0;
        _contentPanel.Controls.Add(_broadcastPage);
        _contentPanel.Controls.Add(_connectionsPage);
        _contentPanel.Controls.Add(_statisticsPage);
        _contentPanel.Controls.Add(_adminPage);
        _contentPanel.Controls.Add(_aboutPage);
        _contentPanel.Controls.Add(_logPage);
        _broadcastPage.Dock = DockStyle.Fill;
        _connectionsPage.Dock = DockStyle.Fill;
        _statisticsPage.Dock = DockStyle.Fill;
        _adminPage.Dock = DockStyle.Fill;
        _aboutPage.Dock = DockStyle.Fill;
        _logPage.Dock = DockStyle.Fill;
        _broadcastPage.Visible = true;
        _connectionsPage.Visible = false;
        _statisticsPage.Visible = false;
        _adminPage.Visible = false;
        _aboutPage.Visible = false;
        _logPage.Visible = false;
        // 
        // _footer
        // 
        _footer.BackColor = Color.FromArgb(4, 13, 24);
        _footer.Controls.Add(_socialLinksPanel);
        _footer.Controls.Add(_footerRight);
        _footer.Controls.Add(_footerLeft);
        _footer.Dock = DockStyle.Bottom;
        _footer.Location = new Point(192, 660);
        _footer.Name = "_footer";
        _footer.Size = new Size(984, 36);
        _footer.TabIndex = 1;
        // 
        // _socialLinksPanel
        // 
        _socialLinksPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _socialLinksPanel.AutoSize = true;
        _socialLinksPanel.BackColor = Color.Transparent;
        _socialLinksPanel.Controls.Add(_youtubeIcon);
        _socialLinksPanel.Controls.Add(_githubIcon);
        _socialLinksPanel.Controls.Add(_twitterIcon);
        _socialLinksPanel.Controls.Add(_instagramIcon);
        _socialLinksPanel.Controls.Add(_facebookIcon);
        _socialLinksPanel.Location = new Point(823, 3);
        _socialLinksPanel.Name = "_socialLinksPanel";
        _socialLinksPanel.Size = new Size(140, 28);
        _socialLinksPanel.TabIndex = 0;
        _socialLinksPanel.WrapContents = false;
        // 
        // _youtubeIcon
        // 
        _youtubeIcon.BackColor = Color.Transparent;
        _youtubeIcon.Cursor = Cursors.Hand;
        _youtubeIcon.Image = Properties.Resources.YouTubeIcon;
        _youtubeIcon.Location = new Point(3, 3);
        _youtubeIcon.Name = "_youtubeIcon";
        _youtubeIcon.Size = new Size(22, 22);
        _youtubeIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _youtubeIcon.TabIndex = 0;
        _youtubeIcon.TabStop = false;
        _youtubeIcon.Tag = "https://www.youtube.com/@trpolaris";
        _youtubeIcon.Click += SocialOpen;
        _youtubeIcon.MouseEnter += SocialEnter;
        _youtubeIcon.MouseLeave += SocialLeave;
        // 
        // _githubIcon
        // 
        _githubIcon.BackColor = Color.Transparent;
        _githubIcon.Cursor = Cursors.Hand;
        _githubIcon.Image = Properties.Resources.GitHubIcon;
        _githubIcon.Location = new Point(31, 3);
        _githubIcon.Name = "_githubIcon";
        _githubIcon.Size = new Size(22, 22);
        _githubIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _githubIcon.TabIndex = 1;
        _githubIcon.TabStop = false;
        _githubIcon.Tag = "https://github.com/trpolaris";
        _githubIcon.Click += SocialOpen;
        _githubIcon.MouseEnter += SocialEnter;
        _githubIcon.MouseLeave += SocialLeave;
        // 
        // _twitterIcon
        // 
        _twitterIcon.BackColor = Color.Transparent;
        _twitterIcon.Cursor = Cursors.Hand;
        _twitterIcon.Image = Properties.Resources.XIcon;
        _twitterIcon.Location = new Point(59, 3);
        _twitterIcon.Name = "_twitterIcon";
        _twitterIcon.Size = new Size(22, 22);
        _twitterIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _twitterIcon.TabIndex = 2;
        _twitterIcon.TabStop = false;
        _twitterIcon.Tag = "https://x.com/celill_ylmz";
        _twitterIcon.Click += SocialOpen;
        _twitterIcon.MouseEnter += SocialEnter;
        _twitterIcon.MouseLeave += SocialLeave;
        // 
        // _instagramIcon
        // 
        _instagramIcon.BackColor = Color.Transparent;
        _instagramIcon.Cursor = Cursors.Hand;
        _instagramIcon.Image = Properties.Resources.InstagramIcon;
        _instagramIcon.Location = new Point(87, 3);
        _instagramIcon.Name = "_instagramIcon";
        _instagramIcon.Size = new Size(22, 22);
        _instagramIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _instagramIcon.TabIndex = 3;
        _instagramIcon.TabStop = false;
        _instagramIcon.Tag = "https://www.instagram.com/celill.ylmz/";
        _instagramIcon.Click += SocialOpen;
        _instagramIcon.MouseEnter += SocialEnter;
        _instagramIcon.MouseLeave += SocialLeave;
        // 
        // _facebookIcon
        // 
        _facebookIcon.BackColor = Color.Transparent;
        _facebookIcon.Cursor = Cursors.Hand;
        _facebookIcon.Image = Properties.Resources.FacebookIcon;
        _facebookIcon.Location = new Point(115, 3);
        _facebookIcon.Name = "_facebookIcon";
        _facebookIcon.Size = new Size(22, 22);
        _facebookIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _facebookIcon.TabIndex = 4;
        _facebookIcon.TabStop = false;
        _facebookIcon.Tag = "https://www.facebook.com/celill.ylmz";
        _facebookIcon.Click += SocialOpen;
        _facebookIcon.MouseEnter += SocialEnter;
        _facebookIcon.MouseLeave += SocialLeave;
        // 
        // _footerRight
        // 
        _footerRight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _footerRight.AutoSize = true;
        _footerRight.Font = new Font("Segoe UI", 8.5F);
        _footerRight.ForeColor = Color.FromArgb(104, 145, 174);
        _footerRight.Location = new Point(218, 11);
        _footerRight.Name = "_footerRight";
        _footerRight.Size = new Size(199, 15);
        _footerRight.TabIndex = 1;
        _footerRight.Text = "VIRTUAL DISPLAY INFRASTRUCTURE";
        // 
        // _footerLeft
        // 
        _footerLeft.AutoSize = true;
        _footerLeft.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _footerLeft.ForeColor = Color.FromArgb(70, 208, 255);
        _footerLeft.Location = new Point(18, 11);
        _footerLeft.Name = "_footerLeft";
        _footerLeft.Size = new Size(140, 15);
        _footerLeft.TabIndex = 2;
        _footerLeft.Text = "●  TRPOLARIS  •  SERVER";
        // 
        // _mainHeader
        // 
        _mainHeader.BackColor = Color.FromArgb(5, 14, 27);
        _mainHeader.Controls.Add(_statusBadge);
        _mainHeader.Controls.Add(_lastClientLabel);
        _mainHeader.Controls.Add(_mainSubtitle);
        _mainHeader.Controls.Add(_mainTitle);
        _mainHeader.Controls.Add(_chromePanel);
        _mainHeader.Dock = DockStyle.Top;
        _mainHeader.Location = new Point(192, 4);
        _mainHeader.Name = "_mainHeader";
        _mainHeader.Padding = new Padding(16, 0, 6, 0);
        _mainHeader.Size = new Size(984, 60);
        _mainHeader.TabIndex = 3;
        // 
        // _statusBadge
        // 
        _statusBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _statusBadge.BackColor = Color.FromArgb(8, 48, 45);
        _statusBadge.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        _statusBadge.ForeColor = Color.FromArgb(91, 245, 177);
        _statusBadge.Location = new Point(704, 13);
        _statusBadge.Name = "_statusBadge";
        _statusBadge.Size = new Size(128, 34);
        _statusBadge.TabIndex = 1;
        _statusBadge.Text = "●  SUNUCU HAZIR";
        _statusBadge.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _mainSubtitle
        // 
        _mainSubtitle.Font = new Font("Segoe UI", 8F);
        _mainSubtitle.ForeColor = Color.FromArgb(104, 181, 218);
        _mainSubtitle.Location = new Point(19, 35);
        _mainSubtitle.Name = "_mainSubtitle";
        _mainSubtitle.Size = new Size(650, 18);
        _mainSubtitle.TabIndex = 2;
        _mainSubtitle.Text = "Sanal ekran altyapısı  •  Düşük gecikmeli yayın  •  Güvenli bağlantı";
        // 
        // _mainTitle
        // 
        _mainTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        _mainTitle.ForeColor = Color.White;
        _mainTitle.Location = new Point(16, 8);
        _mainTitle.Name = "_mainTitle";
        _mainTitle.Size = new Size(520, 27);
        _mainTitle.TabIndex = 3;
        _mainTitle.Text = "TRPOLARIS SERVER";
        // 
        // ServerForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(3, 7, 16);
        ClientSize = new Size(1180, 700);
        Controls.Add(_contentPanel);
        Controls.Add(_footer);
        Controls.Add(_mainHeader);
        Controls.Add(_topPanel);
        ForeColor = Color.White;
        FormBorderStyle = FormBorderStyle.None;
        Icon = Properties.Resources.ApplicationIcon;
        MaximizeBox = false;
        MinimumSize = new Size(1080, 640);
        Name = "ServerForm";
        Padding = new Padding(4);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "TRPOLARIS SERVER";
        _topPanel.ResumeLayout(false);
        _navPanel.ResumeLayout(false);
        _titlePanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_logoBox).EndInit();
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        _socialLinksPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_youtubeIcon).EndInit();
        ((System.ComponentModel.ISupportInitialize)_githubIcon).EndInit();
        ((System.ComponentModel.ISupportInitialize)_twitterIcon).EndInit();
        ((System.ComponentModel.ISupportInitialize)_instagramIcon).EndInit();
        ((System.ComponentModel.ISupportInitialize)_facebookIcon).EndInit();
        _mainHeader.ResumeLayout(false);
        ResumeLayout(false);
    }

}
