namespace MaskApp.App.Controls;

public partial class PreparationProgress : ContentView
{
    public static readonly BindableProperty StatusTextProperty = BindableProperty.Create(nameof(StatusText), typeof(string), typeof(PreparationProgress), "Preparing");
    public static readonly BindableProperty ProgressTextProperty = BindableProperty.Create(nameof(ProgressText), typeof(string), typeof(PreparationProgress), string.Empty);
    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(double), typeof(PreparationProgress), 0d);
    public PreparationProgress() => InitializeComponent();
    public string StatusText { get => (string)GetValue(StatusTextProperty); set => SetValue(StatusTextProperty, value); }
    public string ProgressText { get => (string)GetValue(ProgressTextProperty); set => SetValue(ProgressTextProperty, value); }
    public double Progress { get => (double)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
}
