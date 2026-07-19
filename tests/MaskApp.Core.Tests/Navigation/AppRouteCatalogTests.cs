using MaskApp.Core.Navigation;

namespace MaskApp.Core.Tests.Navigation;

public sealed class AppRouteCatalogTests
{
    [Fact]
    public void RegisteredRoutes_AreUniqueAndContainNoObsoleteConnectRoute()
    {
        Assert.Equal(
            AppRouteCatalog.AllRegisteredRoutes.Count,
            AppRouteCatalog.AllRegisteredRoutes.Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain(
            AppRouteCatalog.AllRegisteredRoutes,
            route => string.Equals(route, "connect", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DeviceDeepLink_ResolvesToRegisteredDeviceRoot()
    {
        Assert.Equal("//device", AppRouteCatalog.AbsoluteRoot(AppRouteCatalog.DeviceRoot));
    }

    [Fact]
    public void DetailRoute_CannotBeUsedAsAnAbsoluteRoot()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => AppRouteCatalog.AbsoluteRoot(AppRouteCatalog.Text));
    }
}
