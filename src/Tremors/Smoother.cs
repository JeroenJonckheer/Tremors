namespace Tremors;

/// <summary>
/// Tremor filter. Raw mouse movement is swallowed and replaced by a smoothed
/// cursor position, computed in four steps:
/// <list type="number">
///   <item><b>Raw position.</b> Every swallowed event adds its movement to a
///   virtual "raw" cursor that follows the hand exactly.</item>
///   <item><b>Intent.</b> The velocity of the hand, averaged over about a tenth of a
///   second. Trembling and short jerks go back and forth and average out; a deliberate
///   movement keeps going in one direction and builds up speed.</item>
///   <item><b>Adaptive low-pass.</b> Two exponential filters in series pull the
///   smoothed position towards the raw one. Two stages damp sudden jerks far more than
///   one. The more intent, the shorter the time constant, so deliberate movement still
///   feels direct.</item>
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
    /// <summary>Filter constants. The values at intensity 100 scale down with the slider.</summary>
    /// <param name="MaxTimeConstantMs">Smoothing of a hand that is not going anywhere.</param>
    /// <param name="IntentThreshold">Average hand speed (px/ms) below which movement counts as tremor.</param>
    /// <param name="IntentSpeed">Speed above the threshold (px/ms) at which smoothing halves.</param>
    /// <param name="IntentWindowMs">Time over which hand speed is averaged.</param>
    /// <param name="MaxLeashPx">Leash radius.</param>
    /// <param name="MaxWheelGuardMs">Wheel debounce.</param>
    internal sealed record Tuning(
        double MaxTimeConstantMs = 500,
        double IntentThreshold = 0.5,
        double IntentSpeed = 0.2,
        double IntentWindowMs = 250,
        double MaxLeashPx = 4,
        double MaxWheelGuardMs = 400);

    private const double MaxStepMs = 50; // cap for long pauses between updates

    private readonly Tuning _tuning;
    private double _timeConstant;
    private double _leash;
    private double _wheelGuard;
    private bool _wheelEnabled = true;

    private bool _synced;
    private double _rawX, _rawY;     // where the hand is
    private double _prevX, _prevY;   // raw position at the previous step
    private double _velX, _velY;     // averaged hand velocity, px/ms
    private double _midX, _midY;     // first filter stage
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
    /// <param name="tuning">Filter constants; defaults when omitted.</param>
    public Smoother(Func<int, int, Mouse.POINT> moveCursor, Func<Rectangle> screen, Tuning? tuning = null)
    {
        _moveCursor = moveCursor;
        _screen = screen;
        _tuning = tuning ?? new Tuning();
    }

    /// <summary>False at intensity 0: movement then passes through untouched.</summary>
    public bool Active => _timeConstant > 0;

    /// <summary>True while the cursor is still gliding towards the hand.</summary>
    public bool Settling => _synced &&
        (Math.Abs(_rawX - _fltX) > 0.5 || Math.Abs(_rawY - _fltY) > 0.5 ||
         Math.Abs(_rawX - _midX) > 0.5 || Math.Abs(_rawY - _midY) > 0.5);

    /// <summary>Maps the 0..100 slider to filter strength.</summary>
    public void Configure(int intensity)
    {
        double k = Math.Clamp(intensity, 0, 100) / 100.0;
        _timeConstant = _tuning.MaxTimeConstantMs * Math.Pow(k, 1.5);
        _leash = _tuning.MaxLeashPx * k;
        _wheelGuard = _tuning.MaxWheelGuardMs * k;
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

        double elapsed = now - _lastStep;
        if (elapsed <= 0) return;
        _lastStep = now;
        double dt = Math.Min(elapsed, MaxStepMs);

        // Intent: hand velocity averaged over the intent window. Uses the real elapsed
        // time, so the speed of an earlier movement does not linger after a pause.
        double v = 1 - Math.Exp(-elapsed / _tuning.IntentWindowMs);
        _velX += v * ((_rawX - _prevX) / elapsed - _velX);
        _velY += v * ((_rawY - _prevY) / elapsed - _velY);
        _prevX = _rawX;
        _prevY = _rawY;
        double speed = Math.Sqrt(_velX * _velX + _velY * _velY);
        double intent = Math.Max(0, speed - _tuning.IntentThreshold) / _tuning.IntentSpeed;

        // Two-stage low-pass; each stage gets half the time constant.
        double tau = _timeConstant / (1 + intent) / 2;
        double a = 1 - Math.Exp(-dt / tau);
        _midX += a * (_rawX - _midX);
        _midY += a * (_rawY - _midY);
        _fltX += a * (_midX - _fltX);
        _fltY += a * (_midY - _fltY);

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
        _rawX = _prevX = _midX = _fltX = _curX;
        _rawY = _prevY = _midY = _fltY = _curY;
        _velX = _velY = 0;
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
        _rawX = _prevX = _midX = _fltX = _curX = p.X;
        _rawY = _prevY = _midY = _fltY = _curY = p.Y;
        _velX = _velY = 0;
        _lastStep = now;
        _synced = true;
    }
}
