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
    public static string Live => Get(nameof(Live));
    public static string Shows => Get(nameof(Shows));
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
    public static string Create => Get(nameof(Create));
    public static string Cancel => Get(nameof(Cancel));
    public static string Done => Get(nameof(Done));
    public static string EditDeck => Get(nameof(EditDeck));
    public static string AddFromLibrary => Get(nameof(AddFromLibrary));
    public static string RequireHoldForActions => Get(nameof(RequireHoldForActions));
    public static string DeleteDeck => Get(nameof(DeleteDeck));
    public static string Reconnect => Get(nameof(Reconnect));
    public static string ChooseMask => Get(nameof(ChooseMask));
    public static string MaskConnected => Get(nameof(MaskConnected));
    public static string Disconnected => Get(nameof(Disconnected));
    public static string Sending => Get(nameof(Sending));
    public static string Written => Get(nameof(Written));
    public static string Confirmed => Get(nameof(Confirmed));
    public static string Failed => Get(nameof(Failed));
    public static string Unknown => Get(nameof(Unknown));
    public static string TextStudio => Get(nameof(TextStudio));
    public static string FaceStudio => Get(nameof(FaceStudio));
    public static string StockCatalog => Get(nameof(StockCatalog));
    public static string ShowBuilder => Get(nameof(ShowBuilder));
    public static string SceneEditor => Get(nameof(SceneEditor));
    public static string ImportMaskPack => Get(nameof(ImportMaskPack));
    public static string ExportMaskPack => Get(nameof(ExportMaskPack));
    public static string GetStarted => Get(nameof(GetStarted));
    public static string ExploreWithoutMask => Get(nameof(ExploreWithoutMask));
    public static string ConnectNearby => Get(nameof(ConnectNearby));
    public static string AllowBluetooth => Get(nameof(AllowBluetooth));
    public static string NotNow => Get(nameof(NotNow));
    public static string SkipForNow => Get(nameof(SkipForNow));
    public static string ScanAgain => Get(nameof(ScanAgain));
    public static string NoMaskFound => Get(nameof(NoMaskFound));
    public static string Continue => Get(nameof(Continue));
    public static string NoMatchingContent => Get(nameof(NoMatchingContent));
    public static string ClearFilters => Get(nameof(ClearFilters));
    public static string BrowseStock => Get(nameof(BrowseStock));
    public static string Import => Get(nameof(Import));
    public static string Export => Get(nameof(Export));
    public static string NewShow => Get(nameof(NewShow));
    public static string AddCue => Get(nameof(AddCue));
    public static string AddStep => Get(nameof(AddStep));
    public static string RunPreflight => Get(nameof(RunPreflight));
    public static string OpenDevice => Get(nameof(OpenDevice));
    public static string Previous => Get(nameof(Previous));
    public static string Next => Get(nameof(Next));
    public static string Retry => Get(nameof(Retry));
    public static string Refresh => Get(nameof(Refresh));
    public static string Appearance => Get(nameof(Appearance));
    public static string Language => Get(nameof(Language));
    public static string ReducedMotion => Get(nameof(ReducedMotion));
    public static string Haptics => Get(nameof(Haptics));

    public static string Get(string key) => ResourceManager.GetString(key, Culture) ?? key;
}
