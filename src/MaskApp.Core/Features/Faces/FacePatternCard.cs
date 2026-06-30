using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Faces;

public sealed class FacePatternCard
{
    public FacePatternCard(
        FacePattern pattern,
        bool isSelected,
        AsyncRelayCommand openCommand,
        AsyncRelayCommand uploadCommand,
        AsyncRelayCommand deleteCommand)
    {
        Pattern = pattern;
        IsSelected = isSelected;
        OpenCommand = openCommand;
        UploadCommand = uploadCommand;
        DeleteCommand = deleteCommand;
    }

    public FacePattern Pattern { get; }

    public string Id => Pattern.Id;

    public string DisplayName => Pattern.DisplayName;

    public string Subtitle => $"{Pattern.SourceLabel} / Slot {Pattern.PreferredSlot}";

    public string StatusText => string.IsNullOrWhiteSpace(Pattern.LastUploadStatus)
        ? "Not uploaded yet"
        : Pattern.LastUploadStatus;

    public string AccentColorHex => Pattern.AccentColorHex;

    public bool IsSelected { get; }

    public bool CanDelete => !Pattern.IsBuiltIn;

    public AsyncRelayCommand OpenCommand { get; }

    public AsyncRelayCommand UploadCommand { get; }

    public AsyncRelayCommand DeleteCommand { get; }
}
