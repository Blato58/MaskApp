using System.Windows.Input;
using MaskApp.App.Resources.Strings;

namespace MaskApp.App.Controls;

public partial class SafetyBar : ContentView
{
    public static readonly BindableProperty StopCommandProperty = BindableProperty.Create(nameof(StopCommand), typeof(ICommand), typeof(SafetyBar));
    public static readonly BindableProperty BlackoutCommandProperty = BindableProperty.Create(nameof(BlackoutCommand), typeof(ICommand), typeof(SafetyBar));
    public static readonly BindableProperty IsTransportAvailableProperty = BindableProperty.Create(nameof(IsTransportAvailable), typeof(bool), typeof(SafetyBar), false);
    public static readonly BindableProperty IsStageProperty = BindableProperty.Create(nameof(IsStage), typeof(bool), typeof(SafetyBar), false, propertyChanged: OnIsStageChanged);
    public static readonly BindableProperty StopTextProperty = BindableProperty.Create(nameof(StopText), typeof(string), typeof(SafetyBar), AppText.Stop);
    public static readonly BindableProperty BlackoutTextProperty = BindableProperty.Create(nameof(BlackoutText), typeof(string), typeof(SafetyBar), AppText.Blackout);

    public SafetyBar() => InitializeComponent();

    public ICommand? StopCommand { get => (ICommand?)GetValue(StopCommandProperty); set => SetValue(StopCommandProperty, value); }
    public ICommand? BlackoutCommand { get => (ICommand?)GetValue(BlackoutCommandProperty); set => SetValue(BlackoutCommandProperty, value); }
    public bool IsTransportAvailable { get => (bool)GetValue(IsTransportAvailableProperty); set => SetValue(IsTransportAvailableProperty, value); }
    public bool IsStage { get => (bool)GetValue(IsStageProperty); set => SetValue(IsStageProperty, value); }
    public string StopText { get => (string)GetValue(StopTextProperty); set => SetValue(StopTextProperty, value); }
    public string BlackoutText { get => (string)GetValue(BlackoutTextProperty); set => SetValue(BlackoutTextProperty, value); }
    public double MinimumTargetHeight => IsStage ? 72 : 48;

    private static void OnIsStageChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((SafetyBar)bindable).OnPropertyChanged(nameof(MinimumTargetHeight));
}
