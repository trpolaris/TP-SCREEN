using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;

namespace PolarisDisplay.Client;

public sealed partial class ClientForm : Form
{
    // Persistent UI is declared by the page Designers. These aliases keep
    // transport/business logic independent from the page layout.
    private NeonPanel _connectPanel => _connectionPage.ConnectPanel;
    private Label _connectTitle => _connectionPage.ConnectTitle;
    private Panel _connectionBody => _connectionPage.ConnectionBody;
    private Label _cardTitle => _connectionPage.CardTitle;
    private Label _cardSub => _connectionPage.CardSub;
    private NeonButton _cardConnect => _connectionPage.CardConnect;
    private Label _ipLabel => _connectionPage.IpLabel;
    private Label _portLabel => _connectionPage.PortLabel;
    private NeonTextBox _host => _connectionPage.Host;
    private NumericUpDown _port => _connectionPage.Port;
    private NeonPanel _quickPanel => _connectionPage.QuickPanel;
    private Panel _quickBody => _connectionPage.QuickBody;
    private Panel _recentListPanel => _connectionPage.RecentListPanel;
    private Panel _favoritesListPanel => _connectionPage.FavoritesListPanel;
    private Button _quickRecent => _connectionPage.QuickRecent;
    private Button _quickFavorites => _connectionPage.QuickFavorites;
    private Button _quickClear => _connectionPage.QuickClear;
    private Panel _viewPanel => _displayPage.ViewPanel;
    private VideoView _view => _displayPage.View;
    private NeonPanel _displayPanel => _displayPage.DisplayPanel;
    private Label _displayTitle => _displayPage.DisplayTitle;
    private NeonComboBox _displayMode => _displayPage.DisplayMode;
    private NeonComboBox _displayQuality => _displayPage.DisplayQuality;
    private NeonComboBox _serverScreenSelect => _displayPage.ServerScreenSelect;
    private Label _serverScreenLabel => _displayPage.ServerScreenLabel;
    private NeonPanel _statusPanel => _displayPage.StatusPanel;
    private Label _status => _displayPage.Status;
    private Label _stats => _displayPage.Stats;
    private NeonButton _fullscreenDisconnectButton => _displayPage.FullscreenDisconnectButton;
    private NeonButton _fullscreenExitButton => _displayPage.FullscreenExitButton;
    private NeonPanel _categoryPanel => _aboutPage.CategoryPanel;
    private Label _categoryTitle => _aboutPage.CategoryTitle;
    private Label _categoryText => _aboutPage.CategoryText;

    private readonly System.Windows.Forms.Timer _hoverHideTimer = new() { Interval = 1600 };
    private bool _pointerInsideDisplayOverlay;
    private bool _pointerInsideFullscreenToolbar;

    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HTCAPTION = 0x2;
    private const int WM_SETICON = 0x0080;
    private static readonly IntPtr ICON_SMALL = IntPtr.Zero;
    private static readonly IntPtr ICON_BIG = new IntPtr(1);
    private int _quickExpanded = 0;
    private bool _loadingServerScreens;
    private bool _applyingServerScreenSelection;
    private string _selectedServerScreen = string.Empty;
    private static readonly HttpClient ScreenHttp = new() { Timeout = TimeSpan.FromSeconds(3) };
    private readonly System.Windows.Forms.Timer _screenLoadTimer = new() { Interval = 350 };

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            // Keep the borderless viewer as a normal taskbar application.
            // WS_EX_TOOLWINDOW suppresses the taskbar button/icon; WS_EX_APPWINDOW forces it.
            cp.ExStyle &= ~0x00000080; // WS_EX_TOOLWINDOW
            cp.ExStyle |= 0x00040000;  // WS_EX_APPWINDOW
            return cp;
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!IsDesignMode() && Icon is not null)
        {
            // Explicitly install both small and large icons on the native window.
            // This makes the icon reliable for a borderless WinForms taskbar button.
            SendMessage(Handle, WM_SETICON, ICON_BIG, Icon.Handle);
            SendMessage(Handle, WM_SETICON, ICON_SMALL, Icon.Handle);
        }
    }

    public ClientForm()
    {
        InitializeComponent();
        // Borderless WinForms forms can lose their taskbar icon when the form icon is only designer-assigned.
        // Re-assign the embedded application icon at runtime as well.
        Icon = Properties.Resources.ApplicationIcon;
        ShowIcon = true;
        ShowInTaskbar = true;
        if (IsDesignMode()) return;
        _chromePanel.BringToFront();
        _fullscreenHeaderButton.BringToFront();
        _minimizeButton.BringToFront();
        _maximizeButton.BringToFront();
        _closeButton.BringToFront();
        ApplySavedSettings();
        PolarisTheme.Apply(this);
        EnsureChromeButtonsVisible();

        _hoverHideTimer.Tick += (_, _) =>
        {
            _hoverHideTimer.Stop();
            bool cursorOverDisplayPanel = _displayPanel.Visible && _displayPanel.RectangleToScreen(_displayPanel.ClientRectangle).Contains(Cursor.Position);
            if (cursorOverDisplayPanel) _pointerInsideDisplayOverlay = true;
            if (!_pointerInsideDisplayOverlay && !cursorOverDisplayPanel) _displayPanel.Visible = false;
            if (!_pointerInsideFullscreenToolbar)
            {
                _fullscreenDisconnectButton.Visible = false;
                _fullscreenExitButton.Visible = false;
            }
        };
        _view.MouseEnter += (_, _) => HandlePointerEnterView();
        _view.MouseMove += (_, _) => HandlePointerEnterView();
        _viewPanel.MouseMove += (_, _) => HandlePointerEnterView();
        _displayPanel.MouseEnter += (_, _) => { _pointerInsideDisplayOverlay = true; ShowDisplayOverlay(); };
        _displayPanel.MouseLeave += (_, _) => { _pointerInsideDisplayOverlay = false; ArmHoverHide(); };
        _fullscreenDisconnectButton.MouseEnter += (_, _) => { _pointerInsideFullscreenToolbar = true; ShowFullscreenToolbar(); };
        _fullscreenExitButton.MouseEnter += (_, _) => { _pointerInsideFullscreenToolbar = true; ShowFullscreenToolbar(); };
        _fullscreenDisconnectButton.MouseLeave += (_, _) => { _pointerInsideFullscreenToolbar = false; ArmHoverHide(); };
        _fullscreenExitButton.MouseLeave += (_, _) => { _pointerInsideFullscreenToolbar = false; ArmHoverHide(); };

        SetupFullscreenOverlay();
        CreateQuickAccessPanels();
        CreateServerScreenSelector();
        _displayQuality.SelectedIndexChanged += async (_, _) =>
        {
            if (_displayQuality.SelectedItem is null) return;
            _settings.QualityMode = _displayQuality.SelectedItem.ToString() ?? "Otomatik";
            ClientSettingsStore.Save(_settings);
            if (_client is not null)
                await SendRemoteCommandAsync($"POLARIS_MODE|{Uri.EscapeDataString(_settings.QualityMode)}\n");
        };
        _screenLoadTimer.Tick += async (_, _) => { _screenLoadTimer.Stop(); await LoadServerScreensAsync(); };
        _host.TextChanged += (_, _) => { _screenLoadTimer.Stop(); _screenLoadTimer.Start(); };
        _host.Leave += (_, _) => _ = LoadServerScreensAsync();
        Shown += (_, _) => BeginInvoke((Action)(() => _ = LoadServerScreensAsync()));
        Shown += async (_, _) => { if (_settings.AutoDiscover) await DiscoverServerAsync(); };
        _tabConnection.Click += (_, _) => SelectTab(0);
        _tabDisplay.Click += (_, _) => SelectTab(1);
        _tabSettings.Click += (_, _) => SelectTab(2);
        _tabAbout.Click += (_, _) => SelectTab(3);
        _settingsPage.OpenSettings += (_, _) => ShowSettings();
        _settingsPage.CheckUpdates += async (_, _) => await CheckForClientUpdateAsync();
        SelectTab(0);
        _cardConnect.Click += async (_, _) => await ToggleConnectionAsync();
        _quickRecent.Click += (_, _) => ToggleQuickPanel(1);
        _quickFavorites.Click += (_, _) => ToggleQuickPanel(2);
        _quickClear.Click += (_, _) => { ClearRecentConnections(); if (_quickExpanded == 1) RefreshQuickAccessPanels(); };
        _host.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) _ = ToggleConnectionAsync(); };
        _headerPanel.DoubleClick += (_, _) => ToggleWindowMaximize();
        _headerPanel.MouseDown += DragWindow; _headerTop.MouseDown += DragWindow; _logoBox.MouseDown += DragWindow;
        _mainHeader.MouseDown += DragWindow; _mainTitle.MouseDown += DragWindow; _mainSubtitle.MouseDown += DragWindow; _statusBadge.MouseDown += DragWindow;
        KeyDown += (_, e) => { if (e.KeyCode == Keys.F11) ToggleFullscreen(); if (e.KeyCode == Keys.Escape && _isFullscreen) ExitFullscreen(); if (e.KeyCode == Keys.Enter && e.Control) _ = ToggleConnectionAsync(); };
        KeyPreview = true;
        KeyDown += (_, e) => { if (e.KeyCode == Keys.F10) ShowSettings(); };
        _pingTimer.Tick += async (_, _) => await SendPingAsync();
        _pingTimer.Start();
        MouseMove += (_, e) => HandleViewerMouseMove(e.Location);
        _headerPanel.MouseMove += (_, e) => HandleViewerMouseMove(e.Location);
        _viewPanel.MouseMove += (_, e) => HandleViewerMouseMove(e.Location);
        _view.TabStop = true; _view.Cursor = Cursors.Default; _view.Click += (_, _) => _view.Focus();
        _contentPanel.MouseMove += (_, e) => HandleViewerMouseMove(e.Location);
        _fullscreenDisconnectButton.MouseMove += (_, e) => HandleViewerMouseMove(e.Location);
        _fullscreenExitButton.MouseMove += (_, e) => HandleViewerMouseMove(e.Location);
        _overlayTimer.Tick += (_, _) => UpdateOverlayVisibility(); _overlayTimer.Start();
        _screenNoticeTimer.Tick += (_, _) => { _screenNoticeTimer.Stop(); _status.Text = _client is null ? "Bağlantı bekleniyor" : $"JPEG • {_streamMode}"; };
        Resize += (_, _) =>
        {
            if (_isFullscreen)
            {
                LayoutFullscreenSurface();
                LayoutFullscreenToolbar();
                return;
            }
            _maximizeButton.Glyph = WindowState == FormWindowState.Maximized ? "❐" : "□";
            PerformLayout();
        };
        _reconnectTimer.Tick += (_, _) =>
        {
            if (_client is not null || Interlocked.CompareExchange(ref _connectionBusy, 0, 0) != 0) return;
            if (_autoReconnect)
            {
                if (_settings.AutoDiscover && string.IsNullOrWhiteSpace(_host.Text)) _ = DiscoverServerAsync();
                else _ = ToggleConnectionAsync();
            }
            else if (_settings.AutoDiscover && string.IsNullOrWhiteSpace(_host.Text))
            {
                _ = DiscoverServerAsync();
            }
        };
        _reconnectTimer.Start();
        FormClosing += (_, _) => { _autoReconnect=false; _reconnectTimer.Stop(); _reconnectTimer.Dispose(); _pingTimer.Stop(); _pingTimer.Dispose(); _overlayTimer.Stop(); _hoverHideTimer.Stop(); _hoverHideTimer.Dispose(); _screenLoadTimer.Stop(); _screenLoadTimer.Dispose(); Disconnect(); };
    }



    private static void SocialOpen(object? sender, EventArgs e)
    {
        if (sender is PictureBox { Tag: string url } && Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            try
            {
                Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
            }
            catch
            {
                // Ignore browser launch failures.
            }
        }
    }

    private static void SocialEnter(object? sender, EventArgs e)
    {
        if (sender is PictureBox icon)
            icon.BackColor = Color.FromArgb(28, 0, 180, 255);
    }

    private static void SocialLeave(object? sender, EventArgs e)
    {
        if (sender is PictureBox icon)
            icon.BackColor = Color.Transparent;
    }

    private void EnsureChromeButtonsVisible()
    {
        _chromePanel.Visible = true;
        _chromePanel.BringToFront();
        _fullscreenHeaderButton.Visible = true;
        _minimizeButton.Visible = true;
        _maximizeButton.Visible = true;
        _closeButton.Visible = true;
        _fullscreenHeaderButton.BringToFront();
        _minimizeButton.BringToFront();
        _maximizeButton.BringToFront();
        _closeButton.BringToFront();
    }

    private void SetupFullscreenOverlay()
    {
        // The controls are Designer-owned direct children of VideoView. A
        // transparent WinForms Panel would otherwise paint a dark rectangle
        // over the JPEG frame behind these buttons.
        _fullscreenDisconnectButton.Visible = false;
        _fullscreenExitButton.Visible = false;
        // Designer owns these controls and the VideoView is added after them, so
        // VideoView is the topmost child by default. Bring the two overlay buttons
        // to the front without changing any Designer geometry.
        _fullscreenDisconnectButton.BringToFront();
        _fullscreenExitButton.BringToFront();
        _fullscreenDisconnectButton.Click += async (_, _) =>
        {
            _autoReconnect = false;
            if (_isFullscreen) ExitFullscreen();
            await ToggleConnectionAsync();
        };

        _fullscreenExitButton.Click += (_, _) => ExitFullscreen();
    }


    private void CreateQuickAccessPanels()
    {
        // Hosts are owned by ConnectionPage.Designer.cs.
        RefreshQuickAccessPanels();
    }

    private void ToggleQuickPanel(int which)
    {
        _quickExpanded = _quickExpanded == which ? 0 : which;
        RefreshQuickAccessPanels();
    }

    private void RefreshQuickAccessPanels()
    {
        if (_recentListPanel == null || _favoritesListPanel == null) return;
        BuildQuickList(_recentListPanel, LoadConnections(), false);
        BuildQuickList(_favoritesListPanel, LoadFavorites(), true);
        _recentListPanel.Visible = _quickExpanded == 1;
        _favoritesListPanel.Visible = _quickExpanded == 2;
    }

    private void BuildQuickList(Panel host, List<SavedConnection> list, bool favorites)
    {
        host.SuspendLayout();
        host.Controls.Clear();
        int y = 0;
        if (list.Count == 0)
        {
            var empty = new Label
            {
                Text = favorites ? "Henüz favori bağlantı yok" : "Henüz başarılı bağlantı yok",
                ForeColor = Color.FromArgb(135, 175, 205),
                Font = new Font("Segoe UI", 9.5F),
                Bounds = new Rectangle(4, 4, Math.Max(180, host.ClientSize.Width - 8), 38),
                TextAlign = ContentAlignment.MiddleLeft
            };
            host.Controls.Add(empty);
            y = 46;
        }
        else
        {
            foreach (var item in list.Take(8))
            {
                var row = new Panel
                {
                    Bounds = new Rectangle(0, y, Math.Max(180, host.ClientSize.Width - 4), 42),
                    BackColor = Color.FromArgb(5, 18, 32),
                    Cursor = Cursors.Hand,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = item,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                var label = new Label
                {
                    Text = $"▣  {item.Host}:{item.Port}",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                    Bounds = new Rectangle(10, 2, Math.Max(160, row.Width - 20), 24),
                    AutoEllipsis = true,
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                var sub = new Label
                {
                    Text = item.LastUsedUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss"),
                    ForeColor = Color.FromArgb(105, 170, 215),
                    Font = new Font("Segoe UI", 7.5F),
                    Bounds = new Rectangle(10, 24, Math.Max(160, row.Width - 20), 14),
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                EventHandler click = async (_, _) =>
                {
                    _host.Text = item.Host;
                    _port.Value = Math.Clamp(item.Port, (int)_port.Minimum, (int)_port.Maximum);
                    if (_client == null) await ToggleConnectionAsync();
                };
                row.Click += click; label.Click += click; sub.Click += click;
                row.Resize += (_, _) => { label.Width = Math.Max(160, row.ClientSize.Width - 20); sub.Width = label.Width; };
                row.Controls.Add(sub); row.Controls.Add(label); host.Controls.Add(row);
                y += 46;
            }
        }

        if (favorites)
        {
            var add = new Label
            {
                Text = "★  Mevcut bağlantıyı favorilere ekle",
                ForeColor = Color.FromArgb(210, 220, 245),
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                Bounds = new Rectangle(0, y, Math.Max(180, host.ClientSize.Width - 4), 34),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(9, 24, 42),
                Cursor = Cursors.Hand
            };
            add.Click += (_, _) =>
            {
                string hostText = _host.Text.Trim();
                int port = (int)_port.Value;
                if (string.IsNullOrWhiteSpace(hostText)) return;
                var favs = LoadFavorites();
                favs.RemoveAll(x => string.Equals(x.Host, hostText, StringComparison.OrdinalIgnoreCase) && x.Port == port);
                favs.Insert(0, new SavedConnection(hostText, port, DateTime.UtcNow));
                SaveFavorites(favs);
                RefreshQuickAccessPanels();
                    };
            host.Controls.Add(add);
        }
        host.ResumeLayout();
    }

    private void UpdateConnectionHeader(bool connected)
    {
        _statusBadge.Text = connected ? "●  BAĞLI" : "●  BEKLENİYOR";
        _statusBadge.BackColor = connected ? Color.FromArgb(7, 55, 44) : Color.FromArgb(8, 35, 50);
        _statusBadge.ForeColor = connected ? Color.FromArgb(95, 245, 175) : Color.FromArgb(92, 220, 255);
        _statusBadge.Invalidate();
    }

    private void UpdateFullscreenChromeVisibility()
    {
        if (IsDisposed || Disposing) return;
    }


    private void LayoutFullscreenSurface()
    {
        if (!_isFullscreen) return;

        // The Designer already owns the Fill/Dock hierarchy. Keep it untouched at
        // runtime; only the shell chrome is hidden during fullscreen. Re-layout and
        // restore z-order so the overlay controls remain interactive.
        _view.BringToFront();
        _fullscreenDisconnectButton.BringToFront();
        _fullscreenExitButton.BringToFront();
        _viewPanel.PerformLayout();
        _displayPage.PerformLayout();
        _contentPanel.PerformLayout();
        LayoutFullscreenToolbar();
    }

    private void SelectTab(int index)
    {
        _tabConnection.Selected = index == 0;
        _tabDisplay.Selected = index == 1;
        _tabSettings.Selected = index == 2;
        _tabAbout.Selected = index == 3;
        _tabConnection.Invalidate();
        _tabDisplay.Invalidate();
        _tabSettings.Invalidate();
        _tabAbout.Invalidate();

        bool connection = index == 0;
        bool display = index == 1;
        bool settings = index == 2;
        bool connected = _client is not null;

        // Pages are siblings in the shell.  Visibility, rather than Z-order,
        // determines the active menu.
        _connectionPage.Visible = connection;
        if (connection)
        {
            _quickPanel.Visible = true;
            RefreshQuickAccessPanels();
        }
        _displayPage.Visible = display;
        _settingsPage.Visible = settings;
        _aboutPage.Visible = index == 3;
        _viewPanel.Visible = connected && display;
        _displayPanel.Visible = false;
        _statusPanel.Visible = display && !connected;
        _fullscreenDisconnectButton.Visible = false;
        _fullscreenExitButton.Visible = false;
        _cardConnect.Visible = connection;
        _cardConnect.Text = connected ? "BAĞLANTIYI KES" : "BAĞLAN";
        // AboutPage content is Designer-owned. Do not overwrite its Text values here;
        // otherwise Visual Studio Designer changes are invisible at runtime.
    }

    private sealed record SavedConnection(string Host, int Port, DateTime LastUsedUtc);
    private static string ConnectionStorePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TRPolaris", "ClientConnections.json");

    private static List<SavedConnection> LoadConnections()
    {
        try
        {
            if (!File.Exists(ConnectionStorePath)) return new List<SavedConnection>();
            return JsonSerializer.Deserialize<List<SavedConnection>>(File.ReadAllText(ConnectionStorePath)) ?? new List<SavedConnection>();
        }
        catch { return new List<SavedConnection>(); }
    }

    private static void SaveConnections(List<SavedConnection> list)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConnectionStorePath)!);
            File.WriteAllText(ConnectionStorePath, JsonSerializer.Serialize(list.Take(20).ToList(), new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    private void RememberConnection()
    {
        string host = _host.Text.Trim();
        int port = (int)_port.Value;
        if (string.IsNullOrWhiteSpace(host)) return;
        var list = LoadConnections();
        list.RemoveAll(x => string.Equals(x.Host, host, StringComparison.OrdinalIgnoreCase) && x.Port == port);
        list.Insert(0, new SavedConnection(host, port, DateTime.UtcNow));
        SaveConnections(list);
    }

    private static string FavoritesPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TRPolaris", "ClientFavorites.json");
    private static List<SavedConnection> LoadFavorites()
    {
        try { if (!File.Exists(FavoritesPath)) return new List<SavedConnection>(); return JsonSerializer.Deserialize<List<SavedConnection>>(File.ReadAllText(FavoritesPath)) ?? new List<SavedConnection>(); } catch { return new List<SavedConnection>(); }
    }
    private static void SaveFavorites(List<SavedConnection> list)
    {
        try { Directory.CreateDirectory(Path.GetDirectoryName(FavoritesPath)!); File.WriteAllText(FavoritesPath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true })); } catch { }
    }

    private void ShowSavedConnections(bool favorites) => ToggleQuickPanel(favorites ? 2 : 1);

    private void ClearRecentConnections()
    {
        try { if (File.Exists(ConnectionStorePath)) File.Delete(ConnectionStorePath); } catch { }
    }

    private void CreateServerScreenSelector()
    {
        _serverScreenSelect.SelectedIndexChanged += async (_, _) =>
        {
            if (_loadingServerScreens || _applyingServerScreenSelection || _serverScreenSelect.SelectedItem is not RemoteScreen screen)
                return;
            if (_client is null) return;
            _pendingScreenToken = Protocol.ScreenToken(screen.DeviceName);
            _serverScreenSelect.Enabled = false;
            _status.Text = $"{screen.Name} seçiliyor…";
            try
            {
                await SendScreenSelectionAsync(screen.DeviceName);
            }
            catch (Exception ex)
            {
                _pendingScreenToken = 0;
                _expectedScreenToken = string.IsNullOrWhiteSpace(_confirmedServerScreen)
                    ? (ushort)0
                    : Protocol.ScreenToken(_confirmedServerScreen);
                _serverScreenSelect.Enabled = true;
                _status.Text = $"Ekran değiştirilemedi: {ex.Message}";
            }
        };
    }


    private async Task LoadServerScreensAsync()
    {
        string host = NormalizeHttpHost(_host.Text);
        if (string.IsNullOrWhiteSpace(host) || _loadingServerScreens) return;
        _loadingServerScreens = true;
        try
        {
            Uri baseUri = new Uri(host);
            string screenApi = $"http://{baseUri.Host}:8765/api/screens";
            using JsonDocument doc = await ScreenHttp.GetFromJsonAsync<JsonDocument>(screenApi) ?? throw new InvalidOperationException();
            var list = new List<RemoteScreen>();
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                string deviceName = item.TryGetProperty("deviceName", out var d) ? d.GetString() ?? string.Empty : string.Empty;
                string name = item.TryGetProperty("name", out var n) ? n.GetString() ?? deviceName : deviceName;
                int width = item.TryGetProperty("width", out var w) ? w.GetInt32() : 0;
                int height = item.TryGetProperty("height", out var h) ? h.GetInt32() : 0;
                if (!string.IsNullOrWhiteSpace(deviceName)) list.Add(new RemoteScreen(deviceName, name, width, height));
            }

            if (_serverScreenSelect is null) return;
            string keep = _selectedServerScreen;
            _serverScreenSelect.BeginUpdate();
            _applyingServerScreenSelection = true;
            try
            {
                _serverScreenSelect.Items.Clear();
                foreach (var item in list) _serverScreenSelect.Items.Add(item);
                int idx = list.FindIndex(x => string.Equals(x.DeviceName, keep, StringComparison.OrdinalIgnoreCase));
                if (idx < 0 && list.Count > 0) idx = 0;
                if (idx >= 0) _serverScreenSelect.SelectedIndex = idx;
            }
            finally { _applyingServerScreenSelection = false; _serverScreenSelect.EndUpdate(); }
        }
        catch
        {
            // Web UI may be disabled or unreachable; normal TCP connection still works.
        }
        finally { _loadingServerScreens = false; }
    }

    private static string NormalizeHttpHost(string host)
    {
        host = host.Trim();
        if (string.IsNullOrWhiteSpace(host)) return string.Empty;
        if (!host.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            host = "http://" + host;
        return host.TrimEnd('/');
    }

    private Task SendScreenSelectionAsync(string deviceName) => SendRemoteCommandAsync($"POLARIS_SELECT|{deviceName}\n");





    private sealed record RemoteScreen(string DeviceName, string Name, int Width, int Height)
    {
        public override string ToString() => $"{Name} • {Width}×{Height}";
    }

    private void DragWindow(object? sender, MouseEventArgs e) { if (e.Button != MouseButtons.Left) return; ReleaseCapture(); SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero); }
    private bool IsDesignMode() => LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode;

    private void HandlePointerEnterView()
    {
        if (IsDisposed) return;
        if (_isFullscreen)
        {
            Point p = _view.PointToClient(Cursor.Position);
            if (p.Y <= 80) ShowFullscreenToolbar();
            if (p.Y >= Math.Max(0, _view.ClientSize.Height - 105)) ShowDisplayOverlay();
        }
        else if (_tabDisplay.Selected)
        {
            Point p = _view.PointToClient(Cursor.Position);
            if (_view.ClientRectangle.Contains(p) && p.Y >= Math.Max(0, _view.ClientSize.Height - 130))
                ShowDisplayOverlay();
        }
    }

    private void UpdateOverlayVisibility()
    {
        if (IsDisposed || Disposing) return;

        // The timer is only a visibility safety net. Do not let it change the
        // normal window chrome or fullscreen state. It only keeps the transient
        // viewer controls synchronized with the current pointer position.
        if (!_isFullscreen)
        {
            if (_fullscreenDisconnectButton.Visible) _fullscreenDisconnectButton.Visible = false;
            if (_fullscreenExitButton.Visible) _fullscreenExitButton.Visible = false;
            return;
        }

        Point p = _view.PointToClient(Cursor.Position);
        bool insideView = _view.ClientRectangle.Contains(p);
        bool inTopZone = insideView && p.Y <= 82;
        bool inBottomZone = insideView && p.Y >= Math.Max(0, _view.ClientSize.Height - 110);
        bool toolbarVisible = _fullscreenDisconnectButton.Visible || _fullscreenExitButton.Visible;

        if (inTopZone || inBottomZone)
        {
            ShowFullscreenToolbar();
            return;
        }

        if (!toolbarVisible) return;
        if (!_pointerInsideFullscreenToolbar)
        {
            _fullscreenDisconnectButton.Visible = false;
            _fullscreenExitButton.Visible = false;
        }
    }

    private void ShowFullscreenToolbar()
    {
        if (!_isFullscreen) return;
        _fullscreenDisconnectButton.Visible = true;
        _fullscreenExitButton.Visible = true;
        LayoutFullscreenToolbar();
        _fullscreenDisconnectButton.BringToFront();
        _fullscreenExitButton.BringToFront();
        ArmHoverHide();
    }

    private void ShowDisplayOverlay()
    {
        if (_client is null || !_tabDisplay.Selected) return;
        _displayPanel.Visible = true;
        _displayPanel.BringToFront();
        _serverScreenSelect.BringToFront();
        _pointerInsideDisplayOverlay = _displayPanel.RectangleToScreen(_displayPanel.ClientRectangle).Contains(Cursor.Position);
        ArmHoverHide();
    }

    private void FullscreenHeaderButton_Click(object? sender, EventArgs e)
    {
        ToggleFullscreen();
    }

    private void MinimizeButton_Click(object? sender, EventArgs e)
    {
        if (_isFullscreen) return;
        WindowState = FormWindowState.Minimized;
    }

    private void MaximizeButton_Click(object? sender, EventArgs e)
    {
        ToggleWindowMaximize();
    }

    private void CloseButton_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void ToggleWindowMaximize()
    {
        if (_isFullscreen) return;
        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;
        _maximizeButton.Glyph = WindowState == FormWindowState.Maximized ? "❐" : "□";
    }

    private void ArmHoverHide()
    {
        _hoverHideTimer.Stop();
        _hoverHideTimer.Start();
    }

}
