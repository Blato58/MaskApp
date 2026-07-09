using MaskApp.App.Infrastructure.Accessibility;
using MaskApp.Core.Features.BuiltIns;

namespace MaskApp.App.Features.BuiltIns;

public partial class BuiltInDetailPage : ContentPage, IQueryAttributable
{
    private readonly BuiltInsViewModel viewModel;
    private readonly IMotionPreference motionPreference;
    private BuiltInAssetType selectedType = BuiltInAssetType.StaticImage;
    private int selectedId;

    public BuiltInDetailPage(BuiltInsViewModel viewModel, IMotionPreference motionPreference)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.motionPreference = motionPreference;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("type", out var typeValue) &&
            Enum.TryParse(typeValue?.ToString(), ignoreCase: true, out BuiltInAssetType parsedType))
        {
            selectedType = parsedType;
        }

        if (query.TryGetValue("id", out var idValue) && int.TryParse(idValue?.ToString(), out var parsedId))
        {
            selectedId = parsedId;
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

    private void OnToggleDiagnosticsClicked(object? sender, EventArgs e)
    {
        DiagnosticsPanel.IsVisible = !DiagnosticsPanel.IsVisible;
        DiagnosticsToggle.Text = DiagnosticsPanel.IsVisible ? "Hide diagnostics" : "Show diagnostics";
    }
}
