namespace Tremors;

/// <summary>User settings, stored as JSON in %AppData%\Tremors\settings.json.</summary>
public sealed class Settings
{
    public const int MinPointerSpeed = 10;
    public const int MaxStillZone = 30;

    public bool Enabled { get; set; } = true;

    /// <summary>Filter strength, 0 (off) to 100.</summary>
    public int Intensity { get; set; } = 70;

    /// <summary>Hand-to-cursor speed in percent, <see cref="MinPointerSpeed"/> to 100.</summary>
    public int PointerSpeed { get; set; } = 100;

    /// <summary>Distance in pixels the hand can wander before the cursor follows, 0 to <see cref="MaxStillZone"/>.</summary>
    public int StillZone { get; set; } = 4;

    public bool Wheel { get; set; } = true;

    public bool AutoStart { get; set; }

    /// <summary>Brings values from an edited or older file back into range.</summary>
    public void Normalize()
    {
        Intensity = Math.Clamp(Intensity, 0, 100);
        PointerSpeed = Math.Clamp(PointerSpeed, MinPointerSpeed, 100);
        StillZone = Math.Clamp(StillZone, 0, MaxStillZone);
    }
}
