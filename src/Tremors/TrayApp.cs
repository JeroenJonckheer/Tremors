namespace Tremors;

/// <summary>
/// The whole application: a tray icon with a right-click menu, the mouse hook,
/// the filter and a small settings window. Nothing else appears on screen.
/// </summary>
internal sealed class TrayApp : ApplicationContext
{
    private readonly Settings _settings;
    private readonly Smoother _filter = new();
    private readonly Mouse _mouse = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 10 };
    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _activeItem;
    private readonly ToolStripMenuItem _autoStartItem;
    private readonly Icon _iconOn = TrayIcon.Create(active: true);
    private readonly Icon _iconOff = TrayIcon.Create(active: false);
    private SettingsForm? _form;

    /// <summary>Raised after any setting changed, so open windows can refresh.</summary>
    public event Action? Changed;

    public Settings Settings => _settings;

    public TrayApp()
    {
        _settings = Store.Load(out bool firstRun);
        _filter.Configure(_settings.Intensity);
        _filter.SetWheel(_settings.Wheel);
        _mouse.Filter = OnMouse;
        _timer.Tick += (_, _) => OnTimer();
        if (_settings.AutoStart) AutoStart.Enable(); // keep the path current if the exe moved

        _activeItem = new ToolStripMenuItem(Text.Active, null, (_, _) => SetEnabled(!_settings.Enabled));
        _autoStartItem = new ToolStripMenuItem(Text.StartWithWindows, null, (_, _) => SetAutoStart(!_settings.AutoStart));
        var menu = new ContextMenuStrip();
        menu.Items.Add(_activeItem);
        menu.Items.Add(new ToolStripMenuItem(Text.Settings, null, (_, _) => ShowSettings()));
        menu.Items.Add(_autoStartItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Text.Quit, null, (_, _) => ExitThread()));

        _tray = new NotifyIcon { ContextMenuStrip = menu, Visible = true };
        _tray.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowSettings(); };

        Apply();

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

    public void SetWheel(bool on)
    {
        _settings.Wheel = on;
        _filter.SetWheel(on);
        Apply();
    }

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
        }
        _timer.Enabled = _mouse.Installed;

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
                if (!_filter.Active) return true;
                _filter.Move(data.pt.X, data.pt.Y, Mouse.CursorPosition(), now);
                return false;

            case Mouse.WM_MOUSEWHEEL:
            case Mouse.WM_MOUSEHWHEEL:
                return _filter.Wheel((short)(data.mouseData >> 16), now);

            case Mouse.WM_LBUTTONDOWN:
            case Mouse.WM_RBUTTONDOWN:
            case Mouse.WM_MBUTTONDOWN:
            case Mouse.WM_XBUTTONDOWN:
                _filter.Click();
                return true;

            default:
                return true;
        }
    }

    /// <summary>Lets the cursor finish gliding after the hand has stopped.</summary>
    private void OnTimer()
    {
        if (_filter.Settling) _filter.Step(Environment.TickCount64);
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
        _form?.Close();
        _tray.Visible = false;
        _tray.ContextMenuStrip?.Dispose();
        _tray.Dispose();
        _iconOn.Dispose();
        _iconOff.Dispose();
        base.ExitThreadCore();
    }
}
