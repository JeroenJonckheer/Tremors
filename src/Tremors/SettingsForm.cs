namespace Tremors;

/// <summary>
/// The settings window. Deliberately simple and large: one slider and three
/// checkboxes. Every change takes effect immediately; there is no Save button.
/// </summary>
internal sealed class SettingsForm : Form
{
    private readonly TrayApp _app;
    private readonly CheckBox _active = NewCheckBox(Tremors.Text.IsOn);
    private readonly Label _strengthLabel = new() { AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
    private readonly TrackBar _strength = new()
    {
        Minimum = 0, Maximum = 100, TickFrequency = 10, SmallChange = 5, LargeChange = 10,
        Width = 340, Margin = new Padding(0, 4, 0, 0),
    };
    private readonly Label _hint = new()
    {
        AutoSize = true, ForeColor = SystemColors.GrayText, MaximumSize = new Size(340, 0),
        Text = Tremors.Text.StrengthHint,
    };
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
        layout.Controls.AddRange([_active, _strengthLabel, _strength, _hint, _wheel, _autoStart, done]);
        Controls.Add(layout);

        _active.CheckedChanged += (_, _) => { if (!_refreshing) _app.SetEnabled(_active.Checked); };
        _strength.ValueChanged += (_, _) => { if (!_refreshing) _app.SetIntensity(_strength.Value); };
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
        _strength.Value = s.Intensity;
        _strengthLabel.Text = Tremors.Text.Strength(s.Intensity);
        _wheel.Checked = s.Wheel;
        _autoStart.Checked = s.AutoStart;
        _strength.Enabled = _wheel.Enabled = s.Enabled;
        _refreshing = false;
    }

    private static CheckBox NewCheckBox(string text) => new()
    {
        Text = text, AutoSize = true, Margin = new Padding(0, 10, 0, 0),
    };
}
