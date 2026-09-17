#nullable disable
namespace PolarisDisplay.Server;
partial class DisplayModeDialogForm
{
 private System.ComponentModel.IContainer components=null;private ComboBox _combo;private Label _info;private Button _apply,_close;
 protected override void Dispose(bool disposing){if(disposing)components?.Dispose();base.Dispose(disposing);}
 private void InitializeComponent(){components=new System.ComponentModel.Container();_combo=new ComboBox();_info=new Label();_apply=new Button();_close=new Button();SuspendLayout();Text="Polaris Ekran • Görüntü Modu";StartPosition=FormStartPosition.CenterParent;ClientSize=new Size(540,220);FormBorderStyle=FormBorderStyle.FixedDialog;MinimizeBox=false;MaximizeBox=false;BackColor=Color.FromArgb(7,18,31);ForeColor=Color.White;_info.Text="Sağ tık ile bu menüyü açabilirsiniz. Değişiklik Windows görüntü moduna uygulanır.";_info.Left=24;_info.Top=20;_info.AutoSize=true;_info.ForeColor=Color.FromArgb(155,190,215);_combo.Left=24;_combo.Top=55;_combo.Width=470;_combo.DropDownStyle=ComboBoxStyle.DropDownList;_combo.BackColor=Color.FromArgb(10,27,44);_combo.ForeColor=Color.White;_apply.Text="Uygula";_apply.Left=280;_apply.Top=150;_apply.Width=100;_apply.Click+=_apply_Click;_close.Text="Kapat";_close.Left=390;_close.Top=150;_close.Width=100;_close.DialogResult=DialogResult.Cancel;Controls.AddRange(new Control[]{_info,_combo,_apply,_close});CancelButton=_close;ResumeLayout(false);PerformLayout();}
}
