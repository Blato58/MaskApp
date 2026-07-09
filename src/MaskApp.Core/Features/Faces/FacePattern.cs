namespace MaskApp.Core.Features.Faces;

public sealed record FacePattern
{
    public const int Width = 46;
    public const int Height = 58;
    public const int PixelCount = Width * Height;
    internal const int LegacyWidth = 36;
    internal const int LegacyHeight = 12;
    internal const int LegacyPixelCount = LegacyWidth * LegacyHeight;
    public const int MinSlot = 1;
    public const int MaxSlot = 20;

    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = "Custom Face";

    public FaceEmotion Emotion { get; init; } = FaceEmotion.Custom;

    public FacePatternSource Source { get; init; } = FacePatternSource.Custom;

    public int PreferredSlot { get; init; } = 7;

    public bool IsFavorite { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastUploadedAt { get; init; }

    public string LastUploadStatus { get; init; } = string.Empty;

    public FacePixel[] Pixels { get; init; } = [];

    public bool IsBuiltIn => Source == FacePatternSource.BuiltIn;

    public string SourceLabel => Source switch
    {
        FacePatternSource.BuiltIn => "Built-in pixel face",
        FacePatternSource.ImportedPhoto => "Imported photo",
        FacePatternSource.CapturedPhoto => "Camera photo",
        _ => "Custom drawing"
    };

    public string AccentColorHex
    {
        get
        {
            var color = Pixels.FirstOrDefault(pixel => pixel.IsLit).Color;
            return color == default ? "#FACC15" : color.Hex;
        }
    }

    public FacePattern Normalize(DateTimeOffset? timestamp = null)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        var pixels = Pixels.Length == LegacyPixelCount
            ? ResizePixels(Pixels, LegacyWidth, LegacyHeight)
            : NormalizePixels(Pixels);

        var id = string.IsNullOrWhiteSpace(Id)
            ? $"face-{Guid.NewGuid():N}"
            : Id.Trim();
        var displayName = string.IsNullOrWhiteSpace(DisplayName)
            ? "Custom Face"
            : DisplayName.Trim();

        return this with
        {
            Id = id,
            DisplayName = displayName,
            PreferredSlot = Math.Clamp(PreferredSlot, MinSlot, MaxSlot),
            CreatedAt = CreatedAt == default ? now : CreatedAt,
            UpdatedAt = UpdatedAt == default ? now : UpdatedAt,
            Pixels = pixels
        };
    }

    public FacePixel GetPixel(int column, int row)
    {
        if (column < 0 || column >= Width || row < 0 || row >= Height)
        {
            return FacePixel.Off;
        }

        var index = (row * Width) + column;
        return index < Pixels.Length ? Pixels[index].Normalize() : FacePixel.Off;
    }

    public FacePattern WithPixel(int column, int row, FacePixel pixel)
    {
        if (column < 0 || column >= Width || row < 0 || row >= Height)
        {
            return this;
        }

        var normalized = Normalize();
        var pixels = normalized.Pixels.ToArray();
        pixels[(row * Width) + column] = pixel.Normalize();
        return normalized with
        {
            Pixels = pixels,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    internal static FacePixel[] ResizePixels(FacePixel[] source, int sourceWidth, int sourceHeight)
    {
        if (sourceWidth <= 0 || sourceHeight <= 0 || source.Length != sourceWidth * sourceHeight)
        {
            throw new ArgumentException("Source pixels must match the supplied canvas dimensions.", nameof(source));
        }

        var target = Enumerable.Repeat(FacePixel.Off, PixelCount).ToArray();
        var scale = Math.Min(Width / (double)sourceWidth, Height / (double)sourceHeight);
        var scaledWidth = Math.Max(1, (int)Math.Round(sourceWidth * scale));
        var scaledHeight = Math.Max(1, (int)Math.Round(sourceHeight * scale));
        var left = (Width - scaledWidth) / 2;
        var top = (Height - scaledHeight) / 2;

        for (var row = 0; row < scaledHeight; row++)
        {
            var sourceRow = Math.Clamp(
                (int)Math.Floor((row + 0.5) * sourceHeight / scaledHeight),
                0,
                sourceHeight - 1);
            for (var column = 0; column < scaledWidth; column++)
            {
                var sourceColumn = Math.Clamp(
                    (int)Math.Floor((column + 0.5) * sourceWidth / scaledWidth),
                    0,
                    sourceWidth - 1);
                target[((top + row) * Width) + left + column] =
                    source[(sourceRow * sourceWidth) + sourceColumn].Normalize();
            }
        }

        return target;
    }

    private static FacePixel[] NormalizePixels(FacePixel[] source)
    {
        var pixels = new FacePixel[PixelCount];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = i < source.Length ? source[i].Normalize() : FacePixel.Off;
        }

        return pixels;
    }
}
