using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskApp.Core.Features.Scenes;

public interface ISceneShowStore
{
    Task<SceneShowState> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(SceneShowState state, CancellationToken cancellationToken = default);
}

public sealed class InMemorySceneShowStore : ISceneShowStore
{
    private SceneShowState state;

    public InMemorySceneShowStore(SceneShowState? initialState = null)
    {
        state = (initialState ?? new SceneShowState()).Normalize();
    }

    public Task<SceneShowState> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(state.Normalize());
    }

    public Task SaveAsync(SceneShowState state, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (state.UsedFallback)
        {
            throw new InvalidOperationException("Unreadable Scene/setlist data cannot be overwritten.");
        }

        this.state = state.Normalize();
        return Task.CompletedTask;
    }
}

public class JsonSceneShowStoreCore : ISceneShowStore
{
    private const long MaxStoreBytes = 4L * 1024 * 1024;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath;

    public JsonSceneShowStoreCore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<SceneShowState> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(filePath))
            {
                return new SceneShowState();
            }

            try
            {
                if (new FileInfo(filePath).Length > MaxStoreBytes)
                {
                    return Fallback("Scene/setlist store exceeds the safe size limit; empty fallback loaded.");
                }

                await using var stream = File.OpenRead(filePath);
                var state = await JsonSerializer.DeserializeAsync<SceneShowState>(
                    stream,
                    SerializerOptions,
                    cancellationToken).ConfigureAwait(false);
                if (state is null || state.SchemaVersion != SceneShowState.CurrentSchemaVersion)
                {
                    return Fallback("Scene/setlist version changed; empty fallback loaded.");
                }

                return state.Normalize();
            }
            catch (Exception exception) when (
                exception is JsonException or IOException or UnauthorizedAccessException or ArgumentException)
            {
                return Fallback("Scenes/setlists could not be read; empty fallback loaded.");
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(SceneShowState state, CancellationToken cancellationToken = default)
    {
        if (state.UsedFallback)
        {
            throw new InvalidOperationException("Unreadable Scene/setlist data cannot be overwritten.");
        }

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = $"{filePath}.tmp";
            try
            {
                await using (var stream = File.Create(tempPath))
                {
                    await JsonSerializer.SerializeAsync(
                        stream,
                        state.Normalize(),
                        SerializerOptions,
                        cancellationToken).ConfigureAwait(false);
                }

                if (new FileInfo(tempPath).Length > MaxStoreBytes)
                {
                    throw new InvalidOperationException("Scene/setlist store exceeds the safe size limit.");
                }

                File.Move(tempPath, filePath, overwrite: true);
            }
            catch
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                throw;
            }
        }
        finally
        {
            gate.Release();
        }
    }

    private static SceneShowState Fallback(string status) => new()
    {
        UsedFallback = true,
        Status = status
    };
}
