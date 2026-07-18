using MaskApp.Core.Features.HolyPriest;

namespace MaskApp.Core.Features.Faces;

public static class FacePatternFactory
{
    public static IReadOnlyList<FacePattern> CreateBuiltIns()
    {
        var now = DateTimeOffset.UnixEpoch;
        return
        [
            CreateSmiley("happy", "Happy", FaceEmotion.Happy, 1, now),
            CreateSmiley("sad", "Sad", FaceEmotion.Sad, 2, now),
            CreateSmiley("angry", "Angry", FaceEmotion.Angry, 3, now),
            CreateSmiley("surprised", "Surprised", FaceEmotion.Surprised, 4, now),
            CreateSmiley("meh", "Meh", FaceEmotion.Meh, 5, now),
            CreateSmiley("wink", "Wink", FaceEmotion.Wink, 6, now),
            CreateCharacter("cool-shades", "Cool Shades", now),
            CreateCharacter("heart-eyes", "Heart Eyes", now),
            CreateCharacter("starstruck", "Starstruck", now),
            CreateCharacter("big-laugh", "Big Laugh", now),
            CreateCharacter("tongue-out", "Tongue Out", now),
            CreateCharacter("vampire", "Vampire", now),
            CreateCharacter("robot", "Robot", now),
            CreateCharacter("alien", "Alien", now),
            CreateCharacter("cat", "Pixel Cat", now),
            CreateCharacter("puppy", "Pixel Puppy", now),
            CreateCharacter("frog", "Pixel Frog", now),
            CreateCharacter("panda", "Pixel Panda", now),
            CreateCharacter("skull", "Neon Skull", now),
            CreateCharacter("ghost", "Tiny Ghost", now),
            CreateCharacter("little-devil", "Little Devil", now),
            CreateCharacter("clown", "Silly Clown", now),
            CreateCharacter("pirate", "Pixel Pirate", now),
            CreateCharacter("ninja", "Pixel Ninja", now),
            CreateCharacter("cowboy", "Pixel Cowboy", now),
            CreateCharacter("mustache", "Fancy Mustache", now),
            CreateCharacter("dj", "Rave DJ", now),
            CreateCharacter("three-eyed-monster", "Three-Eyed Monster", now),
            CreateCharacter("cyclops", "Happy Cyclops", now),
            CreateCharacter("sleepy", "Sleepy Face", now),
            CreateCharacterWithId(HolyPriestBuiltInCatalog.OriginalFaceId, "Holy Priest · Original", "holy-priest-cross", HolyPriestBuiltInCatalog.OriginalSlot, now),
            CreateCharacterWithId(HolyPriestBuiltInCatalog.InvertedFaceId, "Holy Priest · Inverted", "holy-priest-inverted", HolyPriestBuiltInCatalog.InvertedSlot, now),
            CreateCharacterWithId(HolyPriestBuiltInCatalog.RedFaceId, "Holy Priest · Red", "holy-priest-red", HolyPriestBuiltInCatalog.RedSlot, now),
            CreateCharacterWithId(HolyPriestBuiltInCatalog.BlueFaceId, "Holy Priest · Blue", "holy-priest-blue", HolyPriestBuiltInCatalog.BlueSlot, now),
            CreateCharacterWithId(HolyPriestBuiltInCatalog.GoldFaceId, "Holy Priest · Gold", "holy-priest-gold", HolyPriestBuiltInCatalog.GoldSlot, now),
            CreateCharacter("mask-calibration", "Mask Calibration · Color Anchors", now)
        ];
    }

    public static FacePattern CreateBlank(string name = "Custom Face", int preferredSlot = 7) =>
        new FacePattern
        {
            Id = $"face-{Guid.NewGuid():N}",
            DisplayName = name,
            Source = FacePatternSource.Custom,
            Emotion = FaceEmotion.Custom,
            PreferredSlot = preferredSlot,
            Pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray()
        }.Normalize();

    private static FacePattern CreateSmiley(
        string artworkId,
        string name,
        FaceEmotion emotion,
        int slot,
        DateTimeOffset timestamp) =>
        Create(
            $"built-in-smiley-{artworkId}",
            $"{name} Smiley",
            artworkId,
            emotion,
            slot,
            timestamp);

    private static FacePattern CreateCharacter(string artworkId, string name, DateTimeOffset timestamp) =>
        CreateCharacterWithId($"built-in-face-{artworkId}", name, artworkId, 7, timestamp);

    private static FacePattern CreateCharacterWithId(
        string id,
        string name,
        string artworkId,
        int slot,
        DateTimeOffset timestamp) =>
        Create(
            id,
            name,
            artworkId,
            FaceEmotion.Custom,
            slot,
            timestamp);

    internal static FacePattern CreateBuiltInArtwork(
        string id,
        string name,
        string artworkId,
        int preferredSlot) =>
        Create(
            id,
            name,
            artworkId,
            FaceEmotion.Custom,
            preferredSlot,
            DateTimeOffset.UnixEpoch);

    private static FacePattern Create(
        string id,
        string name,
        string artworkId,
        FaceEmotion emotion,
        int slot,
        DateTimeOffset timestamp)
    {
        var canvas = new FaceArtCanvas();
        FaceArtwork.Draw(artworkId, canvas);
        return new FacePattern
        {
            Id = id,
            DisplayName = name,
            Emotion = emotion,
            Source = FacePatternSource.BuiltIn,
            PreferredSlot = slot,
            IsFavorite = true,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            Pixels = canvas.Pixels
        }.Normalize(timestamp);
    }
}
