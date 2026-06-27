namespace MaskApp.Core.Features.AnimationPacks;

public sealed record MaskPackFrame
{
    public string Path { get; init; } = string.Empty;

    public int? DurationMs { get; init; }
}
