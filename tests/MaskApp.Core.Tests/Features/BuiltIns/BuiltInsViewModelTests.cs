using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Tests.Features.BuiltIns;

public sealed class BuiltInsViewModelTests
{
    [Fact]
    public async Task SendCommand_DefaultMode_SendsImageCommand()
    {
        var transport = new RecordingCommandTransport();
        var viewModel = new BuiltInsViewModel(transport)
        {
            CurrentId = 2
        };

        await viewModel.SendCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(MaskCommandKind.Image, command.Kind);
        Assert.Equal(2, command.Plaintext.Span[5]);
        Assert.Equal("Sent, confirm on mask", viewModel.StatusText);
    }

    [Fact]
    public async Task SendCommand_AnimationMode_SendsAnimationCommand()
    {
        var transport = new RecordingCommandTransport();
        var viewModel = new BuiltInsViewModel(transport);

        await viewModel.SelectAnimationCommand.ExecuteAsync();
        viewModel.CurrentId = 5;
        await viewModel.SendCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(MaskCommandKind.Animation, command.Kind);
        Assert.Equal(5, command.Plaintext.Span[5]);
        Assert.Equal("0x05", viewModel.CurrentHexId);
        Assert.Contains("ANIM", viewModel.RangeNote);
    }

    [Fact]
    public async Task NextCommand_AnimationMode_SkipsAndroidExcludedIdFour()
    {
        var transport = new RecordingCommandTransport();
        var viewModel = new BuiltInsViewModel(transport);

        await viewModel.SelectAnimationCommand.ExecuteAsync();
        viewModel.CurrentId = 3;
        await viewModel.NextCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(5, viewModel.CurrentId);
        Assert.Equal(MaskCommandKind.Animation, command.Kind);
        Assert.Equal(5, command.Plaintext.Span[5]);
    }

    [Fact]
    public async Task ScannerMetadata_UsesCatalogCountsAndPreviews()
    {
        var viewModel = new BuiltInsViewModel(new RecordingCommandTransport());

        await viewModel.InitializeAsync();

        Assert.Equal(70, viewModel.AvailableIds.Count);
        Assert.Equal("70 Android static images", viewModel.CatalogCountText);
        Assert.Equal("Android data / 1 frame", viewModel.CurrentPreviewBadgeText);
        Assert.False(string.IsNullOrWhiteSpace(viewModel.CurrentPreviewText));

        await viewModel.SelectAnimationCommand.ExecuteAsync();

        Assert.Equal(45, viewModel.AvailableIds.Count);
        Assert.DoesNotContain(4, viewModel.AvailableIds);
        Assert.Equal("45 Android animations", viewModel.CatalogCountText);
    }

    [Fact]
    public async Task BlackoutCommand_SendsLightOne()
    {
        var transport = new RecordingCommandTransport();
        var viewModel = new BuiltInsViewModel(transport);

        await viewModel.BlackoutCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(MaskCommandKind.Brightness, command.Kind);
        Assert.Equal(1, command.Plaintext.Span[6]);
    }

    [Fact]
    public async Task SendCommand_NotReady_DoesNotSend()
    {
        var transport = new RecordingCommandTransport
        {
            TransportState = MaskCommandTransportState.Disconnected,
            TransportStatusText = "Connect first."
        };
        var viewModel = new BuiltInsViewModel(transport);

        await viewModel.SendCommand.ExecuteAsync();

        Assert.Empty(transport.SentCommands);
        Assert.Equal("Connect to send", viewModel.StatusText);
    }

    [Fact]
    public async Task MarkWorkingCommand_CreatesRecordAutosavesAndUpdatesDeck()
    {
        var store = new RecordingArchiveStore();
        var viewModel = new BuiltInsViewModel(new RecordingCommandTransport(), store)
        {
            CurrentId = 7
        };

        await viewModel.InitializeAsync();
        await viewModel.MarkWorkingCommand.ExecuteAsync();

        Assert.Equal(1, store.SaveCount);
        var record = Assert.Single(store.Archive.Records);
        Assert.Equal(7, record.Id);
        Assert.Equal(BuiltInAssetStatus.Working, record.Status);
        Assert.Single(viewModel.FavoriteFaces);
        Assert.Equal("Ready", viewModel.StatusText);
    }

    [Fact]
    public async Task ToggleFavoriteCommand_AutosavesThroughStore()
    {
        var store = new RecordingArchiveStore();
        var viewModel = new BuiltInsViewModel(new RecordingCommandTransport(), store);

        await viewModel.InitializeAsync();
        await viewModel.ToggleFavoriteCommand.ExecuteAsync();

        var record = Assert.Single(store.Archive.Records);
        Assert.True(record.IsFavorite);
        Assert.Equal(1, store.SaveCount);
        Assert.Single(viewModel.FavoriteFaces);
        Assert.Equal("Ready", viewModel.StatusText);
    }

    [Fact]
    public async Task FavoriteFaceSendCommand_UsesImageOrAnimationCommand()
    {
        var archive = new BuiltInAssetArchive(
        [
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 2) { IsFavorite = true },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 3) { Status = BuiltInAssetStatus.Working }
        ]);
        var store = new RecordingArchiveStore(archive);
        var transport = new RecordingCommandTransport();
        var viewModel = new BuiltInsViewModel(transport, store);

        await viewModel.InitializeAsync();
        await viewModel.FavoriteFaces.Single(item => item.Record.Type == BuiltInAssetType.StaticImage)
            .SendCommand.ExecuteAsync();
        await viewModel.FavoriteFaces.Single(item => item.Record.Type == BuiltInAssetType.Animation)
            .SendCommand.ExecuteAsync();

        Assert.Equal(MaskCommandKind.Image, transport.SentCommands[0].Kind);
        Assert.Equal(2, transport.SentCommands[0].Plaintext.Span[5]);
        Assert.Equal(MaskCommandKind.Animation, transport.SentCommands[1].Kind);
        Assert.Equal(3, transport.SentCommands[1].Plaintext.Span[5]);
    }

    [Fact]
    public async Task InitializeAsync_EmptyArchive_ShowsEmptyDeckWithoutCrashing()
    {
        var viewModel = new BuiltInsViewModel(new RecordingCommandTransport(), new RecordingArchiveStore());

        await viewModel.InitializeAsync();

        Assert.Empty(viewModel.FavoriteFaces);
        Assert.True(viewModel.HasNoFavoriteFaces);
        Assert.Contains("Scan built-ins", viewModel.FavoriteFacesHintText);
    }

    private sealed class RecordingCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

        public string TransportDisplayName => "Recorder";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState { get; init; } = MaskCommandTransportState.Ready;

        public string TransportStatusText { get; init; } = "Ready.";

        public List<MaskCommand> SentCommands { get; } = [];

        public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default)
        {
            SentCommands.Add(command);
            return Task.FromResult(MaskCommandResult.Success($"Sent {command.DisplayName}."));
        }

        public void RaiseStateChanged(MaskCommandTransportState state, string message)
        {
            TransportStateChanged?.Invoke(this, new MaskCommandTransportStateChangedEventArgs(state, message));
        }
    }

    private sealed class RecordingArchiveStore : IBuiltInAssetArchiveStore
    {
        public RecordingArchiveStore()
            : this(BuiltInAssetArchive.Empty)
        {
        }

        public RecordingArchiveStore(BuiltInAssetArchive archive)
        {
            Archive = archive;
        }

        public BuiltInAssetArchive Archive { get; private set; }

        public int SaveCount { get; private set; }

        public Task<BuiltInAssetArchive> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Archive);

        public Task SaveAsync(BuiltInAssetArchive archive, CancellationToken cancellationToken = default)
        {
            Archive = archive;
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
