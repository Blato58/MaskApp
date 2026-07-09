using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MaskApp.Core.Features.Connect;

public sealed class ConnectViewModel : INotifyPropertyChanged
{
    private readonly IBleScanner scanner;
    private readonly BleAutoConnectCoordinator autoConnectCoordinator;
    private DiscoveredMaskDevice? selectedDevice;
    private BleConnectionState connectionState = BleConnectionState.Disconnected;
    private bool isScanning;
    private string statusText = "Ready to scan for masks.";

    public ConnectViewModel(
        IBleScanner scanner,
        IBleDeviceConnection connection,
        BleAutoConnectCoordinator? autoConnectCoordinator = null)
    {
        this.scanner = scanner;
        this.autoConnectCoordinator = autoConnectCoordinator ?? new BleAutoConnectCoordinator(scanner, connection);

        scanner.DeviceDiscovered += OnDeviceDiscovered;
        scanner.ScannerStateChanged += OnScannerStateChanged;
        connection.ConnectionStateChanged += OnConnectionStateChanged;
        this.autoConnectCoordinator.PropertyChanged += OnAutoConnectPropertyChanged;

        StartScanCommand = new AsyncRelayCommand(StartScanAsync, () => !IsScanning);
        StopScanCommand = new AsyncRelayCommand(StopScanAsync, () => IsScanning);
        ConnectCommand = new AsyncRelayCommand(ConnectAsync, () => SelectedDevice is not null && ConnectionState is not BleConnectionState.Connected);
        DisconnectCommand = new AsyncRelayCommand(DisconnectAsync, () => ConnectionState is BleConnectionState.Connected or BleConnectionState.Connecting);
        StartAutoConnectCommand = new AsyncRelayCommand(StartAutoConnectAsync, () => this.autoConnectCoordinator.CanAutoConnectNow);
        ForgetKnownMaskCommand = new AsyncRelayCommand(ForgetKnownMaskAsync, () => this.autoConnectCoordinator.HasKnownDevice);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DiscoveredMaskDevice> Devices { get; } = [];

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

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await autoConnectCoordinator.InitializeAsync(cancellationToken);
        await autoConnectCoordinator.StartForegroundAutoConnectAsync(cancellationToken);
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
    }

    private async Task StartAutoConnectAsync(CancellationToken cancellationToken)
    {
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
        StartAutoConnectCommand.RaiseCanExecuteChanged();
        ForgetKnownMaskCommand.RaiseCanExecuteChanged();
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
