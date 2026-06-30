using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonGalleryLayoutStore : IGalleryLayoutStore
{
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
            await using var stream = File.OpenRead(filePath);
            var state = await JsonSerializer.DeserializeAsync<GalleryLayoutState>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
            return state?.Normalize() ?? new GalleryLayoutState();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return new GalleryLayoutState { Status = "Gallery layout unavailable; using defaults." };
        }
    }

    public async Task SaveAsync(GalleryLayoutState state, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempFilePath = $"{filePath}.tmp";
        await using (var stream = File.Create(tempFilePath))
        {
            await JsonSerializer.SerializeAsync(stream, state.Normalize(), SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        File.Move(tempFilePath, filePath, overwrite: true);
    }
}
