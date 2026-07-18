using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.QuickActions;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryItemCard : INotifyPropertyChanged
{
    private bool isAnimationPlaying;
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

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public string OperationalStatusText => Item.Type switch
    {
        GalleryItemType.BuiltInStaticImage or GalleryItemType.BuiltInAnimation => "Instant",
        GalleryItemType.QuickAction when Item.QuickActionKind is not QuickActionKind.Text and not QuickActionKind.Random => "Instant",
        GalleryItemType.CustomStaticFace or GalleryItemType.AppBuiltInAnimation or GalleryItemType.CustomAnimation
            when Item.LastSendStatus.Contains("Prepared", StringComparison.OrdinalIgnoreCase) => "Prepared",
        GalleryItemType.TextPreset or GalleryItemType.CustomStaticFace or GalleryItemType.AppBuiltInAnimation or GalleryItemType.CustomAnimation => "Upload required",
        _ => "Unverified"
    };

    public string OperationalStatusIcon => OperationalStatusText switch
    {
        "Instant" => "⚡",
        "Prepared" => "✓",
        "Upload required" => "↑",
        _ => "?"
    };

    public string OperationalStatusColorHex => OperationalStatusText switch
    {
        "Instant" => "#22D3EE",
        "Prepared" => "#22C55E",
        "Upload required" => "#F59E0B",
        _ => "#92949B"
    };

    public string PreviewResourceName => Item.PreviewResourceName;

    public string PreviewBadgeText => Item.PreviewBadgeText;

    public string PreviewSourceText => Item.PreviewSourceText;

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

    public bool CanManage => Item.CanManage;

    public bool CanDelete => Item.Type == GalleryItemType.TextPreset;

    public string SelectionText => IsSelected ? "Selected" : "Select";

    public string DeleteEligibilityText => CanDelete ? "Can delete" : "Edit only";

    public AsyncRelayCommand SendCommand { get; }

    public AsyncRelayCommand ToggleSelectionCommand { get; }

    public AsyncRelayCommand EditCommand { get; }

    public AsyncRelayCommand MoveEarlierCommand { get; }

    public AsyncRelayCommand MoveLaterCommand { get; }

    public void SetAnimationPlaying(bool value) =>
        IsAnimationPlaying = PreviewIsAnimated && value;
}
