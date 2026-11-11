using System.Drawing.Drawing2D;

namespace Tremors;

/// <summary>
/// Draws the flags for the language buttons in code (Windows shows flag emoji as
/// letters). English uses the Union Jack.
/// </summary>
internal static class Flags
{
    public static Bitmap Draw(string language, int width, int height)
    {
        var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var area = new Rectangle(0, 0, width, height);

        switch (language)
        {
            case "nl": Horizontal(g, area, Color.FromArgb(174, 28, 40), Color.White, Color.FromArgb(33, 70, 139)); break;
            case "de": Horizontal(g, area, Color.Black, Color.FromArgb(221, 0, 0), Color.FromArgb(255, 206, 0)); break;
            case "fr": Vertical(g, area, Color.FromArgb(0, 85, 164), Color.White, Color.FromArgb(239, 65, 53)); break;
            default: UnionJack(g, area); break;
        }

        using var border = new Pen(Color.FromArgb(120, 0, 0, 0));
        g.SmoothingMode = SmoothingMode.None;
        g.DrawRectangle(border, 0, 0, width - 1, height - 1);
        return bmp;
    }

    private static void Horizontal(Graphics g, Rectangle r, Color top, Color middle, Color bottom)
    {
        int third = r.Height / 3;
        Fill(g, middle, r);
        Fill(g, top, new Rectangle(r.X, r.Y, r.Width, third));
        Fill(g, bottom, new Rectangle(r.X, r.Bottom - third, r.Width, third));
    }

    private static void Vertical(Graphics g, Rectangle r, Color left, Color middle, Color right)
    {
        int third = r.Width / 3;
        Fill(g, middle, r);
        Fill(g, left, new Rectangle(r.X, r.Y, third, r.Height));
        Fill(g, right, new Rectangle(r.Right - third, r.Y, third, r.Height));
    }

    private static void UnionJack(Graphics g, Rectangle r)
    {
        var blue = Color.FromArgb(1, 33, 105);
        var red = Color.FromArgb(200, 16, 46);
        float h = r.Height;
        Fill(g, blue, r);
        g.SetClip(r);
        using (var white = new Pen(Color.White, h / 5))
        {
            g.DrawLine(white, r.Left, r.Top, r.Right, r.Bottom);
            g.DrawLine(white, r.Right, r.Top, r.Left, r.Bottom);
        }
        using (var thinRed = new Pen(red, h / 12))
        {
            g.DrawLine(thinRed, r.Left, r.Top, r.Right, r.Bottom);
            g.DrawLine(thinRed, r.Right, r.Top, r.Left, r.Bottom);
        }
        float cx = r.Left + r.Width / 2f, cy = r.Top + h / 2;
        using (var whiteCross = new SolidBrush(Color.White))
        {
            g.FillRectangle(whiteCross, cx - h / 6, r.Top, h / 3, h);
            g.FillRectangle(whiteCross, r.Left, cy - h / 6, r.Width, h / 3);
        }
        using (var redCross = new SolidBrush(red))
        {
            g.FillRectangle(redCross, cx - h / 10, r.Top, h / 5, h);
            g.FillRectangle(redCross, r.Left, cy - h / 10, r.Width, h / 5);
        }
        g.ResetClip();
    }

    private static void Fill(Graphics g, Color color, Rectangle r)
    {
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, r);
    }
}
