namespace MaskApp.Core.Features.Text;

public sealed record TextUploadOptions
{
    public static TextUploadOptions RequireAcknowledgements { get; } = new();

    public static TextUploadOptions WriteOnlyCompatibility { get; } = new()
    {
        AckRequired = false,
        CompatibilityWriteOnly = true,
        DisplayResetDelay = TimeSpan.FromMilliseconds(40),
        InterFrameDelay = TimeSpan.FromMilliseconds(40)
    };

    public static TextUploadOptions FastWriteOnly { get; } = new()
    {
        AckRequired = false,
        CompatibilityWriteOnly = true,
        InterFrameDelay = TimeSpan.FromMilliseconds(20)
    };

    public bool AckRequired { get; init; } = true;

    public bool CompatibilityWriteOnly { get; init; }

    public bool ResetDisplayBeforeUpload { get; init; } = true;

    public TimeSpan DisplayResetDelay { get; init; } = TimeSpan.FromMilliseconds(20);

    public TimeSpan InterFrameDelay { get; init; } = TimeSpan.Zero;

    public TimeSpan PostUploadQuietPeriod { get; init; } = TimeSpan.FromMilliseconds(40);
}
