using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryAvailableItemCard
{
    public GalleryAvailableItemCard(GalleryItem item, AsyncRelayCommand addCommand)
    {
        Item = item;
        AddCommand = addCommand;
    }

    public GalleryItem Item { get; }

    public string Title => Item.Title;

    public string Subtitle => $"{Item.TypeLabel} / {Item.GroupName}";

    public string ColorHex => Item.ColorHex;

    public string IconLabel => GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == Item.IconKey)?.Label ?? "ITEM";

    public string PreviewResourceName => Item.PreviewResourceName;

    public string PreviewBadgeText => Item.PreviewBadgeText;

    public bool PreviewIsAnimated => Item.PreviewIsAnimated;

    public bool HasPreview => Item.HasPreview;

    public AsyncRelayCommand AddCommand { get; }
}
