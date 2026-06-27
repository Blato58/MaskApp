using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.React;

namespace MaskApp.Core.Tests.Features.React;

public sealed class ReactDeckCatalogTests
{
    [Fact]
    public void Build_PinsBlackoutAndRandomReaction()
    {
        var deck = new ReactDeckCatalog(new QuickActionCatalog()).Build();

        Assert.Collection(
            deck.PinnedCards,
            card =>
            {
                Assert.Equal(QuickActionId.Blackout, card.Id);
                Assert.True(card.IsAlwaysVisible);
                Assert.Equal("Dim the mask fast.", card.Description);
            },
            card =>
            {
                Assert.Equal(QuickActionId.RandomReaction, card.Id);
                Assert.True(card.IsRandom);
                Assert.Equal("Pick a surprise offline caption.", card.Description);
            });
    }

    [Fact]
    public void Build_GroupsTextReactionsByCatalogCategoryWithCaptions()
    {
        var deck = new ReactDeckCatalog(new QuickActionCatalog()).Build();

        var meme = Assert.Single(deck.Groups, group => group.Category == QuickActionCategory.Meme);
        Assert.Contains(meme.Cards, card =>
            card.Id == QuickActionId.Lol &&
            card.Caption == "LOL" &&
            card.Description == "Meme caption: LOL");

        var rave = Assert.Single(deck.Groups, group => group.Category == QuickActionCategory.Rave);
        Assert.Contains(rave.Cards, card =>
            card.Id == QuickActionId.Drop &&
            card.Caption == "DROP" &&
            card.Description == "Manual RAVE caption: DROP");
    }

    [Fact]
    public void Build_DoesNotDuplicatePinnedActionsInsideCategoryGroups()
    {
        var deck = new ReactDeckCatalog(new QuickActionCatalog()).Build();

        Assert.DoesNotContain(
            deck.Groups.SelectMany(group => group.Cards),
            card => card.Id is QuickActionId.Blackout or QuickActionId.RandomReaction);
    }
}
