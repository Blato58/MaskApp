using MaskApp.App.Infrastructure.Accessibility;
using MaskApp.Core.Features.BuiltIns;

namespace MaskApp.App.Features.BuiltIns;

public partial class BuiltInsPage : ContentPage
{
    private readonly BuiltInsViewModel viewModel;
    private readonly IMotionPreference motionPreference;

    public BuiltInsPage(BuiltInsViewModel viewModel, IMotionPreference motionPreference)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.motionPreference = motionPreference;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
        if (viewModel.CatalogItems.Count > 0)
        {
            Dispatcher.Dispatch(() => CatalogView.ScrollTo(0, -1, ScrollToPosition.Start, animate: false));
        }
    }

    protected override void OnDisappearing()
    {
        viewModel.StopCatalogAnimations();
        base.OnDisappearing();
    }

    private void OnCatalogScrolled(object? sender, ItemsViewScrolledEventArgs e) =>
        viewModel.SetCatalogVisibleRange(e.FirstVisibleItemIndex, e.LastVisibleItemIndex, motionPreference.IsReducedMotionEnabled);

    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: BuiltInAssetListItem item })
        {
            return;
        }

        await Shell.Current.GoToAsync($"built-in-detail?type={item.Type}&id={item.Id}");
    }
}
