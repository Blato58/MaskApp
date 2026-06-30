using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Features.Gallery;

public partial class GalleryPage : ContentPage
{
    private readonly GalleryViewModel viewModel;

    public GalleryPage(GalleryViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
    }

    private static async void OnManageClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: GalleryItem item } || !item.CanManage)
        {
            return;
        }

        if (string.Equals(item.ManageTarget, "text", StringComparison.Ordinal))
        {
            await Shell.Current.GoToAsync("text");
            return;
        }

        if (string.Equals(item.ManageTarget, "builtins", StringComparison.Ordinal))
        {
            await Shell.Current.GoToAsync("builtins");
        }
    }

    private static async void OnAddOptionClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: GalleryAddOption option } || !option.IsAvailable)
        {
            return;
        }

        switch (option.Kind)
        {
            case GalleryAddOptionKind.NewTextPreset:
            case GalleryAddOptionKind.EditTextPresets:
                await Shell.Current.GoToAsync("text");
                break;
            case GalleryAddOptionKind.ScanBuiltInStaticFace:
            case GalleryAddOptionKind.ScanBuiltInAnimation:
                await Shell.Current.GoToAsync("builtins");
                break;
        }
    }
}
