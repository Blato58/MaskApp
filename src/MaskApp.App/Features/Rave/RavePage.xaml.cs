using MaskApp.Core.Features.Rave;

namespace MaskApp.App.Features.Rave;

public partial class RavePage : ContentPage
{
    public RavePage(RaveViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private static async void OnConnectClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//connect");
    }
}
