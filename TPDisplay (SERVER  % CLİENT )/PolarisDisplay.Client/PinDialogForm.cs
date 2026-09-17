namespace PolarisDisplay.Client;
internal sealed partial class PinDialogForm:Form
{
 public string Pin=>_box.Text;
 public PinDialogForm(){InitializeComponent(); PolarisTheme.Apply(this);}
 protected override void OnShown(EventArgs e){base.OnShown(e);try{Region=Region.FromHrgn(CreateRoundRectRgn(0,0,Width+1,Height+1,12,12));}catch{} _box.Focus();}
 [System.Runtime.InteropServices.DllImport("gdi32.dll",SetLastError=true)] private static extern IntPtr CreateRoundRectRgn(int l,int t,int r,int b,int w,int h);
 private void _close_Click(object? s,EventArgs e)=>DialogResult=DialogResult.Cancel;
 private void _ok_Click(object? s,EventArgs e)=>DialogResult=DialogResult.OK;
 private void _cancel_Click(object? s,EventArgs e)=>DialogResult=DialogResult.Cancel;
}
