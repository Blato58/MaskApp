using MaskApp.App.Features.Connect;
using MaskApp.App.Features.Home;
using MaskApp.App.Features.Text;

namespace MaskApp.App;

public partial class AppShell : Shell
{
    public AppShell(HomePage homePage, ConnectPage connectPage, TextPage textPage)
    {
        InitializeComponent();

        Items.Add(new TabBar
        {
            Items =
            {
                new ShellContent
                {
                    Title = "Home",
                    Route = "home",
                    Content = homePage
                },
                new ShellContent
                {
                    Title = "Connect",
                    Route = "connect",
                    Content = connectPage
                },
                new ShellContent
                {
                    Title = "Text",
                    Route = "text",
                    Content = textPage
                }
            }
        });
    }
}
