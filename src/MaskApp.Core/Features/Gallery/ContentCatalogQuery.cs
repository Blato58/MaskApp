using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Gallery;

public sealed record ContentCatalogSnapshot(
    IReadOnlyList<GalleryItem> Items,
    GalleryLayoutState Layout,
    TextPresetStoreState Text,
    BuiltInAssetArchive BuiltIns,
    FacePatternStoreState Faces,
    AnimationProjectStoreState Animations,
    SceneShowState Shows);

public sealed class ContentCatalogQuery(
    QuickActionCatalog quickActionCatalog,
    ITextPresetStore textPresetStore,
    IBuiltInAssetArchiveStore builtInArchiveStore,
    IFacePatternStore facePatternStore,
    IGalleryLayoutStore galleryLayoutStore,
    IAnimationProjectStore animationProjectStore,
    ISceneShowStore sceneShowStore)
{
    private readonly GalleryCatalogBuilder builder = new(quickActionCatalog);

    public async Task<ContentCatalogSnapshot> LoadAsync(CancellationToken cancellationToken = default)
    {
        var layout = (await galleryLayoutStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        var text = await textPresetStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var builtIns = await builtInArchiveStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var faces = (await facePatternStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        var animations = await animationProjectStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var shows = (await sceneShowStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        var items = builder.Build(text, builtIns, faces, layout.Order, animations, shows);
        return new ContentCatalogSnapshot(items, layout, text, builtIns, faces, animations, shows);
    }
}
