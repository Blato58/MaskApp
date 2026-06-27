using MaskApp.Core.Features.QuickActions;

namespace MaskApp.Core.Tests.Features.QuickActions;

public sealed class QuickActionCatalogTests
{
    [Fact]
    public void Actions_ExposeStableIdsForRaveFastMvp()
    {
        var catalog = new QuickActionCatalog();

        foreach (var id in Enum.GetValues<QuickActionId>())
        {
            Assert.Contains(catalog.Actions, action => action.Id == id);
        }
    }

    [Fact]
    public void RaveActions_AreGroupedForManualFestivalUse()
    {
        var catalog = new QuickActionCatalog();

        var raveLabels = catalog.ByCategory(QuickActionCategory.Rave).Select(action => action.Label).ToArray();

        Assert.Contains("DROP", raveLabels);
        Assert.Contains("WHEEL UP", raveLabels);
        Assert.Contains("TOO MUCH BASS", raveLabels);
    }

    [Fact]
    public void BuiltInFallbacks_AreClearlyLabeledAsTestActions()
    {
        var catalog = new QuickActionCatalog();

        var builtIns = catalog.ByCategory(QuickActionCategory.BuiltIn);

        Assert.Equal(
            ["Test Image 1", "Test Image 2", "Test Anim 1", "Test Anim 2"],
            builtIns.Select(action => action.Label).ToArray());
        Assert.All(builtIns, action => Assert.True(action.BuiltInId is 1 or 2));
    }
}
