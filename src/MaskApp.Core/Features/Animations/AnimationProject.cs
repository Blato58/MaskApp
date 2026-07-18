using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public enum AnimationProjectSource
{
    Drawn,
    GifImport,
    VideoImport,
    MaskPackImport
}

public sealed record AnimationProjectFrame
{
    public string Id { get; init; } = string.Empty;

    public FacePattern Pattern { get; init; } = new();

    public TimeSpan Duration { get; init; } = PerformanceAnimationBuilder.DefaultFrameDuration;

    public AnimationProjectFrame Normalize(int index)
    {
        if (Duration < PerformanceAnimation.MinFrameDuration ||
            Duration > PerformanceAnimation.MaxFrameDuration)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Duration),
                Duration,
                $"Frame duration must be between {PerformanceAnimation.MinFrameDuration.TotalMilliseconds:0} ms and {PerformanceAnimation.MaxFrameDuration.TotalSeconds:0} seconds.");
        }

        var id = string.IsNullOrWhiteSpace(Id) ? $"frame-{index + 1}" : Id.Trim();
        var pattern = Pattern.Normalize() with
        {
            Id = $"animation-frame-{id}",
            DisplayName = $"Frame {index + 1}",
            Source = FacePatternSource.Custom,
            PreferredSlot = FacePattern.MinSlot
        };
        return this with { Id = id, Pattern = pattern };
    }
}

public sealed record AnimationProject
{
    public const int MaxSourceFrames = 120;
    public const int MaxProjects = 50;

    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = "Custom Animation";

    public AnimationProjectSource Source { get; init; }

    public IReadOnlyList<AnimationProjectFrame> Frames { get; init; } = [];

    public AnimationLoopMode LoopMode { get; init; } = AnimationLoopMode.Continuous;

    public int FiniteLoopCount { get; init; } = 1;

    public double? Bpm { get; init; } = PerformanceAnimationBuilder.DefaultBpm;

    public bool IsFavorite { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    public AnimationProject Normalize(DateTimeOffset? timestamp = null)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        var id = string.IsNullOrWhiteSpace(Id) ? $"animation-{Guid.NewGuid():N}" : Id.Trim();
        var displayName = string.IsNullOrWhiteSpace(DisplayName) ? "Custom Animation" : DisplayName.Trim();
        var frames = (Frames ?? []).Select((frame, index) => frame.Normalize(index)).ToArray();
        if (frames.Length is < 1 or > MaxSourceFrames)
        {
            throw new ArgumentException(
                $"An animation project must contain between 1 and {MaxSourceFrames} frames.",
                nameof(Frames));
        }

        if (frames.Select(frame => frame.Id).Distinct(StringComparer.Ordinal).Count() != frames.Length)
        {
            throw new ArgumentException("Animation frame ids must be unique.", nameof(Frames));
        }

        if (FiniteLoopCount is < 1 or > PerformanceAnimation.MaxFiniteLoops)
        {
            throw new ArgumentOutOfRangeException(nameof(FiniteLoopCount));
        }

        double? bpm = Bpm;
        if (bpm is not null &&
            (double.IsNaN(bpm.Value) || double.IsInfinity(bpm.Value) || bpm.Value is < 30 or > 300))
        {
            throw new ArgumentOutOfRangeException(nameof(Bpm), "BPM must be between 30 and 300.");
        }

        return this with
        {
            Id = id,
            DisplayName = displayName,
            Frames = frames,
            CreatedAt = CreatedAt == default ? now : CreatedAt,
            UpdatedAt = UpdatedAt == default ? now : UpdatedAt
        };
    }

    public static AnimationProject CreateBlank(string displayName = "Custom Animation") => new AnimationProject
    {
        DisplayName = displayName,
        Frames =
        [
            new AnimationProjectFrame
            {
                Id = $"frame-{Guid.NewGuid():N}",
                Pattern = FacePatternFactory.CreateBlank("Animation frame", FacePattern.MinSlot)
            }
        ]
    }.Normalize();
}

public sealed record AnimationProjectStoreState
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public IReadOnlyList<AnimationProject> Projects { get; init; } = [];

    public bool UsedFallback { get; init; }

    public string Status { get; init; } = "Ready.";

    public AnimationProjectStoreState Normalize()
    {
        var projects = (Projects ?? []).Select(project => project.Normalize()).ToArray();
        if (projects.Length > AnimationProject.MaxProjects)
        {
            throw new ArgumentException($"At most {AnimationProject.MaxProjects} animation projects can be stored.");
        }

        return this with
        {
            SchemaVersion = CurrentSchemaVersion,
            Projects = projects
                .GroupBy(project => project.Id, StringComparer.Ordinal)
                .Select(group => group.OrderByDescending(project => project.UpdatedAt).First())
                .OrderByDescending(project => project.UpdatedAt)
                .ToArray(),
            Status = Status?.Trim() ?? string.Empty
        };
    }
}
