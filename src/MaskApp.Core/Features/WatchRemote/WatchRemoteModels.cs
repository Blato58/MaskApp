using System.Text.Json.Serialization;

namespace MaskApp.Core.Features.WatchRemote;

public enum WatchRemoteActionKind
{
    Unknown,
    PreviousCue,
    NextCue,
    TriggerCurrentCue,
    TriggerFavorite,
    SetBrightness,
    Stop,
    Blackout
}

public sealed record WatchRemoteAction
{
    [JsonRequired]
    public WatchRemoteActionKind Kind { get; init; }

    public string FavoriteId { get; init; } = string.Empty;

    public int? Brightness { get; init; }
}

public sealed record WatchRemoteEnvelope
{
    public const int CurrentSchemaVersion = 1;

    [JsonRequired]
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    [JsonRequired]
    public Guid MessageId { get; init; }

    [JsonRequired]
    public string SenderInstanceId { get; init; } = string.Empty;

    [JsonRequired]
    public long Sequence { get; init; }

    [JsonRequired]
    public DateTimeOffset SentAt { get; init; }

    [JsonRequired]
    public WatchRemoteAction? Action { get; init; }
}

public enum WatchRemoteProcessStatus
{
    Accepted,
    Rejected,
    Duplicate,
    Stale,
    Failed
}

public enum WatchRemoteHaptic
{
    None,
    Success,
    Warning,
    Failure
}

public sealed record WatchRemoteFavorite(
    string Id,
    string Label,
    string Kind,
    string ColorHex);

public sealed record WatchRemoteState
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public long Revision { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public int StaleAfterSeconds { get; init; } = 15;

    public string PositionKind { get; init; } = "Page";

    public string PositionTitle { get; init; } = "Pages";

    public string PositionText { get; init; } = "No active position";

    public string CurrentCueId { get; init; } = string.Empty;

    public string CurrentCueLabel { get; init; } = string.Empty;

    public string NextCueLabel { get; init; } = string.Empty;

    public bool MaskConnected { get; init; }

    public string MaskConnectionText { get; init; } = "Disconnected";

    public string ReadinessStatus { get; init; } = "NOT READY";

    public string ReadinessSummary { get; init; } = string.Empty;

    public bool WatchReachable { get; init; }

    public string CompanionStatus { get; init; } = "Watch companion state unavailable.";

    public bool ForegroundExecutionRequired { get; init; } = true;

    public bool PhoneForeground { get; init; }

    public IReadOnlyList<WatchRemoteFavorite> Favorites { get; init; } = [];

    public bool IsStale(DateTimeOffset now) =>
        GeneratedAt == default || now - GeneratedAt > TimeSpan.FromSeconds(StaleAfterSeconds);
}

public sealed record WatchRemoteProcessResult
{
    public int SchemaVersion { get; init; } = WatchRemoteEnvelope.CurrentSchemaVersion;

    public Guid MessageId { get; init; }

    public long Sequence { get; init; }

    public WatchRemoteProcessStatus Status { get; init; }

    public bool Succeeded { get; init; }

    public string Message { get; init; } = string.Empty;

    public WatchRemoteHaptic Haptic { get; init; }

    public WatchRemoteState State { get; init; } = new();
}

public sealed record WatchRemoteDispatchResult(bool Succeeded, string Message)
{
    public static WatchRemoteDispatchResult Success(string message) => new(true, message);

    public static WatchRemoteDispatchResult Failure(string message) => new(false, message);
}

public sealed class WatchRemoteExecutionSession
{
    private int isForeground;

    public bool IsForeground => Volatile.Read(ref isForeground) != 0;

    public void SetForeground(bool value) =>
        Interlocked.Exchange(ref isForeground, value ? 1 : 0);
}
