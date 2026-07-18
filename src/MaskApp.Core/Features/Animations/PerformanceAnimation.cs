using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public enum AnimationLoopMode
{
    Finite,
    Continuous
}

public sealed record PerformanceAnimationFrame
{
    public int Slot { get; init; }

    public TimeSpan Duration { get; init; } = TimeSpan.FromMilliseconds(75);

    public PerformanceAnimationFrame Normalize()
    {
        if (Slot is < FacePattern.MinSlot or > FacePattern.MaxSlot)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Slot),
                Slot,
                $"Animation slot must be between {FacePattern.MinSlot} and {FacePattern.MaxSlot}.");
        }

        if (Duration < PerformanceAnimation.MinFrameDuration ||
            Duration > PerformanceAnimation.MaxFrameDuration)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Duration),
                Duration,
                $"Frame duration must be between {PerformanceAnimation.MinFrameDuration.TotalMilliseconds:0} ms and {PerformanceAnimation.MaxFrameDuration.TotalSeconds:0} seconds.");
        }

        return this;
    }
}

public sealed record PerformanceAnimationStoredFrame
{
    public int Slot { get; init; }

    public FacePattern Pattern { get; init; } = new();

    public string ContentFingerprint { get; init; } = string.Empty;

    public PerformanceAnimationStoredFrame Normalize()
    {
        if (Slot is < FacePattern.MinSlot or > FacePattern.MaxSlot)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Slot),
                Slot,
                $"Animation slot must be between {FacePattern.MinSlot} and {FacePattern.MaxSlot}.");
        }

        var pattern = Pattern.Normalize() with { PreferredSlot = Slot };
        var fingerprint = FaceContentFingerprint.Compute(pattern);
        if (!string.IsNullOrWhiteSpace(ContentFingerprint) &&
            !string.Equals(ContentFingerprint, fingerprint, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Stored-frame fingerprint does not match its pixels.", nameof(ContentFingerprint));
        }

        return this with
        {
            Pattern = pattern,
            ContentFingerprint = fingerprint
        };
    }
}

public sealed record PerformanceAnimation
{
    public const int MaxUniqueFrames = FacePattern.MaxSlot;
    public const int MaxPlaybackFrames = 10_000;
    public const int MaxFiniteLoops = 1000;
    public static readonly TimeSpan MinFrameDuration = TimeSpan.FromMilliseconds(16);
    public static readonly TimeSpan MaxFrameDuration = TimeSpan.FromSeconds(10);

    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public IReadOnlyList<PerformanceAnimationStoredFrame> StoredFrames { get; init; } = [];

    public IReadOnlyList<PerformanceAnimationFrame> Frames { get; init; } = [];

    public AnimationLoopMode LoopMode { get; init; } = AnimationLoopMode.Continuous;

    public int FiniteLoopCount { get; init; } = 1;

    public double? Bpm { get; init; }

    public string RevisionHash { get; init; } = string.Empty;

    public TimeSpan CycleDuration => TimeSpan.FromTicks(Frames.Sum(frame => frame.Duration.Ticks));

    public PerformanceAnimation Normalize()
    {
        var id = Id?.Trim() ?? string.Empty;
        var displayName = DisplayName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Animation id is required.", nameof(Id));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Animation display name is required.", nameof(DisplayName));
        }

        var storedFrames = (StoredFrames ?? []).Select(frame => frame.Normalize()).ToArray();
        if (storedFrames.Length is < 1 or > MaxUniqueFrames)
        {
            throw new ArgumentException($"An animation must use between 1 and {MaxUniqueFrames} unique frames.", nameof(StoredFrames));
        }

        if (storedFrames.Select(frame => frame.Slot).Distinct().Count() != storedFrames.Length)
        {
            throw new ArgumentException("Stored animation slots must be unique.", nameof(StoredFrames));
        }

        if (storedFrames.Select(frame => frame.ContentFingerprint).Distinct(StringComparer.OrdinalIgnoreCase).Count() != storedFrames.Length)
        {
            throw new ArgumentException("Identical stored frames must be deduplicated.", nameof(StoredFrames));
        }

        var frames = (Frames ?? []).Select(frame => frame.Normalize()).ToArray();
        if (frames.Length is < 1 or > MaxPlaybackFrames)
        {
            throw new ArgumentException(
                $"Animation playback must contain between 1 and {MaxPlaybackFrames} frames.",
                nameof(Frames));
        }

        var storedSlots = storedFrames.Select(frame => frame.Slot).ToHashSet();
        if (frames.Any(frame => !storedSlots.Contains(frame.Slot)))
        {
            throw new ArgumentException("Animation playback can reference only stored frame slots.", nameof(Frames));
        }

        if (FiniteLoopCount is < 1 or > MaxFiniteLoops)
        {
            throw new ArgumentOutOfRangeException(
                nameof(FiniteLoopCount),
                FiniteLoopCount,
                $"Finite loop count must be between 1 and {MaxFiniteLoops}.");
        }

        double? bpm = Bpm is null ? null : Math.Clamp(Bpm.Value, 30, 300);
        return this with
        {
            Id = id,
            DisplayName = displayName,
            StoredFrames = storedFrames,
            Frames = frames,
            Bpm = bpm,
            RevisionHash = RevisionHash?.Trim().ToLowerInvariant() ?? string.Empty
        };
    }
}
