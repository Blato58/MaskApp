using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Home;

public sealed record HomeQuickActionCard(
    string Label,
    string Description,
    string Status,
    AsyncRelayCommand TriggerCommand);
