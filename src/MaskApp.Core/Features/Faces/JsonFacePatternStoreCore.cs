using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskApp.Core.Features.Faces;

public class JsonFacePatternStoreCore : IFacePatternStore
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

    public JsonFacePatternStoreCore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<FacePatternStoreState> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(filePath))
            {
                return FacePatternStoreState.Seeded;
            }

            try
            {
                await using var stream = File.OpenRead(filePath);
                var document = await JsonSerializer.DeserializeAsync<StoreDocument>(stream, SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);
                if (document is null ||
                    document.SchemaVersion is not FacePatternStoreState.LegacySchemaVersion and
                        not FacePatternStoreState.PreviousSchemaVersion and
                        not FacePatternStoreState.CurrentSchemaVersion)
                {
                    return FacePatternStoreState.Seeded with
                    {
                        Status = "Face store version changed; seeded fallback loaded.",
                        UsedFallback = true
                    };
                }

                return new FacePatternStoreState
                {
                    SchemaVersion = document.SchemaVersion,
                    SeedVersion = document.SeedVersion,
                    Patterns = document.Patterns,
                    SlotInstallations = document.SlotInstallations,
                    Status = "Ready."
                }.Normalize();
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                return FacePatternStoreState.Seeded with
                {
                    Status = "Face store could not be read; seeded fallback loaded.",
                    UsedFallback = true
                };
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(FacePatternStoreState state, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var normalized = state.Normalize();
            var document = new StoreDocument
            {
                SchemaVersion = FacePatternStoreState.CurrentSchemaVersion,
                SeedVersion = FacePatternStoreState.CurrentSeedVersion,
                Patterns = normalized.Patterns.ToArray(),
                SlotInstallations = normalized.SlotInstallations.ToArray()
            };
            var tempFilePath = $"{filePath}.tmp";

            await using (var stream = File.Create(tempFilePath))
            {
                await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);
            }

            File.Move(tempFilePath, filePath, overwrite: true);
        }
        finally
        {
            gate.Release();
        }
    }

    private sealed class StoreDocument
    {
        public int SchemaVersion { get; init; } = FacePatternStoreState.CurrentSchemaVersion;

        public int SeedVersion { get; init; } = FacePatternStoreState.CurrentSeedVersion;

        public FacePattern[] Patterns { get; init; } = [];

        public FaceSlotInstallation[] SlotInstallations { get; init; } = [];
    }
}
