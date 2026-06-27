namespace MaskApp.Core.Features.QuickActions;

public sealed record QuickActionDefinition(
    QuickActionId Id,
    string Label,
    QuickActionCategory Category,
    QuickActionKind Kind,
    string? Caption = null,
    int? Brightness = null,
    int? BuiltInId = null,
    bool IsRave = false);
