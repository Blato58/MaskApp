namespace MaskApp.Core.Features.BuiltIns;

public sealed record BuiltInAssetRecord
{
    public BuiltInAssetRecord()
    {
    }

    public BuiltInAssetRecord(BuiltInAssetType type, int id)
    {
        Type = type;
        Id = id;
        DisplayName = BuiltInAssetCatalog.GetDefaultName(type, id);
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
        var displayName = BuiltInAssetCatalog.IsGeneratedDefaultName(Type, Id, DisplayName)
            ? BuiltInAssetCatalog.GetDefaultName(Type, Id)
            : DisplayName.Trim();

        return this with
        {
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
}
