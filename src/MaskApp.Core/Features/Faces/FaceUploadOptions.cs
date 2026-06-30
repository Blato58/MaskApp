namespace MaskApp.Core.Features.Faces;

public sealed record FaceUploadOptions
{
    public static FaceUploadOptions RequireAcknowledgements { get; } = new()
    {
        PostUploadDelay = TimeSpan.FromMilliseconds(120),
        CommandDelay = TimeSpan.FromMilliseconds(40)
    };

    public static FaceUploadOptions WriteOnlyCompatibility { get; } = new()
    {
        AckRequired = false,
        CompatibilityWriteOnly = true,
        InterFrameDelay = TimeSpan.FromMilliseconds(40),
        PostUploadDelay = TimeSpan.FromMilliseconds(150),
        CommandDelay = TimeSpan.FromMilliseconds(40)
    };

    public bool AckRequired { get; init; } = true;

    public bool CompatibilityWriteOnly { get; init; }

    public TimeSpan InterFrameDelay { get; init; } = TimeSpan.Zero;

    public TimeSpan PostUploadDelay { get; init; } = TimeSpan.Zero;

    public TimeSpan CommandDelay { get; init; } = TimeSpan.FromMilliseconds(20);

    public TimeSpan PostUploadQuietPeriod { get; init; } = TimeSpan.FromMilliseconds(80);
}
