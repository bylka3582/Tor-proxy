namespace DpiBypass.Tor;

/// <summary>Raised on Tor status transitions.</summary>
public sealed class TorStatusChangedEventArgs : EventArgs
{
    public TorStatusChangedEventArgs(TorStatus status, int bootstrapPercent, string? message)
    {
        Status = status;
        BootstrapPercent = bootstrapPercent;
        Message = message;
    }

    public TorStatus Status { get; }
    public int BootstrapPercent { get; }
    public string? Message { get; }
}

/// <summary>
/// Controls the Tor fallback process. Independent of the DPI-bypass engine.
/// </summary>
public interface ITorController : IAsyncDisposable
{
    TorStatus Status { get; }
    int BootstrapPercent { get; }
    string SocksEndpoint { get; }

    event EventHandler<TorStatusChangedEventArgs>? StatusChanged;
    event EventHandler<string>? LogReceived;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
