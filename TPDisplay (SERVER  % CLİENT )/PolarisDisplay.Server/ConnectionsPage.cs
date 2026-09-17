namespace PolarisDisplay.Server;

/// <summary>Designer-owned connection history and active-client screen.</summary>
public sealed partial class ConnectionsPage : UserControl
{
    public ConnectionsPage()
    {
        InitializeComponent();
        _clearHistory.Visible = true;
        _clearHistory.BringToFront();
    }

    internal FlowLayoutPanel ConnectionsFlow => _connectionsFlow;
    internal Label ConnectionsEmpty => _connectionsEmpty;
    internal event EventHandler? ClearHistoryRequested;
    internal NeonButton ClearHistoryButton => _clearHistory;
    private void ClearHistoryClick(object? sender, EventArgs e) => ClearHistoryRequested?.Invoke(this, EventArgs.Empty);
}
