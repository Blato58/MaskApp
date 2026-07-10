using MaskApp.Core.Features.Faces;
using Microsoft.Maui.Graphics;

namespace MaskApp.App.Controls;

public partial class FacePatternPreviewView : ContentView
{
    public static readonly BindableProperty PatternProperty = BindableProperty.Create(
        nameof(Pattern),
        typeof(FacePattern),
        typeof(FacePatternPreviewView),
        propertyChanged: OnPatternChanged);

    public static readonly BindableProperty SemanticDescriptionProperty = BindableProperty.Create(
        nameof(SemanticDescription),
        typeof(string),
        typeof(FacePatternPreviewView),
        string.Empty);

    private readonly FacePatternPreviewDrawable drawable = new();

    public FacePatternPreviewView()
    {
        InitializeComponent();
        PreviewCanvas.Drawable = drawable;
    }

    public FacePattern? Pattern
    {
        get => (FacePattern?)GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    public string SemanticDescription
    {
        get => (string)GetValue(SemanticDescriptionProperty);
        set => SetValue(SemanticDescriptionProperty, value);
    }

    private static void OnPatternChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var preview = (FacePatternPreviewView)bindable;
        preview.drawable.Pattern = (FacePattern?)newValue;
        preview.PreviewCanvas.Invalidate();
    }

    private sealed class FacePatternPreviewDrawable : IDrawable
    {
        public FacePattern? Pattern { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Color.FromArgb("#05070D");
            canvas.FillRectangle(dirtyRect);

            if (Pattern is null)
            {
                return;
            }

            var pixelSize = MathF.Min(
                dirtyRect.Width / FacePattern.Width,
                dirtyRect.Height / FacePattern.Height);
            if (pixelSize <= 0)
            {
                return;
            }

            var artworkWidth = pixelSize * FacePattern.Width;
            var artworkHeight = pixelSize * FacePattern.Height;
            var left = dirtyRect.X + ((dirtyRect.Width - artworkWidth) / 2);
            var top = dirtyRect.Y + ((dirtyRect.Height - artworkHeight) / 2);

            canvas.Antialias = false;
            for (var row = 0; row < FacePattern.Height; row++)
            {
                for (var column = 0; column < FacePattern.Width; column++)
                {
                    var pixel = Pattern.GetPixel(column, row);
                    if (!pixel.IsLit)
                    {
                        continue;
                    }

                    canvas.FillColor = Color.FromArgb(pixel.Color.Hex);
                    canvas.FillRectangle(
                        left + (column * pixelSize),
                        top + (row * pixelSize),
                        MathF.Ceiling(pixelSize),
                        MathF.Ceiling(pixelSize));
                }
            }
        }
    }
}
