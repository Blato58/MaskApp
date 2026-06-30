namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryAddOption(
    GalleryAddOptionKind Kind,
    string Title,
    string Subtitle,
    string IconKey,
    string ColorHex,
    bool IsAvailable);
