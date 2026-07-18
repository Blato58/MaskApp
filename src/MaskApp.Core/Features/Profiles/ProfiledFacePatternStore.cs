using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Profiles;

public sealed class ProfiledFacePatternStore : IFacePatternStore
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly IFacePatternStore inner;
    private readonly MaskProfileSession profileSession;

    public ProfiledFacePatternStore(IFacePatternStore inner, MaskProfileSession profileSession)
    {
        this.inner = inner;
        this.profileSession = profileSession;
    }

    public async Task<FacePatternStoreState> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await LoadCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(FacePatternStoreState state, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await SaveCoreAsync(state, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<FacePatternStoreState> UpsertAsync(
        FacePattern pattern,
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = await LoadCoreAsync(cancellationToken).ConfigureAwait(false);
            var normalizedPattern = pattern.Normalize();
            var updated = state with
            {
                Patterns = state.Patterns
                    .Where(existing => !string.Equals(existing.Id, normalizedPattern.Id, StringComparison.Ordinal))
                    .Append(normalizedPattern)
                    .ToArray()
            };
            await SaveCoreAsync(updated, cancellationToken).ConfigureAwait(false);
            return updated.Normalize();
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<FacePatternStoreState> DeleteAsync(
        string patternId,
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = await LoadCoreAsync(cancellationToken).ConfigureAwait(false);
            var updated = state with
            {
                Patterns = state.Patterns
                    .Where(pattern =>
                        !string.Equals(pattern.Id, patternId, StringComparison.Ordinal) || pattern.IsBuiltIn)
                    .ToArray()
            };
            await SaveCoreAsync(updated, cancellationToken).ConfigureAwait(false);
            return updated.Normalize();
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<FacePatternStoreState> LoadCoreAsync(CancellationToken cancellationToken)
    {
        var state = (await inner.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        var profile = await profileSession.GetActiveProfileAsync(cancellationToken).ConfigureAwait(false);
        return state with
        {
            SlotInstallations = profile?.PreparedSlots
                .Select(slot => slot.ToFaceSlotInstallation())
                .ToArray() ?? []
        };
    }

    private async Task SaveCoreAsync(
        FacePatternStoreState state,
        CancellationToken cancellationToken)
    {
        var activeProfile = await profileSession.GetActiveProfileAsync(cancellationToken).ConfigureAwait(false);
        if (activeProfile is not null)
        {
            await profileSession
                .ReplacePreparedSlotsAsync(state.Normalize().SlotInstallations, cancellationToken)
                .ConfigureAwait(false);
            await inner
                .SaveAsync(state with { SlotInstallations = [] }, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        var existing = (await inner.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        await inner
            .SaveAsync(state with { SlotInstallations = existing.SlotInstallations }, cancellationToken)
            .ConfigureAwait(false);
    }
}
