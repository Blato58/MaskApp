using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class PageAddItemIconCard
{
    public PageAddItemIconCard(GalleryIconOption icon, bool isSelected, AsyncRelayCommand selectCommand)
    {
        Icon = icon;
        IsSelected = isSelected;
        SelectCommand = selectCommand;
    }

    public GalleryIconOption Icon { get; }

    public string IconKey => Icon.IconKey;

    public string Label => Icon.Label;

    public string Pack => Icon.Pack;

    public string ColorHex => Icon.ColorHex;

    public string PreviewAsset => Icon.PreviewAsset;

    public bool IsSelected { get; }

    public string SelectionText => IsSelected ? "Selected" : "Select";

    public AsyncRelayCommand SelectCommand { get; }
}
