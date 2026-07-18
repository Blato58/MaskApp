using MaskApp.Core.Features.Connect;

namespace MaskApp.App.Controls;

public partial class ConnectionStatusView : ContentView
{
    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State),
        typeof(BleConnectionState),
        typeof(ConnectionStatusView),
        BleConnectionState.Disconnected,
        propertyChanged: OnStateChanged);

    public static readonly BindableProperty StateTextProperty = BindableProperty.Create(
        nameof(StateText),
        typeof(string),
        typeof(ConnectionStatusView),
        "Disconnected");

    public static readonly BindableProperty DetailTextProperty = BindableProperty.Create(
        nameof(DetailText),
        typeof(string),
        typeof(ConnectionStatusView),
        string.Empty,
        propertyChanged: OnDetailChanged);

    public ConnectionStatusView()
    {
        InitializeComponent();
    }

    public BleConnectionState State
    {
        get => (BleConnectionState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string StateText
    {
        get => (string)GetValue(StateTextProperty);
        set => SetValue(StateTextProperty, value);
    }

    public string DetailText
    {
        get => (string)GetValue(DetailTextProperty);
        set => SetValue(DetailTextProperty, value);
    }

    public bool HasDetail => !string.IsNullOrWhiteSpace(DetailText);

    public string StatusIcon => State switch
    {
        BleConnectionState.Connected => "✓",
        BleConnectionState.Scanning or BleConnectionState.Connecting => "…",
        BleConnectionState.Failed or BleConnectionState.Unavailable => "!",
        _ => "×"
    };

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((ConnectionStatusView)bindable).OnPropertyChanged(nameof(StatusIcon));

    private static void OnDetailChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((ConnectionStatusView)bindable).OnPropertyChanged(nameof(HasDetail));
}
