namespace MaskApp.Core.Features.AnimationPacks;

public sealed record MaskPackAsset
{
    public string Id { get; init; } = string.Empty;

    public MaskPackAssetType Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public string[] Tags { get; init; } = [];

    public string Notes { get; init; } = string.Empty;

    public MaskPackFrame[] Frames { get; init; } = [];

    public int FrameDurationMs { get; init; }

    public bool Loop { get; init; }

    public string GeneratedBy { get; init; } = string.Empty;

    public string SourcePrompt { get; init; } = string.Empty;

    public string SafetyNotes { get; init; } = string.Empty;
}
