using Microsoft.Win32;

namespace Tremors;

/// <summary>Start with Windows via the per-user Run key (no admin rights needed).</summary>
internal static class AutoStart
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Tremors";

    /// <summary>
    /// Added to the command line when Windows starts Tremors at sign-in, so it starts quietly
    /// in the notification area. Started by hand, Tremors opens its settings window instead.
    /// </summary>
    public const string Argument = "--autostart";

    public static void Enable()
    {
        if (Environment.ProcessPath is not { } exe) return;
        using var key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true);
        key.SetValue(ValueName, $"\"{exe}\" {Argument}");
    }

    /// <summary>True when Windows started Tremors at sign-in.</summary>
    public static bool StartedByWindows(IEnumerable<string> args) =>
        args.Any(a => string.Equals(a, Argument, StringComparison.OrdinalIgnoreCase));

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
