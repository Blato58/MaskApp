using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class PageAddItemCandidateCard
{
    public PageAddItemCandidateCard(GalleryItem item, bool isSelected, AsyncRelayCommand selectCommand)
    {
        Item = item;
        IsSelected = isSelected;
        SelectCommand = selectCommand;
    }

    public GalleryItem Item { get; }

    public string Title => Item.Title;

    public string Subtitle => $"{Item.TypeLabel} / {Item.GroupName}";

    public string ColorHex => Item.ColorHex;

    public string IconLabel => GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == Item.IconKey)?.Label ?? "ITEM";

    public string IconAsset => GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == Item.IconKey)?.PreviewAsset ?? string.Empty;

    public bool IsSelected { get; }

    public string SelectionText => IsSelected ? "Selected" : "Choose";

    public AsyncRelayCommand SelectCommand { get; }
}
