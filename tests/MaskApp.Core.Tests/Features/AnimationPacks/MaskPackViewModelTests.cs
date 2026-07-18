using MaskApp.Core.Features.AnimationPacks;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.AnimationPacks;

public sealed class MaskPackViewModelTests
{
    [Fact]
    public async Task Inspect_ConflictChoiceRequiresExplicitReplaceConfirmation()
    {
        var local = MaskPackArchiveRoundTripTests.CreateFace("face-vm", "Local", 0, new FaceColor(1, 2, 3));
        var incoming = MaskPackArchiveRoundTripTests.CreateFace("face-vm", "Incoming", 1, new FaceColor(4, 5, 6));
        await using var archive = MaskPackConflictAndRecoveryTests.V2Archive(
            MaskPackConflictAndRecoveryTests.Content(
                MaskPackContentType.Face,
                incoming.Id,
                incoming.DisplayName,
                MaskPackPayloadCodec.SerializeFace(incoming)));
        var faces = new InMemoryFacePatternStore(new FacePatternStoreState { Patterns = [local] });
        var service = MaskPackArchiveRoundTripTests.CreateService(
            new MaskApp.Core.Features.TextPresets.InMemoryTextPresetStore(),
            faces,
            new MaskApp.Core.Features.Animations.InMemoryAnimationProjectStore(),
            new MaskApp.Core.Features.Gallery.InMemoryGalleryLayoutStore(),
            new MaskApp.Core.Features.Scenes.InMemorySceneShowStore());
        var viewModel = new MaskPackViewModel(service);

        await viewModel.InspectAsync(archive);

        Assert.True(viewModel.HasValidInspection);
        var choice = Assert.Single(viewModel.Conflicts);
        choice.Resolution = MaskPackConflictResolution.Replace;
        Assert.True(viewModel.RequiresReplaceConfirmation);
        Assert.False(viewModel.ImportCommand.CanExecute(null));

        viewModel.ConfirmReplace = true;
        Assert.True(viewModel.ImportCommand.CanExecute(null));
        await viewModel.ImportCommand.ExecuteAsync();

        Assert.False(viewModel.HasValidInspection);
        Assert.Contains("Import complete", viewModel.InspectionSummary, StringComparison.Ordinal);
        var stored = Assert.Single((await faces.LoadAsync()).Patterns, item => item.Id == incoming.Id);
        Assert.True(stored.GetPixel(1, 0).IsLit);
    }

    [Fact]
    public async Task Inspect_InvalidArchive_LeavesImportUnavailableAndExplainsNoLocalChange()
    {
        var viewModel = new MaskPackViewModel(MaskPackArchiveRoundTripTests.CreateService(
            new MaskApp.Core.Features.TextPresets.InMemoryTextPresetStore(),
            new InMemoryFacePatternStore(),
            new MaskApp.Core.Features.Animations.InMemoryAnimationProjectStore(),
            new MaskApp.Core.Features.Gallery.InMemoryGalleryLayoutStore(),
            new MaskApp.Core.Features.Scenes.InMemorySceneShowStore()));
        await using var archive = new MemoryStream([1, 2, 3]);

        await viewModel.InspectAsync(archive);

        Assert.False(viewModel.HasValidInspection);
        Assert.False(viewModel.ImportCommand.CanExecute(null));
        Assert.Contains("not safe to import", viewModel.InspectionSummary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Local content was not changed", viewModel.StatusText, StringComparison.Ordinal);
    }
}
