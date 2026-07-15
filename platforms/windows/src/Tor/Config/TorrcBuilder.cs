using System.Text;

namespace DpiBypass.Tor.Config;

/// <summary>Builds a torrc configuration file from options and bridge lines.</summary>
public static class TorrcBuilder
{
    // Pluggable transports provided by lyrebird (the single PT binary shipped in
    // the Tor Expert Bundle). obfs4 built-in bridges are IP-blocked on some ISPs
    // where snowflake still bootstraps, so both must be wired when present.
    private static readonly string[] PluggableTransports =
        { "obfs4", "obfs3", "meek_lite", "snowflake", "webtunnel", "scramblesuit" };

    /// <summary>
    /// Produce torrc text. If bridges are supplied and a pluggable-transport
    /// binary (lyrebird/obfs4proxy) path is given, configures UseBridges plus one
    /// <c>ClientTransportPlugin</c> line per transport that appears in the bridge
    /// lines (obfs4, snowflake, …), all executing the same PT binary.
    /// </summary>
    public static string Build(TorOptions options, IReadOnlyList<string> bridges)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"SocksPort 127.0.0.1:{options.SocksPort}");
        if (!string.IsNullOrWhiteSpace(options.DataDirectory))
            sb.AppendLine($"DataDirectory {options.DataDirectory}");
        sb.AppendLine("Log notice stdout");

        bool useBridges = bridges.Count > 0;
        sb.AppendLine($"UseBridges {(useBridges ? 1 : 0)}");

        if (useBridges)
        {
            if (!string.IsNullOrWhiteSpace(options.Obfs4ProxyPath))
            {
                foreach (var transport in TransportsIn(bridges))
                    sb.AppendLine($"ClientTransportPlugin {transport} exec {options.Obfs4ProxyPath}");
            }

            foreach (var bridge in bridges)
                sb.AppendLine($"Bridge {bridge.Trim()}");
        }

        return sb.ToString();
    }

    /// <summary>Distinct pluggable transports named at the start of the bridge lines, in first-seen order.</summary>
    private static IEnumerable<string> TransportsIn(IReadOnlyList<string> bridges)
    {
        var seen = new List<string>();
        foreach (var bridge in bridges)
        {
            var first = bridge.TrimStart().Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
            if (first.Length == 0)
                continue;
            var match = PluggableTransports.FirstOrDefault(
                t => t.Equals(first[0], StringComparison.OrdinalIgnoreCase));
            if (match is not null && !seen.Contains(match))
                seen.Add(match);
        }
        return seen;
    }
}
