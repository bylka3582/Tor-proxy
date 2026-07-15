namespace DpiBypass.Tor;

/// <summary>Lifecycle state of the Tor fallback.</summary>
public enum TorStatus
{
    Stopped,
    Starting,
    Bootstrapping,
    Ready,
    Stopping,
    Faulted,
}
