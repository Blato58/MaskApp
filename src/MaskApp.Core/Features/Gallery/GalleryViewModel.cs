using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;
using MaskApp.Core.Features.Experience;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryViewModel : INotifyPropertyChanged
{
    private readonly GalleryCatalogBuilder catalogBuilder;
    private readonly ITextPresetStore textPresetStore;
    private readonly IBuiltInAssetArchiveStore builtInArchiveStore;
    private readonly IFacePatternStore facePatternStore;
    private readonly IGalleryLayoutStore layoutStore;
    private readonly IQuickActionDispatcher quickActionDispatcher;
    private readonly ITextPresetDispatcher textPresetDispatcher;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly IFaceUploadTransport faceTransport;
    private readonly DiySlotPlaybackCoordinator diySlotPlayback;
    private readonly IAnimationProjectStore animationProjectStore;
    private readonly ISceneShowStore sceneShowStore;
    private readonly SceneExecutionEngine? sceneEngine;
    private readonly IBleDeviceConnection? deviceConnection;
    private readonly ContentCatalogQuery? contentCatalogQuery;
    private readonly HashSet<string> selectedItemIds = new(StringComparer.Ordinal);
    private GalleryLayoutState layoutState = new();
    private FacePatternStoreState faceState = new();
    private IReadOnlyList<GalleryItem> allItems = [];
    private IReadOnlyList<GalleryGroupCard> groups = [];
    private IReadOnlyList<GalleryListRow> rows = [];
    private IReadOnlyList<GallerySelectionAction> selectionActions = [];
    private string searchText = string.Empty;
    private bool showFavoritesOnly;
    private GalleryGroupingOption selectedGroupingMode;
    private string statusText = "Ready";
    private string lastActionText = "None";
    private bool isSending;
    private bool isAddOptionsVisible;
    private bool isFilterSheetVisible;
    private bool isGroupSheetVisible;
    private bool isManageSheetVisible;
    private GalleryItem? managedItem;
    private bool isEditMode;
    private int firstVisibleRowIndex = -1;
    private int lastVisibleRowIndex = -1;
    private bool reducePreviewMotion = true;
    private GalleryTypeFilter selectedTypeFilter;
    private IReadOnlyList<GalleryItemCard> quickDeckItems = [];
    private bool isLoading;
    private string loadErrorText = string.Empty;
    private BleConnectionState connectionState = BleConnectionState.Disconnected;

    public GalleryViewModel(
        QuickActionCatalog quickActionCatalog,
        ITextPresetStore textPresetStore,
        IBuiltInAssetArchiveStore builtInArchiveStore,
        IFacePatternStore facePatternStore,
        IGalleryLayoutStore layoutStore,
        IQuickActionDispatcher quickActionDispatcher,
        ITextPresetDispatcher textPresetDispatcher,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport,
        IFaceUploadTransport faceTransport,
        DiySlotPlaybackCoordinator diySlotPlayback,
        IAnimationProjectStore? animationProjectStore = null,
        ISceneShowStore? sceneShowStore = null,
        SceneExecutionEngine? sceneEngine = null,
        IBleDeviceConnection? deviceConnection = null,
        ContentCatalogQuery? contentCatalogQuery = null)
    {
        catalogBuilder = new GalleryCatalogBuilder(quickActionCatalog);
        this.textPresetStore = textPresetStore;
        this.builtInArchiveStore = builtInArchiveStore;
        this.facePatternStore = facePatternStore;
        this.layoutStore = layoutStore;
        this.quickActionDispatcher = quickActionDispatcher;
        this.textPresetDispatcher = textPresetDispatcher;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;
        this.faceTransport = faceTransport;
        this.diySlotPlayback = diySlotPlayback;
        this.animationProjectStore = animationProjectStore ?? new InMemoryAnimationProjectStore();
        this.sceneShowStore = sceneShowStore ?? new InMemorySceneShowStore();
        this.sceneEngine = sceneEngine;
        this.deviceConnection = deviceConnection;
        this.contentCatalogQuery = contentCatalogQuery;
        connectionState = deviceConnection?.State ?? BleConnectionState.Disconnected;
        if (deviceConnection is not null)
        {
            deviceConnection.ConnectionStateChanged += OnConnectionStateChanged;
        }
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
            new GalleryAddOption(GalleryAddOptionKind.ImportCustomImage, "Create custom face", "Draw, import, save, upload, and play a native 46x58 DIY face.", "face", "#FACC15", true),
            new GalleryAddOption(GalleryAddOptionKind.ImportCustomAnimation, "Animation Studio", "Draw or import GIF/video frames, then validate, save, prepare, and preview.", "anim", "#FF3D8B", true),
            new GalleryAddOption(GalleryAddOptionKind.NewScene, "Scene Studio", "Build bounded typed cues and ordered setlists for Pages and Stage.", "lucide:clapperboard", "#A78BFA", true),
            new GalleryAddOption(GalleryAddOptionKind.ImportMaskPack, "Import or export MaskPack", "Inspect, resolve conflicts, import, or share a complete offline show package.", "pack", "#22C55E", true)
        ];
        ToggleAddOptionsCommand = new AsyncRelayCommand(_ =>
        {
            IsAddOptionsVisible = !IsAddOptionsVisible;
            if (IsAddOptionsVisible)
            {
                IsFilterSheetVisible = false;
                IsGroupSheetVisible = false;
                IsManageSheetVisible = false;
            }

            return Task.CompletedTask;
        });
        ToggleFilterSheetCommand = new AsyncRelayCommand(_ =>
        {
            IsFilterSheetVisible = !IsFilterSheetVisible;
            if (IsFilterSheetVisible)
            {
                IsAddOptionsVisible = false;
                IsGroupSheetVisible = false;
                IsManageSheetVisible = false;
            }

            return Task.CompletedTask;
        });
        ToggleGroupSheetCommand = new AsyncRelayCommand(_ =>
        {
            IsGroupSheetVisible = !IsGroupSheetVisible;
            if (IsGroupSheetVisible)
            {
                IsAddOptionsVisible = false;
                IsFilterSheetVisible = false;
                IsManageSheetVisible = false;
            }

            return Task.CompletedTask;
        });
        SetBrowseModeCommand = new AsyncRelayCommand(_ =>
        {
            IsEditMode = false;
            ClearSelection();
            return Task.CompletedTask;
        });
        SetArrangeModeCommand = new AsyncRelayCommand(_ =>
        {
            IsEditMode = true;
            return Task.CompletedTask;
        });
        ShowAllItemsCommand = new AsyncRelayCommand(_ =>
        {
            ShowFavoritesOnly = false;
            IsFilterSheetVisible = false;
            return Task.CompletedTask;
        });
        ShowFavoritesCommand = new AsyncRelayCommand(_ =>
        {
            ShowFavoritesOnly = true;
            IsFilterSheetVisible = false;
            return Task.CompletedTask;
        });
        CloseManageSheetCommand = new AsyncRelayCommand(_ =>
        {
            IsManageSheetVisible = false;
            ManagedItem = null;
            return Task.CompletedTask;
        });
        ClearSelectionCommand = new AsyncRelayCommand(_ =>
        {
            ClearSelection();
            return Task.CompletedTask;
        });
        DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedAsync, () => SelectedDeletableCount > 0);
        ShowAllTypesCommand = CreateTypeFilterCommand(GalleryTypeFilter.All);
        ShowFacesCommand = CreateTypeFilterCommand(GalleryTypeFilter.Faces);
        ShowTextCommand = CreateTypeFilterCommand(GalleryTypeFilter.Text);
        ShowAnimationsCommand = CreateTypeFilterCommand(GalleryTypeFilter.Animations);
        ShowScenesCommand = CreateTypeFilterCommand(GalleryTypeFilter.Scenes);
        RetryCommand = new AsyncRelayCommand(InitializeAsync);
        SelectionActions =
        [
            new GallerySelectionAction(
                "Delete selected text",
                "Deletes selected text presets and removes their page shortcuts.",
                DeleteSelectedCommand,
                isDestructive: true),
            new GallerySelectionAction(
                "Clear selection",
                "Keeps all Library items and exits the current selection.",
                ClearSelectionCommand)
        ];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<GalleryGroupingOption> GroupingOptions { get; }

    public IReadOnlyList<GalleryAddOption> AddOptions { get; }

    public AsyncRelayCommand ToggleAddOptionsCommand { get; }

    public AsyncRelayCommand ToggleFilterSheetCommand { get; }

    public AsyncRelayCommand ToggleGroupSheetCommand { get; }

    public AsyncRelayCommand SetBrowseModeCommand { get; }

    public AsyncRelayCommand SetArrangeModeCommand { get; }

    public AsyncRelayCommand ShowAllItemsCommand { get; }

    public AsyncRelayCommand ShowFavoritesCommand { get; }

    public AsyncRelayCommand CloseManageSheetCommand { get; }

    public AsyncRelayCommand ClearSelectionCommand { get; }

    public AsyncRelayCommand DeleteSelectedCommand { get; }

    public AsyncRelayCommand ShowAllTypesCommand { get; }

    public AsyncRelayCommand ShowFacesCommand { get; }

    public AsyncRelayCommand ShowTextCommand { get; }

    public AsyncRelayCommand ShowAnimationsCommand { get; }

    public AsyncRelayCommand ShowScenesCommand { get; }

    public AsyncRelayCommand RetryCommand { get; }

    public IReadOnlyList<GallerySelectionAction> SelectionActions
    {
        get => selectionActions;
        private set => selectionActions = value;
    }

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

    public IReadOnlyList<GalleryListRow> Rows
    {
        get => rows;
        private set
        {
            if (SetField(ref rows, value))
            {
                OnPropertyChanged(nameof(VisibleItemCountText));
            }
        }
    }

    public IReadOnlyList<GalleryItemCard> QuickDeckItems
    {
        get => quickDeckItems;
        private set
        {
            if (SetField(ref quickDeckItems, value))
            {
                OnPropertyChanged(nameof(HasQuickDeckItems));
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
                OnPropertyChanged(nameof(FilterSummaryText));
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
                OnPropertyChanged(nameof(GroupSummaryText));
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

    public bool IsLoading
    {
        get => isLoading;
        private set
        {
            if (SetField(ref isLoading, value))
            {
                OnPropertyChanged(nameof(IsContentVisible));
                OnPropertyChanged(nameof(IsEmptyStateVisible));
            }
        }
    }

    public string LoadErrorText
    {
        get => loadErrorText;
        private set
        {
            if (SetField(ref loadErrorText, value))
            {
                OnPropertyChanged(nameof(HasLoadError));
                OnPropertyChanged(nameof(IsContentVisible));
                OnPropertyChanged(nameof(IsEmptyStateVisible));
                OnPropertyChanged(nameof(EmptyStateTitle));
                OnPropertyChanged(nameof(EmptyStateMessage));
            }
        }
    }

    public BleConnectionState ConnectionState
    {
        get => connectionState;
        private set
        {
            if (SetField(ref connectionState, value))
            {
                OnPropertyChanged(nameof(IsDisconnected));
                OnPropertyChanged(nameof(ConnectionStatusText));
                OnPropertyChanged(nameof(ConnectionDetailText));
                RebuildGroups();
            }
        }
    }

    public GalleryTypeFilter SelectedTypeFilter
    {
        get => selectedTypeFilter;
        private set
        {
            if (SetField(ref selectedTypeFilter, value))
            {
                OnPropertyChanged(nameof(IsAllFilterSelected));
                OnPropertyChanged(nameof(IsFacesFilterSelected));
                OnPropertyChanged(nameof(IsTextFilterSelected));
                OnPropertyChanged(nameof(IsAnimationsFilterSelected));
                OnPropertyChanged(nameof(IsScenesFilterSelected));
                RebuildGroups();
            }
        }
    }

    public bool IsAddOptionsVisible
    {
        get => isAddOptionsVisible;
        private set => SetField(ref isAddOptionsVisible, value);
    }

    public bool IsFilterSheetVisible
    {
        get => isFilterSheetVisible;
        private set => SetField(ref isFilterSheetVisible, value);
    }

    public bool IsGroupSheetVisible
    {
        get => isGroupSheetVisible;
        private set => SetField(ref isGroupSheetVisible, value);
    }

    public bool IsManageSheetVisible
    {
        get => isManageSheetVisible;
        private set => SetField(ref isManageSheetVisible, value);
    }

    public GalleryItem? ManagedItem
    {
        get => managedItem;
        private set
        {
            if (SetField(ref managedItem, value))
            {
                OnPropertyChanged(nameof(ManagedItemTitle));
                OnPropertyChanged(nameof(ManagedItemSubtitle));
                OnPropertyChanged(nameof(ManagedItemTypeText));
                OnPropertyChanged(nameof(ManagedItemStatusText));
                OnPropertyChanged(nameof(ManagedItemCanOpenEditor));
                OnPropertyChanged(nameof(ManagedItemEditorLabel));
            }
        }
    }

    public bool IsEditMode
    {
        get => isEditMode;
        set
        {
            if (SetField(ref isEditMode, value))
            {
                RebuildGroups();
                OnPropertyChanged(nameof(GalleryModeText));
                OnPropertyChanged(nameof(IsBrowseMode));
                OnPropertyChanged(nameof(IsArrangeMode));
            }
        }
    }

    public bool IsBrowseMode => !IsEditMode;

    public bool IsArrangeMode => IsEditMode;

    public string GalleryModeText => IsEditMode ? "Arrange" : "Browse";

    public bool IsAllFilterSelected => SelectedTypeFilter == GalleryTypeFilter.All;

    public bool IsFacesFilterSelected => SelectedTypeFilter == GalleryTypeFilter.Faces;

    public bool IsTextFilterSelected => SelectedTypeFilter == GalleryTypeFilter.Text;

    public bool IsAnimationsFilterSelected => SelectedTypeFilter == GalleryTypeFilter.Animations;

    public bool IsScenesFilterSelected => SelectedTypeFilter == GalleryTypeFilter.Scenes;

    public bool HasQuickDeckItems => QuickDeckItems.Count > 0;

    public bool HasVisibleItems => Groups.Any(group => group.Items.Count > 0);

    public bool HasLoadError => !string.IsNullOrWhiteSpace(LoadErrorText);

    public bool IsContentVisible => !IsLoading && !HasLoadError;

    public bool IsEmptyStateVisible => !IsLoading && (HasLoadError || !HasVisibleItems);

    public bool IsDisconnected => ConnectionState != BleConnectionState.Connected;

    public string ConnectionStatusText => ConnectionState switch
    {
        BleConnectionState.Connected => "Connected",
        BleConnectionState.Scanning => "Scanning",
        BleConnectionState.Connecting => "Connecting",
        BleConnectionState.Failed => "Connection failed",
        BleConnectionState.Unavailable => "Bluetooth unavailable",
        _ => "Disconnected"
    };

    public string ConnectionDetailText => IsDisconnected
        ? "Browse and edit offline. Connect from Device to send."
        : "Send controls use the active mask transport.";

    public string EmptyStateTitle => HasLoadError
        ? "Library unavailable"
        : SearchText.Length > 0
            ? "No matching items"
            : "Nothing in this view";

    public string EmptyStateMessage => HasLoadError
        ? LoadErrorText
        : SearchText.Length > 0
            ? "Try a different search or content filter."
            : "Add content or choose another content type.";

    public string VisibleItemCountText => $"{Groups.Sum(group => group.Items.Count)} items";

    public int SelectedCount => selectedItemIds.Count;

    public int SelectedDeletableCount =>
        allItems.Count(item => selectedItemIds.Contains(item.Id) && item.Type == GalleryItemType.TextPreset);

    public bool HasSelection => SelectedCount > 0;

    public string SelectedCountText => SelectedCount == 0
        ? "No items selected"
        : $"{SelectedCount} selected / {SelectedDeletableCount} deletable text";

    public string FilterSummaryText => ShowFavoritesOnly ? "Favorites only" : "All implemented items";

    public string GroupSummaryText => SelectedGroupingMode.Label;

    public string ManagedItemTitle => ManagedItem?.Title ?? "No item selected";

    public string ManagedItemSubtitle => ManagedItem?.Subtitle ?? "Select an item to manage.";

    public string ManagedItemTypeText => ManagedItem is null
        ? "None"
        : $"{ManagedItem.TypeLabel} / {ManagedItem.GroupName}";

    public string ManagedItemStatusText => ManagedItem is null
        ? "No action selected."
        : !string.IsNullOrWhiteSpace(ManagedItem.LastSendStatus)
            ? ManagedItem.LastSendStatus
        : ManagedItem.CanSend
            ? "Implemented and sendable when the device transport is ready."
            : "Labs placeholder; not sendable yet.";

    public bool ManagedItemCanOpenEditor => ManagedItem?.CanManage ?? false;

    public string ManagedItemEditorLabel => ManagedItem?.ManageTarget switch
    {
        AppRoutes.TextStudio => "Open Text Studio",
        AppRoutes.FaceStudio => "Open Face Studio",
        AppRoutes.StockCatalog => "Open Stock Catalog",
        AppRoutes.AnimationStudio => "Open Animation Studio",
        AppRoutes.SceneEditor => "Open Scene Editor",
        _ => "Editor unavailable"
    };

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            LoadErrorText = string.Empty;
            ConnectionState = deviceConnection?.State ?? ConnectionState;
            if (contentCatalogQuery is not null)
            {
                var snapshot = await contentCatalogQuery.LoadAsync(cancellationToken);
                layoutState = snapshot.Layout;
                faceState = snapshot.Faces;
                allItems = snapshot.Items;
            }
            else
            {
                layoutState = (await layoutStore.LoadAsync(cancellationToken)).Normalize();
                var textState = await textPresetStore.LoadAsync(cancellationToken);
                var builtIns = await builtInArchiveStore.LoadAsync(cancellationToken);
                faceState = await facePatternStore.LoadAsync(cancellationToken);
                var animationState = await animationProjectStore.LoadAsync(cancellationToken);
                var sceneState = await sceneShowStore.LoadAsync(cancellationToken);
                allItems = catalogBuilder.Build(textState, builtIns, faceState, layoutState.Order, animationState, sceneState);
            }
            RebuildGroups();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            allItems = [];
            Groups = [];
            Rows = [];
            QuickDeckItems = [];
            LoadErrorText = "Saved content could not be loaded. Your source files were not overwritten.";
            StatusText = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
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
            await diySlotPlayback.StopAnimationAsync(cancellationToken);

            var result = item.Type switch
            {
                GalleryItemType.TextPreset when item.TextPreset is not null =>
                    await SendTextPresetAsync(item.TextPreset, cancellationToken),
                GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation when item.BuiltInAssetRecord is not null =>
                    await SendBuiltInAsync(item.BuiltInAssetRecord, cancellationToken),
                GalleryItemType.CustomStaticFace when item.FacePattern is not null =>
                    await SendFaceAsync(item.FacePattern, cancellationToken),
                GalleryItemType.AppBuiltInAnimation when item.AppAnimation is not null =>
                    await SendAppAnimationAsync(item.AppAnimation, cancellationToken),
                GalleryItemType.CustomAnimation when item.PerformanceAnimation is not null =>
                    await SendPerformanceAnimationAsync(item.PerformanceAnimation, cancellationToken),
                GalleryItemType.Scene when item.Scene is not null && sceneEngine is not null =>
                    (await sceneEngine.ExecuteAsync(item.Scene, cancellationToken)).Message,
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

    public void OpenManageSheet(GalleryItem item)
    {
        ManagedItem = item;
        IsManageSheetVisible = true;
        IsAddOptionsVisible = false;
        IsFilterSheetVisible = false;
        IsGroupSheetVisible = false;
    }

    public void ToggleSelection(string itemId)
    {
        if (!selectedItemIds.Add(itemId))
        {
            selectedItemIds.Remove(itemId);
        }

        RebuildGroups();
        NotifySelectionChanged();
    }

    private async Task DeleteSelectedAsync(CancellationToken cancellationToken)
    {
        var selectedTextIds = allItems
            .Where(item => selectedItemIds.Contains(item.Id) && item.Type == GalleryItemType.TextPreset && item.TextPreset is not null)
            .Select(item => item.TextPreset!.Id)
            .ToArray();
        if (selectedTextIds.Length == 0)
        {
            StatusText = "Select text presets to delete.";
            return;
        }

        foreach (var id in selectedTextIds)
        {
            await textPresetStore.DeleteAsync(id, cancellationToken);
        }

        var deletedGalleryIds = selectedTextIds.Select(id => $"text:{id.Value}").ToHashSet(StringComparer.Ordinal);
        layoutState = layoutState with
        {
            Pages = layoutState.Pages
                .Select(page => page with
                {
                    Items = page.Items
                        .Where(item => !deletedGalleryIds.Contains(item.GalleryItemId))
                        .ToArray()
                })
                .ToArray()
        };
        await layoutStore.SaveAsync(layoutState, cancellationToken);

        selectedItemIds.Clear();
        await InitializeAsync(cancellationToken);
        NotifySelectionChanged();
        StatusText = selectedTextIds.Length == 1 ? "Deleted 1 text preset." : $"Deleted {selectedTextIds.Length} text presets.";
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
            .Where(MatchesSelectedType)
            .Where(item => query.Length == 0 || item.SearchText.Contains(query, StringComparison.Ordinal))
            .OrderBy(item => layoutState.Order.GetItemSortIndex(item.Id, item.SortIndex))
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        QuickDeckItems = allItems
            .Where(item => item.IsFavorite)
            .Where(MatchesSelectedType)
            .OrderBy(item => layoutState.Order.GetItemSortIndex(item.Id, item.SortIndex))
            .Take(6)
            .Select(CreateCard)
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
                IsEditMode,
                group.Items.Select(CreateCard).ToArray(),
                new AsyncRelayCommand(cancellationToken => MoveGroupAsync($"{SelectedGroupingMode.Mode}:{group.Key}", -1, cancellationToken)),
                new AsyncRelayCommand(cancellationToken => MoveGroupAsync($"{SelectedGroupingMode.Mode}:{group.Key}", 1, cancellationToken))))
            .ToArray();
        Rows = BuildRows(Groups);
        ApplyPreviewAnimationState();
        OnPropertyChanged(nameof(HasVisibleItems));
        OnPropertyChanged(nameof(IsEmptyStateVisible));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
    }

    private static IReadOnlyList<GalleryListRow> BuildRows(IReadOnlyList<GalleryGroupCard> groups)
    {
        var result = new List<GalleryListRow>();
        foreach (var group in groups)
        {
            result.Add(GalleryListRow.GroupHeader(group.Title, group.Items.Count));
            for (var index = 0; index < group.Items.Count; index += 2)
            {
                var right = index + 1 < group.Items.Count ? group.Items[index + 1] : null;
                result.Add(GalleryListRow.ItemPair(group.Items[index], right));
            }
        }

        return result;
    }

    public void SetVisibleRowRange(int firstVisibleIndex, int lastVisibleIndex, bool reduceMotion)
    {
        firstVisibleRowIndex = firstVisibleIndex;
        lastVisibleRowIndex = lastVisibleIndex;
        reducePreviewMotion = reduceMotion;
        ApplyPreviewAnimationState();
    }

    private void ApplyPreviewAnimationState()
    {
        for (var index = 0; index < Rows.Count; index++)
        {
            var shouldPlay = !reducePreviewMotion && index >= firstVisibleRowIndex && index <= lastVisibleRowIndex;
            Rows[index].Left?.SetAnimationPlaying(shouldPlay);
            Rows[index].Right?.SetAnimationPlaying(shouldPlay);
        }
    }

    public void StopPreviewAnimations()
    {
        firstVisibleRowIndex = -1;
        lastVisibleRowIndex = -1;
        reducePreviewMotion = true;
        foreach (var row in Rows)
        {
            row.Left?.SetAnimationPlaying(false);
            row.Right?.SetAnimationPlaying(false);
        }
    }

    public void StopMaskAnimation() => diySlotPlayback.RequestStopAnimation();

    private AsyncRelayCommand CreateTypeFilterCommand(GalleryTypeFilter filter) => new(_ =>
    {
        SelectedTypeFilter = filter;
        return Task.CompletedTask;
    });

    private bool MatchesSelectedType(GalleryItem item) => SelectedTypeFilter switch
    {
        GalleryTypeFilter.Faces => item.Type is GalleryItemType.CustomStaticFace or GalleryItemType.BuiltInStaticImage,
        GalleryTypeFilter.Text => item.Type == GalleryItemType.TextPreset ||
            item.Type == GalleryItemType.QuickAction && item.QuickActionKind is QuickActionKind.Text or QuickActionKind.Random,
        GalleryTypeFilter.Animations => item.Type is GalleryItemType.BuiltInAnimation or GalleryItemType.AppBuiltInAnimation or GalleryItemType.CustomAnimation,
        GalleryTypeFilter.Scenes => item.Type == GalleryItemType.Scene,
        _ => true
    };

    private void OnConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs e) =>
        ConnectionState = e.State;

    private GalleryItemCard CreateCard(GalleryItem item) =>
        new(
            item,
            IsEditMode,
            selectedItemIds.Contains(item.Id),
            new AsyncRelayCommand(cancellationToken => SendAsync(item, cancellationToken), () => item.CanSend && CanSend(item)),
            new AsyncRelayCommand(_ =>
            {
                ToggleSelection(item.Id);
                return Task.CompletedTask;
            }),
            new AsyncRelayCommand(_ =>
            {
                OpenManageSheet(item);
                return Task.CompletedTask;
            }, () => item.CanManage),
            new AsyncRelayCommand(cancellationToken => MoveItemAsync(item.Id, -1, cancellationToken)),
            new AsyncRelayCommand(cancellationToken => MoveItemAsync(item.Id, 1, cancellationToken)));

    private bool CanSend(GalleryItem item) =>
        item.Type switch
        {
            GalleryItemType.TextPreset => textTransport.IsReady,
            GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation
                => commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.CustomStaticFace when item.FacePattern is not null &&
                DiySlotPlaybackCoordinator.IsFacePrepared(item.FacePattern, faceState) =>
                commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.CustomStaticFace =>
                faceTransport.IsReady && commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.AppBuiltInAnimation when item.AppAnimation is not null &&
                DiySlotPlaybackCoordinator.IsAnimationPrepared(item.AppAnimation, faceState) =>
                commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.AppBuiltInAnimation =>
                faceTransport.IsReady && commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.CustomAnimation when item.PerformanceAnimation is not null &&
                DiySlotPlaybackCoordinator.IsAnimationPrepared(item.PerformanceAnimation, faceState) =>
                commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.CustomAnimation =>
                faceTransport.IsReady && commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.Scene => sceneEngine is not null && CanSendScene(item),
            GalleryItemType.QuickAction => item.QuickActionKind switch
            {
                QuickActionKind.Text or QuickActionKind.Random => textTransport.IsReady,
                QuickActionKind.Command or QuickActionKind.Brightness or QuickActionKind.BuiltInImage or QuickActionKind.BuiltInAnimation =>
                    commandTransport.TransportState == MaskCommandTransportState.Ready,
                _ => false
            },
            _ => false
        };

    private bool CanSendScene(GalleryItem item)
    {
        if (item.Scene is null || commandTransport.TransportState != MaskCommandTransportState.Ready)
        {
            return false;
        }

        var dependencies = item.Scene.Steps
            .Where(step => !string.IsNullOrWhiteSpace(step.GalleryItemId))
            .Select(step => allItems.FirstOrDefault(candidate => candidate.Id == step.GalleryItemId))
            .Where(dependency => dependency is not null);
        return dependencies.All(dependency => dependency!.Type switch
        {
            GalleryItemType.TextPreset => textTransport.IsReady,
            GalleryItemType.CustomStaticFace or GalleryItemType.AppBuiltInAnimation or GalleryItemType.CustomAnimation =>
                faceTransport.IsReady,
            _ => true
        });
    }

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

    private async Task<string> SendFaceAsync(FacePattern pattern, CancellationToken cancellationToken)
    {
        var result = await diySlotPlayback.PlayFaceAsync(pattern, cancellationToken);
        await InitializeAsync(cancellationToken);
        return result.Message;
    }

    private async Task<string> SendAppAnimationAsync(
        AppBuiltInAnimation animation,
        CancellationToken cancellationToken)
    {
        var result = await diySlotPlayback.PlayAnimationAsync(animation, cancellationToken);
        await InitializeAsync(cancellationToken);
        return result.Message;
    }

    private async Task<string> SendPerformanceAnimationAsync(
        PerformanceAnimation animation,
        CancellationToken cancellationToken)
    {
        var result = await diySlotPlayback.PlayAnimationAsync(animation, cancellationToken);
        await InitializeAsync(cancellationToken);
        return result.Message;
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

    private void ClearSelection()
    {
        if (selectedItemIds.Count == 0)
        {
            return;
        }

        selectedItemIds.Clear();
        RebuildGroups();
        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(SelectedDeletableCount));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(SelectedCountText));
        DeleteSelectedCommand.RaiseCanExecuteChanged();
        ClearSelectionCommand.RaiseCanExecuteChanged();
    }
}
