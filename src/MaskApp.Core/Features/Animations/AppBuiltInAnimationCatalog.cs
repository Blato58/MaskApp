using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public static class AppBuiltInAnimationCatalog
{
    private const int CrossSlot = 15;
    private const int InvertedSlot = 16;
    private const int AntiheroSlot = 17;
    private const int BassSlot = 18;
    private const int AtlantisSlot = 19;
    private const int NoBalanceSlot = 20;

    public static IReadOnlyList<int> ReservedSlots { get; } =
        Array.AsReadOnly(new[] { CrossSlot, InvertedSlot, AntiheroSlot, BassSlot, AtlantisSlot, NoBalanceSlot });

    public static IReadOnlyList<AppBuiltInAnimation> CreateBuiltIns()
    {
        // Gallery and Pages persist these ids, so keep them stable as the artwork evolves.
        var frameBank = CreateFrameBank();
        return
        [
            Create(
                id: "holy-priest-cross-pulse",
                displayName: "Holy Priest · Black / White Flash",
                description: "A slower black-and-white inversion tuned for reliable prepared DIY playback.",
                colorHex: "#FFFFFF",
                frameDuration: TimeSpan.FromMilliseconds(150),
                frames: SelectFrames(frameBank, CrossSlot, InvertedSlot),
                playbackSlots: [CrossSlot, InvertedSlot, CrossSlot, InvertedSlot, CrossSlot, InvertedSlot, CrossSlot, InvertedSlot]),
            Create(
                id: "holy-priest-red-mass",
                displayName: "Holy Priest · Red Mass",
                description: "Red bass pistons break through the monochrome mask before a blue afterglow.",
                colorHex: "#FF3B30",
                frameDuration: TimeSpan.FromMilliseconds(180),
                frames: SelectFrames(frameBank, BassSlot, CrossSlot, InvertedSlot, AtlantisSlot),
                playbackSlots: [BassSlot, CrossSlot, BassSlot, InvertedSlot, BassSlot, AtlantisSlot, BassSlot, CrossSlot, BassSlot]),
            Create(
                id: "holy-priest-antihero-scan",
                displayName: "Holy Priest · Antihero Scan",
                description: "A cold visor scan cuts between the original mask and its inverted silhouette.",
                colorHex: "#52E3FF",
                frameDuration: TimeSpan.FromMilliseconds(200),
                frames: SelectFrames(frameBank, AntiheroSlot, CrossSlot, InvertedSlot),
                playbackSlots: [AntiheroSlot, CrossSlot, AntiheroSlot, InvertedSlot, AntiheroSlot, CrossSlot, AntiheroSlot, InvertedSlot]),
            Create(
                id: "holy-priest-atlantis-signal",
                displayName: "Holy Priest · Atlantis Signal",
                description: "A deep-blue sonar beacon surfaces through cyan antihero and monochrome echoes.",
                colorHex: "#0A84FF",
                frameDuration: TimeSpan.FromMilliseconds(240),
                frames: SelectFrames(frameBank, AtlantisSlot, AntiheroSlot, CrossSlot),
                playbackSlots: [AtlantisSlot, AtlantisSlot, AntiheroSlot, AtlantisSlot, CrossSlot, AtlantisSlot, AntiheroSlot, AtlantisSlot]),
            Create(
                id: "holy-priest-no-balance",
                displayName: "Holy Priest · No Balance",
                description: "An off-axis gyroscope snaps between violet torque and a submerged blue signal.",
                colorHex: "#BF5AF2",
                frameDuration: TimeSpan.FromMilliseconds(210),
                frames: SelectFrames(frameBank, NoBalanceSlot, AntiheroSlot, AtlantisSlot),
                playbackSlots: [NoBalanceSlot, AntiheroSlot, NoBalanceSlot, AtlantisSlot, NoBalanceSlot, AntiheroSlot, AtlantisSlot, AntiheroSlot]),
            Create(
                id: "holy-priest-ritual-inversion",
                displayName: "Holy Priest · Ritual Inversion",
                description: "Inverted monochrome, unstable geometry, and a red impact rotate in a tight ritual loop.",
                colorHex: "#FF9F0A",
                frameDuration: TimeSpan.FromMilliseconds(170),
                frames: SelectFrames(frameBank, InvertedSlot, NoBalanceSlot, BassSlot),
                playbackSlots: [InvertedSlot, NoBalanceSlot, InvertedSlot, BassSlot, InvertedSlot, NoBalanceSlot, BassSlot, NoBalanceSlot])
        ];
    }

    private static IReadOnlyDictionary<int, AppBuiltInAnimationFrame> CreateFrameBank() =>
        new Dictionary<int, AppBuiltInAnimationFrame>
        {
            [CrossSlot] = CreateFrame("cross", "holy-priest-cross", CrossSlot),
            [InvertedSlot] = CreateFrame("inverted", "holy-priest-inverted", InvertedSlot),
            [AntiheroSlot] = CreateFrame("antihero", "holy-priest-antihero", AntiheroSlot),
            [BassSlot] = CreateFrame("bass", "holy-priest-bass-powah", BassSlot),
            [AtlantisSlot] = CreateFrame("atlantis", "holy-priest-atlantis", AtlantisSlot),
            [NoBalanceSlot] = CreateFrame("no-balance", "holy-priest-no-balance", NoBalanceSlot)
        };

    private static IReadOnlyList<AppBuiltInAnimationFrame> SelectFrames(
        IReadOnlyDictionary<int, AppBuiltInAnimationFrame> frameBank,
        params int[] slots) =>
        slots.Select(slot => frameBank[slot]).ToArray();

    private static AppBuiltInAnimation Create(
        string id,
        string displayName,
        string description,
        string colorHex,
        TimeSpan frameDuration,
        IReadOnlyList<AppBuiltInAnimationFrame> frames,
        IReadOnlyList<int> playbackSlots) =>
        new AppBuiltInAnimation
        {
            Id = id,
            DisplayName = displayName,
            ArtistName = "Holy Priest",
            Description = description,
            ColorHex = colorHex,
            FrameDuration = frameDuration,
            Frames = frames,
            PlaybackSlots = playbackSlots
        }.Normalize();

    private static AppBuiltInAnimationFrame CreateFrame(
        string frameId,
        string artworkId,
        int slot) =>
        new()
        {
            Slot = slot,
            Pattern = FacePatternFactory.CreateBuiltInArtwork(
                $"app-animation-holy-priest-frame-{frameId}",
                $"Holy Priest frame {frameId}",
                artworkId,
                slot)
        };
}
