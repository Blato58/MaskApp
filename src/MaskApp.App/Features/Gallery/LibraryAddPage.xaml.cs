using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Features.Gallery;

public partial class LibraryAddPage : ContentPage
{
    private readonly GalleryViewModel viewModel;

    public LibraryAddPage(GalleryViewModel viewModel)
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

    private static async void OnDoneClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
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
