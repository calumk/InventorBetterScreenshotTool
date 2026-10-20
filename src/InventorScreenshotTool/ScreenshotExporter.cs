using System.Drawing;
using System.Drawing.Drawing2D;

namespace InventorScreenshotTool;

public static class ScreenshotExporter
{
    public static string Capture(Inventor.Application app, ScreenshotOptions options)
    {
        ValidateDimensions(options);
        string modelPath = GetSavedModelPath(app);
        string directory = options.LocationMode switch
        {
            SaveLocationMode.BesideDocument =>
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(modelPath)!, "Screenshots"),
            SaveLocationMode.FixedFolder when !string.IsNullOrWhiteSpace(options.FixedFolder)
                && System.IO.Path.IsPathFullyQualified(options.FixedFolder) => options.FixedFolder,
            _ => throw new InvalidOperationException("Choose a valid absolute output folder.")
        };

        Directory.CreateDirectory(directory);
        string stem = System.IO.Path.GetFileNameWithoutExtension(modelPath);
        string temp = System.IO.Path.Combine(directory, ".BetterScreenshot_" + Guid.NewGuid().ToString("N") + ".png");
        try
        {
            RenderImage(app, temp, options.Width, options.Height);
            if (!System.IO.File.Exists(temp) || new FileInfo(temp).Length == 0)
                throw new IOException("Inventor did not create an image.");

            for (int n = 1; n <= 9999; n++)
            {
                string result = System.IO.Path.Combine(directory, $"{stem}_{n:0000}.png");
                try
                {
                    System.IO.File.Move(temp, result); // no overwrite
                    return result;
                }
                catch (IOException) when (System.IO.File.Exists(result)) { }
            }
            throw new IOException("This model already has 9,999 numbered screenshots.");
        }
        finally
        {
            if (System.IO.File.Exists(temp)) System.IO.File.Delete(temp);
        }
    }

    /// <summary>Renders a small throwaway image with the same Inventor camera settings.</summary>
    public static Bitmap CreatePreview(Inventor.Application app, ScreenshotOptions options)
    {
        ValidateDimensions(options);
        _ = GetSavedModelPath(app); // same document precondition as Save PNG
        double scale = Math.Min(1.0, Math.Min(960.0 / options.Width, 620.0 / options.Height));
        int width = Math.Max(1, (int)Math.Round(options.Width * scale));
        int height = Math.Max(1, (int)Math.Round(options.Height * scale));
        string temp = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
            "BetterScreenshot_preview_" + Guid.NewGuid().ToString("N") + ".png");
        try
        {
            RenderImage(app, temp, width, height);
            if (!System.IO.File.Exists(temp)) throw new IOException("Inventor did not create a preview image.");
            return LoadThumbnail(temp, 960, 620);
        }
        finally
        {
            if (System.IO.File.Exists(temp)) System.IO.File.Delete(temp);
        }
    }

    /// <summary>Read an already-saved PNG without leaving a file lock.</summary>
    public static Bitmap LoadThumbnail(string path, int maxWidth, int maxHeight)
    {
        using var source = Image.FromFile(path);
        double scale = Math.Min(1.0, Math.Min((double)maxWidth / source.Width, (double)maxHeight / source.Height));
        int width = Math.Max(1, (int)Math.Round(source.Width * scale));
        int height = Math.Max(1, (int)Math.Round(source.Height * scale));
        var result = new Bitmap(width, height);
        try
        {
            using Graphics g = Graphics.FromImage(result);
            g.Clear(System.Drawing.Color.White);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(source, 0, 0, width, height);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    private static void RenderImage(Inventor.Application app, string path, int width, int height)
    {
        // Same white-background pipeline for preview and final export.
        // API and renderer calls MUST remain on Inventor's UI thread.
        var schemes = app.ColorSchemes;
        Inventor.BackgroundTypeEnum before = schemes.BackgroundType;
        bool changed = before != Inventor.BackgroundTypeEnum.kOneColorBackgroundType;
        try
        {
            if (changed) schemes.BackgroundType = Inventor.BackgroundTypeEnum.kOneColorBackgroundType;
            Inventor.Color white = app.TransientObjects.CreateColor(255, 255, 255);
            Inventor.Camera camera = app.ActiveView.Camera;
            camera.SaveAsBitmap(path, width, height, white, white);
        }
        finally
        {
            if (changed) schemes.BackgroundType = before;
        }
    }

    private static void ValidateDimensions(ScreenshotOptions options)
    {
        if (options.Width is < 256 or > 8192 || options.Height is < 256 or > 8192)
            throw new ArgumentOutOfRangeException(nameof(options), "Dimensions must be between 256 and 8192 pixels.");
    }

    private static string GetSavedModelPath(Inventor.Application app)
    {
        Inventor.Document? document = app.ActiveDocument;
        if (document is null ||
            (document.DocumentType != Inventor.DocumentTypeEnum.kPartDocumentObject &&
             document.DocumentType != Inventor.DocumentTypeEnum.kAssemblyDocumentObject))
            throw new InvalidOperationException("Open a part (.ipt) or assembly (.iam) first.");
        string path = document.FullFileName;
        if (string.IsNullOrWhiteSpace(path) || !System.IO.Path.IsPathFullyQualified(path) || !System.IO.File.Exists(path))
            throw new InvalidOperationException("Save the part or assembly to disk first.");
        return path;
    }
}
