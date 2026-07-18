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
        Assert.Equal(9, page.Items.Count);
        Assert.Equal(9, page.Items.Select(item => item.SlotId).Distinct(StringComparer.Ordinal).Count());

        var resolvableIds = FacePatternFactory.CreateBuiltIns()
            .Select(face => $"face:{face.Id}")
            .Concat(AppBuiltInAnimationCatalog.CreateBuiltIns().Select(animation => $"app-animation:{animation.Id}"))
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(page.Items, item => Assert.Contains(item.GalleryItemId, resolvableIds));
    }

    [Fact]
    public void Normalize_RefreshesExistingVersionThreeHolyPriestCatalog()
    {
        var legacy = new GalleryLayoutState
        {
            SchemaVersion = 3,
            Pages =
            [
                new GalleryPageLayout
                {
                    PageId = BuiltInGalleryPages.HolyPriestPageId,
                    Title = "Customized old title",
                    ColorHex = "#123456",
                    SortIndex = 7,
                    Items =
                    [
                        new GalleryPageItemLayout
                        {
                            GalleryItemId = "app-animation:holy-priest-no-balance",
                            Label = "Old animation"
                        },
                        new GalleryPageItemLayout
                        {
                            GalleryItemId = "quick:Blackout",
                            Label = "Keep my shortcut",
                            SortIndex = 1
                        }
                    ]
                }
            ]
        };

        var normalized = legacy.Normalize();

        var page = Assert.Single(normalized.Pages);
        Assert.Equal("Customized old title", page.Title);
        Assert.Equal("#123456", page.ColorHex);
        Assert.Equal(7, page.SortIndex);
        Assert.Equal(10, page.Items.Count);
        Assert.DoesNotContain(page.Items, item => item.GalleryItemId == "app-animation:holy-priest-no-balance");
        Assert.Contains(page.Items, item => item.Label == "Blue → Red → Black");
        Assert.Contains(page.Items, item => item.GalleryItemId == "quick:Blackout" && item.Label == "Keep my shortcut");
    }

    [Fact]
    public void Normalize_MigratesLegacyReferencesAcrossCustomPagesAndGalleryOrder()
    {
        var state = new GalleryLayoutState
        {
            Pages =
            [
                new GalleryPageLayout
                {
                    PageId = "custom",
                    Title = "Custom",
                    Items =
                    [
                        new GalleryPageItemLayout
                        {
                            GalleryItemId = "app-animation:holy-priest-red-mass",
                            Label = "Legacy animation",
                            FastMaskSlot = 17,
                            FastContentFingerprint = "stale",
                            FastPreparedAt = DateTimeOffset.UnixEpoch
                        },
                        new GalleryPageItemLayout
                        {
                            GalleryItemId = "face:built-in-face-holy-priest-retro-future",
                            Label = "Legacy face",
                            SortIndex = 1
                        }
                    ]
                }
            ],
            Order = new GalleryOrderState
            {
                ItemOrders =
                [
                    new GalleryItemOrder
                    {
                        ItemId = "app-animation:holy-priest-red-mass",
                        SortIndex = 4
                    }
                ]
            }
        };

        var normalized = state.Normalize();

        var page = Assert.Single(normalized.Pages);
        var migratedAnimation = Assert.Single(
            page.Items,
            item => item.GalleryItemId == "app-animation:holy-priest-blue-red-black");
        Assert.Null(migratedAnimation.FastMaskSlot);
        Assert.Empty(migratedAnimation.FastContentFingerprint);
        Assert.Null(migratedAnimation.FastPreparedAt);
        Assert.Contains(page.Items, item => item.GalleryItemId == "face:built-in-face-holy-priest-original");
        Assert.Equal("app-animation:holy-priest-blue-red-black", Assert.Single(normalized.Order.ItemOrders).ItemId);
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

    [Theory]
    [InlineData(3)]
    [InlineData(GalleryLayoutState.CurrentSchemaVersion)]
    public void Normalize_DoesNotRestoreDeletedBuiltInPageAfterItsIntroduction(int schemaVersion)
    {
        var current = new GalleryLayoutState
        {
            SchemaVersion = schemaVersion,
            Pages = [new GalleryPageLayout { PageId = "live", Title = "Live", SortIndex = 0 }]
        };

        var normalized = current.Normalize();

        Assert.Single(normalized.Pages);
        Assert.DoesNotContain(normalized.Pages, BuiltInGalleryPages.IsHolyPriestPage);
    }
}
