using Microsoft.Win32;

namespace Tremors;

/// <summary>Start with Windows via the per-user Run key (no admin rights needed).</summary>
internal static class AutoStart
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Tremors";

    public static void Enable()
    {
        if (Environment.ProcessPath is not { } exe) return;
        using var key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true);
        key.SetValue(ValueName, $"\"{exe}\"");
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
