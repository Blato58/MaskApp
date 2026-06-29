using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.Connect;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonBleAutoConnectSettingsStore : IBleAutoConnectSettingsStore
{
    private const int SettingsSchemaVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string filePath;

    public JsonBleAutoConnectSettingsStore()
        : this(Path.Combine(FileSystem.AppDataDirectory, "ble-auto-connect-settings.json"))
    {
    }

    public JsonBleAutoConnectSettingsStore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<BleAutoConnectSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return BleAutoConnectSettings.Defaults;
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var document = await JsonSerializer.DeserializeAsync<SettingsDocument>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            if (document?.SchemaVersion != SettingsSchemaVersion)
            {
                return BleAutoConnectSettings.Defaults;
            }

            return document.Settings.Normalize();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return BleAutoConnectSettings.Defaults;
        }
    }

    public async Task SaveAsync(BleAutoConnectSettings settings, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var document = new SettingsDocument
        {
            SchemaVersion = SettingsSchemaVersion,
            Settings = settings.Normalize()
        };
        var tempFilePath = $"{filePath}.tmp";

        await using (var stream = File.Create(tempFilePath))
        {
            await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        File.Move(tempFilePath, filePath, overwrite: true);
    }

    private sealed class SettingsDocument
    {
        public int SchemaVersion { get; init; } = SettingsSchemaVersion;

        public BleAutoConnectSettings Settings { get; init; } = BleAutoConnectSettings.Defaults;
    }
}
