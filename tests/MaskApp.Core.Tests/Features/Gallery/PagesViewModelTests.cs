using MaskApp.Core.Features.BuiltIns;
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
        var available = viewModel.AvailableItems.Single(item => item.Item.Id == $"text:{preset.Id.Value}");

        await available.AddCommand.ExecuteAsync();

        var shortcut = Assert.Single(viewModel.Shortcuts, item => item.Item.Id == available.Item.Id);
        Assert.DoesNotContain(viewModel.AvailableItems, item => item.Item.Id == available.Item.Id);

        await shortcut.RemoveCommand.ExecuteAsync();

        Assert.DoesNotContain(viewModel.Shortcuts, item => item.Item.Id == available.Item.Id);
        Assert.Contains(viewModel.AvailableItems, item => item.Item.Id == available.Item.Id);
    }

    [Fact]
    public async Task MoveShortcutAndCycleIconColor_PersistsPageLayout()
    {
        var first = CreatePreset("Shortcut one");
        var second = CreatePreset("Shortcut two");
        var store = new RecordingGalleryLayoutStore();
        var viewModel = CreateViewModel(textPresets: [first, second], layoutStore: store);

        await viewModel.InitializeAsync();
        await viewModel.AvailableItems.Single(item => item.Item.Id == $"text:{first.Id.Value}").AddCommand.ExecuteAsync();
        await viewModel.AvailableItems.Single(item => item.Item.Id == $"text:{second.Id.Value}").AddCommand.ExecuteAsync();
        var secondShortcut = viewModel.Shortcuts.Single(item => item.Item.Id == $"text:{second.Id.Value}");
        var originalColor = secondShortcut.ColorHex;

        await secondShortcut.MoveEarlierCommand.ExecuteAsync();
        await secondShortcut.CycleIconCommand.ExecuteAsync();
        await secondShortcut.CycleColorCommand.ExecuteAsync();

        var savedPage = store.State.Pages.Single(page => page.PageId == viewModel.SelectedPage.PageId);
        var savedSecond = savedPage.Items.Single(item => item.GalleryItemId == $"text:{second.Id.Value}");
        var savedFirst = savedPage.Items.Single(item => item.GalleryItemId == $"text:{first.Id.Value}");
        Assert.True(savedSecond.SortIndex < savedFirst.SortIndex);
        Assert.NotEqual("txt", savedSecond.IconKey);
        Assert.NotEqual(originalColor, savedSecond.ColorHex);
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

        await viewModel.ToggleAddItemsSheetCommand.ExecuteAsync();
        Assert.True(viewModel.IsAddItemsSheetVisible);
        Assert.False(viewModel.IsPageEditorSheetVisible);

        await viewModel.TogglePageEditorSheetCommand.ExecuteAsync();
        Assert.True(viewModel.IsPageEditorSheetVisible);
        Assert.False(viewModel.IsAddItemsSheetVisible);

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
        await viewModel.AvailableItems.Single(item => item.Item.Id == $"text:{preset.Id.Value}").AddCommand.ExecuteAsync();
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
        await viewModel.AvailableItems.Single(item => item.Item.Id == $"text:{preset.Id.Value}").AddCommand.ExecuteAsync();
        await Assert.Single(viewModel.Shortcuts).SendCommand.ExecuteAsync();

        Assert.Equal(preset.Id, dispatcher.LastPresetId);
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
            layoutStore ?? new RecordingGalleryLayoutStore(),
            new RecordingQuickActionDispatcher(),
            presetDispatcher ?? new RecordingTextPresetDispatcher(),
            new RecordingCommandTransport(),
            new RecordingTextTransport());
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
}
