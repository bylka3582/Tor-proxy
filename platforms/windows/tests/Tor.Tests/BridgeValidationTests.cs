using DpiBypass.Tor.Bridges;

namespace DpiBypass.Tor.Tests;

public class BridgeValidationTests
{
    [Theory]
    [InlineData("obfs4 1.2.3.4:443 0123456789ABCDEF0123456789ABCDEF01234567 cert=abc iat-mode=0")]
    [InlineData("1.2.3.4:9001 0123456789ABCDEF0123456789ABCDEF01234567")]
    [InlineData("obfs4 198.51.100.7:8443 0123456789abcdef0123456789abcdef01234567 cert=xyz")]
    [InlineData("snowflake 192.0.2.3:80 2B280B23E1107BB62ABFC40DDCC8824814F80A72")]
    public void Valid_bridge_lines_pass(string line)
        => Assert.True(BridgeValidation.IsValidBridgeLine(line));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("obfs4")]
    [InlineData("obfs4 not-an-address FINGERPRINT")]
    [InlineData("obfs4 1.2.3.4 0123456789ABCDEF0123456789ABCDEF01234567")] // no port
    [InlineData("obfs4 1.2.3.4:70000 0123456789ABCDEF0123456789ABCDEF01234567")] // bad port
    [InlineData("1.2.3.4:443 FINGERPRINT extra-without-equals")]
    public void Invalid_bridge_lines_fail(string? line)
        => Assert.False(BridgeValidation.IsValidBridgeLine(line));
}
