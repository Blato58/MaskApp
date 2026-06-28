using MaskApp.App.Features.Connect;
using MaskApp.App.Features.BuiltIns;
using MaskApp.App.Features.Home;
using MaskApp.App.Features.React;
using MaskApp.App.Features.Rave;
using MaskApp.App.Features.Text;
using Microsoft.Extensions.DependencyInjection;

namespace MaskApp.App;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Routing.RegisterRoute("text", typeof(TextPage));

        Items.Add(new TabBar
        {
            Items =
            {
                CreateShellContent<HomePage>("Control", "control", "icon_control.svg", services),
                CreateShellContent<ReactPage>("React", "react", "icon_react.svg", services),
                CreateShellContent<RavePage>("RAVE", "rave", "icon_rave.svg", services),
                CreateShellContent<BuiltInsPage>("Faces", "builtins", "icon_builtins.svg", services),
                CreateShellContent<ConnectPage>("Connect", "connect", "icon_connect.svg", services)
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
