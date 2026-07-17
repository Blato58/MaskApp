using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public sealed record AppBuiltInAnimation
{
    public const int MaxPlaybackSlots = 10;

    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string ArtistName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string ColorHex { get; init; } = "#A78BFA";

    public bool IsFavorite { get; init; } = true;

    public IReadOnlyList<AppBuiltInAnimationFrame> Frames { get; init; } = [];

    public IReadOnlyList<int> PlaybackSlots { get; init; } = [];

    public FacePattern PreviewPattern => Frames[0].Pattern;

    public IReadOnlyList<int> ReservedSlots => Frames.Select(frame => frame.Slot).ToArray();

    public AppBuiltInAnimation Normalize()
    {
        var id = Id.Trim();
        var displayName = DisplayName.Trim();
        var frames = Frames.Select(frame => frame.Normalize()).ToArray();
        var playbackSlots = PlaybackSlots.ToArray();

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Animation id is required.", nameof(Id));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Animation display name is required.", nameof(DisplayName));
        }

        if (frames.Length is < 2 or > MaxPlaybackSlots)
        {
            throw new ArgumentException("App-built animations must contain between 2 and 10 stored frames.", nameof(Frames));
        }

        if (frames.Select(frame => frame.Slot).Distinct().Count() != frames.Length)
        {
            throw new ArgumentException("Animation frame slots must be unique.", nameof(Frames));
        }

        if (playbackSlots.Length is < 2 or > MaxPlaybackSlots)
        {
            throw new ArgumentException("Animation playback must contain between 2 and 10 slot steps.", nameof(PlaybackSlots));
        }

        var frameSlots = frames.Select(frame => frame.Slot).ToHashSet();
        if (playbackSlots.Any(slot => !frameSlots.Contains(slot)))
        {
            throw new ArgumentException("Animation playback can only reference its stored frame slots.", nameof(PlaybackSlots));
        }

        return this with
        {
            Id = id,
            DisplayName = displayName,
            ArtistName = ArtistName.Trim(),
            Description = Description.Trim(),
            Frames = frames,
            PlaybackSlots = playbackSlots
        };
    }
}
