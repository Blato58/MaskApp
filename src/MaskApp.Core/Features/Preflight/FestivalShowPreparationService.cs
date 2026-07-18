using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;

namespace MaskApp.Core.Features.Preflight;

public sealed class FestivalShowPreparationService
{
    private readonly IFacePatternStore facePatternStore;
    private readonly IFaceUploadTransport faceUploadTransport;

    public FestivalShowPreparationService(
        IFacePatternStore facePatternStore,
        IFaceUploadTransport faceUploadTransport)
    {
        this.facePatternStore = facePatternStore;
        this.faceUploadTransport = faceUploadTransport;
    }

    public async Task<FestivalShowPreparationResult> PrepareAsync(
        FestivalPreflightReport report,
        IReadOnlyList<GalleryItem> catalog,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(catalog);
        if (report.Status == FestivalPreflightStatus.NotReady)
        {
            return FestivalShowPreparationResult.Failure(
                "Resolve every blocking Preflight issue before preparation.",
                uploadedSlotCount: 0,
                reusedSlotCount: report.SlotAllocations.Count(allocation => allocation.IsPrepared));
        }

        var missing = report.SlotAllocations
            .Where(allocation => !allocation.IsPrepared)
            .GroupBy(allocation => allocation.ContentFingerprint, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        var reusedSlotCount = report.SlotAllocations
            .Where(allocation => allocation.IsPrepared)
            .Select(allocation => allocation.ContentFingerprint)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (missing.Length == 0)
        {
            return FestivalShowPreparationResult.Success(
                "Every persistent DIY frame in this scope is already prepared.",
                uploadedSlotCount: 0,
                reusedSlotCount);
        }

        if (!faceUploadTransport.IsReady)
        {
            return FestivalShowPreparationResult.Failure(
                "Connect the active mask before preparing DIY content.",
                uploadedSlotCount: 0,
                reusedSlotCount);
        }

        var patternsByFingerprint = BuildPatternLookup(catalog);
        var unresolved = missing.FirstOrDefault(allocation =>
            allocation.AssignedSlot != allocation.PreferredSlot
            || !patternsByFingerprint.ContainsKey(allocation.ContentFingerprint));
        if (unresolved is not null)
        {
            return FestivalShowPreparationResult.Failure(
                unresolved.AssignedSlot != unresolved.PreferredSlot
                    ? $"{unresolved.SourceId} requires a remapped playback slot; resolve the collision first."
                    : $"The source pixels for {unresolved.SourceId} are unavailable.",
                uploadedSlotCount: 0,
                reusedSlotCount);
        }

        await FaceUploadOperationLock.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var options = (faceUploadTransport.SupportsAcknowledgements
                ? FaceUploadOptions.RequireAcknowledgements
                : FaceUploadOptions.WriteOnlyCompatibility) with
            {
                PlayAfterUpload = false
            };
            var uploadedSlotCount = 0;
            foreach (var allocation in missing)
            {
                var state = (await facePatternStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
                state = state.ClearSlotInstallation(allocation.AssignedSlot);
                await facePatternStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);

                var pattern = patternsByFingerprint[allocation.ContentFingerprint] with
                {
                    PreferredSlot = allocation.AssignedSlot
                };
                var package = FaceUploadProtocol.CreatePackage(pattern, allocation.AssignedSlot);
                var upload = await faceUploadTransport
                    .UploadAsync(package, options, cancellationToken)
                    .ConfigureAwait(false);
                state = (await facePatternStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
                if (!upload.Succeeded)
                {
                    await facePatternStore
                        .SaveAsync(state.ClearSlotInstallation(allocation.AssignedSlot), cancellationToken)
                        .ConfigureAwait(false);
                    return FestivalShowPreparationResult.Failure(
                        $"Preparation stopped at {allocation.SourceId}: {upload.Message}",
                        uploadedSlotCount,
                        reusedSlotCount);
                }

                var timestamp = DateTimeOffset.UtcNow;
                state = state.MarkSlotInstalled(
                    allocation.AssignedSlot,
                    allocation.ContentFingerprint,
                    allocation.SourceId,
                    timestamp);
                await facePatternStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
                uploadedSlotCount++;
            }

            return FestivalShowPreparationResult.Success(
                $"Prepared {uploadedSlotCount} DIY slot(s) for the active mask profile.",
                uploadedSlotCount,
                reusedSlotCount);
        }
        finally
        {
            FaceUploadOperationLock.Gate.Release();
        }
    }

    private static IReadOnlyDictionary<string, FacePattern> BuildPatternLookup(
        IReadOnlyList<GalleryItem> catalog)
    {
        var patterns = new Dictionary<string, FacePattern>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in catalog)
        {
            if (item.FacePattern is not null)
            {
                var pattern = item.FacePattern.Normalize();
                patterns.TryAdd(FaceContentFingerprint.Compute(pattern), pattern);
            }

            if (item.AppAnimation is not null)
            {
                foreach (var frame in item.AppAnimation.Normalize().Frames)
                {
                    patterns.TryAdd(FaceContentFingerprint.Compute(frame.Pattern), frame.Pattern);
                }
            }

            if (item.PerformanceAnimation is not null)
            {
                foreach (var frame in item.PerformanceAnimation.Normalize().StoredFrames)
                {
                    patterns.TryAdd(frame.ContentFingerprint, frame.Pattern);
                }
            }
        }

        return patterns;
    }
}
