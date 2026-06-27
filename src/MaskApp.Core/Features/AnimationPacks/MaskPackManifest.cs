namespace MaskApp.Core.Features.AnimationPacks;

public sealed record MaskPackManifest
{
    public int SchemaVersion { get; init; }

    public string PackName { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public MaskPackDisplayGeometry TargetDisplay { get; init; } = new();

    public MaskPackAsset[] Assets { get; init; } = [];
}
