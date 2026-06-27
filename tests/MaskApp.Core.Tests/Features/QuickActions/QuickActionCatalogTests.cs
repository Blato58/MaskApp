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
}
