using System.Text.RegularExpressions;

namespace DpiBypass.Tor;

/// <summary>Parsed Tor bootstrap progress from a log line.</summary>
public readonly record struct BootstrapProgress(int Percent, string Tag, string Message);

/// <summary>Parses Tor's "Bootstrapped NN% (tag): message" log lines.</summary>
public static partial class TorBootstrap
{
    [GeneratedRegex(@"Bootstrapped (\d{1,3})% \(([^)]*)\):\s*(.*)$")]
    private static partial Regex BootstrapLine();

    public static bool TryParse(string logLine, out BootstrapProgress progress)
    {
        progress = default;
        if (string.IsNullOrEmpty(logLine))
            return false;

        var match = BootstrapLine().Match(logLine);
        if (!match.Success)
            return false;

        int percent = Math.Clamp(int.Parse(match.Groups[1].Value), 0, 100);
        progress = new BootstrapProgress(percent, match.Groups[2].Value, match.Groups[3].Value.Trim());
        return true;
    }
}
