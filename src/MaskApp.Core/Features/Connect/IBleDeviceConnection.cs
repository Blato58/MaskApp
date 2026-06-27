namespace MaskApp.Core.Features.Connect;

public interface IBleDeviceConnection
{
    event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged;

    BleConnectionState State { get; }

    Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
