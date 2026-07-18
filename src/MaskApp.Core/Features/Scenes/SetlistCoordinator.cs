using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Scenes;

public sealed record SetlistSnapshot(
    string SetlistId,
    string SetlistName,
    int CueIndex,
    int CueCount,
    PerformanceSetlistCue? CurrentCue,
    PerformanceScene? CurrentScene,
    PerformanceSetlistCue? NextCue = null)
{
    public bool HasSetlist => !string.IsNullOrWhiteSpace(SetlistId);

    public string PositionText => !HasSetlist
        ? "Pages"
        : CueCount == 0
            ? $"{SetlistName} · no cues"
            : $"Cue {CueIndex + 1} of {CueCount}";
}

public sealed class SetlistCoordinator
{
    private readonly ISceneShowStore store;
    private readonly SceneExecutionEngine sceneEngine;
    private readonly SemaphoreSlim gate = new(1, 1);
    private SceneShowState state = new();
    private SetlistSnapshot snapshot = new("", "Pages", 0, 0, null, null);

    public SetlistCoordinator(ISceneShowStore store, SceneExecutionEngine sceneEngine)
    {
        this.store = store;
        this.sceneEngine = sceneEngine;
    }

    public SetlistSnapshot Current => snapshot;

    public async Task<SetlistSnapshot> InitializeAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            state = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            snapshot = CreateSnapshot(state.ActiveSetlistId);
            return snapshot;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<SetlistSnapshot> ActivateAsync(
        string setlistId,
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            state = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            if (!state.Setlists.Any(setlist => string.Equals(setlist.Id, setlistId, StringComparison.Ordinal)))
            {
                throw new ArgumentException("Setlist does not exist.", nameof(setlistId));
            }

            EnsureWritable(state);
            state = state with { ActiveSetlistId = setlistId };
            await store.SaveAsync(state, cancellationToken).ConfigureAwait(false);
            snapshot = CreateSnapshot(setlistId);
            return snapshot;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<SetlistSnapshot> UsePagesAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            state = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            EnsureWritable(state);
            state = state with { ActiveSetlistId = string.Empty };
            await store.SaveAsync(state, cancellationToken).ConfigureAwait(false);
            snapshot = CreateSnapshot(string.Empty);
            return snapshot;
        }
        finally
        {
            gate.Release();
        }
    }

    public Task<SetlistSnapshot> PreviousAsync(CancellationToken cancellationToken = default) =>
        MoveAsync(-1, cancellationToken);

    public Task<SetlistSnapshot> NextAsync(CancellationToken cancellationToken = default) =>
        MoveAsync(1, cancellationToken);

    public async Task<SetlistSnapshot> SelectAsync(
        int cueIndex,
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            state = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
            var setlist = state.Setlists.FirstOrDefault(item => item.Id == state.ActiveSetlistId);
            if (setlist is null || setlist.Cues.Count == 0)
            {
                snapshot = CreateSnapshot(state.ActiveSetlistId);
                return snapshot;
            }

            EnsureWritable(state);
            var normalizedIndex = Math.Clamp(cueIndex, 0, setlist.Cues.Count - 1);
            state = state with
            {
                Positions = state.Positions
                    .Where(position => position.SetlistId != setlist.Id)
                    .Append(new SetlistPosition(setlist.Id, normalizedIndex, DateTimeOffset.UtcNow))
                    .ToArray()
            };
            await store.SaveAsync(state, cancellationToken).ConfigureAwait(false);
            snapshot = CreateSnapshot(setlist.Id);
            return snapshot;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<SceneExecutionResult> TriggerCurrentAsync(CancellationToken cancellationToken = default)
    {
        var current = snapshot.HasSetlist ? snapshot : await InitializeAsync(cancellationToken).ConfigureAwait(false);
        if (current.CurrentScene is null)
        {
            return new SceneExecutionResult(SceneExecutionState.Failed, "The active setlist has no current Scene cue.", []);
        }

        return await sceneEngine.ExecuteAsync(current.CurrentScene, cancellationToken).ConfigureAwait(false);
    }

    public Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default) =>
        sceneEngine.StopAsync(cancellationToken);

    public Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default) =>
        sceneEngine.BlackoutAsync(cancellationToken);

    private async Task<SetlistSnapshot> MoveAsync(int delta, CancellationToken cancellationToken)
    {
        if (!snapshot.HasSetlist)
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        return await SelectAsync(snapshot.CueIndex + delta, cancellationToken).ConfigureAwait(false);
    }

    private SetlistSnapshot CreateSnapshot(string setlistId)
    {
        var setlist = state.Setlists.FirstOrDefault(item => item.Id == setlistId);
        if (setlist is null)
        {
            return new SetlistSnapshot("", "Pages", 0, 0, null, null);
        }

        var storedIndex = state.Positions.FirstOrDefault(position => position.SetlistId == setlist.Id)?.CueIndex ?? 0;
        var index = setlist.Cues.Count == 0 ? 0 : Math.Clamp(storedIndex, 0, setlist.Cues.Count - 1);
        var cue = setlist.Cues.Count == 0 ? null : setlist.Cues[index];
        var nextCue = index + 1 < setlist.Cues.Count ? setlist.Cues[index + 1] : null;
        var scene = cue is null ? null : state.Scenes.FirstOrDefault(item => item.Id == cue.SceneId);
        return new SetlistSnapshot(setlist.Id, setlist.DisplayName, index, setlist.Cues.Count, cue, scene, nextCue);
    }

    private static void EnsureWritable(SceneShowState value)
    {
        if (value.UsedFallback)
        {
            throw new InvalidOperationException("Unreadable Scene/setlist data cannot be overwritten.");
        }
    }
}
