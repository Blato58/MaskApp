using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Faces;

public sealed class DiySlotPlaybackCoordinator
{
    private readonly IFacePatternStore facePatternStore;
    private readonly IFaceUploadTransport faceTransport;
    private readonly IMaskCommandTransport commandTransport;

    public DiySlotPlaybackCoordinator(
        IFacePatternStore facePatternStore,
        IFaceUploadTransport faceTransport,
        IMaskCommandTransport commandTransport)
    {
        this.facePatternStore = facePatternStore;
        this.faceTransport = faceTransport;
        this.commandTransport = commandTransport;
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

    private async Task<DiySlotPlaybackResult> ExecuteAsync(
        string displayName,
        IReadOnlyList<SlotContent> content,
        IReadOnlyList<int> playbackSlots,
        bool playAfterPreparation,
        bool forceUpload,
        CancellationToken cancellationToken)
    {
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
                var message = preparation.UploadedSlotCount == 0
                    ? $"{displayName} is already prepared · PLAY only"
                    : forceUpload
                        ? $"Refreshed {displayName} in {content.Count} DIY slots · ready for PLAY"
                        : $"Prepared {displayName} once in {content.Count} DIY slots · later plays skip upload";
                return DiySlotPlaybackResult.Success(
                    message,
                    preparation.UploadedSlotCount,
                    preparation.ReusedSlotCount,
                    playCommandSent: false);
            }

            var playResult = await commandTransport.SendAsync(
                FaceUploadProtocol.BuildPlayCommand(playbackSlots),
                cancellationToken).ConfigureAwait(false);
            if (!playResult.Succeeded)
            {
                return DiySlotPlaybackResult.Failure(
                    playResult.Message,
                    preparation.UploadedSlotCount,
                    preparation.ReusedSlotCount);
            }

            var playedMessage = preparation.UploadedSlotCount == 0
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
