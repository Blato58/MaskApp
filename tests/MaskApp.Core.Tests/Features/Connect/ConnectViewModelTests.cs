using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Tests.Features.Connect;

public sealed class ConnectViewModelTests
{
    [Fact]
    public async Task StartScanCommand_ClearsDevicesAndStartsScanner()
    {
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var viewModel = new ConnectViewModel(scanner, connection);
        scanner.Discover(new DiscoveredMaskDevice("old", "Old Mask", -70));

        await viewModel.StartScanCommand.ExecuteAsync();

        Assert.True(scanner.IsScanning);
        Assert.True(viewModel.IsScanning);
        Assert.Empty(viewModel.Devices);
        Assert.Equal("Scanning for masks...", viewModel.StatusText);
    }

    [Fact]
    public async Task DeviceDiscovered_AddsUniqueMaskDevices()
    {
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var viewModel = new ConnectViewModel(scanner, connection);
        await viewModel.StartScanCommand.ExecuteAsync();

        scanner.Discover(new DiscoveredMaskDevice("mask-1", "Mask One", -51));
        scanner.Discover(new DiscoveredMaskDevice("mask-1", "Mask One Duplicate", -50));

        var device = Assert.Single(viewModel.Devices);
        Assert.Equal("mask-1", device.Id);
        Assert.Equal("1 mask device(s) found.", viewModel.StatusText);
    }

    [Fact]
    public async Task ConnectCommand_ConnectsSelectedDevice()
    {
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var viewModel = new ConnectViewModel(scanner, connection);
        var device = new DiscoveredMaskDevice("mask-1", "Mask One", -51);

        scanner.Discover(device);
        viewModel.SelectedDevice = device;
        await viewModel.ConnectCommand.ExecuteAsync();

        Assert.Equal(device, connection.ConnectedDevice);
        Assert.Equal(BleConnectionState.Connected, viewModel.ConnectionState);
        Assert.Equal("Connected to Mask One.", viewModel.StatusText);
    }

    private sealed class FakeBleScanner : IBleScanner
    {
        public event EventHandler<DiscoveredMaskDevice>? DeviceDiscovered;
        public event EventHandler<BleScannerStateChangedEventArgs>? ScannerStateChanged;

        public bool IsScanning { get; private set; }

        public Task StartScanningAsync(CancellationToken cancellationToken = default)
        {
            IsScanning = true;
            ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(true, "Scanning for masks..."));
            return Task.CompletedTask;
        }

        public Task StopScanningAsync(CancellationToken cancellationToken = default)
        {
            IsScanning = false;
            ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(false, "Scan stopped."));
            return Task.CompletedTask;
        }

        public void Discover(DiscoveredMaskDevice device) => DeviceDiscovered?.Invoke(this, device);
    }

    private sealed class FakeBleConnection : IBleDeviceConnection
    {
        public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged;

        public BleConnectionState State { get; private set; } = BleConnectionState.Disconnected;

        public DiscoveredMaskDevice? ConnectedDevice { get; private set; }

        public Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default)
        {
            ConnectedDevice = device;
            State = BleConnectionState.Connected;
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(State, $"Connected to {device.Name}."));
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            ConnectedDevice = null;
            State = BleConnectionState.Disconnected;
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(State, "Disconnected."));
            return Task.CompletedTask;
        }
    }
}
