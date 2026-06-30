namespace MaskApp.Core.Features.Gallery;

public sealed class InMemoryGalleryLayoutStore : IGalleryLayoutStore
{
    private GalleryLayoutState state;

    public InMemoryGalleryLayoutStore()
        : this(new GalleryLayoutState())
    {
    }

    public InMemoryGalleryLayoutStore(GalleryLayoutState state)
    {
        this.state = state.Normalize();
    }

    public Task<GalleryLayoutState> LoadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(state.Normalize());

    public Task SaveAsync(GalleryLayoutState state, CancellationToken cancellationToken = default)
    {
        this.state = state.Normalize();
        return Task.CompletedTask;
    }
}
