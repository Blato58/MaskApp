namespace MaskApp.Core.Features.QuickActions;

public sealed class InMemoryQuickActionTextSettingsStore : IQuickActionTextSettingsStore
{
    private QuickActionTextSettings settings;

    public InMemoryQuickActionTextSettingsStore(QuickActionTextSettings? settings = null)
    {
        this.settings = (settings ?? QuickActionTextSettings.RaveDefaults).Normalize();
    }

    public Task<QuickActionTextSettings> LoadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(settings);

    public Task SaveAsync(QuickActionTextSettings settings, CancellationToken cancellationToken = default)
    {
        this.settings = settings.Normalize();
        return Task.CompletedTask;
    }
}
