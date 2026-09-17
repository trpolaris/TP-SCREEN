#nullable disable
namespace PolarisDisplay.Client;

partial class ClientForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel _headerPanel;
    private Panel _headerTop;
    private Panel _chromePanel;
    private Panel _footerBar;
    private Panel _navPanel;
    private Panel _contentPanel;
    private TableLayoutPanel _navLayout;
    private PictureBox _logoBox;
    private Label _footerLeft;
    private FlowLayoutPanel _socialLinksPanel;
    private PictureBox _facebookIcon, _instagramIcon, _twitterIcon, _githubIcon, _youtubeIcon;
    private Label _footerRight;
    private Panel _mainHeader;
    private Label _mainTitle, _mainSubtitle, _statusBadge, _sideBrand, _sideCaption;
    private ChromeButton _fullscreenHeaderButton;
    private ChromeButton _minimizeButton;
    private ChromeButton _maximizeButton;
    private ChromeButton _closeButton;
    private NeonTabButton _tabConnection;
    private NeonTabButton _tabDisplay;
    private NeonTabButton _tabSettings;
    private NeonTabButton _tabAbout;
    private ConnectionPage _connectionPage;
    private DisplayPage _displayPage;
    private AboutPage _aboutPage;
    private SettingsPage _settingsPage;
    private System.Windows.Forms.Timer _overlayTimer;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _headerPanel=new Panel(); _headerTop=new Panel(); _logoBox=new PictureBox(); _chromePanel=new Panel(); _closeButton=new ChromeButton(); _maximizeButton=new ChromeButton(); _minimizeButton=new ChromeButton(); _fullscreenHeaderButton=new ChromeButton(); _navPanel=new Panel(); _navLayout=new TableLayoutPanel();
        _tabConnection=new NeonTabButton(); _tabDisplay=new NeonTabButton(); _tabSettings=new NeonTabButton(); _tabAbout=new NeonTabButton(); _contentPanel=new Panel(); _aboutPage=new AboutPage(); _settingsPage=new SettingsPage(); _displayPage=new DisplayPage(); _connectionPage=new ConnectionPage(); _footerBar=new Panel(); _footerRight=new Label(); _footerLeft=new Label(); _overlayTimer=new System.Windows.Forms.Timer(components);
        _socialLinksPanel=new FlowLayoutPanel(); _facebookIcon=new PictureBox(); _instagramIcon=new PictureBox(); _twitterIcon=new PictureBox(); _githubIcon=new PictureBox(); _youtubeIcon=new PictureBox(); _mainHeader=new Panel(); _mainTitle=new Label(); _mainSubtitle=new Label(); _statusBadge=new Label(); _sideBrand=new Label(); _sideCaption=new Label();
        _headerPanel.SuspendLayout(); _headerTop.SuspendLayout(); ((System.ComponentModel.ISupportInitialize)_logoBox).BeginInit(); _chromePanel.SuspendLayout(); _navPanel.SuspendLayout(); _navLayout.SuspendLayout(); _contentPanel.SuspendLayout(); _footerBar.SuspendLayout(); _socialLinksPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_facebookIcon).BeginInit(); ((System.ComponentModel.ISupportInitialize)_instagramIcon).BeginInit(); ((System.ComponentModel.ISupportInitialize)_twitterIcon).BeginInit(); ((System.ComponentModel.ISupportInitialize)_githubIcon).BeginInit(); ((System.ComponentModel.ISupportInitialize)_youtubeIcon).BeginInit(); _mainHeader.SuspendLayout(); SuspendLayout();

        _headerPanel.BackColor=Color.FromArgb(4,12,24); _headerPanel.Controls.Add(_navPanel); _headerPanel.Controls.Add(_headerTop); _headerPanel.Dock=DockStyle.Left; _headerPanel.Location=new Point(4,4); _headerPanel.Name="_headerPanel"; _headerPanel.Padding=new Padding(8); _headerPanel.Size=new Size(188,672); _headerPanel.TabIndex=2;
        _headerTop.BackColor=Color.FromArgb(5,16,29); _headerTop.Controls.Add(_sideCaption); _headerTop.Controls.Add(_sideBrand); _headerTop.Controls.Add(_logoBox); _headerTop.Dock=DockStyle.Top; _headerTop.Location=new Point(10,10); _headerTop.Name="_headerTop"; _headerTop.Size=new Size(172,104); _headerTop.TabIndex=0;
        _logoBox.BackColor=Color.Transparent; _logoBox.Image=Properties.Resources.Logo; _logoBox.Location=new Point(21,4); _logoBox.Name="_logoBox"; _logoBox.Size=new Size(130,44); _logoBox.SizeMode=PictureBoxSizeMode.Zoom; _logoBox.TabStop=false;
        _sideBrand.AutoSize=false; _sideBrand.Font=new Font("Segoe UI",13F,FontStyle.Bold); _sideBrand.ForeColor=Color.White; _sideBrand.Location=new Point(4,50); _sideBrand.Size=new Size(164,24); _sideBrand.Text="TRPOLARIS"; _sideBrand.TextAlign=ContentAlignment.MiddleCenter;
        _sideCaption.AutoSize=false; _sideCaption.Font=new Font("Segoe UI",8F); _sideCaption.ForeColor=Color.FromArgb(73,204,255); _sideCaption.Location=new Point(4,74); _sideCaption.Size=new Size(164,16); _sideCaption.Text="VIRTUAL DISPLAY CLIENT"; _sideCaption.TextAlign=ContentAlignment.MiddleCenter;
        _navPanel.BackColor=Color.Transparent; _navPanel.Controls.Add(_navLayout); _navPanel.Dock=DockStyle.Fill; _navPanel.Location=new Point(8,128); _navPanel.Name="_navPanel"; _navPanel.Padding=new Padding(0,8,0,8); _navPanel.Size=new Size(172,522); _navPanel.TabIndex=1;
        _navLayout.BackColor=Color.Transparent; _navLayout.ColumnCount=1; _navLayout.RowCount=4; _navLayout.Dock=DockStyle.Top; _navLayout.AutoSize=false; _navLayout.Location=new Point(0,16); _navLayout.Name="_navLayout"; _navLayout.Size=new Size(172,190); _navLayout.TabIndex=0;
        _navLayout.RowStyles.Clear();
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,44F));
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,44F));
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,44F));
        _navLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,44F));
        _navLayout.Controls.Add(_tabConnection,0,0); _navLayout.Controls.Add(_tabDisplay,0,1); _navLayout.Controls.Add(_tabSettings,0,2); _navLayout.Controls.Add(_tabAbout,0,3);
        _tabConnection.BackColor=Color.FromArgb(5,16,29); _tabConnection.Dock=DockStyle.Fill; _tabConnection.Font=new Font("Segoe UI Semibold",8.5F,FontStyle.Bold); _tabConnection.ForeColor=Color.FromArgb(151,190,214); _tabConnection.Margin=new Padding(0,2,0,2); _tabConnection.NormalBack=Color.FromArgb(5,16,29); _tabConnection.NormalFore=Color.FromArgb(151,190,214); _tabConnection.SelectedStart=Color.FromArgb(8,74,118); _tabConnection.SelectedEnd=Color.FromArgb(18,49,91); _tabConnection.Text="↗   BAĞLANTI"; _tabConnection.TabStop=false;
        _tabDisplay.BackColor=Color.FromArgb(5,16,29); _tabDisplay.Dock=DockStyle.Fill; _tabDisplay.Font=new Font("Segoe UI Semibold",8.5F,FontStyle.Bold); _tabDisplay.ForeColor=Color.FromArgb(151,190,214); _tabDisplay.Margin=new Padding(0,2,0,2); _tabDisplay.NormalBack=Color.FromArgb(5,16,29); _tabDisplay.NormalFore=Color.FromArgb(151,190,214); _tabDisplay.SelectedStart=Color.FromArgb(8,74,118); _tabDisplay.SelectedEnd=Color.FromArgb(18,49,91); _tabDisplay.Text="▣   GÖRÜNTÜ"; _tabDisplay.TabStop=false;
        _tabSettings.BackColor=Color.FromArgb(5,16,29); _tabSettings.Dock=DockStyle.Fill; _tabSettings.Font=new Font("Segoe UI Semibold",8.5F,FontStyle.Bold); _tabSettings.ForeColor=Color.FromArgb(151,190,214); _tabSettings.Margin=new Padding(0,2,0,2); _tabSettings.NormalBack=Color.FromArgb(5,16,29); _tabSettings.NormalFore=Color.FromArgb(151,190,214); _tabSettings.SelectedStart=Color.FromArgb(8,74,118); _tabSettings.SelectedEnd=Color.FromArgb(18,49,91); _tabSettings.Text="⚙   AYARLAR"; _tabSettings.TabStop=false;
        _tabAbout.BackColor=Color.FromArgb(5,16,29); _tabAbout.Dock=DockStyle.Fill; _tabAbout.Font=new Font("Segoe UI Semibold",8.5F,FontStyle.Bold); _tabAbout.ForeColor=Color.FromArgb(151,190,214); _tabAbout.Margin=new Padding(0,2,0,2); _tabAbout.NormalBack=Color.FromArgb(5,16,29); _tabAbout.NormalFore=Color.FromArgb(151,190,214); _tabAbout.SelectedStart=Color.FromArgb(8,74,118); _tabAbout.SelectedEnd=Color.FromArgb(18,49,91); _tabAbout.Text="ⓘ   HAKKINDA"; _tabAbout.TabStop=false; _tabConnection.Selected=true;

        _mainHeader.BackColor=Color.FromArgb(5,14,27); _mainHeader.Controls.Add(_statusBadge); _mainHeader.Controls.Add(_mainSubtitle); _mainHeader.Controls.Add(_mainTitle); _mainHeader.Controls.Add(_chromePanel); _mainHeader.Dock=DockStyle.Top; _mainHeader.Location=new Point(200,4); _mainHeader.Name="_mainHeader"; _mainHeader.Padding=new Padding(16,0,6,0); _mainHeader.Size=new Size(926,60); _mainHeader.TabIndex=3;
        _mainTitle.AutoSize=false; _mainTitle.Font=new Font("Segoe UI",15F,FontStyle.Bold); _mainTitle.ForeColor=Color.White; _mainTitle.Location=new Point(16,7); _mainTitle.Size=new Size(430,27); _mainTitle.Text="TRPOLARIS CLIENT";
        _mainSubtitle.AutoSize=false; _mainSubtitle.Font=new Font("Segoe UI",8F); _mainSubtitle.ForeColor=Color.FromArgb(104,181,218); _mainSubtitle.Location=new Point(17,34); _mainSubtitle.Size=new Size(520,18); _mainSubtitle.Text="Her yerde senin ekranın  •  Düşük gecikmeli görüntü aktarımı";
        _statusBadge.AutoSize=false; _statusBadge.Anchor=AnchorStyles.Top|AnchorStyles.Right; _statusBadge.BackColor=Color.FromArgb(8,35,50); _statusBadge.Font=new Font("Segoe UI Semibold",8.5F,FontStyle.Bold); _statusBadge.ForeColor=Color.FromArgb(92,220,255); _statusBadge.Location=new Point(620,12); _statusBadge.Size=new Size(118,32); _statusBadge.Text="●  BEKLENİYOR"; _statusBadge.TextAlign=ContentAlignment.MiddleCenter;
        _chromePanel.BackColor=Color.FromArgb(5,14,27); _chromePanel.Controls.Add(_closeButton); _chromePanel.Controls.Add(_maximizeButton); _chromePanel.Controls.Add(_minimizeButton); _chromePanel.Controls.Add(_fullscreenHeaderButton); _chromePanel.Dock=DockStyle.Right; _chromePanel.Location=new Point(894,0); _chromePanel.Name="_chromePanel"; _chromePanel.Size=new Size(168,60); _chromePanel.TabIndex=2;
        _fullscreenHeaderButton.BackColor=Color.FromArgb(5,14,27); _fullscreenHeaderButton.FlatStyle=FlatStyle.Flat; _fullscreenHeaderButton.Font=new Font("Segoe UI",10F,FontStyle.Bold); _fullscreenHeaderButton.ForeColor=Color.White; _fullscreenHeaderButton.Glyph="⛶"; _fullscreenHeaderButton.Location=new Point(0,0); _fullscreenHeaderButton.Size=new Size(42,60); _fullscreenHeaderButton.TabStop=false; _fullscreenHeaderButton.UseVisualStyleBackColor=false;
        _minimizeButton.BackColor=Color.FromArgb(5,14,27); _minimizeButton.FlatStyle=FlatStyle.Flat; _minimizeButton.Font=new Font("Segoe UI",11F,FontStyle.Bold); _minimizeButton.ForeColor=Color.White; _minimizeButton.Glyph="—"; _minimizeButton.Location=new Point(42,0); _minimizeButton.Size=new Size(42,60); _minimizeButton.TabStop=false; _minimizeButton.UseVisualStyleBackColor=false;
        _maximizeButton.BackColor=Color.FromArgb(5,14,27); _maximizeButton.FlatStyle=FlatStyle.Flat; _maximizeButton.Font=new Font("Segoe UI",11F,FontStyle.Bold); _maximizeButton.ForeColor=Color.White; _maximizeButton.Glyph="□"; _maximizeButton.Location=new Point(84,0); _maximizeButton.Size=new Size(42,60); _maximizeButton.TabStop=false; _maximizeButton.UseVisualStyleBackColor=false; _maximizeButton.Visible=true; _maximizeButton.Enabled=true;
        _closeButton.BackColor=Color.FromArgb(5,14,27); _closeButton.FlatStyle=FlatStyle.Flat; _closeButton.Font=new Font("Segoe UI",11F,FontStyle.Bold); _closeButton.ForeColor=Color.White; _closeButton.Glyph="×"; _closeButton.Location=new Point(126,0); _closeButton.Size=new Size(42,60); _closeButton.TabStop=false; _closeButton.UseVisualStyleBackColor=false; _fullscreenHeaderButton.Click += FullscreenHeaderButton_Click; _minimizeButton.Click += MinimizeButton_Click; _maximizeButton.Click += MaximizeButton_Click; _closeButton.Click += CloseButton_Click;

        _contentPanel.BackColor=Color.FromArgb(3,8,18); _contentPanel.Controls.Add(_aboutPage); _contentPanel.Controls.Add(_settingsPage); _contentPanel.Controls.Add(_displayPage); _contentPanel.Controls.Add(_connectionPage); _contentPanel.Dock=DockStyle.Fill; _contentPanel.Location=new Point(200,68); _contentPanel.Name="_contentPanel"; _contentPanel.Padding=new Padding(12); _contentPanel.Size=new Size(918,612); _contentPanel.TabIndex=0;
        _aboutPage.BackColor=Color.FromArgb(3,8,18); _aboutPage.Dock=DockStyle.Fill; _aboutPage.Location=new Point(12,12); _aboutPage.Padding=new Padding(10); _aboutPage.Visible=false; _settingsPage.BackColor=Color.FromArgb(3,8,18); _settingsPage.Dock=DockStyle.Fill; _settingsPage.Location=new Point(12,12); _settingsPage.Padding=new Padding(10); _settingsPage.Visible=false; _displayPage.BackColor=Color.FromArgb(3,8,18); _displayPage.Dock=DockStyle.Fill; _displayPage.Location=new Point(12,12); _displayPage.Size=new Size(782,448); _displayPage.Visible=false; _connectionPage.BackColor=Color.FromArgb(3,8,18); _connectionPage.Dock=DockStyle.Fill; _connectionPage.Location=new Point(12,12); _connectionPage.Padding=new Padding(10); _connectionPage.Size=new Size(782,448); _connectionPage.Visible=true;

        _footerBar.BackColor=Color.FromArgb(4,13,24); _footerBar.Controls.Add(_socialLinksPanel); _footerBar.Controls.Add(_footerRight); _footerBar.Controls.Add(_footerLeft); _footerBar.Dock=DockStyle.Bottom; _footerBar.Location=new Point(200,740); _footerBar.Name="_footerBar"; _footerBar.Size=new Size(918,34); _footerBar.TabIndex=1;
        _footerLeft.AutoSize=true; _footerLeft.Font=new Font("Segoe UI Semibold",8.5F,FontStyle.Bold); _footerLeft.ForeColor=Color.FromArgb(70,208,255); _footerLeft.Location=new Point(16,10); _footerLeft.Name="_footerLeft"; _footerLeft.Text="●  TRPOLARIS  •  CLIENT";
        _footerRight.Anchor=AnchorStyles.Top|AnchorStyles.Right; _footerRight.AutoSize=true; _footerRight.Font=new Font("Segoe UI",8.5F); _footerRight.ForeColor=Color.FromArgb(104,145,174); _footerRight.Location=new Point(500,10); _footerRight.Name="_footerRight"; _footerRight.Text="LOW LATENCY DISPLAY";
        _socialLinksPanel.Anchor=AnchorStyles.Top|AnchorStyles.Right; _socialLinksPanel.AutoSize=true; _socialLinksPanel.BackColor=Color.Transparent; _socialLinksPanel.FlowDirection=FlowDirection.LeftToRight; _socialLinksPanel.Location=new Point(740,5); _socialLinksPanel.Size=new Size(130,26); _socialLinksPanel.WrapContents=false;
        _facebookIcon.BackColor=Color.Transparent; _facebookIcon.Cursor=Cursors.Hand; _facebookIcon.Image=Properties.Resources.FacebookIcon; _facebookIcon.Location=new Point(0,2); _facebookIcon.Size=new Size(22,22); _facebookIcon.SizeMode=PictureBoxSizeMode.Zoom; _facebookIcon.Tag="https://www.facebook.com/celill.ylmz"; _facebookIcon.TabStop=false;
        _instagramIcon.BackColor=Color.Transparent; _instagramIcon.Cursor=Cursors.Hand; _instagramIcon.Image=Properties.Resources.InstagramIcon; _instagramIcon.Location=new Point(26,2); _instagramIcon.Size=new Size(22,22); _instagramIcon.SizeMode=PictureBoxSizeMode.Zoom; _instagramIcon.Tag="https://www.instagram.com/celill.ylmz/"; _instagramIcon.TabStop=false;
        _twitterIcon.BackColor=Color.Transparent; _twitterIcon.Cursor=Cursors.Hand; _twitterIcon.Image=Properties.Resources.XIcon; _twitterIcon.Location=new Point(52,2); _twitterIcon.Size=new Size(22,22); _twitterIcon.SizeMode=PictureBoxSizeMode.Zoom; _twitterIcon.Tag="https://x.com/celill_ylmz"; _twitterIcon.TabStop=false;
        _githubIcon.BackColor=Color.Transparent; _githubIcon.Cursor=Cursors.Hand; _githubIcon.Image=Properties.Resources.GitHubIcon; _githubIcon.Location=new Point(78,2); _githubIcon.Size=new Size(22,22); _githubIcon.SizeMode=PictureBoxSizeMode.Zoom; _githubIcon.Tag="https://github.com/trpolaris"; _githubIcon.TabStop=false;
        _youtubeIcon.BackColor=Color.Transparent; _youtubeIcon.Cursor=Cursors.Hand; _youtubeIcon.Image=Properties.Resources.YouTubeIcon; _youtubeIcon.Location=new Point(104,2); _youtubeIcon.Size=new Size(22,22); _youtubeIcon.SizeMode=PictureBoxSizeMode.Zoom; _youtubeIcon.Tag="https://www.youtube.com/@trpolaris"; _youtubeIcon.TabStop=false;
        _socialLinksPanel.Controls.Add(_youtubeIcon); _socialLinksPanel.Controls.Add(_githubIcon); _socialLinksPanel.Controls.Add(_twitterIcon); _socialLinksPanel.Controls.Add(_instagramIcon); _socialLinksPanel.Controls.Add(_facebookIcon);
        _facebookIcon.Click+=SocialOpen; _facebookIcon.MouseEnter+=SocialEnter; _facebookIcon.MouseLeave+=SocialLeave; _instagramIcon.Click+=SocialOpen; _instagramIcon.MouseEnter+=SocialEnter; _instagramIcon.MouseLeave+=SocialLeave; _twitterIcon.Click+=SocialOpen; _twitterIcon.MouseEnter+=SocialEnter; _twitterIcon.MouseLeave+=SocialLeave; _githubIcon.Click+=SocialOpen; _githubIcon.MouseEnter+=SocialEnter; _githubIcon.MouseLeave+=SocialLeave; _youtubeIcon.Click+=SocialOpen; _youtubeIcon.MouseEnter+=SocialEnter; _youtubeIcon.MouseLeave+=SocialLeave;
        _overlayTimer.Interval=200;

        AutoScaleDimensions=new SizeF(96F,96F); AutoScaleMode=AutoScaleMode.Dpi; BackColor=Color.FromArgb(3,7,16); ForeColor=Color.White; FormBorderStyle=FormBorderStyle.None; MaximizeBox=false; MinimizeBox=true; ClientSize=new Size(1120,680); MinimumSize=new Size(940,580); Name="ClientForm"; Padding=new Padding(4); StartPosition=FormStartPosition.CenterScreen; Text="TRPOLARIS CLIENT"; ShowInTaskbar=true; ShowIcon=true; Icon=Properties.Resources.ApplicationIcon;
        Controls.Add(_contentPanel); Controls.Add(_footerBar); Controls.Add(_mainHeader); Controls.Add(_headerPanel);
        _headerPanel.ResumeLayout(false); _headerTop.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)_logoBox).EndInit(); _chromePanel.ResumeLayout(false); _navPanel.ResumeLayout(false); _navLayout.ResumeLayout(false); _contentPanel.ResumeLayout(false); _footerBar.ResumeLayout(false); _footerBar.PerformLayout(); _socialLinksPanel.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)_facebookIcon).EndInit(); ((System.ComponentModel.ISupportInitialize)_instagramIcon).EndInit(); ((System.ComponentModel.ISupportInitialize)_twitterIcon).EndInit(); ((System.ComponentModel.ISupportInitialize)_githubIcon).EndInit(); ((System.ComponentModel.ISupportInitialize)_youtubeIcon).EndInit(); _mainHeader.ResumeLayout(false); ResumeLayout(false);
    }
}
