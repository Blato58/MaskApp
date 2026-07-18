namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryActionResult(bool Succeeded, string Message)
{
    public static GalleryActionResult Success(string message) => new(true, message);

    public static GalleryActionResult Failure(string message) => new(false, message);
}

public sealed class GalleryActionCompletedEventArgs(
    GalleryItem item,
    GalleryPageItemLayout? layout,
    GalleryActionResult result) : EventArgs
{
    public GalleryItem Item { get; } = item;

    public GalleryPageItemLayout? Layout { get; } = layout;

    public GalleryActionResult Result { get; } = result;
}
