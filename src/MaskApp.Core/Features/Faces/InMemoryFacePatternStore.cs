namespace MaskApp.Core.Features.Faces;

public sealed class InMemoryFacePatternStore : IFacePatternStore
{
    private FacePatternStoreState state;

    public InMemoryFacePatternStore()
        : this(FacePatternStoreState.Seeded)
    {
    }

    public InMemoryFacePatternStore(FacePatternStoreState state)
    {
        this.state = state.Normalize();
    }

    public Task<FacePatternStoreState> LoadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(state.Normalize());

    public Task SaveAsync(FacePatternStoreState state, CancellationToken cancellationToken = default)
    {
        this.state = state.Normalize();
        return Task.CompletedTask;
    }
}
