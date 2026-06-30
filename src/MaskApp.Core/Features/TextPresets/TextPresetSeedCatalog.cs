using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Features.TextPresets;

public static class TextPresetSeedCatalog
{
    public const string CzechBasicPackName = "Czech Basic";
    public const string CzechMemePackName = "Czech Meme";
    public const string CzechPoliticalPackName = "Czech Political/Satire";
    public const string CzechRavePackName = "Czech RAVE";
    public const string LegacyPackName = "Legacy Quick Captions";

    public static IReadOnlyList<TextPreset> CreateSeedPresets(DateTimeOffset? timestamp = null)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        return CreatePack(CzechBasicPackName, TextPresetCategory.CzechBasic,
                ["AHOJ", "CAU", "DIKY", "PROSIM", "POJD", "NE", "JO", "DOBRY", "KLID", "PIVO?", "VODA?", "KDE JSI", "JDU TAM", "POCKEJ"],
                now,
                showInRave: false)
            .Concat(CreatePack(CzechMemePackName, TextPresetCategory.CzechMeme,
                ["TY VOLE", "TO JE SUS", "JSEM NPC", "NEMAM SIGNAL", "MAM DOST", "KDE JE VODA", "JA NIC", "PROC ZASE JA", "KAM JDEM", "KDO TO PUSTIL", "TO JE PEKLO", "DOBRY DEN", "KONECNE"],
                now,
                showInRave: false))
            .Concat(CreatePack(CzechPoliticalPackName, TextPresetCategory.CzechPoliticalSatire,
                ["DEMISI?", "KDE JE PLAN", "SLIBY CHYBY", "VOLBY MODE", "KAMPAN BEZI", "KOALICE?", "OPOZICE?", "SPIN DOCTOR", "TISKOVKA", "MANDAT?", "DOTACE?", "KDO TO PLATI", "TO NEPROJDE", "HLASUJU NE", "HLASUJU JO"],
                now,
                showInRave: false))
            .Concat(CreatePack(CzechRavePackName, TextPresetCategory.CzechRave,
                ["DROP", "BASS", "RELOAD", "PULL UP", "KDE JE VODA", "JEDU", "MOC BASSU", "DNB MODE", "ZTRACEN", "HLEDEJ ME", "JSEM TADY", "HYDRATUJ", "VIBE CHECK"],
                now,
                showInRave: true))
            .Concat(CreateLegacyQuickCaptions(now))
            .ToArray();
    }

    public static IReadOnlyList<TextPresetPack> CreateSeedPacks(DateTimeOffset? timestamp = null) =>
        CreateSeedPresets(timestamp)
            .Where(preset => preset.Category != TextPresetCategory.Legacy)
            .GroupBy(preset => new { preset.PackName, preset.Category })
            .Select(group => new TextPresetPack(group.Key.PackName, group.Key.Category, group.ToArray()))
            .ToArray();

    private static IEnumerable<TextPreset> CreatePack(
        string packName,
        TextPresetCategory category,
        IReadOnlyList<string> texts,
        DateTimeOffset timestamp,
        bool showInRave)
    {
        foreach (var text in texts)
        {
            yield return CreateSeedPreset(packName, category, text, timestamp, showInRave);
        }
    }

    private static IEnumerable<TextPreset> CreateLegacyQuickCaptions(DateTimeOffset timestamp)
    {
        var catalog = new QuickActionCatalog();
        foreach (var action in catalog.Actions.Where(action => action.Kind == QuickActionKind.Text && !string.IsNullOrWhiteSpace(action.Caption)))
        {
            yield return CreateSeedPreset(
                LegacyPackName,
                TextPresetCategory.Legacy,
                action.Caption!,
                timestamp,
                action.IsRave,
                favorite: action.Id is QuickActionId.Lol or QuickActionId.Nope or QuickActionId.VibeCheck or QuickActionId.Drop);
        }
    }

    private static TextPreset CreateSeedPreset(
        string packName,
        TextPresetCategory category,
        string inputText,
        DateTimeOffset timestamp,
        bool showInRave,
        bool favorite = false)
    {
        var normalized = CzechTextNormalizer.Normalize(inputText);
        return new TextPreset
        {
            Id = TextPresetId.Seed(packName, inputText),
            InputText = normalized.InputText,
            MaskText = normalized.MaskText,
            DisplayName = normalized.InputText,
            Category = category,
            PackName = packName,
            Tags = category == TextPresetCategory.CzechPoliticalSatire ? ["satire", "editable", "local"] : [],
            Style = CreateDefaultStyle(category),
            IsFavorite = favorite,
            Visibility = new TextPresetVisibility
            {
                ShowInReact = true,
                ShowInRave = showInRave,
                ShowInControl = favorite
            },
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            IsSeed = true
        };
    }

    private static TextPresetStyle CreateDefaultStyle(TextPresetCategory category)
    {
        var color = category switch
        {
            TextPresetCategory.CzechRave => QuickCaptionForegroundPalette.GetColor(QuickCaptionForegroundPreset.Cyan),
            TextPresetCategory.CzechMeme => QuickCaptionForegroundPalette.GetColor(QuickCaptionForegroundPreset.Pink),
            TextPresetCategory.CzechPoliticalSatire => QuickCaptionForegroundPalette.GetColor(QuickCaptionForegroundPreset.Amber),
            TextPresetCategory.Legacy => QuickCaptionForegroundPalette.GetColor(QuickCaptionForegroundPreset.White),
            _ => QuickCaptionForegroundPalette.GetColor(QuickCaptionForegroundPreset.White)
        };

        return TextPresetStyle.Default with
        {
            ForegroundColor = color,
            LayoutMode = TextPresetLayoutMode.FixedWidthCentered,
            DisplayMode = TextDisplayMode.Blink,
            Speed = 50,
            SendProfile = TextPresetSendProfile.LowStaticFlash,
            UseBlackBackgroundReset = false
        };
    }
}
