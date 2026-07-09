using MaskApp.Core.Features.Gallery;
using MaskApp.App.Infrastructure.Accessibility;

namespace MaskApp.App.Features.Gallery;

public partial class PageAddItemPage : ContentPage, IQueryAttributable
{
    private readonly PageAddItemViewModel viewModel;
    private readonly IMotionPreference motionPreference;
    private string pageId = string.Empty;

    public PageAddItemPage(PageAddItemViewModel viewModel, IMotionPreference motionPreference)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.motionPreference = motionPreference;
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
        if (viewModel.AvailableItems.Count > 0)
        {
            Dispatcher.Dispatch(() => AvailableItemsView.ScrollTo(0, -1, ScrollToPosition.Start, animate: false));
        }
    }

    protected override void OnDisappearing()
    {
        viewModel.StopPreviewAnimations();
        base.OnDisappearing();
    }

    private void OnAvailableItemsScrolled(object? sender, ItemsViewScrolledEventArgs e) =>
        viewModel.SetVisibleCandidateRange(e.FirstVisibleItemIndex, e.LastVisibleItemIndex, motionPreference.IsReducedMotionEnabled);

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
