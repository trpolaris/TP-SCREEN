using System.Drawing.Drawing2D;
namespace PolarisDisplay.Server;
internal sealed partial class CloseChoiceDialogForm:Form
{
 public CloseChoiceDialogForm(){InitializeComponent(); PolarisTheme.Apply(this);}
 protected override void OnShown(EventArgs e){base.OnShown(e);try{Region=Region.FromHrgn(CreateRoundRectRgn(0,0,Width+1,Height+1,12,12));}catch{} }
 [System.Runtime.InteropServices.DllImport("gdi32.dll",SetLastError=true)] private static extern IntPtr CreateRoundRectRgn(int l,int t,int r,int b,int w,int h);
 private void _close_Click(object? s,EventArgs e)=>DialogResult=DialogResult.Cancel;
 private void _yes_Click(object? s,EventArgs e)=>DialogResult=DialogResult.Yes;
 private void _no_Click(object? s,EventArgs e)=>DialogResult=DialogResult.No;
}
