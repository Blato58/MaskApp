using MaskApp.Core.Features.React;

namespace MaskApp.App.Features.React;

public partial class ReactPage : ContentPage
{
    private readonly ReactViewModel viewModel;

    public ReactPage(ReactViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeArchiveAsync();
    }

    private void OnFilterClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: ReactFilterOption filter })
        {
            viewModel.SelectedFilter = filter;
        }
    }
}
