namespace Tremors;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // One instance per user session; a second launch quietly exits.
        using var mutex = new Mutex(initiallyOwned: true, @"Local\Tremors", out bool createdNew);
        if (!createdNew) return;

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApp());
    }
}
