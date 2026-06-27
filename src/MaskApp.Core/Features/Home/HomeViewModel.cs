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
    private int brightness = 60;
    private string lastActionStatus = "No Control Room action sent yet.";
    private string currentLookText = "Unknown; send a reaction or brightness command to update.";
    private MaskCommandTransportState commandTransportState;
    private string commandTransportStatusText;
    private TextUploadTransportState textTransportState;
    private string textTransportStatusText;
    private bool textTransportIsReady;
    private bool textTransportSupportsAcknowledgements;

    public HomeViewModel(
        QuickActionCatalog catalog,
        IQuickActionDispatcher dispatcher,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport)
    {
        this.catalog = catalog;
        this.dispatcher = dispatcher;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;

        commandTransportState = commandTransport.TransportState;
        commandTransportStatusText = commandTransport.TransportStatusText;
        textTransportState = textTransport.State;
        textTransportStatusText = textTransport.StatusText;
        textTransportIsReady = textTransport.IsReady;
        textTransportSupportsAcknowledgements = textTransport.SupportsAcknowledgements;

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

    public IReadOnlyList<HomeQuickActionCard> FavoriteReactions { get; }

    public ObservableCollection<HomeQuickActionCard> RecentReactions { get; }

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

    public bool CanUseControlCommands => CommandTransportState == MaskCommandTransportState.Ready;

    public bool CanUseTextReactions => TextTransportIsReady;

    public string RecoveryHint
    {
        get
        {
            if (CanUseControlCommands && CanUseTextReactions)
            {
                return "Mask controls and text reactions are ready.";
            }

            if (!CanUseControlCommands && !CanUseTextReactions)
            {
                return "Open Connect to scan or reconnect; controls and reactions unlock when the mask transport is ready.";
            }

            if (!CanUseControlCommands)
            {
                return "Open Connect to recover control commands; text reactions may still be available.";
            }

            return "Open Text for upload diagnostics if reactions do not appear on the mask.";
        }
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
            ? $"{displayName}: {result.Message}"
            : $"{displayName}: {result.Message}";

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
