namespace MaskApp.Core.Features.MaskControl;

public enum VisualWorkCancellationReason
{
    Stop,
    Blackout,
    ConnectionChanged,
    SchedulerStopped
}

public sealed class VisualWorkCancelledEventArgs(
    VisualWorkCancellationReason reason,
    string message) : EventArgs
{
    public VisualWorkCancellationReason Reason { get; } = reason;

    public string Message { get; } = message;
}

public interface IVisualWorkCancellationSource
{
    event EventHandler<VisualWorkCancelledEventArgs>? VisualWorkCancelled;
}
