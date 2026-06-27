using Microsoft.Extensions.DependencyInjection;

namespace MaskApp.App;

public partial class App : Application
{
    private readonly IServiceProvider services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        this.services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            return new Window(services.GetRequiredService<AppShell>());
        }
        catch (Exception ex)
        {
            return new Window(StartupErrorPageFactory.Create("Startup failed", ex));
        }
    }
}
