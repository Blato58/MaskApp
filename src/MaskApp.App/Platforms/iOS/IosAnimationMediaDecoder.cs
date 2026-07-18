#if IOS
using System.Runtime.InteropServices;
using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using ImageIO;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.App.Platforms.iOS;

public sealed class IosAnimationMediaDecoder : IAnimationMediaDecoder
{
    private const int MaximumGifSourceFrames = 2400;

    public Task<AnimationMediaDecodeResult> DecodeAsync(
        ReadOnlyMemory<byte> data,
        AnimationMediaDecodeRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        request = request.Normalize();
        return Task.Run(
            () => request.Kind == AnimationMediaKind.Gif
                ? DecodeGif(data, request, cancellationToken)
                : DecodeVideo(data, request, cancellationToken),
            cancellationToken);
    }

    private static AnimationMediaDecodeResult DecodeGif(
        ReadOnlyMemory<byte> data,
        AnimationMediaDecodeRequest request,
        CancellationToken cancellationToken)
    {
        using var nativeData = NSData.FromArray(data.ToArray());
        using var source = CGImageSource.FromData(nativeData);
        if (source is null || source.ImageCount is < 1 or > MaximumGifSourceFrames)
        {
            return AnimationMediaDecodeResult.Failure(
                source?.ImageCount > MaximumGifSourceFrames
                    ? $"GIF contains more than the safe {MaximumGifSourceFrames}-frame source limit."
                    : "GIF contains no decodable frames.");
        }

        var sourceCount = checked((int)source.ImageCount);
        var durations = new TimeSpan[sourceCount];
        var totalDuration = TimeSpan.Zero;
        var propertyOptions = new CGImageOptions();
        for (var index = 0; index < sourceCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var duration = GetGifFrameDuration(source.CopyProperties(propertyOptions, index));
            durations[index] = duration;
            totalDuration += duration;
            if (totalDuration > request.Limits.MaxDuration)
            {
                return AnimationMediaDecodeResult.Failure(
                    $"GIF is longer than the {request.Limits.MaxDuration.TotalSeconds:0}-second import limit.");
            }
        }

        var samples = BuildSamples(durations, request.SampleInterval, request.Limits.MaxFrames);
        var frames = new List<AnimationDecodedFrame>(samples.Count);
        long decodedPixels = 0;
        foreach (var sample in samples)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var image = source.CreateImage(sample.SourceIndex, propertyOptions);
            if (image is null)
            {
                return AnimationMediaDecodeResult.Failure($"GIF frame {sample.SourceIndex + 1} could not be decoded.");
            }

            var validationError = ValidateDimensions(image, request.Limits);
            if (validationError is not null)
            {
                return AnimationMediaDecodeResult.Failure(validationError);
            }

            decodedPixels += checked((long)image.Width * image.Height);
            if (decodedPixels > request.Limits.MaxTotalPixels)
            {
                return AnimationMediaDecodeResult.Failure(
                    $"Decoded GIF exceeds the {request.Limits.MaxTotalPixels:N0}-pixel work limit.");
            }

            frames.Add(new AnimationDecodedFrame(ReadImage(image), sample.Duration));
        }

        return AnimationMediaDecodeResult.Success(
            frames,
            totalDuration,
            $"Decoded {frames.Count} sampled GIF frame(s) from {sourceCount} source frame(s).");
    }

    private static AnimationMediaDecodeResult DecodeVideo(
        ReadOnlyMemory<byte> data,
        AnimationMediaDecodeRequest request,
        CancellationToken cancellationToken)
    {
        var cacheDirectory = FileSystem.CacheDirectory;
        Directory.CreateDirectory(cacheDirectory);
        var path = Path.Combine(cacheDirectory, $"animation-import-{Guid.NewGuid():N}.media");
        try
        {
            File.WriteAllBytes(path, data.ToArray());
            using var url = NSUrl.FromFilename(path);
            using var asset = new AVUrlAsset(url);
            var durationSeconds = asset.Duration.Seconds;
            if (!double.IsFinite(durationSeconds) || durationSeconds <= 0)
            {
                return AnimationMediaDecodeResult.Failure("Video duration is unavailable or invalid.");
            }

            var duration = TimeSpan.FromSeconds(durationSeconds);
            if (duration > request.Limits.MaxDuration)
            {
                return AnimationMediaDecodeResult.Failure(
                    $"Video is longer than the {request.Limits.MaxDuration.TotalSeconds:0}-second import limit.");
            }

            using var generator = new AVAssetImageGenerator(asset)
            {
                AppliesPreferredTrackTransform = true,
                MaximumSize = new CGSize(request.Limits.MaxDimension, request.Limits.MaxDimension),
                RequestedTimeToleranceBefore = CMTime.Zero,
                RequestedTimeToleranceAfter = CMTime.Zero
            };
            var times = BuildVideoSampleTimes(duration, request.SampleInterval, request.Limits.MaxFrames);
            var frames = new List<AnimationDecodedFrame>(times.Count);
            long decodedPixels = 0;
            for (var index = 0; index < times.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var requestedTime = CMTime.FromSeconds(times[index].TotalSeconds, 600);
#pragma warning disable CA1422 // The synchronous API remains the iOS 15 fallback supported by this app.
                using var image = generator.CopyCGImageAtTime(requestedTime, out _, out var error);
#pragma warning restore CA1422
                if (image is null)
                {
                    return AnimationMediaDecodeResult.Failure(
                        $"Video frame {index + 1} could not be decoded: {error?.LocalizedDescription ?? "unknown decoder error"}.");
                }

                var validationError = ValidateDimensions(image, request.Limits);
                if (validationError is not null)
                {
                    return AnimationMediaDecodeResult.Failure(validationError);
                }

                decodedPixels += checked((long)image.Width * image.Height);
                if (decodedPixels > request.Limits.MaxTotalPixels)
                {
                    return AnimationMediaDecodeResult.Failure(
                        $"Decoded video exceeds the {request.Limits.MaxTotalPixels:N0}-pixel work limit.");
                }

                var next = index + 1 < times.Count ? times[index + 1] : duration;
                var frameDuration = next - times[index];
                frames.Add(new AnimationDecodedFrame(
                    ReadImage(image),
                    frameDuration < PerformanceAnimation.MinFrameDuration
                        ? PerformanceAnimation.MinFrameDuration
                        : frameDuration));
            }

            return AnimationMediaDecodeResult.Success(
                frames,
                duration,
                $"Decoded {frames.Count} bounded video sample(s).");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static IReadOnlyList<GifSample> BuildSamples(
        IReadOnlyList<TimeSpan> sourceDurations,
        TimeSpan sampleInterval,
        int maxFrames)
    {
        var total = TimeSpan.FromTicks(sourceDurations.Sum(duration => duration.Ticks));
        var effectiveInterval = sampleInterval;
        if (Math.Ceiling(total.Ticks / (double)effectiveInterval.Ticks) > maxFrames)
        {
            effectiveInterval = TimeSpan.FromTicks((long)Math.Ceiling(total.Ticks / (double)maxFrames));
        }

        var samples = new List<GifSample>();
        var frameStart = TimeSpan.Zero;
        var sourceIndex = 0;
        for (var timestamp = TimeSpan.Zero; timestamp < total && samples.Count < maxFrames; timestamp += effectiveInterval)
        {
            while (sourceIndex + 1 < sourceDurations.Count &&
                timestamp >= frameStart + sourceDurations[sourceIndex])
            {
                frameStart += sourceDurations[sourceIndex++];
            }

            var next = timestamp + effectiveInterval < total ? timestamp + effectiveInterval : total;
            samples.Add(new GifSample(sourceIndex, next - timestamp));
        }

        return samples;
    }

    private static IReadOnlyList<TimeSpan> BuildVideoSampleTimes(
        TimeSpan duration,
        TimeSpan requestedInterval,
        int maxFrames)
    {
        var interval = requestedInterval;
        if (Math.Ceiling(duration.Ticks / (double)interval.Ticks) > maxFrames)
        {
            interval = TimeSpan.FromTicks((long)Math.Ceiling(duration.Ticks / (double)maxFrames));
        }

        var result = new List<TimeSpan>();
        for (var timestamp = TimeSpan.Zero; timestamp < duration && result.Count < maxFrames; timestamp += interval)
        {
            result.Add(timestamp);
        }

        return result;
    }

    private static TimeSpan GetGifFrameDuration(NSDictionary? properties)
    {
        if (properties?[ImageIO.CGImageProperties.GIFDictionary] is NSDictionary gif)
        {
            var number = gif[ImageIO.CGImageProperties.GIFUnclampedDelayTime] as NSNumber
                ?? gif[ImageIO.CGImageProperties.GIFDelayTime] as NSNumber;
            if (number is not null && double.IsFinite(number.DoubleValue) && number.DoubleValue > 0)
            {
                return TimeSpan.FromSeconds(Math.Clamp(number.DoubleValue, 0.02, 10));
            }
        }

        return TimeSpan.FromMilliseconds(100);
    }

    private static string? ValidateDimensions(CGImage image, AnimationMediaImportLimits limits) =>
        image.Width <= 0 || image.Height <= 0
            ? "Decoder returned an empty image."
            : image.Width > limits.MaxDimension || image.Height > limits.MaxDimension
                ? $"Decoded media exceeds the {limits.MaxDimension}px dimension limit."
                : null;

    private static FaceSampleImage ReadImage(CGImage image)
    {
        var width = checked((int)image.Width);
        var height = checked((int)image.Height);
        var bytesPerRow = checked(width * 4);
        var raw = new byte[checked(height * bytesPerRow)];
        var handle = GCHandle.Alloc(raw, GCHandleType.Pinned);
        try
        {
            using var colorSpace = CGColorSpace.CreateDeviceRGB();
            var flags = CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast;
            using var context = new CGBitmapContext(
                handle.AddrOfPinnedObject(),
                width,
                height,
                8,
                bytesPerRow,
                colorSpace,
                (CGImageAlphaInfo)flags);
            context.DrawImage(new CGRect(0, 0, width, height), image);
        }
        finally
        {
            handle.Free();
        }

        var pixels = new FaceSamplePixel[checked(width * height)];
        for (var index = 0; index < pixels.Length; index++)
        {
            var offset = index * 4;
            pixels[index] = new FaceSamplePixel(
                raw[offset],
                raw[offset + 1],
                raw[offset + 2],
                raw[offset + 3]);
        }

        return new FaceSampleImage(width, height, pixels);
    }

    private sealed record GifSample(int SourceIndex, TimeSpan Duration);
}
#endif
