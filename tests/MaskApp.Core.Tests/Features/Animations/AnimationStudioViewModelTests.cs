using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class AnimationStudioViewModelTests
{
    [Fact]
    public async Task TimelineOperations_SelectDuplicateInsertReorderDeleteAndPersist()
    {
        var store = new InMemoryAnimationProjectStore();
        var viewModel = CreateViewModel(store);
        await viewModel.InitializeAsync();

        viewModel.SetCell(0, 0);
        viewModel.DuplicateSelectedFrame();
        viewModel.InsertBlankFrame();
        viewModel.SetSelectedFrameDuration(TimeSpan.FromMilliseconds(140));
        viewModel.MoveSelectedFrame(-2);
        viewModel.OnionSkinEnabled = true;

        Assert.Equal(3, viewModel.TimelineFrames.Count);
        Assert.Equal(0, viewModel.SelectedFrameIndex);
        Assert.Equal(TimeSpan.FromMilliseconds(140), viewModel.SelectedFrame.Duration);
        Assert.Null(viewModel.OnionSkinPattern);
        Assert.Contains("2/20", viewModel.BudgetText, StringComparison.Ordinal);

        viewModel.MoveSelectedFrame(1);
        Assert.NotNull(viewModel.OnionSkinPattern);
        viewModel.DeleteSelectedFrame();
        Assert.Equal(2, viewModel.TimelineFrames.Count);

        await viewModel.SaveCommand.ExecuteAsync();
        var saved = Assert.Single((await store.LoadAsync()).Projects);
        Assert.Equal(2, saved.Frames.Count);
        Assert.Equal(viewModel.CurrentProject.Id, saved.Id);
    }

    [Fact]
    public async Task ImportIsPreviewOnlyUntilExplicitSave()
    {
        var decoded = AnimationMediaDecodeResult.Success(
            [new AnimationDecodedFrame(SolidImage(4, 4), TimeSpan.FromMilliseconds(100))],
            TimeSpan.FromMilliseconds(100));
        var store = new InMemoryAnimationProjectStore();
        var viewModel = CreateViewModel(store, new FixedDecoder(decoded));
        await viewModel.InitializeAsync();

        var result = await viewModel.ImportMediaAsync(
            new MemoryStream("GIF89a"u8.ToArray()),
            "Preview import",
            AnimationMediaKind.Gif,
            new AnimationMediaConversionOptions(),
            TimeSpan.FromMilliseconds(100));

        Assert.True(result.Succeeded);
        Assert.Equal("Preview import", viewModel.ProjectName);
        Assert.Empty((await store.LoadAsync()).Projects);

        await viewModel.SaveCommand.ExecuteAsync();
        Assert.Equal("Preview import", Assert.Single((await store.LoadAsync()).Projects).DisplayName);
    }

    [Fact]
    public async Task ExactRevisionAcknowledgementIsInvalidatedByFrameTimingChange()
    {
        var project = CreateUnsafeProject();
        var projectStore = new InMemoryAnimationProjectStore(new AnimationProjectStoreState
        {
            Projects = [project]
        });
        var acknowledgementStore = new InMemoryFlashSafetyAcknowledgementStore();
        var viewModel = CreateViewModel(projectStore, acknowledgements: acknowledgementStore);
        await viewModel.InitializeAsync();

        Assert.True(viewModel.IsSafetyBlocked);
        await viewModel.AcknowledgeSafetyCommand.ExecuteAsync();
        Assert.True(viewModel.HasSafetyOverride);

        viewModel.SetSelectedFrameDuration(TimeSpan.FromMilliseconds(110));

        Assert.True(viewModel.IsSafetyBlocked);
        Assert.False(viewModel.HasSafetyOverride);
    }

    [Fact]
    public async Task TapTempoAndFrameEditsUpdateLiveRevisionBudgetAndPreview()
    {
        var viewModel = CreateViewModel(new InMemoryAnimationProjectStore());
        await viewModel.InitializeAsync();
        var initialCell = viewModel.PreviewCells[0];

        viewModel.SetCell(0, 0);
        Assert.False(initialCell.IsLit);
        Assert.True(viewModel.PreviewCells[0].IsLit);
        Assert.Null(viewModel.AddTap(TimeSpan.Zero));
        Assert.Equal(120, viewModel.AddTap(TimeSpan.FromMilliseconds(500)).GetValueOrDefault());
        Assert.Equal(120, viewModel.Bpm);

        var durationAt120 = viewModel.SelectedFrame.Duration;
        viewModel.Bpm = 240;
        Assert.Equal(TimeSpan.FromTicks(durationAt120.Ticks / 2), viewModel.SelectedFrame.Duration);
    }

    private static AnimationStudioViewModel CreateViewModel(
        IAnimationProjectStore projectStore,
        IAnimationMediaDecoder? decoder = null,
        IFlashSafetyAcknowledgementStore? acknowledgements = null)
    {
        var commandTransport = new RecordingCommandTransport();
        var acknowledgementStore = acknowledgements ?? new InMemoryFlashSafetyAcknowledgementStore();
        var analyzer = new FlashSafetyAnalyzer();
        var builder = new PerformanceAnimationBuilder();
        var coordinator = new DiySlotPlaybackCoordinator(
            new InMemoryFacePatternStore(),
            new RecordingFaceTransport(),
            commandTransport,
            new PerformanceAnimationEngine(
                commandTransport,
                flashSafetyAnalyzer: analyzer,
                flashSafetyAcknowledgementStore: acknowledgementStore),
            builder);
        return new AnimationStudioViewModel(
            projectStore,
            new AnimationProjectCompiler(builder),
            coordinator,
            analyzer,
            acknowledgementStore,
            new FlashSafetyAcknowledgementService(acknowledgementStore),
            new AnimationMediaImportService(decoder ?? new UnavailableAnimationMediaDecoder()));
    }

    private static AnimationProject CreateUnsafeProject()
    {
        var black = FacePatternFactory.CreateBlank("Black", 1);
        var white = AnimationProjectCompilerTests.CreateSolidPattern("White");
        return new AnimationProject
        {
            Id = "unsafe-project",
            DisplayName = "Unsafe project",
            Frames = Enumerable.Range(0, 10)
                .Select(index => new AnimationProjectFrame
                {
                    Id = $"frame-{index}",
                    Pattern = index % 2 == 0 ? black : white,
                    Duration = TimeSpan.FromMilliseconds(100)
                })
                .ToArray()
        }.Normalize();
    }

    private static FaceSampleImage SolidImage(int width, int height) => new(
        width,
        height,
        Enumerable.Repeat(new FaceSamplePixel(255, 255, 255), width * height).ToArray());

    private sealed class FixedDecoder(AnimationMediaDecodeResult result) : IAnimationMediaDecoder
    {
        public Task<AnimationMediaDecodeResult> DecodeAsync(
            ReadOnlyMemory<byte> data,
            AnimationMediaDecodeRequest request,
            CancellationToken cancellationToken = default) => Task.FromResult(result);
    }

    private sealed class RecordingFaceTransport : IFaceUploadTransport
    {
        public event EventHandler<FaceUploadTransportStateChangedEventArgs>? StateChanged { add { } remove { } }
        public string TransportDisplayName => "Fake face";
        public bool IsSimulated => true;
        public bool IsReady => true;
        public bool SupportsAcknowledgements => true;
        public FaceUploadTransportState State => FaceUploadTransportState.Ready;
        public string StatusText => "Ready.";

        public Task<FaceUploadResult> UploadAsync(
            FaceUploadPackage package,
            FaceUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(FaceUploadResult.Success("Uploaded.", package.Frames.Count));
    }

    private sealed class RecordingCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged { add { } remove { } }
        public string TransportDisplayName => "Fake command";
        public bool IsSimulated => true;
        public MaskCommandTransportState TransportState => MaskCommandTransportState.Ready;
        public string TransportStatusText => "Ready.";

        public Task<MaskCommandResult> SendAsync(
            MaskCommand command,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Sent."));
    }
}
