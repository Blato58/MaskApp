using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public static class AppBuiltInAnimationCatalog
{
    public static IReadOnlyList<int> ReservedSlots { get; } =
        Array.AsReadOnly(new[] { 15, 16, 17, 18, 19, 20 });

    public static IReadOnlyList<AppBuiltInAnimation> CreateBuiltIns()
    {
        return
        [
            Create(
                id: "holy-priest-cross-pulse",
                displayName: "Holy Priest · Cross Pulse",
                description: "White-mask cross pulse built for prepared DIY playback.",
                colorHex: "#F8FAFC",
                frames:
                [
                    CreateFrame("holy-priest-cross-pulse", "dim", "holy-priest-dim", 15),
                    CreateFrame("holy-priest-cross-pulse", "core", "holy-priest-cross", 16),
                    CreateFrame("holy-priest-cross-pulse", "flash", "holy-priest-flash", 17)
                ],
                playbackSlots: [15, 16, 17, 16]),
            Create(
                id: "holy-priest-red-mass",
                displayName: "Holy Priest · Red Mass",
                description: "Red-edged cross strobe built for prepared DIY playback.",
                colorHex: "#EF4444",
                frames:
                [
                    CreateFrame("holy-priest-red-mass", "dim", "holy-priest-red-dim", 18),
                    CreateFrame("holy-priest-red-mass", "core", "holy-priest-red", 19),
                    CreateFrame("holy-priest-red-mass", "flash", "holy-priest-red-flash", 20)
                ],
                playbackSlots: [18, 19, 20, 19])
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
