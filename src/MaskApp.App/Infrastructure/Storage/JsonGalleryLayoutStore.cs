using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonGalleryLayoutStore : IGalleryLayoutStore
{
    private const long MaxStoreBytes = 4L * 1024 * 1024;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string filePath;

    public JsonGalleryLayoutStore()
        : this(Path.Combine(FileSystem.AppDataDirectory, "gallery-layout.json"))
    {
    }

    public JsonGalleryLayoutStore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<GalleryLayoutState> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return new GalleryLayoutState();
        }

        try
        {
            if (new FileInfo(filePath).Length > MaxStoreBytes)
            {
                return Fallback("Gallery layout exceeds the safe size limit; defaults loaded.");
            }

            await using var stream = File.OpenRead(filePath);
            var state = await JsonSerializer.DeserializeAsync<GalleryLayoutState>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
            if (state is null || state.SchemaVersion is <= 0 or > GalleryLayoutState.CurrentSchemaVersion)
            {
                return Fallback("Gallery layout version is unsupported; defaults loaded.");
            }

            return state.Normalize();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            return Fallback("Gallery layout unavailable; protected defaults loaded.");
        }
    }

    public async Task SaveAsync(GalleryLayoutState state, CancellationToken cancellationToken = default)
    {
        if (state.UsedFallback)
        {
            throw new InvalidOperationException("Unreadable Gallery layout cannot be overwritten.");
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempFilePath = $"{filePath}.tmp";
        try
        {
            await using (var stream = File.Create(tempFilePath))
            {
                await JsonSerializer.SerializeAsync(stream, state.Normalize(), SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (new FileInfo(tempFilePath).Length > MaxStoreBytes)
            {
                throw new InvalidOperationException("Gallery layout exceeds the safe size limit.");
            }

            File.Move(tempFilePath, filePath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            throw;
        }
    }

    private static GalleryLayoutState Fallback(string status) => new()
    {
        UsedFallback = true,
        Status = status
    };
}
