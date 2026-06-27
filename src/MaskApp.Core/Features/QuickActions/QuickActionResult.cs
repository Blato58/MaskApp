namespace MaskApp.Core.Features.QuickActions;

public sealed record QuickActionResult(
    QuickActionId ActionId,
    bool Succeeded,
    string Message,
    string Status)
{
    public static QuickActionResult Sent(QuickActionId actionId, string message) =>
        new(actionId, true, message, "sent");

    public static QuickActionResult Failed(QuickActionId actionId, string message, string status = "failed") =>
        new(actionId, false, message, status);
}
