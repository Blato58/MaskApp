using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.Home;

public sealed class HomeViewModel : INotifyPropertyChanged
{
    private readonly QuickActionCatalog catalog;
    private readonly IQuickActionDispatcher dispatcher;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly IQuickActionTextSettingsStore quickCaptionSettingsStore;
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
    private QuickCaptionBackgroundPresetOption selectedQuickCaptionBackgroundPreset;
    private int quickCaptionSpeed = QuickActionTextSettings.RaveDefaults.Speed;
    private bool quickCaptionBackgroundEnabled;
    private bool settingsLoaded;

    public HomeViewModel(
        QuickActionCatalog catalog,
        IQuickActionDispatcher dispatcher,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport,
        IQuickActionTextSettingsStore? quickCaptionSettingsStore = null)
    {
        this.catalog = catalog;
        this.dispatcher = dispatcher;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;
        this.quickCaptionSettingsStore = quickCaptionSettingsStore ?? new InMemoryQuickActionTextSettingsStore();

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
            new QuickCaptionSendModeOption("Fast write-only", QuickCaptionSendMode.FastWriteOnly),
            new QuickCaptionSendModeOption("Reliable ACK", QuickCaptionSendMode.ReliableAcknowledgement)
        ];
        QuickCaptionBackgroundPresetOptions =
        [
            new QuickCaptionBackgroundPresetOption("RAVE purple", QuickCaptionBackgroundPreset.RavePurple, "#A855F7"),
            new QuickCaptionBackgroundPresetOption("Red alert", QuickCaptionBackgroundPreset.RedAlert, "#EF4444"),
            new QuickCaptionBackgroundPresetOption("Deep blue", QuickCaptionBackgroundPreset.DeepBlue, "#1D4ED8"),
            new QuickCaptionBackgroundPresetOption("Black", QuickCaptionBackgroundPreset.Black, "#000000")
        ];
        selectedQuickCaptionMode = QuickCaptionModeOptions[0];
        selectedQuickCaptionSendMode = QuickCaptionSendModeOptions[0];
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

    public IReadOnlyList<HomeQuickActionCard> FavoriteReactions { get; }

    public ObservableCollection<HomeQuickActionCard> RecentReactions { get; }

    public IReadOnlyList<QuickCaptionModeOption> QuickCaptionModeOptions { get; }

    public IReadOnlyList<QuickCaptionSendModeOption> QuickCaptionSendModeOptions { get; }

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

    public string QuickCaptionBackgroundNote =>
        "Background presets use BC/FC evidence and need real-mask test. Captions still send if style commands are unavailable.";

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
        var settings = (await quickCaptionSettingsStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        ApplyQuickCaptionSettings(settings);
        settingsLoaded = true;
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
        var result = await dispatcher.TriggerAsync(actionId, request, cancellationToken).ConfigureAwait(false);
        LastActionStatus = result.Succeeded
            ? "Sent, confirm on mask"
            : "Failed";

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
            BackgroundEnabled = QuickCaptionBackgroundEnabled,
            BackgroundPreset = SelectedQuickCaptionBackgroundPreset.Preset
        }.Normalize();

        _ = PersistQuickCaptionSettingsAsync(settings);
    }

    private async Task PersistQuickCaptionSettingsAsync(QuickActionTextSettings settings)
    {
        try
        {
            await quickCaptionSettingsStore.SaveAsync(settings).ConfigureAwait(false);
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

    private void RaiseCommandStates()
    {
        BlackoutCommand.RaiseCanExecuteChanged();
        RestoreBrightnessCommand.RaiseCanExecuteChanged();
        ApplyBrightnessCommand.RaiseCanExecuteChanged();
        RandomReactionCommand.RaiseCanExecuteChanged();
        foreach (var reaction in FavoriteReactions.Concat(RecentReactions))
        {
            reaction.TriggerCommand.RaiseCanExecuteChanged();
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
}
