using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryPageShortcutCard
{
    public GalleryPageShortcutCard(
        GalleryPageItemLayout layout,
        GalleryItem item,
        AsyncRelayCommand sendCommand,
        AsyncRelayCommand removeCommand,
        AsyncRelayCommand moveEarlierCommand,
        AsyncRelayCommand moveLaterCommand,
        AsyncRelayCommand cycleIconCommand,
        AsyncRelayCommand cycleColorCommand)
    {
        Layout = layout;
        Item = item;
        SendCommand = sendCommand;
        RemoveCommand = removeCommand;
        MoveEarlierCommand = moveEarlierCommand;
        MoveLaterCommand = moveLaterCommand;
        CycleIconCommand = cycleIconCommand;
        CycleColorCommand = cycleColorCommand;
    }

    public GalleryPageItemLayout Layout { get; }

    public GalleryItem Item { get; }

    public string SlotId => Layout.SlotId;

    public string Label => string.IsNullOrWhiteSpace(Layout.Label) ? Item.Title : Layout.Label;

    public string Subtitle => Item.TypeLabel;

    public string IconLabel => GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == Layout.IconKey)?.Label ?? "ITEM";

    public string ColorHex => Layout.ColorHex;

    public bool CanSend => Item.CanSend;

    public AsyncRelayCommand SendCommand { get; }

    public AsyncRelayCommand RemoveCommand { get; }

    public AsyncRelayCommand MoveEarlierCommand { get; }

    public AsyncRelayCommand MoveLaterCommand { get; }

    public AsyncRelayCommand CycleIconCommand { get; }

    public AsyncRelayCommand CycleColorCommand { get; }
}
