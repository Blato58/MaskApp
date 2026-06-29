using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.QuickActions;

public sealed class QuickActionDispatcher : IQuickActionDispatcher
{
    private readonly QuickActionCatalog catalog;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;
    private readonly IQuickActionTextSettingsStore settingsStore;

    public QuickActionDispatcher(
        QuickActionCatalog catalog,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport,
        IQuickActionTextSettingsStore? settingsStore = null)
    {
        this.catalog = catalog;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;
        this.settingsStore = settingsStore ?? new InMemoryQuickActionTextSettingsStore();
    }

    public async Task<QuickActionResult> TriggerAsync(
        QuickActionId actionId,
        QuickActionRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var action = catalog.Get(actionId);
        return action.Kind switch
        {
            QuickActionKind.Command => await SendBrightnessAsync(action, action.Brightness ?? 60, cancellationToken).ConfigureAwait(false),
            QuickActionKind.Brightness => await SendBrightnessAsync(action, request?.Brightness ?? action.Brightness ?? 60, cancellationToken).ConfigureAwait(false),
            QuickActionKind.BuiltInImage => await SendBuiltInCommandAsync(action, cancellationToken).ConfigureAwait(false),
            QuickActionKind.BuiltInAnimation => await SendBuiltInCommandAsync(action, cancellationToken).ConfigureAwait(false),
            QuickActionKind.Text => await SendTextAsync(action, cancellationToken).ConfigureAwait(false),
            QuickActionKind.Random => await SendRandomReactionAsync(action, cancellationToken).ConfigureAwait(false),
            _ => QuickActionResult.Failed(actionId, "Quick action is not supported.")
        };
    }

    private async Task<QuickActionResult> SendBrightnessAsync(
        QuickActionDefinition action,
        int brightness,
        CancellationToken cancellationToken)
    {
        if (commandTransport.TransportState != MaskCommandTransportState.Ready)
        {
            return QuickActionResult.Failed(action.Id, commandTransport.TransportStatusText, "command transport not ready");
        }

        var command = MaskCommandBuilder.Brightness(brightness);
        var result = await commandTransport.SendAsync(command, cancellationToken).ConfigureAwait(false);
        return result.Succeeded
            ? QuickActionResult.Sent(action.Id, result.Message)
            : QuickActionResult.Failed(action.Id, result.Message);
    }

    private async Task<QuickActionResult> SendBuiltInCommandAsync(
        QuickActionDefinition action,
        CancellationToken cancellationToken)
    {
        if (action.BuiltInId is null)
        {
            return QuickActionResult.Failed(action.Id, "Quick action has no built-in ID.");
        }

        if (commandTransport.TransportState != MaskCommandTransportState.Ready)
        {
            return QuickActionResult.Failed(action.Id, commandTransport.TransportStatusText, "command transport not ready");
        }

        var command = action.Kind == QuickActionKind.BuiltInAnimation
            ? MaskCommandBuilder.Animation(action.BuiltInId.Value, action.Label)
            : MaskCommandBuilder.Image(action.BuiltInId.Value, action.Label);
        var result = await commandTransport.SendAsync(command, cancellationToken).ConfigureAwait(false);
        return result.Succeeded
            ? QuickActionResult.Sent(action.Id, result.Message)
            : QuickActionResult.Failed(action.Id, result.Message);
    }

    private async Task<QuickActionResult> SendTextAsync(
        QuickActionDefinition action,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(action.Caption))
        {
            return QuickActionResult.Failed(action.Id, "Quick action has no caption.");
        }

        if (!textTransport.IsReady)
        {
            return QuickActionResult.Failed(action.Id, "Text not ready", "text transport not ready");
        }

        var settings = (await settingsStore.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        if (settings.SendMode == QuickCaptionSendMode.ReliableAcknowledgement && !textTransport.SupportsAcknowledgements)
        {
            return QuickActionResult.Failed(action.Id, "Text not ready", "ack unavailable");
        }

        TextSendPlan plan;
        try
        {
            plan = TextSendPackageFactory.Create(
                action.Caption,
                CreateQuickCaptionProfile(settings),
                textTransport.SupportsAcknowledgements);
        }
        catch (ArgumentException ex)
        {
            return QuickActionResult.Failed(action.Id, "Text not ready", ex.Message);
        }

        try
        {
            var result = await textTransport.UploadAsync(plan.Package, plan.Options, cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
            {
                var status = string.Equals(result.Message, "Uploaded.", StringComparison.Ordinal)
                    ? plan.Summary
                    : $"{plan.Summary} · {result.Message}";
                return QuickActionResult.Sent(action.Id, status) with { Status = status };
            }

            return QuickActionResult.Failed(action.Id, "Failed", result.Message);
        }
        catch (OperationCanceledException)
        {
            return QuickActionResult.Failed(action.Id, "Text send cancelled.", "cancelled");
        }
        catch (Exception ex)
        {
            return QuickActionResult.Failed(action.Id, $"Failed: {GetShortErrorMessage(ex)}");
        }
    }

    private Task<QuickActionResult> SendRandomReactionAsync(
        QuickActionDefinition action,
        CancellationToken cancellationToken)
    {
        var candidates = catalog.Actions
            .Where(candidate => candidate.Kind == QuickActionKind.Text && candidate.Category is QuickActionCategory.Meme or QuickActionCategory.Social)
            .ToArray();
        var selected = candidates[Random.Shared.Next(candidates.Length)];
        return SendTextAsync(selected, cancellationToken);
    }

    private static TextSendProfile CreateQuickCaptionProfile(QuickActionTextSettings settings)
    {
        var profile = settings.SendMode switch
        {
            QuickCaptionSendMode.LowStaticFlash => TextSendProfile.QuickFlashLowStatic,
            QuickCaptionSendMode.FastWriteOnly => TextSendProfile.QuickFlashFast,
            QuickCaptionSendMode.ReliableAcknowledgement => TextSendProfile.QuickFlashStable with
            {
                Name = "Reliable ACK",
                Reliability = TextSendReliability.ReliableAcknowledgement
            },
            _ => TextSendProfile.QuickFlashStable
        };

        var displayMode = settings.DisplayMode switch
        {
            QuickCaptionDisplayMode.ScrollRightToLeft => TextDisplayMode.ScrollRightToLeft,
            QuickCaptionDisplayMode.ScrollLeftToRight => TextDisplayMode.ScrollLeftToRight,
            _ => TextDisplayMode.Blink
        };
        var usesScroll = displayMode is TextDisplayMode.ScrollRightToLeft or TextDisplayMode.ScrollLeftToRight;

        return profile with
        {
            Name = usesScroll ? profile.Name.Replace("Flash", "Scroll", StringComparison.Ordinal) : profile.Name,
            LayoutMode = usesScroll ? TextLayoutMode.VariableWidth : TextLayoutMode.FixedWidthCentered,
            FixedWidthColumns = usesScroll ? null : QuickCaptionLayout.VisibleColumns,
            DisplayMode = displayMode,
            Speed = settings.Speed,
            TextColor = QuickCaptionForegroundPalette.GetColor(settings.ForegroundPreset),
            BackgroundEnabled = settings.BackgroundEnabled,
            BackgroundColor = settings.BackgroundEnabled ? GetBackgroundColor(settings.BackgroundPreset) : null,
            StyleCommandPolicy = settings.BackgroundEnabled ? TextStyleCommandPolicy.FailSoft : TextStyleCommandPolicy.Skip
        };
    }

    private static TextLedColor GetBackgroundColor(QuickCaptionBackgroundPreset preset) =>
        preset switch
        {
            QuickCaptionBackgroundPreset.RedAlert => new TextLedColor(0xEF, 0x44, 0x44),
            QuickCaptionBackgroundPreset.DeepBlue => new TextLedColor(0x1D, 0x4E, 0xD8),
            QuickCaptionBackgroundPreset.Black => new TextLedColor(0x00, 0x00, 0x00),
            _ => new TextLedColor(0xA8, 0x55, 0xF7)
        };

    private static string GetShortErrorMessage(Exception ex)
    {
        var message = string.IsNullOrWhiteSpace(ex.Message)
            ? ex.GetType().Name
            : ex.Message;
        return message.Length <= 96 ? message : string.Concat(message.AsSpan(0, 96), "...");
    }
}
