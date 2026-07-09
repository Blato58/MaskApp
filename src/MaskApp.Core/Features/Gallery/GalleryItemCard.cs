using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryItemCard
{
    public GalleryItemCard(
        GalleryItem item,
        bool isEditMode,
        bool isSelected,
        AsyncRelayCommand sendCommand,
        AsyncRelayCommand toggleSelectionCommand,
        AsyncRelayCommand editCommand,
        AsyncRelayCommand moveEarlierCommand,
        AsyncRelayCommand moveLaterCommand)
    {
        Item = item;
        IsEditMode = isEditMode;
        IsSelected = isSelected;
        SendCommand = sendCommand;
        ToggleSelectionCommand = toggleSelectionCommand;
        EditCommand = editCommand;
        MoveEarlierCommand = moveEarlierCommand;
        MoveLaterCommand = moveLaterCommand;
    }

    public GalleryItem Item { get; }

    public bool IsEditMode { get; }

    public bool IsSelected { get; }

    public bool IsNormalMode => !IsEditMode;

    public string Id => Item.Id;

    public string Title => Item.Title;

    public string Subtitle => Item.Subtitle;

    public string TypeLabel => Item.TypeLabel;

    public string GroupName => Item.GroupName;

    public bool IsFavorite => Item.IsFavorite;

    public string FavoriteLabel => Item.IsFavorite ? "FAV" : string.Empty;

    public string ColorHex => Item.ColorHex;

    public string IconKey => GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == Item.IconKey)?.Label ?? "ITEM";

    public string LastSendStatus => string.IsNullOrWhiteSpace(Item.LastSendStatus) ? "Not sent yet" : Item.LastSendStatus;

    public string PreviewText => Item.PreviewText;

    public string PreviewBadgeText => Item.PreviewBadgeText;

    public string PreviewSourceText => Item.PreviewSourceText;

    public bool HasPreview => Item.HasPreview;

    public bool CanSend => Item.CanSend;

    public bool CanManage => Item.CanManage;

    public bool CanDelete => Item.Type == GalleryItemType.TextPreset;

    public string SelectionText => IsSelected ? "Selected" : "Select";

    public string DeleteEligibilityText => CanDelete ? "Can delete" : "Edit only";

    public AsyncRelayCommand SendCommand { get; }

    public AsyncRelayCommand ToggleSelectionCommand { get; }

    public AsyncRelayCommand EditCommand { get; }

    public AsyncRelayCommand MoveEarlierCommand { get; }

    public AsyncRelayCommand MoveLaterCommand { get; }
}
