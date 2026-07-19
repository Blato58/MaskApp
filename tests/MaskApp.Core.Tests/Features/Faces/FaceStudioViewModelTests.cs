using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class FaceStudioViewModelTests
{
    [Fact]
    public async Task CanvasStroke_CreatesOneUndoStep_AndNewEditInvalidatesRedo()
    {
        var viewModel = await CreateBlankViewModelAsync();

        viewModel.BeginCanvasInteraction(1, 1);
        viewModel.ContinueCanvasInteraction(2, 1);
        viewModel.ContinueCanvasInteraction(3, 1);
        viewModel.EndCanvasInteraction();

        Assert.True(viewModel.CanUndo);
        viewModel.Undo();
        Assert.False(viewModel.CurrentPattern.GetPixel(1, 1).IsLit);
        Assert.False(viewModel.CurrentPattern.GetPixel(3, 1).IsLit);
        Assert.True(viewModel.CanRedo);

        viewModel.Redo();
        Assert.True(viewModel.CurrentPattern.GetPixel(1, 1).IsLit);
        Assert.True(viewModel.CurrentPattern.GetPixel(3, 1).IsLit);

        viewModel.Undo();
        viewModel.SetCell(8, 8);
        Assert.False(viewModel.CanRedo);
    }

    [Fact]
    public async Task Symmetry_DrawsAcrossBothAxes_AndReportsStateWithoutColorOnlyCues()
    {
        var viewModel = await CreateBlankViewModelAsync();
        viewModel.IsHorizontalSymmetryEnabled = true;
        viewModel.IsVerticalSymmetryEnabled = true;

        viewModel.BeginCanvasInteraction(2, 3);
        viewModel.EndCanvasInteraction();

        Assert.True(viewModel.CurrentPattern.GetPixel(2, 3).IsLit);
        Assert.True(viewModel.CurrentPattern.GetPixel(43, 3).IsLit);
        Assert.True(viewModel.CurrentPattern.GetPixel(2, 54).IsLit);
        Assert.True(viewModel.CurrentPattern.GetPixel(43, 54).IsLit);
        Assert.Contains("Active tool: Draw", viewModel.EditorStateText, StringComparison.Ordinal);
        Assert.Contains("Horizontal symmetry on", viewModel.EditorStateText, StringComparison.Ordinal);
        Assert.Contains("Vertical symmetry on", viewModel.EditorStateText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FloodFill_StopsAtColorBoundary_AndNoOpDoesNotAddHistory()
    {
        var viewModel = await CreateBlankViewModelAsync();
        viewModel.SelectColor("White");
        viewModel.BeginCanvasInteraction(1, 0);
        for (var row = 1; row < FacePattern.Height; row++)
        {
            viewModel.ContinueCanvasInteraction(1, row);
        }

        viewModel.EndCanvasInteraction();
        await viewModel.SelectFillToolCommand.ExecuteAsync();
        viewModel.SelectColor("Red");
        viewModel.SetCell(0, 0);

        Assert.Equal("#EF4444", viewModel.CurrentPattern.GetPixel(0, 57).Color.Hex);
        Assert.Equal("#FFFFFF", viewModel.CurrentPattern.GetPixel(1, 57).Color.Hex);
        Assert.False(viewModel.CurrentPattern.GetPixel(2, 57).IsLit);

        viewModel.SetCell(0, 0);
        viewModel.Undo();

        Assert.False(viewModel.CurrentPattern.GetPixel(0, 57).IsLit);
        Assert.True(viewModel.CurrentPattern.GetPixel(1, 57).IsLit);
    }

    [Fact]
    public async Task ConnectedSelectionMove_ClampsToCanvasBounds()
    {
        var viewModel = await CreateBlankViewModelAsync();
        viewModel.BeginCanvasInteraction(43, 55);
        viewModel.ContinueCanvasInteraction(44, 55);
        viewModel.EndCanvasInteraction();
        await viewModel.SelectMoveToolCommand.ExecuteAsync();

        viewModel.BeginCanvasInteraction(43, 55);
        viewModel.ContinueCanvasInteraction(50, 60);
        viewModel.EndCanvasInteraction();

        Assert.False(viewModel.CurrentPattern.GetPixel(43, 55).IsLit);
        Assert.True(viewModel.CurrentPattern.GetPixel(44, 57).IsLit);
        Assert.True(viewModel.CurrentPattern.GetPixel(45, 57).IsLit);
        Assert.Equal(new FaceSelectionBounds(44, 57, 45, 57), viewModel.SelectionBounds);
    }

    [Fact]
    public async Task SavedPalette_PersistsAndReloadsThroughCurrentSchema()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"maskapp-face-studio-{Guid.NewGuid():N}.json");
        var store = new JsonFacePatternStoreCore(filePath);

        try
        {
            var viewModel = await CreateBlankViewModelAsync(store);
            viewModel.SelectColor("Red");
            viewModel.SetCell(1, 1);
            viewModel.SelectColor("Cyan");
            viewModel.SetCell(2, 1);
            viewModel.PaletteName = "Festival";
            await viewModel.SavePaletteCommand.ExecuteAsync();

            var reloaded = new FaceStudioViewModel(store, new SimulatedFaceUploadTransport());
            await reloaded.InitializeAsync();
            var palette = Assert.Single(reloaded.SavedPalettes);
            Assert.Equal("Festival", palette.DisplayName);
            Assert.Contains(new FaceColor(0xEF, 0x44, 0x44), palette.Colors);
            Assert.Contains(new FaceColor(0x52, 0xE3, 0xFF), palette.Colors);

            reloaded.SelectedPalette = palette;
            Assert.Equal(palette.Colors.Length, reloaded.ActiveColorOptions.Count);
            Assert.Contains("Festival active", reloaded.PaletteStatusText, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(filePath);
            File.Delete($"{filePath}.tmp");
        }
    }

    private static async Task<FaceStudioViewModel> CreateBlankViewModelAsync(IFacePatternStore? store = null)
    {
        var viewModel = new FaceStudioViewModel(
            store ?? new InMemoryFacePatternStore(),
            new SimulatedFaceUploadTransport());
        await viewModel.InitializeAsync();
        await viewModel.NewBlankCommand.ExecuteAsync();
        return viewModel;
    }
}
