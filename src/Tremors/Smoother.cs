namespace Tremors;

/// <summary>
/// Tremor filter. Raw mouse movement is swallowed and replaced by a smoothed
/// cursor position, computed in three steps:
/// <list type="number">
///   <item><b>Raw position.</b> Every swallowed event adds its movement to a
///   virtual "raw" cursor that follows the hand exactly.</item>
///   <item><b>Adaptive low-pass.</b> A time-based exponential filter pulls the
///   smoothed position towards the raw one. Its time constant shrinks as the gap
///   grows: small trembling is damped hard, large intentional moves catch up fast.</item>
///   <item><b>Leash.</b> The visible cursor only moves once the smoothed position is
///   more than a few pixels away, and then trails it at that distance. Jitter
///   while holding still on a target never reaches the screen.</item>
/// </list>
/// The scroll wheel is debounced: a notch in the opposite direction shortly after
/// a notch is treated as a tremor and dropped.
/// </summary>
/// <remarks>Not thread-safe; call from the UI thread only (the hook runs there too).</remarks>
internal sealed class Smoother
{
    private const double MaxTimeConstantMs = 250; // smoothing at intensity 100
    private const double MaxLeashPx = 4;          // leash radius at intensity 100
    private const double MaxWheelGuardMs = 400;   // wheel debounce at intensity 100
    private const double CatchUpPx = 40;          // gap at which smoothing halves
    private const double MaxStepMs = 50;          // cap for long pauses between updates

    private double _timeConstant;
    private double _leash;
    private double _wheelGuard;
    private bool _wheelEnabled = true;

    private bool _synced;
    private double _rawX, _rawY;     // where the hand is
    private double _fltX, _fltY;     // smoothed position
    private int _curX, _curY;        // where we last put the cursor
    private long _lastStep;

    private int _wheelDirection;
    private long _wheelTime;

    private readonly Func<int, int, Mouse.POINT> _moveCursor;
    private readonly Func<Rectangle> _screen;

    /// <summary>Filter that drives the real cursor.</summary>
    public Smoother() : this(Mouse.MoveCursor, () => SystemInformation.VirtualScreen) { }

    /// <param name="moveCursor">Moves the cursor and returns where it actually landed.</param>
    /// <param name="screen">Bounds of the whole desktop, all monitors together.</param>
    public Smoother(Func<int, int, Mouse.POINT> moveCursor, Func<Rectangle> screen)
    {
        _moveCursor = moveCursor;
        _screen = screen;
    }

    /// <summary>False at intensity 0: movement then passes through untouched.</summary>
    public bool Active => _timeConstant > 0;

    /// <summary>True while the cursor is still gliding towards the hand.</summary>
    public bool Settling => _synced && (Math.Abs(_rawX - _fltX) > 0.5 || Math.Abs(_rawY - _fltY) > 0.5);

    /// <summary>Maps the 0..100 slider to filter strength.</summary>
    public void Configure(int intensity)
    {
        double k = Math.Clamp(intensity, 0, 100) / 100.0;
        _timeConstant = MaxTimeConstantMs * Math.Pow(k, 1.5);
        _leash = MaxLeashPx * k;
        _wheelGuard = MaxWheelGuardMs * k;
    }

    public void SetWheel(bool enabled)
    {
        _wheelEnabled = enabled;
        _wheelDirection = 0;
    }

    /// <summary>Forget all state; the next event starts from the real cursor.</summary>
    public void Reset() => _synced = false;

    /// <summary>
    /// Handles a mouse move. <paramref name="x"/>/<paramref name="y"/> is where Windows
    /// would put the cursor, <paramref name="cursor"/> is where it is now.
    /// Always swallows the event and moves the cursor itself.
    /// </summary>
    public void Move(int x, int y, Mouse.POINT cursor, long now)
    {
        if (!_synced || cursor.X != _curX || cursor.Y != _curY)
            SyncTo(cursor, now); // first event, or something else moved the cursor

        var screen = _screen();
        _rawX = Math.Clamp(_rawX + x - cursor.X, screen.Left, screen.Right - 1);
        _rawY = Math.Clamp(_rawY + y - cursor.Y, screen.Top, screen.Bottom - 1);
        Step(now);
    }

    /// <summary>Advances the filter in time. Called on every move and from a timer.</summary>
    public void Step(long now)
    {
        if (!_synced) return;

        double dt = Math.Min(now - _lastStep, MaxStepMs);
        _lastStep = now;
        if (dt <= 0) return;

        double gapX = _rawX - _fltX, gapY = _rawY - _fltY;
        double gap = Math.Sqrt(gapX * gapX + gapY * gapY);
        double tau = _timeConstant / (1 + gap / CatchUpPx);
        double a = 1 - Math.Exp(-dt / tau);
        _fltX += a * gapX;
        _fltY += a * gapY;

        // Leash: keep the cursor within _leash pixels of the smoothed position.
        double dx = _fltX - _curX, dy = _fltY - _curY;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        if (dist <= _leash) return;
        double pull = (dist - _leash) / dist;
        int x = (int)Math.Round(_curX + dx * pull);
        int y = (int)Math.Round(_curY + dy * pull);
        if (x == _curX && y == _curY) return;

        var actual = _moveCursor(x, y);
        _curX = actual.X;
        _curY = actual.Y;
    }

    /// <summary>On a click the cursor stays put: drop the remaining glide.</summary>
    public void Click()
    {
        _rawX = _fltX = _curX;
        _rawY = _fltY = _curY;
    }

    /// <summary>Returns false when a wheel notch looks like a tremor.</summary>
    public bool Wheel(int delta, long now)
    {
        if (!_wheelEnabled || delta == 0) return true;

        int direction = Math.Sign(delta);
        if (direction != _wheelDirection && _wheelDirection != 0 && now - _wheelTime < _wheelGuard)
            return false;

        _wheelDirection = direction;
        _wheelTime = now;
        return true;
    }

    private void SyncTo(Mouse.POINT p, long now)
    {
        _rawX = _fltX = _curX = p.X;
        _rawY = _fltY = _curY = p.Y;
        _lastStep = now;
        _synced = true;
    }
}
