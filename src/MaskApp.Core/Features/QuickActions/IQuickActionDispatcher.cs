namespace MaskApp.Core.Features.QuickActions;

public interface IQuickActionDispatcher
{
    Task<QuickActionResult> TriggerAsync(
        QuickActionId actionId,
        QuickActionRequest? request = null,
        CancellationToken cancellationToken = default);
}
