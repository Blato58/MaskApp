using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Scenes;

namespace MaskApp.Core.Features.Shows;

public sealed class ShowsViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly SceneStudioViewModel studio;
    private readonly PreflightStatusSession preflightSession;
    private readonly IBleDeviceConnection connection;
    private PreflightStatusSnapshot preflight;
    private bool isDisposed;

    public ShowsViewModel(
        SceneStudioViewModel studio,
        PreflightStatusSession preflightSession,
        IBleDeviceConnection connection)
    {
        this.studio = studio;
        this.preflightSession = preflightSession;
        this.connection = connection;
        preflight = preflightSession.Snapshot;
        studio.PropertyChanged += OnStudioPropertyChanged;
        preflightSession.SnapshotChanged += OnPreflightChanged;
        connection.ConnectionStateChanged += OnConnectionChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public SceneStudioViewModel Studio => studio;

    public PerformanceSetlist ActiveShow => studio.CurrentSetlist;

    public string ActiveShowName => studio.CurrentSetlist.DisplayName;

    public IReadOnlyList<SetlistCueRow> Cues => studio.CueRows;

    public IReadOnlyList<PerformanceSetlist> Shows => studio.Setlists;

    public string PositionText => studio.SetlistPositionText;

    public string StatusText => studio.StatusText;

    public PreflightStatusSnapshot Preflight
    {
        get => preflight;
        private set => SetField(ref preflight, value);
    }

    public BleConnectionState ConnectionState => connection.State;

    public string ConnectionText => connection.State == BleConnectionState.Connected ? "Connected" : "Disconnected";

    public Task InitializeAsync(CancellationToken cancellationToken = default) => studio.InitializeAsync(cancellationToken);

    public void SelectShow(string showId) => studio.SelectSetlist(showId);

    private void OnStudioPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(SceneStudioViewModel.CurrentSetlist) or nameof(SceneStudioViewModel.Setlists) or
            nameof(SceneStudioViewModel.CueRows) or nameof(SceneStudioViewModel.SetlistPositionText) or
            nameof(SceneStudioViewModel.StatusText))
        {
            OnPropertyChanged(nameof(ActiveShow));
            OnPropertyChanged(nameof(ActiveShowName));
            OnPropertyChanged(nameof(Cues));
            OnPropertyChanged(nameof(Shows));
            OnPropertyChanged(nameof(PositionText));
            OnPropertyChanged(nameof(StatusText));
        }
    }

    private void OnPreflightChanged(object? sender, PreflightStatusSnapshot snapshot) => Preflight = snapshot;

    private void OnConnectionChanged(object? sender, BleConnectionStateChangedEventArgs args)
    {
        OnPropertyChanged(nameof(ConnectionState));
        OnPropertyChanged(nameof(ConnectionText));
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

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        studio.PropertyChanged -= OnStudioPropertyChanged;
        preflightSession.SnapshotChanged -= OnPreflightChanged;
        connection.ConnectionStateChanged -= OnConnectionChanged;
    }
}
