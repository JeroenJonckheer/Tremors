namespace Tremors;

/// <summary>
/// The whole application: a tray icon with a right-click menu, the mouse hook,
/// the filter and a small settings window. Nothing else appears on screen.
/// </summary>
internal sealed class TrayApp : ApplicationContext
{
    private readonly Settings _settings;
    private readonly Smoother _filter = new();
    private readonly ShakeBrake _brake = new();
    private readonly ClickGuard _clicks = new(Mouse.SendButtonDown);
    private readonly Mouse _mouse = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 10 };
    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _activeItem;
    private readonly ToolStripMenuItem _autoStartItem;
    private readonly ToolStripMenuItem _settingsItem;
    private readonly ToolStripMenuItem _quitItem;
    private readonly Icon _iconOn = TrayIcon.Create(active: true);
    private readonly Icon _iconOff = TrayIcon.Create(active: false);
    private readonly EventWaitHandle _showSettings = new(false, EventResetMode.AutoReset, Program.ShowSettingsEvent);
    private readonly RegisteredWaitHandle _showSettingsWait;
    private readonly Control _ui = new();
    private SettingsForm? _form;

    /// <summary>Raised after any setting changed, so open windows can refresh.</summary>
    public event Action? Changed;

    public Settings Settings => _settings;

    /// <param name="startedByWindows">
    /// True when Windows started Tremors at sign-in: then it starts quietly in the notification
    /// area. Started by hand, it opens the settings window, so the user sees that it runs.
    /// </param>
    public TrayApp(bool startedByWindows)
    {
        _settings = Store.Load(out bool firstRun);
        Text.Use(_settings.Language);
        _filter.Configure(_settings.Intensity);
        _filter.SetStillZone(_settings.StillZone);
        _filter.SetWheel(_settings.Wheel);
        _clicks.HoldMs = _settings.ClickHold;
        _brake.MaxSlowdown = _settings.ShakeSlowdown / 100.0;
        _mouse.Filter = OnMouse;
        _timer.Tick += (_, _) => OnTimer();
        if (_settings.AutoStart) AutoStart.Enable(); // keep the path current if the exe moved

        _activeItem = new ToolStripMenuItem(Text.Active, null, (_, _) => SetEnabled(!_settings.Enabled));
        _autoStartItem = new ToolStripMenuItem(Text.StartWithWindows, null, (_, _) => SetAutoStart(!_settings.AutoStart));
        var menu = new ContextMenuStrip();
        menu.Items.Add(_activeItem);
        _settingsItem = new ToolStripMenuItem(Text.Settings, null, (_, _) => ShowSettings());
        menu.Items.Add(_settingsItem);
        menu.Items.Add(_autoStartItem);

        menu.Items.Add(new ToolStripSeparator());
        _quitItem = new ToolStripMenuItem(Text.Quit, null, (_, _) => ExitThread());
        menu.Items.Add(_quitItem);

        _tray = new NotifyIcon { ContextMenuStrip = menu, Visible = true };
        _tray.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowSettings(); };

        // A second launch of Tremors.exe signals this event: open the settings.
        _ui.CreateControl();
        _showSettingsWait = ThreadPool.RegisterWaitForSingleObject(_showSettings,
            (_, _) => _ui.BeginInvoke(ShowSettings), null, Timeout.Infinite, executeOnlyOnce: false);

        Apply();

        if (!startedByWindows)
            ShowSettings();
        if (firstRun)
            _tray.ShowBalloonTip(8000, Text.WelcomeTitle, Text.WelcomeBody, ToolTipIcon.Info);
    }

    public void SetEnabled(bool on)
    {
        _settings.Enabled = on;
        Apply();
    }

    public void SetIntensity(int value)
    {
        _settings.Intensity = Math.Clamp(value, 0, 100);
        _filter.Configure(_settings.Intensity);
        Apply();
    }

    public void SetStillZone(int pixels)
    {
        _settings.StillZone = pixels;
        _settings.Normalize();
        _filter.SetStillZone(_settings.StillZone);
        Apply();
    }

    public void SetClickHold(int milliseconds)
    {
        _settings.ClickHold = milliseconds;
        _settings.Normalize();
        _clicks.HoldMs = _settings.ClickHold;
        Apply();
    }

    /// <summary>Switches the interface language; open windows follow through <see cref="Changed"/>.</summary>
    public void SetLanguage(string code)
    {
        _settings.Language = code;
        Text.Use(code);
        _activeItem.Text = Text.Active;
        _settingsItem.Text = Text.Settings;
        _autoStartItem.Text = Text.StartWithWindows;
        _quitItem.Text = Text.Quit;
        Apply();
    }

    public void SetWheel(bool on)
    {
        _settings.Wheel = on;
        _filter.SetWheel(on);
        Apply();
    }

    public void SetShakeBrake(bool on)
    {
        _settings.ShakeBrake = on;
        _brake.Reset();
        _filter.SetPointerSpeed(100);
        Apply();
    }

    public void SetShakeSlowdown(int percent)
    {
        _settings.ShakeSlowdown = percent;
        _settings.Normalize();
        _brake.MaxSlowdown = _settings.ShakeSlowdown / 100.0;
        Apply();
    }

    /// <summary>True when Tremors moves the cursor itself: the filter or the shake brake is on.</summary>
    private bool Steering => _filter.Active || _settings.ShakeBrake;

    public void SetAutoStart(bool on)
    {
        _settings.AutoStart = on;
        if (on) AutoStart.Enable(); else AutoStart.Disable();
        Apply();
    }

    /// <summary>Brings hook, timer, menu and icon in line with the settings, then saves.</summary>
    private void Apply()
    {
        if (_settings.Enabled)
        {
            try
            {
                _mouse.Install();
            }
            catch (Exception ex)
            {
                _settings.Enabled = false;
                _tray.ShowBalloonTip(8000, "Tremors", Text.HookFailed + ex.Message, ToolTipIcon.Error);
            }
        }
        if (!_settings.Enabled)
        {
            _mouse.Uninstall();
            _filter.Reset();
            _clicks.Reset();
        }
        _timer.Enabled = _mouse.Installed;
        if (_mouse.Installed && Steering) PointerAcceleration.Suspend();
        else PointerAcceleration.Restore();

        _activeItem.Checked = _settings.Enabled;
        _autoStartItem.Checked = _settings.AutoStart;
        _tray.Icon = _settings.Enabled ? _iconOn : _iconOff;
        _tray.Text = _settings.Enabled ? Text.TooltipOn(_settings.Intensity) : Text.TooltipOff;

        Store.Save(_settings);
        Changed?.Invoke();
    }

    private bool OnMouse(int message, Mouse.MSLLHOOKSTRUCT data)
    {
        // Software-generated input (remote desktop, on-screen tools, Tremors itself) passes untouched.
        if ((data.flags & Mouse.LLMHF_INJECTED) != 0) return true;

        long now = Environment.TickCount64;
        switch (message)
        {
            case Mouse.WM_MOUSEMOVE:
                if (_clicks.Waiting) return false; // keep the cursor where the button went down
                if (!Steering) return true;
                var cursor = Mouse.CursorPosition();
                if (_settings.ShakeBrake)
                {
                    // data.pt is where Windows would put the cursor, so this is the hand's movement.
                    double factor = _brake.Update(data.pt.X - cursor.X, data.pt.Y - cursor.Y, now);
                    _filter.SetPointerSpeed((int)Math.Round(100 * factor));
                }
                _filter.Move(data.pt.X, data.pt.Y, cursor, now);
                return false;

            case Mouse.WM_MOUSEWHEEL:
            case Mouse.WM_MOUSEHWHEEL:
                return _filter.Wheel((short)(data.mouseData >> 16), now);

            case Mouse.WM_LBUTTONDOWN:
                _filter.Click();
                return _clicks.Down(MouseButton.Left, now);
            case Mouse.WM_RBUTTONDOWN:
                _filter.Click();
                return _clicks.Down(MouseButton.Right, now);
            case Mouse.WM_MBUTTONDOWN:
                _filter.Click();
                return _clicks.Down(MouseButton.Middle, now);
            case Mouse.WM_XBUTTONDOWN:
                _filter.Click();
                return true;

            case Mouse.WM_LBUTTONUP:
                return _clicks.Up(MouseButton.Left);
            case Mouse.WM_RBUTTONUP:
                return _clicks.Up(MouseButton.Right);
            case Mouse.WM_MBUTTONUP:
                return _clicks.Up(MouseButton.Middle);

            default:
                return true;
        }
    }

    /// <summary>Lets the cursor finish gliding and lets held buttons count.</summary>
    private void OnTimer()
    {
        long now = Environment.TickCount64;
        _clicks.Tick(now);
        if (_filter.Settling) _filter.Step(now);
    }

    private void ShowSettings()
    {
        if (_form is null || _form.IsDisposed)
            _form = new SettingsForm(this);
        _form.Show();
        _form.WindowState = FormWindowState.Normal;
        _form.Activate();
    }

    protected override void ExitThreadCore()
    {
        _mouse.Dispose();
        _timer.Dispose();
        _showSettingsWait.Unregister(null);
        _showSettings.Dispose();
        _ui.Dispose();
        PointerAcceleration.Restore();
        _form?.Close();
        _tray.Visible = false;
        _tray.ContextMenuStrip?.Dispose();
        _tray.Dispose();
        _iconOn.Dispose();
        _iconOff.Dispose();
        base.ExitThreadCore();
    }
}
