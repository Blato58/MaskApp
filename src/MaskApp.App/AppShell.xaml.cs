using MaskApp.App.Features.Connect;
using MaskApp.App.Features.BuiltIns;
using MaskApp.App.Features.Gallery;
using MaskApp.App.Features.Text;
using Microsoft.Extensions.DependencyInjection;

namespace MaskApp.App;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Routing.RegisterRoute("text", typeof(TextPage));
        Routing.RegisterRoute("builtins", typeof(BuiltInsPage));
        Routing.RegisterRoute("library-add", typeof(LibraryAddPage));
        Routing.RegisterRoute("page-add-item", typeof(PageAddItemPage));

        Items.Add(new TabBar
        {
            Items =
            {
                CreateShellContent<GalleryPage>("Library", "library", "icon_library.svg", services),
                CreateShellContent<PagesPage>("Pages", "pages", "icon_pages.svg", services),
                CreateShellContent<ConnectPage>("Device", "device", "icon_device.svg", services)
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
