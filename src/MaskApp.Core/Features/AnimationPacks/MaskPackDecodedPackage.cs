using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.AnimationPacks;

public sealed record MaskPackDecodedItem<T>(MaskPackContentEntry Entry, T Value);

public sealed record MaskPackAppearanceSettings
{
    public GalleryOrderState GalleryOrder { get; init; } = new();
}

public sealed record MaskPackDecodedPackage
{
    public required MaskPackManifest Manifest { get; init; }

    public IReadOnlyList<MaskPackDecodedItem<FacePattern>> Faces { get; init; } = [];

    public IReadOnlyList<MaskPackDecodedItem<AnimationProject>> Animations { get; init; } = [];

    public IReadOnlyList<MaskPackDecodedItem<TextPreset>> TextPresets { get; init; } = [];

    public IReadOnlyList<MaskPackDecodedItem<GalleryPageLayout>> Pages { get; init; } = [];

    public IReadOnlyList<MaskPackDecodedItem<PerformanceScene>> Scenes { get; init; } = [];

    public IReadOnlyList<MaskPackDecodedItem<PerformanceSetlist>> Setlists { get; init; } = [];

    public MaskPackDecodedItem<MaskPackAppearanceSettings>? Appearance { get; init; }

    public bool MigratedFromV1 { get; init; }

    public IReadOnlyList<MaskPackContentEntry> Entries =>
        Faces.Select(item => item.Entry)
            .Concat(Animations.Select(item => item.Entry))
            .Concat(TextPresets.Select(item => item.Entry))
            .Concat(Pages.Select(item => item.Entry))
            .Concat(Scenes.Select(item => item.Entry))
            .Concat(Setlists.Select(item => item.Entry))
            .Concat(Appearance is null ? [] : [Appearance.Entry])
            .ToArray();
}
