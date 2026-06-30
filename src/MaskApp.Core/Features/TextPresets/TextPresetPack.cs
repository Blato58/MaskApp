namespace MaskApp.Core.Features.TextPresets;

public sealed record TextPresetPack(
    string Name,
    TextPresetCategory Category,
    IReadOnlyList<TextPreset> Presets);
