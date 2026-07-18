using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Animations;

public sealed class PerformanceAnimationEngine : IDisposable
{
    private const int MaxFirmwarePlaybackSteps = 10;

    private readonly object sync = new();
    private readonly SemaphoreSlim transitionGate = new(1, 1);
    private readonly SemaphoreSlim frameSendGate = new(1, 1);
    private readonly IMaskCommandTransport commandTransport;
    private readonly IMaskEmergencyControl? emergencyControl;
    private readonly IAnimationClock clock;
    private readonly FlashSafetyAnalyzer flashSafetyAnalyzer;
    private readonly IFlashSafetyAcknowledgementStore flashSafetyAcknowledgementStore;
    private PlaybackSession? currentSession;
    private AnimationPlaybackSnapshot snapshot = new();
    private long nextSessionId;
    private long applicationLifecycleVersion;
    private bool applicationIsBackgrounded;
    private bool disposed;

    public PerformanceAnimationEngine(
        IMaskCommandTransport commandTransport,
        IMaskEmergencyControl? emergencyControl = null,
        IAnimationClock? clock = null,
        FlashSafetyAnalyzer? flashSafetyAnalyzer = null,
        IFlashSafetyAcknowledgementStore? flashSafetyAcknowledgementStore = null)
    {
        this.commandTransport = commandTransport ?? throw new ArgumentNullException(nameof(commandTransport));
        this.emergencyControl = emergencyControl;
        this.clock = clock ?? new MonotonicAnimationClock();
        this.flashSafetyAnalyzer = flashSafetyAnalyzer ?? new FlashSafetyAnalyzer();
        this.flashSafetyAcknowledgementStore = flashSafetyAcknowledgementStore
            ?? new InMemoryFlashSafetyAcknowledgementStore();
        commandTransport.TransportStateChanged += OnTransportStateChanged;
    }

    public event EventHandler<AnimationPlaybackSnapshotChangedEventArgs>? SnapshotChanged;

    public AnimationPlaybackSnapshot GetSnapshot()
    {
        lock (sync)
        {
            return snapshot;
        }
    }

    public async Task<AnimationPlaybackStartResult> StartAsync(
        PerformanceAnimation animation,
        AnimationPlaybackRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var normalized = animation.Normalize();
        if (string.IsNullOrWhiteSpace(normalized.RevisionHash))
        {
            return AnimationPlaybackStartResult.Failure("Animation does not have a content revision hash.");
        }

        if (commandTransport.TransportState != MaskCommandTransportState.Ready)
        {
            return AnimationPlaybackStartResult.Failure("Connect to start animation playback.");
        }

        var safetyAssessment = flashSafetyAnalyzer.Analyze(normalized);
        var acknowledgementState = await flashSafetyAcknowledgementStore.LoadAsync(cancellationToken)
            .ConfigureAwait(false);
        if (acknowledgementState.UsedFallback)
        {
            return AnimationPlaybackStartResult.Failure(
                $"Playback blocked because flash-safety acknowledgements are unavailable: {acknowledgementState.Status}");
        }

        var safetyDecision = flashSafetyAnalyzer.Decide(safetyAssessment, acknowledgementState);
        if (!safetyDecision.CanPlay)
        {
            return AnimationPlaybackStartResult.Failure(safetyDecision.Message);
        }

        await transitionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await StopCurrentSessionCoreAsync(restorePreviousLook: false).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var session = new PlaybackSession(
                Interlocked.Increment(ref nextSessionId),
                normalized,
                request ?? new AnimationPlaybackRequest(),
                cancellationToken);
            lock (sync)
            {
                currentSession = session;
                snapshot = CreateSnapshot(session, AnimationPlaybackState.Starting);
            }

            PublishSnapshot();
            MaskCommandResult firstResult;
            try
            {
                firstResult = await SendFrameCoreAsync(normalized.Frames[0], session.Cancellation.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (session.Cancellation.IsCancellationRequested)
            {
                CompleteSession(session, AnimationPlaybackState.Stopped, "Animation start was cancelled.");
                return AnimationPlaybackStartResult.Failure("Animation start was cancelled.");
            }
            catch (Exception exception)
            {
                CompleteSession(session, AnimationPlaybackState.Faulted, exception.Message);
                return AnimationPlaybackStartResult.Failure($"Animation start failed: {exception.Message}");
            }

            if (!firstResult.Succeeded)
            {
                CompleteSession(session, AnimationPlaybackState.Faulted, firstResult.Message);
                return AnimationPlaybackStartResult.Failure(firstResult.Message);
            }

            session.FramesSent = 1;
            session.StartTimestamp = clock.GetTimestamp();
            session.Task = RunSessionAsync(session);
            UpdateSessionSnapshot(session, AnimationPlaybackState.Playing);
            bool shouldHandOffImmediately;
            long lifecycleVersion;
            lock (sync)
            {
                shouldHandOffImmediately = applicationIsBackgrounded;
                lifecycleVersion = applicationLifecycleVersion;
            }

            var immediateBackgroundHandoff = shouldHandOffImmediately
                ? await HandOffCurrentSessionCoreAsync(CancellationToken.None, lifecycleVersion).ConfigureAwait(false)
                : null;

            var handle = new AnimationPlaybackHandle(this, session.Id);
            var backgroundBehavior = normalized.LoopMode == AnimationLoopMode.Continuous
                ? "Lock/background attempts a one-command handoff to the mask's fixed firmware cadence; returning reclaims playback and resumes configured app timing."
                : "Finite playback pauses while the app is backgrounded and resumes when it returns.";
            var handoffStatus = immediateBackgroundHandoff is null
                ? string.Empty
                : $" {immediateBackgroundHandoff.Message}";
            return AnimationPlaybackStartResult.Success(
                $"Started {normalized.DisplayName} with monotonic frame timing. {backgroundBehavior}{handoffStatus} {safetyDecision.Message}",
                handle);
        }
        finally
        {
            transitionGate.Release();
        }
    }

    public bool Pause()
    {
        lock (sync)
        {
            return currentSession is not null && PauseLocked(currentSession.Id);
        }
    }

    public bool Resume()
    {
        lock (sync)
        {
            return currentSession is not null && ResumeLocked(currentSession.Id);
        }
    }

    public async Task<MaskCommandResult> HandOffToMaskForBackgroundAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        long lifecycleVersion;
        lock (sync)
        {
            applicationIsBackgrounded = true;
            lifecycleVersion = ++applicationLifecycleVersion;
        }

        await transitionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            lock (sync)
            {
                if (!applicationIsBackgrounded || lifecycleVersion != applicationLifecycleVersion)
                {
                    return MaskCommandResult.Success(
                        "Skipped a stale background handoff because the app lifecycle changed.");
                }
            }

            return await HandOffCurrentSessionCoreAsync(cancellationToken, lifecycleVersion).ConfigureAwait(false);
        }
        finally
        {
            transitionGate.Release();
        }
    }

    private async Task<MaskCommandResult> HandOffCurrentSessionCoreAsync(
        CancellationToken cancellationToken,
        long lifecycleVersion)
    {
        PlaybackSession? session;
        bool sendFirmwareLoop;
        lock (sync)
        {
            session = currentSession;
            if (session is null)
            {
                return MaskCommandResult.Success("No active app-timed animation needs a background handoff.");
            }

            if (session.IsBackgrounded && session.MaskOwnsPlayback)
            {
                return MaskCommandResult.Success("Animation is already handed off for background playback.");
            }

            sendFirmwareLoop = session.Animation.LoopMode == AnimationLoopMode.Continuous
                && (!session.IsPaused || session.PausedForBackground);
            session.IsBackgrounded = true;
            session.MaskOwnsPlayback = false;
            if (!session.IsPaused)
            {
                session.IsPaused = true;
                session.PausedForBackground = true;
                session.PauseStartedTimestamp = clock.GetTimestamp();
            }

            snapshot = CreateSnapshot(session, AnimationPlaybackState.Backgrounded);
            SignalStateChangedLocked(session);
        }

        PublishSnapshot();
        if (!sendFirmwareLoop)
        {
            return MaskCommandResult.Success(
                "Animation is paused for background and will resume without replay when the app returns.");
        }

        var backgroundSlots = CreateFirmwarePlaybackSlots(session.Animation);
        using var handoffCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            session.Cancellation.Token);
        await frameSendGate.WaitAsync(handoffCancellation.Token).ConfigureAwait(false);
        try
        {
            lock (sync)
            {
                if (!ReferenceEquals(currentSession, session)
                    || !session.IsBackgrounded
                    || session.Cancellation.IsCancellationRequested)
                {
                    return MaskCommandResult.Failure(
                        "Animation ended before its background handoff could be sent.");
                }

                if (!applicationIsBackgrounded || lifecycleVersion != applicationLifecycleVersion)
                {
                    return MaskCommandResult.Success(
                        "Skipped a stale background handoff because the app lifecycle changed.");
                }
            }

            var result = await commandTransport.SendAsync(
                FaceUploadProtocol.BuildPlayCommand(backgroundSlots),
                handoffCancellation.Token).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                UpdateBackgroundPlaybackError(session, $"Background handoff failed: {result.Message}");
                return result;
            }

            lock (sync)
            {
                if (ReferenceEquals(currentSession, session)
                    && session.IsBackgrounded
                    && !session.Cancellation.IsCancellationRequested)
                {
                    session.MaskOwnsPlayback = true;
                }
            }
        }
        finally
        {
            frameSendGate.Release();
        }

        var samplingNote = session.Animation.Frames.Count > MaxFirmwarePlaybackSteps
            ? " The first 10 playback steps are used because PLAY carries at most 10 steps."
            : string.Empty;
        return MaskCommandResult.Success(
            $"Sent the mask-owned playback handoff for lock/background. " +
            $"The mask's fixed cadence may be slower than the configured app timing.{samplingNote}");
    }

    public async Task<MaskCommandResult> ResumeFromBackgroundAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        long lifecycleVersion;
        lock (sync)
        {
            applicationIsBackgrounded = false;
            lifecycleVersion = ++applicationLifecycleVersion;
        }

        await transitionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            PlaybackSession? session;
            bool reclaimFirmwarePlayback;
            lock (sync)
            {
                if (applicationIsBackgrounded || lifecycleVersion != applicationLifecycleVersion)
                {
                    return MaskCommandResult.Success(
                        "Skipped a stale foreground resume because the app lifecycle changed.");
                }

                session = currentSession;
                if (session is null)
                {
                    return MaskCommandResult.Success("No background animation session needs to resume.");
                }

                if (!session.IsBackgrounded)
                {
                    return MaskCommandResult.Success("Animation is already using app-timed playback.");
                }

                reclaimFirmwarePlayback = session.MaskOwnsPlayback && session.PausedForBackground;
            }

            if (reclaimFirmwarePlayback)
            {
                using var reclaimCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    session.Cancellation.Token);
                await frameSendGate.WaitAsync(reclaimCancellation.Token).ConfigureAwait(false);
                try
                {
                    int currentFrameSlot;
                    lock (sync)
                    {
                        if (!ReferenceEquals(currentSession, session)
                            || session.Cancellation.IsCancellationRequested)
                        {
                            return MaskCommandResult.Failure(
                                "Animation ended before foreground playback could be reclaimed.");
                        }

                        currentFrameSlot = session.CurrentFrameSlot;
                    }

                    var reclaimResult = await commandTransport.SendAsync(
                        FaceUploadProtocol.BuildPlayCommand([currentFrameSlot]),
                        reclaimCancellation.Token).ConfigureAwait(false);
                    if (!reclaimResult.Succeeded)
                    {
                        UpdateBackgroundPlaybackError(
                            session,
                            $"Foreground reclaim failed: {reclaimResult.Message}");
                        return reclaimResult;
                    }
                }
                finally
                {
                    frameSendGate.Release();
                }
            }

            bool resumed;
            lock (sync)
            {
                if (!ReferenceEquals(currentSession, session)
                    || session.Cancellation.IsCancellationRequested)
                {
                    return MaskCommandResult.Failure(
                        "Animation ended before foreground playback could resume.");
                }

                session.IsBackgrounded = false;
                session.MaskOwnsPlayback = false;
                resumed = session.PausedForBackground;
                if (resumed)
                {
                    var now = clock.GetTimestamp();
                    session.PendingDeadlineShift += clock.GetElapsedTime(session.PauseStartedTimestamp, now);
                    session.PausedForBackground = false;
                    session.IsPaused = false;
                    snapshot = CreateSnapshot(session, AnimationPlaybackState.Playing);
                    SignalStateChangedLocked(session);
                }
                else
                {
                    snapshot = CreateSnapshot(session, AnimationPlaybackState.Paused);
                }
            }

            PublishSnapshot();
            return resumed
                ? MaskCommandResult.Success(
                    reclaimFirmwarePlayback
                        ? "Reclaimed mask playback and resumed configured app timing."
                        : "Resumed configured app-timed animation playback.")
                : MaskCommandResult.Success("Animation remains paused because it was paused before backgrounding.");
        }
        finally
        {
            transitionGate.Release();
        }
    }

    public async Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        RequestStopPlayback();
        await transitionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await StopCurrentSessionCoreAsync(restorePreviousLook: true).ConfigureAwait(false);
            return emergencyControl is null
                ? MaskCommandResult.Success("Animation playback stopped.")
                : await emergencyControl.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            transitionGate.Release();
        }
    }

    public async Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        PlaybackSession? requestedSession;
        lock (sync)
        {
            requestedSession = currentSession;
            if (requestedSession is not null)
            {
                requestedSession.RequestedTerminalState = AnimationPlaybackState.BlackedOut;
                requestedSession.Cancellation.Cancel();
                SignalStateChangedLocked(requestedSession);
            }
        }

        await transitionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (requestedSession is not null)
            {
                await AwaitSessionAsync(requestedSession).ConfigureAwait(false);
                CompleteSession(requestedSession, AnimationPlaybackState.BlackedOut, string.Empty);
            }

            return emergencyControl is null
                ? MaskCommandResult.Failure("Blackout is unavailable because no emergency control is configured.")
                : await emergencyControl.BlackoutAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            transitionGate.Release();
        }
    }

    public void RequestStopPlayback()
    {
        lock (sync)
        {
            if (currentSession is null)
            {
                return;
            }

            currentSession.RequestedTerminalState = AnimationPlaybackState.Stopped;
            currentSession.Cancellation.Cancel();
            SignalStateChangedLocked(currentSession);
        }
    }

    public async Task<MaskCommandResult> StopPlaybackAsync(
        bool restorePreviousLook = false,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        PlaybackSession? requestedSession;
        lock (sync)
        {
            requestedSession = currentSession;
            if (requestedSession is not null)
            {
                requestedSession.RequestedTerminalState = AnimationPlaybackState.Stopped;
                requestedSession.Cancellation.Cancel();
                SignalStateChangedLocked(requestedSession);
            }
        }

        await transitionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await StopCurrentSessionCoreAsync(restorePreviousLook).ConfigureAwait(false);
            if (restorePreviousLook && requestedSession is not null)
            {
                await RestorePreviousLookOnceAsync(requestedSession, CancellationToken.None)
                    .ConfigureAwait(false);
            }

            return MaskCommandResult.Success("Animation playback stopped.");
        }
        finally
        {
            transitionGate.Release();
        }
    }

    internal bool Pause(long sessionId)
    {
        lock (sync)
        {
            return PauseLocked(sessionId);
        }
    }

    internal bool Resume(long sessionId)
    {
        lock (sync)
        {
            return ResumeLocked(sessionId);
        }
    }

    internal Task<MaskCommandResult> StopPlaybackAsync(
        long sessionId,
        bool restorePreviousLook,
        CancellationToken cancellationToken)
    {
        lock (sync)
        {
            if (currentSession?.Id != sessionId)
            {
                return Task.FromResult(MaskCommandResult.Success("Animation session is no longer active."));
            }
        }

        return StopPlaybackAsync(restorePreviousLook, cancellationToken);
    }

    internal Task<MaskCommandResult> ReleaseAsync(long sessionId, CancellationToken cancellationToken)
    {
        bool restore;
        lock (sync)
        {
            if (currentSession?.Id != sessionId)
            {
                return Task.FromResult(MaskCommandResult.Success("Hold animation is no longer active."));
            }

            restore = currentSession.Request.IsHoldToPlay && currentSession.Request.RestoreWhenReleased;
        }

        return StopPlaybackAsync(sessionId, restore, cancellationToken);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        commandTransport.TransportStateChanged -= OnTransportStateChanged;
        RequestStopPlayback();
        transitionGate.Dispose();
    }

    private bool PauseLocked(long sessionId)
    {
        var session = currentSession;
        if (session?.Id != sessionId || session.IsPaused || session.Cancellation.IsCancellationRequested)
        {
            return false;
        }

        session.IsPaused = true;
        session.PauseStartedTimestamp = clock.GetTimestamp();
        snapshot = CreateSnapshot(session, AnimationPlaybackState.Paused);
        SignalStateChangedLocked(session);
        PublishSnapshotDeferred();
        return true;
    }

    private bool ResumeLocked(long sessionId)
    {
        var session = currentSession;
        if (session?.Id != sessionId
            || !session.IsPaused
            || session.IsBackgrounded
            || session.Cancellation.IsCancellationRequested)
        {
            return false;
        }

        var now = clock.GetTimestamp();
        session.PendingDeadlineShift += clock.GetElapsedTime(session.PauseStartedTimestamp, now);
        session.IsPaused = false;
        snapshot = CreateSnapshot(session, AnimationPlaybackState.Playing);
        SignalStateChangedLocked(session);
        PublishSnapshotDeferred();
        return true;
    }

    private async Task RunSessionAsync(PlaybackSession session)
    {
        var animation = session.Animation;
        var totalFrameCount = animation.LoopMode == AnimationLoopMode.Finite
            ? checked((long)animation.Frames.Count * animation.FiniteLoopCount)
            : long.MaxValue;
        var completedFrames = 1L;
        var nextFrameIndex = animation.Frames.Count == 1 ? 0 : 1;
        var deadline = clock.Add(session.StartTimestamp, animation.Frames[0].Duration);

        try
        {
            while (true)
            {
                if (completedFrames >= totalFrameCount)
                {
                    await AwaitDeadlineAsync(session, deadline).ConfigureAwait(false);
                    break;
                }

                deadline = await AwaitDeadlineAsync(session, deadline).ConfigureAwait(false);
                session.Cancellation.Token.ThrowIfCancellationRequested();
                if (commandTransport.TransportState != MaskCommandTransportState.Ready)
                {
                    session.RequestedTerminalState = AnimationPlaybackState.Disconnected;
                    break;
                }

                var now = clock.GetTimestamp();
                while (completedFrames < totalFrameCount)
                {
                    var candidate = animation.Frames[nextFrameIndex];
                    var candidateEnd = clock.Add(deadline, candidate.Duration);
                    if (candidateEnd > now)
                    {
                        break;
                    }

                    session.FramesDropped++;
                    completedFrames++;
                    deadline = candidateEnd;
                    nextFrameIndex = (nextFrameIndex + 1) % animation.Frames.Count;
                }

                if (completedFrames >= totalFrameCount)
                {
                    break;
                }

                now = clock.GetTimestamp();
                var lateness = clock.GetElapsedTime(deadline, now);
                if (lateness > TimeSpan.Zero)
                {
                    session.LateFrames++;
                    session.MaximumLateness = session.MaximumLateness > lateness
                        ? session.MaximumLateness
                        : lateness;
                }

                var frame = animation.Frames[nextFrameIndex];
                var result = await TrySendFrameAsync(session, frame, session.Cancellation.Token)
                    .ConfigureAwait(false);
                if (result is null)
                {
                    continue;
                }

                if (!result.Succeeded)
                {
                    session.LastError = result.Message;
                    session.RequestedTerminalState = AnimationPlaybackState.Faulted;
                    break;
                }

                session.FramesSent++;
                completedFrames++;
                deadline = clock.Add(deadline, frame.Duration);
                nextFrameIndex = (nextFrameIndex + 1) % animation.Frames.Count;
                UpdateSessionSnapshot(session, AnimationPlaybackState.Playing);
            }

            var terminalState = session.RequestedTerminalState ?? AnimationPlaybackState.Completed;
            if (terminalState == AnimationPlaybackState.Completed && session.Request.RestoreAfterFinitePlayback)
            {
                await RestorePreviousLookOnceAsync(session, CancellationToken.None).ConfigureAwait(false);
            }

            CompleteSession(session, terminalState, session.LastError);
        }
        catch (OperationCanceledException) when (session.Cancellation.IsCancellationRequested)
        {
            CompleteSession(
                session,
                session.RequestedTerminalState ?? AnimationPlaybackState.Stopped,
                session.LastError);
        }
        catch (Exception exception)
        {
            CompleteSession(session, AnimationPlaybackState.Faulted, exception.Message);
        }
    }

    private async Task<long> AwaitDeadlineAsync(PlaybackSession session, long deadline)
    {
        while (true)
        {
            Task stateChanged;
            TimeSpan deadlineShift;
            bool isPaused;
            lock (sync)
            {
                deadlineShift = session.PendingDeadlineShift;
                session.PendingDeadlineShift = TimeSpan.Zero;
                if (deadlineShift > TimeSpan.Zero)
                {
                    deadline = clock.Add(deadline, deadlineShift);
                }

                isPaused = session.IsPaused;
                stateChanged = session.StateChanged.Task;
            }

            if (isPaused)
            {
                await stateChanged.WaitAsync(session.Cancellation.Token).ConfigureAwait(false);
                continue;
            }

            using var delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(session.Cancellation.Token);
            var delay = clock.DelayUntilAsync(deadline, delayCancellation.Token);
            var completed = await Task.WhenAny(delay, stateChanged).ConfigureAwait(false);
            if (ReferenceEquals(completed, delay))
            {
                await delay.ConfigureAwait(false);
                return deadline;
            }

            delayCancellation.Cancel();
            try
            {
                await delay.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (delayCancellation.IsCancellationRequested)
            {
            }
        }
    }

    private Task<MaskCommandResult> SendFrameCoreAsync(
        PerformanceAnimationFrame frame,
        CancellationToken cancellationToken) =>
        commandTransport.SendAsync(FaceUploadProtocol.BuildPlayCommand([frame.Slot]), cancellationToken);

    private async Task<MaskCommandResult?> TrySendFrameAsync(
        PlaybackSession session,
        PerformanceAnimationFrame frame,
        CancellationToken cancellationToken)
    {
        await frameSendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            lock (sync)
            {
                if (!ReferenceEquals(currentSession, session)
                    || session.IsPaused
                    || session.Cancellation.IsCancellationRequested)
                {
                    return null;
                }
            }

            var result = await SendFrameCoreAsync(frame, cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
            {
                lock (sync)
                {
                    if (ReferenceEquals(currentSession, session))
                    {
                        session.CurrentFrameSlot = frame.Slot;
                    }
                }
            }

            return result;
        }
        finally
        {
            frameSendGate.Release();
        }
    }

    private static IReadOnlyList<int> CreateFirmwarePlaybackSlots(PerformanceAnimation animation)
    {
        if (animation.Frames.Count <= MaxFirmwarePlaybackSteps)
        {
            return animation.Frames.Select(frame => frame.Slot).ToArray();
        }

        return animation.Frames
            .Take(MaxFirmwarePlaybackSteps)
            .Select(frame => frame.Slot)
            .ToArray();
    }

    private void UpdateBackgroundPlaybackError(PlaybackSession session, string message)
    {
        lock (sync)
        {
            if (!ReferenceEquals(currentSession, session))
            {
                return;
            }

            session.LastError = message;
            snapshot = CreateSnapshot(session, AnimationPlaybackState.Backgrounded);
        }

        PublishSnapshot();
    }

    private async Task StopCurrentSessionCoreAsync(bool restorePreviousLook)
    {
        PlaybackSession? session;
        lock (sync)
        {
            session = currentSession;
            if (session is null)
            {
                return;
            }

            session.RequestedTerminalState = AnimationPlaybackState.Stopped;
            session.Cancellation.Cancel();
            SignalStateChangedLocked(session);
        }

        await AwaitSessionAsync(session).ConfigureAwait(false);
        if (restorePreviousLook)
        {
            await RestorePreviousLookOnceAsync(session, CancellationToken.None).ConfigureAwait(false);
        }

        if (!restorePreviousLook || !session.RestoreSucceeded)
        {
            await ReclaimMaskOwnedPlaybackIfNeededAsync(session).ConfigureAwait(false);
        }

        CompleteSession(session, AnimationPlaybackState.Stopped, session.LastError);
    }

    private async Task ReclaimMaskOwnedPlaybackIfNeededAsync(PlaybackSession session)
    {
        int slot;
        lock (sync)
        {
            if (!session.IsBackgrounded
                || !session.PausedForBackground
                || session.Animation.LoopMode != AnimationLoopMode.Continuous)
            {
                return;
            }

            slot = session.CurrentFrameSlot;
            session.MaskOwnsPlayback = false;
        }

        await commandTransport.SendAsync(
            FaceUploadProtocol.BuildPlayCommand([slot]),
            CancellationToken.None).ConfigureAwait(false);
    }

    private static async Task AwaitSessionAsync(PlaybackSession session)
    {
        try
        {
            await session.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (session.Cancellation.IsCancellationRequested)
        {
        }
    }

    private static async Task RestorePreviousLookOnceAsync(
        PlaybackSession session,
        CancellationToken cancellationToken)
    {
        var restore = session.Request.RestorePreviousLookAsync;
        if (restore is null || Interlocked.Exchange(ref session.RestoreAttempted, 1) != 0)
        {
            return;
        }

        var result = await restore(cancellationToken).ConfigureAwait(false);
        session.RestoreSucceeded = result.Succeeded;
        if (!result.Succeeded)
        {
            session.LastError = $"Previous look could not be restored: {result.Message}";
        }
    }

    private void OnTransportStateChanged(
        object? sender,
        MaskCommandTransportStateChangedEventArgs eventArgs)
    {
        if (eventArgs.State == MaskCommandTransportState.Ready)
        {
            return;
        }

        lock (sync)
        {
            if (currentSession is null)
            {
                return;
            }

            currentSession.RequestedTerminalState = AnimationPlaybackState.Disconnected;
            currentSession.LastError = eventArgs.Message;
            currentSession.Cancellation.Cancel();
            SignalStateChangedLocked(currentSession);
        }
    }

    private void UpdateSessionSnapshot(PlaybackSession session, AnimationPlaybackState state)
    {
        lock (sync)
        {
            if (!ReferenceEquals(currentSession, session))
            {
                return;
            }

            snapshot = CreateSnapshot(session, state);
        }

        PublishSnapshot();
    }

    private void CompleteSession(
        PlaybackSession session,
        AnimationPlaybackState state,
        string error)
    {
        var publish = false;
        lock (sync)
        {
            if (ReferenceEquals(currentSession, session))
            {
                session.LastError = error;
                currentSession = null;
                snapshot = CreateSnapshot(session, state);
                publish = true;
            }
        }

        if (publish)
        {
            PublishSnapshot();
            session.Cancellation.Dispose();
        }
    }

    private AnimationPlaybackSnapshot CreateSnapshot(
        PlaybackSession session,
        AnimationPlaybackState state) =>
        new()
        {
            State = state,
            AnimationId = session.Animation.Id,
            RevisionHash = session.Animation.RevisionHash,
            FramesSent = session.FramesSent,
            FramesDropped = session.FramesDropped,
            LateFrames = session.LateFrames,
            MaximumLateness = session.MaximumLateness,
            StartedAt = session.StartedAt,
            LastError = session.LastError
        };

    private void SignalStateChangedLocked(PlaybackSession session)
    {
        var previous = session.StateChanged;
        session.StateChanged = PlaybackSession.CreateSignal();
        previous.TrySetResult(true);
    }

    private void PublishSnapshot()
    {
        AnimationPlaybackSnapshot current;
        lock (sync)
        {
            current = snapshot;
        }

        SnapshotChanged?.Invoke(this, new AnimationPlaybackSnapshotChangedEventArgs(current));
    }

    private void PublishSnapshotDeferred() => _ = Task.Run(PublishSnapshot);

    private sealed class PlaybackSession
    {
        public PlaybackSession(
            long id,
            PerformanceAnimation animation,
            AnimationPlaybackRequest request,
            CancellationToken cancellationToken)
        {
            Id = id;
            Animation = animation;
            Request = request;
            Cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            StartedAt = DateTimeOffset.UtcNow;
            CurrentFrameSlot = animation.Frames[0].Slot;
        }

        public long Id { get; }

        public PerformanceAnimation Animation { get; }

        public AnimationPlaybackRequest Request { get; }

        public CancellationTokenSource Cancellation { get; }

        public DateTimeOffset StartedAt { get; }

        public Task Task { get; set; } = Task.CompletedTask;

        public TaskCompletionSource<bool> StateChanged { get; set; } = CreateSignal();

        public long StartTimestamp { get; set; }

        public long PauseStartedTimestamp { get; set; }

        public TimeSpan PendingDeadlineShift { get; set; }

        public bool IsPaused { get; set; }

        public bool IsBackgrounded { get; set; }

        public bool PausedForBackground { get; set; }

        public bool MaskOwnsPlayback { get; set; }

        public int CurrentFrameSlot { get; set; }

        public long FramesSent { get; set; }

        public long FramesDropped { get; set; }

        public long LateFrames { get; set; }

        public TimeSpan MaximumLateness { get; set; }

        public AnimationPlaybackState? RequestedTerminalState { get; set; }

        public string LastError { get; set; } = string.Empty;

        public int RestoreAttempted;

        public bool RestoreSucceeded { get; set; }

        public static TaskCompletionSource<bool> CreateSignal() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
