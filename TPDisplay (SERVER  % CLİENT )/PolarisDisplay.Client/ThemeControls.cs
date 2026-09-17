using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PolarisDisplay.Client;

public class NeonPanel : Panel
{
    [Category("Neon Theme")] public Color BorderColor { get; set; } = Color.FromArgb(0,150,255);
    [Category("Neon Theme")] public int BorderRadius { get; set; } = 12;
    [Category("Neon Theme")] public int BorderThickness { get; set; } = 1;
    [Category("Neon Theme")] public bool Gradient { get; set; } = true;
    [Category("Neon Theme")] public Color GradientStart { get; set; } = Color.FromArgb(9,23,45);
    [Category("Neon Theme")] public Color GradientEnd { get; set; } = Color.FromArgb(19,16,48);
    public NeonPanel() { DoubleBuffered=true; SetStyle(ControlStyles.ResizeRedraw|ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer,true); BackColor=Color.FromArgb(8,18,34); }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var r=ClientRectangle; if(r.Width<=0||r.Height<=0)return; r.Width=Math.Max(1,r.Width-1);r.Height=Math.Max(1,r.Height-1);
        using var path=Rounded(r,BorderRadius); using Brush brush = Gradient ? (Brush)new LinearGradientBrush(r,GradientStart,GradientEnd,0f) : new SolidBrush(BackColor);
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias; e.Graphics.FillPath(brush,path); using var pen=new Pen(BorderColor,BorderThickness); e.Graphics.DrawPath(pen,path);
    }
    protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);}
    protected override void OnResize(EventArgs e){base.OnResize(e);Invalidate();}
    internal static GraphicsPath Rounded(Rectangle r,int radius){int d=Math.Max(2,Math.Min(radius*2,Math.Min(r.Width,r.Height)));var p=new GraphicsPath();p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
}

public class NeonButton : Button
{
    [Category("Neon Theme")] public Color GradientStart {get;set;}=Color.FromArgb(0,111,255);
    [Category("Neon Theme")] public Color GradientEnd {get;set;}=Color.FromArgb(235,0,220);
    [Category("Neon Theme")] public Color HoverStart {get;set;}=Color.FromArgb(35,140,255);
    [Category("Neon Theme")] public Color HoverEnd {get;set;}=Color.FromArgb(255,30,225);
    [Category("Neon Theme")] public int BorderRadius {get;set;}=9;
    [Category("Neon Theme")] public bool UseBackColorFill {get;set;}=false;
    private bool hover;
    public NeonButton(){FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;ForeColor=Color.White;Cursor=Cursors.Hand;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);MouseEnter+=(_,_)=>{hover=true;Invalidate();};MouseLeave+=(_,_)=>{hover=false;Invalidate();};}
    protected override void OnPaint(PaintEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;var r=ClientRectangle;r.Inflate(-1,-1);using var path=NeonPanel.Rounded(r,BorderRadius);Color a=Enabled?(hover?HoverStart:GradientStart):Color.FromArgb(55,60,72);Color b=Enabled?(hover?HoverEnd:GradientEnd):Color.FromArgb(75,78,90);if(UseBackColorFill){using var solid=new SolidBrush(BackColor);e.Graphics.FillPath(solid,path);}else{using var brush=new LinearGradientBrush(r,a,b,0f);e.Graphics.FillPath(brush,path);}TextRenderer.DrawText(e.Graphics,Text,Font,r,ForeColor,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}
}

public class ChromeButton : Button
{
    [Category("Chrome")] public string Glyph { get; set; } = "—";
    [Category("Chrome")] public Color HoverBackColor { get; set; } = Color.FromArgb(34, 42, 58);
    private bool hover;

    public ChromeButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.FromArgb(5, 14, 27);
        ForeColor = Color.White;
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hover = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!ClientRectangle.Contains(e.Location))
        {
            hover = false;
            Invalidate();
        }
        else if (!hover)
        {
            hover = true;
            Invalidate();
        }
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        if (!Capture)
        {
            hover = false;
            Invalidate();
        }
        base.OnMouseCaptureChanged(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        // MouseLeave can be skipped by WinForms when another child/window
        // receives the pointer. Recalculate hover from the real cursor position
        // so the highlight can never remain visually stuck.
        hover = Enabled && ClientRectangle.Contains(PointToClient(Cursor.Position));
        using (var baseBrush = new SolidBrush(BackColor))
            e.Graphics.FillRectangle(baseBrush, ClientRectangle);
        if (hover)
        {
            using var hoverBrush = new SolidBrush(HoverBackColor);
            e.Graphics.FillRectangle(hoverBrush, ClientRectangle);
        }
        TextRenderer.DrawText(e.Graphics, Glyph, Font, ClientRectangle, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}

public class NeonTabButton : Control
{
    [Category("Neon Theme")] public bool Selected { get; set; }
    [Category("Neon Theme")] public Color SelectedStart { get; set; } = Color.FromArgb(82, 28, 220);
    [Category("Neon Theme")] public Color SelectedEnd { get; set; } = Color.FromArgb(160, 20, 225);
    [Category("Neon Theme")] public Color NormalBack { get; set; } = Color.FromArgb(7, 20, 37);
    [Category("Neon Theme")] public Color NormalFore { get; set; } = Color.FromArgb(165, 205, 235);
    private bool hover;
    public NeonTabButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = NormalBack;
        ForeColor = NormalFore;
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI", 10.5F);
        TabStop = false;
        MouseEnter += (_, _) => { hover = true; Invalidate(); };
        MouseLeave += (_, _) => { hover = false; Invalidate(); };
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var r = ClientRectangle;
        if (r.Width <= 4 || r.Height <= 4) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var fill = Selected ? Color.FromArgb(9, 39, 63) : (hover ? Color.FromArgb(7, 27, 45) : NormalBack);
        r.Inflate(-2, -2);
        using var path = NeonPanel.Rounded(r, Math.Min(12, r.Height / 2));
        using (var brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, path);
        using (var pen = new Pen(Selected ? Color.FromArgb(24, 137, 205) : Color.FromArgb(10, 42, 65), 1))
            e.Graphics.DrawPath(pen, path);
        if (Selected)
        {
            using var accentPath = NeonPanel.Rounded(new Rectangle(r.X, r.Y, 4, r.Height), 2);
            using var accent = new LinearGradientBrush(new Rectangle(r.X, r.Y, 4, Math.Max(1, r.Height)), Color.FromArgb(78, 232, 255), Color.FromArgb(18, 126, 255), 90f);
            e.Graphics.FillPath(accent, accentPath);
        }
        var textRect = new Rectangle(22, 0, Math.Max(10, r.Width - 32), r.Height);
        TextRenderer.DrawText(e.Graphics, Text ?? string.Empty, Font, textRect, Selected ? Color.White : NormalFore, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }
}

public class NeonComboBox : ComboBox
{
    public NeonComboBox()
    {
        DrawMode=DrawMode.OwnerDrawFixed; ItemHeight=34; DropDownStyle=ComboBoxStyle.DropDownList;
        BackColor=Color.FromArgb(7,18,31); ForeColor=Color.White; FlatStyle=FlatStyle.Flat;
        Font=new Font("Segoe UI",10.5F); Cursor=Cursors.Hand; DropDownHeight=260;
        SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer,true);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var r=ClientRectangle; if(r.Width<=1||r.Height<=1)return;
        r.Inflate(-1,-1);
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using var path=NeonPanel.Rounded(r,9);
        using var bg=new SolidBrush(Color.FromArgb(7,18,31)); e.Graphics.FillPath(bg,path);
        using var pen=new Pen(Focused ? Color.FromArgb(225,20,220) : Color.FromArgb(0,190,255),2F); e.Graphics.DrawPath(pen,path);
        string text=SelectedIndex>=0 ? (GetItemText(Items[SelectedIndex]) ?? string.Empty) : string.Empty;
        TextRenderer.DrawText(e.Graphics,text,Font,new Rectangle(12,0,Math.Max(10,r.Width-46),r.Height),Color.White,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        Point[] arrow={new Point(r.Right-22,r.Height/2-3),new Point(r.Right-12,r.Height/2-3),new Point(r.Right-17,r.Height/2+4)};
        using var b=new SolidBrush(Color.FromArgb(85,205,255)); e.Graphics.FillPolygon(b,arrow);
    }
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if(e.Index<0) return;
        e.DrawBackground();
        using var b=new SolidBrush((e.State & DrawItemState.Selected)!=0 ? Color.FromArgb(50,28,105) : Color.FromArgb(7,18,31));
        e.Graphics.FillRectangle(b,e.Bounds);
        using var p=new Pen(Color.FromArgb(22,75,120)); e.Graphics.DrawRectangle(p,new Rectangle(e.Bounds.X,e.Bounds.Y,e.Bounds.Width-1,e.Bounds.Height-1));
        TextRenderer.DrawText(e.Graphics,GetItemText(Items[e.Index]),Font,new Rectangle(e.Bounds.X+10,e.Bounds.Y,e.Bounds.Width-20,e.Bounds.Height),Color.White,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
    }
}

public class NeonTextBox : TextBox
{
    [Category("Neon Theme")] public Color BorderColor { get; set; } = Color.FromArgb(0, 190, 255);
    [Category("Neon Theme")] public Color FocusBorderColor { get; set; } = Color.FromArgb(225, 20, 220);
    [Category("Neon Theme")] public int BorderRadius { get; set; } = 9;
    [Category("Neon Theme")] public int BorderThickness { get; set; } = 1;

    private Color _requestedBackColor = Color.Transparent;

    // WinForms TextBox does not natively support BackColor=Transparent.
    // Keep Transparent as the Designer-facing value, but paint the edit
    // surface with the nearest opaque parent color so startup never throws.
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public override Color BackColor
    {
        get => _requestedBackColor;
        set
        {
            _requestedBackColor = value;
            base.BackColor = value == Color.Transparent ? ResolveOpaqueParentBackColor() : value;
            Invalidate();
        }
    }

    public NeonTextBox()
    {
        BorderStyle = BorderStyle.None;
        _requestedBackColor = Color.Transparent;
        base.BackColor = Color.FromArgb(8, 18, 34);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10.5F);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        Padding = new Padding(10, 2, 10, 2);
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        if (_requestedBackColor == Color.Transparent)
            base.BackColor = ResolveOpaqueParentBackColor();
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (_requestedBackColor == Color.Transparent)
            base.BackColor = ResolveOpaqueParentBackColor();
        UpdateRegion();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRegion();
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        const int WM_NCPAINT = 0x0085;
        const int WM_SETFOCUS = 0x0007;
        const int WM_KILLFOCUS = 0x0008;
        if (m.Msg != WM_NCPAINT && m.Msg != WM_SETFOCUS && m.Msg != WM_KILLFOCUS) return;

        using Graphics g = Graphics.FromHwnd(Handle);
        Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = NeonPanel.Rounded(r, BorderRadius);
        using var pen = new Pen(Focused ? FocusBorderColor : BorderColor, Math.Max(1, BorderThickness));
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.DrawPath(pen, path);
    }

    private Color ResolveOpaqueParentBackColor()
    {
        Control? current = Parent;
        while (current is not null)
        {
            if (current.BackColor != Color.Transparent)
                return current.BackColor;
            current = current.Parent;
        }
        return Color.FromArgb(8, 18, 34);
    }

    private void UpdateRegion()
    {
        // Keep the native TextBox rectangular so its text/caret rendering remains stable.
        // The rounded neon border is drawn in WndProc instead of using a Region.
        Region = null;
    }
}
