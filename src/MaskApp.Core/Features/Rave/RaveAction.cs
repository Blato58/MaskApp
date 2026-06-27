using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.QuickActions;

namespace MaskApp.Core.Features.Rave;

public sealed record RaveAction(QuickActionId Id, string Label, string Caption, string Group, AsyncRelayCommand SendCommand);
