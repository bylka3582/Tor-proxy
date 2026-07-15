using System.IO;

namespace TorProxy;

/// <summary>
/// User-writable locations for the standalone Tor-proxy app. Everything lives
/// under <c>%LOCALAPPDATA%\TorProxy</c> so the app is fully portable and needs
/// no administrator rights.
/// </summary>
public static class AppPaths
{
    public static string RootDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TorProxy");

    /// <summary>Extracted tor.exe / lyrebird.exe / hashes.txt.</summary>
    public static string BinDirectory => Path.Combine(RootDirectory, "bin");

    /// <summary>Tor's DataDirectory (state, cached descriptors, generated torrc).</summary>
    public static string DataDirectory => Path.Combine(RootDirectory, "data");

    /// <summary>User's bridge lines (seeded with working snowflake bridges).</summary>
    public static string BridgesPath => Path.Combine(RootDirectory, "bridges.txt");

    /// <summary>Crash/exception log (hard-capped, never allowed to run away).</summary>
    public static string ErrorLogPath => Path.Combine(RootDirectory, "error.log");

    /// <summary>The two built-in snowflake bridges (work when obfs4 IPs are blocked).</summary>
    private static readonly string[] SeedBridges =
    {
        "# Мосты Tor, по одному в строке. '#' — комментарий.",
        "# Здесь встроенные мосты Snowflake — они работают, когда обычные адреса заблокированы.",
        "# Больше мостов (obfs4): https://bridges.torproject.org или Telegram @GetBridgesBot.",
        "",
        "snowflake 192.0.2.3:80 2B280B23E1107BB62ABFC40DDCC8824814F80A72 fingerprint=2B280B23E1107BB62ABFC40DDCC8824814F80A72 url=https://1098762253.rsc.cdn77.org/ fronts=app.datapacket.com,www.datapacket.com ice=stun:stun.epygi.com:3478,stun:stun.uls.co.za:3478,stun:stun.voipgate.com:3478,stun:stun.mixvoip.com:3478,stun:stun.telnyx.com:3478,stun:stun.hot-chilli.net:3478,stun:stun.fitauto.ru:3478,stun:stun.m-online.net:3478 utls-imitate=hellorandomizedalpn",
        "snowflake 192.0.2.4:80 8838024498816A039FCBBAB14E6F40A0843051FA fingerprint=8838024498816A039FCBBAB14E6F40A0843051FA url=https://1098762253.rsc.cdn77.org/ fronts=app.datapacket.com,www.datapacket.com ice=stun:stun.epygi.com:3478,stun:stun.uls.co.za:3478,stun:stun.voipgate.com:3478,stun:stun.mixvoip.com:3478,stun:stun.telnyx.com:3478,stun:stun.hot-chilli.net:3478,stun:stun.fitauto.ru:3478,stun:stun.m-online.net:3478 utls-imitate=hellorandomizedalpn",
    };

    /// <summary>Create directories and seed bridges.txt on first run.</summary>
    public static void EnsureSeeded()
    {
        Directory.CreateDirectory(RootDirectory);
        Directory.CreateDirectory(BinDirectory);
        Directory.CreateDirectory(DataDirectory);
        if (!File.Exists(BridgesPath))
            File.WriteAllLines(BridgesPath, SeedBridges);
    }
}
