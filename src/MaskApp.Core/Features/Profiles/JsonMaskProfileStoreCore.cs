using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskApp.Core.Features.Profiles;

public class JsonMaskProfileStoreCore : IMaskProfileStore
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

    public JsonMaskProfileStoreCore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<MaskProfileStoreState> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(filePath))
            {
                return new MaskProfileStoreState();
            }

            try
            {
                await using var stream = File.OpenRead(filePath);
                var state = await JsonSerializer
                    .DeserializeAsync<MaskProfileStoreState>(stream, SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);
                if (state is null || state.SchemaVersion != MaskProfileStoreState.CurrentSchemaVersion)
                {
                    return new MaskProfileStoreState
                    {
                        Status = "Mask profile version changed; empty fallback loaded.",
                        UsedFallback = true
                    };
                }

                return state.Normalize();
            }
            catch (Exception exception) when (
                exception is JsonException or IOException or UnauthorizedAccessException)
            {
                return new MaskProfileStoreState
                {
                    Status = "Mask profiles could not be read; empty fallback loaded.",
                    UsedFallback = true
                };
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(MaskProfileStoreState state, CancellationToken cancellationToken = default)
    {
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
                await JsonSerializer
                    .SerializeAsync(stream, state.Normalize(), SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);
            }

            File.Move(tempFilePath, filePath, overwrite: true);
        }
        finally
        {
            gate.Release();
        }
    }
}
