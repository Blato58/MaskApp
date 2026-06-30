using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryViewModel : INotifyPropertyChanged
{
    private readonly GalleryCatalogBuilder catalogBuilder;
    private readonly ITextPresetStore textPresetStore;
    private readonly IBuiltInAssetArchiveStore builtInArchiveStore;
    private readonly IGalleryLayoutStore layoutStore;
    private readonly IQuickActionDispatcher quickActionDispatcher;
    private readonly ITextPresetDispatcher textPresetDispatcher;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;
    private GalleryLayoutState layoutState = new();
    private IReadOnlyList<GalleryItem> allItems = [];
    private IReadOnlyList<GalleryGroupCard> groups = [];
    private string searchText = string.Empty;
    private bool showFavoritesOnly;
    private GalleryGroupingOption selectedGroupingMode;
    private string statusText = "Ready";
    private string lastActionText = "None";
    private bool isSending;
    private bool isAddOptionsVisible;

    public GalleryViewModel(
        QuickActionCatalog quickActionCatalog,
        ITextPresetStore textPresetStore,
        IBuiltInAssetArchiveStore builtInArchiveStore,
        IGalleryLayoutStore layoutStore,
        IQuickActionDispatcher quickActionDispatcher,
        ITextPresetDispatcher textPresetDispatcher,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport)
    {
        catalogBuilder = new GalleryCatalogBuilder(quickActionCatalog);
        this.textPresetStore = textPresetStore;
        this.builtInArchiveStore = builtInArchiveStore;
        this.layoutStore = layoutStore;
        this.quickActionDispatcher = quickActionDispatcher;
        this.textPresetDispatcher = textPresetDispatcher;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;
        GroupingOptions =
        [
            new GalleryGroupingOption("Manual groups", GalleryGroupingMode.Manual),
            new GalleryGroupingOption("Favorites first", GalleryGroupingMode.FavoritesFirst),
            new GalleryGroupingOption("Type", GalleryGroupingMode.Type),
            new GalleryGroupingOption("Group / pack", GalleryGroupingMode.Group),
            new GalleryGroupingOption("Recently sent", GalleryGroupingMode.RecentlySent)
        ];
        selectedGroupingMode = GroupingOptions[0];
        AddOptions =
        [
            new GalleryAddOption(GalleryAddOptionKind.NewTextPreset, "New text preset", "Create a caption in Text Composer.", "txt", "#52E3FF", true),
            new GalleryAddOption(GalleryAddOptionKind.EditTextPresets, "Manage text presets", "Open the saved text preset editor.", "txt", "#A78BFA", true),
            new GalleryAddOption(GalleryAddOptionKind.ScanBuiltInStaticFace, "Scan built-in face", "Send and mark IMAG IDs in the hidden scanner.", "face", "#52E3FF", true),
            new GalleryAddOption(GalleryAddOptionKind.ScanBuiltInAnimation, "Scan built-in animation", "Send and mark ANIM IDs in the hidden scanner.", "anim", "#FF3D8B", true),
            new GalleryAddOption(GalleryAddOptionKind.ImportCustomImage, "Import custom image", "Future/Labs until image upload is implemented.", "face", "#475569", false),
            new GalleryAddOption(GalleryAddOptionKind.ImportCustomAnimation, "Import animation", "Future/Labs until DIY playback is verified.", "anim", "#475569", false),
            new GalleryAddOption(GalleryAddOptionKind.ImportMaskPack, "Import MaskPack", "Manifest support exists; playback remains future work.", "pack", "#475569", false)
        ];
        ToggleAddOptionsCommand = new AsyncRelayCommand(_ =>
        {
            IsAddOptionsVisible = !IsAddOptionsVisible;
            return Task.CompletedTask;
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<GalleryGroupingOption> GroupingOptions { get; }

    public IReadOnlyList<GalleryAddOption> AddOptions { get; }

    public AsyncRelayCommand ToggleAddOptionsCommand { get; }

    public IReadOnlyList<GalleryGroupCard> Groups
    {
        get => groups;
        private set
        {
            if (SetField(ref groups, value))
            {
                OnPropertyChanged(nameof(VisibleItemCountText));
            }
        }
    }

    public string SearchText
    {
        get => searchText;
        set
        {
            if (SetField(ref searchText, value ?? string.Empty))
            {
                RebuildGroups();
            }
        }
    }

    public bool ShowFavoritesOnly
    {
        get => showFavoritesOnly;
        set
        {
            if (SetField(ref showFavoritesOnly, value))
            {
                RebuildGroups();
            }
        }
    }

    public GalleryGroupingOption SelectedGroupingMode
    {
        get => selectedGroupingMode;
        set
        {
            if (value is not null && SetField(ref selectedGroupingMode, value))
            {
                RebuildGroups();
            }
        }
    }

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
        private set => SetField(ref isSending, value);
    }

    public bool IsAddOptionsVisible
    {
        get => isAddOptionsVisible;
        private set => SetField(ref isAddOptionsVisible, value);
    }

    public string VisibleItemCountText => $"{Groups.Sum(group => group.Items.Count)} items";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        layoutState = (await layoutStore.LoadAsync(cancellationToken)).Normalize();
        var textState = await textPresetStore.LoadAsync(cancellationToken);
        var builtIns = await builtInArchiveStore.LoadAsync(cancellationToken);
        allItems = catalogBuilder.Build(textState, builtIns, layoutState.Order);
        RebuildGroups();
    }

    public async Task SendAsync(GalleryItem item, CancellationToken cancellationToken = default)
    {
        if (!item.CanSend)
        {
            StatusText = "Not implemented yet";
            return;
        }

        try
        {
            IsSending = true;
            LastActionText = item.Title;
            StatusText = "Sending...";

            var result = item.Type switch
            {
                GalleryItemType.TextPreset when item.TextPreset is not null =>
                    await SendTextPresetAsync(item.TextPreset, cancellationToken),
                GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation when item.BuiltInAssetRecord is not null =>
                    await SendBuiltInAsync(item.BuiltInAssetRecord, cancellationToken),
                GalleryItemType.QuickAction when item.QuickActionId.HasValue =>
                    await SendQuickActionAsync(item.QuickActionId.Value, cancellationToken),
                _ => "Not implemented yet"
            };

            StatusText = result;
        }
        finally
        {
            IsSending = false;
        }
    }

    public async Task MoveItemAsync(string itemId, int delta, CancellationToken cancellationToken = default)
    {
        var visibleItems = Groups.SelectMany(group => group.Items).Select(card => card.Item).ToArray();
        var currentIndex = Array.FindIndex(visibleItems, item => item.Id == itemId);
        var targetIndex = currentIndex + delta;
        if (currentIndex < 0 || targetIndex < 0 || targetIndex >= visibleItems.Length)
        {
            return;
        }

        var first = visibleItems[currentIndex];
        var second = visibleItems[targetIndex];
        var firstSort = layoutState.Order.GetItemSortIndex(first.Id, first.SortIndex);
        var secondSort = layoutState.Order.GetItemSortIndex(second.Id, second.SortIndex);
        layoutState = layoutState with
        {
            Order = layoutState.Order
                .WithItemSortIndex(first.Id, secondSort)
                .WithItemSortIndex(second.Id, firstSort)
        };
        await layoutStore.SaveAsync(layoutState, cancellationToken);
        await InitializeAsync(cancellationToken);
        StatusText = "Order saved";
    }

    public async Task MoveGroupAsync(string groupKey, int delta, CancellationToken cancellationToken = default)
    {
        var currentIndex = Groups.ToList().FindIndex(group => group.Key == groupKey);
        var targetIndex = currentIndex + delta;
        if (currentIndex < 0 || targetIndex < 0 || targetIndex >= Groups.Count)
        {
            return;
        }

        var first = Groups[currentIndex];
        var second = Groups[targetIndex];
        var firstSort = layoutState.Order.GetGroupSortIndex(first.Key, currentIndex);
        var secondSort = layoutState.Order.GetGroupSortIndex(second.Key, targetIndex);
        layoutState = layoutState with
        {
            Order = layoutState.Order
                .WithGroupSortIndex(first.Key, secondSort)
                .WithGroupSortIndex(second.Key, firstSort)
        };
        await layoutStore.SaveAsync(layoutState, cancellationToken);
        RebuildGroups();
        StatusText = "Group order saved";
    }

    private void RebuildGroups()
    {
        var query = SearchText.Trim().ToUpperInvariant();
        var visible = allItems
            .Where(item => !ShowFavoritesOnly || item.IsFavorite)
            .Where(item => query.Length == 0 || item.SearchText.Contains(query, StringComparison.Ordinal))
            .OrderBy(item => layoutState.Order.GetItemSortIndex(item.Id, item.SortIndex))
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Groups = visible
            .GroupBy(item => GetGroupKey(item, SelectedGroupingMode.Mode))
            .Select((group, index) => new
            {
                Key = group.Key,
                Title = GetGroupTitle(group.Key, SelectedGroupingMode.Mode),
                SortIndex = layoutState.Order.GetGroupSortIndex($"{SelectedGroupingMode.Mode}:{group.Key}", index),
                Items = group.ToArray()
            })
            .OrderBy(group => group.SortIndex)
            .ThenBy(group => group.Title, StringComparer.OrdinalIgnoreCase)
            .Select(group => new GalleryGroupCard(
                $"{SelectedGroupingMode.Mode}:{group.Key}",
                group.Title,
                group.Items.Select(CreateCard).ToArray(),
                new AsyncRelayCommand(cancellationToken => MoveGroupAsync($"{SelectedGroupingMode.Mode}:{group.Key}", -1, cancellationToken)),
                new AsyncRelayCommand(cancellationToken => MoveGroupAsync($"{SelectedGroupingMode.Mode}:{group.Key}", 1, cancellationToken))))
            .ToArray();
    }

    private GalleryItemCard CreateCard(GalleryItem item) =>
        new(
            item,
            new AsyncRelayCommand(cancellationToken => SendAsync(item, cancellationToken), () => item.CanSend && CanSend(item)),
            new AsyncRelayCommand(cancellationToken => MoveItemAsync(item.Id, -1, cancellationToken)),
            new AsyncRelayCommand(cancellationToken => MoveItemAsync(item.Id, 1, cancellationToken)));

    private bool CanSend(GalleryItem item) =>
        item.Type switch
        {
            GalleryItemType.TextPreset => textTransport.IsReady,
            GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation
                => commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.QuickAction => item.QuickActionKind switch
            {
                QuickActionKind.Text or QuickActionKind.Random => textTransport.IsReady,
                QuickActionKind.Command or QuickActionKind.Brightness or QuickActionKind.BuiltInImage or QuickActionKind.BuiltInAnimation =>
                    commandTransport.TransportState == MaskCommandTransportState.Ready,
                _ => false
            },
            _ => false
        };

    private async Task<string> SendTextPresetAsync(TextPreset preset, CancellationToken cancellationToken)
    {
        if (!textTransport.IsReady)
        {
            return "Text not ready";
        }

        var result = await textPresetDispatcher.SendAsync(preset, cancellationToken);
        return result.Message;
    }

    private async Task<string> SendBuiltInAsync(BuiltInAssetRecord record, CancellationToken cancellationToken)
    {
        if (commandTransport.TransportState != MaskCommandTransportState.Ready)
        {
            return "Connect to send";
        }

        var result = await commandTransport.SendAsync(BuiltInAssetCommandFactory.CreateCommand(record), cancellationToken);
        return result.Succeeded ? "Sent, confirm on mask" : result.Message;
    }

    private async Task<string> SendQuickActionAsync(QuickActionId actionId, CancellationToken cancellationToken)
    {
        var result = await quickActionDispatcher.TriggerAsync(actionId, cancellationToken: cancellationToken);
        return result.Message;
    }

    private static string GetGroupKey(GalleryItem item, GalleryGroupingMode mode) =>
        mode switch
        {
            GalleryGroupingMode.FavoritesFirst => item.IsFavorite ? "favorites" : "others",
            GalleryGroupingMode.Type => item.TypeLabel,
            GalleryGroupingMode.Group => item.GroupName,
            GalleryGroupingMode.RecentlySent => item.LastSentAt is null ? "not-sent" : "recent",
            _ => item.GroupName
        };

    private static string GetGroupTitle(string groupKey, GalleryGroupingMode mode) =>
        mode switch
        {
            GalleryGroupingMode.FavoritesFirst => groupKey == "favorites" ? "Favorites" : "Everything else",
            GalleryGroupingMode.RecentlySent => groupKey == "recent" ? "Recently sent" : "Not sent yet",
            _ => groupKey
        };

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
