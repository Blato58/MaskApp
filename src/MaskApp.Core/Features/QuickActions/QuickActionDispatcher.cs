using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.QuickActions;

public sealed class QuickActionDispatcher : IQuickActionDispatcher
{
    private static readonly TextLedColor DefaultTextColor = new(0xFF, 0xFF, 0xFF);
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

        var layout = QuickCaptionLayout.Create(action.Caption);
        if (!layout.Succeeded)
        {
            return QuickActionResult.Failed(action.Id, "Text not ready", layout.Warning ?? "caption unavailable");
        }

        var package = TextUploadProtocol.CreatePackageFromLedData(
            layout.DisplayText,
            layout.LedData,
            QuickCaptionLayout.CreateColumnColors(layout.LedData, DefaultTextColor),
            settings.ProtocolMode,
            settings.Speed);
        var options = settings.SendMode == QuickCaptionSendMode.FastWriteOnly
            ? TextUploadOptions.FastWriteOnly
            : TextUploadOptions.RequireAcknowledgements;

        try
        {
            var result = await textTransport.UploadAsync(package, options, cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return layout.WasShortened
                    ? QuickActionResult.Sent(action.Id, "Sent, confirm on mask")
                        with { Status = layout.Warning ?? "caption shortened" }
                    : QuickActionResult.Sent(action.Id, "Sent, confirm on mask");
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

    private static string GetShortErrorMessage(Exception ex)
    {
        var message = string.IsNullOrWhiteSpace(ex.Message)
            ? ex.GetType().Name
            : ex.Message;
        return message.Length <= 96 ? message : string.Concat(message.AsSpan(0, 96), "...");
    }
}
