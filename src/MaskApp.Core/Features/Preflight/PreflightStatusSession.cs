namespace MaskApp.Core.Features.Preflight;

public sealed record PreflightStatusSnapshot(
    FestivalPreflightStatus Status,
    string StatusText,
    string Summary,
    DateTimeOffset EvaluatedAt)
{
    public static PreflightStatusSnapshot NotRun { get; } = new(
        FestivalPreflightStatus.NotReady,
        "NOT READY",
        "Preflight has not run in this app session.",
        default);
}

public sealed class PreflightStatusSession
{
    private readonly object stateLock = new();
    private PreflightStatusSnapshot snapshot = PreflightStatusSnapshot.NotRun;

    public event EventHandler<PreflightStatusSnapshot>? SnapshotChanged;

    public PreflightStatusSnapshot Snapshot
    {
        get
        {
            lock (stateLock)
            {
                return snapshot;
            }
        }
    }

    public void Update(PreflightStatusSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);
        lock (stateLock)
        {
            snapshot = value;
        }

        SnapshotChanged?.Invoke(this, value);
    }
}
