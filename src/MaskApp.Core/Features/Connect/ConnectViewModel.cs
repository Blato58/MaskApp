using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Lifecycle;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Profiles;

namespace MaskApp.Core.Features.Connect;

public sealed class ConnectViewModel : INotifyPropertyChanged
{
    private readonly IBleScanner scanner;
    private readonly BleAutoConnectCoordinator autoConnectCoordinator;
    private readonly IBleDeviceConnection connection;
    private readonly MaskProfileSession? profileSession;
    private readonly MaskBleScheduler? scheduler;
    private readonly PerformanceAnimationEngine? animationEngine;
    private readonly PreflightStatusSession? preflightStatusSession;
    private readonly AppLifecycleCoordinator? lifecycleCoordinator;
    private DiscoveredMaskDevice? selectedDevice;
    private BleConnectionState connectionState = BleConnectionState.Disconnected;
    private bool isScanning;
    private string statusText = "Ready to scan for masks.";
    private MaskProfile? activeProfile;
    private MaskBleSchedulerSnapshot? schedulerSnapshot;
    private AnimationPlaybackSnapshot animationSnapshot = new();
    private PreflightStatusSnapshot preflightSnapshot = PreflightStatusSnapshot.NotRun;
    private AppLifecycleSnapshot lifecycleSnapshot = new();
    private string diagnosticsStatusText = "Diagnostics use observed state only.";
    private IReadOnlyList<string> reconnectHistory = [];
    private IReadOnlyList<string> recentSchedulerErrors = [];

    public ConnectViewModel(
        IBleScanner scanner,
        IBleDeviceConnection connection,
        BleAutoConnectCoordinator? autoConnectCoordinator = null,
        MaskProfileSession? profileSession = null,
        MaskBleScheduler? scheduler = null,
        PerformanceAnimationEngine? animationEngine = null,
        PreflightStatusSession? preflightStatusSession = null,
        AppLifecycleCoordinator? lifecycleCoordinator = null)
    {
        this.scanner = scanner;
        this.connection = connection;
        connectionState = connection.State;
        this.autoConnectCoordinator = autoConnectCoordinator ?? new BleAutoConnectCoordinator(scanner, connection);
        this.profileSession = profileSession;
        this.scheduler = scheduler;
        this.animationEngine = animationEngine;
        this.preflightStatusSession = preflightStatusSession;
        this.lifecycleCoordinator = lifecycleCoordinator;
        schedulerSnapshot = scheduler?.GetSnapshot();
        animationSnapshot = animationEngine?.GetSnapshot() ?? new AnimationPlaybackSnapshot();
        preflightSnapshot = preflightStatusSession?.Snapshot ?? PreflightStatusSnapshot.NotRun;
        lifecycleSnapshot = lifecycleCoordinator?.Snapshot ?? new AppLifecycleSnapshot();
        CaptureSchedulerError(schedulerSnapshot?.LastError);

        scanner.DeviceDiscovered += OnDeviceDiscovered;
        scanner.ScannerStateChanged += OnScannerStateChanged;
        connection.ConnectionStateChanged += OnConnectionStateChanged;
        this.autoConnectCoordinator.PropertyChanged += OnAutoConnectPropertyChanged;
        if (scheduler is not null)
        {
            scheduler.DiagnosticsChanged += OnSchedulerDiagnosticsChanged;
        }

        if (animationEngine is not null)
        {
            animationEngine.SnapshotChanged += OnAnimationSnapshotChanged;
        }

        if (preflightStatusSession is not null)
        {
            preflightStatusSession.SnapshotChanged += OnPreflightSnapshotChanged;
        }

        if (lifecycleCoordinator is not null)
        {
            lifecycleCoordinator.SnapshotChanged += OnLifecycleSnapshotChanged;
        }

        StartScanCommand = new AsyncRelayCommand(StartScanAsync, () => !IsScanning);
        StopScanCommand = new AsyncRelayCommand(StopScanAsync, () => IsScanning);
        ConnectCommand = new AsyncRelayCommand(ConnectAsync, () => SelectedDevice is not null && ConnectionState is not BleConnectionState.Connected);
        DisconnectCommand = new AsyncRelayCommand(DisconnectAsync, () => ConnectionState is BleConnectionState.Connected or BleConnectionState.Connecting);
        StartAutoConnectCommand = new AsyncRelayCommand(StartAutoConnectAsync, () => this.autoConnectCoordinator.CanAutoConnectNow);
        ForgetKnownMaskCommand = new AsyncRelayCommand(ForgetKnownMaskAsync, () => this.autoConnectCoordinator.HasKnownDevice);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DiscoveredMaskDevice> Devices { get; } = [];

    public IReadOnlyList<string> ReconnectHistory
    {
        get => reconnectHistory;
        private set
        {
            if (SetField(ref reconnectHistory, value))
            {
                OnPropertyChanged(nameof(HasReconnectHistory));
            }
        }
    }

    public bool HasReconnectHistory => ReconnectHistory.Count > 0;

    public IReadOnlyList<string> RecentRedactedErrors => recentSchedulerErrors
        .Concat(lifecycleSnapshot.RecentErrors.Select(error =>
            $"Lifecycle {error.Operation}: {error.ErrorType}: {error.Message}"))
        .Select(RedactDiagnosticText)
        .ToArray();

    public bool HasRecentRedactedErrors => RecentRedactedErrors.Count > 0;

    public AsyncRelayCommand StartScanCommand { get; }

    public AsyncRelayCommand StopScanCommand { get; }

    public AsyncRelayCommand ConnectCommand { get; }

    public AsyncRelayCommand DisconnectCommand { get; }

    public AsyncRelayCommand StartAutoConnectCommand { get; }

    public AsyncRelayCommand ForgetKnownMaskCommand { get; }

    public DiscoveredMaskDevice? SelectedDevice
    {
        get => selectedDevice;
        set
        {
            if (SetField(ref selectedDevice, value))
            {
                OnPropertyChanged(nameof(DeviceNameText));
                OnPropertyChanged(nameof(DeviceSignalText));
                OnPropertyChanged(nameof(SchedulerErrorText));
                OnPropertyChanged(nameof(RecentRedactedErrors));
                RaiseCommandStates();
            }
        }
    }

    public BleConnectionState ConnectionState
    {
        get => connectionState;
        private set
        {
            if (SetField(ref connectionState, value))
            {
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(CanDisconnect));
                OnPropertyChanged(nameof(ConnectionHeadline));
                OnPropertyChanged(nameof(ConnectionDetailText));
                OnPropertyChanged(nameof(DeviceNameText));
                OnPropertyChanged(nameof(DeviceSignalText));
                RaiseCommandStates();
            }
        }
    }

    public bool IsScanning
    {
        get => isScanning;
        private set
        {
            if (SetField(ref isScanning, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public bool AutoConnectEnabled
    {
        get => autoConnectCoordinator.AutoConnectEnabled;
        set
        {
            if (value != autoConnectCoordinator.AutoConnectEnabled)
            {
                _ = SetAutoConnectEnabledAsync(value);
            }
        }
    }

    public bool RememberLastDeviceEnabled
    {
        get => autoConnectCoordinator.RememberLastDeviceEnabled;
        set
        {
            if (value != autoConnectCoordinator.RememberLastDeviceEnabled)
            {
                _ = SetRememberLastDeviceEnabledAsync(value);
            }
        }
    }

    public string AutoConnectStatusText => autoConnectCoordinator.AutoConnectStatusText;

    public string LastKnownMaskText => autoConnectCoordinator.LastKnownMaskText;

    public bool HasKnownMask => autoConnectCoordinator.HasKnownDevice;

    public bool CanStartAutoConnect => autoConnectCoordinator.CanAutoConnectNow;

    public bool IsConnected => ConnectionState == BleConnectionState.Connected;

    public bool CanDisconnect =>
        ConnectionState is BleConnectionState.Connected or BleConnectionState.Connecting;

    public string ConnectionHeadline => ConnectionState switch
    {
        BleConnectionState.Connected => "Connected",
        BleConnectionState.Connecting => "Connecting",
        BleConnectionState.Scanning => "Scanning",
        BleConnectionState.Failed => "Connection Error",
        _ => "Not Connected"
    };

    public string ConnectionDetailText => IsConnected
        ? "Mask command and text transports can send when diagnostics show ready."
        : "Scan nearby masks or reconnect a remembered mask.";

    public string DeviceNameText => SelectedDevice?.Name
        ?? (HasKnownMask ? LastKnownMaskText : "LED Mask");

    public string DeviceSignalText => SelectedDevice is null ? "Signal unavailable" : $"{SelectedDevice.SignalStrength} dBm";

    public bool HasActiveProfile => activeProfile is not null;

    public bool CanResetPreparedSlots => activeProfile is { PreparedSlots.Count: > 0 };

    public string FirmwareText => string.IsNullOrWhiteSpace(activeProfile?.Capabilities.FirmwareRevision)
        ? "Unavailable"
        : activeProfile.Capabilities.FirmwareRevision;

    public string TransportText => string.IsNullOrWhiteSpace(activeProfile?.Capabilities.TransportName)
        ? "Unavailable"
        : activeProfile.Capabilities.TransportName;

    public string AcknowledgementText => activeProfile is null
        ? "Unavailable"
        : activeProfile.Capabilities.AcknowledgementMode.ToString();

    public string CommandWriteText => CapabilityText(activeProfile?.Capabilities.CommandWriteAvailable);

    public string TextUploadText => CapabilityText(activeProfile?.Capabilities.TextUploadAvailable);

    public string FaceUploadText => CapabilityText(activeProfile?.Capabilities.FaceUploadAvailable);

    public string UploadCapabilitiesText => $"{TextUploadText} / {FaceUploadText}";

    public string PreparedSlotText => activeProfile is null
        ? "Unavailable"
        : $"{activeProfile.PreparedSlots.Count} of {activeProfile.Capabilities.DiySlotCapacity} recorded";

    public string PreparedSlotDetailText => activeProfile?.PreparedStateStatus
        ?? "Connect a mask before the app can scope prepared content.";

    public string LatencyText => activeProfile?.AverageCommandLatencyMilliseconds is { } latency
        ? $"{latency:0.#} ms observed"
        : "Unavailable (not measured)";

    public string CadenceText => activeProfile?.SustainableCadenceHz is { } cadence
        ? $"{cadence:0.##} Hz observed"
        : "Unavailable (not measured)";

    public string BatteryText => "Unavailable (not reported)";

    public string SchedulerQueueText => schedulerSnapshot is null
        ? "Unavailable"
        : schedulerSnapshot.PendingOperationCount == 0
            ? "Idle"
            : $"{schedulerSnapshot.PendingOperationCount} queued";

    public string SchedulerLastDurationText => schedulerSnapshot?.LastOperationDuration is { } duration
        ? $"{duration.TotalMilliseconds:0.#} ms"
        : "Unavailable";

    public string SchedulerErrorText => string.IsNullOrWhiteSpace(schedulerSnapshot?.LastError)
        ? "None recorded"
        : RedactDiagnosticText(schedulerSnapshot.LastError!);

    public string AnimationStateText => animationSnapshot.State.ToString();

    public string AnimationDeliveryText => animationSnapshot.FramesSent == 0 && animationSnapshot.FramesDropped == 0
        ? "No playback sample"
        : $"{animationSnapshot.FramesSent} sent · {animationSnapshot.FramesDropped} dropped · {animationSnapshot.LateFrames} late";

    public string LifecycleText =>
        $"{lifecycleSnapshot.Phase} · {lifecycleSnapshot.LastTransitionAtUtc:u}";

    public string LifecycleLimitationText => lifecycleSnapshot.Phase == AppLifecyclePhase.Background
        ? "Backgrounded: scanning, microphone capture, and rapid phone-timed playback are stopped. Reconnect never replays output automatically."
        : "Foreground: scanning, microphone capture, and rapid phone-timed playback are available when their other readiness gates pass.";

    public string PreflightStatusText => preflightSnapshot.StatusText;

    public string PreflightSummaryText => preflightSnapshot.Summary;

    public string PreflightIcon => preflightSnapshot.Status switch
    {
        FestivalPreflightStatus.ShowReady => "✓",
        FestivalPreflightStatus.Degraded => "!",
        _ => "×"
    };

    public string PreflightColorHex => preflightSnapshot.Status switch
    {
        FestivalPreflightStatus.ShowReady => "#22C55E",
        FestivalPreflightStatus.Degraded => "#F59E0B",
        _ => "#EF4444"
    };

    public string DiagnosticsStatusText
    {
        get => diagnosticsStatusText;
        private set => SetField(ref diagnosticsStatusText, value);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await autoConnectCoordinator.InitializeAsync(cancellationToken);
        await autoConnectCoordinator.StartForegroundAutoConnectAsync(cancellationToken);
        await RefreshDiagnosticsAsync(cancellationToken);
    }

    public async Task RefreshDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        activeProfile = profileSession is null
            ? null
            : await profileSession.GetActiveProfileAsync(cancellationToken);
        schedulerSnapshot = scheduler?.GetSnapshot();
        animationSnapshot = animationEngine?.GetSnapshot() ?? new AnimationPlaybackSnapshot();
        preflightSnapshot = preflightStatusSession?.Snapshot ?? PreflightStatusSnapshot.NotRun;
        lifecycleSnapshot = lifecycleCoordinator?.Snapshot ?? new AppLifecycleSnapshot();
        DiagnosticsStatusText = activeProfile is null
            ? "No active mask profile. Connect a mask to collect capability evidence."
            : "Diagnostics refreshed from the active mask profile and current transports.";
        NotifyDiagnosticsChanged();
    }

    public async Task<bool> ResetPreparedSlotsAsync(CancellationToken cancellationToken = default)
    {
        if (profileSession is null || activeProfile is null)
        {
            DiagnosticsStatusText = "No active mask profile to reset.";
            return false;
        }

        await profileSession.ReplacePreparedSlotsAsync([], cancellationToken);
        await RefreshDiagnosticsAsync(cancellationToken);
        DiagnosticsStatusText = "Cleared only the app's prepared-slot ledger for the active mask. Physical mask content was not erased.";
        return true;
    }

    public async Task<string> BuildRedactedDiagnosticsReportAsync(CancellationToken cancellationToken = default)
    {
        await RefreshDiagnosticsAsync(cancellationToken);
        var report = new StringBuilder()
            .AppendLine("MaskApp redacted diagnostics")
            .AppendLine($"Generated (UTC): {DateTimeOffset.UtcNow:O}")
            .AppendLine("Device identifiers and remembered names: REDACTED")
            .AppendLine($"Connection: {ConnectionState}")
            .AppendLine($"Signal: {(SelectedDevice is null ? "Unavailable" : $"{SelectedDevice.SignalStrength} dBm")}")
            .AppendLine($"Transport: {TransportText}")
            .AppendLine($"Firmware: {FirmwareText}")
            .AppendLine($"Acknowledgement mode: {AcknowledgementText}")
            .AppendLine($"Command write: {CommandWriteText}")
            .AppendLine($"Text upload: {TextUploadText}")
            .AppendLine($"Face upload: {FaceUploadText}")
            .AppendLine($"Prepared slots: {PreparedSlotText}")
            .AppendLine($"Command latency: {LatencyText}")
            .AppendLine($"Sustainable cadence: {CadenceText}")
            .AppendLine($"Battery: {BatteryText}")
            .AppendLine($"Scheduler queue: {SchedulerQueueText}")
            .AppendLine($"Scheduler last duration: {SchedulerLastDurationText}")
            .AppendLine($"Scheduler last error: {SchedulerErrorText}")
            .AppendLine($"Animation state: {AnimationStateText}")
            .AppendLine($"Animation delivery: {AnimationDeliveryText}")
            .AppendLine($"Lifecycle: {LifecycleText}")
            .AppendLine($"Lifecycle limitation: {LifecycleLimitationText}")
            .AppendLine($"Last preflight: {PreflightStatusText}")
            .AppendLine($"Preflight summary: {PreflightSummaryText}");
        if (RecentRedactedErrors.Count == 0)
        {
            report.AppendLine("Recent redacted errors: None recorded");
        }
        else
        {
            report.AppendLine("Recent redacted errors:");
            foreach (var error in RecentRedactedErrors)
            {
                report.AppendLine($"- {error}");
            }
        }

        return report.ToString();
    }

    private async Task StartScanAsync(CancellationToken cancellationToken)
    {
        Devices.Clear();
        StatusText = "Scanning for masks...";
        await scanner.StartScanningAsync(cancellationToken);
    }

    private Task StopScanAsync(CancellationToken cancellationToken) => scanner.StopScanningAsync(cancellationToken);

    private Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (SelectedDevice is null)
        {
            return Task.CompletedTask;
        }

        return autoConnectCoordinator.ConnectManuallyAsync(SelectedDevice, cancellationToken);
    }

    private Task DisconnectAsync(CancellationToken cancellationToken) =>
        autoConnectCoordinator.DisconnectManuallyAsync(cancellationToken);

    private void OnDeviceDiscovered(object? sender, DiscoveredMaskDevice device)
    {
        if (Devices.Any(existing => existing.Id == device.Id))
        {
            return;
        }

        Devices.Add(device);
        StatusText = $"{Devices.Count} mask device(s) found.";
    }

    private void OnScannerStateChanged(object? sender, BleScannerStateChangedEventArgs e)
    {
        IsScanning = e.IsScanning;
        StatusText = e.Message;
    }

    private void OnConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs e)
    {
        ConnectionState = e.State;
        StatusText = e.Message;
        AddReconnectHistory($"{e.State}: {e.Message}");
        if (e.State is BleConnectionState.Connected or BleConnectionState.Disconnected or BleConnectionState.Failed)
        {
            _ = RefreshDiagnosticsAsync();
        }
    }

    private async Task StartAutoConnectAsync(CancellationToken cancellationToken)
    {
        AddReconnectHistory("Reconnect requested for the remembered mask.");
        await autoConnectCoordinator.StartForegroundAutoConnectAsync(cancellationToken);
        StatusText = autoConnectCoordinator.AutoConnectStatusText;
    }

    private async Task ForgetKnownMaskAsync(CancellationToken cancellationToken)
    {
        await autoConnectCoordinator.ForgetKnownDeviceAsync(cancellationToken);
        StatusText = "Forgot remembered mask.";
    }

    private async Task SetAutoConnectEnabledAsync(bool enabled)
    {
        await autoConnectCoordinator.SetAutoConnectEnabledAsync(enabled);
        if (enabled)
        {
            await autoConnectCoordinator.StartForegroundAutoConnectAsync();
        }
    }

    private Task SetRememberLastDeviceEnabledAsync(bool enabled) =>
        autoConnectCoordinator.SetRememberLastDeviceEnabledAsync(enabled);

    private void OnAutoConnectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(AutoConnectEnabled));
        OnPropertyChanged(nameof(RememberLastDeviceEnabled));
        OnPropertyChanged(nameof(AutoConnectStatusText));
        OnPropertyChanged(nameof(LastKnownMaskText));
        OnPropertyChanged(nameof(HasKnownMask));
        OnPropertyChanged(nameof(CanStartAutoConnect));
        OnPropertyChanged(nameof(DeviceNameText));
        OnPropertyChanged(nameof(ConnectionDetailText));
        OnPropertyChanged(nameof(SchedulerErrorText));
        OnPropertyChanged(nameof(RecentRedactedErrors));
        StartAutoConnectCommand.RaiseCanExecuteChanged();
        ForgetKnownMaskCommand.RaiseCanExecuteChanged();
    }

    private void OnSchedulerDiagnosticsChanged(object? sender, MaskBleSchedulerDiagnosticsChangedEventArgs e)
    {
        schedulerSnapshot = e.Snapshot;
        CaptureSchedulerError(e.Snapshot.LastError);
        NotifyDiagnosticsChanged();
    }

    private void OnAnimationSnapshotChanged(object? sender, AnimationPlaybackSnapshotChangedEventArgs e)
    {
        animationSnapshot = e.Snapshot;
        NotifyDiagnosticsChanged();
    }

    private void OnPreflightSnapshotChanged(object? sender, PreflightStatusSnapshot e)
    {
        preflightSnapshot = e;
        NotifyDiagnosticsChanged();
    }

    private void OnLifecycleSnapshotChanged(object? sender, AppLifecycleSnapshotChangedEventArgs e)
    {
        lifecycleSnapshot = e.Snapshot;
        NotifyDiagnosticsChanged();
    }

    private void NotifyDiagnosticsChanged()
    {
        OnPropertyChanged(nameof(HasActiveProfile));
        OnPropertyChanged(nameof(CanResetPreparedSlots));
        OnPropertyChanged(nameof(FirmwareText));
        OnPropertyChanged(nameof(TransportText));
        OnPropertyChanged(nameof(AcknowledgementText));
        OnPropertyChanged(nameof(CommandWriteText));
        OnPropertyChanged(nameof(TextUploadText));
        OnPropertyChanged(nameof(FaceUploadText));
        OnPropertyChanged(nameof(UploadCapabilitiesText));
        OnPropertyChanged(nameof(PreparedSlotText));
        OnPropertyChanged(nameof(PreparedSlotDetailText));
        OnPropertyChanged(nameof(LatencyText));
        OnPropertyChanged(nameof(CadenceText));
        OnPropertyChanged(nameof(BatteryText));
        OnPropertyChanged(nameof(SchedulerQueueText));
        OnPropertyChanged(nameof(SchedulerLastDurationText));
        OnPropertyChanged(nameof(SchedulerErrorText));
        OnPropertyChanged(nameof(RecentRedactedErrors));
        OnPropertyChanged(nameof(HasRecentRedactedErrors));
        OnPropertyChanged(nameof(AnimationStateText));
        OnPropertyChanged(nameof(AnimationDeliveryText));
        OnPropertyChanged(nameof(LifecycleText));
        OnPropertyChanged(nameof(LifecycleLimitationText));
        OnPropertyChanged(nameof(PreflightStatusText));
        OnPropertyChanged(nameof(PreflightSummaryText));
        OnPropertyChanged(nameof(PreflightIcon));
        OnPropertyChanged(nameof(PreflightColorHex));
    }

    private void AddReconnectHistory(string detail)
    {
        ReconnectHistory =
        [
            $"{DateTimeOffset.Now:HH:mm:ss} · {detail}",
            .. ReconnectHistory.Take(7)
        ];
    }

    private void CaptureSchedulerError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error)
            || recentSchedulerErrors.FirstOrDefault()?.EndsWith(error, StringComparison.Ordinal) == true)
        {
            return;
        }

        recentSchedulerErrors =
        [
            $"{DateTimeOffset.Now:HH:mm:ss} · {error}",
            .. recentSchedulerErrors.Take(7)
        ];
        OnPropertyChanged(nameof(RecentRedactedErrors));
        OnPropertyChanged(nameof(HasRecentRedactedErrors));
    }

    private string RedactDiagnosticText(string value)
    {
        var redacted = value.Length > 512 ? $"{value[..512]}…" : value;
        var knownValues = new[]
        {
            SelectedDevice?.Id,
            SelectedDevice?.Name,
            HasKnownMask ? LastKnownMaskText : null
        };
        foreach (var knownValue in knownValues.Where(candidate => !string.IsNullOrWhiteSpace(candidate)))
        {
            redacted = redacted.Replace(knownValue!, "[REDACTED]", StringComparison.OrdinalIgnoreCase);
        }

        redacted = Regex.Replace(
            redacted,
            @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
            "[REDACTED UUID]",
            RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(50));
        return Regex.Replace(
            redacted,
            @"\b(?:[0-9a-fA-F]{2}[:-]){5}[0-9a-fA-F]{2}\b",
            "[REDACTED ADDRESS]",
            RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(50));
    }

    private static string CapabilityText(bool? value) => value switch
    {
        true => "Ready",
        false => "Unavailable",
        null => "Unknown"
    };

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

    private void RaiseCommandStates()
    {
        StartScanCommand.RaiseCanExecuteChanged();
        StopScanCommand.RaiseCanExecuteChanged();
        ConnectCommand.RaiseCanExecuteChanged();
        DisconnectCommand.RaiseCanExecuteChanged();
        StartAutoConnectCommand.RaiseCanExecuteChanged();
        ForgetKnownMaskCommand.RaiseCanExecuteChanged();
    }
}
