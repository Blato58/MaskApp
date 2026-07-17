using System.ComponentModel;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryPageShortcutCard : INotifyPropertyChanged
{
    private bool isAnimationPlaying;
    public GalleryPageShortcutCard(
        GalleryPageItemLayout layout,
        GalleryItem item,
        AsyncRelayCommand sendCommand,
        AsyncRelayCommand prepareCommand,
        AsyncRelayCommand removeCommand,
        AsyncRelayCommand moveEarlierCommand,
        AsyncRelayCommand moveLaterCommand,
        bool isFastSlotCapable,
        bool isFastSlotPrepared)
    {
        Layout = layout;
        Item = item;
        SendCommand = sendCommand;
        PrepareCommand = prepareCommand;
        RemoveCommand = removeCommand;
        MoveEarlierCommand = moveEarlierCommand;
        MoveLaterCommand = moveLaterCommand;
        IsFastSlotCapable = isFastSlotCapable;
        IsFastSlotPrepared = isFastSlotPrepared;
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

    public FacePattern? FacePattern => Item.FacePattern;

    public bool HasFacePreview => Item.HasFacePreview;

    public bool HasAnyPreview => Item.HasAnyPreview;

    public bool CanSend => Item.CanSend;

    public bool IsFastSlotCapable { get; }

    public bool IsFastSlotPrepared { get; }

    public string FastSlotStatusText => Item.AppAnimation is not null
        ? IsFastSlotPrepared
            ? $"{Item.AppAnimation.Frames.Count} DIY slots · rapid PLAY"
            : $"{Item.AppAnimation.Frames.Count} DIY slots · prepare once"
        : (IsFastSlotCapable, IsFastSlotPrepared, Layout.FastMaskSlot) switch
        {
            (true, true, int slot) => $"Fast slot {slot} · instant",
            (true, false, _) when Item.Type == GalleryItemType.TextPreset => "Fast slot not prepared · static",
            (true, false, _) => "Fast slot not prepared",
            _ when Item.Type is GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation => "Built in · instant",
            _ => "Direct action"
        };

    public string FastSlotStatusColorHex => IsFastSlotPrepared
        ? "#22C55E"
        : IsFastSlotCapable
            ? "#FACC15"
            : "#94A3B8";

    public string UseActionText => IsFastSlotCapable && !IsFastSlotPrepared ? "Prepare + show" : "Show";

    public string PrepareActionText => Item.AppAnimation is not null
        ? IsFastSlotPrepared ? "Refresh animation" : "Prepare animation"
        : IsFastSlotPrepared ? "Refresh slot" : "Prepare slot";

    public AsyncRelayCommand SendCommand { get; }

    public AsyncRelayCommand PrepareCommand { get; }

    public AsyncRelayCommand RemoveCommand { get; }

    public AsyncRelayCommand MoveEarlierCommand { get; }

    public AsyncRelayCommand MoveLaterCommand { get; }

    public void SetAnimationPlaying(bool value) =>
        IsAnimationPlaying = PreviewIsAnimated && value;

    public void RefreshCommandState()
    {
        SendCommand.RaiseCanExecuteChanged();
        PrepareCommand.RaiseCanExecuteChanged();
    }
}
