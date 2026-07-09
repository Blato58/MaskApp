namespace MaskApp.App.Controls;

public partial class BuiltInPreviewView : ContentView
{
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(
        nameof(Source),
        typeof(ImageSource),
        typeof(BuiltInPreviewView));

    public static readonly BindableProperty IsAnimationPlayingProperty = BindableProperty.Create(
        nameof(IsAnimationPlaying),
        typeof(bool),
        typeof(BuiltInPreviewView),
        false);

    public static readonly BindableProperty SemanticDescriptionProperty = BindableProperty.Create(
        nameof(SemanticDescription),
        typeof(string),
        typeof(BuiltInPreviewView),
        string.Empty);

    public BuiltInPreviewView()
    {
        InitializeComponent();
    }

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public bool IsAnimationPlaying
    {
        get => (bool)GetValue(IsAnimationPlayingProperty);
        set => SetValue(IsAnimationPlayingProperty, value);
    }

    public string SemanticDescription
    {
        get => (string)GetValue(SemanticDescriptionProperty);
        set => SetValue(SemanticDescriptionProperty, value);
    }
}
