using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.QuickActions;

public static class QuickCaptionForegroundPalette
{
    public static TextLedColor GetColor(QuickCaptionForegroundPreset preset) =>
        preset switch
        {
            QuickCaptionForegroundPreset.Cyan => new TextLedColor(0x52, 0xE3, 0xFF),
            QuickCaptionForegroundPreset.Pink => new TextLedColor(0xF4, 0x72, 0xB6),
            QuickCaptionForegroundPreset.Amber => new TextLedColor(0xFA, 0xCC, 0x15),
            QuickCaptionForegroundPreset.Green => new TextLedColor(0x22, 0xC5, 0x5E),
            QuickCaptionForegroundPreset.Red => new TextLedColor(0xEF, 0x44, 0x44),
            QuickCaptionForegroundPreset.Purple => new TextLedColor(0xA8, 0x55, 0xF7),
            _ => new TextLedColor(0xFF, 0xFF, 0xFF)
        };

    public static string GetHex(QuickCaptionForegroundPreset preset) =>
        preset switch
        {
            QuickCaptionForegroundPreset.Cyan => "#52E3FF",
            QuickCaptionForegroundPreset.Pink => "#F472B6",
            QuickCaptionForegroundPreset.Amber => "#FACC15",
            QuickCaptionForegroundPreset.Green => "#22C55E",
            QuickCaptionForegroundPreset.Red => "#EF4444",
            QuickCaptionForegroundPreset.Purple => "#A855F7",
            _ => "#FFFFFF"
        };
}
