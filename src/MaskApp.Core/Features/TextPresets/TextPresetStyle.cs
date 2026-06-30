using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.TextPresets;

public sealed record TextPresetStyle
{
    public static TextPresetStyle Default { get; } = new();

    public TextLedColor ForegroundColor { get; init; } = new(0xFF, 0xFF, 0xFF);

    public TextPresetLayoutMode LayoutMode { get; init; } = TextPresetLayoutMode.FixedWidthCentered;

    public TextDisplayMode DisplayMode { get; init; } = TextDisplayMode.Blink;

    public int Speed { get; init; } = 50;

    public TextPresetSendProfile SendProfile { get; init; } = TextPresetSendProfile.LowStaticFlash;

    public bool UseBlackBackgroundReset { get; init; } = true;

    public string Notes { get; init; } = string.Empty;

    public TextPresetStyle Normalize() =>
        this with
        {
            Speed = Math.Clamp(Speed, 1, 100),
            LayoutMode = Enum.IsDefined(LayoutMode) ? LayoutMode : TextPresetLayoutMode.FixedWidthCentered,
            DisplayMode = Enum.IsDefined(DisplayMode) ? DisplayMode : TextDisplayMode.Blink,
            SendProfile = Enum.IsDefined(SendProfile) ? SendProfile : TextPresetSendProfile.LowStaticFlash
        };

    public TextSendProfile ToTextSendProfile()
    {
        var normalized = Normalize();
        var baseProfile = normalized.SendProfile switch
        {
            TextPresetSendProfile.StableFlash => TextSendProfile.QuickFlashStable,
            TextPresetSendProfile.ComposerScroll => TextSendProfile.ComposerScroll,
            _ => TextSendProfile.QuickFlashLowStatic
        };
        var fixedWidth = normalized.LayoutMode == TextPresetLayoutMode.FixedWidthCentered;

        return baseProfile with
        {
            Name = normalized.SendProfile switch
            {
                TextPresetSendProfile.StableFlash => "Preset Stable Flash",
                TextPresetSendProfile.ComposerScroll => "Preset Composer Scroll",
                _ => "Preset Low-static Flash"
            },
            LayoutMode = fixedWidth ? TextLayoutMode.FixedWidthCentered : TextLayoutMode.VariableWidth,
            FixedWidthColumns = fixedWidth ? QuickCaptionLayout.VisibleColumns : null,
            DisplayMode = normalized.DisplayMode,
            Speed = normalized.Speed,
            TextColor = normalized.ForegroundColor,
            SendBlackBackgroundReset = normalized.UseBlackBackgroundReset
        };
    }

    public string ForegroundHex => $"#{ForegroundColor.Red:X2}{ForegroundColor.Green:X2}{ForegroundColor.Blue:X2}";
}
