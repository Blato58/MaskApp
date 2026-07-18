using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using Microsoft.Maui.Graphics;

namespace MaskApp.App.Features.Animations;

public sealed class AnimationFrameGridDrawable(AnimationStudioViewModel viewModel) : IDrawable
{
    public RectF LastGridBounds { get; private set; }

    public float CellSize { get; private set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromArgb("#05070D");
        canvas.FillRectangle(dirtyRect);
        CellSize = MathF.Floor(MathF.Min(
            dirtyRect.Width / FacePattern.Width,
            dirtyRect.Height / FacePattern.Height));
        if (CellSize <= 0)
        {
            return;
        }

        var width = CellSize * FacePattern.Width;
        var height = CellSize * FacePattern.Height;
        LastGridBounds = new RectF(
            dirtyRect.X + ((dirtyRect.Width - width) / 2),
            dirtyRect.Y + ((dirtyRect.Height - height) / 2),
            width,
            height);

        var onion = viewModel.OnionSkinPattern;
        if (onion is not null)
        {
            canvas.Alpha = 0.24f;
            DrawPattern(canvas, onion.Normalize());
            canvas.Alpha = 1;
        }

        foreach (var cell in viewModel.PreviewCells.Where(cell => cell.IsLit))
        {
            canvas.FillColor = Color.FromArgb(cell.FillColor);
            canvas.FillRectangle(
                LastGridBounds.X + (cell.Column * CellSize) + 1,
                LastGridBounds.Y + (cell.Row * CellSize) + 1,
                Math.Max(1, CellSize - 2),
                Math.Max(1, CellSize - 2));
        }

        canvas.StrokeColor = Color.FromArgb("#30343F");
        canvas.StrokeSize = 1;
        for (var column = 0; column <= FacePattern.Width; column++)
        {
            var x = LastGridBounds.X + (column * CellSize);
            canvas.DrawLine(x, LastGridBounds.Y, x, LastGridBounds.Bottom);
        }

        for (var row = 0; row <= FacePattern.Height; row++)
        {
            var y = LastGridBounds.Y + (row * CellSize);
            canvas.DrawLine(LastGridBounds.X, y, LastGridBounds.Right, y);
        }
    }

    public bool TryGetCell(PointF point, out int column, out int row)
    {
        column = -1;
        row = -1;
        if (CellSize <= 0 || !LastGridBounds.Contains(point))
        {
            return false;
        }

        column = Math.Clamp((int)((point.X - LastGridBounds.X) / CellSize), 0, FacePattern.Width - 1);
        row = Math.Clamp((int)((point.Y - LastGridBounds.Y) / CellSize), 0, FacePattern.Height - 1);
        return true;
    }

    private void DrawPattern(ICanvas canvas, FacePattern pattern)
    {
        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width; column++)
            {
                var pixel = pattern.GetPixel(column, row);
                if (!pixel.IsLit)
                {
                    continue;
                }

                canvas.FillColor = Color.FromArgb(pixel.Color.Hex);
                canvas.FillRectangle(
                    LastGridBounds.X + (column * CellSize) + 1,
                    LastGridBounds.Y + (row * CellSize) + 1,
                    Math.Max(1, CellSize - 2),
                    Math.Max(1, CellSize - 2));
            }
        }
    }
}
