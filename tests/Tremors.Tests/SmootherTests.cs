using Xunit;

namespace Tremors.Tests;

/// <summary>
/// Drives the filter with a simulated hand and a simulated cursor, the way the
/// mouse hook and the timer do in the real application.
/// </summary>
public class SmootherTests
{
    private const int EventIntervalMs = 8; // a 125 Hz mouse

    private sealed class Rig
    {
        public static readonly Rectangle Desktop = new(0, 0, 1920, 1080);

        public Mouse.POINT Cursor;
        public long Now = 1_000;
        public readonly Smoother Filter;

        public Rig(int intensity, int x = 960, int y = 540)
        {
            Cursor = new Mouse.POINT { X = x, Y = y };
            Filter = new Smoother(MoveCursor, () => Desktop);
            Filter.Configure(intensity);
        }

        private Mouse.POINT MoveCursor(int x, int y)
        {
            Cursor.X = Math.Clamp(x, Desktop.Left, Desktop.Right - 1);
            Cursor.Y = Math.Clamp(y, Desktop.Top, Desktop.Bottom - 1);
            return Cursor;
        }

        /// <summary>The hand moved by (dx, dy) since the previous event.</summary>
        public void Hand(int dx, int dy)
        {
            Now += EventIntervalMs;
            Filter.Move(Cursor.X + dx, Cursor.Y + dy, Cursor, Now);
        }

        /// <summary>The hand is still; only the timer runs.</summary>
        public void Wait(int ms)
        {
            for (int t = 0; t < ms; t += 10)
            {
                Now += 10;
                if (Filter.Settling) Filter.Step(Now);
            }
        }
    }

    /// <summary>Feeds an absolute hand path; returns the cursor after every event.</summary>
    private static List<Mouse.POINT> Follow(Rig rig, Func<double, (double X, double Y)> path, int durationMs)
    {
        var seen = new List<Mouse.POINT>();
        var start = rig.Cursor;
        int handX = start.X, handY = start.Y;
        for (int step = EventIntervalMs; step < durationMs + EventIntervalMs; step += EventIntervalMs)
        {
            int t = Math.Min(step, durationMs); // always end exactly on the path's end point
            var (x, y) = path(t / 1000.0);
            int nextX = start.X + (int)Math.Round(x), nextY = start.Y + (int)Math.Round(y);
            rig.Hand(nextX - handX, nextY - handY);
            handX = nextX;
            handY = nextY;
            seen.Add(rig.Cursor);
        }
        return seen;
    }

    private static int Spread(IEnumerable<Mouse.POINT> points) =>
        Math.Max(points.Max(p => p.X) - points.Min(p => p.X), points.Max(p => p.Y) - points.Min(p => p.Y));

    /// <summary>A 5 Hz tremor of ±8 px, typical for an essential tremor.</summary>
    private static (double, double) Tremor(double t) =>
        (8 * Math.Sin(2 * Math.PI * 5 * t), 6 * Math.Sin(2 * Math.PI * 5.3 * t + 1));

    [Fact]
    public void Tremor_while_holding_still_barely_moves_the_cursor()
    {
        var rig = new Rig(intensity: 70);

        var cursor = Follow(rig, Tremor, 3000).Skip(60).ToList(); // after the first half second

        Assert.True(Spread(cursor) <= 4, $"cursor wandered {Spread(cursor)} px against 16 px of tremor");
    }

    /// <summary>A sudden jerk of about 4 cm (150 px) out in 64 ms and back in 96 ms.</summary>
    private static (double, double) Jerk(double t) =>
        (t <= 0.064 ? 150 * t / 0.064 : Math.Max(0, 150 - 150 * (t - 0.064) / 0.096), 0);

    [Fact]
    public void Sudden_jerk_is_mostly_absorbed()
    {
        var rig = new Rig(intensity: 70);

        var cursor = Follow(rig, Jerk, 600);

        int reach = cursor.Max(p => p.X) - 960;
        Assert.True(reach <= 30, $"a 150 px jerk moved the cursor {reach} px");
    }

    [Fact]
    public void Full_strength_absorbs_nearly_all_of_a_jerk()
    {
        var strong = new Rig(intensity: 100);

        int reach = Follow(strong, Jerk, 600).Max(p => p.X) - 960;

        Assert.True(reach <= 15, $"a 150 px jerk moved the cursor {reach} px at full strength");
    }

    [Fact]
    public void Stronger_setting_damps_more()
    {
        int Reach(int intensity) => Follow(new Rig(intensity), Jerk, 600).Max(p => p.X) - 960;

        Assert.True(Reach(100) < Reach(70));
        Assert.True(Reach(70) < Reach(30));
        Assert.True(Reach(30) < Reach(1));
    }

    [Fact]
    public void Lower_pointer_speed_shrinks_the_movement()
    {
        var rig = new Rig(intensity: 70, x: 200, y: 200);
        rig.Filter.SetPointerSpeed(50);

        Follow(rig, t => (2000 * t, 0), 300); // the hand moves 600 px
        rig.Wait(1000);

        Assert.InRange(rig.Cursor.X, 500 - 5, 500); // the cursor 300 px
    }

    [Fact]
    public void Still_zone_absorbs_jerks_smaller_than_the_zone()
    {
        var rig = new Rig(intensity: 30);
        rig.Filter.SetStillZone(25);

        var cursor = Follow(rig, t => (25 * Math.Sin(2 * Math.PI * 4 * t), 0), 2000);

        Assert.All(cursor, p => Assert.Equal(960, p.X));
    }

    [Fact]
    public void Intentional_move_arrives_where_the_hand_went()
    {
        var rig = new Rig(intensity: 70, x: 200, y: 200);

        Follow(rig, t => (2000 * t, 1000 * t), 300); // 600 x 300 px in 0.3 s
        rig.Wait(600);

        Assert.InRange(rig.Cursor.X, 800 - 4, 800);
        Assert.InRange(rig.Cursor.Y, 500 - 4, 500);
    }

    [Fact]
    public void Intentional_move_through_tremor_still_arrives()
    {
        var rig = new Rig(intensity: 70, x: 200, y: 200);

        Follow(rig, t => { var (x, y) = Tremor(t); return (x + 500 * t, y); }, 1000);
        rig.Wait(600);

        Assert.InRange(rig.Cursor.X, 700 - 12, 700 + 12);
        Assert.InRange(rig.Cursor.Y, 200 - 12, 200 + 12);
    }

    [Fact]
    public void Intensity_zero_switches_the_filter_off()
    {
        var filter = new Smoother((x, y) => new Mouse.POINT { X = x, Y = y }, () => Rig.Desktop);

        filter.Configure(0);

        Assert.False(filter.Active);
    }

    [Fact]
    public void Click_stops_the_glide()
    {
        var rig = new Rig(intensity: 70);
        Follow(rig, t => (1000 * t, 0), 200);
        var atClick = rig.Cursor;

        rig.Filter.Click();
        rig.Wait(500);

        Assert.Equal(atClick, rig.Cursor);
    }

    [Fact]
    public void Cursor_moved_by_another_program_is_followed_without_a_jump()
    {
        var rig = new Rig(intensity: 70);
        Follow(rig, Tremor, 500);

        rig.Cursor = new Mouse.POINT { X = 100, Y = 100 }; // e.g. an app centred the cursor
        rig.Hand(1, 0);
        rig.Wait(500);

        Assert.InRange(rig.Cursor.X, 96, 101);
        Assert.InRange(rig.Cursor.Y, 96, 100);
    }

    [Fact]
    public void Hand_cannot_drift_off_screen()
    {
        var rig = new Rig(intensity: 70, x: 10, y: 10);

        Follow(rig, t => (-5000 * t, -5000 * t), 500); // keeps pushing into the corner
        Follow(rig, t => (100 * t, 100 * t), 500);     // and comes back a little
        rig.Wait(600);

        Assert.InRange(rig.Cursor.X, 40, 50);
        Assert.InRange(rig.Cursor.Y, 40, 50);
    }

    [Fact]
    public void Wheel_flick_back_is_ignored_but_a_real_reversal_passes()
    {
        var filter = new Smoother((x, y) => new Mouse.POINT { X = x, Y = y }, () => Rig.Desktop);
        filter.Configure(70);

        Assert.True(filter.Wheel(120, now: 1000));
        Assert.True(filter.Wheel(120, now: 1100));
        Assert.False(filter.Wheel(-120, now: 1150)); // tremor flick
        Assert.True(filter.Wheel(-120, now: 2000));  // the user really scrolls back
    }

    [Fact]
    public void Wheel_filter_can_be_switched_off()
    {
        var filter = new Smoother((x, y) => new Mouse.POINT { X = x, Y = y }, () => Rig.Desktop);
        filter.Configure(70);
        filter.SetWheel(false);

        Assert.True(filter.Wheel(120, now: 1000));
        Assert.True(filter.Wheel(-120, now: 1010));
    }
}
