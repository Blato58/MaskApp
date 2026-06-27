using MaskApp.Core.Features.Home;

namespace MaskApp.Core.Tests.Features.Home;

public sealed class HomeViewModelTests
{
    [Fact]
    public void FeatureCards_AreExposedInMigrationOrder()
    {
        var viewModel = new HomeViewModel();

        Assert.Equal(
            ["Connect", "Text", "Image", "Rhythm", "Microphone", "Settings"],
            viewModel.FeatureCards.Select(card => card.Name));
    }

    [Fact]
    public void Connect_IsAvailable()
    {
        var viewModel = new HomeViewModel();
        var connect = viewModel.FeatureCards.Single(card => card.Name == "Connect");

        Assert.True(connect.IsAvailable);
        Assert.Equal("Ready", connect.Status);
        Assert.Equal("Open", connect.ActionText);
    }

    [Fact]
    public void Text_IsAvailableAsMvpSlice()
    {
        var viewModel = new HomeViewModel();
        var text = viewModel.FeatureCards.Single(card => card.Name == "Text");

        Assert.True(text.IsAvailable);
        Assert.Equal("MVP", text.Status);
        Assert.Equal("Open", text.ActionText);
    }

    [Fact]
    public void PlannedFeatures_AreVisibleButLocked()
    {
        var viewModel = new HomeViewModel();
        var lockedFeatures = viewModel.FeatureCards.Where(card => card.Name is not "Connect" and not "Text").ToArray();

        Assert.NotEmpty(lockedFeatures);
        Assert.All(lockedFeatures, card => Assert.False(card.IsAvailable));
        Assert.All(lockedFeatures, card => Assert.Equal("Locked", card.ActionText));
    }
}
