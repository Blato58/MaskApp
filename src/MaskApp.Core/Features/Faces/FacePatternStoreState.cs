namespace MaskApp.Core.Features.Faces;

public sealed record FacePatternStoreState
{
    public const int LegacySchemaVersion = 1;
    public const int CurrentSchemaVersion = 2;
    public const int CurrentSeedVersion = 4;

    public static FacePatternStoreState Seeded => new()
    {
        SchemaVersion = CurrentSchemaVersion,
        SeedVersion = CurrentSeedVersion,
        Patterns = FacePatternFactory.CreateBuiltIns().ToArray(),
        Status = "Seeded pixel face collection ready."
    };

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public int SeedVersion { get; init; } = CurrentSeedVersion;

    public IReadOnlyList<FacePattern> Patterns { get; init; } = [];

    public string Status { get; init; } = "Ready.";

    public bool UsedFallback { get; init; }

    public FacePatternStoreState Normalize()
    {
        var builtIns = FacePatternFactory.CreateBuiltIns()
            .ToDictionary(pattern => pattern.Id, StringComparer.Ordinal);
        var result = builtIns.Values.ToDictionary(pattern => pattern.Id, StringComparer.Ordinal);

        foreach (var pattern in Patterns.Select(pattern => pattern.Normalize()))
        {
            if (pattern.IsBuiltIn && builtIns.TryGetValue(pattern.Id, out var builtIn))
            {
                result[pattern.Id] = builtIn with
                {
                    IsFavorite = pattern.IsFavorite,
                    PreferredSlot = pattern.PreferredSlot,
                    LastUploadedAt = pattern.LastUploadedAt,
                    LastUploadStatus = pattern.LastUploadStatus
                };
                continue;
            }

            result[pattern.Id] = pattern;
        }

        return this with
        {
            SchemaVersion = CurrentSchemaVersion,
            SeedVersion = CurrentSeedVersion,
            Patterns = result.Values
                .OrderBy(pattern => pattern.Source == FacePatternSource.BuiltIn ? 0 : 1)
                .ThenBy(pattern => pattern.PreferredSlot)
                .ThenBy(pattern => pattern.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToArray()
        };
    }

    public FacePatternStoreState MarkUploaded(string patternId, string status, DateTimeOffset timestamp)
    {
        var patterns = Normalize().Patterns
            .Select(pattern => string.Equals(pattern.Id, patternId, StringComparison.Ordinal)
                ? pattern with
                {
                    LastUploadedAt = timestamp,
                    LastUploadStatus = status,
                    UpdatedAt = timestamp
                }
                : pattern)
            .ToArray();
        return this with { Patterns = patterns };
    }

    public int NextCustomSlot()
    {
        var usedSlots = Normalize().Patterns.Select(pattern => pattern.PreferredSlot).ToHashSet();
        for (var slot = 7; slot <= FacePattern.MaxSlot; slot++)
        {
            if (!usedSlots.Contains(slot))
            {
                return slot;
            }
        }

        return FacePattern.MaxSlot;
    }
}
