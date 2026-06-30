using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.TextPresets;

public sealed class TextPresetDispatcher : ITextPresetDispatcher
{
    private readonly ITextUploadTransport transport;
    private readonly ITextPresetStore? store;

    public TextPresetDispatcher(ITextUploadTransport transport, ITextPresetStore? store = null)
    {
        this.transport = transport;
        this.store = store;
    }

    public async Task<TextPresetDispatchResult> SendAsync(
        TextPreset preset,
        CancellationToken cancellationToken = default)
    {
        var normalizedPreset = preset.Normalize();
        if (!transport.IsReady)
        {
            return TextPresetDispatchResult.Failed(normalizedPreset, "Text not ready", "text transport not ready");
        }

        TextSendPlan plan;
        try
        {
            plan = TextSendPackageFactory.Create(
                normalizedPreset.MaskText,
                normalizedPreset.Style.ToTextSendProfile(),
                transport.SupportsAcknowledgements);
        }
        catch (ArgumentException ex)
        {
            return TextPresetDispatchResult.Failed(normalizedPreset, "Text not ready", ex.Message);
        }

        try
        {
            var result = await transport.UploadAsync(plan.Package, plan.Options, cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                await MarkSentAsync(normalizedPreset.Id, result.Message, cancellationToken).ConfigureAwait(false);
                return TextPresetDispatchResult.Failed(normalizedPreset, "Failed", result.Message);
            }

            var status = string.Equals(result.Message, "Uploaded.", StringComparison.Ordinal)
                ? plan.Summary
                : $"{plan.Summary} · {result.Message}";
            await MarkSentAsync(normalizedPreset.Id, status, cancellationToken).ConfigureAwait(false);
            return TextPresetDispatchResult.Sent(normalizedPreset, status);
        }
        catch (OperationCanceledException)
        {
            return TextPresetDispatchResult.Failed(normalizedPreset, "Text send cancelled.", "cancelled");
        }
        catch (Exception ex)
        {
            var message = $"Failed: {GetShortErrorMessage(ex)}";
            await MarkSentAsync(normalizedPreset.Id, message, cancellationToken).ConfigureAwait(false);
            return TextPresetDispatchResult.Failed(normalizedPreset, message);
        }
    }

    private async Task MarkSentAsync(TextPresetId id, string status, CancellationToken cancellationToken)
    {
        if (store is null)
        {
            return;
        }

        var state = await store.LoadAsync(cancellationToken).ConfigureAwait(false);
        await store.SaveAsync(state.MarkSent(id, status).Normalize(), cancellationToken).ConfigureAwait(false);
    }

    private static string GetShortErrorMessage(Exception ex)
    {
        var message = string.IsNullOrWhiteSpace(ex.Message)
            ? ex.GetType().Name
            : ex.Message;
        return message.Length <= 96 ? message : string.Concat(message.AsSpan(0, 96), "...");
    }
}
