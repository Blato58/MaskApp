namespace MaskApp.Core.Features.WatchRemote;

public enum WatchConnectivityAvailability
{
    Unsupported,
    Activating,
    Unpaired,
    CompanionNotInstalled,
    Ready,
    Unreachable,
    Failed
}

public sealed record WatchConnectivitySnapshot
{
    public WatchConnectivityAvailability Availability { get; init; }

    public bool IsSupported { get; init; }

    public bool IsPaired { get; init; }

    public bool IsCompanionInstalled { get; init; }

    public bool IsReachable { get; init; }

    public string StatusText { get; init; } = "Watch Connectivity is unavailable.";
}

public sealed class WatchConnectivityChangedEventArgs : EventArgs
{
    public WatchConnectivityChangedEventArgs(WatchConnectivitySnapshot snapshot)
    {
        Snapshot = snapshot;
    }

    public WatchConnectivitySnapshot Snapshot { get; }
}

public interface IWatchConnectivityService
{
    event EventHandler<WatchConnectivityChangedEventArgs>? StateChanged;

    WatchConnectivitySnapshot Snapshot { get; }

    Task StartAsync(CancellationToken cancellationToken = default);

    Task PublishStateAsync(
        WatchRemoteState state,
        CancellationToken cancellationToken = default);
}

public sealed class UnavailableWatchConnectivityService : IWatchConnectivityService
{
    public event EventHandler<WatchConnectivityChangedEventArgs>? StateChanged
    {
        add { }
        remove { }
    }

    public WatchConnectivitySnapshot Snapshot { get; } = new()
    {
        Availability = WatchConnectivityAvailability.Unsupported,
        StatusText = "Apple Watch remote requires the iOS app and a future watchOS companion."
    };

    public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task PublishStateAsync(
        WatchRemoteState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        return Task.CompletedTask;
    }
}
