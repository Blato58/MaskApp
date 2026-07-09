using Microsoft.Extensions.DependencyInjection;
using MaskApp.Core.Features.Connect;

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
            var window = new Window(services.GetRequiredService<AppShell>());
            window.Created += (_, _) => _ = StartForegroundAutoConnectAsync();
            window.Activated += (_, _) => _ = StartForegroundAutoConnectAsync();
            return window;
        }
        catch (Exception ex)
        {
            return new Window(StartupErrorPageFactory.Create("Startup failed", ex));
        }
    }

    private async Task StartForegroundAutoConnectAsync()
    {
        try
        {
            await services.GetRequiredService<BleAutoConnectCoordinator>().StartForegroundAutoConnectAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
        }
    }
}
