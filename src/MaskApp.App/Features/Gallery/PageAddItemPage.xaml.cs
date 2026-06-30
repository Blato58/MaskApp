using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Features.Gallery;

public partial class PageAddItemPage : ContentPage, IQueryAttributable
{
    private readonly PageAddItemViewModel viewModel;
    private string pageId = string.Empty;

    public PageAddItemPage(PageAddItemViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("pageId", out var value))
        {
            pageId = Uri.UnescapeDataString(value?.ToString() ?? string.Empty);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync(pageId);
    }

    private static async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (await viewModel.SaveAsync())
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
