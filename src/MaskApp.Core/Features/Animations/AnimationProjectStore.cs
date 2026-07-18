using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public interface IAnimationProjectStore
{
    Task<AnimationProjectStoreState> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AnimationProjectStoreState state, CancellationToken cancellationToken = default);
}

public sealed class InMemoryAnimationProjectStore : IAnimationProjectStore
{
    private AnimationProjectStoreState state;

    public InMemoryAnimationProjectStore(AnimationProjectStoreState? initialState = null)
    {
        state = (initialState ?? new AnimationProjectStoreState()).Normalize();
    }

    public Task<AnimationProjectStoreState> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(state.Normalize());
    }

    public Task SaveAsync(AnimationProjectStoreState state, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (state.UsedFallback)
        {
            throw new InvalidOperationException("Unreadable animation project data cannot be overwritten.");
        }

        this.state = state.Normalize();
        return Task.CompletedTask;
    }
}

public class JsonAnimationProjectStoreCore : IAnimationProjectStore
{
    private const long MaxStoreBytes = 96L * 1024 * 1024;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath;

    public JsonAnimationProjectStoreCore(string filePath)
    {
        this.filePath = filePath;
    }

    public async Task<AnimationProjectStoreState> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(filePath))
            {
                return new AnimationProjectStoreState();
            }

            try
            {
                if (new FileInfo(filePath).Length > MaxStoreBytes)
                {
                    return Fallback("Animation project store exceeds the safe size limit; empty fallback loaded.");
                }

                await using var stream = File.OpenRead(filePath);
                var document = await JsonSerializer.DeserializeAsync<StoreDocument>(
                    stream,
                    SerializerOptions,
                    cancellationToken).ConfigureAwait(false);
                if (document is null || document.SchemaVersion != AnimationProjectStoreState.CurrentSchemaVersion)
                {
                    return Fallback("Animation project version changed; empty fallback loaded.");
                }

                if (document.Projects.Length > AnimationProject.MaxProjects)
                {
                    return Fallback("Animation project count exceeds the safe limit; empty fallback loaded.");
                }

                return new AnimationProjectStoreState
                {
                    Projects = document.Projects.Select(FromDocument).ToArray(),
                    Status = "Ready."
                }.Normalize();
            }
            catch (Exception exception) when (
                exception is JsonException or IOException or UnauthorizedAccessException or
                    ArgumentException or FormatException or OverflowException)
            {
                return Fallback("Animation projects could not be read; empty fallback loaded.");
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(
        AnimationProjectStoreState state,
        CancellationToken cancellationToken = default)
    {
        if (state.UsedFallback)
        {
            throw new InvalidOperationException("Unreadable animation project data cannot be overwritten.");
        }

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var normalized = state.Normalize();
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var document = new StoreDocument
            {
                Projects = normalized.Projects.Select(ToDocument).ToArray()
            };
            var tempFilePath = $"{filePath}.tmp";
            try
            {
                await using (var stream = File.Create(tempFilePath))
                {
                    await JsonSerializer.SerializeAsync(
                        stream,
                        document,
                        SerializerOptions,
                        cancellationToken).ConfigureAwait(false);
                }

                if (new FileInfo(tempFilePath).Length > MaxStoreBytes)
                {
                    throw new InvalidOperationException("Animation project store exceeds the safe size limit.");
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
        finally
        {
            gate.Release();
        }
    }

    private static ProjectDocument ToDocument(AnimationProject source)
    {
        var project = source.Normalize();
        return new ProjectDocument
        {
            Id = project.Id,
            DisplayName = project.DisplayName,
            Source = project.Source,
            LoopMode = project.LoopMode,
            FiniteLoopCount = project.FiniteLoopCount,
            Bpm = project.Bpm,
            IsFavorite = project.IsFavorite,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Frames = project.Frames.Select(frame => new FrameDocument
            {
                Id = frame.Id,
                DurationMilliseconds = frame.Duration.TotalMilliseconds,
                Pixels = PackPixels(frame.Pattern)
            }).ToArray()
        };
    }

    private static AnimationProject FromDocument(ProjectDocument source)
    {
        if (source.Frames.Length is < 1 or > AnimationProject.MaxSourceFrames)
        {
            throw new ArgumentException("Animation project frame count is outside the safe limit.");
        }

        return new AnimationProject
        {
            Id = source.Id,
            DisplayName = source.DisplayName,
            Source = source.Source,
            LoopMode = source.LoopMode,
            FiniteLoopCount = source.FiniteLoopCount,
            Bpm = source.Bpm,
            IsFavorite = source.IsFavorite,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            Frames = source.Frames.Select((frame, index) => new AnimationProjectFrame
            {
                Id = frame.Id,
                Duration = TimeSpan.FromMilliseconds(frame.DurationMilliseconds),
                Pattern = new FacePattern
                {
                    Id = $"animation-frame-{frame.Id}",
                    DisplayName = $"Frame {index + 1}",
                    Pixels = UnpackPixels(frame.Pixels)
                }
            }).ToArray()
        }.Normalize();
    }

    private static string PackPixels(FacePattern source)
    {
        var pattern = source.Normalize();
        var bytes = new byte[FacePattern.PixelCount * 4];
        for (var index = 0; index < pattern.Pixels.Length; index++)
        {
            var pixel = pattern.Pixels[index].Normalize();
            var offset = index * 4;
            bytes[offset] = pixel.IsLit ? (byte)1 : (byte)0;
            bytes[offset + 1] = pixel.Color.Red;
            bytes[offset + 2] = pixel.Color.Green;
            bytes[offset + 3] = pixel.Color.Blue;
        }

        return Convert.ToBase64String(bytes);
    }

    private static FacePixel[] UnpackPixels(string encoded)
    {
        var bytes = Convert.FromBase64String(encoded ?? string.Empty);
        if (bytes.Length != FacePattern.PixelCount * 4)
        {
            throw new FormatException("Animation frame pixel payload has an invalid length.");
        }

        var pixels = new FacePixel[FacePattern.PixelCount];
        for (var index = 0; index < pixels.Length; index++)
        {
            var offset = index * 4;
            pixels[index] = bytes[offset] == 1
                ? new FacePixel(true, new FaceColor(bytes[offset + 1], bytes[offset + 2], bytes[offset + 3]))
                : FacePixel.Off;
        }

        return pixels;
    }

    private static AnimationProjectStoreState Fallback(string status) => new()
    {
        UsedFallback = true,
        Status = status
    };

    private sealed class StoreDocument
    {
        public int SchemaVersion { get; init; } = AnimationProjectStoreState.CurrentSchemaVersion;

        public ProjectDocument[] Projects { get; init; } = [];
    }

    private sealed class ProjectDocument
    {
        public string Id { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public AnimationProjectSource Source { get; init; }
        public AnimationLoopMode LoopMode { get; init; }
        public int FiniteLoopCount { get; init; } = 1;
        public double? Bpm { get; init; }
        public bool IsFavorite { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
        public FrameDocument[] Frames { get; init; } = [];
    }

    private sealed class FrameDocument
    {
        public string Id { get; init; } = string.Empty;
        public double DurationMilliseconds { get; init; }
        public string Pixels { get; init; } = string.Empty;
    }
}
