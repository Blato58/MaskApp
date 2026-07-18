using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public enum AnimationMediaKind
{
    Gif,
    Video
}

public enum AnimationResizeMode
{
    Crop,
    Fit
}

public enum AnimationPaletteMode
{
    FullColor,
    EightColor,
    Monochrome
}

public enum AnimationDitherMode
{
    None,
    FloydSteinberg
}

public sealed record AnimationMediaImportLimits
{
    public const int AbsoluteMaxBytes = 64 * 1024 * 1024;
    public const int AbsoluteMaxDimension = 4096;
    public const int AbsoluteMaxFrames = AnimationProject.MaxSourceFrames;
    public const long AbsoluteMaxTotalPixels = 24_000_000;
    public static readonly TimeSpan AbsoluteMaxDuration = TimeSpan.FromSeconds(60);

    public int MaxBytes { get; init; } = 32 * 1024 * 1024;

    public int MaxDimension { get; init; } = 2048;

    public int MaxFrames { get; init; } = AnimationProject.MaxSourceFrames;

    public long MaxTotalPixels { get; init; } = 12_000_000;

    public TimeSpan MaxDuration { get; init; } = TimeSpan.FromSeconds(30);

    public AnimationMediaImportLimits Normalize()
    {
        if (MaxBytes is < 1 or > AbsoluteMaxBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxBytes));
        }

        if (MaxDimension is < FacePattern.Width or > AbsoluteMaxDimension)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxDimension));
        }

        if (MaxFrames is < 1 or > AbsoluteMaxFrames)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxFrames));
        }

        if (MaxTotalPixels is < FacePattern.PixelCount or > AbsoluteMaxTotalPixels)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxTotalPixels));
        }

        if (MaxDuration < PerformanceAnimation.MinFrameDuration || MaxDuration > AbsoluteMaxDuration)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxDuration));
        }

        return this;
    }
}

public sealed record AnimationMediaDecodeRequest
{
    public AnimationMediaKind Kind { get; init; }

    public AnimationMediaImportLimits Limits { get; init; } = new();

    public TimeSpan SampleInterval { get; init; } = TimeSpan.FromMilliseconds(100);

    public AnimationMediaDecodeRequest Normalize()
    {
        var interval = SampleInterval < PerformanceAnimation.MinFrameDuration
            ? PerformanceAnimation.MinFrameDuration
            : SampleInterval > TimeSpan.FromSeconds(2)
                ? TimeSpan.FromSeconds(2)
                : SampleInterval;
        return this with { Limits = Limits.Normalize(), SampleInterval = interval };
    }
}

public sealed record AnimationDecodedFrame(
    FaceSampleImage Image,
    TimeSpan Duration);

public sealed record AnimationMediaDecodeResult
{
    public bool Succeeded { get; init; }

    public string Message { get; init; } = string.Empty;

    public IReadOnlyList<AnimationDecodedFrame> Frames { get; init; } = [];

    public TimeSpan SourceDuration { get; init; }

    public static AnimationMediaDecodeResult Failure(string message) => new() { Message = message };

    public static AnimationMediaDecodeResult Success(
        IReadOnlyList<AnimationDecodedFrame> frames,
        TimeSpan sourceDuration,
        string message = "Decoded.") => new()
        {
            Succeeded = true,
            Message = message,
            Frames = frames,
            SourceDuration = sourceDuration
        };
}

public interface IAnimationMediaDecoder
{
    Task<AnimationMediaDecodeResult> DecodeAsync(
        ReadOnlyMemory<byte> data,
        AnimationMediaDecodeRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record AnimationMediaConversionOptions
{
    public AnimationResizeMode ResizeMode { get; init; } = AnimationResizeMode.Crop;

    public AnimationPaletteMode PaletteMode { get; init; } = AnimationPaletteMode.FullColor;

    public AnimationDitherMode DitherMode { get; init; }

    public double HorizontalPosition { get; init; }

    public double VerticalPosition { get; init; }

    public int LitThreshold { get; init; } = 40;

    public AnimationMediaConversionOptions Normalize() => this with
    {
        HorizontalPosition = Math.Clamp(HorizontalPosition, -1, 1),
        VerticalPosition = Math.Clamp(VerticalPosition, -1, 1),
        LitThreshold = Math.Clamp(LitThreshold, 0, 255)
    };
}

public sealed record AnimationMediaImportResult
{
    public bool Succeeded { get; init; }

    public string Message { get; init; } = string.Empty;

    public AnimationProject? Project { get; init; }

    public int DecodedFrameCount { get; init; }

    public int OutputFrameCount { get; init; }

    public int RemovedDuplicateCount { get; init; }

    public long InputByteCount { get; init; }

    public static AnimationMediaImportResult Failure(string message, long inputByteCount = 0) => new()
    {
        Message = message,
        InputByteCount = inputByteCount
    };
}

public sealed class AnimationMediaImportService
{
    private readonly IAnimationMediaDecoder decoder;
    private readonly AnimationMediaImportLimits limits;

    public AnimationMediaImportService(
        IAnimationMediaDecoder decoder,
        AnimationMediaImportLimits? limits = null)
    {
        this.decoder = decoder;
        this.limits = (limits ?? new AnimationMediaImportLimits()).Normalize();
    }

    public async Task<AnimationMediaImportResult> ImportAsync(
        Stream source,
        string displayName,
        AnimationMediaKind kind,
        AnimationMediaConversionOptions? conversionOptions = null,
        TimeSpan? sampleInterval = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        byte[] data;
        try
        {
            data = await ReadBoundedAsync(source, limits.MaxBytes, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidDataException exception)
        {
            return AnimationMediaImportResult.Failure(exception.Message, limits.MaxBytes + 1L);
        }

        if (!MatchesDeclaredKind(data, kind))
        {
            return AnimationMediaImportResult.Failure(
                kind == AnimationMediaKind.Gif
                    ? "The selected file is not a GIF image."
                    : "The selected file does not have a supported video container signature.",
                data.Length);
        }

        AnimationMediaDecodeResult decoded;
        try
        {
            decoded = await decoder.DecodeAsync(
                data,
                new AnimationMediaDecodeRequest
                {
                    Kind = kind,
                    Limits = limits,
                    SampleInterval = sampleInterval ?? TimeSpan.FromMilliseconds(100)
                }.Normalize(),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception is InvalidDataException or ArgumentException or NotSupportedException or IOException)
        {
            return AnimationMediaImportResult.Failure(
                $"Media decoder rejected the file: {ShortMessage(exception)}",
                data.Length);
        }

        var validationError = ValidateDecoded(decoded);
        if (validationError is not null)
        {
            return AnimationMediaImportResult.Failure(validationError, data.Length);
        }

        var options = (conversionOptions ?? new AnimationMediaConversionOptions()).Normalize();
        var converted = new List<AnimationProjectFrame>(decoded.Frames.Count);
        var removedDuplicates = 0;
        string? lastFingerprint = null;
        foreach (var frame in decoded.Frames)
        {
            var pattern = new FacePattern
            {
                Id = $"animation-import-{Guid.NewGuid():N}",
                DisplayName = "Imported animation frame",
                Source = kind == AnimationMediaKind.Gif
                    ? FacePatternSource.ImportedPhoto
                    : FacePatternSource.CapturedPhoto,
                Pixels = AnimationMediaFrameConverter.Convert(frame.Image, options)
            }.Normalize();
            var fingerprint = FaceContentFingerprint.Compute(pattern);
            var duration = ClampDuration(frame.Duration);
            if (converted.Count > 0 && string.Equals(lastFingerprint, fingerprint, StringComparison.OrdinalIgnoreCase))
            {
                var previous = converted[^1];
                var combined = previous.Duration + duration;
                if (combined <= PerformanceAnimation.MaxFrameDuration)
                {
                    converted[^1] = previous with { Duration = combined };
                    removedDuplicates++;
                    continue;
                }

                converted[^1] = previous with { Duration = PerformanceAnimation.MaxFrameDuration };
                duration = combined - PerformanceAnimation.MaxFrameDuration;
            }

            converted.Add(new AnimationProjectFrame
            {
                Id = $"frame-{Guid.NewGuid():N}",
                Pattern = pattern,
                Duration = duration
            });
            lastFingerprint = fingerprint;
        }

        if (converted.Count == 0)
        {
            return AnimationMediaImportResult.Failure("No usable frames remained after conversion.", data.Length);
        }

        var project = new AnimationProject
        {
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Imported Animation" : displayName.Trim(),
            Source = kind == AnimationMediaKind.Gif
                ? AnimationProjectSource.GifImport
                : AnimationProjectSource.VideoImport,
            Frames = converted
        }.Normalize();
        return new AnimationMediaImportResult
        {
            Succeeded = true,
            Message = $"Preview ready: {converted.Count} frame(s) on the physical {FacePattern.Width}x{FacePattern.Height} canvas; removed {removedDuplicates} consecutive duplicate(s). Save only after reviewing quality and slot usage.",
            Project = project,
            DecodedFrameCount = decoded.Frames.Count,
            OutputFrameCount = converted.Count,
            RemovedDuplicateCount = removedDuplicates,
            InputByteCount = data.Length
        };
    }

    private string? ValidateDecoded(AnimationMediaDecodeResult result)
    {
        if (!result.Succeeded)
        {
            return string.IsNullOrWhiteSpace(result.Message) ? "The media could not be decoded." : result.Message;
        }

        if (result.Frames.Count is < 1 || result.Frames.Count > limits.MaxFrames)
        {
            return $"Decoded frame count must be between 1 and {limits.MaxFrames}.";
        }

        if (result.SourceDuration <= TimeSpan.Zero || result.SourceDuration > limits.MaxDuration)
        {
            return $"Media duration must be no longer than {limits.MaxDuration.TotalSeconds:0} seconds.";
        }

        long totalPixels = 0;
        long totalDurationTicks = 0;
        foreach (var frame in result.Frames)
        {
            FaceSampleImage image;
            try
            {
                image = frame.Image.Normalize();
            }
            catch (Exception exception) when (exception is ArgumentException or OverflowException)
            {
                return $"Decoder returned an invalid frame: {ShortMessage(exception)}";
            }

            if (image.Width > limits.MaxDimension || image.Height > limits.MaxDimension)
            {
                return $"A decoded frame exceeds the {limits.MaxDimension}px dimension limit.";
            }

            totalPixels = checked(totalPixels + ((long)image.Width * image.Height));
            if (totalPixels > limits.MaxTotalPixels)
            {
                return $"Decoded media exceeds the {limits.MaxTotalPixels:N0}-pixel work limit.";
            }

            if (frame.Duration <= TimeSpan.Zero)
            {
                return "Decoder returned a frame with no duration.";
            }

            totalDurationTicks = checked(totalDurationTicks + frame.Duration.Ticks);
            if (totalDurationTicks > limits.MaxDuration.Ticks + PerformanceAnimation.MinFrameDuration.Ticks)
            {
                return $"Decoded frame timing exceeds the {limits.MaxDuration.TotalSeconds:0}-second work limit.";
            }
        }

        return null;
    }

    private static async Task<byte[]> ReadBoundedAsync(
        Stream source,
        int maxBytes,
        CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream(Math.Min(maxBytes, 1024 * 1024));
        var buffer = new byte[64 * 1024];
        while (true)
        {
            var remaining = maxBytes - checked((int)memory.Length);
            if (remaining < 0)
            {
                throw new InvalidDataException($"Media file exceeds the {maxBytes / (1024 * 1024)} MB import limit.");
            }

            var read = await source.ReadAsync(
                buffer.AsMemory(0, Math.Min(buffer.Length, remaining + 1)),
                cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                return memory.ToArray();
            }

            if (read > remaining)
            {
                throw new InvalidDataException($"Media file exceeds the {maxBytes / (1024 * 1024)} MB import limit.");
            }

            await memory.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool MatchesDeclaredKind(ReadOnlySpan<byte> data, AnimationMediaKind kind)
    {
        if (kind == AnimationMediaKind.Gif)
        {
            return data.Length >= 6 &&
                (data[..6].SequenceEqual("GIF87a"u8) || data[..6].SequenceEqual("GIF89a"u8));
        }

        if (data.Length < 12)
        {
            return false;
        }

        return data.Slice(4, 4).SequenceEqual("ftyp"u8)
            || data[..4].SequenceEqual(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 })
            || data[..4].SequenceEqual("RIFF"u8)
            || data[..4].SequenceEqual(new byte[] { 0x30, 0x26, 0xB2, 0x75 })
            || data[..4].SequenceEqual(new byte[] { 0x00, 0x00, 0x01, 0xBA });
    }

    private static TimeSpan ClampDuration(TimeSpan duration) =>
        duration < PerformanceAnimation.MinFrameDuration
            ? PerformanceAnimation.MinFrameDuration
            : duration > PerformanceAnimation.MaxFrameDuration
                ? PerformanceAnimation.MaxFrameDuration
                : duration;

    private static string ShortMessage(Exception exception)
    {
        var message = string.IsNullOrWhiteSpace(exception.Message) ? exception.GetType().Name : exception.Message;
        return message.Length <= 160 ? message : string.Concat(message.AsSpan(0, 160), "...");
    }
}

public sealed class UnavailableAnimationMediaDecoder : IAnimationMediaDecoder
{
    public Task<AnimationMediaDecodeResult> DecodeAsync(
        ReadOnlyMemory<byte> data,
        AnimationMediaDecodeRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(AnimationMediaDecodeResult.Failure("GIF/video decoding is unavailable on this platform build."));
}
