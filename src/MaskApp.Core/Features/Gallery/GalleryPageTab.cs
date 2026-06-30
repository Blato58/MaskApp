using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryPageTab
{
    public GalleryPageTab(
        GalleryPageLayout page,
        AsyncRelayCommand selectCommand,
        AsyncRelayCommand moveEarlierCommand,
        AsyncRelayCommand moveLaterCommand)
    {
        Page = page;
        SelectCommand = selectCommand;
        MoveEarlierCommand = moveEarlierCommand;
        MoveLaterCommand = moveLaterCommand;
    }

    public GalleryPageLayout Page { get; }

    public string PageId => Page.PageId;

    public string Title => Page.Title;

    public string ColorHex => Page.ColorHex;

    public AsyncRelayCommand SelectCommand { get; }

    public AsyncRelayCommand MoveEarlierCommand { get; }

    public AsyncRelayCommand MoveLaterCommand { get; }
}
