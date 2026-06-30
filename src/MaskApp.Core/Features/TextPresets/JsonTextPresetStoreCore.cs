using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskApp.Core.Features.TextPresets;

public class JsonTextPresetStoreCore : ITextPresetStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string filePath;

    public JsonTextPresetStoreCore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<TextPresetStoreState> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return TextPresetStoreState.Seeded;
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var document = await JsonSerializer.DeserializeAsync<StoreDocument>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
            if (document?.SchemaVersion != TextPresetStoreState.CurrentSchemaVersion)
            {
                return TextPresetStoreState.Seeded with
                {
                    Status = "Preset store version changed; seeded fallback loaded.",
                    UsedFallback = true
                };
            }

            return new TextPresetStoreState
            {
                SchemaVersion = document.SchemaVersion,
                SeedVersion = document.SeedVersion,
                Presets = document.Presets,
                DeletedSeedIds = document.DeletedSeedIds,
                Status = "Ready."
            }.Normalize();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return TextPresetStoreState.Seeded with
            {
                Status = "Preset store could not be read; seeded fallback loaded.",
                UsedFallback = true
            };
        }
    }

    public async Task SaveAsync(TextPresetStoreState state, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var normalized = state.Normalize();
        var document = new StoreDocument
        {
            SchemaVersion = TextPresetStoreState.CurrentSchemaVersion,
            SeedVersion = TextPresetStoreState.CurrentSeedVersion,
            Presets = normalized.Presets.ToArray(),
            DeletedSeedIds = normalized.DeletedSeedIds.ToArray()
        };
        var tempFilePath = $"{filePath}.tmp";

        await using (var stream = File.Create(tempFilePath))
        {
            await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        File.Move(tempFilePath, filePath, overwrite: true);
    }

    private sealed class StoreDocument
    {
        public int SchemaVersion { get; init; } = TextPresetStoreState.CurrentSchemaVersion;

        public int SeedVersion { get; init; } = TextPresetStoreState.CurrentSeedVersion;

        public TextPreset[] Presets { get; init; } = [];

        public TextPresetId[] DeletedSeedIds { get; init; } = [];
    }
}
