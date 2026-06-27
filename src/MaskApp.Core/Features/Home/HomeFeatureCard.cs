namespace MaskApp.Core.Features.Home;

public sealed record HomeFeatureCard(
    string Name,
    string Status,
    string Description,
    bool IsAvailable,
    string ActionText);
