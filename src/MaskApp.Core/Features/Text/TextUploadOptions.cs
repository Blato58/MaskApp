namespace MaskApp.Core.Features.Text;

public sealed record TextUploadOptions
{
    public static TextUploadOptions RequireAcknowledgements { get; } = new()
    {
        PostUploadDelay = TimeSpan.FromMilliseconds(120),
        CommandDelay = TimeSpan.FromMilliseconds(40)
    };

    public static TextUploadOptions WriteOnlyCompatibility { get; } = new()
    {
        AckRequired = false,
        CompatibilityWriteOnly = true,
        DisplayResetDelay = TimeSpan.FromMilliseconds(40),
        InterFrameDelay = TimeSpan.FromMilliseconds(40),
        PostUploadDelay = TimeSpan.FromMilliseconds(150),
        CommandDelay = TimeSpan.FromMilliseconds(40)
    };

    public static TextUploadOptions FastWriteOnly { get; } = new()
    {
        AckRequired = false,
        CompatibilityWriteOnly = true,
        InterFrameDelay = TimeSpan.FromMilliseconds(20),
        PostUploadDelay = TimeSpan.FromMilliseconds(100),
        CommandDelay = TimeSpan.FromMilliseconds(20)
    };

    public bool AckRequired { get; init; } = true;

    public bool CompatibilityWriteOnly { get; init; }

    public bool ResetDisplayBeforeUpload { get; init; } = true;

    public TimeSpan DisplayResetDelay { get; init; } = TimeSpan.FromMilliseconds(20);

    public TimeSpan InterFrameDelay { get; init; } = TimeSpan.Zero;

    public TimeSpan PostUploadDelay { get; init; } = TimeSpan.Zero;

    public TimeSpan CommandDelay { get; init; } = TimeSpan.FromMilliseconds(20);

    public bool RepeatModeAndSpeed { get; init; }

    public bool ForceModeAndSpeed { get; init; } = true;

    public bool StyleCommandsFailSoft { get; init; } = true;

    public TimeSpan PostUploadQuietPeriod { get; init; } = TimeSpan.FromMilliseconds(40);
}
