using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class GallerySelectionAction
{
    public GallerySelectionAction(
        string title,
        string detail,
        AsyncRelayCommand command,
        bool isDestructive = false)
    {
        Title = title;
        Detail = detail;
        Command = command;
        IsDestructive = isDestructive;
    }

    public string Title { get; }

    public string Detail { get; }

    public AsyncRelayCommand Command { get; }

    public bool IsDestructive { get; }
}
