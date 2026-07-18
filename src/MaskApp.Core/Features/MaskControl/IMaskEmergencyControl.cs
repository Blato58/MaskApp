namespace MaskApp.Core.Features.MaskControl;

public interface IMaskEmergencyControl
{
    Task<MaskCommandResult> StopAsync(CancellationToken cancellationToken = default);

    Task<MaskCommandResult> BlackoutAsync(CancellationToken cancellationToken = default);
}
