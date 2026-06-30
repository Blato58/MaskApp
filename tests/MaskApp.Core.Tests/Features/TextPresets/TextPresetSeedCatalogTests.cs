using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.TextPresets;

public sealed class TextPresetSeedCatalogTests
{
    [Fact]
    public void CreateSeedPacks_ContainsRequiredCzechPacks()
    {
        var packs = TextPresetSeedCatalog.CreateSeedPacks();

        Assert.Contains(packs, pack => pack.Name == "Czech Basic" && pack.Presets.Any(preset => preset.MaskText == "AHOJ"));
        Assert.Contains(packs, pack => pack.Name == "Czech Meme" && pack.Presets.Any(preset => preset.MaskText == "TY VOLE"));
        Assert.Contains(packs, pack => pack.Name == "Czech Political/Satire" && pack.Presets.Any(preset => preset.MaskText == "SLIBY CHYBY"));
        Assert.Contains(packs, pack => pack.Name == "Czech RAVE" && pack.Presets.Any(preset => preset.MaskText == "KDE JE VODA"));
    }

    [Fact]
    public void Normalize_DoesNotDuplicateSeeds()
    {
        var seeds = TextPresetSeedCatalog.CreateSeedPresets();
        var state = new TextPresetStoreState
        {
            Presets = seeds.Concat(seeds).ToArray()
        }.Normalize();

        Assert.Equal(state.Presets.Select(preset => preset.Id).Distinct().Count(), state.Presets.Count);
    }

    [Fact]
    public void DeleteSeed_PreventsReseedOnNormalize()
    {
        var seed = TextPresetSeedCatalog.CreateSeedPresets().First(preset => preset.MaskText == "AHOJ");
        var state = TextPresetStoreState.Seeded.Delete(seed.Id).Normalize();

        Assert.DoesNotContain(state.Presets, preset => preset.Id == seed.Id);
        Assert.Contains(seed.Id, state.DeletedSeedIds);
    }
}
