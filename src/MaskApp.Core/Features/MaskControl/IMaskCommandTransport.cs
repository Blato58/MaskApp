namespace MaskApp.Core.Features.MaskControl;

public interface IMaskCommandTransport
{
    event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

    string TransportDisplayName { get; }

    bool IsSimulated { get; }

    MaskCommandTransportState TransportState { get; }

    string TransportStatusText { get; }

    Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default);
}
