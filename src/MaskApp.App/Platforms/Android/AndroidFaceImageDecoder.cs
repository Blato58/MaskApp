#if ANDROID
using Android.Graphics;
using MaskApp.App.Infrastructure.Media;
using MaskApp.Core.Features.Faces;

namespace MaskApp.App.Platforms.Android;

public sealed class AndroidFaceImageDecoder : IFaceImageDecoder
{
    public async Task<FaceSampleImage?> DecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var bitmap = await BitmapFactory.DecodeStreamAsync(stream).ConfigureAwait(false);
        if (bitmap is null || bitmap.Width <= 0 || bitmap.Height <= 0)
        {
            return null;
        }

        var width = bitmap.Width;
        var height = bitmap.Height;
        var rawPixels = new int[width * height];
        bitmap.GetPixels(rawPixels, 0, width, 0, 0, width, height);

        var pixels = new FaceSamplePixel[rawPixels.Length];
        for (var i = 0; i < rawPixels.Length; i++)
        {
            var value = rawPixels[i];
            pixels[i] = new FaceSamplePixel(
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)(value & 0xFF),
                (byte)((value >> 24) & 0xFF));
        }

        return new FaceSampleImage(width, height, pixels);
    }
}
#endif
