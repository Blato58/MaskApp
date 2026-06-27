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
}
