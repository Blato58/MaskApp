using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Profiles;

public sealed class MaskProfileMetricsRecorder : IDisposable
{
    private readonly object sync = new();
    private readonly MaskBleScheduler scheduler;
    private readonly MaskProfileSession profileSession;
    private Task updateTask = Task.CompletedTask;
    private long observedCompletedOperations;
    private bool disposed;

    public MaskProfileMetricsRecorder(
        MaskBleScheduler scheduler,
        MaskProfileSession profileSession)
    {
        this.scheduler = scheduler;
        this.profileSession = profileSession;
        observedCompletedOperations = scheduler.GetSnapshot().TotalCompleted;
        scheduler.DiagnosticsChanged += HandleDiagnosticsChanged;
    }

    public string LastError { get; private set; } = string.Empty;

    public Task UpdateTask
    {
        get
        {
            lock (sync)
            {
                return updateTask;
            }
        }
    }

    public void Dispose()
    {
        lock (sync)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
        }

        scheduler.DiagnosticsChanged -= HandleDiagnosticsChanged;
    }

    private void HandleDiagnosticsChanged(
        object? sender,
        MaskBleSchedulerDiagnosticsChangedEventArgs args)
    {
        var snapshot = args.Snapshot;
        var profileId = profileSession.ActiveProfileId;
        if (snapshot.LastOperationSucceeded != true
            || snapshot.LastOperationDuration is not { } duration
            || duration <= TimeSpan.Zero
            || string.IsNullOrWhiteSpace(profileId))
        {
            return;
        }

        lock (sync)
        {
            if (disposed || snapshot.TotalCompleted <= observedCompletedOperations)
            {
                return;
            }

            observedCompletedOperations = snapshot.TotalCompleted;
            updateTask = RecordAfterAsync(updateTask, profileId, duration);
        }
    }

    private async Task RecordAfterAsync(
        Task precedingUpdate,
        string expectedProfileId,
        TimeSpan duration)
    {
        try
        {
            await precedingUpdate.ConfigureAwait(false);
            await profileSession
                .RecordCommandLatencyForProfileAsync(expectedProfileId, duration)
                .ConfigureAwait(false);
            LastError = string.Empty;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            LastError = exception.Message;
        }
    }
}
