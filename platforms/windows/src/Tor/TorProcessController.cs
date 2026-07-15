using System.Diagnostics;
using DpiBypass.Tor.Bridges;
using DpiBypass.Tor.Config;

namespace DpiBypass.Tor;

/// <summary>
/// Runs the bundled <c>tor.exe</c> as a child process with a generated torrc,
/// parsing its bootstrap progress. Independent of the DPI bypass engine.
///
/// NOTE: the process/driver glue is written but NOT verified on hardware in this
/// project (needs the Tor binaries + network). The pure config/parse helpers
/// (torrc, bridge validation, bootstrap parsing) ARE unit-tested.
/// </summary>
public sealed class TorProcessController : ITorController
{
    private readonly TorOptions _options;
    private readonly object _gate = new();

    private TorStatus _status = TorStatus.Stopped;
    private int _bootstrap;
    private Process? _process;

    public TorProcessController(TorOptions options) => _options = options;

    public TorStatus Status { get { lock (_gate) return _status; } }
    public int BootstrapPercent { get { lock (_gate) return _bootstrap; } }
    public string SocksEndpoint => _options.SocksEndpoint;

    public event EventHandler<TorStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<string>? LogReceived;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (_status is TorStatus.Starting or TorStatus.Bootstrapping or TorStatus.Ready)
                return;
        }

        System.Threading.Interlocked.Exchange(ref _proxyAttempts, 0);
        SetStatus(TorStatus.Starting, 0, "Starting Tor…");

        var binary = TorBinary.Locate(_options.TorExecutablePath, _options.Obfs4ProxyPath);
        if (binary is null)
        {
            SetStatus(TorStatus.Faulted, 0,
                "tor.exe not found. Run scripts/fetch-tor.ps1 or place it in third_party/tor.");
            return;
        }
        if (binary.Warning is not null)
            LogReceived?.Invoke(this, $"[warn] {binary.Warning}");

        var bridges = LoadBridges();
        var effectiveOptions = _options with { Obfs4ProxyPath = binary.Obfs4ProxyPath };
        string torrc = TorrcBuilder.Build(effectiveOptions, bridges);

        string dataDir = string.IsNullOrWhiteSpace(_options.DataDirectory)
            ? Path.Combine(Path.GetTempPath(), "dpibypass-tor")
            : _options.DataDirectory;
        Directory.CreateDirectory(dataDir);
        string torrcPath = Path.Combine(dataDir, "torrc");
        await File.WriteAllTextAsync(torrcPath, torrc, cancellationToken).ConfigureAwait(false);
        LogReceived?.Invoke(this, $"[tor] torrc written to {torrcPath} ({bridges.Count} bridge(s)).");

        var process = CreateProcess(binary.TorExecutablePath, torrcPath, binary.Directory);
        try
        {
            if (!process.Start())
            {
                SetStatus(TorStatus.Faulted, 0, "Failed to start tor.exe.");
                process.Dispose();
                return;
            }
        }
        catch (Exception ex)
        {
            SetStatus(TorStatus.Faulted, 0, $"Failed to start tor.exe: {ex.Message}");
            process.Dispose();
            return;
        }

        // Tie tor to the app's job so it dies if the app is force-killed.
        ChildProcessJob.Shared.Assign(process);

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        lock (_gate) _process = process;

        SetStatus(TorStatus.Bootstrapping, 0, "Tor bootstrapping…");

        try { await Task.Delay(500, cancellationToken).ConfigureAwait(false); }
        catch (OperationCanceledException) { await KillProcessAsync(process).ConfigureAwait(false); SetStatus(TorStatus.Stopped, 0, "Start cancelled."); throw; }

        if (process.HasExited)
        {
            lock (_gate) _process = null;
            process.Dispose();
            SetStatus(TorStatus.Faulted, 0, "tor.exe exited immediately. Check bridges and binaries.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Process? process;
        lock (_gate)
        {
            process = _process;
            _process = null;
        }
        if (process is null)
        {
            if (Status != TorStatus.Stopped)
                SetStatus(TorStatus.Stopped, 0, "Stopped.");
            return;
        }

        SetStatus(TorStatus.Stopping, BootstrapPercent, "Stopping Tor…");
        process.OutputDataReceived -= OnOutput;
        process.ErrorDataReceived -= OnOutput;
        await KillProcessAsync(process).ConfigureAwait(false);
        process.Dispose();
        SetStatus(TorStatus.Stopped, 0, "Tor stopped.");
    }

    public async ValueTask DisposeAsync()
    {
        try { await StopAsync().ConfigureAwait(false); }
        catch { /* best effort */ }
    }

    private IReadOnlyList<string> LoadBridges()
    {
        if (string.IsNullOrWhiteSpace(_options.BridgesPath) || !File.Exists(_options.BridgesPath))
            return Array.Empty<string>();

        var valid = new List<string>();
        foreach (var raw in File.ReadLines(_options.BridgesPath))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            if (BridgeValidation.IsValidBridgeLine(line))
                valid.Add(line);
            else
                LogReceived?.Invoke(this, $"[warn] Ignoring invalid bridge line: {line}");
        }
        return valid;
    }

    private Process CreateProcess(string torExe, string torrcPath, string workingDir)
    {
        var psi = new ProcessStartInfo
        {
            FileName = torExe,
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        psi.ArgumentList.Add("-f");
        psi.ArgumentList.Add(torrcPath);

        var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        process.OutputDataReceived += OnOutput;
        process.ErrorDataReceived += OnOutput;
        return process;
    }

    private int _proxyAttempts;

    private void OnOutput(object? sender, DataReceivedEventArgs e)
    {
        if (e.Data is null)
            return;
        LogReceived?.Invoke(this, e.Data);

        if (TorBootstrap.TryParse(e.Data, out var progress))
        {
            var status = progress.Percent >= 100 ? TorStatus.Ready : TorStatus.Bootstrapping;
            SetStatus(status, progress.Percent,
                progress.Percent >= 100 ? $"Tor ready. SOCKS5 at {SocksEndpoint}." : $"Bootstrapping: {progress.Message}");
            return;
        }

        // Snowflake spends the first ~1-3 min cycling through volunteer WebRTC
        // proxies (many time out) before it finds a live one. Bootstrap % sits at
        // ~10% the whole time, which looks frozen. Surface the retries so the user
        // can see it is actively searching, not hung.
        if (e.Data.Contains("trying a new proxy", StringComparison.OrdinalIgnoreCase))
        {
            int attempts = System.Threading.Interlocked.Increment(ref _proxyAttempts);
            int status; int percent;
            lock (_gate) { status = (int)_status; percent = _bootstrap; }
            SetStatus((TorStatus)status, percent,
                $"Searching for a working Snowflake proxy (attempt {attempts})… " +
                "This is normal and can take 1-3 minutes.");
        }
    }

    private static async Task KillProcessAsync(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
        }
        catch { /* already gone or timed out */ }
    }

    private void SetStatus(TorStatus status, int percent, string? message)
    {
        lock (_gate)
        {
            _status = status;
            _bootstrap = percent;
        }
        StatusChanged?.Invoke(this, new TorStatusChangedEventArgs(status, percent, message));
        if (!string.IsNullOrWhiteSpace(message))
            LogReceived?.Invoke(this, $"[tor] {message}");
    }
}
