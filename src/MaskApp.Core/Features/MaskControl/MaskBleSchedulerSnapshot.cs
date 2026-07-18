namespace MaskApp.Core.Features.MaskControl;

public sealed record MaskBleSchedulerSnapshot(
    long ConnectionGeneration,
    int PendingOperationCount,
    string? ActiveOperationName,
    long TotalEnqueued,
    long TotalCompleted,
    long TotalSuperseded,
    long TotalRejected,
    long TotalEmergencyCancellations,
    TimeSpan? LastOperationDuration,
    string? LastError);

public sealed class MaskBleSchedulerDiagnosticsChangedEventArgs : EventArgs
{
    public MaskBleSchedulerDiagnosticsChangedEventArgs(MaskBleSchedulerSnapshot snapshot)
    {
        Snapshot = snapshot;
    }

    public MaskBleSchedulerSnapshot Snapshot { get; }
}
