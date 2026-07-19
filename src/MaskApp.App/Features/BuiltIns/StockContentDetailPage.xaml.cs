using MaskApp.App.Infrastructure.Accessibility;
using MaskApp.Core.Features.BuiltIns;

namespace MaskApp.App.Features.BuiltIns;

public partial class StockContentDetailPage : ContentPage, IQueryAttributable
{
    private readonly BuiltInsViewModel viewModel;
    private readonly IMotionPreference motionPreference;
    private BuiltInAssetType selectedType = BuiltInAssetType.StaticImage;
    private int selectedId;

    public StockContentDetailPage(BuiltInsViewModel viewModel, IMotionPreference motionPreference)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.motionPreference = motionPreference;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("type", out var typeValue) &&
            Enum.TryParse(typeValue?.ToString(), true, out BuiltInAssetType type))
        {
            selectedType = type;
        }

        if (query.TryGetValue("id", out var idValue) && int.TryParse(idValue?.ToString(), out var id))
        {
            selectedId = id;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.InitializeAsync();
        viewModel.SelectCatalogItem(selectedType, selectedId);
        UpdateAnimationState();
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnDisappearing()
    {
        viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        DetailPreview.IsAnimationPlaying = false;
        base.OnDisappearing();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BuiltInsViewModel.CurrentPreviewIsAnimated) or nameof(BuiltInsViewModel.CurrentPreviewResourceName))
        {
            UpdateAnimationState();
        }
    }

    private void UpdateAnimationState() =>
        DetailPreview.IsAnimationPlaying = viewModel.CurrentPreviewIsAnimated && !motionPreference.IsReducedMotionEnabled;
}
