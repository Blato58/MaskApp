using System.Windows.Input;

namespace MaskApp.App.Controls;

public partial class SectionHeaderView : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(SectionHeaderView), string.Empty);

    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(SectionHeaderView), string.Empty,
        propertyChanged: OnSubtitleChanged);

    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
        nameof(ActionText), typeof(string), typeof(SectionHeaderView), string.Empty);

    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
        nameof(ActionCommand), typeof(ICommand), typeof(SectionHeaderView));

    public static readonly BindableProperty ShowActionProperty = BindableProperty.Create(
        nameof(ShowAction), typeof(bool), typeof(SectionHeaderView), false);

    public SectionHeaderView()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public bool ShowAction
    {
        get => (bool)GetValue(ShowActionProperty);
        set => SetValue(ShowActionProperty, value);
    }

    public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);

    private static void OnSubtitleChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((SectionHeaderView)bindable).OnPropertyChanged(nameof(HasSubtitle));
}
