namespace MaskApp.Core.Features.Lifecycle;

public enum AppLifecyclePhase
{
    Created,
    Foreground,
    Background
}

public sealed record AppLifecycleError(
    DateTimeOffset OccurredAtUtc,
    string Operation,
    string ErrorType,
    string Message);

public sealed record AppLifecycleSnapshot
{
    public AppLifecyclePhase Phase { get; init; } = AppLifecyclePhase.Created;

    public DateTimeOffset LastTransitionAtUtc { get; init; }

    public IReadOnlyList<AppLifecycleError> RecentErrors { get; init; } = [];
}

public sealed class AppLifecycleSnapshotChangedEventArgs(AppLifecycleSnapshot snapshot) : EventArgs
{
    public AppLifecycleSnapshot Snapshot { get; } = snapshot;
}

public interface IAppLifecycleOperations
{
    Task RecoverInterruptedImportAsync(CancellationToken cancellationToken);

    Task StartWatchRemoteAsync(CancellationToken cancellationToken);

    void SetWatchForeground(bool isForeground);

    void CancelSceneExecution();

    void CancelAudioDiagnostic();

    Task StartForegroundAutoConnectAsync(CancellationToken cancellationToken);

    Task StopForegroundAutoConnectAsync(CancellationToken cancellationToken);

    Task StopAudioVisualizerAsync(CancellationToken cancellationToken);

    Task HandOffAnimationForBackgroundAsync(CancellationToken cancellationToken);

    Task ResumeAnimationFromBackgroundAsync(CancellationToken cancellationToken);

    Task PublishWatchRemoteStateAsync(CancellationToken cancellationToken);
}

public sealed class AppLifecycleCoordinator
{
    private const int MaximumRecentErrors = 12;
    private readonly object sync = new();
    private readonly SemaphoreSlim transitionGate = new(1, 1);
    private readonly IAppLifecycleOperations operations;
    private readonly Func<DateTimeOffset> getUtcNow;
    private readonly Queue<AppLifecycleError> recentErrors = new();
    private AppLifecycleSnapshot snapshot = new();
    private long requestedTransitionVersion;
    private long appliedTransitionVersion;

    public AppLifecycleCoordinator(
        IAppLifecycleOperations operations,
        Func<DateTimeOffset>? getUtcNow = null)
    {
        this.operations = operations;
        this.getUtcNow = getUtcNow ?? (() => DateTimeOffset.UtcNow);
        snapshot = snapshot with { LastTransitionAtUtc = this.getUtcNow() };
    }

    public AppLifecycleSnapshot Snapshot
    {
        get
        {
            lock (sync)
            {
                return snapshot;
            }
        }
    }

    public event EventHandler<AppLifecycleSnapshotChangedEventArgs>? SnapshotChanged;

    public async Task OnCreatedAsync(CancellationToken cancellationToken = default)
    {
        var transitionVersion = Interlocked.Increment(ref requestedTransitionVersion);
        await transitionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!TryApplyTransition(transitionVersion))
            {
                return;
            }

            SetPhase(AppLifecyclePhase.Created);
            await RunAsync(
                "Recover interrupted import",
                operations.RecoverInterruptedImportAsync,
                cancellationToken).ConfigureAwait(false);
            await RunAsync(
                "Start Watch remote",
                operations.StartWatchRemoteAsync,
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            transitionGate.Release();
        }
    }

    public async Task OnActivatedAsync(CancellationToken cancellationToken = default)
    {
        var transitionVersion = Interlocked.Increment(ref requestedTransitionVersion);
        await transitionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!TryApplyTransition(transitionVersion))
            {
                return;
            }

            SetPhase(AppLifecyclePhase.Foreground);
            Run("Mark Watch foreground", () => operations.SetWatchForeground(true));
            await RunAsync(
                "Start foreground auto-connect",
                operations.StartForegroundAutoConnectAsync,
                cancellationToken).ConfigureAwait(false);
            await RunAsync(
                "Start Watch remote",
                operations.StartWatchRemoteAsync,
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            transitionGate.Release();
        }
    }

    public async Task OnStoppedAsync(CancellationToken cancellationToken = default)
    {
        var transitionVersion = Interlocked.Increment(ref requestedTransitionVersion);
        SetPhase(AppLifecyclePhase.Background);
        Run("Mark Watch background", () => operations.SetWatchForeground(false));
        Run("Cancel Scene", operations.CancelSceneExecution);
        Run("Cancel Audio diagnostic", operations.CancelAudioDiagnostic);

        await transitionGate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            if (!TryApplyTransition(transitionVersion))
            {
                return;
            }

            if (Snapshot.Phase != AppLifecyclePhase.Background)
            {
                SetPhase(AppLifecyclePhase.Background);
                Run("Mark Watch background", () => operations.SetWatchForeground(false));
            }

            await RunShutdownAsync(
                "Stop foreground auto-connect",
                operations.StopForegroundAutoConnectAsync,
                cancellationToken).ConfigureAwait(false);
            await RunShutdownAsync(
                "Stop Audio visualizer",
                operations.StopAudioVisualizerAsync,
                cancellationToken).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                RecordError(
                    "Complete optional background work",
                    new OperationCanceledException(
                        "Background lifecycle allowance ended before animation handoff and Watch publication."));
                return;
            }

            await RunShutdownAsync(
                "Hand off animation",
                operations.HandOffAnimationForBackgroundAsync,
                cancellationToken).ConfigureAwait(false);
            await RunShutdownAsync(
                "Publish Watch state",
                operations.PublishWatchRemoteStateAsync,
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            transitionGate.Release();
        }
    }

    public async Task OnResumedAsync(CancellationToken cancellationToken = default)
    {
        var transitionVersion = Interlocked.Increment(ref requestedTransitionVersion);
        await transitionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!TryApplyTransition(transitionVersion))
            {
                return;
            }

            SetPhase(AppLifecyclePhase.Foreground);
            await RunAsync(
                "Resume animation",
                operations.ResumeAnimationFromBackgroundAsync,
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            transitionGate.Release();
        }
    }

    private void SetPhase(AppLifecyclePhase phase)
    {
        AppLifecycleSnapshot changedSnapshot;
        lock (sync)
        {
            snapshot = snapshot with
            {
                Phase = phase,
                LastTransitionAtUtc = getUtcNow()
            };
            changedSnapshot = snapshot;
        }

        SnapshotChanged?.Invoke(this, new AppLifecycleSnapshotChangedEventArgs(changedSnapshot));
    }

    private bool TryApplyTransition(long transitionVersion)
    {
        if (transitionVersion < appliedTransitionVersion
            || transitionVersion != Volatile.Read(ref requestedTransitionVersion))
        {
            return false;
        }

        appliedTransitionVersion = transitionVersion;
        return true;
    }

    private void Run(string operation, Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            RecordError(operation, exception);
        }
    }

    private async Task RunAsync(
        string operation,
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        try
        {
            await action(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            RecordError(operation, exception);
        }
    }

    private async Task RunShutdownAsync(
        string operation,
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        try
        {
            await action(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            RecordError(operation, exception);
        }
    }

    private void RecordError(string operation, Exception exception)
    {
        AppLifecycleSnapshot changedSnapshot;
        lock (sync)
        {
            recentErrors.Enqueue(new AppLifecycleError(
                getUtcNow(),
                operation,
                exception.GetType().Name,
                exception.Message));
            while (recentErrors.Count > MaximumRecentErrors)
            {
                recentErrors.Dequeue();
            }

            snapshot = snapshot with { RecentErrors = recentErrors.ToArray() };
            changedSnapshot = snapshot;
        }

        SnapshotChanged?.Invoke(this, new AppLifecycleSnapshotChangedEventArgs(changedSnapshot));
    }
}
