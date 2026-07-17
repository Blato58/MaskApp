namespace MaskApp.Core.Features.Faces;

public sealed record FacePatternStoreState
{
    public const int LegacySchemaVersion = 1;
    public const int PreviousSchemaVersion = 2;
    public const int CurrentSchemaVersion = 3;
    public const int CurrentSeedVersion = 7;

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

    public IReadOnlyList<FaceSlotInstallation> SlotInstallations { get; init; } = [];

    public string Status { get; init; } = "Ready.";

    public bool UsedFallback { get; init; }

    public FacePatternStoreState Normalize()
    {
        var builtIns = FacePatternFactory.CreateBuiltIns()
            .ToDictionary(pattern => pattern.Id, StringComparer.Ordinal);
        var result = builtIns.Values.ToDictionary(pattern => pattern.Id, StringComparer.Ordinal);

        foreach (var pattern in Patterns.Select(pattern => pattern.Normalize()))
        {
            if (pattern.IsBuiltIn)
            {
                if (builtIns.TryGetValue(pattern.Id, out var builtIn))
                {
                    result[pattern.Id] = builtIn with
                    {
                        IsFavorite = pattern.IsFavorite,
                        PreferredSlot = pattern.PreferredSlot,
                        LastUploadedAt = pattern.LastUploadedAt,
                        LastUploadStatus = pattern.LastUploadStatus
                    };
                }

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
                .ToArray(),
            SlotInstallations = (SlotInstallations ?? [])
                .Where(installation => installation.Slot is >= FacePattern.MinSlot and <= FacePattern.MaxSlot)
                .Select(installation => installation.Normalize())
                .Where(installation => !string.IsNullOrWhiteSpace(installation.ContentFingerprint))
                .GroupBy(installation => installation.Slot)
                .Select(group => group.OrderByDescending(installation => installation.InstalledAt).First())
                .OrderBy(installation => installation.Slot)
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

    public FacePatternStoreState MarkUploadFailed(string patternId, string status)
    {
        var normalized = Normalize();
        var patterns = normalized.Patterns
            .Select(pattern => string.Equals(pattern.Id, patternId, StringComparison.Ordinal)
                ? pattern with { LastUploadStatus = status }
                : pattern)
            .ToArray();
        return normalized with { Patterns = patterns };
    }

    public FacePatternStoreState MarkSlotInstalled(
        int slot,
        string contentFingerprint,
        string sourceId,
        DateTimeOffset timestamp)
    {
        var normalized = Normalize();
        var clampedSlot = Math.Clamp(slot, FacePattern.MinSlot, FacePattern.MaxSlot);
        var installations = normalized.SlotInstallations
            .Where(installation => installation.Slot != clampedSlot)
            .Append(new FaceSlotInstallation
            {
                Slot = clampedSlot,
                ContentFingerprint = contentFingerprint,
                SourceId = sourceId,
                InstalledAt = timestamp
            })
            .ToArray();
        return normalized with { SlotInstallations = installations };
    }

    public FacePatternStoreState ClearSlotInstallation(int slot)
    {
        var normalized = Normalize();
        var clampedSlot = Math.Clamp(slot, FacePattern.MinSlot, FacePattern.MaxSlot);
        return normalized with
        {
            SlotInstallations = normalized.SlotInstallations
                .Where(installation => installation.Slot != clampedSlot)
                .ToArray()
        };
    }

    public FaceSlotInstallation? GetSlotInstallation(int slot) =>
        Normalize().SlotInstallations.FirstOrDefault(installation => installation.Slot == slot);

    public int NextCustomSlot(IEnumerable<int>? reservedSlots = null)
    {
        var reserved = reservedSlots?.ToHashSet() ?? [];
        var usedSlots = Normalize().Patterns.Select(pattern => pattern.PreferredSlot).ToHashSet();
        var candidates = Enumerable.Range(7, FacePattern.MaxSlot - 6)
            .Where(slot => !reserved.Contains(slot))
            .ToArray();
        foreach (var slot in candidates)
        {
            if (!usedSlots.Contains(slot))
            {
                return slot;
            }
        }

        return candidates.Length == 0 ? FacePattern.MaxSlot : candidates[^1];
    }
}
