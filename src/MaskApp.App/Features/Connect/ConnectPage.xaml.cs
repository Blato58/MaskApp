using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.App.Features.Connect;

public partial class ConnectPage : ContentPage
{
    public ConnectPage(ConnectViewModel viewModel, MaskControlViewModel maskControlViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        MaskControls.BindingContext = maskControlViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ConnectViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private void OnToggleAdvancedClicked(object? sender, EventArgs e)
    {
        AdvancedPanel.IsVisible = !AdvancedPanel.IsVisible;
        AdvancedToggle.Text = AdvancedPanel.IsVisible ? "Hide advanced status" : "Show advanced status";
    }
}
