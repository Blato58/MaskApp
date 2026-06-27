using MaskApp.App.Features.Connect;
using MaskApp.App.Features.Home;
using MaskApp.App.Features.React;
using MaskApp.App.Features.Rave;
using MaskApp.App.Features.Text;
using MaskApp.App.Infrastructure.Bluetooth;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Home;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.React;
using MaskApp.Core.Features.Rave;
using MaskApp.Core.Features.Text;
using Microsoft.Extensions.Logging;

namespace MaskApp.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ConnectPage>();
        builder.Services.AddTransient<ConnectViewModel>();
        builder.Services.AddTransient<MaskControlViewModel>();
        builder.Services.AddTransient<TextPage>();
        builder.Services.AddTransient<TextUploadViewModel>();
        builder.Services.AddTransient<ReactPage>();
        builder.Services.AddTransient<ReactViewModel>();
        builder.Services.AddTransient<RavePage>();
        builder.Services.AddTransient<RaveViewModel>();
        builder.Services.AddSingleton<QuickActionCatalog>();
        builder.Services.AddTransient<IQuickActionDispatcher, QuickActionDispatcher>();

#if IOS
        builder.Services.AddSingleton<IosBleAdapter>();
        builder.Services.AddSingleton<IBleScanner>(sp => sp.GetRequiredService<IosBleAdapter>());
        builder.Services.AddSingleton<IBleDeviceConnection>(sp => sp.GetRequiredService<IosBleAdapter>());
        builder.Services.AddSingleton<IMaskCommandTransport>(sp => sp.GetRequiredService<IosBleAdapter>());
        builder.Services.AddSingleton<ITextUploadTransport>(sp => sp.GetRequiredService<IosBleAdapter>());
#elif ANDROID
        builder.Services.AddSingleton<AndroidBleAdapter>();
        builder.Services.AddSingleton<IBleScanner>(sp => sp.GetRequiredService<AndroidBleAdapter>());
        builder.Services.AddSingleton<IBleDeviceConnection>(sp => sp.GetRequiredService<AndroidBleAdapter>());
        builder.Services.AddSingleton<IMaskCommandTransport>(sp => sp.GetRequiredService<AndroidBleAdapter>());
        builder.Services.AddSingleton<ITextUploadTransport>(sp => sp.GetRequiredService<AndroidBleAdapter>());
#else
        builder.Services.AddSingleton<UnavailableBleAdapter>();
        builder.Services.AddSingleton<IBleScanner>(sp => sp.GetRequiredService<UnavailableBleAdapter>());
        builder.Services.AddSingleton<IBleDeviceConnection>(sp => sp.GetRequiredService<UnavailableBleAdapter>());
        builder.Services.AddSingleton<SimulatedMaskCommandTransport>();
        builder.Services.AddSingleton<IMaskCommandTransport>(sp => sp.GetRequiredService<SimulatedMaskCommandTransport>());
        builder.Services.AddSingleton<SimulatedTextUploadTransport>();
        builder.Services.AddSingleton<ITextUploadTransport>(sp => sp.GetRequiredService<SimulatedTextUploadTransport>());
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
