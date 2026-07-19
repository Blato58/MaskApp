using MaskApp.Core.Features.Connect;

namespace MaskApp.App.Controls;

public partial class ConnectionPill : ContentView
{
    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State),
        typeof(BleConnectionState),
        typeof(ConnectionPill),
        BleConnectionState.Disconnected,
        propertyChanged: OnStateChanged);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(ConnectionPill),
        "Disconnected");

    public ConnectionPill()
    {
        InitializeComponent();
    }

    public event EventHandler? Clicked;

    public BleConnectionState State
    {
        get => (BleConnectionState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Icon => State switch
    {
        BleConnectionState.Connected => "✓",
        BleConnectionState.Scanning or BleConnectionState.Connecting => "…",
        BleConnectionState.Failed or BleConnectionState.Unavailable => "!",
        _ => "×"
    };

    public Color AccentColor => State switch
    {
        BleConnectionState.Connected => ResolveSemanticColor("ConnectionColor", "#22D3EE", "#087F8C"),
        BleConnectionState.Failed or BleConnectionState.Unavailable => ResolveSemanticColor("DangerColor", "#F43F5E", "#BE123C"),
        _ => ResolveSemanticColor("WarningColor", "#FBBF24", "#8A5A00")
    };

    public string SemanticDescription => $"Connection status: {Text}. Open Device Picker.";

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ConnectionPill)bindable;
        view.OnPropertyChanged(nameof(Icon));
        view.OnPropertyChanged(nameof(AccentColor));
        view.OnPropertyChanged(nameof(SemanticDescription));
    }

    private void OnTapped(object? sender, TappedEventArgs e) => Clicked?.Invoke(this, EventArgs.Empty);

    private static Color ResolveColor(string key, string fallback) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : Color.FromArgb(fallback);

    private static Color ResolveSemanticColor(string suffix, string darkFallback, string lightFallback)
    {
        var isLight = Application.Current?.RequestedTheme == AppTheme.Light;
        return ResolveColor($"{(isLight ? "Light" : "Dark")}{suffix}", isLight ? lightFallback : darkFallback);
    }
}
