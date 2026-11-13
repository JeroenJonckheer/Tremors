using System.Text.Json.Serialization;

namespace Tremors;

/// <summary>User settings, stored as JSON in %AppData%\Tremors\settings.json.</summary>
public sealed class Settings
{
    public const int MinPointerSpeed = 10;
    public const int MaxStillZone = 30;
    public const int MaxClickHold = 500;
    public const int MaxShakeSlowdown = 90;

    public bool Enabled { get; set; } = true;

    /// <summary>Filter strength, 0 (off) to 100.</summary>
    public int Intensity { get; set; } = 70;

    /// <summary>
    /// Only read from older settings files: the pointer speed slider is gone, since Windows
    /// has the same setting. <see cref="Normalize"/> turns a lowered speed into the shake brake.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PointerSpeed { get; set; }

    /// <summary>Distance in pixels the hand can wander before the cursor follows, 0 to <see cref="MaxStillZone"/>.</summary>
    public int StillZone { get; set; } = 4;

    /// <summary>A click only counts when held this many milliseconds; 0 = every click counts.</summary>
    public int ClickHold { get; set; }

    public bool Wheel { get; set; } = true;

    /// <summary>Slow the cursor down while the hand shakes (see <see cref="ShakeBrake"/>).</summary>
    public bool ShakeBrake { get; set; }

    /// <summary>How much slower while shaking, in percent, 0 to <see cref="MaxShakeSlowdown"/>.</summary>
    public int ShakeSlowdown { get; set; } = 60;

    public bool AutoStart { get; set; }

    /// <summary>Interface language ("nl", "en", "fr", "de"); null follows Windows.</summary>
    public string? Language { get; set; }

    /// <summary>Brings values from an edited or older file back into range.</summary>
    public void Normalize()
    {
        Intensity = Math.Clamp(Intensity, 0, 100);
        if (PointerSpeed is int old)
        {
            // Someone who lowered the speed wanted a smaller tremor on screen: keep that,
            // but only while the hand shakes.
            if (old < 100 && !ShakeBrake)
            {
                ShakeBrake = true;
                ShakeSlowdown = 100 - old;
            }
            PointerSpeed = null;
        }
        StillZone = Math.Clamp(StillZone, 0, MaxStillZone);
        ClickHold = Math.Clamp(ClickHold, 0, MaxClickHold);
        ShakeSlowdown = Math.Clamp(ShakeSlowdown, 0, MaxShakeSlowdown);
    }
}
