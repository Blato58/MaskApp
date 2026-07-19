using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Delivery;
using MaskApp.Core.Features.Experience;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Live;

public sealed class LiveViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly PagesViewModel pages;
    private readonly IAppExperienceSettingsStore settingsStore;
    private readonly IBleDeviceConnection connection;
    private readonly IMaskEmergencyControl emergencyControl;
    private AppExperienceSettings settings = AppExperienceSettings.Defaults;
    private DeliveryStatus delivery = DeliveryStatus.Idle;
    private bool isEmergencyBusy;
    private bool isDisposed;

    public LiveViewModel(
        PagesViewModel pages,
        IAppExperienceSettingsStore settingsStore,
        IBleDeviceConnection connection,
        IMaskEmergencyControl emergencyControl)
    {
        this.pages = pages;
        this.settingsStore = settingsStore;
        this.connection = connection;
        this.emergencyControl = emergencyControl;

        StopCommand = new AsyncRelayCommand(StopAsync, CanUseEmergencyControl, SetCommandError);
        BlackoutCommand = new AsyncRelayCommand(BlackoutAsync, CanUseEmergencyControl, SetCommandError);
        pages.PropertyChanged += OnPagesPropertyChanged;
        pages.ActionCompleted += OnActionCompleted;
        connection.ConnectionStateChanged += OnConnectionStateChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PagesViewModel Pages => pages;

    public IReadOnlyList<GalleryPageTab> Decks => pages.Pages;

    public GalleryPageLayout SelectedDeck => pages.SelectedPage;

    public GalleryPageTab? SelectedDeckTab => Decks.FirstOrDefault(deck => deck.IsSelected);

    public IReadOnlyList<GalleryPageShortcutCard> Actions => pages.Shortcuts;

    public string SelectedDeckName => pages.SelectedPageTitle;

    public bool RequireHoldForActions
    {
        get => settings.RequiresHold(pages.SelectedPage.PageId);
        set => _ = SaveDeckHoldPreferenceAsync(value);
    }

    public BleConnectionState ConnectionState => connection.State;

    public bool IsConnected => connection.State == BleConnectionState.Connected;

    public string ConnectionText => connection.State switch
    {
        BleConnectionState.Connected => "Connected",
        BleConnectionState.Connecting => "Connecting",
        BleConnectionState.Scanning => "Scanning",
        BleConnectionState.Failed => "Connection failed",
        BleConnectionState.Unavailable => "Bluetooth unavailable",
        _ => "Disconnected"
    };

    public DeliveryStatus Delivery
    {
        get => delivery;
        private set => SetField(ref delivery, value);
    }

    public AsyncRelayCommand StopCommand { get; }

    public AsyncRelayCommand BlackoutCommand { get; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        settings = (await settingsStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        await pages.InitializeAsync(cancellationToken).ConfigureAwait(false);
        NotifyDeckStateChanged();
        NotifyConnectionChanged();
    }

    public void StartObserving()
    {
        pages.StartObservingTransportState();
        NotifyConnectionChanged();
    }

    public void StopObserving() => pages.StopObservingTransportState();

    public Task SelectDeckAsync(string deckId, CancellationToken cancellationToken = default) =>
        pages.SelectPageAsync(deckId, cancellationToken);

    public void MarkPreparing(string actionName) =>
        Delivery = DeliveryStateMapper.Preparing(string.IsNullOrWhiteSpace(actionName) ? "action" : actionName);

    private async Task SaveDeckHoldPreferenceAsync(bool value)
    {
        var deckId = pages.SelectedPage.PageId;
        if (string.IsNullOrWhiteSpace(deckId) || settings.RequiresHold(deckId) == value)
        {
            return;
        }

        settings = settings.WithDeckHold(deckId, value);
        await settingsStore.SaveAsync(settings).ConfigureAwait(false);
        OnPropertyChanged(nameof(RequireHoldForActions));
    }

    private async Task StopAsync(CancellationToken cancellationToken)
    {
        await RunEmergencyAsync("Stop", emergencyControl.StopAsync, cancellationToken).ConfigureAwait(false);
    }

    private async Task BlackoutAsync(CancellationToken cancellationToken)
    {
        await RunEmergencyAsync("Blackout", emergencyControl.BlackoutAsync, cancellationToken).ConfigureAwait(false);
    }

    private async Task RunEmergencyAsync(
        string actionName,
        Func<CancellationToken, Task<MaskCommandResult>> action,
        CancellationToken cancellationToken)
    {
        isEmergencyBusy = true;
        RefreshEmergencyCommands();
        Delivery = DeliveryStateMapper.Sending(actionName);
        try
        {
            var result = await action(cancellationToken).ConfigureAwait(false);
            Delivery = DeliveryStateMapper.FromResult(result.Succeeded, result.Message);
        }
        finally
        {
            isEmergencyBusy = false;
            RefreshEmergencyCommands();
        }
    }

    private bool CanUseEmergencyControl() => IsConnected && !isEmergencyBusy;

    private void OnActionCompleted(object? sender, GalleryActionCompletedEventArgs args) =>
        Delivery = DeliveryStateMapper.FromResult(args.Result.Succeeded, args.Result.Message);

    private void OnPagesPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(PagesViewModel.Pages) or nameof(PagesViewModel.SelectedPage) or
            nameof(PagesViewModel.SelectedPageTitle) or nameof(PagesViewModel.Shortcuts))
        {
            NotifyDeckStateChanged();
        }
    }

    private void OnConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs args)
    {
        NotifyConnectionChanged();
        if (args.State != BleConnectionState.Connected && Delivery.State == DeliveryState.Sending)
        {
            Delivery = DeliveryStatus.Unknown("Connection was lost. The action will not replay automatically.");
        }
    }

    private void NotifyDeckStateChanged()
    {
        OnPropertyChanged(nameof(Decks));
        OnPropertyChanged(nameof(SelectedDeck));
        OnPropertyChanged(nameof(SelectedDeckTab));
        OnPropertyChanged(nameof(SelectedDeckName));
        OnPropertyChanged(nameof(Actions));
        OnPropertyChanged(nameof(RequireHoldForActions));
    }

    private void NotifyConnectionChanged()
    {
        OnPropertyChanged(nameof(ConnectionState));
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(ConnectionText));
        RefreshEmergencyCommands();
    }

    private void RefreshEmergencyCommands()
    {
        StopCommand.RaiseCanExecuteChanged();
        BlackoutCommand.RaiseCanExecuteChanged();
    }

    private void SetCommandError(Exception exception) =>
        Delivery = DeliveryStatus.Failed(exception.Message);

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
        pages.PropertyChanged -= OnPagesPropertyChanged;
        pages.ActionCompleted -= OnActionCompleted;
        connection.ConnectionStateChanged -= OnConnectionStateChanged;
    }
}
