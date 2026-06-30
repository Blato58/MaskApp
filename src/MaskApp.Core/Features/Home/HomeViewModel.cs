using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Home;

public sealed class HomeViewModel : INotifyPropertyChanged
{
    private readonly QuickActionCatalog catalog;
    private readonly IQuickActionDispatcher dispatcher;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly IQuickActionTextSettingsStore quickCaptionSettingsStore;
    private readonly ITextPresetStore textPresetStore;
    private readonly ITextPresetDispatcher textPresetDispatcher;
    private readonly BleAutoConnectCoordinator autoConnectCoordinator;
    private int brightness = 60;
    private string lastActionStatus = "Ready";
    private string currentLookText = "None";
    private MaskCommandTransportState commandTransportState;
    private string commandTransportStatusText;
    private TextUploadTransportState textTransportState;
    private string textTransportStatusText;
    private bool textTransportIsReady;
    private bool textTransportSupportsAcknowledgements;
    private QuickCaptionModeOption selectedQuickCaptionMode;
    private QuickCaptionSendModeOption selectedQuickCaptionSendMode;
    private QuickCaptionForegroundPresetOption selectedQuickCaptionForegroundPreset;
    private QuickCaptionBackgroundPresetOption selectedQuickCaptionBackgroundPreset;
    private int quickCaptionSpeed = QuickActionTextSettings.RaveDefaults.Speed;
    private bool quickCaptionBackgroundEnabled;
    private bool settingsLoaded;
    private IReadOnlyList<TextPresetCard> favoriteTextPresets = [];

    public HomeViewModel(
        QuickActionCatalog catalog,
        IQuickActionDispatcher dispatcher,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport,
        IQuickActionTextSettingsStore? quickCaptionSettingsStore = null,
        ITextPresetStore? textPresetStore = null,
        ITextPresetDispatcher? textPresetDispatcher = null,
        BleAutoConnectCoordinator? autoConnectCoordinator = null)
    {
        this.catalog = catalog;
        this.dispatcher = dispatcher;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;
        this.quickCaptionSettingsStore = quickCaptionSettingsStore ?? new InMemoryQuickActionTextSettingsStore();
        this.textPresetStore = textPresetStore ?? new InMemoryTextPresetStore();
        this.textPresetDispatcher = textPresetDispatcher ?? new TextPresetDispatcher(textTransport, this.textPresetStore);
        this.autoConnectCoordinator = autoConnectCoordinator ?? new BleAutoConnectCoordinator(
            new UnavailableBleScanner(),
            new UnavailableBleConnection());

        commandTransportState = commandTransport.TransportState;
        commandTransportStatusText = commandTransport.TransportStatusText;
        textTransportState = textTransport.State;
        textTransportStatusText = textTransport.StatusText;
        textTransportIsReady = textTransport.IsReady;
        textTransportSupportsAcknowledgements = textTransport.SupportsAcknowledgements;
        QuickCaptionModeOptions =
        [
            new QuickCaptionModeOption("Flash / Blink", QuickCaptionDisplayMode.FlashBlink),
            new QuickCaptionModeOption("Scroll right-to-left", QuickCaptionDisplayMode.ScrollRightToLeft),
            new QuickCaptionModeOption("Scroll left-to-right", QuickCaptionDisplayMode.ScrollLeftToRight)
        ];
        QuickCaptionSendModeOptions =
        [
            new QuickCaptionSendModeOption("Low-static Flash", QuickCaptionSendMode.LowStaticFlash),
            new QuickCaptionSendModeOption("Stable Flash", QuickCaptionSendMode.StableFlash),
            new QuickCaptionSendModeOption("Fast Flash unstable", QuickCaptionSendMode.FastWriteOnly),
            new QuickCaptionSendModeOption("Reliable ACK", QuickCaptionSendMode.ReliableAcknowledgement)
        ];
        QuickCaptionForegroundPresetOptions =
        [
            CreateForegroundOption("White", QuickCaptionForegroundPreset.White),
            CreateForegroundOption("Cyan", QuickCaptionForegroundPreset.Cyan),
            CreateForegroundOption("Pink", QuickCaptionForegroundPreset.Pink),
            CreateForegroundOption("Amber", QuickCaptionForegroundPreset.Amber),
            CreateForegroundOption("Green", QuickCaptionForegroundPreset.Green),
            CreateForegroundOption("Red", QuickCaptionForegroundPreset.Red),
            CreateForegroundOption("Purple", QuickCaptionForegroundPreset.Purple)
        ];
        QuickCaptionBackgroundPresetOptions =
        [
            new QuickCaptionBackgroundPresetOption("Black", QuickCaptionBackgroundPreset.Black, "#000000")
        ];
        selectedQuickCaptionMode = QuickCaptionModeOptions[0];
        selectedQuickCaptionSendMode = QuickCaptionSendModeOptions[0];
        selectedQuickCaptionForegroundPreset = QuickCaptionForegroundPresetOptions[0];
        selectedQuickCaptionBackgroundPreset = QuickCaptionBackgroundPresetOptions[0];

        BlackoutCommand = new AsyncRelayCommand(
            cancellationToken => TriggerAsync(QuickActionId.Blackout, "Blackout", cancellationToken),
            () => CanUseControlCommands);
        RestoreBrightnessCommand = new AsyncRelayCommand(
            cancellationToken => TriggerAsync(QuickActionId.RestoreBrightness, "Restore brightness", cancellationToken),
            () => CanUseControlCommands);
        ApplyBrightnessCommand = new AsyncRelayCommand(
            cancellationToken => TriggerAsync(
                QuickActionId.SetBrightness,
                $"Brightness {Brightness}%",
                cancellationToken,
                new QuickActionRequest(Brightness)),
            () => CanUseControlCommands);
        RandomReactionCommand = new AsyncRelayCommand(
            cancellationToken => TriggerAsync(QuickActionId.RandomReaction, "Random reaction", cancellationToken),
            () => CanUseTextReactions);
        ResetQuickCaptionSettingsCommand = new AsyncRelayCommand(ResetQuickCaptionSettingsAsync);
        StartAutoConnectCommand = new AsyncRelayCommand(StartAutoConnectAsync, () => this.autoConnectCoordinator.CanAutoConnectNow);
        ForgetKnownMaskCommand = new AsyncRelayCommand(ForgetKnownMaskAsync, () => this.autoConnectCoordinator.HasKnownDevice);

        FavoriteReactions =
        [
            CreateReactionCard(QuickActionId.Lol, "Favorite"),
            CreateReactionCard(QuickActionId.Nope, "Favorite"),
            CreateReactionCard(QuickActionId.VibeCheck, "Favorite"),
            CreateReactionCard(QuickActionId.Drop, "Favorite")
        ];

        RecentReactions =
        [
            CreateReactionCard(QuickActionId.Bruh, "Recent"),
            CreateReactionCard(QuickActionId.SendHelp, "Recent"),
            CreateReactionCard(QuickActionId.Buffering, "Recent")
        ];

        commandTransport.TransportStateChanged += OnCommandTransportStateChanged;
        textTransport.StateChanged += OnTextTransportStateChanged;
        this.autoConnectCoordinator.PropertyChanged += OnAutoConnectPropertyChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string AppTitle => "Control Room";

    public string Summary =>
        "Recover the mask connection, control brightness, blackout, and send fast reactions from the first screen.";

    public AsyncRelayCommand BlackoutCommand { get; }

    public AsyncRelayCommand RestoreBrightnessCommand { get; }

    public AsyncRelayCommand ApplyBrightnessCommand { get; }

    public AsyncRelayCommand RandomReactionCommand { get; }

    public AsyncRelayCommand ResetQuickCaptionSettingsCommand { get; }

    public AsyncRelayCommand StartAutoConnectCommand { get; }

    public AsyncRelayCommand ForgetKnownMaskCommand { get; }

    public IReadOnlyList<HomeQuickActionCard> FavoriteReactions { get; }

    public ObservableCollection<HomeQuickActionCard> RecentReactions { get; }

    public IReadOnlyList<TextPresetCard> FavoriteTextPresets
    {
        get => favoriteTextPresets;
        private set
        {
            if (SetField(ref favoriteTextPresets, value))
            {
                OnPropertyChanged(nameof(HasFavoriteTextPresets));
            }
        }
    }

    public bool HasFavoriteTextPresets => FavoriteTextPresets.Count > 0;

    public IReadOnlyList<QuickCaptionModeOption> QuickCaptionModeOptions { get; }

    public IReadOnlyList<QuickCaptionSendModeOption> QuickCaptionSendModeOptions { get; }

    public IReadOnlyList<QuickCaptionForegroundPresetOption> QuickCaptionForegroundPresetOptions { get; }

    public IReadOnlyList<QuickCaptionBackgroundPresetOption> QuickCaptionBackgroundPresetOptions { get; }

    public int Brightness
    {
        get => brightness;
        set
        {
            if (SetField(ref brightness, Math.Clamp(value, 1, 100)))
            {
                OnPropertyChanged(nameof(BrightnessText));
            }
        }
    }

    public string BrightnessText => $"{Brightness}%";

    public string LastActionStatus
    {
        get => lastActionStatus;
        private set => SetField(ref lastActionStatus, value);
    }

    public string CurrentLookText
    {
        get => currentLookText;
        private set => SetField(ref currentLookText, value);
    }

    public MaskCommandTransportState CommandTransportState
    {
        get => commandTransportState;
        private set
        {
            if (SetField(ref commandTransportState, value))
            {
                OnPropertyChanged(nameof(CanUseControlCommands));
                OnPropertyChanged(nameof(RecoveryHint));
                RaiseCommandStates();
            }
        }
    }

    public string CommandTransportStatusText
    {
        get => commandTransportStatusText;
        private set
        {
            if (SetField(ref commandTransportStatusText, value))
            {
                OnPropertyChanged(nameof(RecoveryHint));
            }
        }
    }

    public TextUploadTransportState TextTransportState
    {
        get => textTransportState;
        private set
        {
            if (SetField(ref textTransportState, value))
            {
                OnPropertyChanged(nameof(RecoveryHint));
            }
        }
    }

    public string TextTransportStatusText
    {
        get => textTransportStatusText;
        private set
        {
            if (SetField(ref textTransportStatusText, value))
            {
                OnPropertyChanged(nameof(RecoveryHint));
            }
        }
    }

    public bool TextTransportIsReady
    {
        get => textTransportIsReady;
        private set
        {
            if (SetField(ref textTransportIsReady, value))
            {
                OnPropertyChanged(nameof(CanUseTextReactions));
                OnPropertyChanged(nameof(RecoveryHint));
                RaiseCommandStates();
            }
        }
    }

    public bool TextTransportSupportsAcknowledgements
    {
        get => textTransportSupportsAcknowledgements;
        private set
        {
            if (SetField(ref textTransportSupportsAcknowledgements, value))
            {
                OnPropertyChanged(nameof(TextAcknowledgementText));
            }
        }
    }

    public string ControlTransportText =>
        commandTransport.IsSimulated
            ? $"{commandTransport.TransportDisplayName} control (simulated)"
            : $"{commandTransport.TransportDisplayName} control (real)";

    public string TextTransportText =>
        textTransport.IsSimulated
            ? $"{textTransport.TransportDisplayName} text (simulated)"
            : $"{textTransport.TransportDisplayName} text (real)";

    public string TextAcknowledgementText =>
        TextTransportSupportsAcknowledgements
            ? "Text reactions wait for ACK confirmation."
            : "Text reactions use write-only compatibility when available.";

    public QuickCaptionModeOption SelectedQuickCaptionMode
    {
        get => selectedQuickCaptionMode;
        set
        {
            if (value is not null && SetField(ref selectedQuickCaptionMode, value))
            {
                PersistQuickCaptionSettings();
            }
        }
    }

    public QuickCaptionSendModeOption SelectedQuickCaptionSendMode
    {
        get => selectedQuickCaptionSendMode;
        set
        {
            if (value is not null && SetField(ref selectedQuickCaptionSendMode, value))
            {
                PersistQuickCaptionSettings();
            }
        }
    }

    public int QuickCaptionSpeed
    {
        get => quickCaptionSpeed;
        set
        {
            if (SetField(ref quickCaptionSpeed, Math.Clamp(value, 1, 100)))
            {
                OnPropertyChanged(nameof(QuickCaptionSpeedText));
                PersistQuickCaptionSettings();
            }
        }
    }

    public string QuickCaptionSpeedText => $"{QuickCaptionSpeed}";

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

    public bool QuickCaptionBackgroundEnabled
    {
        get => quickCaptionBackgroundEnabled;
        set
        {
            if (SetField(ref quickCaptionBackgroundEnabled, value))
            {
                PersistQuickCaptionSettings();
            }
        }
    }

    public QuickCaptionBackgroundPresetOption SelectedQuickCaptionBackgroundPreset
    {
        get => selectedQuickCaptionBackgroundPreset;
        set
        {
            if (value is not null && SetField(ref selectedQuickCaptionBackgroundPreset, value))
            {
                PersistQuickCaptionSettings();
            }
        }
    }

    public QuickCaptionForegroundPresetOption SelectedQuickCaptionForegroundPreset
    {
        get => selectedQuickCaptionForegroundPreset;
        set
        {
            if (value is not null && SetField(ref selectedQuickCaptionForegroundPreset, value))
            {
                OnPropertyChanged(nameof(QuickCaptionForegroundText));
                PersistQuickCaptionSettings();
            }
        }
    }

    public string QuickCaptionForegroundText => $"Text color {SelectedQuickCaptionForegroundPreset.Label}";

    public string QuickCaptionBackgroundNote =>
        "Low-static Flash pre-arms Blink and skips the black reset delay. Stable Flash keeps the reset as fallback; Fast Flash stayed left-aligned or solid on hardware.";

    public bool CanUseControlCommands => CommandTransportState == MaskCommandTransportState.Ready;

    public bool CanUseTextReactions => TextTransportIsReady;

    public string RecoveryHint
    {
        get
        {
            if (CanUseControlCommands && CanUseTextReactions)
            {
                return "Ready";
            }

            if (!CanUseControlCommands && !CanUseTextReactions)
            {
                return "Connect to send";
            }

            if (!CanUseControlCommands)
            {
                return "Connect to send";
            }

            return "Text not ready";
        }
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var settings = (await quickCaptionSettingsStore.LoadAsync(cancellationToken)).Normalize();
        ApplyQuickCaptionSettings(settings);
        await InitializeTextPresetsAsync(cancellationToken);
        await autoConnectCoordinator.InitializeAsync(cancellationToken);
        await autoConnectCoordinator.StartForegroundAutoConnectAsync(cancellationToken);
        settingsLoaded = true;
    }

    public async Task InitializeTextPresetsAsync(CancellationToken cancellationToken = default)
    {
        var state = await textPresetStore.LoadAsync(cancellationToken);
        FavoriteTextPresets = state.Presets
            .Where(preset => preset.ShowInControl)
            .OrderByDescending(preset => preset.IsFavorite)
            .ThenBy(preset => preset.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(CreatePresetCard)
            .ToArray();
    }

    private HomeQuickActionCard CreateReactionCard(QuickActionId actionId, string status)
    {
        var action = catalog.Get(actionId);
        return new HomeQuickActionCard(
            action.Id,
            action.Label,
            action.Caption ?? action.Label,
            status,
            new AsyncRelayCommand(
                cancellationToken => TriggerAsync(actionId, action.Label, cancellationToken),
                () => CanUseTextReactions));
    }

    private async Task TriggerAsync(
        QuickActionId actionId,
        string displayName,
        CancellationToken cancellationToken,
        QuickActionRequest? request = null)
    {
        var result = await dispatcher.TriggerAsync(actionId, request, cancellationToken);
        LastActionStatus = result.Message;

        if (!result.Succeeded)
        {
            return;
        }

        CurrentLookText = displayName;
        if (catalog.Get(actionId).Kind == QuickActionKind.Text)
        {
            MoveToRecent(actionId);
        }
    }

    private TextPresetCard CreatePresetCard(TextPreset preset)
    {
        TextPresetCard? card = null;
        card = new TextPresetCard(
            preset,
            new AsyncRelayCommand(
                cancellationToken => SendPresetAsync(card!.Preset, cancellationToken),
                () => card is not null && CanUseTextReactions));
        return card;
    }

    private async Task SendPresetAsync(TextPreset preset, CancellationToken cancellationToken)
    {
        if (!CanUseTextReactions)
        {
            LastActionStatus = "Text not ready";
            return;
        }

        var result = await textPresetDispatcher.SendAsync(preset, cancellationToken);
        LastActionStatus = result.Message;
        if (result.Succeeded)
        {
            CurrentLookText = preset.DisplayName;
        }
    }

    private Task ResetQuickCaptionSettingsAsync(CancellationToken cancellationToken)
    {
        ApplyQuickCaptionSettings(QuickActionTextSettings.RaveDefaults);
        settingsLoaded = true;
        PersistQuickCaptionSettings();
        LastActionStatus = "RAVE defaults restored";
        return Task.CompletedTask;
    }

    private void ApplyQuickCaptionSettings(QuickActionTextSettings settings)
    {
        SelectedQuickCaptionMode = QuickCaptionModeOptions.Single(option => option.Mode == settings.DisplayMode);
        SelectedQuickCaptionSendMode = QuickCaptionSendModeOptions.Single(option => option.SendMode == settings.SendMode);
        QuickCaptionSpeed = settings.Speed;
        SelectedQuickCaptionForegroundPreset = QuickCaptionForegroundPresetOptions.Single(option => option.Preset == settings.ForegroundPreset);
        QuickCaptionBackgroundEnabled = settings.BackgroundEnabled;
        SelectedQuickCaptionBackgroundPreset = QuickCaptionBackgroundPresetOptions.Single(option => option.Preset == settings.BackgroundPreset);
    }

    private void PersistQuickCaptionSettings()
    {
        if (!settingsLoaded)
        {
            return;
        }

        var settings = new QuickActionTextSettings
        {
            DisplayMode = SelectedQuickCaptionMode.Mode,
            Speed = QuickCaptionSpeed,
            SendMode = SelectedQuickCaptionSendMode.SendMode,
            ForegroundPreset = SelectedQuickCaptionForegroundPreset.Preset,
            BackgroundEnabled = QuickCaptionBackgroundEnabled,
            BackgroundPreset = SelectedQuickCaptionBackgroundPreset.Preset
        }.Normalize();

        _ = PersistQuickCaptionSettingsAsync(settings);
    }

    private async Task PersistQuickCaptionSettingsAsync(QuickActionTextSettings settings)
    {
        try
        {
            await quickCaptionSettingsStore.SaveAsync(settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            LastActionStatus = "Failed";
        }
    }

    private void MoveToRecent(QuickActionId actionId)
    {
        var action = catalog.Get(actionId);
        var existing = RecentReactions.FirstOrDefault(reaction => reaction.Label == action.Label);
        if (existing is not null)
        {
            RecentReactions.Remove(existing);
        }

        RecentReactions.Insert(0, CreateReactionCard(actionId, "Recent"));
        while (RecentReactions.Count > 4)
        {
            RecentReactions.RemoveAt(RecentReactions.Count - 1);
        }
    }

    private void OnCommandTransportStateChanged(object? sender, MaskCommandTransportStateChangedEventArgs e)
    {
        CommandTransportState = e.State;
        CommandTransportStatusText = e.Message;
        OnPropertyChanged(nameof(ControlTransportText));
    }

    private void OnTextTransportStateChanged(object? sender, TextUploadTransportStateChangedEventArgs e)
    {
        TextTransportState = e.State;
        TextTransportStatusText = e.Message;
        TextTransportIsReady = e.IsReady;
        TextTransportSupportsAcknowledgements = e.SupportsAcknowledgements;
        OnPropertyChanged(nameof(TextTransportText));
    }

    private async Task StartAutoConnectAsync(CancellationToken cancellationToken)
    {
        await autoConnectCoordinator.StartForegroundAutoConnectAsync(cancellationToken);
        LastActionStatus = autoConnectCoordinator.AutoConnectStatusText;
    }

    private async Task ForgetKnownMaskAsync(CancellationToken cancellationToken)
    {
        await autoConnectCoordinator.ForgetKnownDeviceAsync(cancellationToken);
        LastActionStatus = "Forgot mask";
    }

    private async Task SetAutoConnectEnabledAsync(bool enabled)
    {
        try
        {
            await autoConnectCoordinator.SetAutoConnectEnabledAsync(enabled);
            if (enabled)
            {
                await autoConnectCoordinator.StartForegroundAutoConnectAsync();
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            LastActionStatus = "Failed";
        }
    }

    private async Task SetRememberLastDeviceEnabledAsync(bool enabled)
    {
        try
        {
            await autoConnectCoordinator.SetRememberLastDeviceEnabledAsync(enabled);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            LastActionStatus = "Failed";
        }
    }

    private void OnAutoConnectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(AutoConnectEnabled));
        OnPropertyChanged(nameof(RememberLastDeviceEnabled));
        OnPropertyChanged(nameof(AutoConnectStatusText));
        OnPropertyChanged(nameof(LastKnownMaskText));
        OnPropertyChanged(nameof(HasKnownMask));
        OnPropertyChanged(nameof(CanStartAutoConnect));
        StartAutoConnectCommand.RaiseCanExecuteChanged();
        ForgetKnownMaskCommand.RaiseCanExecuteChanged();
    }

    private void RaiseCommandStates()
    {
        BlackoutCommand.RaiseCanExecuteChanged();
        RestoreBrightnessCommand.RaiseCanExecuteChanged();
        ApplyBrightnessCommand.RaiseCanExecuteChanged();
        RandomReactionCommand.RaiseCanExecuteChanged();
        StartAutoConnectCommand.RaiseCanExecuteChanged();
        ForgetKnownMaskCommand.RaiseCanExecuteChanged();
        foreach (var reaction in FavoriteReactions.Concat(RecentReactions))
        {
            reaction.TriggerCommand.RaiseCanExecuteChanged();
        }

        foreach (var preset in FavoriteTextPresets)
        {
            preset.SendCommand.RaiseCanExecuteChanged();
        }
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

    private static QuickCaptionForegroundPresetOption CreateForegroundOption(
        string label,
        QuickCaptionForegroundPreset preset) =>
        new(label, preset, QuickCaptionForegroundPalette.GetHex(preset));

    private sealed class UnavailableBleScanner : IBleScanner
    {
        public event EventHandler<DiscoveredMaskDevice>? DeviceDiscovered
        {
            add { }
            remove { }
        }

        public event EventHandler<BleScannerStateChangedEventArgs>? ScannerStateChanged
        {
            add { }
            remove { }
        }

        public bool IsScanning => false;

        public Task StartScanningAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task StopScanningAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class UnavailableBleConnection : IBleDeviceConnection
    {
        public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged
        {
            add { }
            remove { }
        }

        public BleConnectionState State => BleConnectionState.Unavailable;

        public Task ConnectAsync(DiscoveredMaskDevice device, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
