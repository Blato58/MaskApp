namespace MaskApp.Core.Features.Scenes;

public enum SceneStepKind
{
    Brightness,
    AnimationSpeed,
    Face,
    Text,
    Animation,
    Wait,
    Repeat,
    RestorePrevious,
    Stop,
    Blackout
}

public enum SceneFailurePolicy
{
    StopScene,
    Continue
}

public sealed record PerformanceSceneStep
{
    public string Id { get; init; } = string.Empty;

    public SceneStepKind Kind { get; init; }

    public string GalleryItemId { get; init; } = string.Empty;

    public int Value { get; init; } = 50;

    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(1);

    public string RepeatFromStepId { get; init; } = string.Empty;

    public int RepeatCount { get; init; } = 2;

    public PerformanceSceneStep Normalize(int index)
    {
        var id = string.IsNullOrWhiteSpace(Id) ? $"step-{index + 1}" : Id.Trim();
        var galleryItemId = GalleryItemId?.Trim() ?? string.Empty;
        var repeatFrom = RepeatFromStepId?.Trim() ?? string.Empty;
        switch (Kind)
        {
            case SceneStepKind.Brightness or SceneStepKind.AnimationSpeed when Value is < 1 or > 100:
                throw new ArgumentOutOfRangeException(nameof(Value), "Brightness and speed must be between 1 and 100.");
            case SceneStepKind.Face or SceneStepKind.Text or SceneStepKind.Animation
                when string.IsNullOrWhiteSpace(galleryItemId):
                throw new ArgumentException($"{Kind} steps require a Library item.", nameof(GalleryItemId));
            case SceneStepKind.Wait when Duration < PerformanceScene.MinWaitDuration || Duration > PerformanceScene.MaxWaitDuration:
                throw new ArgumentOutOfRangeException(
                    nameof(Duration),
                    $"Wait duration must be between {PerformanceScene.MinWaitDuration.TotalMilliseconds:0} ms and {PerformanceScene.MaxWaitDuration.TotalSeconds:0} seconds.");
            case SceneStepKind.Repeat when string.IsNullOrWhiteSpace(repeatFrom):
                throw new ArgumentException("Repeat steps require an earlier start step.", nameof(RepeatFromStepId));
            case SceneStepKind.Repeat when RepeatCount is < 2 or > PerformanceScene.MaxRepeatCount:
                throw new ArgumentOutOfRangeException(
                    nameof(RepeatCount),
                    $"Repeat count must be between 2 and {PerformanceScene.MaxRepeatCount}.");
        }

        return this with
        {
            Id = id,
            GalleryItemId = galleryItemId,
            RepeatFromStepId = repeatFrom
        };
    }
}

public sealed record PerformanceScene
{
    public const int MaxSteps = 32;
    public const int MaxExpandedSteps = 128;
    public const int MaxRepeatCount = 10;
    public static readonly TimeSpan MinWaitDuration = TimeSpan.FromMilliseconds(16);
    public static readonly TimeSpan MaxWaitDuration = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan MaxTotalWaitDuration = TimeSpan.FromMinutes(5);

    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = "New Scene";

    public string ColorHex { get; init; } = "#A78BFA";

    public IReadOnlyList<PerformanceSceneStep> Steps { get; init; } = [];

    public SceneFailurePolicy FailurePolicy { get; init; } = SceneFailurePolicy.StopScene;

    public bool IsFavorite { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    public PerformanceScene Normalize(DateTimeOffset? timestamp = null)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        var id = string.IsNullOrWhiteSpace(Id) ? $"scene-{Guid.NewGuid():N}" : Id.Trim();
        var name = string.IsNullOrWhiteSpace(DisplayName) ? "New Scene" : DisplayName.Trim();
        var steps = (Steps ?? []).Select((step, index) => step.Normalize(index)).ToArray();
        if (steps.Length is < 1 or > MaxSteps)
        {
            throw new ArgumentException($"A Scene must contain between 1 and {MaxSteps} steps.", nameof(Steps));
        }

        if (steps.Select(step => step.Id).Distinct(StringComparer.Ordinal).Count() != steps.Length)
        {
            throw new ArgumentException("Scene step ids must be unique.", nameof(Steps));
        }

        var color = string.IsNullOrWhiteSpace(ColorHex) ? "#A78BFA" : ColorHex.Trim();
        return this with
        {
            Id = id,
            DisplayName = name,
            ColorHex = color,
            Steps = steps,
            CreatedAt = CreatedAt == default ? now : CreatedAt,
            UpdatedAt = UpdatedAt == default ? now : UpdatedAt
        };
    }

    public static PerformanceScene CreateBlank(string displayName = "New Scene") => new PerformanceScene
    {
        DisplayName = displayName,
        Steps =
        [
            new PerformanceSceneStep
            {
                Id = $"step-{Guid.NewGuid():N}",
                Kind = SceneStepKind.Brightness,
                Value = 60
            }
        ]
    }.Normalize();
}

public sealed record PerformanceSetlistCue
{
    public string Id { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string SceneId { get; init; } = string.Empty;

    public PerformanceSetlistCue Normalize(int index)
    {
        var id = string.IsNullOrWhiteSpace(Id) ? $"cue-{index + 1}" : Id.Trim();
        var sceneId = SceneId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(sceneId))
        {
            throw new ArgumentException("Setlist cues require a Scene.", nameof(SceneId));
        }

        return this with
        {
            Id = id,
            SceneId = sceneId,
            Label = string.IsNullOrWhiteSpace(Label) ? $"Cue {index + 1}" : Label.Trim()
        };
    }
}

public sealed record PerformanceSetlist
{
    public const int MaxCues = 64;

    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = "New Setlist";

    public IReadOnlyList<PerformanceSetlistCue> Cues { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    public PerformanceSetlist Normalize(DateTimeOffset? timestamp = null)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        var id = string.IsNullOrWhiteSpace(Id) ? $"setlist-{Guid.NewGuid():N}" : Id.Trim();
        var cues = (Cues ?? []).Select((cue, index) => cue.Normalize(index)).ToArray();
        if (cues.Length > MaxCues)
        {
            throw new ArgumentException($"A setlist can contain at most {MaxCues} cues.", nameof(Cues));
        }

        if (cues.Select(cue => cue.Id).Distinct(StringComparer.Ordinal).Count() != cues.Length)
        {
            throw new ArgumentException("Setlist cue ids must be unique.", nameof(Cues));
        }

        return this with
        {
            Id = id,
            DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? "New Setlist" : DisplayName.Trim(),
            Cues = cues,
            CreatedAt = CreatedAt == default ? now : CreatedAt,
            UpdatedAt = UpdatedAt == default ? now : UpdatedAt
        };
    }

    public static PerformanceSetlist CreateBlank(string displayName = "New Setlist") => new PerformanceSetlist
    {
        DisplayName = displayName
    }.Normalize();
}

public sealed record SetlistPosition(
    string SetlistId,
    int CueIndex,
    DateTimeOffset UpdatedAt)
{
    public SetlistPosition Normalize(int cueCount) => this with
    {
        SetlistId = SetlistId?.Trim() ?? string.Empty,
        CueIndex = cueCount == 0 ? 0 : Math.Clamp(CueIndex, 0, cueCount - 1),
        UpdatedAt = UpdatedAt == default ? DateTimeOffset.UtcNow : UpdatedAt
    };
}

public sealed record SceneShowState
{
    public const int CurrentSchemaVersion = 1;
    public const int MaxScenes = 50;
    public const int MaxSetlists = 20;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public IReadOnlyList<PerformanceScene> Scenes { get; init; } = [];

    public IReadOnlyList<PerformanceSetlist> Setlists { get; init; } = [];

    public IReadOnlyList<SetlistPosition> Positions { get; init; } = [];

    public string ActiveSetlistId { get; init; } = string.Empty;

    public bool UsedFallback { get; init; }

    public string Status { get; init; } = "Ready.";

    public SceneShowState Normalize()
    {
        var scenes = (Scenes ?? []).Select(scene => scene.Normalize()).ToArray();
        var setlists = (Setlists ?? []).Select(setlist => setlist.Normalize()).ToArray();
        if (scenes.Length > MaxScenes)
        {
            throw new ArgumentException($"At most {MaxScenes} Scenes can be stored.");
        }

        if (setlists.Length > MaxSetlists)
        {
            throw new ArgumentException($"At most {MaxSetlists} setlists can be stored.");
        }

        var sceneIds = scenes.Select(scene => scene.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var cue in setlists.SelectMany(setlist => setlist.Cues))
        {
            if (!sceneIds.Contains(cue.SceneId))
            {
                throw new ArgumentException($"Setlist cue {cue.Label} references missing Scene {cue.SceneId}.");
            }
        }

        var setlistsById = setlists.ToDictionary(setlist => setlist.Id, StringComparer.Ordinal);
        var positions = (Positions ?? [])
            .Where(position => setlistsById.ContainsKey(position.SetlistId))
            .GroupBy(position => position.SetlistId, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(position => position.UpdatedAt).First())
            .Select(position => position.Normalize(setlistsById[position.SetlistId].Cues.Count))
            .ToArray();
        var activeSetlistId = ActiveSetlistId?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(activeSetlistId) && !setlistsById.ContainsKey(activeSetlistId))
        {
            activeSetlistId = string.Empty;
        }

        return this with
        {
            SchemaVersion = CurrentSchemaVersion,
            Scenes = scenes.OrderByDescending(scene => scene.UpdatedAt).ToArray(),
            Setlists = setlists.OrderByDescending(setlist => setlist.UpdatedAt).ToArray(),
            Positions = positions,
            ActiveSetlistId = activeSetlistId,
            Status = Status?.Trim() ?? string.Empty
        };
    }
}
