using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MaskApp.Core.Features.Connect;

public sealed class BleAutoConnectCoordinator : INotifyPropertyChanged
{
    private static readonly TimeSpan DefaultNameFallbackDelay = TimeSpan.FromSeconds(3);

    private readonly IBleScanner scanner;
    private readonly IBleDeviceConnection connection;
    private readonly IBleAutoConnectSettingsStore settingsStore;
    private readonly TimeProvider timeProvider;
    private readonly Func<TimeSpan, CancellationToken, Task> delayAsync;
    private readonly List<DiscoveredMaskDevice> nameFallbackCandidates = [];
    private BleAutoConnectSettings settings = BleAutoConnectSettings.Defaults;
    private DiscoveredMaskDevice? pendingRememberedDevice;
    private CancellationTokenSource? autoConnectCancellation;
    private bool isInitialized;
    private bool isAutoConnectSearching;
    private bool isNameFallbackEvaluationPending;
    private bool coordinatorStartedScan;
    private bool manualDisconnectRequested;
    private string autoConnectStatusText = "Auto-connect: Off";

    public BleAutoConnectCoordinator(
        IBleScanner scanner,
        IBleDeviceConnection connection,
        IBleAutoConnectSettingsStore? settingsStore = null,
        TimeProvider? timeProvider = null,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        this.scanner = scanner;
        this.connection = connection;
        this.settingsStore = settingsStore ?? new InMemoryBleAutoConnectSettingsStore();
        this.timeProvider = timeProvider ?? TimeProvider.System;
        this.delayAsync = delayAsync ?? Task.Delay;

        scanner.DeviceDiscovered += OnDeviceDiscovered;
        connection.ConnectionStateChanged += OnConnectionStateChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool AutoConnectEnabled => settings.AutoConnectEnabled;

    public bool RememberLastDeviceEnabled => settings.RememberLastDeviceEnabled;

    public KnownMaskDevice? LastKnownDevice => settings.LastKnownDevice;

    public bool HasKnownDevice => LastKnownDevice is not null;

    public bool IsAutoConnectSearching
    {
        get => isAutoConnectSearching;
        private set
        {
            if (SetField(ref isAutoConnectSearching, value))
            {
                OnPropertyChanged(nameof(CanAutoConnectNow));
            }
        }
    }

    public bool CanAutoConnectNow => AutoConnectEnabled && HasKnownDevice && !IsAutoConnectSearching;

    public string LastKnownMaskText => LastKnownDevice is null
        ? "No remembered mask"
        : LastKnownDevice.Name;

    public string AutoConnectStatusText
    {
        get => autoConnectStatusText;
        private set => SetField(ref autoConnectStatusText, value);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (isInitialized)
        {
            return;
        }

        settings = (await settingsStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        isInitialized = true;
        AutoConnectStatusText = settings.LastKnownDevice is null
            ? "Auto-connect: no remembered mask"
            : settings.AutoConnectEnabled ? "Auto-connect: ready" : "Auto-connect: Off";
        RaiseSettingsChanged();
    }

    public async Task StartForegroundAutoConnectAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        if (!settings.AutoConnectEnabled)
        {
            AutoConnectStatusText = settings.LastKnownDevice is null
                ? "Auto-connect: no remembered mask"
                : "Auto-connect: Off";
            return;
        }

        if (settings.LastKnownDevice is null)
        {
            AutoConnectStatusText = "Auto-connect: no remembered mask";
            return;
        }

        if (connection.State == BleConnectionState.Connected)
        {
            AutoConnectStatusText = "Auto-connect: connected";
            return;
        }

        if (IsAutoConnectSearching)
        {
            return;
        }

        manualDisconnectRequested = false;
        autoConnectCancellation?.Cancel();
        autoConnectCancellation?.Dispose();
        autoConnectCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        nameFallbackCandidates.Clear();
        isNameFallbackEvaluationPending = false;
        IsAutoConnectSearching = true;
        coordinatorStartedScan = true;
        AutoConnectStatusText = $"Auto-connect: searching for {settings.LastKnownDevice.Name}";

        await scanner.StartScanningAsync(autoConnectCancellation.Token).ConfigureAwait(false);
    }

    public async Task ConnectManuallyAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        pendingRememberedDevice = device;
        await connection.ConnectAsync(device, cancellationToken).ConfigureAwait(false);
    }

    public async Task DisconnectManuallyAsync(CancellationToken cancellationToken = default)
    {
        manualDisconnectRequested = true;
        pendingRememberedDevice = null;
        await StopCoordinatorScanAsync(cancellationToken).ConfigureAwait(false);
        await connection.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        AutoConnectStatusText = settings.LastKnownDevice is null
            ? "Auto-connect: no remembered mask"
            : "Auto-connect: paused after manual disconnect";
    }

    public async Task SetAutoConnectEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        settings = settings with { AutoConnectEnabled = enabled && settings.LastKnownDevice is not null };
        await SaveSettingsAsync(cancellationToken).ConfigureAwait(false);

        if (!settings.AutoConnectEnabled)
        {
            await StopCoordinatorScanAsync(cancellationToken).ConfigureAwait(false);
            AutoConnectStatusText = settings.LastKnownDevice is null
                ? "Auto-connect: no remembered mask"
                : "Auto-connect: Off";
        }
        else
        {
            AutoConnectStatusText = "Auto-connect: ready";
        }
    }

    public async Task SetRememberLastDeviceEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        settings = settings with { RememberLastDeviceEnabled = enabled };
        await SaveSettingsAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ForgetKnownDeviceAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        pendingRememberedDevice = null;
        nameFallbackCandidates.Clear();
        settings = settings with
        {
            AutoConnectEnabled = false,
            LastKnownDevice = null
        };
        await SaveSettingsAsync(cancellationToken).ConfigureAwait(false);
        await StopCoordinatorScanAsync(cancellationToken).ConfigureAwait(false);
        AutoConnectStatusText = "Auto-connect: no remembered mask";
    }

    private async Task TryNameFallbackAfterDelayAsync(KnownMaskDevice knownDevice, CancellationToken cancellationToken)
    {
        try
        {
            await delayAsync(DefaultNameFallbackDelay, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            await FailAutoConnectAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (cancellationToken.IsCancellationRequested || !IsAutoConnectSearching)
        {
            isNameFallbackEvaluationPending = false;
            return;
        }

        isNameFallbackEvaluationPending = false;

        var candidates = nameFallbackCandidates
            .Where(candidate => string.Equals(candidate.Name, knownDevice.Name, StringComparison.Ordinal))
            .GroupBy(candidate => candidate.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();

        if (candidates.Length == 1)
        {
            await ConnectAutoAsync(candidates[0], cancellationToken).ConfigureAwait(false);
            return;
        }

        if (candidates.Length > 1)
        {
            await StopCoordinatorScanAsync(cancellationToken).ConfigureAwait(false);
            AutoConnectStatusText = "Auto-connect: name matched multiple masks. Manual connect available.";
        }
    }

    private async void OnDeviceDiscovered(object? sender, DiscoveredMaskDevice device)
    {
        try
        {
            if (!IsAutoConnectSearching || settings.LastKnownDevice is null)
            {
                return;
            }

            if (string.Equals(device.Id, settings.LastKnownDevice.Id, StringComparison.Ordinal))
            {
                await ConnectAutoAsync(device, autoConnectCancellation?.Token ?? CancellationToken.None).ConfigureAwait(false);
                return;
            }

            if (string.Equals(device.Name, settings.LastKnownDevice.Name, StringComparison.Ordinal) &&
                !nameFallbackCandidates.Any(candidate => candidate.Id == device.Id))
            {
                nameFallbackCandidates.Add(device);
                AutoConnectStatusText = $"Auto-connect: verifying {device.Name}";
                if (!isNameFallbackEvaluationPending)
                {
                    isNameFallbackEvaluationPending = true;
                    _ = TryNameFallbackAfterDelayAsync(settings.LastKnownDevice, autoConnectCancellation?.Token ?? CancellationToken.None);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            await FailAutoConnectAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    private async Task ConnectAutoAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken)
    {
        if (!IsAutoConnectSearching)
        {
            return;
        }

        try
        {
            pendingRememberedDevice = device;
            AutoConnectStatusText = $"Auto-connect: connecting to {device.Name}";
            IsAutoConnectSearching = false;
            await connection.ConnectAsync(device, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            pendingRememberedDevice = null;
        }
        catch (Exception)
        {
            await FailAutoConnectAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    private async void OnConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs e)
    {
        if (e.State == BleConnectionState.Failed && pendingRememberedDevice is not null)
        {
            pendingRememberedDevice = null;
            await FailAutoConnectAsync(CancellationToken.None).ConfigureAwait(false);
            return;
        }

        if (e.State == BleConnectionState.Disconnected)
        {
            if (manualDisconnectRequested)
            {
                manualDisconnectRequested = false;
                AutoConnectStatusText = settings.LastKnownDevice is null
                    ? "Auto-connect: no remembered mask"
                    : "Auto-connect: paused after manual disconnect";
                return;
            }

            if (settings.AutoConnectEnabled && settings.LastKnownDevice is not null)
            {
                await StartForegroundAutoConnectAsync(CancellationToken.None).ConfigureAwait(false);
            }

            return;
        }

        if (e.State != BleConnectionState.Connected)
        {
            return;
        }

        var device = pendingRememberedDevice;
        pendingRememberedDevice = null;
        if (device is null)
        {
            AutoConnectStatusText = "Auto-connect: connected";
            return;
        }

        if (settings.RememberLastDeviceEnabled)
        {
            var now = timeProvider.GetUtcNow();
            settings = settings with
            {
                AutoConnectEnabled = true,
                LastKnownDevice = KnownMaskDevice.FromDiscoveredDevice(device, now)
            };
            await SaveSettingsAsync(CancellationToken.None).ConfigureAwait(false);
        }

        await StopCoordinatorScanAsync(CancellationToken.None).ConfigureAwait(false);
        AutoConnectStatusText = "Auto-connect: connected";
    }

    private async Task FailAutoConnectAsync(CancellationToken cancellationToken)
    {
        pendingRememberedDevice = null;
        await StopCoordinatorScanAsync(cancellationToken).ConfigureAwait(false);
        AutoConnectStatusText = "Auto-connect failed. Manual connect available.";
    }

    private async Task StopCoordinatorScanAsync(CancellationToken cancellationToken)
    {
        autoConnectCancellation?.Cancel();
        autoConnectCancellation?.Dispose();
        autoConnectCancellation = null;
        nameFallbackCandidates.Clear();
        isNameFallbackEvaluationPending = false;
        IsAutoConnectSearching = false;
        if (coordinatorStartedScan && scanner.IsScanning)
        {
            await scanner.StopScanningAsync(cancellationToken).ConfigureAwait(false);
        }

        coordinatorStartedScan = false;
    }

    private async Task SaveSettingsAsync(CancellationToken cancellationToken)
    {
        settings = settings.Normalize();
        await settingsStore.SaveAsync(settings, cancellationToken).ConfigureAwait(false);
        RaiseSettingsChanged();
    }

    private void RaiseSettingsChanged()
    {
        OnPropertyChanged(nameof(AutoConnectEnabled));
        OnPropertyChanged(nameof(RememberLastDeviceEnabled));
        OnPropertyChanged(nameof(LastKnownDevice));
        OnPropertyChanged(nameof(HasKnownDevice));
        OnPropertyChanged(nameof(LastKnownMaskText));
        OnPropertyChanged(nameof(CanAutoConnectNow));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
