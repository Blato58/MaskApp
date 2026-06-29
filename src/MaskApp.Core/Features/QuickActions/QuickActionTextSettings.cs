namespace MaskApp.Core.Features.QuickActions;

public sealed record QuickActionTextSettings
{
    public static QuickActionTextSettings RaveDefaults { get; } = new();

    public QuickCaptionDisplayMode DisplayMode { get; init; } = QuickCaptionDisplayMode.FlashBlink;

    public int Speed { get; init; } = 100;

    public QuickCaptionSendMode SendMode { get; init; } = QuickCaptionSendMode.StableFlash;

    public bool BackgroundEnabled { get; init; }

    public QuickCaptionBackgroundPreset BackgroundPreset { get; init; } = QuickCaptionBackgroundPreset.RavePurple;

    public QuickActionTextSettings Normalize() =>
        this with
        {
            Speed = Math.Clamp(Speed, 1, 100)
        };

    public int ProtocolMode => DisplayMode switch
    {
        QuickCaptionDisplayMode.FlashBlink => 2,
        QuickCaptionDisplayMode.ScrollRightToLeft => 3,
        QuickCaptionDisplayMode.ScrollLeftToRight => 4,
        _ => 2
    };
}
