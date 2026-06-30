namespace MaskApp.Core.Features.Faces;

public static class FacePatternFactory
{
    public static IReadOnlyList<FacePattern> CreateBuiltIns()
    {
        var now = DateTimeOffset.UnixEpoch;
        return
        [
            Create(FaceEmotion.Happy, "Happy", 1, new FaceColor(0xFA, 0xCC, 0x15), DrawHappy, now),
            Create(FaceEmotion.Sad, "Sad", 2, new FaceColor(0x60, 0xA5, 0xFA), DrawSad, now),
            Create(FaceEmotion.Angry, "Angry", 3, new FaceColor(0xEF, 0x44, 0x44), DrawAngry, now),
            Create(FaceEmotion.Surprised, "Surprised", 4, new FaceColor(0x52, 0xE3, 0xFF), DrawSurprised, now),
            Create(FaceEmotion.Meh, "Meh", 5, new FaceColor(0xE5, 0xE7, 0xEB), DrawMeh, now),
            Create(FaceEmotion.Wink, "Wink", 6, new FaceColor(0xF4, 0x72, 0xB6), DrawWink, now)
        ];
    }

    public static FacePattern CreateBlank(string name = "Custom Face", int preferredSlot = 7) =>
        new FacePattern
        {
            Id = $"face-{Guid.NewGuid():N}",
            DisplayName = name,
            Source = FacePatternSource.Custom,
            Emotion = FaceEmotion.Custom,
            PreferredSlot = preferredSlot,
            Pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray()
        }.Normalize();

    private static FacePattern Create(
        FaceEmotion emotion,
        string name,
        int slot,
        FaceColor color,
        Action<Canvas, FaceColor> draw,
        DateTimeOffset timestamp)
    {
        var canvas = new Canvas();
        draw(canvas, color);
        return new FacePattern
        {
            Id = $"built-in-smiley-{emotion.ToString().ToLowerInvariant()}",
            DisplayName = $"{name} Smiley",
            Emotion = emotion,
            Source = FacePatternSource.BuiltIn,
            PreferredSlot = slot,
            IsFavorite = true,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            Pixels = canvas.Pixels
        }.Normalize(timestamp);
    }

    private static void DrawHappy(Canvas canvas, FaceColor color)
    {
        DrawEyes(canvas, color);
        canvas.PlotMany(color, (13, 7), (14, 8), (15, 9), (16, 9), (17, 10), (18, 10), (19, 10), (20, 10), (21, 9), (22, 9), (23, 8), (24, 7));
    }

    private static void DrawSad(Canvas canvas, FaceColor color)
    {
        DrawEyes(canvas, color);
        canvas.PlotMany(color, (13, 10), (14, 9), (15, 8), (16, 8), (17, 7), (18, 7), (19, 7), (20, 7), (21, 8), (22, 8), (23, 9), (24, 10));
    }

    private static void DrawAngry(Canvas canvas, FaceColor color)
    {
        canvas.PlotMany(color, (10, 3), (11, 4), (12, 5), (24, 5), (25, 4), (26, 3));
        canvas.FillRect(10, 5, 3, 2, color);
        canvas.FillRect(24, 5, 3, 2, color);
        canvas.Line(14, 9, 23, 8, color);
    }

    private static void DrawSurprised(Canvas canvas, FaceColor color)
    {
        DrawEyes(canvas, color);
        canvas.PlotMany(color, (17, 7), (18, 7), (19, 7), (20, 7), (16, 8), (21, 8), (16, 9), (21, 9), (17, 10), (18, 10), (19, 10), (20, 10));
    }

    private static void DrawMeh(Canvas canvas, FaceColor color)
    {
        DrawEyes(canvas, color);
        canvas.Line(13, 9, 24, 9, color);
    }

    private static void DrawWink(Canvas canvas, FaceColor color)
    {
        canvas.FillRect(10, 4, 3, 2, color);
        canvas.Line(23, 5, 27, 4, color);
        canvas.PlotMany(color, (13, 8), (14, 9), (15, 10), (16, 10), (17, 10), (18, 10), (19, 10), (20, 10), (21, 10), (22, 9), (23, 8));
    }

    private static void DrawEyes(Canvas canvas, FaceColor color)
    {
        canvas.FillRect(10, 4, 3, 2, color);
        canvas.FillRect(24, 4, 3, 2, color);
    }

    private sealed class Canvas
    {
        public FacePixel[] Pixels { get; } = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();

        public void FillRect(int left, int top, int width, int height, FaceColor color)
        {
            for (var row = top; row < top + height; row++)
            {
                for (var column = left; column < left + width; column++)
                {
                    Plot(column, row, color);
                }
            }
        }

        public void Line(int x0, int y0, int x1, int y1, FaceColor color)
        {
            var dx = Math.Abs(x1 - x0);
            var sx = x0 < x1 ? 1 : -1;
            var dy = -Math.Abs(y1 - y0);
            var sy = y0 < y1 ? 1 : -1;
            var error = dx + dy;

            while (true)
            {
                Plot(x0, y0, color);
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                var doubledError = 2 * error;
                if (doubledError >= dy)
                {
                    error += dy;
                    x0 += sx;
                }

                if (doubledError <= dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        public void PlotMany(FaceColor color, params (int Column, int Row)[] points)
        {
            foreach (var point in points)
            {
                Plot(point.Column, point.Row, color);
            }
        }

        private void Plot(int column, int row, FaceColor color)
        {
            if (column < 0 || column >= FacePattern.Width || row < 0 || row >= FacePattern.Height)
            {
                return;
            }

            Pixels[(row * FacePattern.Width) + column] = new FacePixel(true, color);
        }
    }
}
