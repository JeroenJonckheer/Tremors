namespace Tremors;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // One instance per user session; a second launch quietly exits.
        using var mutex = new Mutex(initiallyOwned: true, @"Local\Tremors", out bool createdNew);
        if (!createdNew) return;

        // Never leave the user's mouse acceleration switched off, however Tremors ends.
        AppDomain.CurrentDomain.ProcessExit += (_, _) => PointerAcceleration.Restore();

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApp());
    }
}
