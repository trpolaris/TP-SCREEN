namespace PolarisDisplay.Client;

/// <summary>
/// Designer-owned display screen. The fullscreen controls are direct children
/// of VideoView so no opaque toolbar is painted behind them.
/// </summary>
public sealed partial class DisplayPage : UserControl
{
    public DisplayPage()
    {
        InitializeComponent();
    }

    internal Panel ViewPanel => _viewPanel;
    internal VideoView View => _view;
    internal NeonPanel DisplayPanel => _displayPanel;
    internal Label DisplayTitle => _displayTitle;
    internal NeonComboBox DisplayMode => _displayMode;
    internal NeonComboBox DisplayQuality => _displayQuality;
    internal NeonComboBox ServerScreenSelect => _serverScreenSelect;
    internal Label ServerScreenLabel => _serverScreenLabel;
    internal NeonPanel StatusPanel => _statusPanel;
    internal Label Status => _status;
    internal Label Stats => _stats;
    internal NeonButton FullscreenDisconnectButton => _fullscreenDisconnectButton;
    internal NeonButton FullscreenExitButton => _fullscreenExitButton;
}
