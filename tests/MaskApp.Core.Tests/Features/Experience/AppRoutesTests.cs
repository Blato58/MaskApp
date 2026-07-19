using MaskApp.Core.Features.Experience;

namespace MaskApp.Core.Tests.Features.Experience;

public sealed class AppRoutesTests
{
    [Fact]
    public void RootRoutes_UseFourProductJobs()
    {
        Assert.Equal("//live", AppRoutes.LiveRoot);
        Assert.Equal("//library", AppRoutes.LibraryRoot);
        Assert.Equal("//shows", AppRoutes.ShowsRoot);
        Assert.Equal("//device", AppRoutes.DeviceRoot);
    }

    [Fact]
    public void DeepRoutes_EscapeStableIdentifiers()
    {
        Assert.Equal("deck-editor?deckId=holy%20priest", AppRoutes.ForDeckEditor("holy priest"));
        Assert.Equal("preflight?scope=live-deck&sourceId=deck%2Fone", AppRoutes.ForPreflight("live-deck", "deck/one"));
        Assert.Equal("maskpack-transfer?mode=import", AppRoutes.ForMaskPackTransfer("import"));
    }
}
