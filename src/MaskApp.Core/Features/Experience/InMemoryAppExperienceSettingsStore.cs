namespace MaskApp.Core.Features.Experience;

public sealed class InMemoryAppExperienceSettingsStore(
    AppExperienceSettings? initialSettings = null) : IAppExperienceSettingsStore
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private AppExperienceSettings settings = (initialSettings ?? AppExperienceSettings.Defaults).Normalize();

    public async Task<AppExperienceSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return settings with
            {
                DeckHoldPreferences = new Dictionary<string, bool>(
                    settings.DeckHoldPreferences,
                    StringComparer.Ordinal)
            };
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(AppExperienceSettings value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            settings = value.Normalize();
        }
        finally
        {
            gate.Release();
        }
    }
}
