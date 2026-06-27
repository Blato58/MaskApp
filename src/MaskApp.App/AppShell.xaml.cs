using MaskApp.App.Features.Connect;
using MaskApp.App.Features.Home;
using MaskApp.App.Features.Text;
using Microsoft.Extensions.DependencyInjection;

namespace MaskApp.App;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        Items.Add(new TabBar
        {
            Items =
            {
                CreateShellContent<HomePage>("Home", "home", services),
                CreateShellContent<ConnectPage>("Connect", "connect", services),
                CreateShellContent<TextPage>("Text", "text", services)
            }
        });
    }

    private static ShellContent CreateShellContent<TPage>(string title, string route, IServiceProvider services)
        where TPage : Page
    {
        return new ShellContent
        {
            Title = title,
            Route = route,
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
