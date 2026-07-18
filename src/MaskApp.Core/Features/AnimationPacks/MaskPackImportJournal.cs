using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.AnimationPacks;

public sealed record MaskPackImportSnapshot
{
    public TextPresetStoreState TextPresets { get; init; } = new();

    public FacePatternStoreState Faces { get; init; } = new();

    public AnimationProjectStoreState Animations { get; init; } = new();

    public GalleryLayoutState GalleryLayout { get; init; } = new();

    public SceneShowState SceneShow { get; init; } = new();
}

public sealed record MaskPackImportJournal
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public string ImportId { get; init; } = string.Empty;

    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    public MaskPackImportSnapshot Original { get; init; } = new();
}

public interface IMaskPackImportJournalStore
{
    Task<MaskPackImportJournal?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(MaskPackImportJournal journal, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}

public sealed class InMemoryMaskPackImportJournalStore : IMaskPackImportJournalStore
{
    private MaskPackImportJournal? journal;

    public Task<MaskPackImportJournal?> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(journal);
    }

    public Task SaveAsync(MaskPackImportJournal journal, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.journal = journal;
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        journal = null;
        return Task.CompletedTask;
    }
}

public class JsonMaskPackImportJournalStoreCore : IMaskPackImportJournalStore
{
    private const long MaxJournalBytes = 256L * 1024 * 1024;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath;

    public JsonMaskPackImportJournalStoreCore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<MaskPackImportJournal?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            if (new FileInfo(filePath).Length > MaxJournalBytes)
            {
                throw new InvalidDataException("MaskPack import recovery journal exceeds the safe size limit.");
            }

            try
            {
                await using var stream = File.OpenRead(filePath);
                var journal = await JsonSerializer.DeserializeAsync<MaskPackImportJournal>(
                    stream,
                    SerializerOptions,
                    cancellationToken).ConfigureAwait(false);
                if (journal is null || journal.SchemaVersion != MaskPackImportJournal.CurrentSchemaVersion)
                {
                    throw new InvalidDataException("MaskPack import recovery journal has an unsupported version.");
                }

                return journal;
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                throw new InvalidDataException(
                    "MaskPack import recovery journal is unreadable; it was preserved for manual recovery.",
                    exception);
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(MaskPackImportJournal journal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(journal);
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
                        journal,
                        SerializerOptions,
                        cancellationToken).ConfigureAwait(false);
                }

                if (new FileInfo(tempPath).Length > MaxJournalBytes)
                {
                    throw new InvalidOperationException("MaskPack import recovery journal exceeds the safe size limit.");
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

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            gate.Release();
        }
    }
}
