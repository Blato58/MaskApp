namespace MaskApp.Core.Features.QuickActions;

public sealed class QuickActionCatalog
{
    private static readonly QuickActionDefinition[] ActionDefinitions =
    [
        new(QuickActionId.Blackout, "BLACKOUT", QuickActionCategory.General, QuickActionKind.Command, Brightness: 1),
        new(QuickActionId.RestoreBrightness, "Restore", QuickActionCategory.General, QuickActionKind.Command, Brightness: 60),
        new(QuickActionId.SetBrightness, "Brightness", QuickActionCategory.General, QuickActionKind.Brightness),
        new(QuickActionId.RandomReaction, "Random", QuickActionCategory.General, QuickActionKind.Random),
        new(QuickActionId.Nope, "NOPE", QuickActionCategory.Meme, QuickActionKind.Text, "NOPE"),
        new(QuickActionId.Lol, "LOL", QuickActionCategory.Meme, QuickActionKind.Text, "LOL"),
        new(QuickActionId.Sus, "SUS", QuickActionCategory.Meme, QuickActionKind.Text, "SUS"),
        new(QuickActionId.Bruh, "BRUH", QuickActionCategory.Meme, QuickActionKind.Text, "BRUH"),
        new(QuickActionId.SendHelp, "SEND HELP", QuickActionCategory.Meme, QuickActionKind.Text, "SEND HELP"),
        new(QuickActionId.Buffering, "BUFFERING", QuickActionCategory.Meme, QuickActionKind.Text, "BUFFERING"),
        new(QuickActionId.NpcMode, "NPC MODE", QuickActionCategory.Meme, QuickActionKind.Text, "NPC MODE"),
        new(QuickActionId.Face404, "404 FACE", QuickActionCategory.Meme, QuickActionKind.Text, "404 FACE"),
        new(QuickActionId.NiceFit, "NICE FIT", QuickActionCategory.Social, QuickActionKind.Text, "NICE FIT"),
        new(QuickActionId.VibeCheck, "VIBE CHECK", QuickActionCategory.Social, QuickActionKind.Text, "VIBE CHECK"),
        new(QuickActionId.Drop, "DROP", QuickActionCategory.Rave, QuickActionKind.Text, "DROP", IsRave: true),
        new(QuickActionId.WheelUp, "WHEEL UP", QuickActionCategory.Rave, QuickActionKind.Text, "WHEEL UP", IsRave: true),
        new(QuickActionId.Reload, "RELOAD", QuickActionCategory.Rave, QuickActionKind.Text, "RELOAD", IsRave: true),
        new(QuickActionId.Boh, "BOH", QuickActionCategory.Rave, QuickActionKind.Text, "BOH", IsRave: true),
        new(QuickActionId.PullUp, "PULL UP", QuickActionCategory.Rave, QuickActionKind.Text, "PULL UP", IsRave: true),
        new(QuickActionId.RunItBack, "RUN IT BACK", QuickActionCategory.Rave, QuickActionKind.Text, "RUN IT BACK", IsRave: true),
        new(QuickActionId.BassFaceManual, "BASS FACE", QuickActionCategory.Rave, QuickActionKind.Text, "BASS FACE", IsRave: true),
        new(QuickActionId.Hydrate, "HYDRATE", QuickActionCategory.Welfare, QuickActionKind.Text, "HYDRATE"),
        new(QuickActionId.Water, "WATER?", QuickActionCategory.Welfare, QuickActionKind.Text, "WATER?"),
        new(QuickActionId.AllGood, "ALL GOOD?", QuickActionCategory.Welfare, QuickActionKind.Text, "ALL GOOD?"),
        new(QuickActionId.NiceMoves, "NICE MOVES", QuickActionCategory.Social, QuickActionKind.Text, "NICE MOVES", IsRave: true),
        new(QuickActionId.TooMuchBass, "TOO MUCH BASS", QuickActionCategory.Rave, QuickActionKind.Text, "TOO MUCH BASS", IsRave: true),
        new(QuickActionId.NoThoughts, "NO THOUGHTS", QuickActionCategory.Rave, QuickActionKind.Text, "NO THOUGHTS", IsRave: true),
        new(QuickActionId.WhereWater, "WHERE WATER?", QuickActionCategory.Welfare, QuickActionKind.Text, "WHERE WATER?", IsRave: true),
        new(QuickActionId.ILiveHere, "I LIVE HERE", QuickActionCategory.Rave, QuickActionKind.Text, "I LIVE HERE", IsRave: true)
    ];

    public IReadOnlyList<QuickActionDefinition> Actions { get; } = ActionDefinitions;

    public QuickActionDefinition Get(QuickActionId id) => Actions.Single(action => action.Id == id);

    public IReadOnlyList<QuickActionDefinition> ByCategory(QuickActionCategory category) =>
        Actions.Where(action => action.Category == category).ToArray();
}
