using System.ComponentModel;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class PageAddItemCandidateCard : INotifyPropertyChanged
{
    private bool isAnimationPlaying;
    public PageAddItemCandidateCard(GalleryItem item, bool isSelected, AsyncRelayCommand selectCommand)
    {
        Item = item;
        IsSelected = isSelected;
        SelectCommand = selectCommand;
    }

    public GalleryItem Item { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Item.Title;

    public string Subtitle => $"{Item.TypeLabel} / {Item.GroupName}";

    public string ColorHex => Item.ColorHex;

    public string IconLabel => GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == Item.IconKey)?.Label ?? "ITEM";

    public string IconAsset => GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == Item.IconKey)?.PreviewAsset ?? string.Empty;

    public string PreviewResourceName => Item.PreviewResourceName;

    public string PreviewBadgeText => Item.PreviewBadgeText;

    public bool PreviewIsAnimated => Item.PreviewIsAnimated;

    public bool IsAnimationPlaying
    {
        get => isAnimationPlaying;
        private set
        {
            if (isAnimationPlaying == value)
            {
                return;
            }

            isAnimationPlaying = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAnimationPlaying)));
        }
    }

    public bool HasPreview => Item.HasPreview;

    public bool IsSelected { get; }

    public string SelectionText => IsSelected ? "Selected" : "Choose";

    public AsyncRelayCommand SelectCommand { get; }

    public void SetAnimationPlaying(bool value) =>
        IsAnimationPlaying = PreviewIsAnimated && value;
}
