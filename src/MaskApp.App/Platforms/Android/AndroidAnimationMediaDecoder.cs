#if ANDROID
using Android.Graphics;
using Android.Media;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.App.Platforms.Android;

public sealed class AndroidAnimationMediaDecoder : IAnimationMediaDecoder
{
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
#pragma warning disable CA1422 // Movie is the only bounded frame-stepping API available across the app's Android 23+ range.
        var bytes = data.ToArray();
        using var movie = Movie.DecodeByteArray(bytes, 0, bytes.Length);
        if (movie is null || movie.Width() <= 0 || movie.Height() <= 0)
        {
            return AnimationMediaDecodeResult.Failure("GIF contains no decodable frames.");
        }

        if (movie.Width() > request.Limits.MaxDimension || movie.Height() > request.Limits.MaxDimension)
        {
            return AnimationMediaDecodeResult.Failure(
                $"Decoded media exceeds the {request.Limits.MaxDimension}px dimension limit.");
        }

        var durationMilliseconds = movie.Duration();
        if (durationMilliseconds <= 0)
        {
            durationMilliseconds = 1000;
        }

        var duration = TimeSpan.FromMilliseconds(durationMilliseconds);
        if (duration > request.Limits.MaxDuration)
        {
            return AnimationMediaDecodeResult.Failure(
                $"GIF is longer than the {request.Limits.MaxDuration.TotalSeconds:0}-second import limit.");
        }

        var interval = GetEffectiveInterval(duration, request.SampleInterval, request.Limits.MaxFrames);
        var frames = new List<AnimationDecodedFrame>();
        for (var timestamp = TimeSpan.Zero; timestamp < duration && frames.Count < request.Limits.MaxFrames; timestamp += interval)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var decodedPixels = checked((long)(frames.Count + 1) * movie.Width() * movie.Height());
            if (decodedPixels > request.Limits.MaxTotalPixels)
            {
                return AnimationMediaDecodeResult.Failure(
                    $"Decoded GIF exceeds the {request.Limits.MaxTotalPixels:N0}-pixel work limit.");
            }

            using var bitmap = Bitmap.CreateBitmap(movie.Width(), movie.Height(), Bitmap.Config.Argb8888!);
            using var canvas = new Canvas(bitmap);
            canvas.DrawColor(global::Android.Graphics.Color.Transparent, PorterDuff.Mode.Clear!);
            movie.SetTime((int)timestamp.TotalMilliseconds);
            movie.Draw(canvas, 0, 0);
            var next = timestamp + interval < duration ? timestamp + interval : duration;
            frames.Add(new AnimationDecodedFrame(ReadBitmap(bitmap), next - timestamp));
        }

        var result = AnimationMediaDecodeResult.Success(
            frames,
            duration,
            $"Decoded {frames.Count} sampled GIF frame(s).");
#pragma warning restore CA1422
        return result;
    }

    private static AnimationMediaDecodeResult DecodeVideo(
        ReadOnlyMemory<byte> data,
        AnimationMediaDecodeRequest request,
        CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(27))
        {
            return AnimationMediaDecodeResult.Failure(
                "Safe bounded video-frame decoding requires Android 8.1 or newer; GIF import remains available.");
        }

        var cacheDirectory = FileSystem.CacheDirectory;
        Directory.CreateDirectory(cacheDirectory);
        var path = System.IO.Path.Combine(cacheDirectory, $"animation-import-{Guid.NewGuid():N}.media");
        try
        {
            File.WriteAllBytes(path, data.ToArray());
            using var retriever = new MediaMetadataRetriever();
            retriever.SetDataSource(path);
            if (!long.TryParse(retriever.ExtractMetadata(MetadataKey.Duration), out var durationMilliseconds) ||
                durationMilliseconds <= 0)
            {
                return AnimationMediaDecodeResult.Failure("Video duration is unavailable or invalid.");
            }

            _ = int.TryParse(retriever.ExtractMetadata(MetadataKey.VideoWidth), out var sourceWidth);
            _ = int.TryParse(retriever.ExtractMetadata(MetadataKey.VideoHeight), out var sourceHeight);
            if (sourceWidth <= 0 || sourceHeight <= 0)
            {
                return AnimationMediaDecodeResult.Failure("Video dimensions are unavailable or invalid.");
            }

            var duration = TimeSpan.FromMilliseconds(durationMilliseconds);
            if (duration > request.Limits.MaxDuration)
            {
                return AnimationMediaDecodeResult.Failure(
                    $"Video is longer than the {request.Limits.MaxDuration.TotalSeconds:0}-second import limit.");
            }

            var scale = Math.Min(1, request.Limits.MaxDimension / (double)Math.Max(sourceWidth, sourceHeight));
            var targetWidth = Math.Max(1, (int)Math.Round(sourceWidth * scale));
            var targetHeight = Math.Max(1, (int)Math.Round(sourceHeight * scale));
            var interval = GetEffectiveInterval(duration, request.SampleInterval, request.Limits.MaxFrames);
            var frames = new List<AnimationDecodedFrame>();
            for (var timestamp = TimeSpan.Zero; timestamp < duration && frames.Count < request.Limits.MaxFrames; timestamp += interval)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var decodedPixels = checked((long)(frames.Count + 1) * targetWidth * targetHeight);
                if (decodedPixels > request.Limits.MaxTotalPixels)
                {
                    return AnimationMediaDecodeResult.Failure(
                        $"Decoded video exceeds the {request.Limits.MaxTotalPixels:N0}-pixel work limit.");
                }

                using var bitmap = retriever.GetScaledFrameAtTime(
                    checked((long)(timestamp.TotalMilliseconds * 1000)),
                    Option.Closest,
                    targetWidth,
                    targetHeight);
                if (bitmap is null)
                {
                    return AnimationMediaDecodeResult.Failure($"Video frame {frames.Count + 1} could not be decoded.");
                }

                var next = timestamp + interval < duration ? timestamp + interval : duration;
                frames.Add(new AnimationDecodedFrame(ReadBitmap(bitmap), next - timestamp));
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

    private static TimeSpan GetEffectiveInterval(TimeSpan duration, TimeSpan requested, int maxFrames)
    {
        if (Math.Ceiling(duration.Ticks / (double)requested.Ticks) <= maxFrames)
        {
            return requested;
        }

        return TimeSpan.FromTicks((long)Math.Ceiling(duration.Ticks / (double)maxFrames));
    }

    private static FaceSampleImage ReadBitmap(Bitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var raw = new int[checked(width * height)];
        bitmap.GetPixels(raw, 0, width, 0, 0, width, height);
        var pixels = new FaceSamplePixel[raw.Length];
        for (var index = 0; index < raw.Length; index++)
        {
            var value = raw[index];
            pixels[index] = new FaceSamplePixel(
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)(value & 0xFF),
                (byte)((value >> 24) & 0xFF));
        }

        return new FaceSampleImage(width, height, pixels);
    }
}
#endif
