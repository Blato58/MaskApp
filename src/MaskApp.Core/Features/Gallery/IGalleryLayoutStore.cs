namespace MaskApp.Core.Features.Gallery;

public interface IGalleryLayoutStore
{
    Task<GalleryLayoutState> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(GalleryLayoutState state, CancellationToken cancellationToken = default);
}
