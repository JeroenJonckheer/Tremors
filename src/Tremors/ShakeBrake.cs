namespace Tremors;

/// <summary>
/// "Slow down when shaking", in the spirit of the Angle Mouse (Wobbrock et al., CHI 2009):
/// looks at how straight the hand moves. Over the last quarter second it compares the
/// net distance covered with the length of the path. A straight movement scores 1 and
/// keeps full speed; a hand that goes back and forth (tremor, small corrections) scores
/// near 0 and gets a lower speed. It never looks at what is on screen.
/// </summary>
internal sealed class ShakeBrake
{
    /// <summary>How far back (ms) the movement is looked at; at least one tremor period.</summary>
    public double WindowMs { get; set; } = 250;

    /// <summary>Straightness above which nothing happens.</summary>
    public double Calm { get; set; } = 0.8;

    /// <summary>Straightness at or below which the full slowdown applies.</summary>
    public double Shaky { get; set; } = 0.3;

    /// <summary>Path (px) needed within the window before it is judged at all.</summary>
    public double MinPathPx { get; set; } = 6;

    /// <summary>Largest slowdown, 0 (off) to 0.9: at 0.6 the cursor drops to 40% speed.</summary>
    public double MaxSlowdown { get; set; } = 0.6;

    /// <summary>How quickly the speed follows (ms).</summary>
    public double FollowMs { get; set; } = 80;

    /// <summary>Speed factor now: 1 = full speed.</summary>
    public double Factor { get; private set; } = 1;

    /// <summary>Straightness over the window, 0..1 (for display).</summary>
    public double Straightness { get; private set; } = 1;

    private readonly Queue<(long At, double Dx, double Dy)> moves = new();
    private long lastAt;

    /// <summary>Feeds one raw hand movement and returns the speed factor to use for it.</summary>
    public double Update(double dx, double dy, long now)
    {
        moves.Enqueue((now, dx, dy));
        while (now - moves.Peek().At > WindowMs) moves.Dequeue();

        double netX = 0, netY = 0, path = 0;
        foreach (var m in moves)
        {
            netX += m.Dx;
            netY += m.Dy;
            path += Math.Sqrt(m.Dx * m.Dx + m.Dy * m.Dy);
        }
        Straightness = path < MinPathPx ? 1 : Math.Sqrt(netX * netX + netY * netY) / path;

        double shaky = Math.Clamp((Calm - Straightness) / (Calm - Shaky), 0, 1);
        double target = 1 - Math.Clamp(MaxSlowdown, 0, 0.9) * shaky;

        double elapsed = lastAt == 0 ? 0 : Math.Min(now - lastAt, 100);
        lastAt = now;
        Factor += (1 - Math.Exp(-elapsed / FollowMs)) * (target - Factor);
        return Factor;
    }

    /// <summary>Forget the recent movement: start at full speed.</summary>
    public void Reset()
    {
        moves.Clear();
        Factor = 1;
        Straightness = 1;
    }
}
