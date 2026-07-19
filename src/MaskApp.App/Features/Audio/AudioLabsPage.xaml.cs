using MaskApp.App.Resources.Strings;

namespace MaskApp.App.Features.Audio;

public partial class AudioLabsPage : ContentPage
{
    private readonly AudioLabsViewModel viewModel;

    public AudioLabsPage(AudioLabsViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        viewModel.Activate();
        await viewModel.InitializeAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        try
        {
            await viewModel.StopMicrophoneForNavigationAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Audio Labs navigation stop failed: {exception}");
        }
        finally
        {
            viewModel.Deactivate();
        }
    }

    private async void OnConfirmPassedClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(
            AppText.Get("Ui369"),
            AppText.Get("Ui464"),
            AppText.Get("Ui370"),
            AppText.Get("Ui056"));
        if (confirmed)
        {
            await viewModel.ConfirmPhysicalResultAsync(passed: true);
        }
    }

    private async void OnConfirmFailedClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(
            AppText.Get("Ui371"),
            AppText.Get("Ui372"),
            AppText.Get("Ui373"),
            AppText.Get("Ui056"));
        if (confirmed)
        {
            await viewModel.ConfirmPhysicalResultAsync(passed: false);
        }
    }
}
