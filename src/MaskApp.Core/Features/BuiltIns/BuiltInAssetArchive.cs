namespace MaskApp.Core.Features.BuiltIns;

public sealed class BuiltInAssetArchive
{
    private readonly List<BuiltInAssetRecord> records;

    public BuiltInAssetArchive()
        : this([])
    {
    }

    public BuiltInAssetArchive(IEnumerable<BuiltInAssetRecord> records)
    {
        this.records = records
            .Select(record => record.Normalize())
            .GroupBy(record => (record.Type, record.Id))
            .Select(group => group.Last())
            .OrderBy(record => record.Type)
            .ThenBy(record => record.Id)
            .ToList();
    }

    public static BuiltInAssetArchive Empty { get; } = new();

    public IReadOnlyList<BuiltInAssetRecord> Records => records;

    public BuiltInAssetRecord GetOrCreate(BuiltInAssetType type, int id)
    {
        var clampedId = BuiltInAssetRange.Clamp(type, id);
        return records.FirstOrDefault(record => record.Type == type && record.Id == clampedId)
            ?? new BuiltInAssetRecord(type, clampedId);
    }

    public BuiltInAssetArchive Upsert(BuiltInAssetRecord record)
    {
        var normalized = record.Normalize();
        var updated = records
            .Where(existing => existing.Type != normalized.Type || existing.Id != normalized.Id)
            .Append(normalized);
        return new BuiltInAssetArchive(updated);
    }

    public IReadOnlyList<BuiltInAssetRecord> FavoriteOrTestedRecords() =>
        records
            .Where(record => record.IsFavorite || record.Status != BuiltInAssetStatus.Untested || record.LastTestedAt is not null)
            .OrderByDescending(record => record.IsFavorite)
            .ThenBy(record => record.Type)
            .ThenBy(record => record.Id)
            .ToArray();

    public IReadOnlyList<BuiltInAssetRecord> FavoriteDeckRecords() =>
        records
            .Where(record => IsFavorite(record) || record.Status == BuiltInAssetStatus.Working)
            .OrderByDescending(IsFavorite)
            .ThenByDescending(record => record.Status == BuiltInAssetStatus.Working)
            .ThenByDescending(record => record.LastTestedAt ?? record.LastUpdatedAt ?? DateTimeOffset.MinValue)
            .ThenBy(record => record.Type)
            .ThenBy(record => record.Id)
            .ToArray();

    public IReadOnlyList<BuiltInAssetRecord> FavoriteRecords() =>
        records
            .Where(IsFavorite)
            .OrderBy(record => record.Type)
            .ThenBy(record => record.Id)
            .ToArray();

    private static bool IsFavorite(BuiltInAssetRecord record) =>
        record.IsFavorite || record.Status == BuiltInAssetStatus.Favorite;
}
