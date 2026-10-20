using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventorScreenshotTool;

/// <summary>
/// Always lays out the actual PNG within its bounds at paint time.
/// Resizing is handled by WinForms invalidation, not by one-off calculations
/// in Resize handlers. The background is deliberately NOT white.
/// </summary>
internal sealed class PreviewCanvas : Control
{
    private Bitmap? _image;

    public PreviewCanvas()
    {
        BackColor = Color.FromArgb(229, 234, 242);
        ForeColor = Color.FromArgb(101, 113, 132);
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        TabStop = false;
    }

    public bool HasImage => _image is not null;

    public void ReplaceImage(Bitmap newImage)
    {
        ArgumentNullException.ThrowIfNull(newImage);
        Bitmap? old = _image;
        _image = newImage;
        old?.Dispose();
        Invalidate();
    }

    public void ClearImage()
    {
        Bitmap? old = _image;
        _image = null;
        old?.Dispose();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);

        if (_image is null)
        {
            TextRenderer.DrawText(e.Graphics, "Preview will appear here",
                Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
            return;
        }

        // Leave room for the drop shadow and a deliberate grey margin.
        int inset = 28;
        int availW = Math.Max(1, ClientSize.Width - 2 * inset);
        int availH = Math.Max(1, ClientSize.Height - 2 * inset);
        double scale = Math.Min((double)availW / _image.Width,
                                (double)availH / _image.Height);
        int w = Math.Max(1, (int)Math.Floor(_image.Width * scale));
        int h = Math.Max(1, (int)Math.Floor(_image.Height * scale));
        int x = (ClientSize.Width - w) / 2;
        int y = (ClientSize.Height - h) / 2;
        var bounds = new Rectangle(x, y, w, h);

        using (var shadow = new SolidBrush(Color.FromArgb(38, 40, 55, 75)))
            e.Graphics.FillRectangle(shadow, new Rectangle(x + 5, y + 6, w, h));
        using (var paper = new SolidBrush(Color.White))
            e.Graphics.FillRectangle(paper, bounds);
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        e.Graphics.DrawImage(_image, bounds);
        using var edge = new Pen(Color.FromArgb(167, 179, 196), 1);
        e.Graphics.DrawRectangle(edge, x, y, Math.Max(0, w - 1), Math.Max(0, h - 1));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _image?.Dispose();
            _image = null;
        }
        base.Dispose(disposing);
    }
}
