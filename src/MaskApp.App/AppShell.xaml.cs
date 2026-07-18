using MaskApp.App.Features.Connect;
using MaskApp.App.Features.Animations;
using MaskApp.App.Features.AnimationPacks;
using MaskApp.App.Features.BuiltIns;
using MaskApp.App.Features.Faces;
using MaskApp.App.Features.Gallery;
using MaskApp.App.Features.Preflight;
using MaskApp.App.Features.Scenes;
using MaskApp.App.Features.Stage;
using MaskApp.App.Features.Text;
using Microsoft.Extensions.DependencyInjection;
using MaskApp.App.Resources.Strings;

namespace MaskApp.App;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Routing.RegisterRoute("text", typeof(TextPage));
        Routing.RegisterRoute("builtins", typeof(BuiltInsPage));
        Routing.RegisterRoute("built-in-detail", typeof(BuiltInDetailPage));
        Routing.RegisterRoute("faces", typeof(FaceStudioPage));
        Routing.RegisterRoute("animation-studio", typeof(AnimationStudioPage));
        Routing.RegisterRoute("maskpack", typeof(MaskPackPage));
        Routing.RegisterRoute("scene-studio", typeof(SceneStudioPage));
        Routing.RegisterRoute("library-add", typeof(LibraryAddPage));
        Routing.RegisterRoute("page-add-item", typeof(PageAddItemPage));
        Routing.RegisterRoute("pages-manage", typeof(PagesPage));
        Routing.RegisterRoute("preflight", typeof(FestivalPreflightPage));
        Routing.RegisterRoute("stage", typeof(StageModePage));

        Items.Add(new TabBar
        {
            Items =
            {
                CreateShellContent<GalleryPage>(AppText.Library, "library", "icon_library.svg", services),
                CreateShellContent<StageHubPage>(AppText.Stage, "stage-hub", "icon_pages.svg", services),
                CreateShellContent<ConnectPage>(AppText.Device, "device", "icon_device.svg", services)
            }
        });
    }

    private static ShellContent CreateShellContent<TPage>(
        string title,
        string route,
        string icon,
        IServiceProvider services)
        where TPage : Page
    {
        return new ShellContent
        {
            Title = title,
            Route = route,
            Icon = ImageSource.FromFile(icon),
            ContentTemplate = new DataTemplate(() => CreatePage<TPage>(title, services))
        };
    }

    private static Page CreatePage<TPage>(string title, IServiceProvider services)
        where TPage : Page
    {
        try
        {
            return services.GetRequiredService<TPage>();
        }
        catch (Exception ex)
        {
            return StartupErrorPageFactory.Create($"{title} failed", ex);
        }
    }
}
