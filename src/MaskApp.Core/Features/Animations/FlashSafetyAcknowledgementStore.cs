using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskApp.Core.Features.Animations;

public interface IFlashSafetyAcknowledgementStore
{
    Task<FlashSafetyAcknowledgementState> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(FlashSafetyAcknowledgementState state, CancellationToken cancellationToken = default);
}

public sealed class InMemoryFlashSafetyAcknowledgementStore : IFlashSafetyAcknowledgementStore
{
    private FlashSafetyAcknowledgementState state;

    public InMemoryFlashSafetyAcknowledgementStore(FlashSafetyAcknowledgementState? initialState = null)
    {
        state = (initialState ?? new FlashSafetyAcknowledgementState()).Normalize();
    }

    public Task<FlashSafetyAcknowledgementState> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(state.Normalize());
    }

    public Task SaveAsync(
        FlashSafetyAcknowledgementState state,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.state = state.Normalize();
        return Task.CompletedTask;
    }
}

public class JsonFlashSafetyAcknowledgementStoreCore : IFlashSafetyAcknowledgementStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath;

    public JsonFlashSafetyAcknowledgementStoreCore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<FlashSafetyAcknowledgementState> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(filePath))
            {
                return new FlashSafetyAcknowledgementState();
            }

            try
            {
                await using var stream = File.OpenRead(filePath);
                var state = await JsonSerializer.DeserializeAsync<FlashSafetyAcknowledgementState>(
                    stream,
                    SerializerOptions,
                    cancellationToken).ConfigureAwait(false);
                if (state is null || state.SchemaVersion != FlashSafetyAcknowledgementState.CurrentSchemaVersion)
                {
                    return Fallback("Flash-safety acknowledgement version changed; empty fallback loaded.");
                }

                return state.Normalize();
            }
            catch (Exception exception) when (
                exception is JsonException or IOException or UnauthorizedAccessException)
            {
                return Fallback("Flash-safety acknowledgements could not be read; empty fallback loaded.");
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(
        FlashSafetyAcknowledgementState state,
        CancellationToken cancellationToken = default)
    {
        if (state.UsedFallback)
        {
            throw new InvalidOperationException("Unreadable flash-safety data cannot be overwritten.");
        }

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempFilePath = $"{filePath}.tmp";
            await using (var stream = File.Create(tempFilePath))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    state.Normalize(),
                    SerializerOptions,
                    cancellationToken).ConfigureAwait(false);
            }

            File.Move(tempFilePath, filePath, overwrite: true);
        }
        finally
        {
            gate.Release();
        }
    }

    private static FlashSafetyAcknowledgementState Fallback(string status) => new()
    {
        UsedFallback = true,
        Status = status
    };
}

public sealed class FlashSafetyAcknowledgementService
{
    public const string RequiredWarning =
        "This animation exceeds the conservative flash limit and may trigger photosensitive reactions.";

    private readonly IFlashSafetyAcknowledgementStore store;

    public FlashSafetyAcknowledgementService(IFlashSafetyAcknowledgementStore store)
    {
        this.store = store;
    }

    public async Task<FlashSafetyAcknowledgement> AcknowledgeAsync(
        FlashSafetyAssessment assessment,
        CancellationToken cancellationToken = default)
    {
        if (assessment.IsSafeByDefault)
        {
            throw new InvalidOperationException("Safe content does not require a flash-risk override.");
        }

        var state = await store.LoadAsync(cancellationToken).ConfigureAwait(false);
        if (state.UsedFallback)
        {
            throw new InvalidOperationException(state.Status);
        }

        var acknowledgement = new FlashSafetyAcknowledgement
        {
            ContentId = assessment.ContentId,
            RevisionHash = assessment.RevisionHash,
            AcknowledgedAt = DateTimeOffset.UtcNow,
            Warning = RequiredWarning
        }.Normalize();
        await store.SaveAsync(state with
        {
            Acknowledgements = state.Acknowledgements
                .Where(item => !string.Equals(item.ContentId, assessment.ContentId, StringComparison.Ordinal))
                .Append(acknowledgement)
                .ToArray()
        }, cancellationToken).ConfigureAwait(false);
        return acknowledgement;
    }

    public async Task RevokeAsync(string contentId, CancellationToken cancellationToken = default)
    {
        var state = await store.LoadAsync(cancellationToken).ConfigureAwait(false);
        if (state.UsedFallback)
        {
            throw new InvalidOperationException(state.Status);
        }

        await store.SaveAsync(state with
        {
            Acknowledgements = state.Acknowledgements
                .Where(item => !string.Equals(item.ContentId, contentId, StringComparison.Ordinal))
                .ToArray()
        }, cancellationToken).ConfigureAwait(false);
    }
}
