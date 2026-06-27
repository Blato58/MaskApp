using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.QuickActions;

namespace MaskApp.Core.Features.Home;

public sealed record HomeQuickActionCard(
    QuickActionId Id,
    string Label,
    string Description,
    string Status,
    AsyncRelayCommand TriggerCommand);
