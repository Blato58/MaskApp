using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.Rave;

public sealed class RaveViewModel : INotifyPropertyChanged
{
    private readonly IMaskCommandTransport maskTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly QuickActionCatalog catalog;
    private readonly IQuickActionDispatcher dispatcher;
    private int brightnessCap = 65;
    private int restoreBrightness = 65;
    private bool festivalLock;
    private bool isSending;
    private string sendStatusText = "Ready for manual offline captions.";
    private string maskStatusText;
    private string textStatusText;
    private string lastActionText = "None";
    private string lastPayloadText = "None";

    public RaveViewModel(
        QuickActionCatalog catalog,
        IQuickActionDispatcher dispatcher,
        IMaskCommandTransport maskTransport,
        ITextUploadTransport textTransport)
    {
        this.catalog = catalog;
        this.dispatcher = dispatcher;
        this.maskTransport = maskTransport;
        this.textTransport = textTransport;
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

        BlackoutCommand = new AsyncRelayCommand(BlackoutAsync, CanSendBrightnessCommand);
        RestoreCommand = new AsyncRelayCommand(RestoreAsync, CanSendBrightnessCommand);
        ApplyBrightnessCapCommand = new AsyncRelayCommand(ApplyBrightnessCapAsync, CanSendBrightnessCommand);

        maskTransport.TransportStateChanged += OnMaskTransportStateChanged;
        textTransport.StateChanged += OnTextTransportStateChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<RaveAction> Actions { get; }

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

    public string ConnectionStatusText => $"Control: {maskStatusText} Text: {textStatusText}";

    public int BrightnessCap
    {
        get => brightnessCap;
        set => SetField(ref brightnessCap, Math.Clamp(value, 1, 100));
    }

    public bool FestivalLock
    {
        get => festivalLock;
        set
        {
            if (SetField(ref festivalLock, value))
            {
                OnPropertyChanged(nameof(ShowSecondaryControls));
                OnPropertyChanged(nameof(FestivalLockStatusText));
            }
        }
    }

    public bool ShowSecondaryControls => !FestivalLock;

    public string FestivalLockStatusText => FestivalLock
        ? "Festival Lock: big buttons and BLACKOUT stay visible."
        : "Festival Lock off: brightness and diagnostics are visible.";

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
            SendStatusText = textTransport.SupportsAcknowledgements
                ? $"Sending {label} with ACK confirmation..."
                : $"Sending {label} without ACK confirmation...";

            var result = await dispatcher.TriggerAsync(actionId, cancellationToken: cancellationToken).ConfigureAwait(false);
            SendStatusText = result.Succeeded && !textTransport.SupportsAcknowledgements
                ? $"{result.Message} Sent without ACK confirmation; confirm on mask."
                : result.Message;
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

        await SendBrightnessAsync(QuickActionId.Blackout, "BLACKOUT", 1, cancellationToken).ConfigureAwait(false);
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

    private async Task SendBrightnessAsync(QuickActionId actionId, string label, int brightness, CancellationToken cancellationToken)
    {
        if (!CanSendBrightnessCommand())
        {
            SendStatusText = "Mask brightness controls are not ready.";
            return;
        }

        LastActionText = label;
        LastPayloadText = $"Intent {actionId}";

        try
        {
            IsSending = true;
            SendStatusText = $"Sending {label}...";
            var result = await dispatcher.TriggerAsync(
                actionId,
                actionId == QuickActionId.SetBrightness ? new QuickActionRequest(brightness) : null,
                cancellationToken).ConfigureAwait(false);
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
            return textTransport.StatusText;
        }

        return "Text upload is not ready.";
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

    private void RaiseCommandStates()
    {
        foreach (var action in Actions)
        {
            action.SendCommand.RaiseCanExecuteChanged();
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
