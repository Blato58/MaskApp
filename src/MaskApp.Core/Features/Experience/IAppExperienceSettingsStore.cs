namespace MaskApp.Core.Features.Experience;

public interface IAppExperienceSettingsStore
{
    Task<AppExperienceSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AppExperienceSettings settings, CancellationToken cancellationToken = default);
}
