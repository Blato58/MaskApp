using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.Experience;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonAppExperienceSettingsStore : IAppExperienceSettingsStore
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

    public JsonAppExperienceSettingsStore()
        : this(Path.Combine(FileSystem.AppDataDirectory, "experience-settings.json"))
    {
    }

    public JsonAppExperienceSettingsStore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<AppExperienceSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return AppExperienceSettings.Defaults;
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var document = await JsonSerializer.DeserializeAsync<SettingsDocument>(
                stream,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);
            return document?.SchemaVersion == SettingsSchemaVersion
                ? document.Settings.Normalize()
                : AppExperienceSettings.Defaults;
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            return AppExperienceSettings.Defaults;
        }
    }

    public async Task SaveAsync(AppExperienceSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var document = new SettingsDocument
        {
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

        public AppExperienceSettings Settings { get; init; } = AppExperienceSettings.Defaults;
    }
}
