using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Lifecycle;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Connect;

public sealed class ConnectViewModelTests
{
    [Fact]
    public async Task Diagnostics_ReportCurrentLifecycleAndForegroundLimitations()
    {
        var lifecycle = new AppLifecycleCoordinator(new NoOpLifecycleOperations());
        await lifecycle.OnStoppedAsync();
        var viewModel = new ConnectViewModel(
            new FakeBleScanner(),
            new FakeBleConnection(),
            lifecycleCoordinator: lifecycle);

        var report = await viewModel.BuildRedactedDiagnosticsReportAsync();

        Assert.Contains("Lifecycle: Background", report, StringComparison.Ordinal);
        Assert.Contains("rapid phone-timed playback are stopped", report, StringComparison.Ordinal);
        Assert.Contains("Reconnect never replays output automatically", report, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Diagnostics_ObserveLifecycleTransitionsWithoutManualRefresh()
    {
        var lifecycle = new AppLifecycleCoordinator(new NoOpLifecycleOperations());
        var viewModel = new ConnectViewModel(
            new FakeBleScanner(),
            new FakeBleConnection(),
            lifecycleCoordinator: lifecycle);

        await lifecycle.OnStoppedAsync();

        Assert.StartsWith("Background", viewModel.LifecycleText, StringComparison.Ordinal);
        Assert.Contains("backgrounded", viewModel.LifecycleLimitationText, StringComparison.OrdinalIgnoreCase);
    }

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
        var store = new InMemoryBleAutoConnectSettingsStore();
        var coordinator = new BleAutoConnectCoordinator(scanner, connection, store);
        var viewModel = new ConnectViewModel(scanner, connection, coordinator);
        var device = new DiscoveredMaskDevice("mask-1", "Mask One", -51);

        scanner.Discover(device);
        viewModel.SelectedDevice = device;
        await viewModel.ConnectCommand.ExecuteAsync();

        Assert.Equal(device, connection.ConnectedDevice);
        Assert.Equal(BleConnectionState.Connected, viewModel.ConnectionState);
        Assert.Equal("Connected to Mask One.", viewModel.StatusText);
        Assert.True(viewModel.IsConnected);
        Assert.True(viewModel.CanDisconnect);
        Assert.Equal("Connected", viewModel.ConnectionHeadline);
        Assert.Equal("Mask One", viewModel.DeviceNameText);
        Assert.Equal("-51 dBm", viewModel.DeviceSignalText);
        Assert.True(viewModel.HasReconnectHistory);
        Assert.Contains("Connected", viewModel.ReconnectHistory[0], StringComparison.Ordinal);
        Assert.True(viewModel.AutoConnectEnabled);
        Assert.Equal("Mask One", viewModel.LastKnownMaskText);
        Assert.Equal("mask-1", (await store.LoadAsync()).LastKnownDevice?.Id);
    }

    [Fact]
    public async Task DashboardText_UsesRememberedMaskWithoutFakeTelemetry()
    {
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = false,
            LastKnownDevice = new KnownMaskDevice("mask-1", "Saved Mask", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var coordinator = new BleAutoConnectCoordinator(scanner, connection, store);
        var viewModel = new ConnectViewModel(scanner, connection, coordinator);

        await viewModel.InitializeAsync();

        Assert.False(viewModel.IsConnected);
        Assert.False(viewModel.CanDisconnect);
        Assert.Equal("Not Connected", viewModel.ConnectionHeadline);
        Assert.Equal("Saved Mask", viewModel.DeviceNameText);
        Assert.Equal("Signal unavailable", viewModel.DeviceSignalText);
        Assert.Contains("Scan nearby", viewModel.ConnectionDetailText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Constructor_UsesCurrentConnectionStateWithoutWaitingForAnotherEvent()
    {
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        await connection.ConnectAsync(new DiscoveredMaskDevice("mask-1", "Mask One", -51));

        var viewModel = new ConnectViewModel(scanner, connection);

        Assert.True(viewModel.IsConnected);
        Assert.Equal("Connected", viewModel.ConnectionHeadline);
    }

    [Fact]
    public async Task ForgetKnownMaskCommand_ClearsRememberedDevice()
    {
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var store = new InMemoryBleAutoConnectSettingsStore(new BleAutoConnectSettings
        {
            AutoConnectEnabled = true,
            LastKnownDevice = new KnownMaskDevice("mask-1", "Mask One", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        });
        var coordinator = new BleAutoConnectCoordinator(scanner, connection, store);
        var viewModel = new ConnectViewModel(scanner, connection, coordinator);
        await viewModel.InitializeAsync();

        await viewModel.ForgetKnownMaskCommand.ExecuteAsync();

        Assert.False(viewModel.AutoConnectEnabled);
        Assert.Equal("No remembered mask", viewModel.LastKnownMaskText);
        Assert.Null((await store.LoadAsync()).LastKnownDevice);
    }

    [Fact]
    public async Task DiagnosticsExportRedactsIdentifiersAndResetClearsOnlyActivePreparedLedger()
    {
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var profileStore = new InMemoryMaskProfileStore();
        var profileSession = new MaskProfileSession(profileStore);
        await profileSession.ActivateAsync(new DiscoveredMaskDevice(
            "secret-device-id",
            "Private Mask Name",
            -42));
        await profileSession.ReplacePreparedSlotsAsync(
        [
            new FaceSlotInstallation
            {
                Slot = 7,
                ContentFingerprint = "fingerprint",
                SourceId = "face:private",
                InstalledAt = DateTimeOffset.UtcNow
            }
        ]);
        var viewModel = new ConnectViewModel(
            scanner,
            connection,
            profileSession: profileSession);

        var report = await viewModel.BuildRedactedDiagnosticsReportAsync();

        Assert.Contains("REDACTED", report, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-device-id", report, StringComparison.Ordinal);
        Assert.DoesNotContain("Private Mask Name", report, StringComparison.Ordinal);
        Assert.True(viewModel.CanResetPreparedSlots);

        Assert.True(await viewModel.ResetPreparedSlotsAsync());
        Assert.Empty((await profileSession.GetActiveProfileAsync())!.PreparedSlots);
        Assert.False(viewModel.CanResetPreparedSlots);
        Assert.Contains("Physical mask content was not erased", viewModel.DiagnosticsStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SchedulerErrorHistory_RedactsKnownAndBleIdentifiers()
    {
        const string deviceId = "AA:BB:CC:DD:EE:FF";
        const string deviceName = "Private Mask One";
        const string bluetoothUuid = "12345678-1234-1234-1234-1234567890ab";
        var scanner = new FakeBleScanner();
        var connection = new FakeBleConnection();
        var transport = new FailingCombinedTransport(
            $"Write failed for {deviceName} at {deviceId} ({bluetoothUuid}).");
        await using var scheduler = new MaskBleScheduler(transport, transport, transport);
        var viewModel = new ConnectViewModel(scanner, connection, scheduler: scheduler)
        {
            SelectedDevice = new DiscoveredMaskDevice(deviceId, deviceName, -51)
        };
        var historyUpdated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ConnectViewModel.HasRecentRedactedErrors)
                && viewModel.HasRecentRedactedErrors)
            {
                historyUpdated.TrySetResult();
            }
        };

        var result = await scheduler.SendAsync(MaskCommandBuilder.Brightness(50));
        await historyUpdated.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var report = await viewModel.BuildRedactedDiagnosticsReportAsync();

        Assert.False(result.Succeeded);
        Assert.DoesNotContain(deviceId, report, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(deviceName, report, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(bluetoothUuid, report, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[REDACTED", report, StringComparison.Ordinal);
        Assert.Single(viewModel.RecentRedactedErrors);
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

    private sealed class NoOpLifecycleOperations : IAppLifecycleOperations
    {
        public Task RecoverInterruptedImportAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StartWatchRemoteAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void SetWatchForeground(bool isForeground)
        {
        }

        public void CancelSceneExecution()
        {
        }

        public void CancelAudioDiagnostic()
        {
        }

        public Task StartForegroundAutoConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopForegroundAutoConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAudioVisualizerAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task HandOffAnimationForBackgroundAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ResumeAnimationFromBackgroundAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task PublishWatchRemoteStateAsync(CancellationToken cancellationToken) => Task.CompletedTask;
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

    private sealed class FailingCombinedTransport(
        string failureMessage) : IMaskCommandTransport, ITextUploadTransport, IFaceUploadTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
        {
            add { }
            remove { }
        }

        event EventHandler<TextUploadTransportStateChangedEventArgs>? ITextUploadTransport.StateChanged
        {
            add { }
            remove { }
        }

        event EventHandler<FaceUploadTransportStateChangedEventArgs>? IFaceUploadTransport.StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Failing transport";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState => MaskCommandTransportState.Ready;

        public string TransportStatusText => "Ready.";

        public bool IsReady => true;

        public bool SupportsAcknowledgements => true;

        TextUploadTransportState ITextUploadTransport.State => TextUploadTransportState.Simulated;

        FaceUploadTransportState IFaceUploadTransport.State => FaceUploadTransportState.Simulated;

        public string StatusText => "Ready.";

        public Task<MaskCommandResult> SendAsync(
            MaskCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromException<MaskCommandResult>(new InvalidOperationException(failureMessage));

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TextUploadResult.Failure(failureMessage, 0));

        public Task<FaceUploadResult> UploadAsync(
            FaceUploadPackage package,
            FaceUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(FaceUploadResult.Failure(failureMessage, 0));
    }
}
