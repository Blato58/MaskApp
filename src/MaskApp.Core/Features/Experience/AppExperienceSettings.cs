namespace MaskApp.Core.Features.Experience;

public enum AppAppearance
{
    System,
    Dark,
    Light
}

public enum AppLanguage
{
    System,
    English,
    Czech
}

public sealed record AppExperienceSettings
{
    public static AppExperienceSettings Defaults { get; } = new();

    public bool OnboardingCompleted { get; init; }

    public int OnboardingStep { get; init; }

    public AppAppearance Appearance { get; init; } = AppAppearance.System;

    public AppLanguage Language { get; init; } = AppLanguage.System;

    public bool? ReduceMotionOverride { get; init; }

    public bool HapticsEnabled { get; init; } = true;

    public IReadOnlyDictionary<string, bool> DeckHoldPreferences { get; init; } =
        new Dictionary<string, bool>(StringComparer.Ordinal);

    public bool RequiresHold(string deckId) =>
        !string.IsNullOrWhiteSpace(deckId) &&
        DeckHoldPreferences.TryGetValue(deckId.Trim(), out var requiresHold) &&
        requiresHold;

    public AppExperienceSettings WithDeckHold(string deckId, bool requiresHold)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deckId);

        var preferences = DeckHoldPreferences
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        if (requiresHold)
        {
            preferences[deckId.Trim()] = true;
        }
        else
        {
            preferences.Remove(deckId.Trim());
        }

        return this with { DeckHoldPreferences = preferences };
    }

    public AppExperienceSettings Normalize()
    {
        var preferences = DeckHoldPreferences
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && pair.Value)
            .ToDictionary(pair => pair.Key.Trim(), _ => true, StringComparer.Ordinal);

        return this with
        {
            OnboardingStep = Math.Clamp(OnboardingStep, 0, 2),
            DeckHoldPreferences = preferences
        };
    }
}
