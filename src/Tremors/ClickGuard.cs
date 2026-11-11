namespace Tremors;

/// <summary>
/// Ignores accidental clicks: a mouse button only counts once it has been held for
/// <see cref="HoldMs"/>. A tap that is released sooner is dropped.
/// </summary>
/// <remarks>
/// The physical button-down is swallowed. When the button is still down after the hold
/// time, the down is sent on to Windows, and the physical button-up later passes through
/// normally. So dragging works, and a double-click keeps its rhythm because both clicks
/// are delayed by the same amount. Not thread-safe; UI thread only.
/// </remarks>
internal sealed class ClickGuard(Action<MouseButton> sendDown)
{
    private enum State { Idle, Pending, Sent }

    private readonly State[] _state = new State[3];
    private readonly long[] _downAt = new long[3];

    /// <summary>Hold time in milliseconds; 0 lets every click through.</summary>
    public int HoldMs { get; set; }

    /// <summary>True while a button is down but does not count yet.</summary>
    public bool Waiting => Array.IndexOf(_state, State.Pending) >= 0;

    /// <summary>Returns true to pass the button-down on, false to hold it back.</summary>
    public bool Down(MouseButton button, long now)
    {
        if (HoldMs <= 0) return true;
        _state[(int)button] = State.Pending;
        _downAt[(int)button] = now;
        return false;
    }

    /// <summary>Returns true to pass the button-up on, false when the click was too short.</summary>
    public bool Up(MouseButton button)
    {
        var state = _state[(int)button];
        _state[(int)button] = State.Idle;
        return state != State.Pending;
    }

    /// <summary>Sends the button-down for every button that has now been held long enough.</summary>
    public void Tick(long now)
    {
        for (int i = 0; i < _state.Length; i++)
        {
            if (_state[i] != State.Pending || now - _downAt[i] < HoldMs) continue;
            _state[i] = State.Sent;
            sendDown((MouseButton)i);
        }
    }

    /// <summary>Forget all buttons, for example when Tremors is paused.</summary>
    public void Reset() => Array.Clear(_state);
}

internal enum MouseButton { Left, Right, Middle }
