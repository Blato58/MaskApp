using MaskApp.Core.Features.QuickActions;

namespace MaskApp.Core.Features.React;

public sealed record ReactReactionGroup(
    QuickActionCategory Category,
    string Title,
    IReadOnlyList<ReactReactionCard> Cards);
