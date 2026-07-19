using System.Globalization;
using System.Resources;

namespace MaskApp.App.Resources.Strings;

public static class AppText
{
    private static readonly ResourceManager ResourceManager = new(
        "MaskApp.App.Resources.Strings.AppText",
        typeof(AppText).Assembly);

    public static CultureInfo? Culture { get; set; }

    public static string Library => Get(nameof(Library));
    public static string Stage => Get(nameof(Stage));
    public static string Device => Get(nameof(Device));
    public static string SearchLibrary => Get(nameof(SearchLibrary));
    public static string Add => Get(nameof(Add));
    public static string QuickDeck => Get(nameof(QuickDeck));
    public static string All => Get(nameof(All));
    public static string Faces => Get(nameof(Faces));
    public static string Text => Get(nameof(Text));
    public static string Animations => Get(nameof(Animations));
    public static string Scenes => Get(nameof(Scenes));
    public static string Build => Get(nameof(Build));
    public static string Preflight => Get(nameof(Preflight));
    public static string Connection => Get(nameof(Connection));
    public static string Diagnostics => Get(nameof(Diagnostics));
    public static string ShowReady => Get(nameof(ShowReady));
    public static string ShowDegraded => Get(nameof(ShowDegraded));
    public static string ShowNotReady => Get(nameof(ShowNotReady));
    public static string EnterStage => Get(nameof(EnterStage));
    public static string Stop => Get(nameof(Stop));
    public static string Blackout => Get(nameof(Blackout));
    public static string AnimationStudio => Get(nameof(AnimationStudio));
    public static string Save => Get(nameof(Save));

    public static string Get(string key) => ResourceManager.GetString(key, Culture) ?? key;
}
