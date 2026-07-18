using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.HolyPriest;

namespace MaskApp.Core.Features.Animations;

public static class AppBuiltInAnimationCatalog
{
    public static IReadOnlyList<int> ReservedSlots { get; } =
        Array.AsReadOnly(new[]
        {
            HolyPriestBuiltInCatalog.OriginalSlot,
            HolyPriestBuiltInCatalog.InvertedSlot,
            HolyPriestBuiltInCatalog.RedSlot,
            HolyPriestBuiltInCatalog.BlueSlot,
            HolyPriestBuiltInCatalog.GoldSlot,
            HolyPriestBuiltInCatalog.BlackoutSlot
        });

    public static IReadOnlyList<AppBuiltInAnimation> CreateBuiltIns()
    {
        // Gallery and Pages persist these ids, so keep them stable as the artwork evolves.
        var frameBank = CreateFrameBank();
        return
        [
            Create(
                id: HolyPriestBuiltInCatalog.BlackWhiteAnimationId,
                displayName: "Holy Priest · Black / White Flash",
                description: "A slower black-and-white inversion tuned for reliable prepared DIY playback.",
                colorHex: "#FFFFFF",
                frameDuration: TimeSpan.FromMilliseconds(150),
                frames: SelectFrames(frameBank, HolyPriestBuiltInCatalog.OriginalSlot, HolyPriestBuiltInCatalog.InvertedSlot),
                playbackSlots: [HolyPriestBuiltInCatalog.OriginalSlot, HolyPriestBuiltInCatalog.InvertedSlot, HolyPriestBuiltInCatalog.OriginalSlot, HolyPriestBuiltInCatalog.InvertedSlot, HolyPriestBuiltInCatalog.OriginalSlot, HolyPriestBuiltInCatalog.InvertedSlot, HolyPriestBuiltInCatalog.OriginalSlot, HolyPriestBuiltInCatalog.InvertedSlot]),
            Create(
                id: HolyPriestBuiltInCatalog.BlueRedBlackAnimationId,
                displayName: "Holy Priest · Blue → Red → Black",
                description: "Blue and red versions of the original mask drop into blackout, then finish on the original white mask.",
                colorHex: "#0A84FF",
                frameDuration: TimeSpan.FromMilliseconds(180),
                frames: SelectFrames(frameBank, HolyPriestBuiltInCatalog.BlueSlot, HolyPriestBuiltInCatalog.RedSlot, HolyPriestBuiltInCatalog.BlackoutSlot, HolyPriestBuiltInCatalog.OriginalSlot),
                playbackSlots: [HolyPriestBuiltInCatalog.BlueSlot, HolyPriestBuiltInCatalog.RedSlot, HolyPriestBuiltInCatalog.BlackoutSlot, HolyPriestBuiltInCatalog.BlueSlot, HolyPriestBuiltInCatalog.RedSlot, HolyPriestBuiltInCatalog.BlackoutSlot, HolyPriestBuiltInCatalog.OriginalSlot, HolyPriestBuiltInCatalog.BlackoutSlot]),
            Create(
                id: HolyPriestBuiltInCatalog.FiveMaskAnimationId,
                displayName: "Holy Priest · Five Mask Cycle",
                description: "All five original-mask colorways move through a smooth circular procession.",
                colorHex: "#FFD60A",
                frameDuration: TimeSpan.FromMilliseconds(220),
                frames: SelectFrames(frameBank, HolyPriestBuiltInCatalog.OriginalSlot, HolyPriestBuiltInCatalog.InvertedSlot, HolyPriestBuiltInCatalog.RedSlot, HolyPriestBuiltInCatalog.BlueSlot, HolyPriestBuiltInCatalog.GoldSlot),
                playbackSlots: [HolyPriestBuiltInCatalog.OriginalSlot, HolyPriestBuiltInCatalog.RedSlot, HolyPriestBuiltInCatalog.GoldSlot, HolyPriestBuiltInCatalog.BlueSlot, HolyPriestBuiltInCatalog.InvertedSlot, HolyPriestBuiltInCatalog.BlueSlot, HolyPriestBuiltInCatalog.GoldSlot, HolyPriestBuiltInCatalog.RedSlot]),
            Create(
                id: HolyPriestBuiltInCatalog.ColorPulseAnimationId,
                displayName: "Holy Priest · Color Pulse",
                description: "Red, blue, and gold masks pulse against a dedicated black frame.",
                colorHex: "#FF3B30",
                frameDuration: TimeSpan.FromMilliseconds(200),
                frames: SelectFrames(frameBank, HolyPriestBuiltInCatalog.RedSlot, HolyPriestBuiltInCatalog.BlueSlot, HolyPriestBuiltInCatalog.GoldSlot, HolyPriestBuiltInCatalog.BlackoutSlot),
                playbackSlots: [HolyPriestBuiltInCatalog.RedSlot, HolyPriestBuiltInCatalog.BlackoutSlot, HolyPriestBuiltInCatalog.BlueSlot, HolyPriestBuiltInCatalog.BlackoutSlot, HolyPriestBuiltInCatalog.GoldSlot, HolyPriestBuiltInCatalog.BlackoutSlot, HolyPriestBuiltInCatalog.BlueSlot, HolyPriestBuiltInCatalog.BlackoutSlot])
        ];
    }

    private static IReadOnlyDictionary<int, AppBuiltInAnimationFrame> CreateFrameBank() =>
        new Dictionary<int, AppBuiltInAnimationFrame>
        {
            [HolyPriestBuiltInCatalog.OriginalSlot] = CreateFrame("original", "holy-priest-cross", HolyPriestBuiltInCatalog.OriginalSlot),
            [HolyPriestBuiltInCatalog.InvertedSlot] = CreateFrame("inverted", "holy-priest-inverted", HolyPriestBuiltInCatalog.InvertedSlot),
            [HolyPriestBuiltInCatalog.RedSlot] = CreateFrame("red", "holy-priest-red", HolyPriestBuiltInCatalog.RedSlot),
            [HolyPriestBuiltInCatalog.BlueSlot] = CreateFrame("blue", "holy-priest-blue", HolyPriestBuiltInCatalog.BlueSlot),
            [HolyPriestBuiltInCatalog.GoldSlot] = CreateFrame("gold", "holy-priest-gold", HolyPriestBuiltInCatalog.GoldSlot),
            [HolyPriestBuiltInCatalog.BlackoutSlot] = CreateFrame("blackout", "holy-priest-blackout", HolyPriestBuiltInCatalog.BlackoutSlot)
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
