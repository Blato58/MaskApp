using MaskApp.App.Features.Connect;
using MaskApp.App.Features.Audio;
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
using MaskApp.Core.Navigation;

namespace MaskApp.App;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Routing.RegisterRoute(AppRouteCatalog.Text, typeof(TextPage));
        Routing.RegisterRoute(AppRouteCatalog.AudioLabs, typeof(AudioLabsPage));
        Routing.RegisterRoute(AppRouteCatalog.BuiltIns, typeof(BuiltInsPage));
        Routing.RegisterRoute(AppRouteCatalog.BuiltInDetail, typeof(BuiltInDetailPage));
        Routing.RegisterRoute(AppRouteCatalog.Faces, typeof(FaceStudioPage));
        Routing.RegisterRoute(AppRouteCatalog.AnimationStudio, typeof(AnimationStudioPage));
        Routing.RegisterRoute(AppRouteCatalog.MaskPack, typeof(MaskPackPage));
        Routing.RegisterRoute(AppRouteCatalog.SceneStudio, typeof(SceneStudioPage));
        Routing.RegisterRoute(AppRouteCatalog.LibraryAdd, typeof(LibraryAddPage));
        Routing.RegisterRoute(AppRouteCatalog.PageAddItem, typeof(PageAddItemPage));
        Routing.RegisterRoute(AppRouteCatalog.PagesManage, typeof(PagesPage));
        Routing.RegisterRoute(AppRouteCatalog.Preflight, typeof(FestivalPreflightPage));
        Routing.RegisterRoute(AppRouteCatalog.Stage, typeof(StageModePage));

        Items.Add(new TabBar
        {
            Items =
            {
                CreateShellContent<GalleryPage>(AppText.Library, AppRouteCatalog.LibraryRoot, "icon_library.svg", services),
                CreateShellContent<StageHubPage>(AppText.Stage, AppRouteCatalog.StageRoot, "icon_pages.svg", services),
                CreateShellContent<ConnectPage>(AppText.Device, AppRouteCatalog.DeviceRoot, "icon_device.svg", services)
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
