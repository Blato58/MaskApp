using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.QuickActions;

public sealed class QuickActionDispatcher : IQuickActionDispatcher
{
    private static readonly TextLedColor DefaultTextColor = new(0xFF, 0xFF, 0xFF);
    private readonly QuickActionCatalog catalog;
    private readonly IMaskCommandTransport commandTransport;
    private readonly ITextUploadTransport textTransport;

    public QuickActionDispatcher(
        QuickActionCatalog catalog,
        IMaskCommandTransport commandTransport,
        ITextUploadTransport textTransport)
    {
        this.catalog = catalog;
        this.commandTransport = commandTransport;
        this.textTransport = textTransport;
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
            return QuickActionResult.Failed(action.Id, textTransport.StatusText, "text transport not ready");
        }

        var package = TextUploadProtocol.CreatePackage(action.Caption, DefaultTextColor, mode: 3, speed: 70);
        var options = textTransport.SupportsAcknowledgements
            ? TextUploadOptions.RequireAcknowledgements
            : TextUploadOptions.WriteOnlyCompatibility;
        var result = await textTransport.UploadAsync(package, options, cancellationToken).ConfigureAwait(false);
        if (result.Succeeded && !textTransport.SupportsAcknowledgements)
        {
            return QuickActionResult.Sent(action.Id, $"{result.Message} Write-only mode; confirm on mask.");
        }

        return result.Succeeded
            ? QuickActionResult.Sent(action.Id, result.Message)
            : QuickActionResult.Failed(action.Id, result.Message);
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
}
