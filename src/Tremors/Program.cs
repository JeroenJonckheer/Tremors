namespace Tremors;

internal static class Program
{
    /// <summary>Signalled by a second launch, so the running instance opens its settings.</summary>
    public const string ShowSettingsEvent = @"Local\Tremors.ShowSettings";

    [STAThread]
    private static void Main()
    {
        // One instance per user session. Starting Tremors again opens the settings
        // of the instance that is already running.
        using var mutex = new Mutex(initiallyOwned: true, @"Local\Tremors", out bool createdNew);
        if (!createdNew)
        {
            if (EventWaitHandle.TryOpenExisting(ShowSettingsEvent, out var running))
                using (running) running.Set();
            return;
        }

        // Never leave the user's mouse acceleration switched off, however Tremors ends.
        AppDomain.CurrentDomain.ProcessExit += (_, _) => PointerAcceleration.Restore();

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApp());
    }
}
