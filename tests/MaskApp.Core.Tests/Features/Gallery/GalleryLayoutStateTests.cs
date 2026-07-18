using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;

namespace MaskApp.Core.Tests.Features.Gallery;

public sealed class GalleryLayoutStateTests
{
    [Fact]
    public void Defaults_IncludeResolvableHolyPriestStagePage()
    {
        var state = new GalleryLayoutState().Normalize();

        var page = Assert.Single(state.Pages, BuiltInGalleryPages.IsHolyPriestPage);
        Assert.Equal("Holy Priest", page.Title);
        Assert.Equal(12, page.Items.Count);
        Assert.Equal(12, page.Items.Select(item => item.SlotId).Distinct(StringComparer.Ordinal).Count());

        var resolvableIds = FacePatternFactory.CreateBuiltIns()
            .Select(face => $"face:{face.Id}")
            .Concat(AppBuiltInAnimationCatalog.CreateBuiltIns().Select(animation => $"app-animation:{animation.Id}"))
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(page.Items, item => Assert.Contains(item.GalleryItemId, resolvableIds));
    }

    [Fact]
    public void Normalize_MigratesLegacyLayoutWithHolyPriestPageOnce()
    {
        var legacy = new GalleryLayoutState
        {
            SchemaVersion = 2,
            Pages =
            [
                new GalleryPageLayout { PageId = "live", Title = "Live", SortIndex = 0 },
                new GalleryPageLayout { PageId = "rave", Title = "RAVE", SortIndex = 1 }
            ]
        };

        var normalized = legacy.Normalize();

        Assert.Equal(GalleryLayoutState.CurrentSchemaVersion, normalized.SchemaVersion);
        Assert.Single(normalized.Pages, BuiltInGalleryPages.IsHolyPriestPage);
        Assert.Equal(3, normalized.Pages.Count);
    }

    [Fact]
    public void Normalize_DoesNotRestoreDeletedBuiltInPageAtCurrentSchema()
    {
        var current = new GalleryLayoutState
        {
            Pages = [new GalleryPageLayout { PageId = "live", Title = "Live", SortIndex = 0 }]
        };

        var normalized = current.Normalize();

        Assert.Single(normalized.Pages);
        Assert.DoesNotContain(normalized.Pages, BuiltInGalleryPages.IsHolyPriestPage);
    }
}
