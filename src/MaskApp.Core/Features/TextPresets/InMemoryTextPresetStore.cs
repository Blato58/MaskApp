namespace MaskApp.Core.Features.TextPresets;

public sealed class InMemoryTextPresetStore : ITextPresetStore
{
    private TextPresetStoreState state;

    public InMemoryTextPresetStore(TextPresetStoreState? state = null)
    {
        this.state = (state ?? TextPresetStoreState.Seeded).Normalize();
    }

    public Task<TextPresetStoreState> LoadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(state);

    public Task SaveAsync(TextPresetStoreState state, CancellationToken cancellationToken = default)
    {
        this.state = state.Normalize();
        return Task.CompletedTask;
    }

    public Task<TextPresetStoreState> UpsertAsync(TextPreset preset, CancellationToken cancellationToken = default)
    {
        state = state.Upsert(preset).Normalize();
        return Task.FromResult(state);
    }

    public Task<TextPresetStoreState> DeleteAsync(TextPresetId id, CancellationToken cancellationToken = default)
    {
        state = state.Delete(id).Normalize();
        return Task.FromResult(state);
    }
}
