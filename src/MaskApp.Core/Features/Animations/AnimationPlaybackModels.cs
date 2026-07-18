using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Animations;

public enum AnimationPlaybackState
{
    Idle,
    Starting,
    Playing,
    Paused,
    Completed,
    Stopped,
    Disconnected,
    Faulted,
    BlackedOut
}

public sealed record AnimationPlaybackRequest
{
    public Func<CancellationToken, Task<MaskCommandResult>>? RestorePreviousLookAsync { get; init; }

    public bool RestoreAfterFinitePlayback { get; init; } = true;

    public bool RestoreWhenReleased { get; init; } = true;

    public bool IsHoldToPlay { get; init; }
}

public sealed record AnimationPlaybackSnapshot
{
    public AnimationPlaybackState State { get; init; } = AnimationPlaybackState.Idle;

    public string AnimationId { get; init; } = string.Empty;

    public string RevisionHash { get; init; } = string.Empty;

    public long FramesSent { get; init; }

    public long FramesDropped { get; init; }

    public long LateFrames { get; init; }

    public TimeSpan MaximumLateness { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public string LastError { get; init; } = string.Empty;

    public bool IsActive => State is AnimationPlaybackState.Starting
        or AnimationPlaybackState.Playing
        or AnimationPlaybackState.Paused;
}

public sealed class AnimationPlaybackSnapshotChangedEventArgs(AnimationPlaybackSnapshot snapshot) : EventArgs
{
    public AnimationPlaybackSnapshot Snapshot { get; } = snapshot;
}

public sealed record AnimationPlaybackStartResult(
    bool Succeeded,
    string Message,
    AnimationPlaybackHandle? Handle)
{
    public static AnimationPlaybackStartResult Failure(string message) => new(false, message, null);

    public static AnimationPlaybackStartResult Success(string message, AnimationPlaybackHandle handle) =>
        new(true, message, handle);
}

public sealed class AnimationPlaybackHandle
{
    private readonly PerformanceAnimationEngine owner;
    private readonly long sessionId;

    internal AnimationPlaybackHandle(PerformanceAnimationEngine owner, long sessionId)
    {
        this.owner = owner;
        this.sessionId = sessionId;
    }

    public bool Pause() => owner.Pause(sessionId);

    public bool Resume() => owner.Resume(sessionId);

    public Task<MaskCommandResult> StopAsync(
        bool restorePreviousLook = true,
        CancellationToken cancellationToken = default) =>
        owner.StopPlaybackAsync(sessionId, restorePreviousLook, cancellationToken);

    public Task<MaskCommandResult> ReleaseAsync(CancellationToken cancellationToken = default) =>
        owner.ReleaseAsync(sessionId, cancellationToken);
}
