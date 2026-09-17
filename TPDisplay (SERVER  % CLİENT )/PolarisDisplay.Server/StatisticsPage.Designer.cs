#nullable disable
namespace PolarisDisplay.Server;
partial class StatisticsPage
{
    private System.ComponentModel.IContainer components = null;
    private Label _title, _v1, _v2, _v3, _v4, _v5, _v6, _v7, _v8, _v9;
    private Label _l1, _l2, _l3, _l4, _l5, _l7, _l8, _l9, _telemetryTitle;
    protected override void Dispose(bool disposing){ if(disposing) components?.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _title=new Label(); _v1=new Label(); _v2=new Label(); _v3=new Label(); _v4=new Label(); _v5=new Label(); _v6=new Label(); _v7=new Label(); _v8=new Label(); _v9=new Label();
        _l1=new Label(); _l2=new Label(); _l3=new Label(); _l4=new Label(); _l5=new Label(); _l7=new Label(); _l8=new Label(); _l9=new Label(); _telemetryTitle=new Label();
        SuspendLayout();
        BackColor=Color.FromArgb(3,8,18); Padding=new Padding(16); Name="StatisticsPage";
        _title.Text="◌  Canlı İstatistikler"; _title.Dock=DockStyle.Top; _title.Height=32; _title.Font=new Font("Segoe UI Semibold",16F,FontStyle.Bold); _title.ForeColor=Color.White;
        _l1.Text="BAĞLI CLIENT"; _l1.Left=0; _l1.Top=54; _l1.AutoSize=true; _l1.ForeColor=Color.FromArgb(125,170,205); _l1.Font=new Font("Segoe UI",9F);
        _l2.Text="TOPLAM HIZ"; _l2.Left=190; _l2.Top=54; _l2.AutoSize=true; _l2.ForeColor=Color.FromArgb(125,170,205); _l2.Font=new Font("Segoe UI",9F);
        _l3.Text="UPTIME"; _l3.Left=380; _l3.Top=54; _l3.AutoSize=true; _l3.ForeColor=Color.FromArgb(125,170,205); _l3.Font=new Font("Segoe UI",9F);
        _l4.Text="CPU"; _l4.Left=570; _l4.Top=54; _l4.AutoSize=true; _l4.ForeColor=Color.FromArgb(125,170,205); _l4.Font=new Font("Segoe UI",9F);
        _l5.Text="RAM"; _l5.Left=0; _l5.Top=116; _l5.AutoSize=true; _l5.ForeColor=Color.FromArgb(125,170,205); _l5.Font=new Font("Segoe UI",9F);
        _l7.Text="PING ORT."; _l7.Left=190; _l7.Top=116; _l7.AutoSize=true; _l7.ForeColor=Color.FromArgb(125,170,205); _l7.Font=new Font("Segoe UI",9F);
        _l8.Text="MAKS. ÇÖZÜNÜRLÜK"; _l8.Left=380; _l8.Top=116; _l8.AutoSize=true; _l8.ForeColor=Color.FromArgb(125,170,205); _l8.Font=new Font("Segoe UI",9F);
        _l9.Text="AKTİF MOD"; _l9.Left=570; _l9.Top=116; _l9.AutoSize=true; _l9.ForeColor=Color.FromArgb(125,170,205); _l9.Font=new Font("Segoe UI",9F);
        _v1.Left=0; _v1.Top=74; _v1.AutoSize=true; _v1.Font=new Font("Segoe UI Semibold",14F,FontStyle.Bold); _v1.ForeColor=Color.White;
        _v2.Left=190; _v2.Top=74; _v2.AutoSize=true; _v2.Font=new Font("Segoe UI Semibold",14F,FontStyle.Bold); _v2.ForeColor=Color.White;
        _v3.Left=380; _v3.Top=74; _v3.AutoSize=true; _v3.Font=new Font("Segoe UI Semibold",14F,FontStyle.Bold); _v3.ForeColor=Color.White;
        _v4.Left=570; _v4.Top=74; _v4.AutoSize=true; _v4.Font=new Font("Segoe UI Semibold",14F,FontStyle.Bold); _v4.ForeColor=Color.White;
        _v5.Left=0; _v5.Top=136; _v5.AutoSize=true; _v5.Font=new Font("Segoe UI Semibold",14F,FontStyle.Bold); _v5.ForeColor=Color.White;
        _v7.Left=190; _v7.Top=136; _v7.AutoSize=true; _v7.Font=new Font("Segoe UI Semibold",14F,FontStyle.Bold); _v7.ForeColor=Color.White;
        _v8.Left=380; _v8.Top=136; _v8.AutoSize=true; _v8.Font=new Font("Segoe UI Semibold",14F,FontStyle.Bold); _v8.ForeColor=Color.White;
        _v9.Left=570; _v9.Top=136; _v9.AutoSize=true; _v9.Font=new Font("Segoe UI Semibold",14F,FontStyle.Bold); _v9.ForeColor=Color.White;
        _telemetryTitle.Text="CLIENT TELEMETRİSİ"; _telemetryTitle.Left=0; _telemetryTitle.Top=190; _telemetryTitle.AutoSize=true; _telemetryTitle.ForeColor=Color.FromArgb(95,210,255); _telemetryTitle.Font=new Font("Segoe UI Semibold",10F,FontStyle.Bold);
        _v6.Left=0; _v6.Top=216; _v6.Width=900; _v6.Height=90; _v6.Font=new Font("Consolas",10F); _v6.ForeColor=Color.FromArgb(185,220,245); _v6.AutoSize=false;
        Controls.Add(_v6); Controls.Add(_telemetryTitle); Controls.Add(_v1); Controls.Add(_v2); Controls.Add(_v3); Controls.Add(_v4); Controls.Add(_v5); Controls.Add(_v7); Controls.Add(_v8); Controls.Add(_v9); Controls.Add(_l1); Controls.Add(_l2); Controls.Add(_l3); Controls.Add(_l4); Controls.Add(_l5); Controls.Add(_l7); Controls.Add(_l8); Controls.Add(_l9); Controls.Add(_title);
        ResumeLayout(false);
    }
}
