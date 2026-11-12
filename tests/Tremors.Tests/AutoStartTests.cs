using Xunit;

namespace Tremors.Tests;

public class AutoStartTests
{
    [Theory]
    [InlineData(new string[0], false)]                       // started by hand
    [InlineData(new[] { "--autostart" }, true)]              // started by Windows at sign-in
    [InlineData(new[] { "--AUTOSTART" }, true)]
    [InlineData(new[] { "--something-else" }, false)]
    public void Knows_whether_Windows_started_it(string[] args, bool expected)
    {
        Assert.Equal(expected, AutoStart.StartedByWindows(args));
    }
}
