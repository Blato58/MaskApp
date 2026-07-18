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
    private readonly DiySlotPlaybackCoordinator diySlotPlayback;
    private readonly IAnimationProjectStore animationProjectStore;
    private readonly ISceneShowStore sceneShowStore;
    private readonly SceneExecutionEngine? sceneEngine;
    private readonly SceneReadinessEvaluator sceneReadinessEvaluator;
    private GalleryLayoutState layoutState = new();
    private FacePatternStoreState faceState = new();
    private IReadOnlyList<GalleryItem> allItems = [];
    private GalleryPageLayout selectedPage = new();
    private IReadOnlyList<GalleryPageTab> pages = [];
    private IReadOnlyList<GalleryPageShortcutCard> shortcuts = [];
    private IReadOnlyList<GalleryAvailableItemCard> availableItems = [];
    private string statusText = "Ready";
    private string newPageTitle = "New page";
    private string pageTitleDraft = string.Empty;
    private bool isSending;
    private int activeOperationCount;
    private bool isManageMode;
    private bool isPageEditorSheetVisible;
    private int firstVisibleShortcutIndex = -1;
    private int lastVisibleShortcutIndex = -1;
    private bool reducePreviewMotion = true;
    private bool isDeletePageConfirmationVisible;
    private bool isObservingTransportState;
    private GalleryActionResult lastActionResult = GalleryActionResult.Success("No Stage action sent yet.");

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
        IFaceUploadTransport faceTransport,
        DiySlotPlaybackCoordinator diySlotPlayback,
        IAnimationProjectStore? animationProjectStore = null,
        ISceneShowStore? sceneShowStore = null,
        SceneExecutionEngine? sceneEngine = null,
        SceneReadinessEvaluator? sceneReadinessEvaluator = null)
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
        this.sceneReadinessEvaluator = sceneReadinessEvaluator ?? new SceneReadinessEvaluator();

        AddPageCommand = new AsyncRelayCommand(AddPageAsync);
        RemovePageCommand = new AsyncRelayCommand(OpenDeleteConfirmationAsync, () => Pages.Count > 1);
        ConfirmRemovePageCommand = new AsyncRelayCommand(RemoveSelectedPageAsync, () => Pages.Count > 1);
        CyclePageColorCommand = new AsyncRelayCommand(CyclePageColorAsync);
        SavePageTitleCommand = new AsyncRelayCommand(SavePageTitleAsync);
        MoveSelectedPageEarlierCommand = new AsyncRelayCommand(
            cancellationToken => MovePageAsync(SelectedPage.PageId, -1, cancellationToken));
        MoveSelectedPageLaterCommand = new AsyncRelayCommand(
            cancellationToken => MovePageAsync(SelectedPage.PageId, 1, cancellationToken));
        PreparePageCommand = new AsyncRelayCommand(PreparePageAsync, CanPreparePage);
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
                PageTitleDraft = SelectedPageTitle;
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

    public event EventHandler<GalleryActionCompletedEventArgs>? ActionCompleted;

    public AsyncRelayCommand AddPageCommand { get; }

    public AsyncRelayCommand RemovePageCommand { get; }

    public AsyncRelayCommand ConfirmRemovePageCommand { get; }

    public AsyncRelayCommand CyclePageColorCommand { get; }

    public AsyncRelayCommand SavePageTitleCommand { get; }

    public AsyncRelayCommand MoveSelectedPageEarlierCommand { get; }

    public AsyncRelayCommand MoveSelectedPageLaterCommand { get; }

    public AsyncRelayCommand PreparePageCommand { get; }

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
                OnPropertyChanged(nameof(PageReadinessText));
                PreparePageCommand.RaiseCanExecuteChanged();
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

    public string PageTitleDraft
    {
        get => pageTitleDraft;
        set => SetField(ref pageTitleDraft, value ?? string.Empty);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public GalleryActionResult LastActionResult
    {
        get => lastActionResult;
        private set => SetField(ref lastActionResult, value);
    }

    public bool IsSending
    {
        get => isSending;
        private set
        {
            if (SetField(ref isSending, value))
            {
                RefreshTransportCommandState();
            }
        }
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

    public string PageReadinessText
    {
        get
        {
            var fastItems = Shortcuts.Where(shortcut => shortcut.IsFastSlotCapable).ToArray();
            if (fastItems.Length == 0)
            {
                return "All actions are already instant";
            }

            var prepared = fastItems.Count(shortcut => shortcut.IsFastSlotPrepared);
            return prepared == fastItems.Length
                ? $"{prepared} fast slots ready"
                : $"{prepared} of {fastItems.Length} fast slots ready";
        }
    }

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
        faceState = (await facePatternStore.LoadAsync(cancellationToken)).Normalize();
        var animationState = await animationProjectStore.LoadAsync(cancellationToken);
        var sceneState = await sceneShowStore.LoadAsync(cancellationToken);
        allItems = catalogBuilder.Build(textState, archive, faceState, layoutState.Order, animationState, sceneState);
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
        await SendCoreAsync(null, item, cancellationToken);
    }

    public async Task<GalleryActionResult> SendStageShortcutAsync(
        string slotId,
        CancellationToken cancellationToken = default)
    {
        var layout = SelectedPage.Items.FirstOrDefault(item =>
            string.Equals(item.SlotId, slotId, StringComparison.Ordinal));
        var item = layout is null
            ? null
            : allItems.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, layout.GalleryItemId, StringComparison.Ordinal));
        if (layout is null || item is null)
        {
            return GalleryActionResult.Failure("This Stage tile is no longer present on the selected Page.");
        }

        try
        {
            await SendCoreAsync(layout, item, cancellationToken);
            return LastActionResult;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            var failure = GalleryActionResult.Failure($"Stage action failed: {exception.Message}");
            LastActionResult = failure;
            StatusText = failure.Message;
            ActionCompleted?.Invoke(this, new GalleryActionCompletedEventArgs(item, layout, failure));
            return failure;
        }
    }

    private async Task SendShortcutAsync(
        GalleryPageItemLayout layout,
        GalleryItem item,
        CancellationToken cancellationToken)
    {
        await SendCoreAsync(layout, item, cancellationToken);
    }

    private async Task SendCoreAsync(
        GalleryPageItemLayout? layout,
        GalleryItem item,
        CancellationToken cancellationToken)
    {
        if (!item.CanSend)
        {
            var unavailable = GalleryActionResult.Failure("Not implemented yet");
            LastActionResult = unavailable;
            StatusText = unavailable.Message;
            ActionCompleted?.Invoke(this, new GalleryActionCompletedEventArgs(item, layout, unavailable));
            return;
        }

        try
        {
            BeginOperation();
            StatusText = "Sending...";
            await diySlotPlayback.StopAnimationAsync(cancellationToken);
            var result = item.Type switch
            {
                GalleryItemType.TextPreset when layout is not null && item.TextPreset is not null =>
                    await SendTextShortcutAsync(layout, item, cancellationToken),
                GalleryItemType.TextPreset when item.TextPreset is not null =>
                    await SendTextPresetAsync(item.TextPreset, cancellationToken),
                GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation when item.BuiltInAssetRecord is not null =>
                    await SendBuiltInAsync(item.BuiltInAssetRecord, cancellationToken),
                GalleryItemType.CustomStaticFace when layout is not null && item.FacePattern is not null =>
                    await SendFaceShortcutAsync(layout, item, cancellationToken),
                GalleryItemType.CustomStaticFace when item.FacePattern is not null =>
                    await SendFaceAsync(item.FacePattern, cancellationToken),
                GalleryItemType.AppBuiltInAnimation when item.AppAnimation is not null =>
                    await SendAppAnimationShortcutAsync(item.AppAnimation, cancellationToken),
                GalleryItemType.CustomAnimation when item.PerformanceAnimation is not null =>
                    await SendPerformanceAnimationShortcutAsync(item.PerformanceAnimation, cancellationToken),
                GalleryItemType.Scene when item.Scene is not null && sceneEngine is not null =>
                    ToGalleryActionResult(await sceneEngine.ExecuteAsync(item.Scene, cancellationToken)),
                GalleryItemType.QuickAction when item.QuickActionId.HasValue =>
                    ToGalleryActionResult(await quickActionDispatcher.TriggerAsync(
                        item.QuickActionId.Value,
                        cancellationToken: cancellationToken)),
                _ => GalleryActionResult.Failure("Not implemented yet")
            };
            LastActionResult = result;
            StatusText = result.Message;
            ActionCompleted?.Invoke(this, new GalleryActionCompletedEventArgs(item, layout, result));
        }
        finally
        {
            EndOperation();
        }
    }

    private async Task AddPageAsync(CancellationToken cancellationToken)
    {
        var title = string.IsNullOrWhiteSpace(NewPageTitle) || string.Equals(NewPageTitle.Trim(), "New page", StringComparison.OrdinalIgnoreCase)
            ? $"Page {layoutState.Pages.Count + 1}"
            : NewPageTitle.Trim();
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

    private async Task SavePageTitleAsync(CancellationToken cancellationToken)
    {
        var title = string.IsNullOrWhiteSpace(PageTitleDraft) ? SelectedPageTitle : PageTitleDraft.Trim();
        await UpdateSelectedPageAsync(SelectedPage with { Title = title }, cancellationToken);
        IsPageEditorSheetVisible = false;
        StatusText = "Page name saved";
    }

    private async Task<GalleryActionResult> SendTextShortcutAsync(
        GalleryPageItemLayout layout,
        GalleryItem item,
        CancellationToken cancellationToken)
    {
        if (IsFastSlotPrepared(layout, item))
        {
            return await PlayFastSlotAsync(layout.FastMaskSlot!.Value, cancellationToken);
        }

        if (faceTransport.IsReady && ResolveFastSlot(layout, item) is not null)
        {
            return ToGalleryActionResult(await PrepareFastSlotCoreAsync(layout, item, cancellationToken));
        }

        return item.TextPreset is null
            ? GalleryActionResult.Failure("Text not ready")
            : await SendTextPresetAsync(item.TextPreset, cancellationToken);
    }

    private async Task<GalleryActionResult> SendFaceShortcutAsync(
        GalleryPageItemLayout layout,
        GalleryItem item,
        CancellationToken cancellationToken)
    {
        if (IsFastSlotPrepared(layout, item))
        {
            return await PlayFastSlotAsync(layout.FastMaskSlot!.Value, cancellationToken);
        }

        return ToGalleryActionResult(await PrepareFastSlotCoreAsync(layout, item, cancellationToken));
    }

    private async Task<GalleryActionResult> SendAppAnimationShortcutAsync(
        AppBuiltInAnimation animation,
        CancellationToken cancellationToken)
    {
        var result = await diySlotPlayback.PlayAnimationAsync(animation, cancellationToken);
        await InitializeAsync(cancellationToken);
        return new GalleryActionResult(result.Succeeded, result.Message);
    }

    private async Task<GalleryActionResult> SendPerformanceAnimationShortcutAsync(
        PerformanceAnimation animation,
        CancellationToken cancellationToken)
    {
        var result = await diySlotPlayback.PlayAnimationAsync(animation, cancellationToken);
        await InitializeAsync(cancellationToken);
        return new GalleryActionResult(result.Succeeded, result.Message);
    }

    private async Task<GalleryActionResult> PlayFastSlotAsync(int slot, CancellationToken cancellationToken)
    {
        if (commandTransport.TransportState != MaskCommandTransportState.Ready)
        {
            return GalleryActionResult.Failure("Connect to use the fast slot");
        }

        var result = await commandTransport.SendAsync(FaceUploadProtocol.BuildPlayCommand([slot]), cancellationToken);
        return result.Succeeded
            ? GalleryActionResult.Success($"Fast slot {slot} sent · confirm on mask")
            : GalleryActionResult.Failure(result.Message);
    }

    private async Task PrepareShortcutAsync(
        GalleryPageItemLayout layout,
        GalleryItem item,
        CancellationToken cancellationToken)
    {
        try
        {
            BeginOperation();
            StatusText = $"Preparing {item.Title}...";
            StatusText = (await PrepareReusableSlotCoreAsync(layout, item, cancellationToken)).Message;
        }
        finally
        {
            EndOperation();
        }
    }

    private async Task PreparePageAsync(CancellationToken cancellationToken)
    {
        var pending = Shortcuts
            .Where(shortcut => shortcut.Item.Type != GalleryItemType.Scene
                && shortcut.IsFastSlotCapable
                && !shortcut.IsFastSlotPrepared)
            .Select(shortcut => (shortcut.Layout, shortcut.Item))
            .ToArray();
        if (pending.Length == 0)
        {
            StatusText = Shortcuts.Any(shortcut => shortcut.Item.Type == GalleryItemType.Scene && !shortcut.IsFastSlotPrepared)
                ? "Run Preflight to prepare every persistent dependency in the Scene."
                : "This page is ready";
            return;
        }

        var prepared = 0;
        try
        {
            BeginOperation();
            foreach (var (layout, item) in pending)
            {
                if (IsFastSlotPrepared(layout, item))
                {
                    continue;
                }

                StatusText = $"Preparing {prepared + 1} of {pending.Length}: {item.Title}";
                var result = await PrepareReusableSlotCoreAsync(layout, item, cancellationToken);
                if (!result.Succeeded)
                {
                    StatusText = result.Message;
                    return;
                }

                prepared++;
            }

            StatusText = $"Prepared {prepared} fast slots";
        }
        finally
        {
            EndOperation();
        }
    }

    private async Task<(bool Succeeded, string Message)> PrepareFastSlotCoreAsync(
        GalleryPageItemLayout layout,
        GalleryItem item,
        CancellationToken cancellationToken)
    {
        await FaceUploadOperationLock.Gate.WaitAsync(cancellationToken);
        try
        {
            return await PrepareFastSlotLockedAsync(layout, item, cancellationToken);
        }
        finally
        {
            FaceUploadOperationLock.Gate.Release();
        }
    }

    private async Task<(bool Succeeded, string Message)> PrepareReusableSlotCoreAsync(
        GalleryPageItemLayout layout,
        GalleryItem item,
        CancellationToken cancellationToken)
    {
        if (item.AppAnimation is not null)
        {
            var result = DiySlotPlaybackCoordinator.IsAnimationPrepared(item.AppAnimation, faceState)
                ? await diySlotPlayback.RefreshAnimationAsync(item.AppAnimation, cancellationToken)
                : await diySlotPlayback.PrepareAnimationAsync(item.AppAnimation, cancellationToken);
            await InitializeAsync(cancellationToken);
            return (result.Succeeded, result.Message);
        }

        if (item.PerformanceAnimation is not null)
        {
            var result = DiySlotPlaybackCoordinator.IsAnimationPrepared(item.PerformanceAnimation, faceState)
                ? await diySlotPlayback.RefreshAnimationAsync(item.PerformanceAnimation, cancellationToken)
                : await diySlotPlayback.PrepareAnimationAsync(item.PerformanceAnimation, cancellationToken);
            await InitializeAsync(cancellationToken);
            return (result.Succeeded, result.Message);
        }

        return await PrepareFastSlotCoreAsync(layout, item, cancellationToken);
    }

    private async Task<(bool Succeeded, string Message)> PrepareFastSlotLockedAsync(
        GalleryPageItemLayout layout,
        GalleryItem item,
        CancellationToken cancellationToken)
    {
        if (!PageFastSlotSnapshotFactory.Supports(item))
        {
            return (false, "This shortcut does not need a fast slot");
        }

        if (!faceTransport.IsReady)
        {
            return (false, "Connect to prepare a fast slot");
        }

        var slot = ResolveFastSlot(layout, item);
        if (slot is null)
        {
            return (false, "All 20 DIY slots are reserved; remove or reassign a face first");
        }

        PageFastSlotSnapshot snapshot;
        try
        {
            snapshot = PageFastSlotSnapshotFactory.Create(item, slot.Value);
        }
        catch (ArgumentException ex)
        {
            return (false, ex.Message);
        }

        var options = faceTransport.SupportsAcknowledgements
            ? FaceUploadOptions.RequireAcknowledgements
            : FaceUploadOptions.WriteOnlyCompatibility;
        var package = FaceUploadProtocol.CreatePackage(snapshot.Pattern, slot.Value);
        var state = (await facePatternStore.LoadAsync(cancellationToken))
            .ClearSlotInstallation(slot.Value);
        await facePatternStore.SaveAsync(state, cancellationToken);
        var result = await faceTransport.UploadAsync(package, options, cancellationToken);
        state = await facePatternStore.LoadAsync(cancellationToken);
        if (!result.Succeeded)
        {
            state = state.ClearSlotInstallation(slot.Value);
            if (item.FacePattern is not null)
            {
                state = state.MarkUploadFailed(item.FacePattern.Id, result.Message);
            }

            await facePatternStore.SaveAsync(state, cancellationToken);
            await InitializeAsync(cancellationToken);
            return (false, result.Message);
        }

        var timestamp = DateTimeOffset.UtcNow;
        if (item.FacePattern is not null)
        {
            state = state.MarkUploaded(item.FacePattern.Id, $"Prepared fast slot {slot.Value}.", timestamp);
        }

        state = state.MarkSlotInstalled(slot.Value, snapshot.ContentFingerprint, item.Id, timestamp);
        await facePatternStore.SaveAsync(state, cancellationToken);

        await SavePreparedSlotAsync(
            layout,
            slot.Value,
            snapshot.ContentFingerprint,
            timestamp,
            cancellationToken);
        var qualifier = item.Type == GalleryItemType.TextPreset ? " · static text" : string.Empty;
        return (true, $"Prepared fast slot {slot.Value}{qualifier} · sent to mask");
    }

    private async Task SavePreparedSlotAsync(
        GalleryPageItemLayout sourceLayout,
        int slot,
        string fingerprint,
        DateTimeOffset preparedAt,
        CancellationToken cancellationToken)
    {
        var pages = layoutState.Pages
            .Select(page => page with
            {
                Items = page.Items
                    .Select(item =>
                    {
                        if (string.Equals(item.GalleryItemId, sourceLayout.GalleryItemId, StringComparison.Ordinal))
                        {
                            return item with
                            {
                                FastMaskSlot = slot,
                                FastContentFingerprint = fingerprint,
                                FastPreparedAt = preparedAt
                            };
                        }

                        return item.FastMaskSlot == slot
                            ? item with
                            {
                                FastMaskSlot = null,
                                FastContentFingerprint = string.Empty,
                                FastPreparedAt = null
                            }
                            : item;
                    })
                    .ToArray()
            })
            .ToArray();
        layoutState = (layoutState with { Pages = pages }).Normalize();
        await layoutStore.SaveAsync(layoutState, cancellationToken);
        await InitializeAsync(cancellationToken);
    }

    private int? ResolveFastSlot(GalleryPageItemLayout layout, GalleryItem item)
    {
        if (item.FacePattern is not null)
        {
            return item.FacePattern.Normalize().PreferredSlot;
        }

        var sharedSlot = layoutState.Pages
            .SelectMany(page => page.Items)
            .FirstOrDefault(candidate =>
                candidate.FastMaskSlot is not null &&
                string.Equals(candidate.GalleryItemId, layout.GalleryItemId, StringComparison.Ordinal))
            ?.FastMaskSlot;
        if (sharedSlot is not null && !IsReservedFaceSlot(sharedSlot.Value))
        {
            return sharedSlot;
        }

        if (layout.FastMaskSlot is int existingSlot && !IsReservedFaceSlot(existingSlot))
        {
            return existingSlot;
        }

        var usedSlots = layoutState.Pages
            .SelectMany(page => page.Items)
            .Where(candidate => !string.Equals(candidate.GalleryItemId, layout.GalleryItemId, StringComparison.Ordinal))
            .Select(candidate => candidate.FastMaskSlot)
            .OfType<int>()
            .ToHashSet();
        var faceSlots = allItems
            .Select(candidate => candidate.FacePattern?.Normalize().PreferredSlot)
            .OfType<int>()
            .ToHashSet();
        var animationSlots = allItems
            .SelectMany(candidate => candidate.AppAnimation?.ReservedSlots
                ?? candidate.PerformanceAnimation?.StoredFrames.Select(frame => frame.Slot)
                ?? [])
            .ToHashSet();

        for (var slot = FacePattern.MaxSlot; slot >= FacePattern.MinSlot; slot--)
        {
            if (!usedSlots.Contains(slot) && !faceSlots.Contains(slot) && !animationSlots.Contains(slot))
            {
                return slot;
            }
        }

        return null;
    }

    private bool IsReservedFaceSlot(int slot) =>
        allItems.Any(candidate =>
            candidate.FacePattern?.Normalize().PreferredSlot == slot ||
            candidate.AppAnimation?.ReservedSlots.Contains(slot) == true ||
            candidate.PerformanceAnimation?.StoredFrames.Any(frame => frame.Slot == slot) == true);

    private bool IsFastSlotPrepared(GalleryPageItemLayout layout, GalleryItem item)
    {
        if (item.AppAnimation is not null)
        {
            return DiySlotPlaybackCoordinator.IsAnimationPrepared(item.AppAnimation, faceState);
        }

        if (item.PerformanceAnimation is not null)
        {
            return DiySlotPlaybackCoordinator.IsAnimationPrepared(item.PerformanceAnimation, faceState);
        }

        if (layout.FastMaskSlot is not int slot ||
            layout.FastPreparedAt is null ||
            string.IsNullOrWhiteSpace(layout.FastContentFingerprint))
        {
            return false;
        }

        try
        {
            var snapshot = PageFastSlotSnapshotFactory.Create(item, slot);
            if (!string.Equals(
                layout.FastContentFingerprint,
                snapshot.ContentFingerprint,
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var installation = faceState.GetSlotInstallation(slot);
            return installation is not null &&
                string.Equals(
                    installation.ContentFingerprint,
                    snapshot.ContentFingerprint,
                    StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private bool CanPreparePage() =>
        !IsSending && faceTransport.IsReady && Shortcuts.Any(shortcut => shortcut.IsFastSlotCapable && !shortcut.IsFastSlotPrepared);

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
        ApplyPreviewAnimationState();

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

    public void SetVisibleShortcutRange(int firstVisibleIndex, int lastVisibleIndex, bool reduceMotion)
    {
        firstVisibleShortcutIndex = firstVisibleIndex;
        lastVisibleShortcutIndex = lastVisibleIndex;
        reducePreviewMotion = reduceMotion;
        ApplyPreviewAnimationState();
    }

    private void ApplyPreviewAnimationState()
    {
        for (var index = 0; index < Shortcuts.Count; index++)
        {
            Shortcuts[index].SetAnimationPlaying(!reducePreviewMotion && index >= firstVisibleShortcutIndex && index <= lastVisibleShortcutIndex);
        }
    }

    public void StopPreviewAnimations()
    {
        firstVisibleShortcutIndex = -1;
        lastVisibleShortcutIndex = -1;
        reducePreviewMotion = true;
        foreach (var shortcut in Shortcuts)
        {
            shortcut.SetAnimationPlaying(false);
        }
    }

    public void StopMaskAnimation() => diySlotPlayback.RequestStopAnimation();

    public void StartObservingTransportState()
    {
        if (isObservingTransportState)
        {
            return;
        }

        commandTransport.TransportStateChanged += OnCommandTransportStateChanged;
        textTransport.StateChanged += OnTextTransportStateChanged;
        faceTransport.StateChanged += OnFaceTransportStateChanged;
        isObservingTransportState = true;
        RefreshTransportCommandState();
    }

    public void StopObservingTransportState()
    {
        if (!isObservingTransportState)
        {
            return;
        }

        commandTransport.TransportStateChanged -= OnCommandTransportStateChanged;
        textTransport.StateChanged -= OnTextTransportStateChanged;
        faceTransport.StateChanged -= OnFaceTransportStateChanged;
        isObservingTransportState = false;
    }

    private void CloseSheets()
    {
        IsPageEditorSheetVisible = false;
        IsDeletePageConfirmationVisible = false;
    }

    private GalleryPageShortcutCard CreateShortcutCard(GalleryPageItemLayout layout, GalleryItem item)
    {
        var isFastSlotCapable = PageFastSlotSnapshotFactory.Supports(item) ||
            item.AppAnimation is not null || item.PerformanceAnimation is not null || item.Scene is not null;
        var isFastSlotPrepared = item.Scene is not null
            ? IsScenePrepared(item.Scene)
            : isFastSlotCapable && IsFastSlotPrepared(layout, item);
        return new GalleryPageShortcutCard(
            layout,
            item,
            new AsyncRelayCommand(
                cancellationToken => SendShortcutAsync(layout, item, cancellationToken),
                () => item.CanSend && CanSend(layout, item)),
            new AsyncRelayCommand(
                cancellationToken => PrepareShortcutAsync(layout, item, cancellationToken),
                () => !IsSending &&
                    item.Type != GalleryItemType.Scene &&
                    isFastSlotCapable &&
                    faceTransport.IsReady &&
                    (item.AppAnimation is not null || item.PerformanceAnimation is not null || ResolveFastSlot(layout, item) is not null)),
            new AsyncRelayCommand(cancellationToken => RemoveItemAsync(layout.SlotId, cancellationToken)),
            new AsyncRelayCommand(cancellationToken => MoveItemAsync(layout.SlotId, -1, cancellationToken)),
            new AsyncRelayCommand(cancellationToken => MoveItemAsync(layout.SlotId, 1, cancellationToken)),
            isFastSlotCapable,
            isFastSlotPrepared);
    }

    private bool CanSend(GalleryPageItemLayout layout, GalleryItem item)
    {
        if (IsSending)
        {
            return false;
        }

        return item.Type switch
        {
            GalleryItemType.TextPreset when IsFastSlotPrepared(layout, item) =>
                commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.TextPreset => textTransport.IsReady || faceTransport.IsReady,
            GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation => commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.CustomStaticFace when IsFastSlotPrepared(layout, item) =>
                commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.CustomStaticFace => faceTransport.IsReady,
            GalleryItemType.AppBuiltInAnimation when item.AppAnimation is not null && IsFastSlotPrepared(layout, item) =>
                commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.AppBuiltInAnimation =>
                faceTransport.IsReady && commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.CustomAnimation when item.PerformanceAnimation is not null && IsFastSlotPrepared(layout, item) =>
                commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.CustomAnimation =>
                faceTransport.IsReady && commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.Scene => sceneEngine is not null
                && item.Scene is not null
                && commandTransport.TransportState == MaskCommandTransportState.Ready,
            GalleryItemType.QuickAction => item.QuickActionKind switch
            {
                QuickActionKind.Text or QuickActionKind.Random => textTransport.IsReady,
                QuickActionKind.Command or QuickActionKind.Brightness or QuickActionKind.BuiltInImage or QuickActionKind.BuiltInAnimation =>
                    commandTransport.TransportState == MaskCommandTransportState.Ready,
                _ => false
            },
            _ => false
        };
    }

    private async Task<GalleryActionResult> SendTextPresetAsync(
        TextPreset preset,
        CancellationToken cancellationToken)
    {
        if (!textTransport.IsReady)
        {
            return GalleryActionResult.Failure("Text not ready");
        }

        var result = await textPresetDispatcher.SendAsync(preset, cancellationToken);
        return new GalleryActionResult(result.Succeeded, result.Message);
    }

    private async Task<GalleryActionResult> SendBuiltInAsync(
        BuiltInAssetRecord record,
        CancellationToken cancellationToken)
    {
        if (commandTransport.TransportState != MaskCommandTransportState.Ready)
        {
            return GalleryActionResult.Failure("Connect to send");
        }

        var result = await commandTransport.SendAsync(BuiltInAssetCommandFactory.CreateCommand(record), cancellationToken);
        return result.Succeeded
            ? GalleryActionResult.Success("Sent, confirm on mask")
            : GalleryActionResult.Failure(result.Message);
    }

    private async Task<GalleryActionResult> SendFaceAsync(
        FacePattern pattern,
        CancellationToken cancellationToken)
    {
        var result = await diySlotPlayback.PlayFaceAsync(pattern, cancellationToken);
        await InitializeAsync(cancellationToken);
        return new GalleryActionResult(result.Succeeded, result.Message);
    }

    private static GalleryActionResult ToGalleryActionResult(
        (bool Succeeded, string Message) result) =>
        new(result.Succeeded, result.Message);

    private static GalleryActionResult ToGalleryActionResult(QuickActionResult result) =>
        new(result.Succeeded, result.Message);

    private static GalleryActionResult ToGalleryActionResult(SceneExecutionResult result) =>
        new(result.Succeeded, result.Message);

    private bool IsScenePrepared(PerformanceScene scene)
    {
        var catalog = allItems.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return sceneReadinessEvaluator.Evaluate(scene, catalog, faceState).IsReady;
    }

    private void OnCommandTransportStateChanged(object? sender, MaskCommandTransportStateChangedEventArgs e) =>
        RefreshTransportCommandState();

    private void OnTextTransportStateChanged(object? sender, TextUploadTransportStateChangedEventArgs e) =>
        RefreshTransportCommandState();

    private void OnFaceTransportStateChanged(object? sender, FaceUploadTransportStateChangedEventArgs e) =>
        RefreshTransportCommandState();

    private void RefreshTransportCommandState()
    {
        foreach (var shortcut in Shortcuts)
        {
            shortcut.RefreshCommandState();
        }

        PreparePageCommand.RaiseCanExecuteChanged();
    }

    private void BeginOperation()
    {
        if (Interlocked.Increment(ref activeOperationCount) == 1)
        {
            IsSending = true;
        }
    }

    private void EndOperation()
    {
        if (Interlocked.Decrement(ref activeOperationCount) == 0)
        {
            IsSending = false;
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
