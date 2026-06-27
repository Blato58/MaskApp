using MaskApp.Core.Features.Connect;

namespace MaskApp.App.Infrastructure.Bluetooth;

public sealed class UnavailableBleAdapter : IBleScanner, IBleDeviceConnection
{
    event EventHandler<DiscoveredMaskDevice>? IBleScanner.DeviceDiscovered
    {
        add { }
        remove { }
    }

    public event EventHandler<BleScannerStateChangedEventArgs>? ScannerStateChanged;
    public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged;

    public bool IsScanning => false;
    public BleConnectionState State => BleConnectionState.Unavailable;

    public Task StartScanningAsync(CancellationToken cancellationToken = default)
    {
        ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(false, "BLE scanning is not implemented for this platform yet."));
        return Task.CompletedTask;
    }

    public Task StopScanningAsync(CancellationToken cancellationToken = default)
    {
        ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(false, "BLE scanning stopped."));
        return Task.CompletedTask;
    }

    public Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default)
    {
        ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(BleConnectionState.Unavailable, "BLE connection is not implemented for this platform yet."));
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(BleConnectionState.Disconnected, "Disconnected."));
        return Task.CompletedTask;
    }
}
