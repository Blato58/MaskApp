using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Scenes;

public sealed class GallerySceneCatalogSource(ContentCatalogQuery query) : ISceneCatalogSource
{
    public async Task<IReadOnlyList<GalleryItem>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await query.LoadAsync(cancellationToken).ConfigureAwait(false);
        return snapshot.Items;
    }
}
