using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;

namespace InventorScreenshotTool;

internal static class RibbonIcons
{
    internal static Bitmap Load(int size)
    {
        // Resources are embedded: a user's installed add-in does not require
        // separate loose PNG files or any network connection.
        string name = $"InventorScreenshotTool.Assets.cube-focus-{size}.png";
        using Stream stream = typeof(RibbonIcons).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Missing embedded ribbon resource: {name}");
        using var decoded = new Bitmap(stream);
        if (size is 16 or 32)
        {
            // Autodesk Inventor's dark ribbon can render semi-transparent
            // PNG edges poorly through IPictureDisp. Supply an explicitly
            // opaque 24bpp white-backed image for its small and large icons.
            var opaque = new Bitmap(size, size, PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(opaque))
            {
                graphics.Clear(Color.White);
                graphics.DrawImageUnscaled(decoded, 0, 0);
            }
            return opaque;
        }

        // Keep the 48px header image transparent on the navy title banner.
        return new Bitmap(decoded);
    }

    // Microsoft.VisualBasic.Compatibility.VB6's IconToIPicture is unavailable
    // in .NET 8. AxHost provides the supported OLE IPictureDisp conversion.
    internal static object ToPictureDisp(Image image) => PictureHost.Convert(image);

    private sealed class PictureHost : AxHost
    {
        private PictureHost() : base("") { }

        internal static object Convert(Image image) => GetIPictureDispFromPicture(image);
    }
}
