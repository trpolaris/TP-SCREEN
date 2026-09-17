namespace PolarisDisplay.Client;

/// <summary>
/// Designer-owned connection and quick-access screen.
/// Keep layout and visual changes in ConnectionPage.Designer.cs.
/// </summary>
public sealed partial class ConnectionPage : UserControl
{
    internal Panel RecentListPanel => _recentListPanel;
    internal Panel FavoritesListPanel => _favoritesListPanel;
    public ConnectionPage()
    {
        InitializeComponent();
    }

    internal NeonPanel ConnectPanel => _connectPanel;
    internal Label ConnectTitle => _connectTitle;
    internal Panel ConnectionBody => _connectionBody;
    internal Label CardTitle => _cardTitle;
    internal Label CardSub => _cardSub;
    internal NeonButton CardConnect => _cardConnect;
    internal Label IpLabel => _ipLabel;
    internal Label PortLabel => _portLabel;
    internal NeonTextBox Host => _host;
    internal NumericUpDown Port => _port;
    internal NeonPanel QuickPanel => _quickPanel;
    internal Panel QuickBody => _quickBody;
    internal Button QuickRecent => _quickRecent;
    internal Button QuickFavorites => _quickFavorites;
    internal Button QuickClear => _quickClear;

    private void _connectionBody_Paint(object sender, PaintEventArgs e)
    {

    }
}
