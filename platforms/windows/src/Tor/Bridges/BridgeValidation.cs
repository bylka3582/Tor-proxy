using System.Net;
using System.Text.RegularExpressions;

namespace DpiBypass.Tor.Bridges;

/// <summary>
/// Validates Tor bridge lines. Supports pluggable-transport bridges
/// (e.g. <c>obfs4 1.2.3.4:443 FINGERPRINT cert=… iat-mode=0</c>) and plain
/// bridges (<c>1.2.3.4:443 FINGERPRINT</c>).
/// </summary>
public static partial class BridgeValidation
{
    private static readonly string[] KnownTransports =
        { "obfs4", "obfs3", "meek_lite", "snowflake", "webtunnel", "scramblesuit" };

    [GeneratedRegex(@"^[0-9a-fA-F]{40}$")]
    private static partial Regex Fingerprint();

    /// <summary>True if the line is a syntactically plausible bridge line.</summary>
    public static bool IsValidBridgeLine(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        var tokens = line.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        int i = 0;

        // Optional transport name.
        bool hasTransport = KnownTransports.Contains(tokens[0], StringComparer.OrdinalIgnoreCase);
        if (hasTransport)
            i = 1;

        // Need an address token next.
        if (i >= tokens.Length || !IsHostPort(tokens[i]))
            return false;
        i++;

        // Optional fingerprint (40 hex). Plain bridges usually include it.
        if (i < tokens.Length && Fingerprint().IsMatch(tokens[i]))
            i++;

        // Any remaining tokens must look like key=value transport params.
        for (; i < tokens.Length; i++)
        {
            if (!tokens[i].Contains('='))
                return false;
        }
        return true;
    }

    private static bool IsHostPort(string token)
    {
        int colon = token.LastIndexOf(':');
        if (colon <= 0 || colon == token.Length - 1)
            return false;

        var host = token[..colon].Trim('[', ']'); // allow [ipv6]:port
        var portText = token[(colon + 1)..];

        if (!int.TryParse(portText, out int port) || port < 1 || port > 65535)
            return false;

        // Host may be an IP or a hostname; accept an IP or a non-empty host.
        return IPAddress.TryParse(host, out _) || host.Length > 0;
    }
}
