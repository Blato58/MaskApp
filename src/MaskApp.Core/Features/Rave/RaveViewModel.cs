using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Rave;

public sealed class RaveViewModel : INotifyPropertyChanged
{
    private readonly IMaskCommandTransport maskTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly QuickActionCatalog catalog;
    private readonly IQuickActionDispatcher dispatcher;
    private readonly IBuiltInAssetArchiveStore archiveStore;
    private readonly ITextPresetStore textPresetStore;
    private readonly ITextPresetDispatcher textPresetDispatcher;
    private readonly IQuickActionTextSettingsStore quickCaptionSettingsStore;
    private readonly BleAutoConnectCoordinator? autoConnectCoordinator;
    private int brightnessCap = 65;
    private int restoreBrightness = 65;
    private bool isSending;
    private IReadOnlyList<BuiltInAssetAction> favoriteBuiltIns = [];
    private IReadOnlyList<TextPresetGroup> presetGroups = [];
    private string sendStatusText = "Ready";
    private string maskStatusText;
    private string textStatusText;
    private string lastActionText = "None";
    private string lastPayloadText = "None";
    private string quickCaptionProfileText = "Text: Low-static Flash · white";

    public RaveViewModel(
        QuickActionCatalog catalog,
        IQuickActionDispatcher dispatcher,
        IMaskCommandTransport maskTransport,
        ITextUploadTransport textTransport,
        IBuiltInAssetArchiveStore? archiveStore = null,
        IQuickActionTextSettingsStore? quickCaptionSettingsStore = null,
        ITextPresetStore? textPresetStore = null,
        ITextPresetDispatcher? textPresetDispatcher = null,
        BleAutoConnectCoordinator? autoConnectCoordinator = null)
    {
        this.catalog = catalog;
        this.dispatcher = dispatcher;
        this.maskTransport = maskTransport;
        this.textTransport = textTransport;
        this.archiveStore = archiveStore ?? new InMemoryBuiltInAssetArchiveStore();
        this.quickCaptionSettingsStore = quickCaptionSettingsStore ?? new InMemoryQuickActionTextSettingsStore();
        this.textPresetStore = textPresetStore ?? new InMemoryTextPresetStore();
        this.textPresetDispatcher = textPresetDispatcher ?? new TextPresetDispatcher(textTransport, this.textPresetStore);
        this.autoConnectCoordinator = autoConnectCoordinator;
        maskStatusText = maskTransport.TransportStatusText;
        textStatusText = textTransport.StatusText;

        Actions =
        [
            CreateAction(QuickActionId.Drop, "DnB"),
            CreateAction(QuickActionId.WheelUp, "DnB"),
            CreateAction(QuickActionId.Reload, "DnB"),
            CreateAction(QuickActionId.Boh, "DnB"),
            CreateAction(QuickActionId.PullUp, "DnB"),
            CreateAction(QuickActionId.RunItBack, "DnB"),
            CreateAction(QuickActionId.BassFaceManual, "DnB"),
            CreateAction(QuickActionId.Hydrate, "Welfare"),
            CreateAction(QuickActionId.Water, "Welfare"),
            CreateAction(QuickActionId.AllGood, "Welfare"),
            CreateAction(QuickActionId.NiceMoves, "Social"),
            CreateAction(QuickActionId.VibeCheck, "Social"),
            CreateAction(QuickActionId.NoThoughts, "DnB"),
            CreateAction(QuickActionId.WhereWater, "Welfare"),
            CreateAction(QuickActionId.ILiveHere, "DnB"),
            CreateAction(QuickActionId.TooMuchBass, "DnB")
        ];

        CommandFallbackActions =
        [
            CreateCommandFallbackAction(QuickActionId.TestImage1),
            CreateCommandFallbackAction(QuickActionId.TestImage2),
            CreateCommandFallbackAction(QuickActionId.TestAnimation1),
            CreateCommandFallbackAction(QuickActionId.TestAnimation2)
        ];

        BlackoutCommand = new AsyncRelayCommand(BlackoutAsync, CanSendBrightnessCommand);
        RestoreCommand = new AsyncRelayCommand(RestoreAsync, CanSendBrightnessCommand);
        ApplyBrightnessCapCommand = new AsyncRelayCommand(ApplyBrightnessCapAsync, CanSendBrightnessCommand);

        maskTransport.TransportStateChanged += OnMaskTransportStateChanged;
        textTransport.StateChanged += OnTextTransportStateChanged;
        if (this.autoConnectCoordinator is not null)
        {
            this.autoConnectCoordinator.PropertyChanged += OnAutoConnectPropertyChanged;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<RaveAction> Actions { get; }

    public IReadOnlyList<RaveAction> CommandFallbackActions { get; }

    public IReadOnlyList<BuiltInAssetAction> FavoriteBuiltIns
    {
        get => favoriteBuiltIns;
        private set
        {
            if (SetField(ref favoriteBuiltIns, value))
            {
                OnPropertyChanged(nameof(HasFavoriteBuiltIns));
                OnPropertyChanged(nameof(FavoriteBuiltInsHintText));
            }
        }
    }

    public bool HasFavoriteBuiltIns => FavoriteBuiltIns.Count > 0;

    public IReadOnlyList<TextPresetGroup> PresetGroups
    {
        get => presetGroups;
        private set
        {
            if (SetField(ref presetGroups, value))
            {
                OnPropertyChanged(nameof(HasPresetGroups));
            }
        }
    }

    public bool HasPresetGroups => PresetGroups.Count > 0;

    public string FavoriteBuiltInsHintText => HasFavoriteBuiltIns
        ? "Favorite built-ins are command-only and low-bandwidth."
        : "Use Faces scanner to favorite fast command-only looks.";

    public AsyncRelayCommand BlackoutCommand { get; }

    public AsyncRelayCommand RestoreCommand { get; }

    public AsyncRelayCommand ApplyBrightnessCapCommand { get; }

    public string ModeStatusText => "Manual-first RAVE MVP: offline short captions, low-bandwidth sends, no detector, no microphone, no AI.";

    public string ActiveTransportText
    {
        get
        {
            var commandTransport = maskTransport.IsSimulated
                ? $"{maskTransport.TransportDisplayName} controls (simulated)"
                : $"{maskTransport.TransportDisplayName} controls (real)";
            var uploadTransport = textTransport.IsSimulated
                ? $"{textTransport.TransportDisplayName} text (simulated)"
                : $"{textTransport.TransportDisplayName} text (real)";
            return $"{commandTransport}; {uploadTransport}";
        }
    }

    public string ConnectionStatusText => BuildConnectionStatusText();

    public string AutoConnectStatusText => autoConnectCoordinator?.AutoConnectStatusText ?? "Auto-connect: Off";

    public string QuickCaptionProfileText
    {
        get => quickCaptionProfileText;
        private set => SetField(ref quickCaptionProfileText, value);
    }

    public string ControlsStatusText => "BLACKOUT always available";

    public int BrightnessCap
    {
        get => brightnessCap;
        set => SetField(ref brightnessCap, Math.Clamp(value, 1, 100));
    }

    public bool IsSending
    {
        get => isSending;
        private set
        {
            if (SetField(ref isSending, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string SendStatusText
    {
        get => sendStatusText;
        private set => SetField(ref sendStatusText, value);
    }

    public string LastActionText
    {
        get => lastActionText;
        private set => SetField(ref lastActionText, value);
    }

    public string LastPayloadText
    {
        get => lastPayloadText;
        private set => SetField(ref lastPayloadText, value);
    }

    public async Task InitializeArchiveAsync(CancellationToken cancellationToken = default)
    {
        var archive = await archiveStore.LoadAsync(cancellationToken);
        FavoriteBuiltIns = archive.FavoriteDeckRecords()
            .Select(CreateBuiltInAction)
            .ToArray();
        await InitializePresetsAsync(cancellationToken);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await InitializeArchiveAsync(cancellationToken);
        var settings = (await quickCaptionSettingsStore.LoadAsync(cancellationToken)).Normalize();
        QuickCaptionProfileText = $"Text: {GetSendModeLabel(settings.SendMode)} · {settings.ForegroundPreset.ToString().ToLowerInvariant()}";
        if (autoConnectCoordinator is not null)
        {
            await autoConnectCoordinator.InitializeAsync(cancellationToken);
            await autoConnectCoordinator.StartForegroundAutoConnectAsync(cancellationToken);
        }
    }

    public async Task InitializePresetsAsync(CancellationToken cancellationToken = default)
    {
        var state = await textPresetStore.LoadAsync(cancellationToken);
        var presets = state.Presets
            .Where(preset =>
                preset.Category == TextPresetCategory.CzechRave ||
                (preset.IsFavorite && preset.ShowInRave) ||
                (preset.Category == TextPresetCategory.CzechPoliticalSatire && preset.IsFavorite && preset.ShowInRave))
            .OrderBy(preset => preset.IsFavorite ? 0 : 1)
            .ThenBy(preset => preset.Category == TextPresetCategory.CzechRave ? 0 : 1)
            .ThenBy(preset => preset.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        PresetGroups = presets
            .GroupBy(preset => preset.IsFavorite ? "RAVE favorites" : preset.PackName)
            .Select(group => new TextPresetGroup(
                group.Any(preset => preset.IsFavorite) ? TextPresetCategory.Custom : group.First().Category,
                group.Key,
                group.Select(CreatePresetCard).ToArray()))
            .ToArray();
    }

    private RaveAction CreateAction(QuickActionId actionId, string group)
    {
        var action = catalog.Get(actionId);
        return new RaveAction(
            action.Id,
            action.Label,
            action.Caption ?? action.Label,
            group,
            new AsyncRelayCommand(cancellationToken => SendActionAsync(action.Id, action.Label, cancellationToken), CanSendCaption));
    }

    private RaveAction CreateCommandFallbackAction(QuickActionId actionId)
    {
        var action = catalog.Get(actionId);
        return new RaveAction(
            action.Id,
            action.Label,
            $"Command-only built-in ID {action.BuiltInId}",
            "Fallback",
            new AsyncRelayCommand(cancellationToken => SendCommandFallbackAsync(action.Id, action.Label, cancellationToken), CanSendBrightnessCommand));
    }

    private BuiltInAssetAction CreateBuiltInAction(BuiltInAssetRecord record)
    {
        BuiltInAssetAction? action = null;
        action = new BuiltInAssetAction(
            record,
            record.DisplayName,
            $"{record.Type} {record.HexId}",
            $"{record.Status}; command-only/low-bandwidth.",
            new AsyncRelayCommand(
                cancellationToken => SendBuiltInAsync(record, cancellationToken),
                () => action is not null && CanSendBrightnessCommand()));
        return action;
    }

    private TextPresetCard CreatePresetCard(TextPreset preset)
    {
        TextPresetCard? card = null;
        card = new TextPresetCard(
            preset,
            new AsyncRelayCommand(
                cancellationToken => SendPresetAsync(card!.Preset, cancellationToken),
                () => card is not null && CanSendCaption()));
        return card;
    }

    private async Task SendActionAsync(QuickActionId actionId, string label, CancellationToken cancellationToken)
    {
        if (!CanSendCaption())
        {
            SendStatusText = BuildTextUnavailableStatus();
            return;
        }

        LastActionText = label;
        LastPayloadText = $"Intent {actionId}";

        try
        {
            IsSending = true;
            SendStatusText = "Ready";

            var result = await dispatcher.TriggerAsync(actionId, cancellationToken: cancellationToken);
            SendStatusText = result.Message;
            LastPayloadText = result.Status;
        }
        finally
        {
            IsSending = false;
        }
    }

    private async Task BlackoutAsync(CancellationToken cancellationToken)
    {
        if (BrightnessCap > 1)
        {
            restoreBrightness = BrightnessCap;
        }

        await SendBrightnessAsync(QuickActionId.Blackout, "BLACKOUT", 1, cancellationToken);
    }

    private Task RestoreAsync(CancellationToken cancellationToken) =>
        SendBrightnessAsync(QuickActionId.SetBrightness, "RESTORE", restoreBrightness, cancellationToken);

    private Task ApplyBrightnessCapAsync(CancellationToken cancellationToken)
    {
        var cappedBrightness = Math.Clamp(BrightnessCap, 1, 100);
        if (cappedBrightness > 1)
        {
            restoreBrightness = cappedBrightness;
        }

        return SendBrightnessAsync(QuickActionId.SetBrightness, $"BRIGHTNESS CAP {cappedBrightness}%", cappedBrightness, cancellationToken);
    }

    private async Task SendCommandFallbackAsync(QuickActionId actionId, string label, CancellationToken cancellationToken)
    {
        if (!CanSendBrightnessCommand())
        {
            SendStatusText = "Connect to send";
            return;
        }

        LastActionText = label;
        LastPayloadText = $"Intent {actionId}";

        try
        {
            IsSending = true;
            SendStatusText = "Needs real-mask test";
            var result = await dispatcher.TriggerAsync(actionId, cancellationToken: cancellationToken);
            SendStatusText = result.Message;
            LastPayloadText = result.Status;
        }
        finally
        {
            IsSending = false;
        }
    }

    private async Task SendBuiltInAsync(BuiltInAssetRecord record, CancellationToken cancellationToken)
    {
        if (!CanSendBrightnessCommand())
        {
            SendStatusText = "Connect to send";
            return;
        }

        LastActionText = record.DisplayName;
        LastPayloadText = $"{record.Type} {record.HexId}";

        try
        {
            IsSending = true;
            SendStatusText = "Needs real-mask test";
            var result = await maskTransport.SendAsync(BuiltInAssetCommandFactory.CreateCommand(record), cancellationToken);
            SendStatusText = result.Succeeded
                ? "Sent, confirm on mask"
                : result.Message;
            LastPayloadText = result.Succeeded ? "sent" : "failed";
        }
        finally
        {
            IsSending = false;
        }
    }

    private async Task SendPresetAsync(TextPreset preset, CancellationToken cancellationToken)
    {
        if (!CanSendCaption())
        {
            SendStatusText = BuildTextUnavailableStatus();
            return;
        }

        LastActionText = preset.DisplayName;
        LastPayloadText = preset.MaskText;

        try
        {
            IsSending = true;
            SendStatusText = "Ready";
            var result = await textPresetDispatcher.SendAsync(preset, cancellationToken);
            SendStatusText = result.Message;
            LastPayloadText = result.Status;
        }
        finally
        {
            IsSending = false;
        }
    }

    private async Task SendBrightnessAsync(QuickActionId actionId, string label, int brightness, CancellationToken cancellationToken)
    {
        if (!CanSendBrightnessCommand())
        {
            SendStatusText = "Connect to send";
            return;
        }

        LastActionText = label;
        LastPayloadText = $"Intent {actionId}";

        try
        {
            IsSending = true;
            SendStatusText = "Ready";
            var result = await dispatcher.TriggerAsync(
                actionId,
                actionId == QuickActionId.SetBrightness ? new QuickActionRequest(brightness) : null,
                cancellationToken);
            SendStatusText = result.Message;
            if (result.Succeeded && brightness > 1)
            {
                BrightnessCap = brightness;
                restoreBrightness = brightness;
            }

            LastPayloadText = result.Status;
        }
        finally
        {
            IsSending = false;
        }
    }

    private bool CanSendCaption() => !IsSending && textTransport.IsReady;

    private bool CanSendBrightnessCommand() =>
        !IsSending && maskTransport.TransportState == MaskCommandTransportState.Ready;

    private string BuildTextUnavailableStatus()
    {
        if (!textTransport.IsReady)
        {
            return "Text not ready";
        }

        return "Text not ready";
    }

    private string BuildConnectionStatusText()
    {
        if (maskTransport.TransportState == MaskCommandTransportState.Ready && textTransport.IsReady)
        {
            return "Ready";
        }

        return "Connect to send";
    }

    private void OnMaskTransportStateChanged(object? sender, MaskCommandTransportStateChangedEventArgs e)
    {
        maskStatusText = e.Message;
        OnPropertyChanged(nameof(ConnectionStatusText));
        RaiseCommandStates();
    }

    private void OnTextTransportStateChanged(object? sender, TextUploadTransportStateChangedEventArgs e)
    {
        textStatusText = e.Message;
        OnPropertyChanged(nameof(ConnectionStatusText));
        OnPropertyChanged(nameof(ActiveTransportText));
        RaiseCommandStates();
    }

    private void OnAutoConnectPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        OnPropertyChanged(nameof(AutoConnectStatusText));

    private static string GetSendModeLabel(QuickCaptionSendMode sendMode) =>
        sendMode switch
        {
            QuickCaptionSendMode.LowStaticFlash => "Low-static Flash",
            QuickCaptionSendMode.FastWriteOnly => "Fast Flash unstable",
            QuickCaptionSendMode.ReliableAcknowledgement => "Reliable ACK",
            _ => "Stable Flash"
        };

    private void RaiseCommandStates()
    {
        foreach (var action in Actions)
        {
            action.SendCommand.RaiseCanExecuteChanged();
        }

        foreach (var action in CommandFallbackActions)
        {
            action.SendCommand.RaiseCanExecuteChanged();
        }

        foreach (var action in FavoriteBuiltIns)
        {
            action.SendCommand.RaiseCanExecuteChanged();
        }

        foreach (var card in PresetGroups.SelectMany(group => group.Cards))
        {
            card.SendCommand.RaiseCanExecuteChanged();
        }

        BlackoutCommand.RaiseCanExecuteChanged();
        RestoreCommand.RaiseCanExecuteChanged();
        ApplyBrightnessCapCommand.RaiseCanExecuteChanged();
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
