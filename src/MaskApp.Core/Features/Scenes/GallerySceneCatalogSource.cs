using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Scenes;

public sealed class GallerySceneCatalogSource : ISceneCatalogSource
{
    private readonly QuickActionCatalog quickActionCatalog;
    private readonly ITextPresetStore textPresetStore;
    private readonly IBuiltInAssetArchiveStore builtInArchiveStore;
    private readonly IFacePatternStore facePatternStore;
    private readonly IGalleryLayoutStore galleryLayoutStore;
    private readonly IAnimationProjectStore animationProjectStore;
    private readonly ISceneShowStore sceneShowStore;

    public GallerySceneCatalogSource(
        QuickActionCatalog quickActionCatalog,
        ITextPresetStore textPresetStore,
        IBuiltInAssetArchiveStore builtInArchiveStore,
        IFacePatternStore facePatternStore,
        IGalleryLayoutStore galleryLayoutStore,
        IAnimationProjectStore animationProjectStore,
        ISceneShowStore sceneShowStore)
    {
        this.quickActionCatalog = quickActionCatalog;
        this.textPresetStore = textPresetStore;
        this.builtInArchiveStore = builtInArchiveStore;
        this.facePatternStore = facePatternStore;
        this.galleryLayoutStore = galleryLayoutStore;
        this.animationProjectStore = animationProjectStore;
        this.sceneShowStore = sceneShowStore;
    }

    public async Task<IReadOnlyList<GalleryItem>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var text = await textPresetStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var builtIns = await builtInArchiveStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var faces = await facePatternStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var layout = await galleryLayoutStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var animations = await animationProjectStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var scenes = await sceneShowStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        return new GalleryCatalogBuilder(quickActionCatalog).Build(
            text,
            builtIns,
            faces,
            layout.Order,
            animations,
            scenes);
    }
}
