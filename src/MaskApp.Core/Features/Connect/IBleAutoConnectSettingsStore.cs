namespace MaskApp.Core.Features.Connect;

public interface IBleAutoConnectSettingsStore
{
    Task<BleAutoConnectSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(BleAutoConnectSettings settings, CancellationToken cancellationToken = default);
}
