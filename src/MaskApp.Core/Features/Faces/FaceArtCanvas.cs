namespace MaskApp.Core.Features.Faces;

internal sealed class FaceArtCanvas
{
    public FaceArtCanvas()
    {
        Pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
    }

    public FacePixel[] Pixels { get; }

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

    public void FillEllipse(int centerX, int centerY, int radiusX, int radiusY, FaceColor color)
    {
        if (radiusX <= 0 || radiusY <= 0)
        {
            return;
        }

        for (var row = centerY - radiusY; row <= centerY + radiusY; row++)
        {
            var normalizedY = (row - centerY) / (double)radiusY;
            for (var column = centerX - radiusX; column <= centerX + radiusX; column++)
            {
                var normalizedX = (column - centerX) / (double)radiusX;
                if ((normalizedX * normalizedX) + (normalizedY * normalizedY) <= 1)
                {
                    Plot(column, row, color);
                }
            }
        }
    }

    public void FillCircle(int centerX, int centerY, int radius, FaceColor color) =>
        FillEllipse(centerX, centerY, radius, radius, color);

    public void FillPolygon(FaceColor color, params (int X, int Y)[] points)
    {
        if (points.Length < 3)
        {
            return;
        }

        var minX = points.Min(point => point.X);
        var maxX = points.Max(point => point.X);
        var minY = points.Min(point => point.Y);
        var maxY = points.Max(point => point.Y);
        for (var row = minY; row <= maxY; row++)
        {
            for (var column = minX; column <= maxX; column++)
            {
                if (Contains(points, column + 0.5, row + 0.5))
                {
                    Plot(column, row, color);
                }
            }
        }
    }

    public void Line(int x0, int y0, int x1, int y1, FaceColor color, int thickness = 1)
    {
        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var error = dx + dy;

        while (true)
        {
            PlotBrush(x0, y0, color, thickness);
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

    public void PlotMany(FaceColor color, params (int X, int Y)[] points)
    {
        foreach (var (x, y) in points)
        {
            Plot(x, y, color);
        }
    }

    private static bool Contains((int X, int Y)[] points, double x, double y)
    {
        var inside = false;
        for (int current = 0, previous = points.Length - 1; current < points.Length; previous = current++)
        {
            var currentPoint = points[current];
            var previousPoint = points[previous];
            var crosses = currentPoint.Y > y != previousPoint.Y > y &&
                x < ((previousPoint.X - currentPoint.X) * (y - currentPoint.Y) /
                    (previousPoint.Y - currentPoint.Y)) + currentPoint.X;
            if (crosses)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private void PlotBrush(int centerX, int centerY, FaceColor color, int thickness)
    {
        var radius = Math.Max(0, thickness - 1);
        if (radius == 0)
        {
            Plot(centerX, centerY, color);
            return;
        }

        FillCircle(centerX, centerY, radius, color);
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
