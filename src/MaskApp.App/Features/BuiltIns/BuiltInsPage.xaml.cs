using MaskApp.Core.Features.BuiltIns;

namespace MaskApp.App.Features.BuiltIns;

public partial class BuiltInsPage : ContentPage
{
    public BuiltInsPage(BuiltInsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
