namespace MaskApp.Core.Features.MaskControl;

public sealed record MaskBleSchedulerOptions
{
    public int MaxPendingOperations { get; init; } = 64;

    public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromSeconds(10);

    public TimeSpan TextUploadTimeout { get; init; } = TimeSpan.FromMinutes(2);

    public TimeSpan FaceUploadTimeout { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan AudioVisualizationTimeout { get; init; } = TimeSpan.FromSeconds(2);
}
