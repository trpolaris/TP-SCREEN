namespace PolarisDisplay.Client;
public sealed partial class SettingsPage : UserControl
{
 public event EventHandler? OpenSettings; public event EventHandler? CheckUpdates;
 public SettingsPage(){InitializeComponent();}
 private void OpenSettingsClick(object? s,EventArgs e)=>OpenSettings?.Invoke(this,EventArgs.Empty);
 private void CheckUpdatesClick(object? s,EventArgs e)=>CheckUpdates?.Invoke(this,EventArgs.Empty);
}
