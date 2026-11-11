namespace Tremors;

/// <summary>
/// The settings window: three sliders and three checkboxes, in large text.
/// Every change takes effect immediately; there is no Save button.
/// </summary>
internal sealed class SettingsForm : Form
{
    private const int ContentWidth = 360;

    private readonly TrayApp _app;
    private readonly CheckBox _active = NewCheckBox(Tremors.Text.IsOn);
    private readonly Slider _strength = new(0, 100, 5, Tremors.Text.StrengthHint);
    private readonly Slider _speed = new(Settings.MinPointerSpeed, 100, 5, Tremors.Text.PointerSpeedHint);
    private readonly Slider _stillZone = new(0, Settings.MaxStillZone, 1, Tremors.Text.StillZoneHint);
    private readonly CheckBox _wheel = NewCheckBox(Tremors.Text.SteadyWheel);
    private readonly CheckBox _autoStart = NewCheckBox(Tremors.Text.StartWithWindows);
    private bool _refreshing;

    public SettingsForm(TrayApp app)
    {
        _app = app;

        Text = "Tremors";
        Icon = TrayIcon.Create(active: true);
        Font = new Font(SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);

        var done = new Button { Text = Tremors.Text.Done, AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(0, 16, 0, 0) };
        done.Click += (_, _) => Close();
        AcceptButton = done;
        CancelButton = done;

        var layout = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Dock = DockStyle.Fill };
        layout.Controls.Add(_active);
        _strength.AddTo(layout);
        _speed.AddTo(layout);
        _stillZone.AddTo(layout);
        layout.Controls.AddRange([_wheel, _autoStart, done]);
        Controls.Add(layout);

        _active.CheckedChanged += (_, _) => { if (!_refreshing) _app.SetEnabled(_active.Checked); };
        _strength.Bar.ValueChanged += (_, _) => { if (!_refreshing) _app.SetIntensity(_strength.Bar.Value); };
        _speed.Bar.ValueChanged += (_, _) => { if (!_refreshing) _app.SetPointerSpeed(_speed.Bar.Value); };
        _stillZone.Bar.ValueChanged += (_, _) => { if (!_refreshing) _app.SetStillZone(_stillZone.Bar.Value); };
        _wheel.CheckedChanged += (_, _) => { if (!_refreshing) _app.SetWheel(_wheel.Checked); };
        _autoStart.CheckedChanged += (_, _) => { if (!_refreshing) _app.SetAutoStart(_autoStart.Checked); };

        _app.Changed += RefreshFromSettings;
        FormClosed += (_, _) =>
        {
            _app.Changed -= RefreshFromSettings;
            Icon?.Dispose();
        };
        RefreshFromSettings();
    }

    private void RefreshFromSettings()
    {
        var s = _app.Settings;
        _refreshing = true;
        _active.Checked = s.Enabled;
        _strength.Show(s.Intensity, Tremors.Text.Strength(s.Intensity), s.Enabled);
        _speed.Show(s.PointerSpeed, Tremors.Text.PointerSpeed(s.PointerSpeed), s.Enabled);
        _stillZone.Show(s.StillZone, Tremors.Text.StillZone(s.StillZone), s.Enabled);
        _wheel.Checked = s.Wheel;
        _wheel.Enabled = s.Enabled;
        _autoStart.Checked = s.AutoStart;
        _refreshing = false;
    }

    private static CheckBox NewCheckBox(string text) => new()
    {
        Text = text, AutoSize = true, Margin = new Padding(0, 10, 0, 0),
    };

    /// <summary>A caption showing the current value, a slider and a grey explanation.</summary>
    private sealed class Slider(int minimum, int maximum, int step, string hint)
    {
        private readonly Label _caption = new() { AutoSize = true, Margin = new Padding(0, 14, 0, 0) };

        private readonly Label _hint = new()
        {
            AutoSize = true, ForeColor = SystemColors.GrayText, MaximumSize = new Size(ContentWidth, 0), Text = hint,
        };

        public TrackBar Bar { get; } = new()
        {
            Minimum = minimum, Maximum = maximum, SmallChange = step, LargeChange = step * 2,
            TickFrequency = Math.Max(1, (maximum - minimum) / 10), Width = ContentWidth, Margin = new Padding(0, 4, 0, 0),
        };

        public void AddTo(TableLayoutPanel layout) => layout.Controls.AddRange([_caption, Bar, _hint]);

        public void Show(int value, string caption, bool enabled)
        {
            Bar.Value = Math.Clamp(value, Bar.Minimum, Bar.Maximum);
            Bar.Enabled = enabled;
            _caption.Text = caption;
        }
    }
}
