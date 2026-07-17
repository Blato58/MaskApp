using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Faces;

public sealed class DiySlotPlaybackCoordinator
{
    private static readonly TimeSpan FastAnimationFrameInterval = TimeSpan.FromMilliseconds(75);

    private readonly IFacePatternStore facePatternStore;
    private readonly IFaceUploadTransport faceTransport;
    private readonly IMaskCommandTransport commandTransport;
    private readonly TimeSpan fastAnimationFrameInterval;
    private readonly SemaphoreSlim animationSessionGate = new(1, 1);
    private readonly object animationStateLock = new();
    private CancellationTokenSource? animationLoopCancellation;
    private Task animationLoopTask = Task.CompletedTask;
    private long animationStopVersion;

    public DiySlotPlaybackCoordinator(
        IFacePatternStore facePatternStore,
        IFaceUploadTransport faceTransport,
        IMaskCommandTransport commandTransport,
        TimeSpan? fastAnimationFrameInterval = null)
    {
        this.facePatternStore = facePatternStore;
        this.faceTransport = faceTransport;
        this.commandTransport = commandTransport;
        this.fastAnimationFrameInterval = fastAnimationFrameInterval ?? FastAnimationFrameInterval;
        if (this.fastAnimationFrameInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fastAnimationFrameInterval),
                "Animation frame interval must be greater than zero.");
        }

        commandTransport.TransportStateChanged += OnCommandTransportStateChanged;
    }

    public bool IsAnimationPlaying
    {
        get
        {
            lock (animationStateLock)
            {
                return animationLoopCancellation is not null && !animationLoopTask.IsCompleted;
            }
        }
    }

    public Task<DiySlotPlaybackResult> PlayFaceAsync(
        FacePattern pattern,
        CancellationToken cancellationToken = default)
    {
        var normalized = pattern.Normalize();
        return ExecuteAsync(
            normalized.DisplayName,
            [new SlotContent(normalized.PreferredSlot, normalized, $"face:{normalized.Id}", normalized.Id)],
            [normalized.PreferredSlot],
            playAfterPreparation: true,
            useRapidAnimationPlayback: false,
            forceUpload: false,
            cancellationToken);
    }

    public Task<DiySlotPlaybackResult> PrepareAnimationAsync(
        AppBuiltInAnimation animation,
        CancellationToken cancellationToken = default)
    {
        var normalized = animation.Normalize();
        return ExecuteAsync(
            normalized.DisplayName,
            normalized.Frames
                .Select((frame, index) => new SlotContent(
                    frame.Slot,
                    frame.Pattern,
                    $"animation:{normalized.Id}:frame:{index}",
                    null))
                .ToArray(),
            normalized.PlaybackSlots,
            playAfterPreparation: false,
            useRapidAnimationPlayback: true,
            forceUpload: false,
            cancellationToken);
    }

    public Task<DiySlotPlaybackResult> RefreshAnimationAsync(
        AppBuiltInAnimation animation,
        CancellationToken cancellationToken = default)
    {
        var normalized = animation.Normalize();
        return ExecuteAsync(
            normalized.DisplayName,
            normalized.Frames
                .Select((frame, index) => new SlotContent(
                    frame.Slot,
                    frame.Pattern,
                    $"animation:{normalized.Id}:frame:{index}",
                    null))
                .ToArray(),
            normalized.PlaybackSlots,
            playAfterPreparation: false,
            useRapidAnimationPlayback: true,
            forceUpload: true,
            cancellationToken);
    }

    public Task<DiySlotPlaybackResult> PlayAnimationAsync(
        AppBuiltInAnimation animation,
        CancellationToken cancellationToken = default)
    {
        var normalized = animation.Normalize();
        return ExecuteAsync(
            normalized.DisplayName,
            normalized.Frames
                .Select((frame, index) => new SlotContent(
                    frame.Slot,
                    frame.Pattern,
                    $"animation:{normalized.Id}:frame:{index}",
                    null))
                .ToArray(),
            normalized.PlaybackSlots,
            playAfterPreparation: true,
            useRapidAnimationPlayback: true,
            forceUpload: false,
            cancellationToken);
    }

    public static bool IsFacePrepared(FacePattern pattern, FacePatternStoreState state)
    {
        var normalized = pattern.Normalize();
        return InstallationMatches(
            state.Normalize(),
            normalized.PreferredSlot,
            FaceContentFingerprint.Compute(normalized));
    }

    public static bool IsAnimationPrepared(AppBuiltInAnimation animation, FacePatternStoreState state)
    {
        var normalizedAnimation = animation.Normalize();
        var normalizedState = state.Normalize();
        return normalizedAnimation.Frames.All(frame =>
            InstallationMatches(
                normalizedState,
                frame.Slot,
                FaceContentFingerprint.Compute(frame.Pattern)));
    }

    public async Task StopAnimationAsync(CancellationToken cancellationToken = default)
    {
        InvalidateAnimationSession();
        await animationSessionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await StopAnimationCoreAsync().ConfigureAwait(false);
        }
        finally
        {
            animationSessionGate.Release();
        }
    }

    public void RequestStopAnimation()
    {
        InvalidateAnimationSession();
    }

    private void InvalidateAnimationSession()
    {
        Interlocked.Increment(ref animationStopVersion);
        lock (animationStateLock)
        {
            animationLoopCancellation?.Cancel();
        }
    }

    private async Task<DiySlotPlaybackResult> ExecuteAsync(
        string displayName,
        IReadOnlyList<SlotContent> content,
        IReadOnlyList<int> playbackSlots,
        bool playAfterPreparation,
        bool useRapidAnimationPlayback,
        bool forceUpload,
        CancellationToken cancellationToken)
    {
        await StopAnimationAsync(cancellationToken).ConfigureAwait(false);
        var playbackVersion = Volatile.Read(ref animationStopVersion);

        if (playAfterPreparation && commandTransport.TransportState != MaskCommandTransportState.Ready)
        {
            return DiySlotPlaybackResult.Failure("Connect to play prepared DIY content");
        }

        await FaceUploadOperationLock.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var preparation = await PrepareLockedAsync(content, forceUpload, cancellationToken).ConfigureAwait(false);
            if (!preparation.Succeeded)
            {
                return DiySlotPlaybackResult.Failure(
                    preparation.Message,
                    preparation.UploadedSlotCount,
                    preparation.ReusedSlotCount);
            }

            if (!playAfterPreparation)
            {
                var preparedPlaybackLabel = useRapidAnimationPlayback ? "continuous rapid PLAY" : "PLAY";
                var message = preparation.UploadedSlotCount == 0
                    ? $"{displayName} is already prepared · {preparedPlaybackLabel} only"
                    : forceUpload
                        ? $"Refreshed {displayName} in {content.Count} DIY slots · ready for {preparedPlaybackLabel}"
                        : $"Prepared {displayName} once in {content.Count} DIY slots · later plays skip upload";
                return DiySlotPlaybackResult.Success(
                    message,
                    preparation.UploadedSlotCount,
                    preparation.ReusedSlotCount,
                    playCommandSent: false);
            }

            var playResult = await SendPlaybackAsync(
                playbackSlots,
                useRapidAnimationPlayback,
                playbackVersion,
                cancellationToken).ConfigureAwait(false);
            if (!playResult.Succeeded)
            {
                return DiySlotPlaybackResult.Failure(
                    playResult.Message,
                    preparation.UploadedSlotCount,
                    preparation.ReusedSlotCount);
            }

            var playedMessage = useRapidAnimationPlayback
                ? preparation.UploadedSlotCount == 0
                    ? $"Started continuous {fastAnimationFrameInterval.TotalMilliseconds:0} ms playback for {displayName} from prepared DIY slots · no upload; keep this page open"
                    : $"Uploaded {preparation.UploadedSlotCount} DIY slot(s) once and started continuous {fastAnimationFrameInterval.TotalMilliseconds:0} ms playback for {displayName} · keep this page open; later plays skip upload"
                : preparation.UploadedSlotCount == 0
                    ? $"Sent PLAY for {displayName} from prepared DIY slots · no upload; confirm on mask"
                    : $"Uploaded {preparation.UploadedSlotCount} DIY slot(s) once and sent PLAY for {displayName} · confirm on mask; later plays use PLAY only";
            return DiySlotPlaybackResult.Success(
                playedMessage,
                preparation.UploadedSlotCount,
                preparation.ReusedSlotCount,
                playCommandSent: true);
        }
        finally
        {
            FaceUploadOperationLock.Gate.Release();
        }
    }

    private async Task<MaskCommandResult> SendPlaybackAsync(
        IReadOnlyList<int> playbackSlots,
        bool useRapidAnimationPlayback,
        long playbackVersion,
        CancellationToken cancellationToken)
    {
        if (!useRapidAnimationPlayback)
        {
            return await commandTransport.SendAsync(
                FaceUploadProtocol.BuildPlayCommand(playbackSlots),
                cancellationToken).ConfigureAwait(false);
        }

        return await StartAnimationLoopAsync(
            playbackSlots,
            playbackVersion,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<MaskCommandResult> StartAnimationLoopAsync(
        IReadOnlyList<int> playbackSlots,
        long playbackVersion,
        CancellationToken cancellationToken)
    {
        if (playbackSlots.Count == 0)
        {
            return MaskCommandResult.Failure("Animation has no playback steps.");
        }

        await animationSessionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await StopAnimationCoreAsync().ConfigureAwait(false);
            if (playbackVersion != Volatile.Read(ref animationStopVersion))
            {
                return MaskCommandResult.Failure("Animation playback was stopped before it could start.");
            }

            var firstResult = await commandTransport.SendAsync(
                FaceUploadProtocol.BuildPlayCommand([playbackSlots[0]]),
                cancellationToken).ConfigureAwait(false);
            if (!firstResult.Succeeded)
            {
                return firstResult;
            }

            var loopCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var loopTask = RunAnimationLoopAsync(
                playbackSlots,
                1 % playbackSlots.Count,
                loopCancellation.Token);
            var started = false;
            lock (animationStateLock)
            {
                if (playbackVersion == Volatile.Read(ref animationStopVersion))
                {
                    animationLoopCancellation = loopCancellation;
                    animationLoopTask = loopTask;
                    started = true;
                }
            }

            if (!started)
            {
                loopCancellation.Cancel();
                await loopTask.ConfigureAwait(false);
                loopCancellation.Dispose();
                return MaskCommandResult.Failure("Animation playback was stopped before it could start.");
            }

            _ = ObserveAnimationLoopCompletionAsync(loopCancellation, loopTask);
            return firstResult;
        }
        finally
        {
            animationSessionGate.Release();
        }
    }

    private async Task RunAnimationLoopAsync(
        IReadOnlyList<int> playbackSlots,
        int nextSlotIndex,
        CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await Task.Delay(fastAnimationFrameInterval, cancellationToken).ConfigureAwait(false);
                if (commandTransport.TransportState != MaskCommandTransportState.Ready)
                {
                    return;
                }

                var result = await commandTransport.SendAsync(
                    FaceUploadProtocol.BuildPlayCommand([playbackSlots[nextSlotIndex]]),
                    cancellationToken).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    return;
                }

                nextSlotIndex = (nextSlotIndex + 1) % playbackSlots.Count;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            // A background transport failure ends this session; it must not fault a later foreground send.
        }
    }

    private async Task ObserveAnimationLoopCompletionAsync(
        CancellationTokenSource loopCancellation,
        Task loopTask)
    {
        await loopTask.ConfigureAwait(false);

        var ownsCompletedSession = false;
        lock (animationStateLock)
        {
            if (ReferenceEquals(animationLoopCancellation, loopCancellation) &&
                ReferenceEquals(animationLoopTask, loopTask))
            {
                animationLoopCancellation = null;
                animationLoopTask = Task.CompletedTask;
                ownsCompletedSession = true;
            }
        }

        if (ownsCompletedSession)
        {
            loopCancellation.Dispose();
        }
    }

    private async Task StopAnimationCoreAsync()
    {
        CancellationTokenSource? loopCancellation;
        Task loopTask;
        lock (animationStateLock)
        {
            loopCancellation = animationLoopCancellation;
            loopTask = animationLoopTask;
            animationLoopCancellation = null;
            animationLoopTask = Task.CompletedTask;
            loopCancellation?.Cancel();
        }

        if (loopCancellation is null)
        {
            return;
        }

        try
        {
            await loopTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (loopCancellation.IsCancellationRequested)
        {
        }
        finally
        {
            loopCancellation.Dispose();
        }
    }

    private void OnCommandTransportStateChanged(
        object? sender,
        MaskCommandTransportStateChangedEventArgs eventArgs)
    {
        if (eventArgs.State != MaskCommandTransportState.Ready)
        {
            RequestStopAnimation();
        }
    }

    private async Task<PreparationResult> PrepareLockedAsync(
        IReadOnlyList<SlotContent> content,
        bool forceUpload,
        CancellationToken cancellationToken)
    {
        var state = (await facePatternStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        var missing = forceUpload
            ? content.ToArray()
            : content
                .Where(frame => !InstallationMatches(state, frame.Slot, frame.ContentFingerprint))
                .ToArray();
        var reusedSlotCount = content.Count - missing.Length;

        if (missing.Length == 0)
        {
            return PreparationResult.Success(0, reusedSlotCount);
        }

        if (!faceTransport.IsReady)
        {
            return PreparationResult.Failure("Connect to prepare DIY slots", 0, reusedSlotCount);
        }

        if (forceUpload)
        {
            foreach (var frame in content)
            {
                state = state.ClearSlotInstallation(frame.Slot);
            }

            await facePatternStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        }

        var options = (faceTransport.SupportsAcknowledgements
            ? FaceUploadOptions.RequireAcknowledgements
            : FaceUploadOptions.WriteOnlyCompatibility) with
        {
            PlayAfterUpload = false
        };
        var uploadedSlotCount = 0;

        foreach (var frame in missing)
        {
            state = state.ClearSlotInstallation(frame.Slot);
            await facePatternStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);

            var package = FaceUploadProtocol.CreatePackage(frame.Pattern, frame.Slot);
            var uploadResult = await faceTransport.UploadAsync(package, options, cancellationToken).ConfigureAwait(false);
            state = (await facePatternStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            if (!uploadResult.Succeeded)
            {
                state = state.ClearSlotInstallation(frame.Slot);
                if (frame.LibraryPatternId is not null)
                {
                    state = state.MarkUploadFailed(frame.LibraryPatternId, uploadResult.Message);
                }

                await facePatternStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
                return PreparationResult.Failure(uploadResult.Message, uploadedSlotCount, reusedSlotCount);
            }

            uploadedSlotCount++;
            var timestamp = DateTimeOffset.UtcNow;
            if (frame.LibraryPatternId is not null)
            {
                state = state.MarkUploaded(
                    frame.LibraryPatternId,
                    $"Prepared DIY slot {frame.Slot}; later plays skip upload.",
                    timestamp);
            }

            state = state.MarkSlotInstalled(
                frame.Slot,
                frame.ContentFingerprint,
                frame.SourceId,
                timestamp);
            await facePatternStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        }

        return PreparationResult.Success(uploadedSlotCount, reusedSlotCount);
    }

    private static bool InstallationMatches(
        FacePatternStoreState state,
        int slot,
        string contentFingerprint)
    {
        var installation = state.GetSlotInstallation(slot);
        return installation is not null &&
            string.Equals(
                installation.ContentFingerprint,
                contentFingerprint,
                StringComparison.OrdinalIgnoreCase);
    }

    private sealed record SlotContent(
        int Slot,
        FacePattern Pattern,
        string SourceId,
        string? LibraryPatternId)
    {
        public string ContentFingerprint { get; } = FaceContentFingerprint.Compute(Pattern);
    }

    private sealed record PreparationResult(
        bool Succeeded,
        string Message,
        int UploadedSlotCount,
        int ReusedSlotCount)
    {
        public static PreparationResult Success(int uploadedSlotCount, int reusedSlotCount) =>
            new(true, string.Empty, uploadedSlotCount, reusedSlotCount);

        public static PreparationResult Failure(
            string message,
            int uploadedSlotCount,
            int reusedSlotCount) =>
            new(false, message, uploadedSlotCount, reusedSlotCount);
    }
}
