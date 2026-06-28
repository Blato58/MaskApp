using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.QuickActions;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonQuickActionTextSettingsStore : IQuickActionTextSettingsStore
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

    public JsonQuickActionTextSettingsStore()
        : this(Path.Combine(FileSystem.AppDataDirectory, "quick-action-text-settings.json"))
    {
    }

    public JsonQuickActionTextSettingsStore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<QuickActionTextSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return QuickActionTextSettings.RaveDefaults;
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var document = await JsonSerializer.DeserializeAsync<SettingsDocument>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            if (document?.SchemaVersion != SettingsSchemaVersion)
            {
                return QuickActionTextSettings.RaveDefaults;
            }

            return document.Settings.Normalize();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return QuickActionTextSettings.RaveDefaults;
        }
    }

    public async Task SaveAsync(QuickActionTextSettings settings, CancellationToken cancellationToken = default)
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

        public QuickActionTextSettings Settings { get; init; } = QuickActionTextSettings.RaveDefaults;
    }
}
