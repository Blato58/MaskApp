using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.Stage;

namespace MaskApp.Core.Features.WatchRemote;

public sealed class WatchRemoteCoordinator : IWatchRemoteActionDispatcher, IWatchRemoteStateProvider
{
    private const int MaximumFavorites = 12;
    private readonly IStageShowSource showSource;
    private readonly ISceneCatalogSource catalogSource;
    private readonly ISceneItemDispatcher itemDispatcher;
    private readonly IStageReadinessProvider readinessProvider;
    private readonly IBleDeviceConnection connection;
    private readonly WatchRemoteExecutionSession executionSession;
    private readonly SceneExecutionEngine? sceneEngine;
    private readonly SemaphoreSlim gate = new(1, 1);
    private long stateRevision;

    public WatchRemoteCoordinator(
        IStageShowSource showSource,
        ISceneCatalogSource catalogSource,
        ISceneItemDispatcher itemDispatcher,
        IStageReadinessProvider readinessProvider,
        IBleDeviceConnection connection,
        WatchRemoteExecutionSession executionSession,
        SceneExecutionEngine? sceneEngine = null)
    {
        this.showSource = showSource;
        this.catalogSource = catalogSource;
        this.itemDispatcher = itemDispatcher;
        this.readinessProvider = readinessProvider;
        this.connection = connection;
        this.executionSession = executionSession;
        this.sceneEngine = sceneEngine;
    }

    public async Task<WatchRemoteDispatchResult> DispatchAsync(
        WatchRemoteAction action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (action.Kind == WatchRemoteActionKind.Stop)
        {
            return ToDispatchResult(sceneEngine is null
                ? await itemDispatcher.StopAsync(cancellationToken).ConfigureAwait(false)
                : await sceneEngine.StopAsync(cancellationToken).ConfigureAwait(false));
        }

        if (action.Kind == WatchRemoteActionKind.Blackout)
        {
            return ToDispatchResult(sceneEngine is null
                ? await itemDispatcher.BlackoutAsync(cancellationToken).ConfigureAwait(false)
                : await sceneEngine.BlackoutAsync(cancellationToken).ConfigureAwait(false));
        }

        if (!executionSession.IsForeground)
        {
            return WatchRemoteDispatchResult.Failure(
                "Bring MaskApp to the foreground before sending non-emergency Watch actions.");
        }

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!executionSession.IsForeground)
            {
                return WatchRemoteDispatchResult.Failure(
                    "Bring MaskApp to the foreground before sending non-emergency Watch actions.");
            }

            return action.Kind switch
            {
                WatchRemoteActionKind.PreviousCue => await MoveAsync(-1, cancellationToken).ConfigureAwait(false),
                WatchRemoteActionKind.NextCue => await MoveAsync(1, cancellationToken).ConfigureAwait(false),
                WatchRemoteActionKind.TriggerCurrentCue => await TriggerCurrentCueAsync(cancellationToken)
                    .ConfigureAwait(false),
                WatchRemoteActionKind.TriggerFavorite => await TriggerFavoriteAsync(
                    action.FavoriteId,
                    cancellationToken).ConfigureAwait(false),
                WatchRemoteActionKind.SetBrightness => await SetBrightnessAsync(
                    action.Brightness,
                    cancellationToken).ConfigureAwait(false),
                _ => WatchRemoteDispatchResult.Failure("Unsupported Watch action.")
            };
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<WatchRemoteState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var show = await showSource.InitializeAsync(cancellationToken).ConfigureAwait(false);
            var readiness = await readinessProvider.EvaluateAsync(cancellationToken).ConfigureAwait(false);
            var favorites = await LoadFavoritesAsync(cancellationToken).ConfigureAwait(false);
            var isCue = string.Equals(show.PositionLabel, "Cue", StringComparison.OrdinalIgnoreCase);
            var currentCue = isCue ? show.Tiles.FirstOrDefault() : null;
            return new WatchRemoteState
            {
                Revision = Interlocked.Increment(ref stateRevision),
                GeneratedAt = DateTimeOffset.UtcNow,
                PositionKind = isCue ? "Setlist" : "Page",
                PositionTitle = Bound(show.PageTitle, 120, "No active position"),
                PositionText = Bound(show.PagePositionText, 80, "No active position"),
                CurrentCueId = Bound(currentCue?.TileId, 128),
                CurrentCueLabel = Bound(currentCue?.Label, 120),
                NextCueLabel = Bound(show.NextCueLabel, 120),
                MaskConnected = connection.State == BleConnectionState.Connected,
                MaskConnectionText = GetConnectionText(connection.State),
                ReadinessStatus = Bound(readiness.StatusText, 40, "NOT READY"),
                ReadinessSummary = Bound(readiness.Summary, 300),
                ForegroundExecutionRequired = true,
                PhoneForeground = executionSession.IsForeground,
                Favorites = favorites
            };
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<WatchRemoteDispatchResult> MoveAsync(
        int delta,
        CancellationToken cancellationToken)
    {
        var show = await showSource.InitializeAsync(cancellationToken).ConfigureAwait(false);
        if (show.PageCount == 0)
        {
            return WatchRemoteDispatchResult.Failure("No prepared Page or setlist position is available.");
        }

        var target = show.PageIndex + delta;
        if (target < 0 || target >= show.PageCount)
        {
            return WatchRemoteDispatchResult.Failure(
                delta < 0 ? "Already at the first position." : "Already at the last position.");
        }

        var moved = await showSource.SelectPageAsync(target, cancellationToken).ConfigureAwait(false);
        return WatchRemoteDispatchResult.Success($"Selected {moved.PagePositionText}: {moved.PageTitle}.");
    }

    private async Task<WatchRemoteDispatchResult> TriggerCurrentCueAsync(
        CancellationToken cancellationToken)
    {
        var show = await showSource.InitializeAsync(cancellationToken).ConfigureAwait(false);
        if (!string.Equals(show.PositionLabel, "Cue", StringComparison.OrdinalIgnoreCase))
        {
            return WatchRemoteDispatchResult.Failure(
                "Trigger Current Cue requires an active setlist selected on the phone.");
        }

        var cue = show.Tiles.FirstOrDefault();
        if (cue is null)
        {
            return WatchRemoteDispatchResult.Failure("The current setlist position has no cue to trigger.");
        }

        var result = await showSource.TriggerAsync(cue.TileId, cancellationToken).ConfigureAwait(false);
        return new WatchRemoteDispatchResult(result.Succeeded, result.Message);
    }

    private async Task<WatchRemoteDispatchResult> TriggerFavoriteAsync(
        string favoriteId,
        CancellationToken cancellationToken)
    {
        var item = (await catalogSource.LoadAsync(cancellationToken).ConfigureAwait(false))
            .FirstOrDefault(candidate =>
                candidate.IsFavorite
                && candidate.CanSend
                && IsWatchTriggerable(candidate)
                && string.Equals(candidate.Id, favoriteId, StringComparison.Ordinal));
        if (item is null)
        {
            return WatchRemoteDispatchResult.Failure(
                "That favorite is unavailable or no longer marked as a favorite on the phone.");
        }

        var result = await itemDispatcher.TriggerAsync(item, cancellationToken).ConfigureAwait(false);
        return new WatchRemoteDispatchResult(result.Succeeded, result.Message);
    }

    private async Task<WatchRemoteDispatchResult> SetBrightnessAsync(
        int? brightness,
        CancellationToken cancellationToken)
    {
        if (brightness is not int value || value is < 1 or > 100)
        {
            return WatchRemoteDispatchResult.Failure("Brightness must be between 1 and 100.");
        }

        return ToDispatchResult(
            await itemDispatcher.SetBrightnessAsync(value, cancellationToken)
                .ConfigureAwait(false));
    }

    private async Task<IReadOnlyList<WatchRemoteFavorite>> LoadFavoritesAsync(
        CancellationToken cancellationToken)
    {
        return (await catalogSource.LoadAsync(cancellationToken).ConfigureAwait(false))
            .Where(item => item.IsFavorite && item.CanSend && IsWatchTriggerable(item))
            .OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Id, StringComparer.Ordinal)
            .Take(MaximumFavorites)
            .Select(item => new WatchRemoteFavorite(
                Bound(item.Id, 128),
                Bound(item.Title, 120, "Favorite"),
                Bound(item.TypeLabel, 40),
                NormalizeColor(item.ColorHex)))
            .ToArray();
    }

    private static bool IsWatchTriggerable(GalleryItem item) => item.Type != GalleryItemType.Scene;

    private static WatchRemoteDispatchResult ToDispatchResult(MaskControl.MaskCommandResult result) =>
        new(result.Succeeded, result.Message);

    private static string GetConnectionText(BleConnectionState state) => state switch
    {
        BleConnectionState.Connected => "Connected",
        BleConnectionState.Connecting => "Connecting",
        BleConnectionState.Scanning => "Scanning",
        BleConnectionState.Failed => "Connection failed",
        BleConnectionState.Unavailable => "Bluetooth unavailable",
        _ => "Disconnected"
    };

    private static string NormalizeColor(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 16 ? value : "#A78BFA";

    private static string Bound(string? value, int maximumLength, string fallback = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return normalized.Length <= maximumLength ? normalized : normalized[..maximumLength];
    }
}
