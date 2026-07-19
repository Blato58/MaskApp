using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Features.Live;

public partial class ContentPickerPage : ContentPage
{
    public ContentPickerPage(PagesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnDoneClicked(object? sender, EventArgs e) =>
        await Navigation.PopModalAsync();
}
