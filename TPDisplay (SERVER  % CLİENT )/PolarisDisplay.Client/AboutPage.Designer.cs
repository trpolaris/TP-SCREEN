#nullable disable
namespace PolarisDisplay.Client;

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
        _categoryTitle = new Label();
        _categoryText = new Label();
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
        _categoryPanel.Size = new Size(1010, 610);
        _categoryPanel.TabIndex = 0;
        //
        // _categoryTitle
        //
        _categoryTitle.Dock = DockStyle.Top;
        _categoryTitle.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
        _categoryTitle.ForeColor = Color.White;
        _categoryTitle.Location = new Point(24, 24);
        _categoryTitle.Name = "_categoryTitle";
        _categoryTitle.Size = new Size(852, 44);
        _categoryTitle.TabIndex = 1;
        _categoryTitle.Text = "ⓘ  Hakkında  •  Sürüm 1.0.0";
        _categoryTitle.TextAlign = ContentAlignment.MiddleLeft;
        //
        // _categoryText
        //
        _categoryText.Dock = DockStyle.Fill;
        _categoryText.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
        _categoryText.ForeColor = Color.FromArgb(170, 205, 232);
        _categoryText.Location = new Point(24, 68);
        _categoryText.Name = "_categoryText";
        _categoryText.Padding = new Padding(2, 14, 2, 2);
        _categoryText.Size = new Size(852, 528);
        _categoryText.TabIndex = 0;
         _categoryText.Text = @"TRPOLARIS CLIENT

TRPOLARIS Client, TRPOLARIS Server tarafından sunulan sanal ekranları ağ üzerinden güvenilir ve düşük gecikmeli şekilde görüntülemek için geliştirilmiş Windows istemcisidir.

ÖNE ÇIKAN ÖZELLİKLER
• Sanal ekran seçimi ve uzak ekran yönetimi
• Düşük gecikmeli JPEG görüntü aktarımı
• Otomatik yeniden bağlantı desteği
• PIN tabanlı güvenli bağlantı
• Tam ekran görüntüleme
• Ekran yönü ve görüntü modu desteği
• Bağlantı ve performans bilgileri

TRPOLARIS Client; sade kullanım, kararlı bağlantı ve yüksek performans hedefleriyle, TRPOLARIS Server altyapısının uzak görüntüleme bileşeni olarak tasarlanmıştır.

Sürüm: 1.0.0
TRPOLARIS — Virtual Display Infrastructure";
        //
        // AboutPage
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(3, 8, 18);
        Controls.Add(_categoryPanel);
        Dock = DockStyle.Fill;
        Name = "AboutPage";
        Padding = new Padding(14);
        Size = new Size(1058, 672);
        _categoryPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
