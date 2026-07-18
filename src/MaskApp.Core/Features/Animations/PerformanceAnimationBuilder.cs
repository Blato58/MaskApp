using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public sealed class PerformanceAnimationBuilder
{
    public static readonly TimeSpan DefaultFrameDuration = TimeSpan.FromMilliseconds(75);
    public const double DefaultBpm = 120;

    private readonly TimeSpan defaultFrameDuration;

    public PerformanceAnimationBuilder(TimeSpan? defaultFrameDuration = null)
    {
        this.defaultFrameDuration = defaultFrameDuration ?? DefaultFrameDuration;
        if (this.defaultFrameDuration < PerformanceAnimation.MinFrameDuration ||
            this.defaultFrameDuration > PerformanceAnimation.MaxFrameDuration)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultFrameDuration));
        }
    }

    public PerformanceAnimation FromAppBuiltIn(
        AppBuiltInAnimation source,
        AnimationLoopMode loopMode = AnimationLoopMode.Continuous,
        int finiteLoopCount = 1,
        int slotBudget = PerformanceAnimation.MaxUniqueFrames)
    {
        var animation = source.Normalize();
        if (slotBudget is < 1 or > PerformanceAnimation.MaxUniqueFrames)
        {
            throw new ArgumentOutOfRangeException(nameof(slotBudget));
        }

        var firstSlotByFingerprint = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var playbackSlotMap = new Dictionary<int, int>();
        var storedFrames = new List<PerformanceAnimationStoredFrame>();
        foreach (var sourceFrame in animation.Frames)
        {
            var pattern = sourceFrame.Pattern.Normalize() with { PreferredSlot = sourceFrame.Slot };
            var fingerprint = FaceContentFingerprint.Compute(pattern);
            if (!firstSlotByFingerprint.TryGetValue(fingerprint, out var storedSlot))
            {
                if (storedFrames.Count >= slotBudget)
                {
                    throw new InvalidOperationException(
                        $"{animation.DisplayName} needs more than the verified {slotBudget}-slot budget.");
                }

                storedSlot = sourceFrame.Slot;
                firstSlotByFingerprint.Add(fingerprint, storedSlot);
                storedFrames.Add(new PerformanceAnimationStoredFrame
                {
                    Slot = storedSlot,
                    Pattern = pattern,
                    ContentFingerprint = fingerprint
                });
            }

            playbackSlotMap[sourceFrame.Slot] = storedSlot;
        }

        var result = new PerformanceAnimation
        {
            Id = animation.Id,
            DisplayName = animation.DisplayName,
            StoredFrames = storedFrames,
            Frames = animation.PlaybackSlots
                .Select(slot => new PerformanceAnimationFrame
                {
                    Slot = playbackSlotMap[slot],
                    Duration = animation.FrameDuration ?? defaultFrameDuration
                })
                .ToArray(),
            LoopMode = loopMode,
            FiniteLoopCount = finiteLoopCount,
            Bpm = DefaultBpm
        }.Normalize();

        return WithRevision(result);
    }

    public PerformanceAnimation WithBpm(PerformanceAnimation source, double bpm)
    {
        if (bpm is < 30 or > 300 || double.IsNaN(bpm) || double.IsInfinity(bpm))
        {
            throw new ArgumentOutOfRangeException(nameof(bpm), "BPM must be between 30 and 300.");
        }

        var animation = source.Normalize();
        var sourceBpm = animation.Bpm ?? DefaultBpm;
        var ratio = sourceBpm / bpm;
        var frames = animation.Frames
            .Select(frame => frame with
            {
                Duration = ClampDuration(TimeSpan.FromTicks((long)Math.Round(frame.Duration.Ticks * ratio)))
            })
            .ToArray();
        return WithRevision(animation with { Frames = frames, Bpm = bpm, RevisionHash = string.Empty });
    }

    public PerformanceAnimation WithRevision(PerformanceAnimation source)
    {
        var animation = source.Normalize() with { RevisionHash = string.Empty };
        var canonical = new StringBuilder()
            .Append("maskapp-animation-v1\n")
            .Append(animation.Id).Append('\n')
            .Append(animation.LoopMode).Append('\n')
            .Append(animation.FiniteLoopCount.ToString(CultureInfo.InvariantCulture)).Append('\n')
            .Append(animation.Bpm?.ToString("R", CultureInfo.InvariantCulture) ?? "none").Append('\n');
        foreach (var frame in animation.StoredFrames)
        {
            canonical.Append(frame.Slot).Append(':').Append(frame.ContentFingerprint.ToLowerInvariant()).Append('\n');
        }

        canonical.Append("playback\n");
        foreach (var frame in animation.Frames)
        {
            canonical.Append(frame.Slot).Append(':').Append(frame.Duration.Ticks).Append('\n');
        }

        var revision = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())))
            .ToLowerInvariant();
        return animation with { RevisionHash = revision };
    }

    private static TimeSpan ClampDuration(TimeSpan duration) =>
        duration < PerformanceAnimation.MinFrameDuration
            ? PerformanceAnimation.MinFrameDuration
            : duration > PerformanceAnimation.MaxFrameDuration
                ? PerformanceAnimation.MaxFrameDuration
                : duration;
}
