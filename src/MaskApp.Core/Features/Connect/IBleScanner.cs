namespace MaskApp.Core.Features.Connect;

public interface IBleScanner
{
    event EventHandler<DiscoveredMaskDevice>? DeviceDiscovered;

    event EventHandler<BleScannerStateChangedEventArgs>? ScannerStateChanged;

    bool IsScanning { get; }

    Task StartScanningAsync(CancellationToken cancellationToken = default);

    Task StopScanningAsync(CancellationToken cancellationToken = default);
}
