using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public sealed record AnimationCompilationResult
{
    public bool Succeeded { get; init; }

    public string Message { get; init; } = string.Empty;

    public PerformanceAnimation? Animation { get; init; }

    public int SourceFrameCount { get; init; }

    public int UniqueFrameCount { get; init; }

    public int DuplicateFrameCount => Math.Max(0, SourceFrameCount - UniqueFrameCount);

    public int SlotBudget { get; init; }

    public double SlotUsageRatio => SlotBudget <= 0 ? 1 : UniqueFrameCount / (double)SlotBudget;
}

public sealed class AnimationProjectCompiler
{
    private readonly PerformanceAnimationBuilder animationBuilder;

    public AnimationProjectCompiler(PerformanceAnimationBuilder? animationBuilder = null)
    {
        this.animationBuilder = animationBuilder ?? new PerformanceAnimationBuilder();
    }

    public AnimationCompilationResult Compile(
        AnimationProject source,
        int firstSlot = FacePattern.MinSlot,
        int slotBudget = PerformanceAnimation.MaxUniqueFrames)
    {
        if (firstSlot is < FacePattern.MinSlot or > FacePattern.MaxSlot)
        {
            throw new ArgumentOutOfRangeException(nameof(firstSlot));
        }

        if (slotBudget < 1 || firstSlot + slotBudget - 1 > FacePattern.MaxSlot)
        {
            throw new ArgumentOutOfRangeException(nameof(slotBudget));
        }

        AnimationProject project;
        try
        {
            project = source.Normalize();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return new AnimationCompilationResult
            {
                Message = exception.Message,
                SlotBudget = slotBudget
            };
        }

        var slotByFingerprint = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var storedFrames = new List<PerformanceAnimationStoredFrame>();
        var playbackFrames = new List<PerformanceAnimationFrame>(project.Frames.Count);
        foreach (var sourceFrame in project.Frames)
        {
            var fingerprint = FaceContentFingerprint.Compute(sourceFrame.Pattern);
            if (!slotByFingerprint.TryGetValue(fingerprint, out var slot))
            {
                if (storedFrames.Count >= slotBudget)
                {
                    return new AnimationCompilationResult
                    {
                        Message = $"{project.DisplayName} needs {storedFrames.Count + 1} unique DIY frames, exceeding the {slotBudget}-slot budget. Remove or merge frames before saving.",
                        SourceFrameCount = project.Frames.Count,
                        UniqueFrameCount = storedFrames.Count + 1,
                        SlotBudget = slotBudget
                    };
                }

                slot = firstSlot + storedFrames.Count;
                slotByFingerprint.Add(fingerprint, slot);
                var pattern = sourceFrame.Pattern.Normalize() with { PreferredSlot = slot };
                storedFrames.Add(new PerformanceAnimationStoredFrame
                {
                    Slot = slot,
                    Pattern = pattern,
                    ContentFingerprint = fingerprint
                });
            }

            playbackFrames.Add(new PerformanceAnimationFrame
            {
                Slot = slot,
                Duration = sourceFrame.Duration
            });
        }

        var animation = animationBuilder.WithRevision(new PerformanceAnimation
        {
            Id = project.Id,
            DisplayName = project.DisplayName,
            StoredFrames = storedFrames,
            Frames = playbackFrames,
            LoopMode = project.LoopMode,
            FiniteLoopCount = project.FiniteLoopCount,
            Bpm = project.Bpm
        }.Normalize());
        var duplicateCount = project.Frames.Count - storedFrames.Count;
        return new AnimationCompilationResult
        {
            Succeeded = true,
            Message = duplicateCount == 0
                ? $"Ready: {storedFrames.Count}/{slotBudget} DIY slots used."
                : $"Ready: {storedFrames.Count}/{slotBudget} DIY slots used; {duplicateCount} duplicate playback frame(s) reuse a slot.",
            Animation = animation,
            SourceFrameCount = project.Frames.Count,
            UniqueFrameCount = storedFrames.Count,
            SlotBudget = slotBudget
        };
    }
}
