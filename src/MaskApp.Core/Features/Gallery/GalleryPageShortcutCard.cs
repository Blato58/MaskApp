using System.ComponentModel;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryPageShortcutCard : INotifyPropertyChanged
{
    private bool isAnimationPlaying;
    public GalleryPageShortcutCard(
        GalleryPageItemLayout layout,
        GalleryItem item,
        AsyncRelayCommand sendCommand,
        AsyncRelayCommand removeCommand,
        AsyncRelayCommand moveEarlierCommand,
        AsyncRelayCommand moveLaterCommand)
    {
        Layout = layout;
        Item = item;
        SendCommand = sendCommand;
        RemoveCommand = removeCommand;
        MoveEarlierCommand = moveEarlierCommand;
        MoveLaterCommand = moveLaterCommand;
    }

    public GalleryPageItemLayout Layout { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public GalleryItem Item { get; }

    public string SlotId => Layout.SlotId;

    public string Label => string.IsNullOrWhiteSpace(Layout.Label) ? Item.Title : Layout.Label;

    public string Subtitle => Item.TypeLabel;

    public string IconLabel => GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == Layout.IconKey)?.Label ?? "ITEM";

    public string IconAsset => GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == Layout.IconKey)?.PreviewAsset ?? string.Empty;

    public string ColorHex => Layout.ColorHex;

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

    public bool CanSend => Item.CanSend;

    public AsyncRelayCommand SendCommand { get; }

    public AsyncRelayCommand RemoveCommand { get; }

    public AsyncRelayCommand MoveEarlierCommand { get; }

    public AsyncRelayCommand MoveLaterCommand { get; }

    public void SetAnimationPlaying(bool value) =>
        IsAnimationPlaying = PreviewIsAnimated && value;
}
