namespace MaskApp.Core.Features.TextPresets;

public interface ITextPresetStore
{
    Task<TextPresetStoreState> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(TextPresetStoreState state, CancellationToken cancellationToken = default);

    async Task<TextPresetStoreState> UpsertAsync(TextPreset preset, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var updated = state.Upsert(preset).Normalize();
        await SaveAsync(updated, cancellationToken).ConfigureAwait(false);
        return updated;
    }

    async Task<TextPresetStoreState> DeleteAsync(TextPresetId id, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var updated = state.Delete(id).Normalize();
        await SaveAsync(updated, cancellationToken).ConfigureAwait(false);
        return updated;
    }
}
