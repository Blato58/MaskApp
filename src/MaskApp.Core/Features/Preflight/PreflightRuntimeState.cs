namespace MaskApp.Core.Features.Preflight;

[Flags]
public enum PreflightRuntimeRequirement
{
    None = 0,
    Bluetooth = 1,
    Microphone = 2
}

public enum PreflightRuntimeAccessStatus
{
    Unknown,
    Granted,
    Denied,
    Unavailable
}

public sealed record PreflightRuntimeSnapshot
{
    public PreflightRuntimeAccessStatus BluetoothAccess { get; init; }

    public string BluetoothDetail { get; init; } = "Bluetooth permission has not been checked.";

    public PreflightRuntimeAccessStatus MicrophoneAccess { get; init; }

    public string MicrophoneDetail { get; init; } = "Microphone permission has not been checked.";

    public static PreflightRuntimeSnapshot GrantedForTests { get; } = new()
    {
        BluetoothAccess = PreflightRuntimeAccessStatus.Granted,
        BluetoothDetail = "Bluetooth permission is granted.",
        MicrophoneAccess = PreflightRuntimeAccessStatus.Granted,
        MicrophoneDetail = "Microphone permission is granted."
    };
}

public interface IPreflightRuntimeStateProvider
{
    Task<PreflightRuntimeSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
