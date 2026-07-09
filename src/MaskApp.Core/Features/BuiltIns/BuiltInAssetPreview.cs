namespace MaskApp.Core.Features.BuiltIns;

public sealed record BuiltInAssetPreview
{
    public static BuiltInAssetPreview Empty { get; } = new(string.Empty, 52, 64, false, 0, 0, "Unavailable");

    public BuiltInAssetPreview(
        string resourceName,
        int width,
        int height,
        bool isAnimated,
        int frameCount,
        int frameDurationMilliseconds,
        string provenance)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegative(frameCount);
        ArgumentOutOfRangeException.ThrowIfNegative(frameDurationMilliseconds);

        ResourceName = resourceName;
        Width = width;
        Height = height;
        IsAnimated = isAnimated;
        FrameCount = frameCount;
        FrameDurationMilliseconds = frameDurationMilliseconds;
        Provenance = provenance;
    }

    public string ResourceName { get; }

    public int Width { get; }

    public int Height { get; }

    public bool IsAnimated { get; }

    public int FrameCount { get; }

    public int FrameDurationMilliseconds { get; }

    public string Provenance { get; }

    public bool IsAvailable => !string.IsNullOrWhiteSpace(ResourceName);

    public string BadgeText => IsAnimated ? $"{FrameCount} frames" : "Stock preview";
}
