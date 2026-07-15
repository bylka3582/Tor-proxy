namespace DpiBypass.Tor;

/// <summary>
/// Configuration for the Tor fallback. This is an <b>independent</b> feature: it
/// routes traffic for IP-blocked destinations through the Tor network (optionally
/// via obfs4 bridges). Unlike the DPI bypass, this is NOT local-only — it is a
/// separate, opt-in tool with different trade-offs (slower, but reaches routes
/// the provider drops).
/// </summary>
public sealed record TorOptions
{
    /// <summary>Local SOCKS5 port Tor listens on (9150 to avoid a system Tor on 9050).</summary>
    public int SocksPort { get; init; } = 9150;

    /// <summary>Tor data directory (state, cached descriptors).</summary>
    public string DataDirectory { get; init; } = string.Empty;

    /// <summary>Explicit path to tor.exe; auto-located when null.</summary>
    public string? TorExecutablePath { get; init; }

    /// <summary>Explicit path to the obfs4 pluggable-transport binary; auto-located when null.</summary>
    public string? Obfs4ProxyPath { get; init; }

    /// <summary>Path to the user's bridge lines file (one per line).</summary>
    public string? BridgesPath { get; init; }

    /// <summary>The SOCKS5 endpoint applications should use once Ready.</summary>
    public string SocksEndpoint => $"127.0.0.1:{SocksPort}";
}
