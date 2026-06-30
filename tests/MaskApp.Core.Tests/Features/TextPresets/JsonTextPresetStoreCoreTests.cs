using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.TextPresets;

public sealed class JsonTextPresetStoreCoreTests
{
    [Fact]
    public async Task LoadAsync_MissingFile_ReturnsSeededPresets()
    {
        var store = new JsonTextPresetStoreCore(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "text-presets.json"));

        var state = await store.LoadAsync();

        Assert.Contains(state.Presets, preset => preset.PackName == TextPresetSeedCatalog.CzechBasicPackName);
        Assert.False(state.UsedFallback);
    }

    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsUserPreset()
    {
        var filePath = CreateTempFilePath();
        var store = new JsonTextPresetStoreCore(filePath);
        var preset = new TextPreset
        {
            Id = TextPresetId.NewUserPreset(),
            InputText = "ČAU",
            DisplayName = "Pozdrav",
            Category = TextPresetCategory.Custom,
            Style = TextPresetStyle.Default with
            {
                ForegroundColor = new TextLedColor(0xF4, 0x72, 0xB6),
                LayoutMode = TextPresetLayoutMode.ThreeLineCentered,
                IsBold = true
            }
        };

        await store.SaveAsync(TextPresetStoreState.Seeded.Upsert(preset));

        var loaded = await store.LoadAsync();

        var loadedPreset = loaded.Presets.Single(item => item.DisplayName == "Pozdrav");
        Assert.Equal("CAU", loadedPreset.MaskText);
        Assert.Equal(new TextLedColor(0xF4, 0x72, 0xB6), loadedPreset.Style.ForegroundColor);
        Assert.Equal(TextPresetLayoutMode.ThreeLineCentered, loadedPreset.Style.LayoutMode);
        Assert.True(loadedPreset.Style.IsBold);
    }

    [Fact]
    public async Task LoadAsync_CorruptFile_ReturnsSeededFallback()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "{ not json");
        var store = new JsonTextPresetStoreCore(filePath);

        var state = await store.LoadAsync();

        Assert.True(state.UsedFallback);
        Assert.Contains(state.Presets, preset => preset.PackName == TextPresetSeedCatalog.CzechRavePackName);
    }

    private static string CreateTempFilePath() =>
        Path.Combine(Path.GetTempPath(), "maskapp-tests", Guid.NewGuid().ToString("N"), "text-presets.json");
}
