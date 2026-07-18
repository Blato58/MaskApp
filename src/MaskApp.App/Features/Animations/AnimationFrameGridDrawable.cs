using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using Microsoft.Maui.Graphics;

namespace MaskApp.App.Features.Animations;

public sealed class AnimationFrameGridDrawable(AnimationStudioViewModel viewModel) : IDrawable
{
    private static readonly (int Left, int Right)[] MaskRowBounds =
    [
        (18,27), (13,32), (11,34), (9,36), (8,37), (7,38), (6,39), (6,39), (5,40), (4,41),
        (4,41), (3,42), (3,42), (3,42), (2,43), (2,43), (2,44), (1,43), (1,44), (1,44),
        (1,44), (1,44), (1,44), (1,44), (1,44), (1,44), (1,44), (1,44), (1,44), (1,44),
        (1,44), (1,44), (1,44), (1,44), (2,43), (2,43), (2,43), (2,43), (3,42), (3,42),
        (4,41), (4,41), (4,41), (5,40), (5,40), (6,39), (6,39), (7,38), (8,37), (9,36),
        (9,36), (10,35), (11,34), (12,33), (13,32), (15,30), (17,28), (21,24)
    ];

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

        if (viewModel.GuidesEnabled)
        {
            DrawMaskGuides(canvas);
        }

        if (viewModel.SelectionBounds is { } selection)
        {
            canvas.StrokeColor = Color.FromArgb("#F59E0B");
            canvas.StrokeSize = 2;
            canvas.StrokeDashPattern = [4, 3];
            canvas.DrawRectangle(
                LastGridBounds.X + (selection.Left * CellSize),
                LastGridBounds.Y + (selection.Top * CellSize),
                (selection.Right - selection.Left + 1) * CellSize,
                (selection.Bottom - selection.Top + 1) * CellSize);
            canvas.StrokeDashPattern = null;
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

    private void DrawMaskGuides(ICanvas canvas)
    {
        canvas.SaveState();
        canvas.StrokeColor = Color.FromArgb("#F7F7F8");
        canvas.StrokeSize = Math.Max(1.5f, CellSize * 0.28f);
        canvas.Alpha = 0.9f;

        var outline = new PathF();
        outline.MoveTo(CellX(MaskRowBounds[0].Left), CellY(0.5f));
        for (var row = 1; row < MaskRowBounds.Length; row++)
        {
            outline.LineTo(CellX(MaskRowBounds[row].Left), CellY(row + 0.5f));
        }

        for (var row = MaskRowBounds.Length - 1; row >= 0; row--)
        {
            outline.LineTo(CellX(MaskRowBounds[row].Right + 1), CellY(row + 0.5f));
        }

        outline.Close();
        canvas.DrawPath(outline);
        DrawEyeGuide(canvas, [(5, 15), (6, 17), (7, 17), (9, 14)], 16);
        DrawEyeGuide(canvas, [(30, 40), (28, 38), (28, 38), (31, 36)], 16);
        canvas.RestoreState();
    }

    private void DrawEyeGuide(ICanvas canvas, IReadOnlyList<(int Left, int Right)> rows, int firstRow)
    {
        var path = new PathF();
        path.MoveTo(CellX(rows[0].Left), CellY(firstRow));
        for (var index = 0; index < rows.Count; index++)
        {
            path.LineTo(CellX(rows[index].Left), CellY(firstRow + index + 0.5f));
        }

        path.LineTo(CellX(rows[^1].Left), CellY(firstRow + rows.Count));
        path.LineTo(CellX(rows[^1].Right + 1), CellY(firstRow + rows.Count));
        for (var index = rows.Count - 1; index >= 0; index--)
        {
            path.LineTo(CellX(rows[index].Right + 1), CellY(firstRow + index + 0.5f));
        }

        path.LineTo(CellX(rows[0].Right + 1), CellY(firstRow));
        path.Close();
        canvas.DrawPath(path);
    }

    private float CellX(float column) => LastGridBounds.X + (column * CellSize);

    private float CellY(float row) => LastGridBounds.Y + (row * CellSize);
}
