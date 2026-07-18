namespace MaskApp.Core.Features.Profiles;

public sealed class InMemoryMaskProfileStore : IMaskProfileStore
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private MaskProfileStoreState state;

    public InMemoryMaskProfileStore(MaskProfileStoreState? initialState = null)
    {
        state = (initialState ?? new MaskProfileStoreState()).Normalize();
    }

    public async Task<MaskProfileStoreState> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return state.Normalize();
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(MaskProfileStoreState state, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            this.state = state.Normalize();
        }
        finally
        {
            gate.Release();
        }
    }
}
