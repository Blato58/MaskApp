using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public static class AppBuiltInAnimationCatalog
{
    public static IReadOnlyList<int> ReservedSlots { get; } =
        Array.AsReadOnly(new[] { 15, 16, 17, 18, 19 });

    public static IReadOnlyList<AppBuiltInAnimation> CreateBuiltIns()
    {
        // Gallery and Pages persist these ids, so keep them stable as the artwork evolves.
        return
        [
            Create(
                id: "holy-priest-cross-pulse",
                displayName: "Holy Priest · Black / White Flash",
                description: "Looping black-and-white inversion with BLE-controlled playback speed.",
                colorHex: "#FFFFFF",
                frames:
                [
                    CreateFrame("holy-priest-cross-pulse", "normal", "holy-priest-cross", 15),
                    CreateFrame("holy-priest-cross-pulse", "inverted", "holy-priest-inverted", 16)
                ],
                playbackSlots: [15, 16, 15, 16, 15, 16, 15, 16, 15, 16]),
            Create(
                id: "holy-priest-red-mass",
                displayName: "Holy Priest · Black → Red → Blue",
                description: "Looping black-to-red-to-blue cross cycle with BLE-controlled playback speed.",
                colorHex: "#0000FF",
                frames:
                [
                    CreateFrame("holy-priest-red-mass", "black", "holy-priest-cross", 17),
                    CreateFrame("holy-priest-red-mass", "red", "holy-priest-red", 18),
                    CreateFrame("holy-priest-red-mass", "blue", "holy-priest-blue", 19)
                ],
                playbackSlots: [17, 18, 19, 17, 18, 19, 17, 18, 19])
        ];
    }

    private static AppBuiltInAnimation Create(
        string id,
        string displayName,
        string description,
        string colorHex,
        IReadOnlyList<AppBuiltInAnimationFrame> frames,
        IReadOnlyList<int> playbackSlots) =>
        new AppBuiltInAnimation
        {
            Id = id,
            DisplayName = displayName,
            ArtistName = "Holy Priest",
            Description = description,
            ColorHex = colorHex,
            Frames = frames,
            PlaybackSlots = playbackSlots
        }.Normalize();

    private static AppBuiltInAnimationFrame CreateFrame(
        string animationId,
        string frameId,
        string artworkId,
        int slot) =>
        new()
        {
            Slot = slot,
            Pattern = FacePatternFactory.CreateBuiltInArtwork(
                $"app-animation-{animationId}-{frameId}",
                $"{animationId} {frameId}",
                artworkId,
                slot)
        };
}
