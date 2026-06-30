#if IOS
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using MaskApp.App.Infrastructure.Media;
using MaskApp.Core.Features.Faces;
using UIKit;

namespace MaskApp.App.Platforms.iOS;

public sealed class IosFaceImageDecoder : IFaceImageDecoder
{
    public async Task<FaceSampleImage?> DecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);
        using var data = NSData.FromArray(memory.ToArray());
        using var image = UIImage.LoadFromData(data);
        var cgImage = image?.CGImage;
        if (cgImage is null || cgImage.Width <= 0 || cgImage.Height <= 0)
        {
            return null;
        }

        var width = (int)cgImage.Width;
        var height = (int)cgImage.Height;
        var bytesPerRow = width * 4;
        var raw = new byte[height * bytesPerRow];
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
            context.DrawImage(new CGRect(0, 0, width, height), cgImage);
        }
        finally
        {
            handle.Free();
        }

        var pixels = new FaceSamplePixel[width * height];
        var sourceOffset = 0;
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new FaceSamplePixel(
                raw[sourceOffset],
                raw[sourceOffset + 1],
                raw[sourceOffset + 2],
                raw[sourceOffset + 3]);
            sourceOffset += 4;
        }

        return new FaceSampleImage(width, height, pixels);
    }
}
#endif
