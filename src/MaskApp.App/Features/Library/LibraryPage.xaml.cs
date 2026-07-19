using MaskApp.App.Controls;
using MaskApp.App.Infrastructure.Accessibility;
using MaskApp.App.Resources.Strings;
using MaskApp.Core.Features.Experience;
using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Features.Library;

public partial class LibraryPage : ContentPage
{
    private readonly GalleryViewModel viewModel;
    private readonly IMotionPreference motionPreference;

    public LibraryPage(GalleryViewModel viewModel, IMotionPreference motionPreference)
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
    }

    protected override void OnDisappearing()
    {
        viewModel.StopPreviewAnimations();
        base.OnDisappearing();
    }

    private void OnSearchDone(object? sender, EventArgs e) =>
        (sender as SearchBar)?.Unfocus();

    private async void OnStockClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(AppRoutes.StockCatalog);

    private async void OnCreateClicked(object? sender, EventArgs e)
    {
        var choice = await DisplayActionSheetAsync(
            AppText.Create,
            AppText.Cancel,
            null,
            AppText.Text,
            AppText.Get("Face"),
            AppText.Get("Animation"),
            AppText.Get("Scene"),
            AppText.ImportMaskPack);
        await NavigateCreateChoiceAsync(choice);
    }

    private async void OnActionsRequested(object? sender, EventArgs e)
    {
        if (sender is not LibraryRow { Item: { } card })
        {
            return;
        }

        var actions = new List<string>();
        if (card.CanSend)
        {
            actions.Add(AppText.Get("SendNow"));
        }

        if (card.CanManage)
        {
            actions.Add(AppText.Get("OpenDetails"));
        }

        var choice = await DisplayActionSheetAsync(card.Title, AppText.Cancel, null, [.. actions]);
        if (choice == AppText.Get("SendNow"))
        {
            card.SendCommand.Execute(null);
        }
        else if (choice == AppText.Get("OpenDetails"))
        {
            await OpenEditorAsync(card.Item);
        }
    }

    private static async Task NavigateCreateChoiceAsync(string? choice)
    {
        var route = choice switch
        {
            var value when value == AppText.Text => AppRoutes.TextStudio,
            var value when value == AppText.Get("Face") => AppRoutes.FaceStudio,
            var value when value == AppText.Get("Animation") => AppRoutes.AnimationStudio,
            var value when value == AppText.Get("Scene") => AppRoutes.SceneEditor,
            var value when value == AppText.ImportMaskPack => AppRoutes.ForMaskPackTransfer("import"),
            _ => string.Empty
        };
        if (!string.IsNullOrEmpty(route))
        {
            await Shell.Current.GoToAsync(route);
        }
    }

    private static async Task OpenEditorAsync(GalleryItem item)
    {
        if (item.TextPreset is not null)
        {
            await Shell.Current.GoToAsync(AppRoutes.ForTextStudio(item.TextPreset.Id.Value));
        }
        else if (item.BuiltInAssetRecord is { } stock)
        {
            await Shell.Current.GoToAsync(AppRoutes.ForStockDetail(stock.Type.ToString(), stock.Id));
        }
        else if (item.FacePattern is not null)
        {
            await Shell.Current.GoToAsync(AppRoutes.ForFaceStudio(item.FacePattern.Id));
        }
        else if (item.AnimationProject is not null)
        {
            await Shell.Current.GoToAsync(AppRoutes.ForAnimationStudio(item.AnimationProject.Id));
        }
        else if (item.Scene is not null)
        {
            await Shell.Current.GoToAsync(AppRoutes.ForSceneEditor(item.Scene.Id));
        }
    }
}
