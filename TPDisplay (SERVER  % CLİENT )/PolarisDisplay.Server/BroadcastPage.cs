namespace PolarisDisplay.Server;

/// <summary>Designer-owned broadcast configuration and statistics screen.</summary>
public sealed partial class BroadcastPage : UserControl
{
    public BroadcastPage() => InitializeComponent();

    internal NeonPanel SettingsCard => _settingsCard;
    internal Label SettingsTitle => _settingsTitle;
    internal NumericUpDown Port => _port;
    internal NumericUpDown Fps => _fps;
    internal NumericUpDown Quality => _quality;
    internal NumericUpDown Bitrate => _bitrate;
    internal NeonComboBox Resolution => _resolution;
    internal NeonComboBox BitrateMode => _bitrateMode;
    internal NeonComboBox Screen => _screen;
    internal Button RefreshButton => _refresh;
    internal NeonButton StartButton => _start;
    internal Label Status => _status;
    internal NeonPanel StatsPanel => _statsPanel;
    internal Label M1Value => _m1Value;
    internal Label M2Value => _m2Value;
    internal Label M3Value => _m3Value;
    internal Label M4Value => _m4Value;
    internal Label M5Value => _m5Value;
    internal void SetActiveScreens(IReadOnlyList<System.Windows.Forms.Screen> screens)
    {
        var cards = new[] { _screen1, _screen2, _screen3 };
        var titles = new[] { _screen1Title, _screen2Title, _screen3Title };
        var subs = new[] { _screen1Sub, _screen2Sub, _screen3Sub };
        for (int i = 0; i < cards.Length; i++)
        {
            bool active = i < screens.Count;
            cards[i].Visible = active;
            if (active)
            {
                var screen = screens[i];
                titles[i].Text = $"●  POLARIS-{i + 1}";
                subs[i].Text = $"{screen.Bounds.Width} × {screen.Bounds.Height}   •   60 Hz";
            }
        }
        _screenGrid.ColumnStyles.Clear();
        int count = Math.Max(1, Math.Min(3, screens.Count));
        _screenGrid.ColumnCount = count;
        for (int i = 0; i < count; i++)
            _screenGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / count));
        _screenGrid.PerformLayout();
    }

}
