namespace MaskApp.Core.Features.QuickActions;

public interface IQuickActionTextSettingsStore
{
    Task<QuickActionTextSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(QuickActionTextSettings settings, CancellationToken cancellationToken = default);
}
