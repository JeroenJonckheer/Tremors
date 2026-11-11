using System.Text.Json;

namespace Tremors;

/// <summary>Reads and writes <see cref="Settings"/> as JSON in %AppData%\Tremors\settings.json.</summary>
internal static class Store
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private static string Folder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tremors");

    private static string FilePath => Path.Combine(Folder, "settings.json");

    /// <summary>Loads the settings; defaults when the file is missing or unreadable.</summary>
    public static Settings Load(out bool firstRun)
    {
        firstRun = !File.Exists(FilePath);
        try
        {
            if (!firstRun && JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) is { } s)
            {
                s.Intensity = Math.Clamp(s.Intensity, 0, 100);
                return s;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
        }
        return new Settings();
    }

    public static void Save(Settings settings)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Settings are a convenience; Tremors keeps working without them.
        }
    }
}
