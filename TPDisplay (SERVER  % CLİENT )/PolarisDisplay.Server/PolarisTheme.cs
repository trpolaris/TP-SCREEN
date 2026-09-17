using System.Drawing.Drawing2D;

namespace PolarisDisplay.Server;

internal static class PolarisTheme
{
    internal static readonly Color Background = Color.FromArgb(3, 8, 18);
    internal static readonly Color Surface = Color.FromArgb(7, 18, 31);
    internal static readonly Color Surface2 = Color.FromArgb(9, 25, 43);
    internal static readonly Color Border = Color.FromArgb(20, 83, 125);
    internal static readonly Color BorderBright = Color.FromArgb(48, 190, 245);
    internal static readonly Color Cyan = Color.FromArgb(74, 218, 255);
    internal static readonly Color Blue = Color.FromArgb(24, 126, 255);
    internal static readonly Color Violet = Color.FromArgb(117, 76, 255);
    internal static readonly Color Text = Color.FromArgb(220, 239, 250);
    internal static readonly Color Muted = Color.FromArgb(126, 166, 193);

    internal static void Apply(Control root)
    {
        // Static UI properties are owned by the WinForms Designer. Runtime
        // styling must never overwrite Designer geometry, colors, fonts,
        // anchors, docking or custom-control properties.
        if (root is Form form)
        {
            form.Paint -= PaintForm;
            form.Paint += PaintForm;
        }
    }

    private static void PaintForm(object? sender, PaintEventArgs e)
    {
        if (sender is not Form form || form.ClientSize.Width <= 0 || form.ClientSize.Height <= 0) return;
        var r = form.ClientRectangle;
        using var brush = new LinearGradientBrush(r, Color.FromArgb(3, 8, 18), Color.FromArgb(5, 18, 32), 25f);
        e.Graphics.FillRectangle(brush, r);
        using var glow = new PathGradientBrush(new PointF[]
        {
            new(0, 0), new(Math.Max(1, r.Width * .42f), 0), new(Math.Max(1, r.Width * .62f), Math.Max(1, r.Height * .20f)),
            new(Math.Max(1, r.Width * .25f), Math.Max(1, r.Height * .32f))
        })
        {
            CenterColor = Color.FromArgb(18, 20, 93),
            SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0) }
        };
        e.Graphics.FillRectangle(glow, new Rectangle(0, 0, Math.Max(1, r.Width / 2), Math.Max(1, r.Height / 2)));
    }
}
