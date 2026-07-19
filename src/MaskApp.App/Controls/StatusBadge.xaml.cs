namespace MaskApp.App.Controls;

public partial class StatusBadge : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(StatusBadge), string.Empty);
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(StatusBadge), "•");
    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(StatusBadge), Color.FromArgb("#A7ADBA"));

    public StatusBadge() => InitializeComponent();

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public Color AccentColor { get => (Color)GetValue(AccentColorProperty); set => SetValue(AccentColorProperty, value); }
}
