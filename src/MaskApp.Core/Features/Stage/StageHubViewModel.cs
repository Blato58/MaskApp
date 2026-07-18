using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Scenes;

namespace MaskApp.Core.Features.Stage;

public sealed class StageHubViewModel : INotifyPropertyChanged
{
    private readonly IBleDeviceConnection deviceConnection;
    private bool isPreflightMode;
    private bool isBusy;
    private string statusText = "Loading show…";
    private BleConnectionState connectionState;

    public StageHubViewModel(
        SceneStudioViewModel sceneStudio,
        FestivalPreflightViewModel preflight,
        IBleDeviceConnection deviceConnection)
    {
        SceneStudio = sceneStudio;
        Preflight = preflight;
        this.deviceConnection = deviceConnection;
        connectionState = deviceConnection.State;
        deviceConnection.ConnectionStateChanged += OnConnectionStateChanged;
        ShowBuildCommand = new AsyncRelayCommand(_ =>
        {
            IsPreflightMode = false;
            return Task.CompletedTask;
        });
        ShowPreflightCommand = new AsyncRelayCommand(_ =>
        {
            IsPreflightMode = true;
            return Task.CompletedTask;
        });
        RefreshCommand = new AsyncRelayCommand(InitializeAsync, () => !IsBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public SceneStudioViewModel SceneStudio { get; }

    public FestivalPreflightViewModel Preflight { get; }

    public AsyncRelayCommand ShowBuildCommand { get; }

    public AsyncRelayCommand ShowPreflightCommand { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public bool IsPreflightMode
    {
        get => isPreflightMode;
        private set
        {
            if (SetField(ref isPreflightMode, value))
            {
                OnPropertyChanged(nameof(IsBuildMode));
            }
        }
    }

    public bool IsBuildMode => !IsPreflightMode;

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetField(ref isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public BleConnectionState ConnectionState
    {
        get => connectionState;
        private set
        {
            if (SetField(ref connectionState, value))
            {
                OnPropertyChanged(nameof(ConnectionStatusText));
                OnPropertyChanged(nameof(ConnectionDetailText));
                OnPropertyChanged(nameof(IsDisconnected));
            }
        }
    }

    public string ConnectionStatusText => ConnectionState switch
    {
        BleConnectionState.Connected => "Connected",
        BleConnectionState.Connecting => "Connecting",
        BleConnectionState.Scanning => "Scanning",
        BleConnectionState.Failed => "Connection failed",
        BleConnectionState.Unavailable => "Bluetooth unavailable",
        _ => "Disconnected"
    };

    public string ConnectionDetailText => ConnectionState == BleConnectionState.Connected
        ? "Stage controls target the active show mask."
        : "Preflight cannot mark the show ready until the mask is connected.";

    public bool IsDisconnected => ConnectionState != BleConnectionState.Connected;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            ConnectionState = deviceConnection.State;
            await SceneStudio.InitializeAsync(cancellationToken);
            await Preflight.InitializeAsync(cancellationToken);
            StatusText = $"{SceneStudio.Setlists.Count} setlist(s) · {SceneStudio.Scenes.Count} scene(s)";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            StatusText = $"Show data could not be loaded: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs e) =>
        ConnectionState = e.State;

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
