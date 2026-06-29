using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Tests.Features.Connect;

public sealed class BleAutoConnectCoordinatorTests
{
    [Fact]
    public async Task InitializeAsync_LoadsSafeDefaults()
    {
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var coordinator = new BleAutoConnectCoordinator(scanner, connection);

        await coordinator.InitializeAsync();

        Assert.False(coordinator.AutoConnectEnabled);
        Assert.True(coordinator.RememberLastDeviceEnabled);
        Assert.Null(coordinator.LastKnownDevice);
        Assert.Equal("Auto-connect: no remembered mask", coordinator.AutoConnectStatusText);
    }

    [Fact]
    public async Task StartForegroundAutoConnectAsync_WithoutKnownMask_DoesNotScan()
    {
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var coordinator = new BleAutoConnectCoordinator(scanner, connection);

        await coordinator.StartForegroundAutoConnectAsync();

        Assert.False(scanner.IsScanning);
        Assert.Null(connection.ConnectedDevice);
        Assert.Equal("Auto-connect: no remembered mask", coordinator.AutoConnectStatusText);
    }

    [Fact]
    public async Task ConnectManuallyAsync_RemembersDeviceAndEnablesAutoConnect()
    {
        var store = new InMemoryBleAutoConnectSettingsStore();
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var coordinator = new BleAutoConnectCoordinator(scanner, connection, store);
        var device = new DiscoveredMaskDevice("mask-1", "Stage Mask", -40);

        await coordinator.ConnectManuallyAsync(device);

        var settings = await store.LoadAsync();
        Assert.True(settings.AutoConnectEnabled);
        Assert.True(settings.RememberLastDeviceEnabled);
        Assert.Equal("mask-1", settings.LastKnownDevice?.Id);
        Assert.Equal("Stage Mask", coordinator.LastKnownMaskText);
        Assert.Equal("Auto-connect: connected", coordinator.AutoConnectStatusText);
    }

    [Fact]
    public async Task StartForegroundAutoConnectAsync_Disabled_DoesNotScan()
    {
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = false,
            LastKnownDevice = new KnownMaskDevice("mask-1", "Stage Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var coordinator = new BleAutoConnectCoordinator(scanner, connection, store);

        await coordinator.StartForegroundAutoConnectAsync();

        Assert.False(scanner.IsScanning);
        Assert.Null(connection.ConnectedDevice);
        Assert.Equal("Auto-connect: Off", coordinator.AutoConnectStatusText);
    }

    [Fact]
    public async Task DeviceDiscovered_ExactIdMatch_ConnectsRememberedMask()
    {
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = true,
            LastKnownDevice = new KnownMaskDevice("mask-1", "Stage Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var coordinator = new BleAutoConnectCoordinator(scanner, connection, store);

        await coordinator.StartForegroundAutoConnectAsync();
        scanner.Discover(new DiscoveredMaskDevice("mask-1", "Stage Mask", -42));

        Assert.Equal("mask-1", connection.ConnectedDevice?.Id);
        Assert.False(scanner.IsScanning);
        Assert.Equal("Auto-connect: connected", coordinator.AutoConnectStatusText);
    }

    [Fact]
    public async Task DeviceDiscovered_AutoConnectFailure_StopsScanAndKeepsManualAvailable()
    {
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = true,
            LastKnownDevice = new KnownMaskDevice("mask-1", "Stage Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection
        {
            ConnectException = new InvalidOperationException("Transport unavailable.")
        };
        var coordinator = new BleAutoConnectCoordinator(scanner, connection, store);

        await coordinator.StartForegroundAutoConnectAsync();
        scanner.Discover(new DiscoveredMaskDevice("mask-1", "Stage Mask", -42));
        await WaitUntilAsync(() => !scanner.IsScanning);

        Assert.Null(connection.ConnectedDevice);
        Assert.True(coordinator.CanAutoConnectNow);
        Assert.Equal("Auto-connect failed. Manual connect available.", coordinator.AutoConnectStatusText);
    }

    [Fact]
    public async Task DeviceDiscovered_SingleExactNameFallback_ConnectsOnlyAfterDelay()
    {
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = true,
            LastKnownDevice = new KnownMaskDevice("old-ios-id", "Stage Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var fallbackGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var coordinator = new BleAutoConnectCoordinator(
            scanner,
            connection,
            store,
            delayAsync: (_, _) => fallbackGate.Task);

        await coordinator.StartForegroundAutoConnectAsync();
        scanner.Discover(new DiscoveredMaskDevice("new-ios-id", "Stage Mask", -42));
        fallbackGate.SetResult();
        await WaitUntilAsync(() => connection.ConnectedDevice is not null);

        Assert.Equal("new-ios-id", connection.ConnectedDevice?.Id);
        Assert.Equal("Stage Mask", coordinator.LastKnownMaskText);
    }

    [Fact]
    public async Task DeviceDiscovered_MultipleNameFallbacks_DoesNotConnect()
    {
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = true,
            LastKnownDevice = new KnownMaskDevice("old-ios-id", "Stage Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var fallbackGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var coordinator = new BleAutoConnectCoordinator(
            scanner,
            connection,
            store,
            delayAsync: (_, _) => fallbackGate.Task);

        await coordinator.StartForegroundAutoConnectAsync();
        scanner.Discover(new DiscoveredMaskDevice("new-ios-id-1", "Stage Mask", -42));
        scanner.Discover(new DiscoveredMaskDevice("new-ios-id-2", "Stage Mask", -43));
        fallbackGate.SetResult();
        await WaitUntilAsync(() => !scanner.IsScanning);

        Assert.Null(connection.ConnectedDevice);
        Assert.Equal("Auto-connect: name matched multiple masks. Manual connect available.", coordinator.AutoConnectStatusText);
    }

    [Fact]
    public async Task ForgetKnownDeviceAsync_ClearsRememberedMaskAndDisablesAutoConnect()
    {
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = true,
            LastKnownDevice = new KnownMaskDevice("mask-1", "Stage Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var coordinator = new BleAutoConnectCoordinator(scanner, connection, store);

        await coordinator.ForgetKnownDeviceAsync();

        var settings = await store.LoadAsync();
        Assert.False(settings.AutoConnectEnabled);
        Assert.Null(settings.LastKnownDevice);
        Assert.Equal("Auto-connect: no remembered mask", coordinator.AutoConnectStatusText);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.True(condition());
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

        public Exception? ConnectException { get; init; }

        public Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default)
        {
            if (ConnectException is not null)
            {
                return Task.FromException(ConnectException);
            }

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
