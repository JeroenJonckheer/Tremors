using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Tremors;

/// <summary>Who made Tremors, which version this is and where to get the latest one.</summary>
internal static class Maker
{
    public const string Name = "Jeroen Jonckheer";
    public const string LatestUrl = "https://github.com/JeroenJonckheer/Tremors/releases/latest";

    /// <summary>The version of this build, such as "1.1.1".</summary>
    public static string Version { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "";

    /// <summary>The month this build was made, from the BuildMonth metadata in Tremors.csproj.</summary>
    public static DateTime? BuildMonth { get; } = ReadBuildMonth();

    /// <summary>Opens the download page of the latest version in the browser.</summary>
    public static void OpenLatest()
    {
        try
        {
            Process.Start(new ProcessStartInfo(LatestUrl) { UseShellExecute = true });
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // No browser is set up; nothing else to do.
        }
    }

    private static DateTime? ReadBuildMonth()
    {
        string? value = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "BuildMonth")?.Value;
        return DateTime.TryParseExact(value, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month)
            ? month : null;
    }
}
