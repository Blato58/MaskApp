using MaskApp.Core.Features.Experience;

namespace MaskApp.Core.Tests.Features.Experience;

public sealed class AppExperienceSettingsTests
{
    [Fact]
    public async Task DeckHoldPreference_IsLocalAndKeyedByStableDeckId()
    {
        var store = new InMemoryAppExperienceSettingsStore();
        var settings = (await store.LoadAsync()).WithDeckHold("deck-live", true);

        await store.SaveAsync(settings);
        var reloaded = await store.LoadAsync();

        Assert.True(reloaded.RequiresHold("deck-live"));
        Assert.False(reloaded.RequiresHold("deck-other"));
        Assert.DoesNotContain("deck-other", reloaded.DeckHoldPreferences.Keys);
    }

    [Fact]
    public void Normalize_RemovesFalseAndBlankDeckPreferences()
    {
        var settings = new AppExperienceSettings
        {
            DeckHoldPreferences = new Dictionary<string, bool>
            {
                [" deck-live "] = true,
                ["deck-off"] = false,
                [" "] = true
            }
        }.Normalize();

        Assert.Single(settings.DeckHoldPreferences);
        Assert.True(settings.RequiresHold("deck-live"));
    }
}
