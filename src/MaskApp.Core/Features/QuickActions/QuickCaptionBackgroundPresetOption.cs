namespace MaskApp.Core.Features.QuickActions;

public sealed record QuickCaptionBackgroundPresetOption(
    string Label,
    QuickCaptionBackgroundPreset Preset,
    string HexColor);
