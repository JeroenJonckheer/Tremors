using System.Globalization;
using Tremors;

// A hand moves from the left of a window to a small button, holds still on it and clicks.
// On top: a 5 Hz tremor of about ±25 px and two sudden jerks of about 2 cm. The same hand is fed through
// the real Tremors filter at its default settings. Output, one row per 10 ms:
// time_ms, raw_x, raw_y, filtered_x, filtered_y
const int DurationMs = 5200;
const int EventMs = 8;   // 125 Hz mouse
const int TimerMs = 10;  // Tremors' settle timer

var start = (X: 70.0, Y: 230.0);
var target = (X: 251.0, Y: 118.0);
var cursor = new Mouse.POINT { X = (int)start.X, Y = (int)start.Y };
var screen = new Rectangle(0, 0, 480, 300);
var filter = new Smoother((x, y) =>
{
    cursor.X = Math.Clamp(x, screen.Left, screen.Right - 1);
    cursor.Y = Math.Clamp(y, screen.Top, screen.Bottom - 1);
    return cursor;
}, () => screen);
filter.Configure(70);

(double X, double Y) Hand(double ms)
{
    double t = ms / 1000;
    // Deliberate movement: minimum-jerk profile from 0.4 s to 1.9 s, then hold.
    double u = Math.Clamp((t - 0.4) / 1.5, 0, 1);
    double s = u * u * u * (10 - 15 * u + 6 * u * u);
    double x = start.X + (target.X - start.X) * s;
    double y = start.Y + (target.Y - start.Y) * s;
    // Tremor: two incommensurate frequencies so it never looks periodic.
    x += 19 * Math.Sin(2 * Math.PI * 5.1 * t) + 6 * Math.Sin(2 * Math.PI * 7.7 * t + 1.3);
    y += 15 * Math.Sin(2 * Math.PI * 4.6 * t + 0.7) + 5 * Math.Sin(2 * Math.PI * 8.3 * t + 2.1);
    // Two sudden jerks of about 2 cm, out and back within 0.15 s.
    foreach (var (at, dx, dy) in new[] { (2.6, 60.0, -40.0), (3.7, -52.0, 44.0) })
    {
        double k = (t - at) / 0.15;
        if (k is > 0 and < 1) { double bump = Math.Sin(Math.PI * k); x += dx * bump; y += dy * bump; }
    }
    return (x, y);
}

var output = new StreamWriter(args.Length > 0 ? args[0] : "trace.csv");
output.WriteLine("time_ms,raw_x,raw_y,filtered_x,filtered_y");
var hand = Hand(0);
int lastX = (int)Math.Round(hand.X), lastY = (int)Math.Round(hand.Y);
long nextEvent = EventMs, nextTimer = TimerMs;
for (long now = 1; now <= DurationMs; now++)
{
    if (now == nextEvent)
    {
        nextEvent += EventMs;
        hand = Hand(now);
        int x = (int)Math.Round(hand.X), y = (int)Math.Round(hand.Y);
        if (x != lastX || y != lastY)
            filter.Move(cursor.X + x - lastX, cursor.Y + y - lastY, cursor, now);
        lastX = x;
        lastY = y;
    }
    if (now == nextTimer)
    {
        nextTimer += TimerMs;
        if (filter.Settling) filter.Step(now);
        output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"{now},{hand.X:F1},{hand.Y:F1},{cursor.X},{cursor.Y}"));
    }
}
output.Dispose();
Console.WriteLine("ok");
