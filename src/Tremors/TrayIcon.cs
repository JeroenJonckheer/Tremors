using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Tremors;

/// <summary>
/// Draws the tray icon in code, so no .ico file is needed: a crosshair with a
/// dot in the middle. Green when Tremors is active, grey when it is paused.
/// </summary>
internal static class TrayIcon
{
    public static Icon Create(bool active)
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using var background = new SolidBrush(Color.FromArgb(24, 30, 40));
            g.FillEllipse(background, 1, 1, 30, 30);

            using var cross = new Pen(Color.FromArgb(224, 248, 255), 2.5f);
            g.DrawLine(cross, 16, 5, 16, 27);
            g.DrawLine(cross, 5, 16, 27, 16);

            using var dot = new SolidBrush(active ? Color.FromArgb(0, 200, 120) : Color.FromArgb(130, 136, 145));
            g.FillEllipse(dot, 10, 10, 12, 12);
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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
