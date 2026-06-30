using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.React;

public sealed class ReactViewModel : INotifyPropertyChanged
{
    private readonly IQuickActionDispatcher dispatcher;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly IBuiltInAssetArchiveStore archiveStore;
    private readonly ITextPresetStore textPresetStore;
    private readonly ITextPresetDispatcher textPresetDispatcher;
    private readonly List<ReactReactionCard> allCards;
    private readonly IReadOnlyList<ReactReactionGroup> allGroups;
    private IReadOnlyList<ReactReactionGroup> groups;
    private ReactFilterOption selectedFilter;
    private IReadOnlyList<BuiltInAssetAction> favoriteBuiltIns = [];
    private IReadOnlyList<TextPresetGroup> presetGroups = [];
    private string statusText;
    private string lastActionText = "None";
    private bool isSending;

    public ReactViewModel(
        QuickActionCatalog catalog,
        IQuickActionDispatcher dispatcher,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport,
        IBuiltInAssetArchiveStore? archiveStore = null,
        ITextPresetStore? textPresetStore = null,
        ITextPresetDispatcher? textPresetDispatcher = null)
    {
        this.dispatcher = dispatcher;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;
        this.archiveStore = archiveStore ?? new InMemoryBuiltInAssetArchiveStore();
        this.textPresetStore = textPresetStore ?? new InMemoryTextPresetStore();
        this.textPresetDispatcher = textPresetDispatcher ?? new TextPresetDispatcher(textTransport, this.textPresetStore);

        var deck = new ReactDeckCatalog(catalog).Build();
        PinnedCards = deck.PinnedCards.Select(CreateCard).ToArray();
        allGroups = deck.Groups
            .Select(group => new ReactReactionGroup(group.Category, group.Title, group.Cards.Select(CreateCard).ToArray()))
            .ToArray();
        groups = allGroups;
        allCards = PinnedCards.Concat(allGroups.SelectMany(group => group.Cards)).ToList();
        FilterOptions =
        [
            new ReactFilterOption("All", null),
            new ReactFilterOption("Meme", QuickActionCategory.Meme),
            new ReactFilterOption("Social", QuickActionCategory.Social),
            new ReactFilterOption("RAVE", QuickActionCategory.Rave),
            new ReactFilterOption("Welfare", QuickActionCategory.Welfare)
        ];
        selectedFilter = FilterOptions[0];

        statusText = BuildPrimaryReadinessText();
        commandTransport.TransportStateChanged += OnCommandTransportStateChanged;
        textTransport.StateChanged += OnTextTransportStateChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<ReactReactionCard> PinnedCards { get; }

    public IReadOnlyList<ReactReactionGroup> Groups
    {
        get => groups;
        private set => SetField(ref groups, value);
    }

    public IReadOnlyList<ReactFilterOption> FilterOptions { get; }

    public ReactFilterOption SelectedFilter
    {
        get => selectedFilter;
        set
        {
            if (value is null)
            {
                return;
            }

            if (SetField(ref selectedFilter, value))
            {
                ApplyFilter();
            }
        }
    }

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
        ? "Favorite Faces send IMAG/ANIM command IDs only."
        : "Use Faces scanner to favorite fast command-only looks.";

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

    public string ReadinessText => BuildPrimaryReadinessText();

    public string CommandReadinessText => commandTransport.TransportState == MaskCommandTransportState.Ready
        ? "Ready"
        : "Connect to send";

    public string TextReadinessText => textTransport.IsReady
        ? BuildTextReadyText()
        : "Text not ready";

    public async Task InitializeArchiveAsync(CancellationToken cancellationToken = default)
    {
        var archive = await archiveStore.LoadAsync(cancellationToken);
        FavoriteBuiltIns = archive.FavoriteDeckRecords()
            .Select(CreateBuiltInAction)
            .ToArray();
        await InitializePresetsAsync(cancellationToken);
    }

    public async Task InitializePresetsAsync(CancellationToken cancellationToken = default)
    {
        var state = await textPresetStore.LoadAsync(cancellationToken);
        PresetGroups = state.Presets
            .Where(preset => preset.ShowInReact && preset.Category != TextPresetCategory.Legacy)
            .OrderBy(preset => GetPresetGroupSortOrder(preset.Category))
            .GroupBy(preset => preset.Category)
            .Select(group => new TextPresetGroup(
                group.Key,
                GetPresetGroupTitle(group.Key),
                group.OrderBy(preset => preset.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .Select(CreatePresetCard)
                    .ToArray()))
            .ToArray();
    }

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
            StatusText = "Ready";

            var result = await dispatcher.TriggerAsync(card.Id, cancellationToken: cancellationToken);
            StatusText = result.Message;
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

    private BuiltInAssetAction CreateBuiltInAction(BuiltInAssetRecord record)
    {
        BuiltInAssetAction? action = null;
        action = new BuiltInAssetAction(
            record,
            record.DisplayName,
            $"{record.Type} {record.HexId}",
            $"{record.Status}; command-only built-in.",
            new AsyncRelayCommand(
                cancellationToken => SendBuiltInAsync(record, cancellationToken),
                () => action is not null && CanSendBuiltIn()));
        return action;
    }

    private TextPresetCard CreatePresetCard(TextPreset preset)
    {
        TextPresetCard? card = null;
        card = new TextPresetCard(
            preset,
            new AsyncRelayCommand(
                cancellationToken => SendPresetAsync(card!.Preset, cancellationToken),
                () => card is not null && CanSendPreset()));
        return card;
    }

    private void ApplyFilter()
    {
        Groups = SelectedFilter.Category is null
            ? allGroups
            : allGroups.Where(group => group.Category == SelectedFilter.Category).ToArray();
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

    private bool CanSendBuiltIn() =>
        !IsSending && commandTransport.TransportState == MaskCommandTransportState.Ready;

    private bool CanSendPreset() => !IsSending && textTransport.IsReady;

    private string GetUnavailableStatus(ReactReactionCard card) =>
        card.Kind switch
        {
            QuickActionKind.Command => "Connect to send",
            QuickActionKind.BuiltInImage or QuickActionKind.BuiltInAnimation => "Connect to send",
            QuickActionKind.Text or QuickActionKind.Random => "Text not ready",
            _ => "This reaction is not available."
        };

    private async Task SendBuiltInAsync(BuiltInAssetRecord record, CancellationToken cancellationToken)
    {
        if (!CanSendBuiltIn())
        {
            StatusText = "Connect to send";
            return;
        }

        try
        {
            IsSending = true;
            LastActionText = record.DisplayName;
            StatusText = "Needs real-mask test";
            var result = await commandTransport.SendAsync(BuiltInAssetCommandFactory.CreateCommand(record), cancellationToken);
            StatusText = result.Succeeded
                ? "Sent, confirm on mask"
                : result.Message;
        }
        finally
        {
            IsSending = false;
        }
    }

    private async Task SendPresetAsync(TextPreset preset, CancellationToken cancellationToken)
    {
        if (!CanSendPreset())
        {
            StatusText = "Text not ready";
            return;
        }

        try
        {
            IsSending = true;
            LastActionText = preset.DisplayName;
            StatusText = "Ready";
            var result = await textPresetDispatcher.SendAsync(preset, cancellationToken);
            StatusText = result.Message;
        }
        finally
        {
            IsSending = false;
        }
    }

    private string BuildPrimaryReadinessText()
    {
        if (commandTransport.TransportState == MaskCommandTransportState.Ready && textTransport.IsReady)
        {
            return "Ready";
        }

        if (commandTransport.TransportState == MaskCommandTransportState.Ready)
        {
            return "Text not ready";
        }

        if (textTransport.IsReady)
        {
            return "Ready";
        }

        return "Connect to send";
    }

    private string BuildTextReadyText()
    {
        if (!textTransport.SupportsAcknowledgements)
        {
            return "Ready";
        }

        return textTransport.IsSimulated
            ? "Ready"
            : "Ready";
    }

    private void OnCommandTransportStateChanged(object? sender, MaskCommandTransportStateChangedEventArgs e)
    {
        StatusText = BuildPrimaryReadinessText();
        OnPropertyChanged(nameof(ReadinessText));
        OnPropertyChanged(nameof(CommandReadinessText));
        RaiseCardCommandStates();
    }

    private void OnTextTransportStateChanged(object? sender, TextUploadTransportStateChangedEventArgs e)
    {
        StatusText = BuildPrimaryReadinessText();
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

        foreach (var action in FavoriteBuiltIns)
        {
            action.SendCommand.RaiseCanExecuteChanged();
        }

        foreach (var card in PresetGroups.SelectMany(group => group.Cards))
        {
            card.SendCommand.RaiseCanExecuteChanged();
        }
    }

    private static string GetPresetGroupTitle(TextPresetCategory category) =>
        category switch
        {
            TextPresetCategory.CzechBasic => "Czech Basic",
            TextPresetCategory.CzechMeme => "Czech Meme",
            TextPresetCategory.CzechPoliticalSatire => "Czech Political/Satire",
            TextPresetCategory.CzechRave => "Czech RAVE",
            TextPresetCategory.Custom => "Custom",
            _ => category.ToString()
        };

    private static int GetPresetGroupSortOrder(TextPresetCategory category) =>
        category switch
        {
            TextPresetCategory.CzechBasic => 0,
            TextPresetCategory.CzechMeme => 1,
            TextPresetCategory.CzechPoliticalSatire => 2,
            TextPresetCategory.CzechRave => 3,
            TextPresetCategory.Custom => 4,
            _ => 5
        };

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
