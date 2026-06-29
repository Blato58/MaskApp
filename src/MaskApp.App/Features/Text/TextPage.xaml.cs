using MaskApp.Core.Features.Text;

namespace MaskApp.App.Features.Text;

public partial class TextPage : ContentPage
{
    public TextPage(TextUploadViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TextUploadViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private void OnColorClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not TextUploadViewModel viewModel ||
            sender is not Button { CommandParameter: string colorName })
        {
            return;
        }

        var color = viewModel.TextColorOptions.FirstOrDefault(option => option.Name == colorName);
        if (color is not null)
        {
            viewModel.SelectColor(color);
        }
    }
}
