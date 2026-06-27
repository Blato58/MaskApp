using MaskApp.Core.Features.QuickActions;

namespace MaskApp.Core.Features.React;

public sealed class ReactDeckCatalog
{
    private static readonly QuickActionId[] PinnedActionIds =
    [
        QuickActionId.Blackout,
        QuickActionId.RandomReaction
    ];

    private readonly QuickActionCatalog catalog;

    public ReactDeckCatalog(QuickActionCatalog catalog)
    {
        this.catalog = catalog;
    }

    public ReactDeck Build()
    {
        var pinnedCards = PinnedActionIds
            .Select(id => CreateCard(catalog.Get(id), isAlwaysVisible: true))
            .ToArray();

        var groups = catalog.Actions
            .Where(action => action.Kind == QuickActionKind.Text)
            .GroupBy(action => action.Category)
            .OrderBy(group => GetSortOrder(group.Key))
            .Select(group => new ReactDeckGroup(
                group.Key,
                GetGroupTitle(group.Key),
                group.Select(action => CreateCard(action, isAlwaysVisible: false)).ToArray()))
            .ToArray();

        return new ReactDeck(pinnedCards, groups);
    }

    private static ReactDeckCard CreateCard(QuickActionDefinition action, bool isAlwaysVisible)
    {
        var caption = action.Caption ?? action.Label;
        return new ReactDeckCard(
            action.Id,
            action.Category,
            action.Kind,
            action.Label,
            caption,
            BuildDescription(action, caption),
            isAlwaysVisible,
            action.Kind == QuickActionKind.Random);
    }

    private static string BuildDescription(QuickActionDefinition action, string caption) =>
        action.Kind switch
        {
            QuickActionKind.Command when action.Id == QuickActionId.Blackout => "Dim the mask fast.",
            QuickActionKind.Random => "Pick a surprise offline caption.",
            QuickActionKind.Text => action.Category switch
            {
                QuickActionCategory.Meme => $"Meme caption: {caption}",
                QuickActionCategory.Social => $"Social caption: {caption}",
                QuickActionCategory.Rave => $"Manual RAVE caption: {caption}",
                QuickActionCategory.Welfare => $"Check-in caption: {caption}",
                _ => $"Caption: {caption}"
            },
            _ => action.Label
        };

    private static string GetGroupTitle(QuickActionCategory category) =>
        category switch
        {
            QuickActionCategory.Meme => "Meme",
            QuickActionCategory.Social => "Social",
            QuickActionCategory.Rave => "RAVE",
            QuickActionCategory.Welfare => "Welfare",
            QuickActionCategory.General => "General",
            _ => category.ToString()
        };

    private static int GetSortOrder(QuickActionCategory category) =>
        category switch
        {
            QuickActionCategory.Meme => 0,
            QuickActionCategory.Social => 1,
            QuickActionCategory.Rave => 2,
            QuickActionCategory.Welfare => 3,
            QuickActionCategory.General => 4,
            _ => 5
        };
}
