using System.Text.Json;
using Xunit;

namespace Tremors.Tests;

public class SettingsTests
{
    private static Settings Read(string json)
    {
        var s = JsonSerializer.Deserialize<Settings>(json)!;
        s.Normalize();
        return s;
    }

    [Fact]
    public void A_lowered_pointer_speed_from_an_older_version_becomes_the_shake_brake()
    {
        var s = Read("""{ "PointerSpeed": 60 }""");

        Assert.True(s.ShakeBrake);
        Assert.Equal(40, s.ShakeSlowdown);
        Assert.Null(s.PointerSpeed);
    }

    [Fact]
    public void Full_pointer_speed_from_an_older_version_changes_nothing()
    {
        var s = Read("""{ "PointerSpeed": 100 }""");

        Assert.False(s.ShakeBrake);
        Assert.Equal(60, s.ShakeSlowdown);
    }

    [Fact]
    public void A_shake_brake_that_is_already_on_is_kept()
    {
        var s = Read("""{ "PointerSpeed": 30, "ShakeBrake": true, "ShakeSlowdown": 50 }""");

        Assert.True(s.ShakeBrake);
        Assert.Equal(50, s.ShakeSlowdown);
    }

    [Fact]
    public void The_pointer_speed_is_no_longer_written()
    {
        var json = JsonSerializer.Serialize(Read("""{ "PointerSpeed": 60 }"""));

        Assert.DoesNotContain("PointerSpeed", json);
    }

    [Fact]
    public void The_slowdown_stays_within_range()
    {
        Assert.Equal(Settings.MaxShakeSlowdown, Read("""{ "ShakeSlowdown": 500 }""").ShakeSlowdown);
        Assert.Equal(0, Read("""{ "ShakeSlowdown": -5 }""").ShakeSlowdown);
    }
}
