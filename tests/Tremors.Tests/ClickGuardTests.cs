using Xunit;

namespace Tremors.Tests;

public class ClickGuardTests
{
    private readonly List<MouseButton> _sent = [];
    private readonly ClickGuard _guard;

    public ClickGuardTests() => _guard = new ClickGuard(_sent.Add) { HoldMs = 150 };

    /// <summary>Runs the timer from <paramref name="from"/> to <paramref name="to"/> in 10 ms steps.</summary>
    private void Run(long from, long to)
    {
        for (long t = from; t <= to; t += 10) _guard.Tick(t);
    }

    [Fact]
    public void Off_lets_every_click_through()
    {
        _guard.HoldMs = 0;

        Assert.True(_guard.Down(MouseButton.Left, 1000));
        Assert.True(_guard.Up(MouseButton.Left));
        Assert.Empty(_sent);
    }

    [Fact]
    public void Short_tap_is_ignored()
    {
        Assert.False(_guard.Down(MouseButton.Left, 1000));
        Run(1000, 1100);

        Assert.False(_guard.Up(MouseButton.Left));
        Run(1100, 1500);
        Assert.Empty(_sent);
    }

    [Fact]
    public void Held_click_counts_once_the_hold_time_has_passed()
    {
        _guard.Down(MouseButton.Left, 1000);
        Run(1000, 1140);
        Assert.Empty(_sent);
        Assert.True(_guard.Waiting);

        Run(1150, 1150);
        Assert.Equal([MouseButton.Left], _sent);
        Assert.False(_guard.Waiting);

        Assert.True(_guard.Up(MouseButton.Left)); // the real button-up completes the click
    }

    [Fact]
    public void Drag_keeps_the_button_down_until_release()
    {
        _guard.Down(MouseButton.Left, 1000);
        Run(1000, 3000); // held for two seconds while dragging

        Assert.Single(_sent);
        Assert.True(_guard.Up(MouseButton.Left));
    }

    [Fact]
    public void Double_click_keeps_its_rhythm()
    {
        var sentAt = new List<long>();
        var guard = new ClickGuard(_ => sentAt.Add(_now)) { HoldMs = 150 };

        guard.Down(MouseButton.Left, 1000);
        Advance(guard, 1000, 1160);
        guard.Up(MouseButton.Left);
        guard.Down(MouseButton.Left, 1300);
        Advance(guard, 1160, 1460);
        guard.Up(MouseButton.Left);

        Assert.Equal(2, sentAt.Count);
        Assert.Equal(300, sentAt[1] - sentAt[0]); // same gap as between the physical presses
    }

    private long _now;

    private void Advance(ClickGuard guard, long from, long to)
    {
        for (_now = from; _now <= to; _now += 10) guard.Tick(_now);
    }

    [Fact]
    public void Buttons_are_independent()
    {
        _guard.Down(MouseButton.Left, 1000);
        _guard.Down(MouseButton.Right, 1100);
        Run(1000, 1160);
        Assert.Equal([MouseButton.Left], _sent);

        Assert.False(_guard.Up(MouseButton.Right)); // right was only a tap
        Run(1170, 1400);
        Assert.Equal([MouseButton.Left], _sent);
    }

    [Fact]
    public void Reset_forgets_held_buttons()
    {
        _guard.Down(MouseButton.Left, 1000);

        _guard.Reset();
        Run(1000, 1500);

        Assert.Empty(_sent);
        Assert.False(_guard.Waiting);
    }
}

public class MouseInteropTests
{
    [Fact]
    public void Input_struct_has_the_native_size()
    {
        var input = typeof(Mouse).GetNestedType("INPUT", System.Reflection.BindingFlags.NonPublic)!;

        int size = System.Runtime.InteropServices.Marshal.SizeOf(input);

        Assert.Equal(Environment.Is64BitProcess ? 40 : 28, size); // sizeof(INPUT) in C
    }
}
