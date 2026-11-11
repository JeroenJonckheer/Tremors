using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Tremors;

/// <summary>
/// Draws the Tremors icon in code: a steady mouse pointer with two faint, offset copies
/// behind it, the tremor that is being absorbed. Navy and mint when active, grey when
/// paused. The same design is in Tremors.ico and docs/icon.png (tools/make_icon.py).
/// </summary>
internal static class TrayIcon
{
    // Classic pointer, tip at (0, 0), height 1.
    private static readonly PointF[] Arrow =
        [new(0, 0), new(0, 0.895f), new(0.211f, 0.684f), new(0.368f, 1f), new(0.474f, 0.947f), new(0.316f, 0.632f), new(0.632f, 0.632f)];

    private const float Height = 0.56f;   // pointer height, fraction of the icon
    private const float Rounding = 0.018f; // softens the pointer's corners

    private static readonly (float X, float Y, int Alpha)[] Ghosts = [(-0.07f, 0.025f, 80), (0.055f, -0.045f, 130)];

    private static readonly Color Navy = Color.FromArgb(28, 52, 78);
    private static readonly Color Grey = Color.FromArgb(120, 128, 138);
    private static readonly Color White = Color.FromArgb(240, 246, 248);
    private static readonly Color Mint = Color.FromArgb(110, 222, 172);
    private static readonly Color Pale = Color.FromArgb(210, 216, 222);

    /// <summary>Creates the icon at the size Windows uses for the notification area.</summary>
    public static Icon Create(bool active)
    {
        int size = Math.Max(16, SystemInformation.SmallIconSize.Width);
        using var large = Draw(active, size * 4); // draw large, scale down for smooth edges
        using var bmp = new Bitmap(size, size);
        using (var g = Graphics.FromImage(bmp))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(large, 0, 0, size, size);
        }

        IntPtr handle = bmp.GetHicon();
        try
        {
            using var temporary = Icon.FromHandle(handle);
            return (Icon)temporary.Clone(); // owns its own copy
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    private static Bitmap Draw(bool active, int size)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        float margin = size * 0.02f;
        using (var background = new SolidBrush(active ? Navy : Grey))
            g.FillEllipse(background, margin, margin, size - 2 * margin, size - 2 * margin);

        float h = size * Height;
        var tip = new PointF(size / 2f - 0.632f * h / 2, size / 2f - h / 2); // centre the pointer
        foreach (var (x, y, alpha) in Ghosts)
            Pointer(g, new PointF(tip.X + x * size, tip.Y + y * size), h, Color.FromArgb(alpha, active ? Mint : Pale), size);
        Pointer(g, tip, h, White, size);
        return bmp;
    }

    private static void Pointer(Graphics g, PointF tip, float height, Color color, int size)
    {
        var points = Array.ConvertAll(Arrow, p => new PointF(tip.X + p.X * height, tip.Y + p.Y * height));
        using var path = new GraphicsPath();
        path.AddPolygon(points);
        using var brush = new SolidBrush(color);
        using var pen = new Pen(color, size * Rounding) { LineJoin = LineJoin.Round };
        g.FillPath(brush, path);
        g.DrawPath(pen, path);
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
