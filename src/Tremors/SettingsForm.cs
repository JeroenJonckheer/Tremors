namespace Tremors;

/// <summary>
/// The settings window: four sliders in two columns and three checkboxes, in large
/// text. Every change takes effect immediately; there is no Save button.
/// </summary>
internal sealed class SettingsForm : Form
{
    private const int ColumnWidth = 340; // logical pixels at 100 % display scaling

    private readonly TrayApp _app;
    private readonly CheckBox _active = NewCheckBox();
    private readonly Slider _strength;
    private readonly Slider _speed;
    private readonly Slider _stillZone;
    private readonly Slider _clickHold;
    private readonly CheckBox _wheel = NewCheckBox();
    private readonly CheckBox _autoStart = NewCheckBox();
    private readonly Button _done = new() { AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(0, 16, 0, 0) };
    private readonly LinkLabel _about = new()
    {
        AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Margin = new Padding(0, 16, 0, 4),
    };
    private readonly List<(string Code, Button Button)> _flags = [];
    private bool _refreshing;

    public SettingsForm(TrayApp app)
    {
        _app = app;

        Text = "Tremors";
        Icon = TrayIcon.Create(active: true);
        Font = new Font(SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif, 12f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowOnly; // a shorter language does not make the window jump
        Padding = new Padding(LogicalToDeviceUnits(16));

        int width = LogicalToDeviceUnits(ColumnWidth);
        _strength = new Slider(0, 100, 5, width);
        _speed = new Slider(Settings.MinPointerSpeed, 100, 5, width);
        _stillZone = new Slider(0, Settings.MaxStillZone, 1, width);
        _clickHold = new Slider(0, Settings.MaxClickHold, 10, width);

        _done.Click += (_, _) => Close();
        _about.Font = new Font(Font.FontFamily, 10f);
        _about.LinkClicked += (_, _) =>
        {
            using var about = new AboutForm(Font);
            about.ShowDialog(this);
        };
        AcceptButton = _done;
        CancelButton = _done;

        var grid = new TableLayoutPanel
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2,
            Location = new Point(Padding.Left, Padding.Top),
        };
        int gap = LogicalToDeviceUnits(24);
        _speed.Margin = _clickHold.Margin = new Padding(gap, _speed.Margin.Top, 0, 0);
        _autoStart.Margin = new Padding(gap, _wheel.Margin.Top, 0, 0);
        grid.Controls.Add(_active, 0, 0);
        grid.Controls.Add(LanguageButtons(), 1, 0);
        grid.Controls.Add(_strength, 0, 1);
        grid.Controls.Add(_speed, 1, 1);
        grid.Controls.Add(_stillZone, 0, 2);
        grid.Controls.Add(_clickHold, 1, 2);
        grid.Controls.Add(_wheel, 0, 3);
        grid.Controls.Add(_autoStart, 1, 3);
        grid.Controls.Add(_about, 0, 4);
        grid.Controls.Add(_done, 1, 4);
        Controls.Add(grid);

        _active.CheckedChanged += (_, _) => { if (!_refreshing) _app.SetEnabled(_active.Checked); };
        _strength.Bar.ValueChanged += (_, _) => { if (!_refreshing) _app.SetIntensity(_strength.Bar.Value); };
        _speed.Bar.ValueChanged += (_, _) => { if (!_refreshing) _app.SetPointerSpeed(_speed.Bar.Value); };
        _stillZone.Bar.ValueChanged += (_, _) => { if (!_refreshing) _app.SetStillZone(_stillZone.Bar.Value); };
        _clickHold.Bar.ValueChanged += (_, _) => { if (!_refreshing) _app.SetClickHold(_clickHold.Bar.Value); };
        _wheel.CheckedChanged += (_, _) => { if (!_refreshing) _app.SetWheel(_wheel.Checked); };
        _autoStart.CheckedChanged += (_, _) => { if (!_refreshing) _app.SetAutoStart(_autoStart.Checked); };

        _app.Changed += RefreshFromSettings;
        FormClosed += (_, _) =>
        {
            _app.Changed -= RefreshFromSettings;
            Icon?.Dispose();
            _about.Font.Dispose();
        };
        RefreshFromSettings();
    }

    /// <summary>One flag per language; the one in use is outlined.</summary>
    private FlowLayoutPanel LanguageButtons()
    {
        var panel = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.Right | AnchorStyles.Top, Margin = Padding.Empty };
        var tips = new ToolTip();
        Disposed += (_, _) => tips.Dispose();
        int w = LogicalToDeviceUnits(30), h = LogicalToDeviceUnits(20);
        foreach (var language in Tremors.Text.All)
        {
            var flag = new Button
            {
                Image = Flags.Draw(language.Code, w, h),
                Size = new Size(w + LogicalToDeviceUnits(10), h + LogicalToDeviceUnits(10)),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(LogicalToDeviceUnits(4), 0, 0, 0),
                AccessibleName = language.Name,
                TabStop = true,
            };
            flag.FlatAppearance.BorderColor = SystemColors.Highlight;
            string code = language.Code;
            flag.Click += (_, _) => { if (code != Tremors.Text.CurrentCode) _app.SetLanguage(code); };
            flag.Disposed += (_, _) => flag.Image?.Dispose();
            tips.SetToolTip(flag, language.Name);
            panel.Controls.Add(flag);
            _flags.Add((code, flag));
        }
        return panel;
    }

    /// <summary>Shows the current settings, in the current language.</summary>
    private void RefreshFromSettings()
    {
        var s = _app.Settings;
        _refreshing = true;
        SuspendLayout();

        _active.Text = Tremors.Text.IsOn;
        _active.Checked = s.Enabled;
        _strength.Show(s.Intensity, Tremors.Text.Strength(s.Intensity), Tremors.Text.StrengthHint, s.Enabled);
        _speed.Show(s.PointerSpeed, Tremors.Text.PointerSpeed(s.PointerSpeed), Tremors.Text.PointerSpeedHint, s.Enabled);
        _stillZone.Show(s.StillZone, Tremors.Text.StillZone(s.StillZone), Tremors.Text.StillZoneHint, s.Enabled);
        _clickHold.Show(s.ClickHold, Tremors.Text.ClickHold(s.ClickHold), Tremors.Text.ClickHoldHint, s.Enabled);
        _wheel.Text = Tremors.Text.SteadyWheel;
        _wheel.Checked = s.Wheel;
        _wheel.Enabled = s.Enabled;
        _autoStart.Text = Tremors.Text.StartWithWindows;
        _autoStart.Checked = s.AutoStart;
        _done.Text = Tremors.Text.Done;
        _about.Text = Tremors.Text.About;
        foreach (var (code, button) in _flags)
            button.FlatAppearance.BorderSize = code == Tremors.Text.CurrentCode ? LogicalToDeviceUnits(2) : 0;

        ResumeLayout();
        _refreshing = false;
    }

    private static CheckBox NewCheckBox() => new()
    {
        AutoSize = true, Margin = new Padding(0, 10, 0, 0),
    };

    /// <summary>A caption showing the current value, a slider and a grey explanation.</summary>
    private sealed class Slider : FlowLayoutPanel
    {
        private readonly Label _caption = new() { AutoSize = true, Margin = Padding.Empty };
        private readonly Label _explanation;

        public TrackBar Bar { get; }

        public Slider(int minimum, int maximum, int step, int width)
        {
            FlowDirection = FlowDirection.TopDown;
            WrapContents = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Margin = new Padding(0, 14, 0, 0);

            Bar = new TrackBar
            {
                Minimum = minimum, Maximum = maximum, SmallChange = step, LargeChange = step * 2,
                TickFrequency = Math.Max(1, (maximum - minimum) / 10), Width = width, Margin = new Padding(0, 4, 0, 0),
            };
            _explanation = new Label
            {
                AutoSize = true, ForeColor = SystemColors.GrayText, MaximumSize = new Size(width, 0), Margin = Padding.Empty,
            };
            Controls.AddRange([_caption, Bar, _explanation]);
        }

        public void Show(int value, string caption, string explanation, bool enabled)
        {
            Bar.Value = Math.Clamp(value, Bar.Minimum, Bar.Maximum);
            Bar.Enabled = enabled;
            _caption.Text = caption;
            _explanation.Text = explanation;
        }
    }
}
