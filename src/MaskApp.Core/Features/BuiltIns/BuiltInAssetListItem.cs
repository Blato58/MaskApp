using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.BuiltIns;

public sealed class BuiltInAssetListItem : INotifyPropertyChanged
{
    private bool isAnimationPlaying;

    public BuiltInAssetListItem(
        BuiltInAssetRecord record,
        string title,
        string subtitle,
        string typeLabel,
        string idLabel,
        string tagsLabel,
        string statusLabel,
        string previewResourceName,
        bool previewIsAnimated,
        string previewBadgeText,
        string previewSourceText,
        AsyncRelayCommand sendCommand,
        AsyncRelayCommand editCommand,
        AsyncRelayCommand toggleFavoriteCommand)
    {
        Record = record;
        Title = title;
        Subtitle = subtitle;
        TypeLabel = typeLabel;
        IdLabel = idLabel;
        TagsLabel = tagsLabel;
        StatusLabel = statusLabel;
        PreviewResourceName = previewResourceName;
        PreviewIsAnimated = previewIsAnimated;
        PreviewBadgeText = previewBadgeText;
        PreviewSourceText = previewSourceText;
        SendCommand = sendCommand;
        EditCommand = editCommand;
        ToggleFavoriteCommand = toggleFavoriteCommand;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public BuiltInAssetRecord Record { get; }

    public BuiltInAssetType Type => Record.Type;

    public int Id => Record.Id;

    public string Title { get; }

    public string Subtitle { get; }

    public string TypeLabel { get; }

    public string IdLabel { get; }

    public string TagsLabel { get; }

    public string StatusLabel { get; }

    public bool IsFavorite => Record.IsFavorite || Record.Status == BuiltInAssetStatus.Favorite;

    public string FavoriteLabel => IsFavorite ? "★" : "☆";

    public string FavoriteAccessibilityLabel => IsFavorite ? "Remove from favorites" : "Add to favorites";

    public string PreviewResourceName { get; }

    public bool PreviewIsAnimated { get; }

    public string PreviewBadgeText { get; }

    public string PreviewSourceText { get; }

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
            OnPropertyChanged();
        }
    }

    public AsyncRelayCommand SendCommand { get; }

    public AsyncRelayCommand EditCommand { get; }

    public AsyncRelayCommand ToggleFavoriteCommand { get; }

    public void SetAnimationPlaying(bool value) =>
        IsAnimationPlaying = PreviewIsAnimated && value;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
