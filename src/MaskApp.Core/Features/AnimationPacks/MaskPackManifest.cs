namespace MaskApp.Core.Features.AnimationPacks;

public sealed record MaskPackManifest
{
    public const int CurrentSchemaVersion = 2;

    public int SchemaVersion { get; init; }

    public string PackName { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public MaskPackDisplayGeometry TargetDisplay { get; init; } = new();

    public MaskPackAsset[] Assets { get; init; } = [];

    public MaskPackDisplayGeometry ArtDisplay { get; init; } = new();

    public MaskPackDisplayGeometry TextDisplay { get; init; } = new();

    public MaskPackContentEntry[] Contents { get; init; } = [];
}
