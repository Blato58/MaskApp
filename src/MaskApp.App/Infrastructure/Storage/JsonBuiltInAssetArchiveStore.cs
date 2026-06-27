using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.BuiltIns;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonBuiltInAssetArchiveStore : IBuiltInAssetArchiveStore
{
    private const int ArchiveSchemaVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string filePath;

    public JsonBuiltInAssetArchiveStore()
        : this(Path.Combine(FileSystem.AppDataDirectory, "built-in-asset-archive.json"))
    {
    }

    public JsonBuiltInAssetArchiveStore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<BuiltInAssetArchive> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return BuiltInAssetArchive.Empty;
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var document = await JsonSerializer.DeserializeAsync<ArchiveDocument>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            if (document?.SchemaVersion != ArchiveSchemaVersion)
            {
                return BuiltInAssetArchive.Empty;
            }

            return new BuiltInAssetArchive(document.BuiltIns);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return BuiltInAssetArchive.Empty;
        }
    }

    public async Task SaveAsync(BuiltInAssetArchive archive, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var document = new ArchiveDocument
        {
            SchemaVersion = ArchiveSchemaVersion,
            BuiltIns = archive.Records.ToArray()
        };
        var tempFilePath = $"{filePath}.tmp";

        await using (var stream = File.Create(tempFilePath))
        {
            await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        File.Move(tempFilePath, filePath, overwrite: true);
    }

    private sealed class ArchiveDocument
    {
        public int SchemaVersion { get; init; } = ArchiveSchemaVersion;

        public BuiltInAssetRecord[] BuiltIns { get; init; } = [];
    }
}
