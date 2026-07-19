using MaskApp.Core.Features.Delivery;

namespace MaskApp.App.Controls;

public partial class DeliveryStatus : ContentView
{
    public static readonly BindableProperty StateProperty = BindableProperty.Create(nameof(State), typeof(DeliveryState), typeof(DeliveryStatus), DeliveryState.Idle, propertyChanged: OnStateChanged);
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(DeliveryStatus), "Idle");
    public static readonly BindableProperty DetailProperty = BindableProperty.Create(nameof(Detail), typeof(string), typeof(DeliveryStatus), string.Empty);

    public DeliveryStatus() => InitializeComponent();

    public DeliveryState State { get => (DeliveryState)GetValue(StateProperty); set => SetValue(StateProperty, value); }
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string Detail { get => (string)GetValue(DetailProperty); set => SetValue(DetailProperty, value); }

    public string Icon => State switch
    {
        DeliveryState.Confirmed => "✓",
        DeliveryState.Written => "→",
        DeliveryState.Sending or DeliveryState.Preparing => "…",
        DeliveryState.Failed => "!",
        DeliveryState.Unknown => "?",
        _ => "•"
    };

    public Color AccentColor => State switch
    {
        DeliveryState.Confirmed => ThemeColor("#34D399", "#087A55"),
        DeliveryState.Written or DeliveryState.Preparing => ThemeColor("#FBBF24", "#8A5A00"),
        DeliveryState.Sending => ThemeColor("#22D3EE", "#087F8C"),
        DeliveryState.Failed => ThemeColor("#F43F5E", "#BE123C"),
        _ => ThemeColor("#A7ADBA", "#5B6270")
    };

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (DeliveryStatus)bindable;
        view.OnPropertyChanged(nameof(Icon));
        view.OnPropertyChanged(nameof(AccentColor));
    }

    private static Color ThemeColor(string dark, string light) =>
        Color.FromArgb(Application.Current?.RequestedTheme == AppTheme.Light ? light : dark);
}
