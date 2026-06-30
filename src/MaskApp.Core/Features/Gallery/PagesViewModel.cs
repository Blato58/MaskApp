using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Gallery;

public sealed class PagesViewModel : INotifyPropertyChanged
{
    private static readonly string[] ColorCycle =
    [
        "#52E3FF",
        "#A78BFA",
        "#FF3D8B",
        "#FACC15",
        "#22C55E",
        "#FFFFFF"
    ];

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
    private GalleryLayoutState layoutState = new();
    private IReadOnlyList<GalleryItem> allItems = [];
    private GalleryPageLayout selectedPage = new();
    private IReadOnlyList<GalleryPageTab> pages = [];
    private IReadOnlyList<GalleryPageShortcutCard> shortcuts = [];
    private IReadOnlyList<GalleryAvailableItemCard> availableItems = [];
    private string statusText = "Ready";
    private string newPageTitle = "New page";
    private bool isSending;
    private bool isManageMode;
    private bool isPageEditorSheetVisible;
    private bool isDeletePageConfirmationVisible;

    public PagesViewModel(
        QuickActionCatalog quickActionCatalog,
        ITextPresetStore textPresetStore,
        IBuiltInAssetArchiveStore builtInArchiveStore,
        IFacePatternStore facePatternStore,
        IGalleryLayoutStore layoutStore,
        IQuickActionDispatcher quickActionDispatcher,
        ITextPresetDispatcher textPresetDispatcher,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport,
        IFaceUploadTransport faceTransport)
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

        AddPageCommand = new AsyncRelayCommand(AddPageAsync);
        RemovePageCommand = new AsyncRelayCommand(OpenDeleteConfirmationAsync, () => Pages.Count > 1);
        ConfirmRemovePageCommand = new AsyncRelayCommand(RemoveSelectedPageAsync, () => Pages.Count > 1);
        CyclePageColorCommand = new AsyncRelayCommand(CyclePageColorAsync);
        SetUseModeCommand = new AsyncRelayCommand(_ =>
        {
            IsManageMode = false;
            CloseSheets();
            return Task.CompletedTask;
        });
        SetManageModeCommand = new AsyncRelayCommand(_ =>
        {
            IsManageMode = true;
            return Task.CompletedTask;
        });
        TogglePageEditorSheetCommand = new AsyncRelayCommand(_ =>
        {
            IsPageEditorSheetVisible = !IsPageEditorSheetVisible;
            if (IsPageEditorSheetVisible)
            {
                IsDeletePageConfirmationVisible = false;
            }

            return Task.CompletedTask;
        });
        CloseSheetsCommand = new AsyncRelayCommand(_ =>
        {
            CloseSheets();
            return Task.CompletedTask;
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand AddPageCommand { get; }

    public AsyncRelayCommand RemovePageCommand { get; }

    public AsyncRelayCommand ConfirmRemovePageCommand { get; }

    public AsyncRelayCommand CyclePageColorCommand { get; }

    public AsyncRelayCommand SetUseModeCommand { get; }

    public AsyncRelayCommand SetManageModeCommand { get; }

    public AsyncRelayCommand TogglePageEditorSheetCommand { get; }

    public AsyncRelayCommand CloseSheetsCommand { get; }

    public IReadOnlyList<GalleryPageTab> Pages
    {
        get => pages;
        private set => SetField(ref pages, value);
    }

    public GalleryPageLayout SelectedPage
    {
        get => selectedPage;
        private set
        {
            if (SetField(ref selectedPage, value))
            {
                OnPropertyChanged(nameof(SelectedPageTitle));
                OnPropertyChanged(nameof(SelectedPageColorHex));
                OnPropertyChanged(nameof(PagePositionText));
                OnPropertyChanged(nameof(DeletePagePrompt));
            }
        }
    }

    public string SelectedPageTitle => SelectedPage.Title;

    public string SelectedPageColorHex => SelectedPage.ColorHex;

    public IReadOnlyList<GalleryPageShortcutCard> Shortcuts
    {
        get => shortcuts;
        private set
        {
            if (SetField(ref shortcuts, value))
            {
                OnPropertyChanged(nameof(ShortcutCountText));
            }
        }
    }

    public IReadOnlyList<GalleryAvailableItemCard> AvailableItems
    {
        get => availableItems;
        private set => SetField(ref availableItems, value);
    }

    public string NewPageTitle
    {
        get => newPageTitle;
        set => SetField(ref newPageTitle, value ?? string.Empty);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public bool IsSending
    {
        get => isSending;
        private set => SetField(ref isSending, value);
    }

    public bool IsManageMode
    {
        get => isManageMode;
        set
        {
            if (SetField(ref isManageMode, value))
            {
                OnPropertyChanged(nameof(IsUseMode));
                OnPropertyChanged(nameof(PagesModeText));
            }
        }
    }

    public bool IsUseMode => !IsManageMode;

    public string PagesModeText => IsManageMode ? "Manage" : "Use";

    public bool IsPageEditorSheetVisible
    {
        get => isPageEditorSheetVisible;
        private set => SetField(ref isPageEditorSheetVisible, value);
    }

    public bool IsDeletePageConfirmationVisible
    {
        get => isDeletePageConfirmationVisible;
        private set => SetField(ref isDeletePageConfirmationVisible, value);
    }

    public string ShortcutCountText => $"{Shortcuts.Count} shortcuts";

    public string PagePositionText
    {
        get
        {
            var ordered = Pages.ToArray();
            var index = Array.FindIndex(ordered, page => page.PageId == SelectedPage.PageId);
            return ordered.Length == 0 || index < 0 ? "No pages" : $"Page {index + 1} of {ordered.Length}";
        }
    }

    public string DeletePagePrompt => $"Delete \"{SelectedPageTitle}\" page?";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        layoutState = (await layoutStore.LoadAsync(cancellationToken)).Normalize();
        var textState = await textPresetStore.LoadAsync(cancellationToken);
        var archive = await builtInArchiveStore.LoadAsync(cancellationToken);
        var faces = await facePatternStore.LoadAsync(cancellationToken);
        allItems = catalogBuilder.Build(textState, archive, faces, layoutState.Order);
        SelectedPage = layoutState.Pages.FirstOrDefault(page => page.PageId == SelectedPage.PageId)
            ?? layoutState.Pages.First();
        RebuildCards();
    }

    public async Task SelectPageAsync(string pageId, CancellationToken cancellationToken = default)
    {
        var page = layoutState.Pages.FirstOrDefault(item => item.PageId == pageId);
        if (page is null)
        {
            return;
        }

        SelectedPage = page;
        RebuildCards();
        CloseSheets();
        await Task.CompletedTask;
    }

    public async Task AddItemAsync(GalleryItem item, CancellationToken cancellationToken = default)
    {
        await AddItemAsync(item.Id, item.Title, item.IconKey, item.ColorHex, cancellationToken);
    }

    public async Task AddItemAsync(
        string galleryItemId,
        string label,
        string iconKey,
        string colorHex,
        CancellationToken cancellationToken = default)
    {
        var page = SelectedPage;
        if (page.Items.Any(item => item.GalleryItemId == galleryItemId))
        {
            StatusText = "Shortcut already exists";
            return;
        }

        var item = allItems.FirstOrDefault(candidate => candidate.Id == galleryItemId);
        if (item is null)
        {
            StatusText = "Gallery item unavailable";
            return;
        }

        var layoutItem = new GalleryPageItemLayout
        {
            GalleryItemId = galleryItemId,
            Label = string.IsNullOrWhiteSpace(label) ? item.Title : label.Trim(),
            IconKey = string.IsNullOrWhiteSpace(iconKey) ? item.IconKey : iconKey.Trim(),
            ColorHex = string.IsNullOrWhiteSpace(colorHex) ? item.ColorHex : colorHex.Trim(),
            SortIndex = page.Items.Count == 0 ? 0 : page.Items.Max(existing => existing.SortIndex) + 1
        };
        await UpdateSelectedPageAsync(page with { Items = page.Items.Append(layoutItem).ToArray() }, cancellationToken);
        StatusText = "Shortcut added";
    }

    public async Task RemoveItemAsync(string slotId, CancellationToken cancellationToken = default)
    {
        await UpdateSelectedPageAsync(
            SelectedPage with { Items = SelectedPage.Items.Where(item => item.SlotId != slotId).ToArray() },
            cancellationToken);
        StatusText = "Shortcut removed";
    }

    public async Task MoveItemAsync(string slotId, int delta, CancellationToken cancellationToken = default)
    {
        var items = SelectedPage.Items.OrderBy(item => item.SortIndex).ToArray();
        var currentIndex = Array.FindIndex(items, item => item.SlotId == slotId);
        var targetIndex = currentIndex + delta;
        if (currentIndex < 0 || targetIndex < 0 || targetIndex >= items.Length)
        {
            return;
        }

        var first = items[currentIndex];
        var second = items[targetIndex];
        var updatedItems = items
            .Select(item => item.SlotId == first.SlotId
                ? item with { SortIndex = second.SortIndex }
                : item.SlotId == second.SlotId
                    ? item with { SortIndex = first.SortIndex }
                    : item)
            .ToArray();

        await UpdateSelectedPageAsync(SelectedPage with { Items = updatedItems }, cancellationToken);
        StatusText = "Shortcut order saved";
    }

    public async Task MovePageAsync(string pageId, int delta, CancellationToken cancellationToken = default)
    {
        var pages = layoutState.Pages.OrderBy(page => page.SortIndex).ToArray();
        var currentIndex = Array.FindIndex(pages, page => page.PageId == pageId);
        var targetIndex = currentIndex + delta;
        if (currentIndex < 0 || targetIndex < 0 || targetIndex >= pages.Length)
        {
            return;
        }

        var first = pages[currentIndex];
        var second = pages[targetIndex];
        var updatedPages = pages
            .Select(page => page.PageId == first.PageId
                ? page with { SortIndex = second.SortIndex }
                : page.PageId == second.PageId
                    ? page with { SortIndex = first.SortIndex }
                    : page)
            .ToArray();

        await SaveLayoutAsync(layoutState with { Pages = updatedPages }, cancellationToken);
        StatusText = "Page order saved";
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
            StatusText = "Sending...";
            StatusText = item.Type switch
            {
                GalleryItemType.TextPreset when item.TextPreset is not null =>
                    await SendTextPresetAsync(item.TextPreset, cancellationToken),
                GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation when item.BuiltInAssetRecord is not null =>
                    await SendBuiltInAsync(item.BuiltInAssetRecord, cancellationToken),
                GalleryItemType.CustomStaticFace when item.FacePattern is not null =>
                    await SendFaceAsync(item.FacePattern, cancellationToken),
                GalleryItemType.QuickAction when item.QuickActionId.HasValue =>
                    (await quickActionDispatcher.TriggerAsync(item.QuickActionId.Value, cancellationToken: cancellationToken)).Message,
                _ => "Not implemented yet"
            };
        }
        finally
        {
            IsSending = false;
        }
    }

    private async Task AddPageAsync(CancellationToken cancellationToken)
    {
        var title = string.IsNullOrWhiteSpace(NewPageTitle) ? "New page" : NewPageTitle.Trim();
        var page = new GalleryPageLayout
        {
            Title = title,
            ColorHex = ColorCycle[layoutState.Pages.Count % ColorCycle.Length],
            SortIndex = layoutState.Pages.Count == 0 ? 0 : layoutState.Pages.Max(item => item.SortIndex) + 1
        };
        await SaveLayoutAsync(layoutState with { Pages = layoutState.Pages.Append(page).ToArray() }, cancellationToken);
        SelectedPage = layoutState.Pages.Last();
        NewPageTitle = "New page";
        RebuildCards();
        StatusText = "Page added";
    }

    private Task OpenDeleteConfirmationAsync(CancellationToken cancellationToken)
    {
        if (layoutState.Pages.Count <= 1)
        {
            StatusText = "Keep at least one page";
            return Task.CompletedTask;
        }

        IsDeletePageConfirmationVisible = true;
        IsPageEditorSheetVisible = false;
        return Task.CompletedTask;
    }

    private async Task RemoveSelectedPageAsync(CancellationToken cancellationToken)
    {
        if (layoutState.Pages.Count <= 1)
        {
            StatusText = "Keep at least one page";
            return;
        }

        var pages = layoutState.Pages.Where(page => page.PageId != SelectedPage.PageId).ToArray();
        await SaveLayoutAsync(layoutState with { Pages = pages }, cancellationToken);
        SelectedPage = layoutState.Pages.First();
        RebuildCards();
        IsDeletePageConfirmationVisible = false;
        StatusText = "Page removed";
    }

    private Task CyclePageColorAsync(CancellationToken cancellationToken)
    {
        var currentIndex = Array.IndexOf(ColorCycle, SelectedPage.ColorHex);
        var color = ColorCycle[(currentIndex + 1 + ColorCycle.Length) % ColorCycle.Length];
        return UpdateSelectedPageAsync(SelectedPage with { ColorHex = color }, cancellationToken);
    }

    private Task UpdateSelectedPageAsync(GalleryPageLayout page, CancellationToken cancellationToken)
    {
        var pages = layoutState.Pages
            .Select(existing => existing.PageId == page.PageId ? page : existing)
            .ToArray();
        return SaveLayoutAsync(layoutState with { Pages = pages }, cancellationToken);
    }

    private async Task SaveLayoutAsync(GalleryLayoutState state, CancellationToken cancellationToken)
    {
        layoutState = state.Normalize();
        await layoutStore.SaveAsync(layoutState, cancellationToken);
        SelectedPage = layoutState.Pages.FirstOrDefault(page => page.PageId == SelectedPage.PageId)
            ?? layoutState.Pages.First();
        RebuildCards();
    }

    private void RebuildCards()
    {
        Pages = layoutState.Pages
            .OrderBy(page => page.SortIndex)
            .Select(page => new GalleryPageTab(
                page,
                page.PageId == SelectedPage.PageId,
                new AsyncRelayCommand(cancellationToken => SelectPageAsync(page.PageId, cancellationToken)),
                new AsyncRelayCommand(cancellationToken => MovePageAsync(page.PageId, -1, cancellationToken)),
                new AsyncRelayCommand(cancellationToken => MovePageAsync(page.PageId, 1, cancellationToken))))
            .ToArray();

        OnPropertyChanged(nameof(PagePositionText));
        OnPropertyChanged(nameof(DeletePagePrompt));

        Shortcuts = SelectedPage.Items
            .OrderBy(item => item.SortIndex)
            .Select(layout =>
            {
                var item = allItems.FirstOrDefault(candidate => candidate.Id == layout.GalleryItemId);
                return item is null ? null : CreateShortcutCard(layout, item);
            })
            .Where(card => card is not null)
            .Cast<GalleryPageShortcutCard>()
            .ToArray();

        var usedIds = SelectedPage.Items.Select(item => item.GalleryItemId).ToHashSet(StringComparer.Ordinal);
        AvailableItems = allItems
            .Where(item => item.CanSend && !usedIds.Contains(item.Id))
            .OrderByDescending(item => item.IsFavorite)
            .ThenBy(item => item.GroupName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .Select(item => new GalleryAvailableItemCard(
                item,
                new AsyncRelayCommand(cancellationToken => AddItemAsync(item, cancellationToken))))
            .ToArray();

        RemovePageCommand.RaiseCanExecuteChanged();
        ConfirmRemovePageCommand.RaiseCanExecuteChanged();
    }

    private void CloseSheets()
    {
        IsPageEditorSheetVisible = false;
        IsDeletePageConfirmationVisible = false;
    }

    private GalleryPageShortcutCard CreateShortcutCard(GalleryPageItemLayout layout, GalleryItem item) =>
        new(
            layout,
            item,
            new AsyncRelayCommand(cancellationToken => SendAsync(item, cancellationToken), () => item.CanSend && CanSend(item)),
            new AsyncRelayCommand(cancellationToken => RemoveItemAsync(layout.SlotId, cancellationToken)),
            new AsyncRelayCommand(cancellationToken => MoveItemAsync(layout.SlotId, -1, cancellationToken)),
            new AsyncRelayCommand(cancellationToken => MoveItemAsync(layout.SlotId, 1, cancellationToken)));

    private bool CanSend(GalleryItem item) =>
        item.Type switch
        {
            GalleryItemType.TextPreset => textTransport.IsReady,
            GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation => commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.CustomStaticFace => faceTransport.IsReady,
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

        return (await textPresetDispatcher.SendAsync(preset, cancellationToken)).Message;
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
        if (!faceTransport.IsReady)
        {
            return "Face upload not ready";
        }

        var normalized = pattern.Normalize();
        var options = faceTransport.SupportsAcknowledgements
            ? FaceUploadOptions.RequireAcknowledgements
            : FaceUploadOptions.WriteOnlyCompatibility;
        var package = FaceUploadProtocol.CreatePackage(normalized, normalized.PreferredSlot);
        var result = await faceTransport.UploadAsync(package, options, cancellationToken);
        var status = result.Succeeded
            ? $"Uploaded slot {normalized.PreferredSlot}; needs real-mask visual confirmation."
            : result.Message;
        var state = await facePatternStore.LoadAsync(cancellationToken);
        await facePatternStore.SaveAsync(state.MarkUploaded(normalized.Id, status, DateTimeOffset.UtcNow), cancellationToken);
        await InitializeAsync(cancellationToken);
        return status;
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
