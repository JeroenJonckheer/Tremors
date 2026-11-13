using Xunit;

namespace Tremors.Tests;

/// <summary>Feeds a simulated hand at 125 Hz into the shake brake.</summary>
public class ShakeBrakeTests
{
    private const int EventIntervalMs = 8;

    /// <summary>Runs the hand for two seconds; returns the lowest factor after the first half second.</summary>
    private static double Lowest(Func<double, (double X, double Y)> hand, double slowdown = 0.6)
    {
        var brake = new ShakeBrake { MaxSlowdown = slowdown };
        var previous = hand(0);
        double lowest = 1;
        for (int ms = EventIntervalMs; ms <= 2000; ms += EventIntervalMs)
        {
            var p = hand(ms / 1000.0);
            double factor = brake.Update(p.X - previous.X, p.Y - previous.Y, 1_000_000 + ms);
            previous = p;
            if (ms > 500) lowest = Math.Min(lowest, factor);
        }
        return lowest;
    }

    private static (double X, double Y) Tremor(double t, double size) =>
        (size * Math.Sin(2 * Math.PI * 5.1 * t), 0.75 * size * Math.Sin(2 * Math.PI * 4.6 * t + 0.7));

    [Theory]
    [InlineData(60)]
    [InlineData(600)]
    public void A_straight_movement_keeps_full_speed(double pixelsPerSecond)
    {
        Assert.Equal(1, Lowest(t => (pixelsPerSecond * t, 0.3 * pixelsPerSecond * t)), 2);
    }

    [Fact]
    public void A_curve_keeps_full_speed()
    {
        Assert.Equal(1, Lowest(t => (300 * Math.Cos(t * Math.PI / 2), 300 * Math.Sin(t * Math.PI / 2))), 2);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(20)]
    public void Shaking_in_place_slows_down_by_the_chosen_amount(double size)
    {
        Assert.Equal(0.4, Lowest(t => Tremor(t, size)), 2);
    }

    [Fact]
    public void Shaking_while_moving_slowly_slows_down_partly()
    {
        double lowest = Lowest(t => { var s = Tremor(t, 20); return (300 * t + s.X, s.Y); });
        Assert.InRange(lowest, 0.45, 0.85);
    }

    [Fact]
    public void A_fast_sweep_with_a_tremor_keeps_full_speed()
    {
        Assert.Equal(1, Lowest(t => { var s = Tremor(t, 20); return (800 * t + s.X, s.Y); }), 2);
    }

    [Fact]
    public void No_slowdown_means_no_effect()
    {
        Assert.Equal(1, Lowest(t => Tremor(t, 20), slowdown: 0), 2);
    }

    [Fact]
    public void Slowdown_is_capped_at_ninety_percent()
    {
        Assert.Equal(0.1, Lowest(t => Tremor(t, 20), slowdown: 2), 2);
    }
}
