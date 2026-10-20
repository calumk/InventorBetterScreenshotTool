using System.Text.Json;

namespace InventorScreenshotTool;

public enum SaveLocationMode
{
    BesideDocument = 0,
    FixedFolder = 1
}

public sealed class ScreenshotOptions
{
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public SaveLocationMode LocationMode { get; set; } = SaveLocationMode.BesideDocument;
    public string FixedFolder { get; set; } = string.Empty;
}

public static class SettingsStore
{
    public static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "InventorScreenshotTool", "settings.json");

    public static ScreenshotOptions Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return new ScreenshotOptions();
            var value = JsonSerializer.Deserialize<ScreenshotOptions>(File.ReadAllText(SettingsPath));
            if (value is null) return new ScreenshotOptions();
            value.Width = Math.Clamp(value.Width, 256, 8192);
            value.Height = Math.Clamp(value.Height, 256, 8192);
            if (!Enum.IsDefined(value.LocationMode))
                value.LocationMode = SaveLocationMode.BesideDocument;
            value.FixedFolder ??= string.Empty;
            return value;
        }
        catch (Exception)
        {
            // A corrupt/unreadable preference file should not block capture.
            return new ScreenshotOptions();
        }
    }

    public static void Save(ScreenshotOptions options)
    {
        string path = SettingsPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        // Atomic replace prevents half-written JSON if Inventor exits unexpectedly.
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(options,
                new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temp, path, true);
        }
        finally
        {
            if (File.Exists(temp)) File.Delete(temp);
        }
    }
}
