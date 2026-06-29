namespace MaskApp.Core.Features.Connect;

public sealed class InMemoryBleAutoConnectSettingsStore : IBleAutoConnectSettingsStore
{
    private BleAutoConnectSettings settings;

    public InMemoryBleAutoConnectSettingsStore(BleAutoConnectSettings? settings = null)
    {
        this.settings = (settings ?? BleAutoConnectSettings.Defaults).Normalize();
    }

    public Task<BleAutoConnectSettings> LoadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(settings);

    public Task SaveAsync(BleAutoConnectSettings settings, CancellationToken cancellationToken = default)
    {
        this.settings = settings.Normalize();
        return Task.CompletedTask;
    }
}
