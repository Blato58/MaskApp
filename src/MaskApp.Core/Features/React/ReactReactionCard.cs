using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.QuickActions;

namespace MaskApp.Core.Features.React;

public sealed class ReactReactionCard
{
    public ReactReactionCard(ReactDeckCard card, AsyncRelayCommand sendCommand)
    {
        Id = card.Id;
        Category = card.Category;
        Kind = card.Kind;
        Label = card.Label;
        Caption = card.Caption;
        Description = card.Description;
        IsAlwaysVisible = card.IsAlwaysVisible;
        IsRandom = card.IsRandom;
        SendCommand = sendCommand;
    }

    public QuickActionId Id { get; }

    public QuickActionCategory Category { get; }

    public QuickActionKind Kind { get; }

    public string Label { get; }

    public string Caption { get; }

    public string Description { get; }

    public bool IsAlwaysVisible { get; }

    public bool IsRandom { get; }

    public AsyncRelayCommand SendCommand { get; }
}
