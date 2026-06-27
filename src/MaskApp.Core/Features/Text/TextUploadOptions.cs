namespace MaskApp.Core.Features.Text;

public sealed record TextUploadOptions
{
    public static TextUploadOptions RequireAcknowledgements { get; } = new();

    public static TextUploadOptions WriteOnlyCompatibility { get; } = new()
    {
        AckRequired = false,
        CompatibilityWriteOnly = true,
        InterFrameDelay = TimeSpan.FromMilliseconds(40)
    };

    public bool AckRequired { get; init; } = true;

    public bool CompatibilityWriteOnly { get; init; }

    public TimeSpan InterFrameDelay { get; init; } = TimeSpan.Zero;
}
