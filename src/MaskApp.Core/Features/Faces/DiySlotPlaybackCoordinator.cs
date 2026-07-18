using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Faces;

public sealed class DiySlotPlaybackCoordinator
{
    private static readonly TimeSpan FastAnimationFrameInterval = TimeSpan.FromMilliseconds(75);

    private readonly IFacePatternStore facePatternStore;
    private readonly IFaceUploadTransport faceTransport;
    private readonly IMaskCommandTransport commandTransport;
    private readonly PerformanceAnimationEngine animationEngine;
    private readonly PerformanceAnimationBuilder animationBuilder;
    private readonly object stableLookSync = new();
    private MaskCommand? lastStableLookCommand;
    private long animationStopVersion;

    public DiySlotPlaybackCoordinator(
        IFacePatternStore facePatternStore,
        IFaceUploadTransport faceTransport,
        IMaskCommandTransport commandTransport,
        TimeSpan? fastAnimationFrameInterval = null)
        : this(
            facePatternStore,
            faceTransport,
            commandTransport,
            new PerformanceAnimationEngine(commandTransport),
            new PerformanceAnimationBuilder(NormalizeAnimationInterval(fastAnimationFrameInterval)))
    {
    }

    private static TimeSpan NormalizeAnimationInterval(TimeSpan? interval)
    {
        var value = interval ?? FastAnimationFrameInterval;
        return value < PerformanceAnimation.MinFrameDuration
            ? PerformanceAnimation.MinFrameDuration
            : value > PerformanceAnimation.MaxFrameDuration
                ? PerformanceAnimation.MaxFrameDuration
                : value;
    }

    public DiySlotPlaybackCoordinator(
        IFacePatternStore facePatternStore,
        IFaceUploadTransport faceTransport,
        IMaskCommandTransport commandTransport,
        PerformanceAnimationEngine animationEngine,
        PerformanceAnimationBuilder animationBuilder)
    {
        this.facePatternStore = facePatternStore;
        this.faceTransport = faceTransport;
        this.commandTransport = commandTransport;
        this.animationEngine = animationEngine;
        this.animationBuilder = animationBuilder;
    }

    public bool IsAnimationPlaying => animationEngine.GetSnapshot().IsActive;

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
            animation: null,
            forceUpload: false,
            cancellationToken);
    }

    public Task<DiySlotPlaybackResult> PrepareAnimationAsync(
        AppBuiltInAnimation animation,
        CancellationToken cancellationToken = default)
    {
        var normalized = animation.Normalize();
        var performanceAnimation = animationBuilder.FromAppBuiltIn(normalized);
        return ExecuteAsync(
            normalized.DisplayName,
            performanceAnimation.StoredFrames
                .Select((frame, index) => new SlotContent(
                    frame.Slot,
                    frame.Pattern,
                    $"animation:{normalized.Id}:frame:{index}",
                    null))
                .ToArray(),
            performanceAnimation.Frames.Select(frame => frame.Slot).ToArray(),
            playAfterPreparation: false,
            performanceAnimation,
            forceUpload: false,
            cancellationToken);
    }

    public Task<DiySlotPlaybackResult> PrepareAnimationAsync(
        PerformanceAnimation animation,
        CancellationToken cancellationToken = default) =>
        ExecutePerformanceAnimationAsync(animation, playAfterPreparation: false, forceUpload: false, cancellationToken);

    public Task<DiySlotPlaybackResult> RefreshAnimationAsync(
        AppBuiltInAnimation animation,
        CancellationToken cancellationToken = default)
    {
        var normalized = animation.Normalize();
        var performanceAnimation = animationBuilder.FromAppBuiltIn(normalized);
        return ExecuteAsync(
            normalized.DisplayName,
            performanceAnimation.StoredFrames
                .Select((frame, index) => new SlotContent(
                    frame.Slot,
                    frame.Pattern,
                    $"animation:{normalized.Id}:frame:{index}",
                    null))
                .ToArray(),
            performanceAnimation.Frames.Select(frame => frame.Slot).ToArray(),
            playAfterPreparation: false,
            performanceAnimation,
            forceUpload: true,
            cancellationToken);
    }

    public Task<DiySlotPlaybackResult> RefreshAnimationAsync(
        PerformanceAnimation animation,
        CancellationToken cancellationToken = default) =>
        ExecutePerformanceAnimationAsync(animation, playAfterPreparation: false, forceUpload: true, cancellationToken);

    public Task<DiySlotPlaybackResult> PlayAnimationAsync(
        AppBuiltInAnimation animation,
        CancellationToken cancellationToken = default)
    {
        var normalized = animation.Normalize();
        var performanceAnimation = animationBuilder.FromAppBuiltIn(normalized);
        return ExecuteAsync(
            normalized.DisplayName,
            performanceAnimation.StoredFrames
                .Select((frame, index) => new SlotContent(
                    frame.Slot,
                    frame.Pattern,
                    $"animation:{normalized.Id}:frame:{index}",
                    null))
                .ToArray(),
            performanceAnimation.Frames.Select(frame => frame.Slot).ToArray(),
            playAfterPreparation: true,
            performanceAnimation,
            forceUpload: false,
            cancellationToken);
    }

    public Task<DiySlotPlaybackResult> PlayAnimationAsync(
        PerformanceAnimation animation,
        CancellationToken cancellationToken = default) =>
        ExecutePerformanceAnimationAsync(animation, playAfterPreparation: true, forceUpload: false, cancellationToken);

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
        var normalizedAnimation = new PerformanceAnimationBuilder().FromAppBuiltIn(animation);
        var normalizedState = state.Normalize();
        return normalizedAnimation.StoredFrames.All(frame =>
            InstallationMatches(
                normalizedState,
                frame.Slot,
                frame.ContentFingerprint));
    }

    public static bool IsAnimationPrepared(PerformanceAnimation animation, FacePatternStoreState state)
    {
        var normalizedAnimation = animation.Normalize();
        var normalizedState = state.Normalize();
        return normalizedAnimation.StoredFrames.All(frame =>
            InstallationMatches(
                normalizedState,
                frame.Slot,
                frame.ContentFingerprint));
    }

    public async Task StopAnimationAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref animationStopVersion);
        await animationEngine.StopPlaybackAsync(
            restorePreviousLook: true,
            cancellationToken).ConfigureAwait(false);
    }

    public void RequestStopAnimation()
    {
        InvalidateAnimationSession();
    }

    private void InvalidateAnimationSession()
    {
        Interlocked.Increment(ref animationStopVersion);
        animationEngine.RequestStopPlayback();
    }

    private Task<DiySlotPlaybackResult> ExecutePerformanceAnimationAsync(
        PerformanceAnimation source,
        bool playAfterPreparation,
        bool forceUpload,
        CancellationToken cancellationToken)
    {
        var animation = source.Normalize();
        if (string.IsNullOrWhiteSpace(animation.RevisionHash))
        {
            animation = animationBuilder.WithRevision(animation);
        }

        return ExecuteAsync(
            animation.DisplayName,
            animation.StoredFrames
                .Select((frame, index) => new SlotContent(
                    frame.Slot,
                    frame.Pattern,
                    $"animation:{animation.Id}:frame:{index}",
                    null))
                .ToArray(),
            animation.Frames.Select(frame => frame.Slot).ToArray(),
            playAfterPreparation,
            animation,
            forceUpload,
            cancellationToken);
    }

    private async Task<DiySlotPlaybackResult> ExecuteAsync(
        string displayName,
        IReadOnlyList<SlotContent> content,
        IReadOnlyList<int> playbackSlots,
        bool playAfterPreparation,
        PerformanceAnimation? animation,
        bool forceUpload,
        CancellationToken cancellationToken)
    {
        await StopAnimationForReplacementAsync(cancellationToken).ConfigureAwait(false);
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
                var preparedPlaybackLabel = animation is not null ? "continuous timed PLAY" : "PLAY";
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
                animation,
                playbackVersion,
                cancellationToken).ConfigureAwait(false);
            if (!playResult.Succeeded)
            {
                return DiySlotPlaybackResult.Failure(
                    playResult.Message,
                    preparation.UploadedSlotCount,
                    preparation.ReusedSlotCount);
            }

            var playedMessage = animation is not null
                ? preparation.UploadedSlotCount == 0
                    ? $"Started continuous deadline-timed playback for {displayName} from prepared DIY slots · no upload; keep this page open"
                    : $"Uploaded {preparation.UploadedSlotCount} DIY slot(s) once and started continuous deadline-timed playback for {displayName} · keep this page open; later plays skip upload"
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
        PerformanceAnimation? animation,
        long playbackVersion,
        CancellationToken cancellationToken)
    {
        if (animation is null)
        {
            var command = FaceUploadProtocol.BuildPlayCommand(playbackSlots);
            var staticResult = await commandTransport.SendAsync(command, cancellationToken).ConfigureAwait(false);
            if (staticResult.Succeeded)
            {
                lock (stableLookSync)
                {
                    lastStableLookCommand = command;
                }
            }

            return staticResult;
        }

        if (playbackVersion != Volatile.Read(ref animationStopVersion))
        {
            return MaskCommandResult.Failure("Animation playback was stopped before it could start.");
        }

        MaskCommand? previousLook;
        lock (stableLookSync)
        {
            previousLook = lastStableLookCommand;
        }

        var request = new AnimationPlaybackRequest
        {
            RestorePreviousLookAsync = previousLook is null
                ? null
                : token => commandTransport.SendAsync(previousLook, token)
        };
        var startResult = await animationEngine.StartAsync(animation, request, cancellationToken)
            .ConfigureAwait(false);
        return startResult.Succeeded
            ? MaskCommandResult.Success(startResult.Message)
            : MaskCommandResult.Failure(startResult.Message);
    }

    private async Task StopAnimationForReplacementAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref animationStopVersion);
        await animationEngine.StopPlaybackAsync(
            restorePreviousLook: false,
            cancellationToken).ConfigureAwait(false);
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
