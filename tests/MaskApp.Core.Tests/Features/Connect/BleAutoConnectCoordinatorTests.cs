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
    public async Task StartForegroundAutoConnectAsync_KeepsScanningUntilRememberedMaskAppears()
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

        Assert.True(scanner.IsScanning);
        Assert.True(coordinator.IsAutoConnectSearching);

        scanner.Discover(new DiscoveredMaskDevice("mask-1", "Stage Mask", -42));

        Assert.Equal("mask-1", connection.ConnectedDevice?.Id);
        Assert.False(scanner.IsScanning);
        Assert.Equal("Auto-connect: connected", coordinator.AutoConnectStatusText);
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
    public async Task DeviceDiscovered_NameFallbackAfterLongSearch_ConnectsOnlyAfterDelay()
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

        Assert.True(scanner.IsScanning);
        Assert.Null(connection.ConnectedDevice);

        scanner.Discover(new DiscoveredMaskDevice("new-ios-id", "Stage Mask", -42));
        Assert.Null(connection.ConnectedDevice);

        fallbackGate.SetResult();
        await WaitUntilAsync(() => connection.ConnectedDevice is not null);

        Assert.Equal("new-ios-id", connection.ConnectedDevice?.Id);
        Assert.Equal("Auto-connect: connected", coordinator.AutoConnectStatusText);
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
    public async Task ConnectionLost_RestartsForegroundAutoConnect()
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
        await connection.DisconnectAsync();
        await WaitUntilAsync(() => scanner.IsScanning);

        Assert.Null(connection.ConnectedDevice);
        Assert.True(coordinator.IsAutoConnectSearching);
        Assert.Equal("Auto-connect: searching for Stage Mask", coordinator.AutoConnectStatusText);
    }

    [Fact]
    public async Task ConnectionLostInBackground_DoesNotScanUntilExplicitForegroundReturn()
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
        await coordinator.StopForegroundAutoConnectAsync();
        await connection.DisconnectAsync();

        Assert.False(scanner.IsScanning);
        Assert.False(coordinator.IsAutoConnectSearching);
        Assert.Equal(
            "Auto-connect: paused until the app returns to foreground",
            coordinator.AutoConnectStatusText);

        await coordinator.StartForegroundAutoConnectAsync();
        Assert.True(scanner.IsScanning);
        Assert.True(coordinator.IsAutoConnectSearching);
    }

    [Fact]
    public async Task BackgroundStop_WaitsForLateScanStartThenLeavesScannerStopped()
    {
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = true,
            LastKnownDevice = new KnownMaskDevice("mask-1", "Stage Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var scanner = new FakeBleScanner
        {
            StartRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        var coordinator = new BleAutoConnectCoordinator(scanner, new FakeBleConnection(), store);

        var start = coordinator.StartForegroundAutoConnectAsync();
        await scanner.StartEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        var stop = coordinator.StopForegroundAutoConnectAsync();
        scanner.StartRelease.SetResult();

        await Task.WhenAll(start, stop).WaitAsync(TimeSpan.FromSeconds(1));
        Assert.False(scanner.IsScanning);
        Assert.False(coordinator.IsAutoConnectSearching);
        Assert.Contains("paused", coordinator.AutoConnectStatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BackgroundStop_ClearsScanRequestThatHasNotStartedYet()
    {
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = true,
            LastKnownDevice = new KnownMaskDevice("mask-1", "Stage Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var scanner = new FakeBleScanner { LeaveStartPending = true };
        var coordinator = new BleAutoConnectCoordinator(scanner, new FakeBleConnection(), store);

        await coordinator.StartForegroundAutoConnectAsync();
        Assert.False(scanner.IsScanning);

        await coordinator.StopForegroundAutoConnectAsync();

        Assert.Equal(1, scanner.StopCount);
        Assert.False(coordinator.IsAutoConnectSearching);
    }

    [Fact]
    public async Task BackgroundStop_CancelsAutoConnectThatStartedWhileForegrounded()
    {
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = true,
            LastKnownDevice = new KnownMaskDevice("mask-1", "Stage Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection
        {
            ConnectRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        var coordinator = new BleAutoConnectCoordinator(scanner, connection, store);

        await coordinator.StartForegroundAutoConnectAsync();
        scanner.Discover(new DiscoveredMaskDevice("mask-1", "Stage Mask", -42));
        await connection.ConnectEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));

        var stop = coordinator.StopForegroundAutoConnectAsync();
        connection.ConnectRelease.SetResult();

        await stop.WaitAsync(TimeSpan.FromSeconds(1));
        await WaitUntilAsync(() => connection.State == BleConnectionState.Disconnected);
        Assert.Null(connection.ConnectedDevice);
        Assert.False(scanner.IsScanning);
        Assert.Contains("paused", coordinator.AutoConnectStatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DisconnectManuallyAsync_DoesNotRestartForegroundAutoConnect()
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
        await coordinator.DisconnectManuallyAsync();

        Assert.Null(connection.ConnectedDevice);
        Assert.False(scanner.IsScanning);
        Assert.False(coordinator.IsAutoConnectSearching);
        Assert.Equal("Auto-connect: paused after manual disconnect", coordinator.AutoConnectStatusText);
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

        public TaskCompletionSource StartEntered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource? StartRelease { get; init; }

        public bool LeaveStartPending { get; init; }

        public int StopCount { get; private set; }

        public async Task StartScanningAsync(CancellationToken cancellationToken = default)
        {
            StartEntered.TrySetResult();
            if (StartRelease is not null)
            {
                await StartRelease.Task.WaitAsync(cancellationToken);
            }

            if (LeaveStartPending)
            {
                return;
            }

            IsScanning = true;
            ScannerStateChanged?.Invoke(this, new BleScannerStateChangedEventArgs(true, "Scanning for masks..."));
        }

        public Task StopScanningAsync(CancellationToken cancellationToken = default)
        {
            StopCount++;
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

        public TaskCompletionSource ConnectEntered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource? ConnectRelease { get; init; }

        public async Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default)
        {
            if (ConnectException is not null)
            {
                throw ConnectException;
            }

            ConnectEntered.TrySetResult();
            if (ConnectRelease is not null)
            {
                await ConnectRelease.Task;
            }

            ConnectedDevice = device;
            State = BleConnectionState.Connected;
            ConnectionStateChanged?.Invoke(this, new BleConnectionStateChangedEventArgs(State, $"Connected to {device.Name}."));
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
