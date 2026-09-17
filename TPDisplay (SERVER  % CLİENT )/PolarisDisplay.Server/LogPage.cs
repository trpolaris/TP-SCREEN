namespace PolarisDisplay.Server;

/// <summary>Designer-owned server log screen.</summary>
public sealed partial class LogPage : UserControl
{
    public LogPage() => InitializeComponent();

    internal RichTextBox Log => _log;
}
