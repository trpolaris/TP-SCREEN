namespace PolarisDisplay.Client;

/// <summary>
/// Designer-owned about screen.
/// </summary>
public sealed partial class AboutPage : UserControl
{
    public AboutPage()
    {
        InitializeComponent();
    }

    internal NeonPanel CategoryPanel => _categoryPanel;
    internal Label CategoryTitle => _categoryTitle;
    internal Label CategoryText => _categoryText;
}
