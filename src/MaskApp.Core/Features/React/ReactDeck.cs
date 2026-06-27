using MaskApp.Core.Features.QuickActions;

namespace MaskApp.Core.Features.React;

public sealed record ReactDeck(
    IReadOnlyList<ReactDeckCard> PinnedCards,
    IReadOnlyList<ReactDeckGroup> Groups);

public sealed record ReactDeckGroup(
    QuickActionCategory Category,
    string Title,
    IReadOnlyList<ReactDeckCard> Cards);

public sealed record ReactDeckCard(
    QuickActionId Id,
    QuickActionCategory Category,
    QuickActionKind Kind,
    string Label,
    string Caption,
    string Description,
    bool IsAlwaysVisible,
    bool IsRandom);
