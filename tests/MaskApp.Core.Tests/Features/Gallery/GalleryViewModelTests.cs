using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.Gallery;

public sealed class GalleryViewModelTests
{
    [Fact]
    public async Task InitializeAsync_ProjectsTextBuiltInsAndQuickActions()
    {
        var preset = CreatePreset("Gallery unique text", "Gallery Pack", favorite: true);
        var archive = new BuiltInAssetArchive(
        [
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 7)
            {
                DisplayName = "Gallery face",
                IsFavorite = true
            }
        ]);
        var viewModel = CreateViewModel(textPresets: [preset], archive: archive);

        await viewModel.InitializeAsync();

        var items = Flatten(viewModel);
        Assert.Contains(items, item => item.Item.Id == $"text:{preset.Id.Value}" && item.IsFavorite);
        Assert.Contains(items, item => item.Item.Id == "built-in:StaticImage:7");
        Assert.Contains(items, item => item.Item.Id == $"quick:{QuickActionId.Lol}");
    }

    [Fact]
    public async Task SearchFavoritesAndGrouping_FilterProjectedItems()
    {
        var favorite = CreatePreset("Gallery favorite only", "Favorite Pack", favorite: true);
        var normal = CreatePreset("Gallery normal only", "Normal Pack", favorite: false);
        var viewModel = CreateViewModel(textPresets: [favorite, normal]);

        await viewModel.InitializeAsync();
        viewModel.SearchText = "Gallery";
        viewModel.ShowFavoritesOnly = true;
        viewModel.SelectedGroupingMode = viewModel.GroupingOptions.Single(option => option.Mode == GalleryGroupingMode.FavoritesFirst);

        var groups = viewModel.Groups;
        Assert.Contains(groups, group => group.Title == "Favorites");
        Assert.Contains(Flatten(viewModel), card => card.Title == favorite.DisplayName);
        Assert.DoesNotContain(Flatten(viewModel), card => card.Title == normal.DisplayName);
    }

    [Fact]
    public async Task MoveItemAsync_UpdatesGlobalOrderFromFilteredView()
    {
        var first = CreatePreset("Move target one", "Move Pack");
        var second = CreatePreset("Move target two", "Move Pack");
        var layoutStore = new RecordingGalleryLayoutStore();
        var viewModel = CreateViewModel(textPresets: [first, second], layoutStore: layoutStore);

        await viewModel.InitializeAsync();
        viewModel.SearchText = "Move target";
        var cards = Flatten(viewModel)
            .Where(card => card.Title.StartsWith("Move target", StringComparison.Ordinal))
            .ToArray();

        await cards[1].MoveEarlierCommand.ExecuteAsync();

        var saved = layoutStore.State.Order;
        Assert.True(
            saved.GetItemSortIndex(cards[1].Id, int.MaxValue) <
            saved.GetItemSortIndex(cards[0].Id, int.MaxValue));
    }

    [Fact]
    public async Task IsEditMode_RebuildsCardsIntoEditState()
    {
        var preset = CreatePreset("Editable gallery item", "Edit Pack");
        var viewModel = CreateViewModel(textPresets: [preset]);

        await viewModel.InitializeAsync();
        var normalCard = Flatten(viewModel).Single(card => card.Id == $"text:{preset.Id.Value}");

        viewModel.IsEditMode = true;
        var editCard = Flatten(viewModel).Single(card => card.Id == $"text:{preset.Id.Value}");

        Assert.True(normalCard.IsNormalMode);
        Assert.False(normalCard.IsEditMode);
        Assert.True(editCard.IsEditMode);
        Assert.False(editCard.IsNormalMode);
    }

    [Fact]
    public async Task ModeAndSheets_ExposeLibraryConceptState()
    {
        var preset = CreatePreset("Sheet managed item", "Concept Pack", favorite: true);
        var viewModel = CreateViewModel(textPresets: [preset]);

        await viewModel.InitializeAsync();
        Assert.True(viewModel.IsBrowseMode);
        Assert.Equal("Browse", viewModel.GalleryModeText);

        await viewModel.SetArrangeModeCommand.ExecuteAsync();

        Assert.True(viewModel.IsArrangeMode);
        Assert.Equal("Arrange", viewModel.GalleryModeText);

        await viewModel.ToggleFilterSheetCommand.ExecuteAsync();
        Assert.True(viewModel.IsFilterSheetVisible);
        await viewModel.ShowFavoritesCommand.ExecuteAsync();
        Assert.True(viewModel.ShowFavoritesOnly);
        Assert.Equal("Favorites only", viewModel.FilterSummaryText);
        Assert.False(viewModel.IsFilterSheetVisible);

        var card = Flatten(viewModel).Single(item => item.Id == $"text:{preset.Id.Value}");
        viewModel.OpenManageSheet(card.Item);

        Assert.True(viewModel.IsManageSheetVisible);
        Assert.Equal("Sheet managed item", viewModel.ManagedItemTitle);
        Assert.True(viewModel.ManagedItemCanOpenEditor);
        Assert.Equal("Open Text Composer", viewModel.ManagedItemEditorLabel);
    }

    [Fact]
    public async Task SendAsync_RoutesTextBuiltInAndQuickActionItems()
    {
        var preset = CreatePreset("Send text route", "Send Pack");
        var archive = new BuiltInAssetArchive(
        [
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 3)
            {
                DisplayName = "Send anim",
                Status = BuiltInAssetStatus.Working
            }
        ]);
        var quickDispatcher = new RecordingQuickActionDispatcher();
        var presetDispatcher = new RecordingTextPresetDispatcher();
        var commandTransport = new RecordingCommandTransport();
        var viewModel = CreateViewModel(
            textPresets: [preset],
            archive: archive,
            quickDispatcher: quickDispatcher,
            presetDispatcher: presetDispatcher,
            commandTransport: commandTransport);

        await viewModel.InitializeAsync();
        var text = Flatten(viewModel).Single(card => card.Id == $"text:{preset.Id.Value}");
        var builtIn = Flatten(viewModel).Single(card => card.Id == "built-in:Animation:3");
        var quick = Flatten(viewModel).Single(card => card.Id == $"quick:{QuickActionId.Lol}");

        await text.SendCommand.ExecuteAsync();
        await builtIn.SendCommand.ExecuteAsync();
        await quick.SendCommand.ExecuteAsync();

        Assert.Equal(preset.Id, presetDispatcher.LastPresetId);
        Assert.Equal(MaskCommandKind.Animation, Assert.Single(commandTransport.SentCommands).Kind);
        Assert.Equal(QuickActionId.Lol, quickDispatcher.LastActionId);
    }

    private static GalleryViewModel CreateViewModel(
        IReadOnlyList<TextPreset>? textPresets = null,
        BuiltInAssetArchive? archive = null,
        RecordingGalleryLayoutStore? layoutStore = null,
        RecordingQuickActionDispatcher? quickDispatcher = null,
        RecordingTextPresetDispatcher? presetDispatcher = null,
        RecordingCommandTransport? commandTransport = null)
    {
        var textStore = new InMemoryTextPresetStore(new TextPresetStoreState { Presets = textPresets ?? [] });
        var builtInStore = new InMemoryBuiltInAssetArchiveStore(archive ?? BuiltInAssetArchive.Empty);
        var textTransport = new RecordingTextTransport();
        return new GalleryViewModel(
            new QuickActionCatalog(),
            textStore,
            builtInStore,
            layoutStore ?? new RecordingGalleryLayoutStore(),
            quickDispatcher ?? new RecordingQuickActionDispatcher(),
            presetDispatcher ?? new RecordingTextPresetDispatcher(),
            commandTransport ?? new RecordingCommandTransport(),
            textTransport);
    }

    private static GalleryItemCard[] Flatten(GalleryViewModel viewModel) =>
        viewModel.Groups.SelectMany(group => group.Items).ToArray();

    private static TextPreset CreatePreset(string name, string pack, bool favorite = false) =>
        new()
        {
            Id = TextPresetId.NewUserPreset(),
            InputText = name,
            DisplayName = name,
            PackName = pack,
            Category = TextPresetCategory.Custom,
            IsFavorite = favorite
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

    private sealed class RecordingQuickActionDispatcher : IQuickActionDispatcher
    {
        public QuickActionId? LastActionId { get; private set; }

        public Task<QuickActionResult> TriggerAsync(
            QuickActionId actionId,
            QuickActionRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            LastActionId = actionId;
            return Task.FromResult(QuickActionResult.Sent(actionId, "Sent quick action."));
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

    private sealed class RecordingCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

        public string TransportDisplayName => "Recorder";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState { get; private set; } = MaskCommandTransportState.Ready;

        public string TransportStatusText { get; private set; } = "Ready.";

        public List<MaskCommand> SentCommands { get; } = [];

        public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default)
        {
            SentCommands.Add(command);
            return Task.FromResult(MaskCommandResult.Success("Sent."));
        }

        public void RaiseStateChanged(MaskCommandTransportState state, string message)
        {
            TransportState = state;
            TransportStatusText = message;
            TransportStateChanged?.Invoke(this, new MaskCommandTransportStateChangedEventArgs(state, message));
        }
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
