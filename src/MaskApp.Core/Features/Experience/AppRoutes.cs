namespace MaskApp.Core.Features.Experience;

public static class AppRoutes
{
    public const string Back = "..";
    public const string Live = "live";
    public const string Library = "library";
    public const string Shows = "shows";
    public const string Device = "device";

    public const string LiveRoot = "//live";
    public const string LibraryRoot = "//library";
    public const string ShowsRoot = "//shows";
    public const string DeviceRoot = "//device";

    public const string Onboarding = "onboarding";
    public const string DeckEditor = "deck-editor";
    public const string TextStudio = "text-studio";
    public const string FaceStudio = "face-studio";
    public const string AnimationStudio = "animation-studio";
    public const string StockCatalog = "stock-catalog";
    public const string ContentDetail = "content-detail";
    public const string ShowBuilder = "show-builder";
    public const string SceneEditor = "scene-editor";
    public const string Preflight = "preflight";
    public const string Stage = "stage";
    public const string DevicePicker = "device-picker";
    public const string Diagnostics = "diagnostics";
    public const string MaskPackTransfer = "maskpack-transfer";

    public static IReadOnlySet<string> DeprecatedRoutes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "connect",
        "stage-hub",
        "text",
        "builtins",
        "built-in-detail",
        "faces",
        "maskpack",
        "scene-studio",
        "library-add",
        "page-add-item",
        "pages-manage"
    };

    public static string ForDeckEditor(string? deckId = null) => WithOptionalQuery(DeckEditor, "deckId", deckId);

    public static string ForTextStudio(string? presetId = null) => WithOptionalQuery(TextStudio, "presetId", presetId);

    public static string ForFaceStudio(string? faceId = null) => WithOptionalQuery(FaceStudio, "faceId", faceId);

    public static string ForAnimationStudio(string? projectId = null) => WithOptionalQuery(AnimationStudio, "projectId", projectId);

    public static string ForShowBuilder(string? showId = null) => WithOptionalQuery(ShowBuilder, "showId", showId);

    public static string ForSceneEditor(string? sceneId = null) => WithOptionalQuery(SceneEditor, "sceneId", sceneId);

    public static string ForStockDetail(string type, int id) =>
        $"{ContentDetail}?source=stock&type={Escape(type)}&id={id}";

    public static string ForPreflight(string scope, string? sourceId = null)
    {
        var route = $"{Preflight}?scope={Escape(scope)}";
        return string.IsNullOrWhiteSpace(sourceId)
            ? route
            : $"{route}&sourceId={Escape(sourceId)}";
    }

    public static string ForMaskPackTransfer(string mode) =>
        $"{MaskPackTransfer}?mode={Escape(mode)}";

    private static string WithOptionalQuery(string route, string key, string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? route
            : $"{route}?{key}={Escape(value)}";

    private static string Escape(string value) => Uri.EscapeDataString(value.Trim());
}
