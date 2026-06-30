using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.TextPresets;

public sealed record TextPresetStyle
{
    public static TextPresetStyle Default { get; } = new();

    public TextLedColor ForegroundColor { get; init; } = new(0xFF, 0xFF, 0xFF);

    public TextPresetLayoutMode LayoutMode { get; init; } = TextPresetLayoutMode.FixedWidthCentered;

    public bool IsBold { get; init; }

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
        var fixedWidth = normalized.LayoutMode is TextPresetLayoutMode.FixedWidthCentered or TextPresetLayoutMode.ThreeLineCentered;

        return baseProfile with
        {
            Name = normalized.SendProfile switch
            {
                TextPresetSendProfile.StableFlash => normalized.LayoutMode == TextPresetLayoutMode.ThreeLineCentered
                    ? "Preset Stable 3-line Flash"
                    : "Preset Stable Flash",
                TextPresetSendProfile.ComposerScroll => normalized.LayoutMode == TextPresetLayoutMode.ThreeLineCentered
                    ? "Preset Composer 3-line"
                    : "Preset Composer Scroll",
                _ => normalized.LayoutMode == TextPresetLayoutMode.ThreeLineCentered
                    ? "Preset Low-static 3-line Flash"
                    : "Preset Low-static Flash"
            },
            LayoutMode = normalized.LayoutMode switch
            {
                TextPresetLayoutMode.ThreeLineCentered => TextLayoutMode.ThreeLineCentered,
                TextPresetLayoutMode.FixedWidthCentered => TextLayoutMode.FixedWidthCentered,
                _ => TextLayoutMode.VariableWidth
            },
            FixedWidthColumns = fixedWidth ? QuickCaptionLayout.VisibleColumns : null,
            DisplayMode = normalized.DisplayMode,
            Speed = normalized.Speed,
            TextColor = normalized.ForegroundColor,
            IsBold = normalized.IsBold,
            SendBlackBackgroundReset = normalized.UseBlackBackgroundReset
        };
    }

    public string ForegroundHex => $"#{ForegroundColor.Red:X2}{ForegroundColor.Green:X2}{ForegroundColor.Blue:X2}";

    public string WeightText => IsBold ? "Bold" : "Regular";
}
