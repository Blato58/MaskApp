using MaskApp.Core.Features.Lifecycle;

namespace MaskApp.Core.Tests.Features.Lifecycle;

public sealed class AppLifecycleCoordinatorTests
{
    [Fact]
    public async Task Created_ContinuesWatchStartupWhenRecoveryFails()
    {
        var operations = new RecordingOperations { FailOperation = "recover" };
        var coordinator = new AppLifecycleCoordinator(operations, () => DateTimeOffset.UnixEpoch);

        await coordinator.OnCreatedAsync();

        Assert.Equal(["recover", "watch-start"], operations.Calls);
        var error = Assert.Single(coordinator.Snapshot.RecentErrors);
        Assert.Equal("Recover interrupted import", error.Operation);
        Assert.Equal(AppLifecyclePhase.Created, coordinator.Snapshot.Phase);
    }

    [Fact]
    public async Task Activated_MarksForegroundBeforeStartingForegroundServices()
    {
        var operations = new RecordingOperations();
        var coordinator = new AppLifecycleCoordinator(operations);

        await coordinator.OnActivatedAsync();

        Assert.Equal(["watch-foreground:True", "auto-start", "watch-start"], operations.Calls);
        Assert.Equal(AppLifecyclePhase.Foreground, coordinator.Snapshot.Phase);
    }

    [Fact]
    public async Task Stopped_CancelsSynchronousWorkBeforeOrderedShutdown_AndIsolatesFailures()
    {
        var operations = new RecordingOperations { FailOperation = "auto-stop" };
        var coordinator = new AppLifecycleCoordinator(operations);

        await coordinator.OnStoppedAsync();

        Assert.Equal(
            [
                "watch-foreground:False",
                "scene-cancel",
                "audio-diagnostic-cancel",
                "auto-stop",
                "audio-stop",
                "animation-handoff",
                "watch-publish"
            ],
            operations.Calls);
        var error = Assert.Single(coordinator.Snapshot.RecentErrors);
        Assert.Equal("Stop foreground auto-connect", error.Operation);
        Assert.Equal(AppLifecyclePhase.Background, coordinator.Snapshot.Phase);
    }

    [Fact]
    public async Task Resumed_OnlyResumesTheRetainedAnimationSession()
    {
        var operations = new RecordingOperations();
        var coordinator = new AppLifecycleCoordinator(operations);

        await coordinator.OnResumedAsync();

        Assert.Equal(["animation-resume"], operations.Calls);
        Assert.Equal(AppLifecyclePhase.Foreground, coordinator.Snapshot.Phase);
        Assert.DoesNotContain(operations.Calls, call => call.Contains("watch-start", StringComparison.Ordinal));
        Assert.DoesNotContain(operations.Calls, call => call.Contains("auto-start", StringComparison.Ordinal));
    }

    [Fact]
    public async Task StopThenActivate_SerializesShutdownBeforeStartingNewForegroundServices()
    {
        var operations = new BlockingStopOperations();
        var coordinator = new AppLifecycleCoordinator(operations);

        var stopping = coordinator.OnStoppedAsync();
        await operations.StopStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var activating = coordinator.OnActivatedAsync();

        Assert.DoesNotContain("auto-start", operations.Calls);
        operations.AllowStop.TrySetResult();
        await Task.WhenAll(stopping, activating);

        Assert.Equal(
            [
                "watch-foreground:False",
                "scene-cancel",
                "audio-diagnostic-cancel",
                "auto-stop",
                "audio-stop",
                "animation-handoff",
                "watch-publish",
                "watch-foreground:True",
                "auto-start",
                "watch-start"
            ],
            operations.Calls);
        Assert.Equal(AppLifecyclePhase.Foreground, coordinator.Snapshot.Phase);
    }

    [Fact]
    public async Task ExpiredBackgroundAllowance_AttemptsSafetyStopsAndSkipsOptionalWork()
    {
        var operations = new CancellationAwareOperations();
        var coordinator = new AppLifecycleCoordinator(operations);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await coordinator.OnStoppedAsync(cancellation.Token);

        Assert.Equal(
            [
                "watch-foreground:False",
                "scene-cancel",
                "audio-diagnostic-cancel",
                "auto-stop",
                "audio-stop"
            ],
            operations.Calls);
        Assert.DoesNotContain("animation-handoff", operations.Calls);
        Assert.DoesNotContain("watch-publish", operations.Calls);
        Assert.Contains(
            coordinator.Snapshot.RecentErrors,
            error => error.Operation == "Complete optional background work");
    }

    [Fact]
    public async Task SnapshotChanged_PublishesPhaseAndFailureUpdates()
    {
        var operations = new RecordingOperations { FailOperation = "auto-start" };
        var coordinator = new AppLifecycleCoordinator(operations);
        var snapshots = new List<AppLifecycleSnapshot>();
        coordinator.SnapshotChanged += (_, args) => snapshots.Add(args.Snapshot);

        await coordinator.OnActivatedAsync();

        Assert.Contains(snapshots, item => item.Phase == AppLifecyclePhase.Foreground);
        Assert.Contains(
            snapshots,
            item => item.RecentErrors.Any(error => error.Operation == "Start foreground auto-connect"));
    }

    [Fact]
    public async Task QueuedActivation_IsSkippedWhenAStopWasRequestedLater()
    {
        var operations = new BlockingRecoveryOperations();
        var coordinator = new AppLifecycleCoordinator(operations);

        var creating = coordinator.OnCreatedAsync();
        await operations.RecoveryStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var activating = coordinator.OnActivatedAsync();
        var stopping = coordinator.OnStoppedAsync();
        operations.AllowRecovery.TrySetResult();

        await Task.WhenAll(creating, activating, stopping);

        Assert.DoesNotContain("auto-start", operations.Calls);
        Assert.Equal(1, operations.Calls.Count(call => call == "watch-start"));
        Assert.Contains("auto-stop", operations.Calls);
        Assert.Equal(AppLifecyclePhase.Background, coordinator.Snapshot.Phase);
    }

    private sealed class RecordingOperations : IAppLifecycleOperations
    {
        public List<string> Calls { get; } = [];

        public string? FailOperation { get; init; }

        public Task RecoverInterruptedImportAsync(CancellationToken cancellationToken) =>
            RecordAsync("recover");

        public Task StartWatchRemoteAsync(CancellationToken cancellationToken) =>
            RecordAsync("watch-start");

        public void SetWatchForeground(bool isForeground) =>
            Record($"watch-foreground:{isForeground}");

        public void CancelSceneExecution() => Record("scene-cancel");

        public void CancelAudioDiagnostic() => Record("audio-diagnostic-cancel");

        public Task StartForegroundAutoConnectAsync(CancellationToken cancellationToken) =>
            RecordAsync("auto-start");

        public Task StopForegroundAutoConnectAsync(CancellationToken cancellationToken) =>
            RecordAsync("auto-stop");

        public Task StopAudioVisualizerAsync(CancellationToken cancellationToken) =>
            RecordAsync("audio-stop");

        public Task HandOffAnimationForBackgroundAsync(CancellationToken cancellationToken) =>
            RecordAsync("animation-handoff");

        public Task ResumeAnimationFromBackgroundAsync(CancellationToken cancellationToken) =>
            RecordAsync("animation-resume");

        public Task PublishWatchRemoteStateAsync(CancellationToken cancellationToken) =>
            RecordAsync("watch-publish");

        private Task RecordAsync(string operation)
        {
            Record(operation);
            return Task.CompletedTask;
        }

        private void Record(string operation)
        {
            Calls.Add(operation);
            if (string.Equals(FailOperation, operation, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Injected {operation} failure.");
            }
        }
    }

    private sealed class BlockingStopOperations : IAppLifecycleOperations
    {
        private readonly object sync = new();

        public List<string> Calls { get; } = [];

        public TaskCompletionSource StopStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource AllowStop { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task RecoverInterruptedImportAsync(CancellationToken cancellationToken) =>
            RecordAsync("recover");

        public Task StartWatchRemoteAsync(CancellationToken cancellationToken) =>
            RecordAsync("watch-start");

        public void SetWatchForeground(bool isForeground) => Record($"watch-foreground:{isForeground}");

        public void CancelSceneExecution() => Record("scene-cancel");

        public void CancelAudioDiagnostic() => Record("audio-diagnostic-cancel");

        public Task StartForegroundAutoConnectAsync(CancellationToken cancellationToken) =>
            RecordAsync("auto-start");

        public async Task StopForegroundAutoConnectAsync(CancellationToken cancellationToken)
        {
            Record("auto-stop");
            StopStarted.TrySetResult();
            await AllowStop.Task.WaitAsync(cancellationToken);
        }

        public Task StopAudioVisualizerAsync(CancellationToken cancellationToken) =>
            RecordAsync("audio-stop");

        public Task HandOffAnimationForBackgroundAsync(CancellationToken cancellationToken) =>
            RecordAsync("animation-handoff");

        public Task ResumeAnimationFromBackgroundAsync(CancellationToken cancellationToken) =>
            RecordAsync("animation-resume");

        public Task PublishWatchRemoteStateAsync(CancellationToken cancellationToken) =>
            RecordAsync("watch-publish");

        private Task RecordAsync(string value)
        {
            Record(value);
            return Task.CompletedTask;
        }

        private void Record(string value)
        {
            lock (sync)
            {
                Calls.Add(value);
            }
        }
    }

    private sealed class CancellationAwareOperations : IAppLifecycleOperations
    {
        public List<string> Calls { get; } = [];

        public Task RecoverInterruptedImportAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StartWatchRemoteAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void SetWatchForeground(bool isForeground) => Calls.Add($"watch-foreground:{isForeground}");

        public void CancelSceneExecution() => Calls.Add("scene-cancel");

        public void CancelAudioDiagnostic() => Calls.Add("audio-diagnostic-cancel");

        public Task StartForegroundAutoConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopForegroundAutoConnectAsync(CancellationToken cancellationToken) =>
            RecordAndObserveCancellationAsync("auto-stop", cancellationToken);

        public Task StopAudioVisualizerAsync(CancellationToken cancellationToken) =>
            RecordAndObserveCancellationAsync("audio-stop", cancellationToken);

        public Task HandOffAnimationForBackgroundAsync(CancellationToken cancellationToken)
        {
            Calls.Add("animation-handoff");
            return Task.CompletedTask;
        }

        public Task ResumeAnimationFromBackgroundAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task PublishWatchRemoteStateAsync(CancellationToken cancellationToken)
        {
            Calls.Add("watch-publish");
            return Task.CompletedTask;
        }

        private Task RecordAndObserveCancellationAsync(
            string operation,
            CancellationToken cancellationToken)
        {
            Calls.Add(operation);
            return Task.FromCanceled(cancellationToken);
        }
    }

    private sealed class BlockingRecoveryOperations : IAppLifecycleOperations
    {
        private readonly object sync = new();

        public List<string> Calls { get; } = [];

        public TaskCompletionSource RecoveryStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource AllowRecovery { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task RecoverInterruptedImportAsync(CancellationToken cancellationToken)
        {
            Record("recover");
            RecoveryStarted.TrySetResult();
            await AllowRecovery.Task.WaitAsync(cancellationToken);
        }

        public Task StartWatchRemoteAsync(CancellationToken cancellationToken) => RecordAsync("watch-start");

        public void SetWatchForeground(bool isForeground) => Record($"watch-foreground:{isForeground}");

        public void CancelSceneExecution() => Record("scene-cancel");

        public void CancelAudioDiagnostic() => Record("audio-diagnostic-cancel");

        public Task StartForegroundAutoConnectAsync(CancellationToken cancellationToken) => RecordAsync("auto-start");

        public Task StopForegroundAutoConnectAsync(CancellationToken cancellationToken) => RecordAsync("auto-stop");

        public Task StopAudioVisualizerAsync(CancellationToken cancellationToken) => RecordAsync("audio-stop");

        public Task HandOffAnimationForBackgroundAsync(CancellationToken cancellationToken) =>
            RecordAsync("animation-handoff");

        public Task ResumeAnimationFromBackgroundAsync(CancellationToken cancellationToken) =>
            RecordAsync("animation-resume");

        public Task PublishWatchRemoteStateAsync(CancellationToken cancellationToken) =>
            RecordAsync("watch-publish");

        private Task RecordAsync(string value)
        {
            Record(value);
            return Task.CompletedTask;
        }

        private void Record(string value)
        {
            lock (sync)
            {
                Calls.Add(value);
            }
        }
    }
}
