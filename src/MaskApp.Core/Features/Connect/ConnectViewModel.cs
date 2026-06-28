using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MaskApp.Core.Features.Connect;

public sealed class ConnectViewModel : INotifyPropertyChanged
{
    private readonly IBleScanner scanner;
    private readonly IBleDeviceConnection connection;
    private DiscoveredMaskDevice? selectedDevice;
    private BleConnectionState connectionState = BleConnectionState.Disconnected;
    private bool isScanning;
    private string statusText = "Ready to scan for masks.";

    public ConnectViewModel(IBleScanner scanner, IBleDeviceConnection connection)
    {
        this.scanner = scanner;
        this.connection = connection;

        scanner.DeviceDiscovered += OnDeviceDiscovered;
        scanner.ScannerStateChanged += OnScannerStateChanged;
        connection.ConnectionStateChanged += OnConnectionStateChanged;

        StartScanCommand = new AsyncRelayCommand(StartScanAsync, () => !IsScanning);
        StopScanCommand = new AsyncRelayCommand(StopScanAsync, () => IsScanning);
        ConnectCommand = new AsyncRelayCommand(ConnectAsync, () => SelectedDevice is not null && ConnectionState is not BleConnectionState.Connected);
        DisconnectCommand = new AsyncRelayCommand(DisconnectAsync, () => ConnectionState is BleConnectionState.Connected or BleConnectionState.Connecting);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DiscoveredMaskDevice> Devices { get; } = [];

    public AsyncRelayCommand StartScanCommand { get; }

    public AsyncRelayCommand StopScanCommand { get; }

    public AsyncRelayCommand ConnectCommand { get; }

    public AsyncRelayCommand DisconnectCommand { get; }

    public DiscoveredMaskDevice? SelectedDevice
    {
        get => selectedDevice;
        set
        {
            if (SetField(ref selectedDevice, value))
            {
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

        return connection.ConnectAsync(SelectedDevice, cancellationToken);
    }

    private Task DisconnectAsync(CancellationToken cancellationToken) => connection.DisconnectAsync(cancellationToken);

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

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void RaiseCommandStates()
    {
        StartScanCommand.RaiseCanExecuteChanged();
        StopScanCommand.RaiseCanExecuteChanged();
        ConnectCommand.RaiseCanExecuteChanged();
        DisconnectCommand.RaiseCanExecuteChanged();
    }
}
