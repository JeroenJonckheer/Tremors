namespace Tremors;

/// <summary>User settings, stored as JSON in %AppData%\Tremors\settings.json.</summary>
public sealed class Settings
{
    public bool Enabled { get; set; } = true;
    public int Intensity { get; set; } = 70;
    public bool Wheel { get; set; } = true;
    public bool AutoStart { get; set; }
}
