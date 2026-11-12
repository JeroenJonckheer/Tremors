namespace Tremors;

/// <summary>A small window with the version, the maker and a link to the latest version.</summary>
internal sealed class AboutForm : Form
{
    public AboutForm(Font font)
    {
        Text = Tremors.Text.About;
        Font = font;
        Icon = TrayIcon.Create(active: true);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(LogicalToDeviceUnits(16));

        string version = "Tremors " + Maker.Version;
        if (Maker.BuildMonth is { } month)
            version += " · " + Tremors.Text.Month(month);
        var title = new Label { AutoSize = true, Text = version, Margin = Padding.Empty };
        var maker = new Label { AutoSize = true, Text = Tremors.Text.MadeBy(Maker.Name), Margin = Padding.Empty };
        var free = new Label
        {
            AutoSize = true, Text = Tremors.Text.Free, ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, LogicalToDeviceUnits(8), 0, 0),
        };
        var latest = new LinkLabel
        {
            AutoSize = true, Text = Tremors.Text.Latest, Margin = new Padding(0, LogicalToDeviceUnits(8), 0, 0),
        };
        latest.LinkClicked += (_, _) => Maker.OpenLatest();
        var ok = new Button
        {
            AutoSize = true, Text = "OK", Anchor = AnchorStyles.Right,
            Margin = new Padding(0, LogicalToDeviceUnits(16), 0, 0),
        };
        ok.Click += (_, _) => Close();
        AcceptButton = ok;
        CancelButton = ok;

        var stack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink, Location = new Point(Padding.Left, Padding.Top),
        };
        stack.Controls.AddRange([title, maker, free, latest, ok]);
        Controls.Add(stack);
        FormClosed += (_, _) => Icon?.Dispose();
    }
}
