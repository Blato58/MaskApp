namespace MaskApp.Core.Features.Faces;

public sealed record FaceUploadOptions
{
    public static FaceUploadOptions RequireAcknowledgements { get; } = new()
    {
        DeleteSlotBeforeUpload = true,
        PreUploadDeleteDelay = TimeSpan.FromMilliseconds(500),
        DeleteAcknowledgementTimeout = TimeSpan.FromMilliseconds(1500),
        PostUploadDelay = TimeSpan.FromMilliseconds(1000),
        CommandDelay = TimeSpan.FromMilliseconds(40)
    };

    public static FaceUploadOptions WriteOnlyCompatibility { get; } = new()
    {
        AckRequired = false,
        CompatibilityWriteOnly = true,
        DeleteSlotBeforeUpload = true,
        PreUploadDeleteDelay = TimeSpan.FromMilliseconds(500),
        DeleteAcknowledgementTimeout = TimeSpan.FromMilliseconds(1500),
        InterFrameDelay = TimeSpan.FromMilliseconds(40),
        PostUploadDelay = TimeSpan.FromMilliseconds(1000),
        CommandDelay = TimeSpan.FromMilliseconds(40)
    };

    public bool AckRequired { get; init; } = true;

    public bool CompatibilityWriteOnly { get; init; }

    public bool DeleteSlotBeforeUpload { get; init; } = true;

    public bool PlayAfterUpload { get; init; } = true;

    public TimeSpan PreUploadDeleteDelay { get; init; } = TimeSpan.FromMilliseconds(500);

    public TimeSpan DeleteAcknowledgementTimeout { get; init; } = TimeSpan.FromMilliseconds(1500);

    public TimeSpan InterFrameDelay { get; init; } = TimeSpan.Zero;

    public TimeSpan PostUploadDelay { get; init; } = TimeSpan.Zero;

    public TimeSpan CommandDelay { get; init; } = TimeSpan.FromMilliseconds(20);

    public TimeSpan PostUploadQuietPeriod { get; init; } = TimeSpan.FromMilliseconds(80);
}
