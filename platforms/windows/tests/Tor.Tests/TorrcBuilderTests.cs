using DpiBypass.Tor;
using DpiBypass.Tor.Config;

namespace DpiBypass.Tor.Tests;

public class TorrcBuilderTests
{
    [Fact]
    public void Without_bridges_disables_UseBridges()
    {
        var torrc = TorrcBuilder.Build(new TorOptions { SocksPort = 9150 }, Array.Empty<string>());

        Assert.Contains("SocksPort 127.0.0.1:9150", torrc);
        Assert.Contains("UseBridges 0", torrc);
        Assert.DoesNotContain("ClientTransportPlugin", torrc);
    }

    [Fact]
    public void With_obfs4_bridges_configures_transport_and_bridges()
    {
        var options = new TorOptions
        {
            SocksPort = 9150,
            Obfs4ProxyPath = @"C:\tor\lyrebird.exe",
            DataDirectory = @"C:\data",
        };
        var bridges = new[]
        {
            "obfs4 1.2.3.4:443 0123456789ABCDEF0123456789ABCDEF01234567 cert=abc iat-mode=0",
        };

        var torrc = TorrcBuilder.Build(options, bridges);

        Assert.Contains("UseBridges 1", torrc);
        Assert.Contains(@"ClientTransportPlugin obfs4 exec C:\tor\lyrebird.exe", torrc);
        Assert.Contains("Bridge obfs4 1.2.3.4:443", torrc);
        Assert.Contains(@"DataDirectory C:\data", torrc);
    }

    [Fact]
    public void With_snowflake_bridges_configures_snowflake_transport()
    {
        var options = new TorOptions
        {
            SocksPort = 9150,
            Obfs4ProxyPath = @"C:\tor\lyrebird.exe",
        };
        var bridges = new[]
        {
            "snowflake 192.0.2.3:80 2B280B23E1107BB62ABFC40DDCC8824814F80A72 url=https://x/ fronts=a.com,b.com",
        };

        var torrc = TorrcBuilder.Build(options, bridges);

        Assert.Contains("UseBridges 1", torrc);
        Assert.Contains(@"ClientTransportPlugin snowflake exec C:\tor\lyrebird.exe", torrc);
        Assert.DoesNotContain("ClientTransportPlugin obfs4", torrc);
        Assert.Contains("Bridge snowflake 192.0.2.3:80", torrc);
    }

    [Fact]
    public void With_mixed_bridges_configures_each_transport_once()
    {
        var options = new TorOptions { Obfs4ProxyPath = @"C:\tor\lyrebird.exe" };
        var bridges = new[]
        {
            "obfs4 1.2.3.4:443 0123456789ABCDEF0123456789ABCDEF01234567 cert=abc iat-mode=0",
            "obfs4 5.6.7.8:443 0123456789ABCDEF0123456789ABCDEF01234568 cert=def iat-mode=0",
            "snowflake 192.0.2.3:80 2B280B23E1107BB62ABFC40DDCC8824814F80A72 url=https://x/",
        };

        var torrc = TorrcBuilder.Build(options, bridges);

        // Each transport wired exactly once, both via the same lyrebird binary.
        var lines = torrc.Split('\n');
        Assert.Single(lines, l => l.Contains("ClientTransportPlugin obfs4 exec"));
        Assert.Single(lines, l => l.Contains("ClientTransportPlugin snowflake exec"));
    }

    [Fact]
    public void Bridges_without_obfs4_path_still_list_bridges()
    {
        var torrc = TorrcBuilder.Build(
            new TorOptions(),
            new[] { "1.2.3.4:9001 0123456789ABCDEF0123456789ABCDEF01234567" });

        Assert.Contains("UseBridges 1", torrc);
        Assert.DoesNotContain("ClientTransportPlugin", torrc);
        Assert.Contains("Bridge 1.2.3.4:9001", torrc);
    }
}
