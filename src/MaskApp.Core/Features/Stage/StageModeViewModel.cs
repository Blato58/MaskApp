using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Scenes;

namespace MaskApp.Core.Features.Stage;

public sealed class StageModeViewModel : INotifyPropertyChanged, IDisposable
{
    public static readonly TimeSpan UnlockHoldDuration = TimeSpan.FromSeconds(2);

    private readonly IStageShowSource showSource;
    private readonly IStageReadinessProvider readinessProvider;
    private readonly IBleDeviceConnection connection;
    private readonly DiySlotPlaybackCoordinator playbackCoordinator;
    private readonly PerformanceAnimationEngine animationEngine;
    private readonly IMaskEmergencyControl emergencyControl;
    private readonly IStageDeviceFeedback feedback;
    private readonly IStageDisplayControl displayControl;
    private readonly ISceneExecutionControl? sceneExecutionControl;
    private readonly SemaphoreSlim actionGate = new(1, 1);
    private SynchronizationContext? synchronizationContext;
    private StageShowSnapshot show = new("", "No Page", "#52E3FF", 0, 0, []);
    private StageReadinessSnapshot readiness = StageReadinessSnapshot.NotReady("Run Festival Preflight before live use.");
    private StageLayoutMode layoutMode = StageLayoutMode.Grid2x2;
    private BleConnectionState connectionState;
    private string connectionMessage = "Not connected.";
    private string statusText = "Stage is locked. No output is replayed automatically.";
    private string currentCueText = "Current: none";
    private string nextCueText = "Next: choose a tile";
    private bool requiresExplicitRecovery;
    private bool isBusy;
    private bool isActive;
    private bool isLocked = true;
    private StageTile? lastStableTile;
    private StageTile? holdRestoreTile;
    private string? activeHoldTileId;
    private bool disposed;

    public StageModeViewModel(
        IStageShowSource showSource,
        IStageReadinessProvider readinessProvider,
        IBleDeviceConnection connection,
        DiySlotPlaybackCoordinator playbackCoordinator,
        PerformanceAnimationEngine animationEngine,
        IMaskEmergencyControl emergencyControl,
        IStageDeviceFeedback feedback,
        IStageDisplayControl displayControl,
        ISceneExecutionControl? sceneExecutionControl = null)
    {
        this.showSource = showSource;
        this.readinessProvider = readinessProvider;
        this.connection = connection;
        this.playbackCoordinator = playbackCoordinator;
        this.animationEngine = animationEngine;
        this.emergencyControl = emergencyControl;
        this.feedback = feedback;
        this.displayControl = displayControl;
        this.sceneExecutionControl = sceneExecutionControl;
        connectionState = connection.State;

        PreviousPageCommand = new AsyncRelayCommand(
            cancellationToken => MovePageAsync(-1, cancellationToken),
            () => !IsBusy && Show.PageIndex > 0);
        NextPageCommand = new AsyncRelayCommand(
            cancellationToken => MovePageAsync(1, cancellationToken),
            () => !IsBusy && Show.PageIndex + 1 < Show.PageCount);
        CycleLayoutCommand = new AsyncRelayCommand(_ =>
        {
            LayoutMode = LayoutMode switch
            {
                StageLayoutMode.Giant => StageLayoutMode.Grid2x2,
                StageLayoutMode.Grid2x2 => StageLayoutMode.Dense,
                _ => StageLayoutMode.Giant
            };
            return Task.CompletedTask;
        });
        StopCommand = new AsyncRelayCommand(StopAsync);
        BlackoutCommand = new AsyncRelayCommand(BlackoutAsync);
        RecoverCommand = new AsyncRelayCommand(RecoverAsync, () => !IsBusy && CanRecover);
        RefreshReadinessCommand = new AsyncRelayCommand(RefreshReadinessAsync, () => !IsBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand PreviousPageCommand { get; }

    public AsyncRelayCommand NextPageCommand { get; }

    public AsyncRelayCommand CycleLayoutCommand { get; }

    public AsyncRelayCommand StopCommand { get; }

    public AsyncRelayCommand BlackoutCommand { get; }

    public AsyncRelayCommand RecoverCommand { get; }

    public AsyncRelayCommand RefreshReadinessCommand { get; }

    public StageShowSnapshot Show
    {
        get => show;
        private set
        {
            if (SetField(ref show, value))
            {
                OnPropertyChanged(nameof(Tiles));
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(PageColorHex));
                OnPropertyChanged(nameof(PagePositionText));
                RefreshCommandState();
            }
        }
    }

    public IReadOnlyList<StageTile> Tiles => Show.Tiles;

    public string PageTitle => Show.PageTitle;

    public string PageColorHex => Show.PageColorHex;

    public string PagePositionText => Show.PagePositionText;

    public StageReadinessSnapshot Readiness
    {
        get => readiness;
        private set
        {
            if (SetField(ref readiness, value))
            {
                OnPropertyChanged(nameof(ReadinessText));
                OnPropertyChanged(nameof(ReadinessSummary));
                OnPropertyChanged(nameof(ReadinessColorHex));
            }
        }
    }

    public string ReadinessText => Readiness.StatusText;

    public string ReadinessSummary => Readiness.Summary;

    public string ReadinessColorHex => Readiness.Status switch
    {
        FestivalPreflightStatus.ShowReady => "#30D158",
        FestivalPreflightStatus.Degraded => "#FFD60A",
        _ => "#FF453A"
    };

    public StageLayoutMode LayoutMode
    {
        get => layoutMode;
        private set
        {
            if (SetField(ref layoutMode, value))
            {
                OnPropertyChanged(nameof(LayoutButtonText));
                OnPropertyChanged(nameof(GridSpan));
                OnPropertyChanged(nameof(TileMinimumHeight));
            }
        }
    }

    public string LayoutButtonText => LayoutMode switch
    {
        StageLayoutMode.Giant => "Layout: Giant",
        StageLayoutMode.Grid2x2 => "Layout: 2×2",
        _ => "Layout: Dense"
    };

    public int GridSpan => LayoutMode switch
    {
        StageLayoutMode.Giant => 1,
        StageLayoutMode.Grid2x2 => 2,
        _ => 3
    };

    public double TileMinimumHeight => LayoutMode switch
    {
        StageLayoutMode.Giant => 230,
        StageLayoutMode.Grid2x2 => 150,
        _ => 105
    };

    public BleConnectionState ConnectionState
    {
        get => connectionState;
        private set
        {
            if (SetField(ref connectionState, value))
            {
                OnPropertyChanged(nameof(ConnectionStatusText));
                OnPropertyChanged(nameof(CanRecover));
                OnPropertyChanged(nameof(HasConnectionOverlay));
                RecoverCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ConnectionMessage
    {
        get => connectionMessage;
        private set => SetField(ref connectionMessage, value);
    }

    public string ConnectionStatusText => ConnectionState == BleConnectionState.Connected
        ? RequiresExplicitRecovery
            ? "RECONNECTED · RECOVERY REQUIRED"
            : "CONNECTED"
        : "CONNECTION LOST";

    public bool RequiresExplicitRecovery
    {
        get => requiresExplicitRecovery;
        private set
        {
            if (SetField(ref requiresExplicitRecovery, value))
            {
                OnPropertyChanged(nameof(ConnectionStatusText));
                OnPropertyChanged(nameof(CanRecover));
                OnPropertyChanged(nameof(HasConnectionOverlay));
                RecoverCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanRecover => RequiresExplicitRecovery && ConnectionState == BleConnectionState.Connected;

    public bool HasConnectionOverlay => IsLocked
        && (ConnectionState != BleConnectionState.Connected || RequiresExplicitRecovery);

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetField(ref isBusy, value))
            {
                RefreshCommandState();
            }
        }
    }

    public bool IsLocked
    {
        get => isLocked;
        private set
        {
            if (SetField(ref isLocked, value))
            {
                OnPropertyChanged(nameof(HasConnectionOverlay));
                OnPropertyChanged(nameof(LockStatusText));
            }
        }
    }

    public string LockStatusText => IsLocked ? "STAGE LOCKED" : "STAGE UNLOCKED";

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string CurrentCueText
    {
        get => currentCueText;
        private set => SetField(ref currentCueText, value);
    }

    public string NextCueText
    {
        get => nextCueText;
        private set => SetField(ref nextCueText, value);
    }

    public bool IsHoldActive => activeHoldTileId is not null;

    public async Task ActivateAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        synchronizationContext = SynchronizationContext.Current;
        if (!isActive)
        {
            connection.ConnectionStateChanged += OnConnectionStateChanged;
            showSource.StartObservingTransportState();
            isActive = true;
        }

        IsLocked = true;
        displayControl.SetKeepAwake(true);
        ConnectionState = connection.State;
        RequiresExplicitRecovery = connection.State != BleConnectionState.Connected;
        Show = await showSource.InitializeAsync(cancellationToken);
        await RefreshReadinessAsync(cancellationToken);
        StatusText = "Stage ready. Output will occur only when you press a tile.";
    }

    public async Task DeactivateAsync(CancellationToken cancellationToken = default)
    {
        displayControl.SetKeepAwake(false);
        if (isActive)
        {
            connection.ConnectionStateChanged -= OnConnectionStateChanged;
            showSource.StopObservingTransportState();
            isActive = false;
        }

        sceneExecutionControl?.RequestCancel();
        await playbackCoordinator.StopAnimationAsync(cancellationToken);
    }

    public async Task TriggerAsync(StageTile tile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tile);
        if (tile.IsHoldAction)
        {
            await BeginHoldAsync(tile, cancellationToken);
            return;
        }

        await TriggerCoreAsync(tile, rememberAsStable: true, cancellationToken);
    }

    public async Task BeginHoldAsync(StageTile tile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tile);
        if (!tile.IsHoldAction || activeHoldTileId is not null)
        {
            return;
        }

        holdRestoreTile = lastStableTile;
        activeHoldTileId = tile.TileId;
        OnPropertyChanged(nameof(IsHoldActive));
        var succeeded = await TriggerCoreAsync(tile, rememberAsStable: false, cancellationToken);
        if (!succeeded)
        {
            activeHoldTileId = null;
            holdRestoreTile = null;
            OnPropertyChanged(nameof(IsHoldActive));
        }
    }

    public async Task EndHoldAsync(StageTile tile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tile);
        if (!string.Equals(activeHoldTileId, tile.TileId, StringComparison.Ordinal))
        {
            return;
        }

        var restoreTile = holdRestoreTile;
        activeHoldTileId = null;
        holdRestoreTile = null;
        OnPropertyChanged(nameof(IsHoldActive));
        await playbackCoordinator.StopAnimationAsync(cancellationToken);
        if (restoreTile is not null && restoreTile.ItemType != GalleryItemType.CustomStaticFace)
        {
            var restoreResult = await showSource.TriggerAsync(restoreTile.TileId, cancellationToken);
            StatusText = restoreResult.Succeeded
                ? $"Released {tile.Label}; restored {restoreTile.Label}."
                : $"Released {tile.Label}; previous look could not be restored: {restoreResult.Message}";
            if (!restoreResult.Succeeded)
            {
                feedback.Failure();
                return;
            }
        }
        else
        {
            StatusText = restoreTile is null
                ? $"Released {tile.Label}; playback stopped with no previous look to restore."
                : $"Released {tile.Label}; restored {restoreTile.Label}.";
        }

        feedback.Success();
    }

    public bool TryUnlock(TimeSpan heldDuration)
    {
        if (heldDuration < UnlockHoldDuration)
        {
            feedback.Warning();
            StatusText = $"Keep holding for {UnlockHoldDuration.TotalSeconds:0} seconds to exit Stage.";
            return false;
        }

        IsLocked = false;
        displayControl.SetKeepAwake(false);
        feedback.Success();
        StatusText = "Stage unlocked. No mask output was replayed.";
        return true;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        displayControl.SetKeepAwake(false);
        if (isActive)
        {
            connection.ConnectionStateChanged -= OnConnectionStateChanged;
            showSource.StopObservingTransportState();
            isActive = false;
        }

        actionGate.Dispose();
    }

    private async Task<bool> TriggerCoreAsync(
        StageTile tile,
        bool rememberAsStable,
        CancellationToken cancellationToken)
    {
        if (HasConnectionOverlay)
        {
            StatusText = "Recover the connection explicitly before triggering another Stage action.";
            feedback.Failure();
            return false;
        }

        if (!tile.IsPrepared)
        {
            StatusText = $"{tile.Label} is not prepared for this mask. Hold to exit, prepare it in Preflight, then re-enter Stage.";
            feedback.Failure();
            return false;
        }

        if (Readiness.Status == FestivalPreflightStatus.NotReady)
        {
            StatusText = "Stage Preflight is NOT READY. Resolve its blocking issue before triggering show output.";
            feedback.Failure();
            return false;
        }

        if (!await actionGate.WaitAsync(0, cancellationToken))
        {
            StatusText = "A Stage action is already running.";
            feedback.Warning();
            return false;
        }

        var succeeded = false;
        try
        {
            IsBusy = true;
            GalleryActionResult result;
            try
            {
                result = await showSource.TriggerAsync(tile.TileId, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                result = GalleryActionResult.Failure($"Stage action failed: {exception.Message}");
            }
            succeeded = result.Succeeded;
            StatusText = result.Message;
            if (result.Succeeded)
            {
                if (rememberAsStable)
                {
                    lastStableTile = tile;
                }

                UpdateCueLabels(tile);
                feedback.Success();
            }
            else
            {
                feedback.Failure();
            }
        }
        finally
        {
            IsBusy = false;
            actionGate.Release();
        }

        return succeeded;
    }

    private async Task MovePageAsync(int delta, CancellationToken cancellationToken)
    {
        var target = Show.PageIndex + delta;
        if (target < 0 || target >= Show.PageCount)
        {
            feedback.Warning();
            return;
        }

        Show = await showSource.SelectPageAsync(target, cancellationToken);
        CurrentCueText = "Current: none on this Page";
        NextCueText = !string.IsNullOrWhiteSpace(Show.NextCueLabel)
            ? $"Next: {Show.NextCueLabel}"
            : Show.Tiles.Count == 0 ? "Next: no tiles" : $"Next: {Show.Tiles[0].Label}";
        feedback.Success();
    }

    private async Task StopAsync(CancellationToken cancellationToken)
    {
        sceneExecutionControl?.RequestCancel();
        await playbackCoordinator.StopAnimationAsync(cancellationToken);
        var result = await emergencyControl.StopAsync(cancellationToken);
        StatusText = result.Succeeded
            ? "STOP: queued/active visual work cancelled; the last stable look remains when available."
            : result.Message;
        if (result.Succeeded)
        {
            feedback.Success();
        }
        else
        {
            feedback.Failure();
        }
    }

    private async Task BlackoutAsync(CancellationToken cancellationToken)
    {
        sceneExecutionControl?.RequestCancel();
        var result = await animationEngine.BlackoutAsync(cancellationToken);
        StatusText = result.Succeeded
            ? "BLACKOUT sent. Brightness is forced to the protocol minimum and no previous look is restored."
            : result.Message;
        CurrentCueText = "Current: BLACKOUT";
        if (result.Succeeded)
        {
            feedback.Success();
        }
        else
        {
            feedback.Failure();
        }
    }

    private async Task RecoverAsync(CancellationToken cancellationToken)
    {
        if (connection.State != BleConnectionState.Connected)
        {
            StatusText = "The mask is still disconnected. Blackout remains available to retry when the link returns.";
            feedback.Failure();
            return;
        }

        Show = await showSource.InitializeAsync(cancellationToken);
        await RefreshReadinessAsync(cancellationToken);
        RequiresExplicitRecovery = false;
        StatusText = "Connection recovered explicitly. Nothing was replayed; choose the next cue yourself.";
        feedback.Success();
    }

    private async Task RefreshReadinessAsync(CancellationToken cancellationToken)
    {
        Readiness = await readinessProvider.EvaluateAsync(cancellationToken);
    }

    private void UpdateCueLabels(StageTile tile)
    {
        CurrentCueText = $"Current: {tile.Label}";
        var index = Show.Tiles.ToList().FindIndex(candidate =>
            string.Equals(candidate.TileId, tile.TileId, StringComparison.Ordinal));
        NextCueText = !string.IsNullOrWhiteSpace(Show.NextCueLabel)
            ? $"Next: {Show.NextCueLabel}"
            : index >= 0 && index + 1 < Show.Tiles.Count
            ? $"Next: {Show.Tiles[index + 1].Label}"
            : "Next: end of Page";
    }

    private void OnConnectionStateChanged(object? sender, BleConnectionStateChangedEventArgs eventArgs)
    {
        PostToContext(() =>
        {
            ConnectionState = eventArgs.State;
            ConnectionMessage = eventArgs.Message;
            if (eventArgs.State != BleConnectionState.Connected)
            {
                sceneExecutionControl?.RequestCancel();
                RequiresExplicitRecovery = true;
                StatusText = "Connection lost. The Stage session is preserved, output is stopped, and reconnect will not replay it.";
                feedback.Failure();
            }
            else if (RequiresExplicitRecovery)
            {
                StatusText = "Mask reconnected. Press Recover to refresh readiness; no output has been replayed.";
                feedback.Warning();
            }
        });
    }

    private void PostToContext(Action action)
    {
        var context = synchronizationContext;
        if (context is null || ReferenceEquals(context, SynchronizationContext.Current))
        {
            action();
            return;
        }

        context.Post(_ => action(), null);
    }

    private void RefreshCommandState()
    {
        PreviousPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
        StopCommand.RaiseCanExecuteChanged();
        BlackoutCommand.RaiseCanExecuteChanged();
        RecoverCommand.RaiseCanExecuteChanged();
        RefreshReadinessCommand.RaiseCanExecuteChanged();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
