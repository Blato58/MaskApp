namespace MaskApp.Core.Features.TextPresets;

public sealed record TextPresetStoreState
{
    public const int CurrentSchemaVersion = 1;
    public const int CurrentSeedVersion = 1;

    public static TextPresetStoreState Seeded { get; } = new()
    {
        Presets = TextPresetSeedCatalog.CreateSeedPresets(),
        Status = "Seeded presets loaded."
    };

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public int SeedVersion { get; init; } = CurrentSeedVersion;

    public IReadOnlyList<TextPreset> Presets { get; init; } = [];

    public IReadOnlyList<TextPresetId> DeletedSeedIds { get; init; } = [];

    public string Status { get; init; } = "Ready.";

    public bool UsedFallback { get; init; }

    public TextPresetStoreState Normalize(DateTimeOffset? timestamp = null)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        var deletedSeedIds = DeletedSeedIds
            .Where(id => !string.IsNullOrWhiteSpace(id.Value))
            .Distinct()
            .ToArray();
        var normalizedPresets = Presets
            .Where(preset => !string.IsNullOrWhiteSpace(preset.InputText) || !string.IsNullOrWhiteSpace(preset.MaskText))
            .Select(preset => preset.Normalize(now))
            .GroupBy(preset => preset.Id)
            .Select(group => group.Last())
            .ToArray();

        return this with
        {
            SchemaVersion = CurrentSchemaVersion,
            SeedVersion = CurrentSeedVersion,
            Presets = MergeSeeds(normalizedPresets, deletedSeedIds, now),
            DeletedSeedIds = deletedSeedIds
        };
    }

    public TextPresetStoreState Upsert(TextPreset preset, DateTimeOffset? timestamp = null)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        var normalizedPreset = preset.Normalize(now);
        var presets = Presets
            .Where(existing => existing.Id != normalizedPreset.Id)
            .Append(normalizedPreset)
            .OrderBy(existing => GetSortOrder(existing.Category))
            .ThenBy(existing => existing.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return this with
        {
            Presets = presets,
            Status = "Preset saved."
        };
    }

    public TextPresetStoreState Delete(TextPresetId id)
    {
        var preset = Presets.FirstOrDefault(existing => existing.Id == id);
        var deletedSeedIds = DeletedSeedIds;
        if (preset?.IsSeed == true && !DeletedSeedIds.Contains(id))
        {
            deletedSeedIds = DeletedSeedIds.Append(id).ToArray();
        }

        return this with
        {
            Presets = Presets.Where(existing => existing.Id != id).ToArray(),
            DeletedSeedIds = deletedSeedIds,
            Status = "Preset deleted."
        };
    }

    public TextPresetStoreState MarkSent(TextPresetId id, string status, DateTimeOffset? timestamp = null)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        var presets = Presets
            .Select(preset => preset.Id == id
                ? preset with
                {
                    LastSentAt = now,
                    LastSendStatus = status,
                    UpdatedAt = now
                }
                : preset)
            .ToArray();

        return this with
        {
            Presets = presets,
            Status = status
        };
    }

    private static IReadOnlyList<TextPreset> MergeSeeds(
        IReadOnlyList<TextPreset> existingPresets,
        IReadOnlyList<TextPresetId> deletedSeedIds,
        DateTimeOffset timestamp)
    {
        var byId = existingPresets.ToDictionary(preset => preset.Id);
        foreach (var seed in TextPresetSeedCatalog.CreateSeedPresets(timestamp))
        {
            if (!byId.ContainsKey(seed.Id) && !deletedSeedIds.Contains(seed.Id))
            {
                byId[seed.Id] = seed;
            }
        }

        return byId.Values
            .OrderBy(preset => GetSortOrder(preset.Category))
            .ThenBy(preset => preset.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static int GetSortOrder(TextPresetCategory category) =>
        category switch
        {
            TextPresetCategory.CzechBasic => 0,
            TextPresetCategory.CzechMeme => 1,
            TextPresetCategory.CzechPoliticalSatire => 2,
            TextPresetCategory.CzechRave => 3,
            TextPresetCategory.Custom => 4,
            TextPresetCategory.Legacy => 5,
            _ => 6
        };
}
