namespace MaskApp.App.Controls;

public partial class ReadinessCardView : ContentView
{
    public static readonly BindableProperty StatusTextProperty = BindableProperty.Create(
        nameof(StatusText), typeof(string), typeof(ReadinessCardView), "NOT READY");

    public static readonly BindableProperty SummaryProperty = BindableProperty.Create(
        nameof(Summary), typeof(string), typeof(ReadinessCardView), string.Empty);

    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(string), typeof(ReadinessCardView), "!");

    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(
        nameof(AccentColor), typeof(Color), typeof(ReadinessCardView), Color.FromArgb("#EF4444"));

    public ReadinessCardView()
    {
        InitializeComponent();
    }

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public string Summary
    {
        get => (string)GetValue(SummaryProperty);
        set => SetValue(SummaryProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }
}
