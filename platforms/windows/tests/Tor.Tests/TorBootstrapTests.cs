using DpiBypass.Tor;

namespace DpiBypass.Tor.Tests;

public class TorBootstrapTests
{
    [Fact]
    public void Parses_percent_tag_and_message()
    {
        const string line = "Jul 13 12:00:00.000 [notice] Bootstrapped 45% (conn_done): Connected to a relay";

        Assert.True(TorBootstrap.TryParse(line, out var progress));
        Assert.Equal(45, progress.Percent);
        Assert.Equal("conn_done", progress.Tag);
        Assert.Contains("Connected", progress.Message);
    }

    [Fact]
    public void Parses_completion()
    {
        Assert.True(TorBootstrap.TryParse("[notice] Bootstrapped 100% (done): Done", out var progress));
        Assert.Equal(100, progress.Percent);
        Assert.Equal("done", progress.Tag);
    }

    [Theory]
    [InlineData("")]
    [InlineData("[notice] Opening Socks listener")]
    [InlineData("random log line")]
    public void Non_bootstrap_lines_return_false(string line)
        => Assert.False(TorBootstrap.TryParse(line, out _));
}
