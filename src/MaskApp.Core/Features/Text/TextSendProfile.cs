namespace MaskApp.Core.Features.Text;

public sealed record TextSendProfile
{
    public static TextSendProfile QuickFlashLowStatic { get; } = new()
    {
        Name = "Low-static Flash",
        Intent = TextSendIntent.QuickCaption,
        LayoutMode = TextLayoutMode.FixedWidthCentered,
        DisplayMode = TextDisplayMode.Blink,
        Reliability = TextSendReliability.StableAuto,
        Speed = 50,
        FixedWidthColumns = QuickCaptionLayout.VisibleColumns,
        TextColor = new TextLedColor(0xFF, 0xFF, 0xFF),
        InterFrameDelay = TimeSpan.FromMilliseconds(40),
        PostUploadDelay = TimeSpan.FromMilliseconds(20),
        CommandDelay = TimeSpan.FromMilliseconds(20),
        PostUploadQuietPeriod = TimeSpan.FromMilliseconds(40),
        ResetDisplayBeforeUpload = false,
        PreArmModeAndSpeed = true,
        ApplyModeBeforeSpeedAfterUpload = true,
        RepeatModeCommand = true,
        SendBlackBackgroundReset = false
    };

    public static TextSendProfile QuickFlashFast { get; } = new()
    {
        Name = "Fast Flash unstable",
        Intent = TextSendIntent.QuickCaption,
        LayoutMode = TextLayoutMode.FixedWidthCentered,
        DisplayMode = TextDisplayMode.Blink,
        Reliability = TextSendReliability.FastWriteOnly,
        Speed = 50,
        FixedWidthColumns = QuickCaptionLayout.VisibleColumns,
        TextColor = new TextLedColor(0xFF, 0xFF, 0xFF),
        InterFrameDelay = TimeSpan.FromMilliseconds(20),
        PostUploadDelay = TimeSpan.FromMilliseconds(100),
        CommandDelay = TimeSpan.FromMilliseconds(20),
        PostUploadQuietPeriod = TimeSpan.FromMilliseconds(40)
    };

    public static TextSendProfile QuickFlashStable { get; } = new()
    {
        Name = "Stable Flash",
        Intent = TextSendIntent.QuickCaption,
        LayoutMode = TextLayoutMode.FixedWidthCentered,
        DisplayMode = TextDisplayMode.Blink,
        Reliability = TextSendReliability.StableAuto,
        Speed = 50,
        FixedWidthColumns = QuickCaptionLayout.VisibleColumns,
        TextColor = new TextLedColor(0xFF, 0xFF, 0xFF),
        InterFrameDelay = TimeSpan.FromMilliseconds(60),
        PostUploadDelay = TimeSpan.FromMilliseconds(200),
        CommandDelay = TimeSpan.FromMilliseconds(60),
        PostUploadQuietPeriod = TimeSpan.FromMilliseconds(80),
        RepeatModeAndSpeed = true
    };

    public static TextSendProfile ComposerScroll { get; } = new()
    {
        Name = "Composer Scroll",
        Intent = TextSendIntent.Composer,
        LayoutMode = TextLayoutMode.VariableWidth,
        DisplayMode = TextDisplayMode.ScrollRightToLeft,
        Reliability = TextSendReliability.ReliableAcknowledgement,
        Speed = 50,
        TextColor = new TextLedColor(0x52, 0xE3, 0xFF),
        InterFrameDelay = TimeSpan.FromMilliseconds(40),
        PostUploadDelay = TimeSpan.FromMilliseconds(120),
        CommandDelay = TimeSpan.FromMilliseconds(40),
        PostUploadQuietPeriod = TimeSpan.FromMilliseconds(40)
    };

    public static TextSendProfile ComposerCentered { get; } = new()
    {
        Name = "Composer Centered",
        Intent = TextSendIntent.Composer,
        LayoutMode = TextLayoutMode.FixedWidthCentered,
        DisplayMode = TextDisplayMode.Blink,
        Reliability = TextSendReliability.ReliableAcknowledgement,
        Speed = 50,
        FixedWidthColumns = QuickCaptionLayout.VisibleColumns,
        TextColor = new TextLedColor(0x52, 0xE3, 0xFF),
        InterFrameDelay = TimeSpan.FromMilliseconds(40),
        PostUploadDelay = TimeSpan.FromMilliseconds(120),
        CommandDelay = TimeSpan.FromMilliseconds(40),
        PostUploadQuietPeriod = TimeSpan.FromMilliseconds(40)
    };

    public string Name { get; init; } = "Text";

    public TextSendIntent Intent { get; init; }

    public TextLayoutMode LayoutMode { get; init; } = TextLayoutMode.VariableWidth;

    public TextDisplayMode DisplayMode { get; init; } = TextDisplayMode.ScrollRightToLeft;

    public TextSendReliability Reliability { get; init; } = TextSendReliability.ReliableAcknowledgement;

    public int Speed { get; init; } = 50;

    public int? FixedWidthColumns { get; init; }

    public TextLedColor TextColor { get; init; } = new(0xFF, 0xFF, 0xFF);

    public bool BackgroundEnabled { get; init; }

    public TextLedColor? BackgroundColor { get; init; }

    public TextStyleCommandPolicy StyleCommandPolicy { get; init; } = TextStyleCommandPolicy.Skip;

    public TimeSpan InterFrameDelay { get; init; }

    public TimeSpan PostUploadDelay { get; init; }

    public TimeSpan CommandDelay { get; init; } = TimeSpan.FromMilliseconds(20);

    public TimeSpan PostUploadQuietPeriod { get; init; } = TimeSpan.FromMilliseconds(40);

    public bool RepeatModeAndSpeed { get; init; }

    public bool RepeatModeCommand { get; init; }

    public bool ResetDisplayBeforeUpload { get; init; } = true;

    public bool PreArmModeAndSpeed { get; init; }

    public bool ApplyModeBeforeSpeedAfterUpload { get; init; }

    public bool SendBlackBackgroundReset { get; init; } = true;

    public bool UseLargeMtu { get; init; }

    public int ProtocolMode => (int)DisplayMode;

    public TextUploadOptions CreateOptions(bool acknowledgementsAvailable)
    {
        var useAcknowledgements = Reliability switch
        {
            TextSendReliability.ReliableAcknowledgement => true,
            TextSendReliability.StableAuto => acknowledgementsAvailable,
            _ => false
        };

        return new TextUploadOptions
        {
            AckRequired = useAcknowledgements,
            CompatibilityWriteOnly = !useAcknowledgements,
            ResetDisplayBeforeUpload = ResetDisplayBeforeUpload,
            DisplayResetDelay = Reliability == TextSendReliability.FastWriteOnly
                ? TimeSpan.FromMilliseconds(20)
                : TimeSpan.FromMilliseconds(40),
            InterFrameDelay = useAcknowledgements ? TimeSpan.Zero : InterFrameDelay,
            PostUploadDelay = PostUploadDelay,
            CommandDelay = CommandDelay,
            RepeatModeAndSpeed = RepeatModeAndSpeed,
            RepeatModeCommand = RepeatModeCommand,
            ForceModeAndSpeed = true,
            PreArmModeAndSpeed = PreArmModeAndSpeed,
            ApplyModeBeforeSpeedAfterUpload = ApplyModeBeforeSpeedAfterUpload,
            StyleCommandsFailSoft = true,
            PostUploadQuietPeriod = PostUploadQuietPeriod
        };
    }
}
