namespace MaskApp.Core.Features.AnimationPacks;

public enum MaskPackContentType
{
    Face,
    Animation,
    TextPreset,
    Page,
    Scene,
    Setlist,
    Appearance
}

public sealed record MaskPackContentEntry
{
    public string Id { get; init; } = string.Empty;

    public MaskPackContentType Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public string Sha256 { get; init; } = string.Empty;

    public int FormatVersion { get; init; } = 1;
}

public enum MaskPackConflictResolution
{
    Merge,
    Rename,
    Skip,
    Replace
}

public sealed record MaskPackConflict(
    string Key,
    MaskPackContentType Type,
    string Id,
    string Name,
    string ExistingSha256,
    string ImportedSha256,
    bool IsExactMatch,
    string SuggestedId);

public sealed record MaskPackInspection
{
    public MaskPackDecodedPackage? Package { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = [];

    public IReadOnlyList<string> Warnings { get; init; } = [];

    public IReadOnlyList<MaskPackConflict> Conflicts { get; init; } = [];

    public bool IsValid => Package is not null && Errors.Count == 0;

    public bool MigratedFromV1 => Package?.MigratedFromV1 == true;
}

public sealed record MaskPackImportRequest
{
    public required MaskPackInspection Inspection { get; init; }

    public MaskPackConflictResolution DefaultResolution { get; init; } = MaskPackConflictResolution.Merge;

    public IReadOnlyDictionary<string, MaskPackConflictResolution> ConflictResolutions { get; init; } =
        new Dictionary<string, MaskPackConflictResolution>(StringComparer.Ordinal);

    public bool ConfirmReplace { get; init; }
}

public sealed record MaskPackImportResult(
    bool Succeeded,
    string Message,
    int ImportedCount,
    int RenamedCount,
    int SkippedCount,
    int ReplacedCount,
    bool RecoveredInterruptedImport = false);

public sealed record MaskPackExportRequest
{
    public string PackName { get; init; } = "MaskApp Show";

    public string Author { get; init; } = "MaskApp";

    public bool IncludeUnreferencedUserContent { get; init; } = true;
}

public sealed record MaskPackExportResult(
    bool Succeeded,
    string Message,
    int ContentCount,
    long BytesWritten);
