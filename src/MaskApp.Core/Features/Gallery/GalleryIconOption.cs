namespace MaskApp.Core.Features.Gallery;

public sealed record GalleryIconOption(
    string IconKey,
    string Label,
    string ColorHex,
    string Pack = "Mask",
    string PreviewAsset = "",
    string SearchText = "",
    string Category = "General")
{
    public static IReadOnlyList<string> Packs { get; } =
    [
        "Mask",
        "Lucide",
        "Material",
        "Phosphor"
    ];

    public static IReadOnlyList<GalleryIconOption> Defaults { get; } =
    [
        new("txt", "TXT", "#52E3FF", "Mask", "txt.svg", "text caption words message", "Mask"),
        new("face", "FACE", "#A78BFA", "Mask", "face.svg", "face mask static image", "Mask"),
        new("anim", "ANIM", "#FF3D8B", "Mask", "anim.svg", "animation moving mask", "Mask"),
        new("rave", "RAVE", "#FACC15", "Mask", "rave.svg", "rave party dance", "Mask"),
        new("fav", "FAV", "#22C55E", "Mask", "fav.svg", "favorite star saved", "Mask"),
        new("safe", "SAFE", "#FFFFFF", "Mask", "safe.svg", "safe blackout recovery", "Mask"),
        new("pack", "PACK", "#F472B6", "Mask", "pack.svg", "pack collection", "Mask"),

        new("lucide:message-circle", "MSG", "#52E3FF", "Lucide", "lucide_message_circle.svg", "message circle chat caption", "Communication"),
        new("lucide:smile", "SMILE", "#FACC15", "Lucide", "lucide_smile.svg", "smile happy face", "Mood"),
        new("lucide:laugh", "LOL", "#FACC15", "Lucide", "lucide_laugh.svg", "laugh lol funny", "Mood"),
        new("lucide:meh", "MEH", "#A78BFA", "Lucide", "lucide_meh.svg", "meh neutral", "Mood"),
        new("lucide:heart", "LOVE", "#FF3D8B", "Lucide", "lucide_heart.svg", "heart love", "Mood"),
        new("lucide:zap", "ZAP", "#FACC15", "Lucide", "lucide_zap.svg", "zap lightning fast", "Energy"),
        new("lucide:music", "MUSIC", "#52E3FF", "Lucide", "lucide_music.svg", "music note song", "Audio"),
        new("lucide:radio", "RADIO", "#52E3FF", "Lucide", "lucide_radio.svg", "radio signal", "Audio"),
        new("lucide:mic", "MIC", "#F472B6", "Lucide", "lucide_mic.svg", "microphone voice", "Audio"),
        new("lucide:eye", "EYE", "#FFFFFF", "Lucide", "lucide_eye.svg", "eye look watch", "Expression"),
        new("lucide:star", "STAR", "#FACC15", "Lucide", "lucide_star.svg", "star favorite", "Mood"),
        new("lucide:flame", "FIRE", "#FF3D8B", "Lucide", "lucide_flame.svg", "flame fire hot", "Energy"),
        new("lucide:moon", "MOON", "#A78BFA", "Lucide", "lucide_moon.svg", "moon night", "Mood"),
        new("lucide:sun", "SUN", "#FACC15", "Lucide", "lucide_sun.svg", "sun bright", "Mood"),
        new("lucide:party-popper", "PARTY", "#FF3D8B", "Lucide", "lucide_party_popper.svg", "party celebration", "RAVE"),
        new("lucide:palette", "ART", "#22C55E", "Lucide", "lucide_palette.svg", "palette color art", "Creative"),

        new("material:waving_hand", "WAVE", "#FACC15", "Material", "material_waving_hand.svg", "waving hand hello hey", "Gesture"),
        new("material:pets", "PET", "#A78BFA", "Material", "material_pets.svg", "pets animal cat dog", "Mood"),
        new("material:sentiment_excited", "HYPE", "#FACC15", "Material", "material_sentiment_excited.svg", "sentiment excited happy", "Mood"),
        new("material:bolt", "BOLT", "#FACC15", "Material", "material_bolt.svg", "bolt lightning fast", "Energy"),
        new("material:favorite", "LOVE", "#FF3D8B", "Material", "material_favorite.svg", "favorite heart love", "Mood"),
        new("material:visibility", "LOOK", "#FFFFFF", "Material", "material_visibility.svg", "visibility eye look", "Expression"),
        new("material:graphic_eq", "EQ", "#52E3FF", "Material", "material_graphic_eq.svg", "graphic equalizer audio", "Audio"),
        new("material:theater_comedy", "DRAMA", "#A78BFA", "Material", "material_theater_comedy.svg", "theater comedy mask", "Expression"),
        new("material:celebration", "PARTY", "#FF3D8B", "Material", "material_celebration.svg", "celebration party", "RAVE"),
        new("material:volume_up", "LOUD", "#52E3FF", "Material", "material_volume_up.svg", "volume loud sound", "Audio"),
        new("material:light_mode", "LIGHT", "#FACC15", "Material", "material_light_mode.svg", "light mode sun bright", "Device"),
        new("material:dark_mode", "DARK", "#A78BFA", "Material", "material_dark_mode.svg", "dark mode moon night", "Device"),

        new("phosphor:cat", "CAT", "#FACC15", "Phosphor", "phosphor_cat.svg", "cat animal", "Mood"),
        new("phosphor:dog", "DOG", "#FACC15", "Phosphor", "phosphor_dog.svg", "dog animal", "Mood"),
        new("phosphor:alien", "ALIEN", "#22C55E", "Phosphor", "phosphor_alien.svg", "alien weird", "Mood"),
        new("phosphor:mask-happy", "MASK", "#A78BFA", "Phosphor", "phosphor_mask_happy.svg", "mask happy theater", "Expression"),
        new("phosphor:music-notes", "SONG", "#52E3FF", "Phosphor", "phosphor_music_notes.svg", "music notes song", "Audio"),
        new("phosphor:lightning", "FAST", "#FACC15", "Phosphor", "phosphor_lightning.svg", "lightning fast", "Energy"),
        new("phosphor:fire", "FIRE", "#FF3D8B", "Phosphor", "phosphor_fire.svg", "fire flame", "Energy"),
        new("phosphor:skull", "SKULL", "#FFFFFF", "Phosphor", "phosphor_skull.svg", "skull dead", "Mood"),
        new("phosphor:smiley", "JOY", "#FACC15", "Phosphor", "phosphor_smiley.svg", "smiley happy", "Mood"),
        new("phosphor:eyes", "EYES", "#FFFFFF", "Phosphor", "phosphor_eyes.svg", "eyes look", "Expression"),
        new("phosphor:disco-ball", "DISCO", "#FF3D8B", "Phosphor", "phosphor_disco_ball.svg", "disco ball rave party", "RAVE"),
        new("phosphor:sparkle", "SPARK", "#22C55E", "Phosphor", "phosphor_sparkle.svg", "sparkle magic", "Mood")
    ];
}
