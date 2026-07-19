using MaskApp.App.Features.Animations;
using MaskApp.App.Features.AnimationPacks;
using MaskApp.App.Features.BuiltIns;
using MaskApp.App.Features.Faces;
using MaskApp.App.Features.Preflight;
using MaskApp.App.Features.Stage;
using MaskApp.App.Features.Text;
using MaskApp.App.Features.Device;
using MaskApp.App.Features.Library;
using MaskApp.App.Features.Live;
using MaskApp.App.Features.Onboarding;
using MaskApp.App.Features.Shows;
using MaskApp.Core.Features.Experience;
using Microsoft.Extensions.DependencyInjection;
using MaskApp.App.Resources.Strings;

namespace MaskApp.App;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Routing.RegisterRoute(AppRoutes.Onboarding, typeof(OnboardingPage));
        Routing.RegisterRoute(AppRoutes.DeckEditor, typeof(DeckEditorPage));
        Routing.RegisterRoute(AppRoutes.TextStudio, typeof(TextPage));
        Routing.RegisterRoute(AppRoutes.FaceStudio, typeof(FaceStudioPage));
        Routing.RegisterRoute(AppRoutes.AnimationStudio, typeof(AnimationStudioPage));
        Routing.RegisterRoute(AppRoutes.StockCatalog, typeof(BuiltInsPage));
        Routing.RegisterRoute(AppRoutes.ContentDetail, typeof(StockContentDetailPage));
        Routing.RegisterRoute(AppRoutes.ShowBuilder, typeof(ShowBuilderPage));
        Routing.RegisterRoute(AppRoutes.SceneEditor, typeof(SceneEditorPage));
        Routing.RegisterRoute(AppRoutes.Preflight, typeof(FestivalPreflightPage));
        Routing.RegisterRoute(AppRoutes.Stage, typeof(StageModePage));
        Routing.RegisterRoute(AppRoutes.DevicePicker, typeof(DevicePickerPage));
        Routing.RegisterRoute(AppRoutes.Diagnostics, typeof(DiagnosticsPage));
        Routing.RegisterRoute(AppRoutes.MaskPackTransfer, typeof(MaskPackPage));

        Items.Add(new TabBar
        {
            Items =
            {
                CreateShellContent<LivePage>(AppText.Live, AppRoutes.Live, "icon_pages.svg", services),
                CreateShellContent<LibraryPage>(AppText.Library, AppRoutes.Library, "icon_library.svg", services),
                CreateShellContent<ShowsPage>(AppText.Shows, AppRoutes.Shows, "icon_pages.svg", services),
                CreateShellContent<DevicePage>(AppText.Device, AppRoutes.Device, "icon_device.svg", services)
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
