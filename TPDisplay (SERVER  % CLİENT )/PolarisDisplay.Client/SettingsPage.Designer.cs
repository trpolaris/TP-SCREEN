#nullable disable
namespace PolarisDisplay.Client;
partial class SettingsPage
{
 private System.ComponentModel.IContainer components=null; private Label _title,_info; private NeonButton _settings,_updates;
 protected override void Dispose(bool disposing){if(disposing)components?.Dispose();base.Dispose(disposing);}
 private void InitializeComponent(){components=new System.ComponentModel.Container();_title=new Label();_info=new Label();_settings=new NeonButton();_updates=new NeonButton();SuspendLayout();BackColor=Color.FromArgb(3,8,18);Padding=new Padding(28);
 _title.Text="⚙  Ayarlar";_title.Dock=DockStyle.Top;_title.Height=44;_title.Font=new Font("Segoe UI Semibold",16F,FontStyle.Bold);_title.ForeColor=Color.White;
 _info.Text="PIN, LAN keşfi, otomatik yeniden bağlanma ve güncelleme ayarlarını yönetin.";_info.Dock=DockStyle.Top;_info.Height=64;_info.Font=new Font("Segoe UI",10F);_info.ForeColor=Color.FromArgb(150,185,215);
 _settings.Text="AYARLAR";_settings.Location=new Point(28,125);_settings.Size=new Size(260,48);_settings.FlatStyle=FlatStyle.Flat;_settings.ForeColor=Color.White;_settings.Click+=OpenSettingsClick;
 _updates.Text="GÜNCELLEME KONTROLÜ";_updates.Location=new Point(304,125);_updates.Size=new Size(260,48);_updates.FlatStyle=FlatStyle.Flat;_updates.ForeColor=Color.White;_updates.Click+=CheckUpdatesClick;
 Controls.Add(_updates);Controls.Add(_settings);Controls.Add(_info);Controls.Add(_title);Name="SettingsPage";ResumeLayout(false);}
}
