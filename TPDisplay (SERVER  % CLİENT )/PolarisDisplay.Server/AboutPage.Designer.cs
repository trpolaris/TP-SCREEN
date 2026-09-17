#nullable disable
namespace PolarisDisplay.Server;

partial class AboutPage
{
    private System.ComponentModel.IContainer components = null;
    private NeonPanel _categoryPanel;
    private Label _categoryTitle;
    private Label _categoryText;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _categoryPanel = new NeonPanel();
        _categoryText = new Label();
        _categoryTitle = new Label();
        _categoryPanel.SuspendLayout();
        SuspendLayout();
        // 
        // _categoryPanel
        // 
        _categoryPanel.BackColor = Color.FromArgb(8, 18, 34);
        _categoryPanel.BorderColor = Color.FromArgb(0, 145, 235);
        _categoryPanel.BorderRadius = 12;
        _categoryPanel.BorderThickness = 1;
        _categoryPanel.Controls.Add(_categoryText);
        _categoryPanel.Controls.Add(_categoryTitle);
        _categoryPanel.Dock = DockStyle.Fill;
        _categoryPanel.Gradient = true;
        _categoryPanel.GradientEnd = Color.FromArgb(25, 14, 54);
        _categoryPanel.GradientStart = Color.FromArgb(7, 25, 49);
        _categoryPanel.Location = new Point(14, 14);
        _categoryPanel.Name = "_categoryPanel";
        _categoryPanel.Padding = new Padding(24);
        _categoryPanel.Size = new Size(1180, 648);
        _categoryPanel.TabIndex = 0;
        // 
        // _categoryText
        // 
        _categoryText.Dock = DockStyle.Fill;
        _categoryText.Font = new Font("Segoe UI", 10.5F);
        _categoryText.ForeColor = Color.FromArgb(155, 200, 235);
        _categoryText.Location = new Point(24, 66);
        _categoryText.Name = "_categoryText";
        _categoryText.Padding = new Padding(2, 10, 2, 2);
        _categoryText.Size = new Size(1526, 662);
        _categoryText.TabIndex = 0;
        _categoryText.Text = @"TRPOLARIS SERVER

TRPOLARIS Server, Windows üzerinde sanal ekranları yönetmek ve bu ekranları uzak istemcilere düşük gecikmeli ve güvenilir şekilde sunmak için geliştirilmiş profesyonel sanal ekran sunucusudur.

ÖNE ÇIKAN ÖZELLİKLER
• Windows Indirect Display Driver altyapısı
• Birden fazla sanal ekran desteği
• Düşük gecikmeli JPEG görüntü aktarımı
• TCP Client ve Web Viewer bağlantıları
• PIN tabanlı güvenli bağlantı
• Otomatik ekran ve sürücü yönetimi
• Çözünürlük, FPS, görüntü kalitesi ve bant genişliği kontrolü

TRPOLARIS Server; performans, kararlılık ve merkezi yönetim odaklı modern bir virtual display altyapısı sunar.

Sürüm: 1.0.0
TRPOLARIS — Virtual Display Infrastructure";
        // 
        // _categoryTitle
        // 
        _categoryTitle.Dock = DockStyle.Top;
        _categoryTitle.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
        _categoryTitle.ForeColor = Color.White;
        _categoryTitle.Location = new Point(24, 24);
        _categoryTitle.Name = "_categoryTitle";
        _categoryTitle.Size = new Size(1526, 42);
        _categoryTitle.TabIndex = 1;
        _categoryTitle.Text = "ⓘ  Hakkında  •  Sürüm 1.0.0";
        // 
        // AboutPage
        // 
        BackColor = Color.FromArgb(3, 8, 18);
        Controls.Add(_categoryPanel);
        Name = "AboutPage";
        Padding = new Padding(14);
        Size = new Size(1236, 704);
        _categoryPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
