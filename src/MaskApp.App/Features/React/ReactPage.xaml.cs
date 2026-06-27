using MaskApp.Core.Features.React;

namespace MaskApp.App.Features.React;

public partial class ReactPage : ContentPage
{
    public ReactPage(ReactViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
