using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.React;

public sealed class ReactViewModel : INotifyPropertyChanged
{
    private readonly IQuickActionDispatcher dispatcher;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly List<ReactReactionCard> allCards;
    private string statusText;
    private string lastActionText = "None";
    private bool isSending;

    public ReactViewModel(
        QuickActionCatalog catalog,
        IQuickActionDispatcher dispatcher,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport)
    {
        this.dispatcher = dispatcher;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;

        var deck = new ReactDeckCatalog(catalog).Build();
        PinnedCards = deck.PinnedCards.Select(CreateCard).ToArray();
        Groups = deck.Groups
            .Select(group => new ReactReactionGroup(group.Category, group.Title, group.Cards.Select(CreateCard).ToArray()))
            .ToArray();
        allCards = PinnedCards.Concat(Groups.SelectMany(group => group.Cards)).ToList();

        statusText = BuildReadinessText();
        commandTransport.TransportStateChanged += OnCommandTransportStateChanged;
        textTransport.StateChanged += OnTextTransportStateChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<ReactReactionCard> PinnedCards { get; }

    public IReadOnlyList<ReactReactionGroup> Groups { get; }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string LastActionText
    {
        get => lastActionText;
        private set => SetField(ref lastActionText, value);
    }

    public bool IsSending
    {
        get => isSending;
        private set
        {
            if (SetField(ref isSending, value))
            {
                RaiseCardCommandStates();
            }
        }
    }

    public string ReadinessText => BuildReadinessText();

    public string CommandReadinessText => commandTransport.TransportState == MaskCommandTransportState.Ready
        ? "BLACKOUT ready."
        : $"BLACKOUT unavailable: {commandTransport.TransportStatusText}";

    public string TextReadinessText => textTransport.IsReady
        ? BuildTextReadyText()
        : $"Text reactions unavailable: {textTransport.StatusText}";

    public async Task SendAsync(ReactReactionCard card, CancellationToken cancellationToken = default)
    {
        if (!CanSend(card))
        {
            StatusText = GetUnavailableStatus(card);
            return;
        }

        try
        {
            IsSending = true;
            LastActionText = card.Label;
            StatusText = $"Sending {card.Label}...";

            var result = await dispatcher.TriggerAsync(card.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            StatusText = result.Succeeded
                ? $"Sent {card.Label}. {result.Message}"
                : $"{card.Label} failed: {result.Message}";
        }
        finally
        {
            IsSending = false;
        }
    }

    private ReactReactionCard CreateCard(ReactDeckCard card)
    {
        ReactReactionCard? reactionCard = null;
        reactionCard = new ReactReactionCard(
            card,
            new AsyncRelayCommand(
                cancellationToken => SendAsync(reactionCard!, cancellationToken),
                () => reactionCard is not null && CanSend(reactionCard)));
        return reactionCard;
    }

    private bool CanSend(ReactReactionCard card)
    {
        if (IsSending)
        {
            return false;
        }

        return card.Kind switch
        {
            QuickActionKind.Command => commandTransport.TransportState == MaskCommandTransportState.Ready,
            QuickActionKind.BuiltInImage or QuickActionKind.BuiltInAnimation => commandTransport.TransportState == MaskCommandTransportState.Ready,
            QuickActionKind.Text or QuickActionKind.Random => textTransport.IsReady,
            _ => false
        };
    }

    private string GetUnavailableStatus(ReactReactionCard card) =>
        card.Kind switch
        {
            QuickActionKind.Command => commandTransport.TransportStatusText,
            QuickActionKind.BuiltInImage or QuickActionKind.BuiltInAnimation => commandTransport.TransportStatusText,
            QuickActionKind.Text or QuickActionKind.Random => textTransport.StatusText,
            _ => "This reaction is not available."
        };

    private string BuildReadinessText()
    {
        if (commandTransport.TransportState == MaskCommandTransportState.Ready && textTransport.IsReady)
        {
            return "Ready for BLACKOUT and one-tap reactions.";
        }

        if (commandTransport.TransportState == MaskCommandTransportState.Ready)
        {
            return $"BLACKOUT ready. Text reactions need transport: {textTransport.StatusText}";
        }

        if (textTransport.IsReady)
        {
            return $"Text reactions ready. BLACKOUT needs command transport: {commandTransport.TransportStatusText}";
        }

        return $"Connect first. Command: {commandTransport.TransportStatusText} Text: {textTransport.StatusText}";
    }

    private string BuildTextReadyText()
    {
        if (!textTransport.SupportsAcknowledgements)
        {
            return "Text reactions ready in write-only compatibility mode.";
        }

        return textTransport.IsSimulated
            ? "Text reactions ready on simulated transport."
            : "Text reactions ready with ACK support.";
    }

    private void OnCommandTransportStateChanged(object? sender, MaskCommandTransportStateChangedEventArgs e)
    {
        StatusText = BuildReadinessText();
        OnPropertyChanged(nameof(ReadinessText));
        OnPropertyChanged(nameof(CommandReadinessText));
        RaiseCardCommandStates();
    }

    private void OnTextTransportStateChanged(object? sender, TextUploadTransportStateChangedEventArgs e)
    {
        StatusText = BuildReadinessText();
        OnPropertyChanged(nameof(ReadinessText));
        OnPropertyChanged(nameof(TextReadinessText));
        RaiseCardCommandStates();
    }

    private void RaiseCardCommandStates()
    {
        foreach (var card in allCards)
        {
            card.SendCommand.RaiseCanExecuteChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
}
