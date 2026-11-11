using System.Runtime.InteropServices;

namespace Tremors;

/// <summary>
/// Switches off Windows pointer acceleration ("Enhance pointer precision") while
/// Tremors filters. Acceleration enlarges fast movements, and a tremor is fast, so a
/// shake would reach the filter already magnified.
/// </summary>
/// <remarks>
/// The change is made for this session only and is never written to the user's
/// profile, so the user's own setting returns after signing in again, even if
/// Tremors would crash before restoring it.
/// </remarks>
internal static class PointerAcceleration
{
    private const uint SPI_GETMOUSE = 0x0003;
    private const uint SPI_SETMOUSE = 0x0004;
    private const uint SPIF_SENDCHANGE = 0x0002;

    private static int[]? _original;

    /// <summary>Turns acceleration off and remembers the previous values.</summary>
    public static void Suspend()
    {
        if (_original is not null) return;

        var values = new int[3]; // threshold 1, threshold 2, acceleration (0 = off)
        if (!SystemParametersInfo(SPI_GETMOUSE, 0, values, 0) || values[2] == 0) return;

        _original = (int[])values.Clone();
        values[2] = 0;
        SystemParametersInfo(SPI_SETMOUSE, 0, values, SPIF_SENDCHANGE);
    }

    /// <summary>Puts back what <see cref="Suspend"/> changed.</summary>
    public static void Restore()
    {
        if (_original is null) return;
        SystemParametersInfo(SPI_SETMOUSE, 0, _original, SPIF_SENDCHANGE);
        _original = null;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, int[] pvParam, uint fWinIni);
}
