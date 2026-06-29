namespace MaskApp.Core.Features.QuickActions;

public sealed record QuickCaptionForegroundPresetOption(
    string Label,
    QuickCaptionForegroundPreset Preset,
    string Hex);
