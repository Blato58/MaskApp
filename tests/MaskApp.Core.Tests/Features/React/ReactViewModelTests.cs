using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.React;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.React;

public sealed class ReactViewModelTests
{
    [Fact]
    public void Constructor_DefaultsFilterToAllGroups()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("All", viewModel.SelectedFilter.Label);
        Assert.Contains(viewModel.Groups, group => group.Category == QuickActionCategory.Meme);
        Assert.Contains(viewModel.Groups, group => group.Category == QuickActionCategory.Social);
        Assert.Contains(viewModel.Groups, group => group.Category == QuickActionCategory.Rave);
        Assert.Contains(viewModel.Groups, group => group.Category == QuickActionCategory.Welfare);
        Assert.Contains(viewModel.FilterOptions, option => option.Category == QuickActionCategory.Meme);
    }

    [Theory]
    [InlineData(QuickActionCategory.Meme)]
    [InlineData(QuickActionCategory.Social)]
    [InlineData(QuickActionCategory.Rave)]
    [InlineData(QuickActionCategory.Welfare)]
    public void SelectedFilter_FiltersVisibleGroupsByCategory(QuickActionCategory category)
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedFilter = viewModel.FilterOptions.Single(option => option.Category == category);

        var group = Assert.Single(viewModel.Groups);
        Assert.Equal(category, group.Category);
    }

    [Fact]
    public async Task SendAsync_Reaction_TriggersDispatcherAndUpdatesStatus()
    {
        var dispatcher = new FakeQuickActionDispatcher(QuickActionResult.Sent(QuickActionId.Lol, "Uploaded."));
        var viewModel = CreateViewModel(dispatcher);
        var card = viewModel.Groups.Single(group => group.Category == QuickActionCategory.Meme)
            .Cards.Single(card => card.Id == QuickActionId.Lol);

        await viewModel.SendAsync(card);

        Assert.Equal(QuickActionId.Lol, dispatcher.LastActionId);
        Assert.Equal("LOL", viewModel.LastActionText);
        Assert.Equal("Uploaded.", viewModel.StatusText);
    }

    [Fact]
    public void TextReactionCommand_IsDisabledWhenTextTransportIsNotReady()
    {
        var viewModel = CreateViewModel(
            textTransport: new FakeTextUploadTransport(IsReady: false, StatusText: "Connect first."));
        var card = viewModel.Groups.Single(group => group.Category == QuickActionCategory.Meme)
            .Cards.Single(card => card.Id == QuickActionId.Lol);

        Assert.False(card.SendCommand.CanExecute(null));
        Assert.Equal("Text not ready", viewModel.TextReadinessText);
    }

    [Fact]
    public void BlackoutCommand_StaysAvailableWhenTextTransportIsNotReady()
    {
        var viewModel = CreateViewModel(
            new FakeQuickActionDispatcher(QuickActionResult.Sent(QuickActionId.Blackout, "Sent.")),
            textTransport: new FakeTextUploadTransport(IsReady: false, StatusText: "Connect first."));
        var blackout = viewModel.PinnedCards.Single(card => card.Id == QuickActionId.Blackout);
        var random = viewModel.PinnedCards.Single(card => card.Id == QuickActionId.RandomReaction);

        Assert.True(blackout.SendCommand.CanExecute(null));
        Assert.False(random.SendCommand.CanExecute(null));
        Assert.Equal("Text not ready", viewModel.ReadinessText);
    }

    [Fact]
    public void BuiltInFallbackCommand_StaysAvailableWhenTextTransportIsNotReady()
    {
        var viewModel = CreateViewModel(
            new FakeQuickActionDispatcher(QuickActionResult.Sent(QuickActionId.TestImage1, "Sent image.")),
            textTransport: new FakeTextUploadTransport(IsReady: false, StatusText: "Connect text first."));
        var card = viewModel.Groups.Single(group => group.Category == QuickActionCategory.BuiltIn)
            .Cards.Single(card => card.Id == QuickActionId.TestImage1);

        Assert.True(card.SendCommand.CanExecute(null));
        Assert.Contains("Needs real-mask test", card.Description);
    }

    [Fact]
    public async Task InitializeArchiveAsync_LoadsFavoriteAndWorkingBuiltIns()
    {
        var archive = new BuiltInAssetArchive(
        [
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 2) { Status = BuiltInAssetStatus.Working },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 3) { IsFavorite = true },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 4) { Status = BuiltInAssetStatus.Bad }
        ]);
        var viewModel = CreateViewModel(archiveStore: new InMemoryBuiltInAssetArchiveStore(archive));

        await viewModel.InitializeArchiveAsync();

        Assert.Equal([3, 2], viewModel.FavoriteBuiltIns.Select(action => action.Record.Id));
        Assert.Contains("Favorite Faces", viewModel.FavoriteBuiltInsHintText);
    }

    [Fact]
    public async Task InitializePresetsAsync_LoadsCzechPresetGroups()
    {
        var viewModel = CreateViewModel();

        await viewModel.InitializePresetsAsync();

        Assert.Contains(viewModel.PresetGroups, group => group.Title == "Czech Basic");
        Assert.Contains(viewModel.PresetGroups, group => group.Title == "Czech Meme");
        Assert.Contains(viewModel.PresetGroups, group => group.Title == "Czech Political/Satire");
        Assert.Contains(viewModel.PresetGroups.SelectMany(group => group.Cards), card => card.MaskText == "AHOJ");
    }

    [Fact]
    public async Task PresetCard_SendsThroughPresetDispatcher()
    {
        var dispatcher = new FakeTextPresetDispatcher();
        var viewModel = CreateViewModel(textPresetDispatcher: dispatcher);
        await viewModel.InitializePresetsAsync();
        var preset = viewModel.PresetGroups.SelectMany(group => group.Cards).First(card => card.MaskText == "AHOJ");

        await preset.SendCommand.ExecuteAsync();

        Assert.Equal(preset.Id, dispatcher.LastPresetId);
        Assert.Equal("Sent preset.", viewModel.StatusText);
        Assert.Equal(preset.DisplayName, viewModel.LastActionText);
    }

    private static ReactViewModel CreateViewModel(
        FakeQuickActionDispatcher? dispatcher = null,
        FakeCommandTransport? commandTransport = null,
        FakeTextUploadTransport? textTransport = null,
        IBuiltInAssetArchiveStore? archiveStore = null,
        ITextPresetStore? textPresetStore = null,
        ITextPresetDispatcher? textPresetDispatcher = null) =>
        new(
            new QuickActionCatalog(),
            dispatcher ?? new FakeQuickActionDispatcher(QuickActionResult.Sent(QuickActionId.Lol, "Uploaded.")),
            commandTransport ?? new FakeCommandTransport(),
            textTransport ?? new FakeTextUploadTransport(),
            archiveStore,
            textPresetStore,
            textPresetDispatcher);

    private sealed class FakeQuickActionDispatcher : IQuickActionDispatcher
    {
        private readonly QuickActionResult result;

        public FakeQuickActionDispatcher(QuickActionResult result)
        {
            this.result = result;
        }

        public QuickActionId? LastActionId { get; private set; }

        public Task<QuickActionResult> TriggerAsync(
            QuickActionId actionId,
            QuickActionRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            LastActionId = actionId;
            return Task.FromResult(result);
        }
    }

    private sealed class FakeCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Fake commands";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState => MaskCommandTransportState.Ready;

        public string TransportStatusText => "Command ready.";

        public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Sent."));
    }

    private sealed class FakeTextPresetDispatcher : ITextPresetDispatcher
    {
        public TextPresetId? LastPresetId { get; private set; }

        public Task<TextPresetDispatchResult> SendAsync(TextPreset preset, CancellationToken cancellationToken = default)
        {
            LastPresetId = preset.Id;
            return Task.FromResult(new TextPresetDispatchResult(true, preset.Id, "Sent preset.", "sent"));
        }
    }

    private sealed class FakeTextUploadTransport : ITextUploadTransport
    {
        public FakeTextUploadTransport(
            bool IsReady = true,
            string StatusText = "Text ready.",
            bool SupportsAcknowledgements = true)
        {
            this.IsReady = IsReady;
            this.StatusText = StatusText;
            this.SupportsAcknowledgements = SupportsAcknowledgements;
        }

        public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Fake text";

        public bool IsSimulated => true;

        public bool IsReady { get; }

        public bool SupportsAcknowledgements { get; }

        public TextUploadTransportState State => IsReady ? TextUploadTransportState.Ready : TextUploadTransportState.Disconnected;

        public string StatusText { get; }

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TextUploadResult.Success("Uploaded.", package.Frames.Count));
    }
}
