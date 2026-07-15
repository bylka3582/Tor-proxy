using System.Security.Cryptography;

namespace DpiBypass.Tor;

/// <summary>Result of locating the Tor binaries.</summary>
public sealed record TorBinaryInfo(
    string TorExecutablePath,
    string? Obfs4ProxyPath,
    string Directory,
    bool HashVerified,
    string? Warning);

/// <summary>
/// Locates <c>tor.exe</c> (and optionally the obfs4 transport) and verifies
/// tor.exe against a pinned SHA-256 in <c>hashes.txt</c>. Official releases only.
/// </summary>
public static class TorBinary
{
    private const string TorName = "tor.exe";
    private static readonly string[] Obfs4Names = { "obfs4proxy.exe", "lyrebird.exe" };

    public static TorBinaryInfo? Locate(string? explicitTorPath = null, string? explicitObfs4Path = null)
    {
        foreach (var candidate in Candidates(explicitTorPath))
        {
            if (File.Exists(candidate))
                return Describe(Path.GetFullPath(candidate), explicitObfs4Path);
        }
        return null;
    }

    private static IEnumerable<string> Candidates(string? explicitPath)
    {
        if (!string.IsNullOrWhiteSpace(explicitPath))
            yield return explicitPath!;

        string appDir = AppContext.BaseDirectory;
        yield return Path.Combine(appDir, "tor", TorName);
        yield return Path.Combine(appDir, "tor", "pluggable_transports", TorName);
        yield return Path.Combine(appDir, TorName);

        var dir = new DirectoryInfo(appDir);
        for (int i = 0; i < 6 && dir is not null; i++, dir = dir.Parent)
            yield return Path.Combine(dir.FullName, "third_party", "tor", TorName);
    }

    private static TorBinaryInfo Describe(string torPath, string? explicitObfs4Path)
    {
        string dir = Path.GetDirectoryName(torPath)!;
        string? obfs4 = explicitObfs4Path ?? FindObfs4(dir);
        var (verified, warning) = VerifyHash(torPath, dir);
        return new TorBinaryInfo(torPath, obfs4, dir, verified, warning);
    }

    private static string? FindObfs4(string torDir)
    {
        foreach (var name in Obfs4Names)
        {
            foreach (var candidate in new[]
            {
                Path.Combine(torDir, name),
                Path.Combine(torDir, "pluggable_transports", name),
            })
            {
                if (File.Exists(candidate))
                    return candidate;
            }
        }
        return null;
    }

    private static (bool Verified, string? Warning) VerifyHash(string torPath, string dir)
    {
        string hashesFile = Path.Combine(dir, "hashes.txt");
        if (!File.Exists(hashesFile))
            return (false, "No hashes.txt found — tor.exe integrity is NOT verified. See third_party/README.md.");

        string? expected = null;
        foreach (var raw in File.ReadLines(hashesFile))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && parts[^1].TrimStart('*').Equals(TorName, StringComparison.OrdinalIgnoreCase))
            {
                expected = parts[0];
                break;
            }
        }

        if (expected is null)
            return (false, $"hashes.txt has no entry for {TorName} — integrity NOT verified.");

        using var stream = File.OpenRead(torPath);
        string actual = Convert.ToHexString(SHA256.HashData(stream));
        if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"tor.exe SHA-256 mismatch. Expected {expected}, got {actual}. Refusing to run.");

        return (true, null);
    }
}
