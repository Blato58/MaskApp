namespace MaskApp.Core.Features.Faces;

public interface IFacePatternStore
{
    Task<FacePatternStoreState> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(FacePatternStoreState state, CancellationToken cancellationToken = default);

    async Task<FacePatternStoreState> UpsertAsync(FacePattern pattern, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var normalizedPattern = pattern.Normalize();
        var patterns = state.Patterns
            .Where(existing => !string.Equals(existing.Id, normalizedPattern.Id, StringComparison.Ordinal))
            .Append(normalizedPattern)
            .ToArray();
        var updated = state with { Patterns = patterns };
        await SaveAsync(updated, cancellationToken).ConfigureAwait(false);
        return updated.Normalize();
    }

    async Task<FacePatternStoreState> DeleteAsync(string patternId, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var updated = state with
        {
            Patterns = state.Patterns
                .Where(pattern => !string.Equals(pattern.Id, patternId, StringComparison.Ordinal) || pattern.IsBuiltIn)
                .ToArray()
        };
        await SaveAsync(updated, cancellationToken).ConfigureAwait(false);
        return updated.Normalize();
    }
}
