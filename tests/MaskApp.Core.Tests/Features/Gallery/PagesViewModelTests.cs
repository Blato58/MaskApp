using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.Gallery;

public sealed class PagesViewModelTests
{
    [Fact]
    public async Task AddAndRemoveShortcut_DoesNotDeleteSourceItem()
    {
        var preset = CreatePreset("Page shortcut source");
        var viewModel = CreateViewModel(textPresets: [preset]);

        await viewModel.InitializeAsync();
        var itemId = $"text:{preset.Id.Value}";

        await viewModel.AddItemAsync(itemId, preset.DisplayName, "txt", "#52E3FF");

        var shortcut = Assert.Single(viewModel.Shortcuts, item => item.Item.Id == itemId);
        Assert.DoesNotContain(viewModel.AvailableItems, item => item.Item.Id == itemId);

        await shortcut.RemoveCommand.ExecuteAsync();

        Assert.DoesNotContain(viewModel.Shortcuts, item => item.Item.Id == itemId);
        Assert.Contains(viewModel.AvailableItems, item => item.Item.Id == itemId);
    }

    [Fact]
    public async Task MoveShortcutAndCustomMetadata_PersistsPageLayout()
    {
        var first = CreatePreset("Shortcut one");
        var second = CreatePreset("Shortcut two");
        var store = new RecordingGalleryLayoutStore();
        var viewModel = CreateViewModel(textPresets: [first, second], layoutStore: store);

        await viewModel.InitializeAsync();
        await viewModel.AddItemAsync($"text:{first.Id.Value}", "First", "txt", "#52E3FF");
        await viewModel.AddItemAsync($"text:{second.Id.Value}", "Second custom", "lucide:heart", "#FF3D8B");
        var secondShortcut = viewModel.Shortcuts.Single(item => item.Item.Id == $"text:{second.Id.Value}");

        await secondShortcut.MoveEarlierCommand.ExecuteAsync();

        var savedPage = store.State.Pages.Single(page => page.PageId == viewModel.SelectedPage.PageId);
        var savedSecond = savedPage.Items.Single(item => item.GalleryItemId == $"text:{second.Id.Value}");
        var savedFirst = savedPage.Items.Single(item => item.GalleryItemId == $"text:{first.Id.Value}");
        Assert.True(savedSecond.SortIndex < savedFirst.SortIndex);
        Assert.Equal("Second custom", savedSecond.Label);
        Assert.Equal("lucide:heart", savedSecond.IconKey);
        Assert.Equal("#FF3D8B", savedSecond.ColorHex);
    }

    [Fact]
    public async Task ManageModeAndSheets_ControlPageEditingSurface()
    {
        var viewModel = CreateViewModel();

        await viewModel.InitializeAsync();
        Assert.True(viewModel.IsUseMode);
        Assert.Equal("Use", viewModel.PagesModeText);

        await viewModel.SetManageModeCommand.ExecuteAsync();
        Assert.True(viewModel.IsManageMode);
        Assert.Equal("Manage", viewModel.PagesModeText);

        await viewModel.TogglePageEditorSheetCommand.ExecuteAsync();
        Assert.True(viewModel.IsPageEditorSheetVisible);

        await viewModel.SetUseModeCommand.ExecuteAsync();
        Assert.True(viewModel.IsUseMode);
        Assert.False(viewModel.IsPageEditorSheetVisible);
    }

    [Fact]
    public async Task PageDotsAndSelection_UpdateCurrentPagePosition()
    {
        var viewModel = CreateViewModel();

        await viewModel.InitializeAsync();
        await viewModel.AddPageCommand.ExecuteAsync();

        Assert.Equal(3, viewModel.Pages.Count);
        Assert.Equal("Page 3 of 3", viewModel.PagePositionText);
        Assert.Equal("ON", viewModel.Pages[2].DotText);
        Assert.Equal("OFF", viewModel.Pages[0].DotText);

        await viewModel.Pages[0].SelectCommand.ExecuteAsync();

        Assert.Equal("Page 1 of 3", viewModel.PagePositionText);
        Assert.Equal("ON", viewModel.Pages[0].DotText);
        Assert.Equal("OFF", viewModel.Pages[2].DotText);
    }

    [Fact]
    public async Task RemovePageCommand_RequiresConfirmationAndKeepsLibraryItems()
    {
        var preset = CreatePreset("Delete page source");
        var store = new RecordingGalleryLayoutStore();
        var viewModel = CreateViewModel(textPresets: [preset], layoutStore: store);

        await viewModel.InitializeAsync();
        await viewModel.AddItemAsync($"text:{preset.Id.Value}", preset.DisplayName, "txt", "#52E3FF");
        var originalPageCount = viewModel.Pages.Count;

        await viewModel.RemovePageCommand.ExecuteAsync();

        Assert.True(viewModel.IsDeletePageConfirmationVisible);
        Assert.Equal(originalPageCount, viewModel.Pages.Count);
        Assert.DoesNotContain(viewModel.AvailableItems, item => item.Item.Id == $"text:{preset.Id.Value}");

        await viewModel.ConfirmRemovePageCommand.ExecuteAsync();

        Assert.False(viewModel.IsDeletePageConfirmationVisible);
        Assert.Equal(originalPageCount - 1, viewModel.Pages.Count);
        Assert.Contains(viewModel.AvailableItems, item => item.Item.Id == $"text:{preset.Id.Value}");
    }

    [Fact]
    public async Task SendShortcut_RoutesToUnderlyingItem()
    {
        var preset = CreatePreset("Shortcut send");
        var dispatcher = new RecordingTextPresetDispatcher();
        var viewModel = CreateViewModel(textPresets: [preset], presetDispatcher: dispatcher);

        await viewModel.InitializeAsync();
        await viewModel.AddItemAsync($"text:{preset.Id.Value}", preset.DisplayName, "txt", "#52E3FF");
        await Assert.Single(viewModel.Shortcuts).SendCommand.ExecuteAsync();

        Assert.Equal(preset.Id, dispatcher.LastPresetId);
    }

    [Fact]
    public async Task AddItemDraft_InitializesForSelectedPageAndRequiresGalleryItem()
    {
        var preset = CreatePreset("Draft source");
        var viewModel = CreateAddItemViewModel(textPresets: [preset]);

        await viewModel.InitializeAsync(string.Empty);

        Assert.Equal("Live", viewModel.PageTitle);
        Assert.False(viewModel.CanSave);
        Assert.Contains(viewModel.AvailableItems, item => item.Item.Id == $"text:{preset.Id.Value}");
        Assert.Contains(viewModel.IconPacks, pack => pack.Label == "Lucide");
        Assert.Contains(viewModel.Icons, icon => icon.Pack == "Mask");
    }

    [Fact]
    public async Task AddItemDraft_SelectingFaceExposesPixelPreviewData()
    {
        var viewModel = CreateAddItemViewModel();

        await viewModel.InitializeAsync(string.Empty);
        var face = viewModel.AvailableItems.First(item => item.Item.Type == GalleryItemType.CustomStaticFace);
        viewModel.SelectItem(face.Item.Id);

        Assert.True(face.HasFacePreview);
        Assert.True(face.HasAnyPreview);
        Assert.False(face.HasPreview);
        Assert.Same(face.FacePattern, viewModel.SelectedFacePattern);
        Assert.True(viewModel.SelectedItemHasFacePreview);
        Assert.True(viewModel.SelectedItemHasAnyPreview);
        Assert.False(viewModel.SelectedItemHasPreview);
    }

    [Fact]
    public async Task AddItemDraft_SelectingItemDefaultsDraftAndCustomSavePersists()
    {
        var preset = CreatePreset("Draft custom");
        var store = new RecordingGalleryLayoutStore();
        var viewModel = CreateAddItemViewModel(textPresets: [preset], layoutStore: store);

        await viewModel.InitializeAsync(string.Empty);
        viewModel.SelectItem($"text:{preset.Id.Value}");
        viewModel.DraftLabel = "Custom label";
        viewModel.SelectIconPack("Lucide");
        viewModel.SelectIcon("lucide:heart");
        viewModel.SelectColor("#FF3D8B");

        Assert.True(viewModel.CanSave);
        Assert.Equal("Custom label", viewModel.PreviewLabel);
        Assert.Equal("LOVE", viewModel.PreviewIconLabel);

        var saved = await viewModel.SaveAsync();

        Assert.True(saved);
        var pageItem = store.State.Pages.First().Items.Single(item => item.GalleryItemId == $"text:{preset.Id.Value}");
        Assert.Equal("Custom label", pageItem.Label);
        Assert.Equal("lucide:heart", pageItem.IconKey);
        Assert.Equal("#FF3D8B", pageItem.ColorHex);
    }

    [Fact]
    public async Task AddItemDraft_HidesAndRejectsDuplicateGalleryItem()
    {
        var preset = CreatePreset("Duplicate source");
        var store = new RecordingGalleryLayoutStore();
        var viewModel = CreateAddItemViewModel(textPresets: [preset], layoutStore: store);

        await viewModel.InitializeAsync(string.Empty);
        viewModel.SelectItem($"text:{preset.Id.Value}");
        Assert.True(await viewModel.SaveAsync());

        await viewModel.InitializeAsync(string.Empty);

        Assert.DoesNotContain(viewModel.AvailableItems, item => item.Item.Id == $"text:{preset.Id.Value}");
        viewModel.SelectItem($"text:{preset.Id.Value}");
        Assert.False(await viewModel.SaveAsync());
    }

    private static PagesViewModel CreateViewModel(
        IReadOnlyList<TextPreset>? textPresets = null,
        RecordingGalleryLayoutStore? layoutStore = null,
        RecordingTextPresetDispatcher? presetDispatcher = null)
    {
        return new PagesViewModel(
            new QuickActionCatalog(),
            new InMemoryTextPresetStore(new TextPresetStoreState { Presets = textPresets ?? [] }),
            new InMemoryBuiltInAssetArchiveStore(),
            new InMemoryFacePatternStore(),
            layoutStore ?? new RecordingGalleryLayoutStore(),
            new RecordingQuickActionDispatcher(),
            presetDispatcher ?? new RecordingTextPresetDispatcher(),
            new RecordingCommandTransport(),
            new RecordingTextTransport(),
            new RecordingFaceTransport());
    }

    private static PageAddItemViewModel CreateAddItemViewModel(
        IReadOnlyList<TextPreset>? textPresets = null,
        RecordingGalleryLayoutStore? layoutStore = null)
    {
        return new PageAddItemViewModel(
            new QuickActionCatalog(),
            new InMemoryTextPresetStore(new TextPresetStoreState { Presets = textPresets ?? [] }),
            new InMemoryBuiltInAssetArchiveStore(),
            new InMemoryFacePatternStore(),
            layoutStore ?? new RecordingGalleryLayoutStore());
    }

    private static TextPreset CreatePreset(string name) =>
        new()
        {
            Id = TextPresetId.NewUserPreset(),
            InputText = name,
            DisplayName = name,
            PackName = "Pages Pack",
            Category = TextPresetCategory.Custom
        };

    private sealed class RecordingGalleryLayoutStore : IGalleryLayoutStore
    {
        public GalleryLayoutState State { get; private set; } = new();

        public Task<GalleryLayoutState> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(State);

        public Task SaveAsync(GalleryLayoutState state, CancellationToken cancellationToken = default)
        {
            State = state.Normalize();
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingTextPresetDispatcher : ITextPresetDispatcher
    {
        public TextPresetId? LastPresetId { get; private set; }

        public Task<TextPresetDispatchResult> SendAsync(TextPreset preset, CancellationToken cancellationToken = default)
        {
            LastPresetId = preset.Id;
            return Task.FromResult(new TextPresetDispatchResult(true, preset.Id, "Sent preset.", "sent"));
        }
    }

    private sealed class RecordingQuickActionDispatcher : IQuickActionDispatcher
    {
        public Task<QuickActionResult> TriggerAsync(
            QuickActionId actionId,
            QuickActionRequest? request = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(QuickActionResult.Sent(actionId, "Sent."));
    }

    private sealed class RecordingCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Recorder";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState => MaskCommandTransportState.Ready;

        public string TransportStatusText => "Ready.";

        public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Sent."));
    }

    private sealed class RecordingTextTransport : ITextUploadTransport
    {
        public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Text";

        public bool IsSimulated => true;

        public bool IsReady => true;

        public bool SupportsAcknowledgements => true;

        public TextUploadTransportState State => TextUploadTransportState.Ready;

        public string StatusText => "Ready.";

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TextUploadResult.Success("Sent.", package.Frames.Count));
    }

    private sealed class RecordingFaceTransport : IFaceUploadTransport
    {
        public event EventHandler<FaceUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Face";

        public bool IsSimulated => true;

        public bool IsReady => true;

        public bool SupportsAcknowledgements => true;

        public FaceUploadTransportState State => FaceUploadTransportState.Ready;

        public string StatusText => "Ready.";

        public Task<FaceUploadResult> UploadAsync(
            FaceUploadPackage package,
            FaceUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(FaceUploadResult.Success("Sent.", package.Frames.Count));
    }
}
