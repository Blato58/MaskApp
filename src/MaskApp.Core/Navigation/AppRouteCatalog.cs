namespace MaskApp.Core.Navigation;

public static class AppRouteCatalog
{
    public const string LibraryRoot = "library";
    public const string StageRoot = "stage-hub";
    public const string DeviceRoot = "device";

    public const string Text = "text";
    public const string AudioLabs = "audio-labs";
    public const string BuiltIns = "builtins";
    public const string BuiltInDetail = "built-in-detail";
    public const string Faces = "faces";
    public const string AnimationStudio = "animation-studio";
    public const string MaskPack = "maskpack";
    public const string SceneStudio = "scene-studio";
    public const string LibraryAdd = "library-add";
    public const string PageAddItem = "page-add-item";
    public const string PagesManage = "pages-manage";
    public const string Preflight = "preflight";
    public const string Stage = "stage";

    public static IReadOnlyList<string> RootRoutes { get; } =
        [LibraryRoot, StageRoot, DeviceRoot];

    public static IReadOnlyList<string> DetailRoutes { get; } =
        [
            Text,
            AudioLabs,
            BuiltIns,
            BuiltInDetail,
            Faces,
            AnimationStudio,
            MaskPack,
            SceneStudio,
            LibraryAdd,
            PageAddItem,
            PagesManage,
            Preflight,
            Stage
        ];

    public static IReadOnlyList<string> AllRegisteredRoutes { get; } =
        [.. RootRoutes, .. DetailRoutes];

    public static string AbsoluteRoot(string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        if (!RootRoutes.Contains(route, StringComparer.Ordinal))
        {
            throw new ArgumentOutOfRangeException(nameof(route), route, "Only registered root routes can be absolute.");
        }

        return $"//{route}";
    }
}
