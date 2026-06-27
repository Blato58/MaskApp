namespace MaskApp.Core.Features.BuiltIns;

public sealed record BuiltInAssetRecord
{
    public BuiltInAssetRecord()
    {
    }

    public BuiltInAssetRecord(BuiltInAssetType type, int id)
    {
        Type = type;
        Id = BuiltInAssetRange.Clamp(type, id);
        DisplayName = $"{GetDefaultName(type)} {Id}";
    }

    public BuiltInAssetType Type { get; init; }

    public int Id { get; init; }

    public string HexId => BuiltInAssetRange.ToHexId(Id);

    public string DisplayName { get; init; } = string.Empty;

    public string[] Tags { get; init; } = [];

    public string Notes { get; init; } = string.Empty;

    public BuiltInAssetStatus Status { get; init; } = BuiltInAssetStatus.Untested;

    public bool IsFavorite { get; init; }

    public DateTimeOffset? LastTestedAt { get; init; }

    public DateTimeOffset? LastUpdatedAt { get; init; }

    public string LastSendStatus { get; init; } = "Never sent";

    public BuiltInAssetRecord Normalize()
    {
        var clampedId = BuiltInAssetRange.Clamp(Type, Id);
        var displayName = string.IsNullOrWhiteSpace(DisplayName)
            ? $"{GetDefaultName(Type)} {clampedId}"
            : DisplayName.Trim();

        return this with
        {
            Id = clampedId,
            DisplayName = displayName,
            Tags = Tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim().TrimStart('#'))
                .Where(tag => tag.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Notes = Notes.Trim(),
            LastSendStatus = string.IsNullOrWhiteSpace(LastSendStatus) ? "Never sent" : LastSendStatus.Trim()
        };
    }

    private static string GetDefaultName(BuiltInAssetType type) =>
        type == BuiltInAssetType.StaticImage ? "Image" : "Animation";
}
