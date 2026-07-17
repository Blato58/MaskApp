using MaskApp.App.Features.Connect;
using MaskApp.App.Features.BuiltIns;
using MaskApp.App.Features.Faces;
using MaskApp.App.Features.Gallery;
using MaskApp.App.Features.Home;
using MaskApp.App.Features.React;
using MaskApp.App.Features.Rave;
using MaskApp.App.Features.Text;
using MaskApp.App.Infrastructure.Bluetooth;
using MaskApp.App.Infrastructure.Accessibility;
using MaskApp.App.Infrastructure.Media;
using MaskApp.App.Infrastructure.Storage;
#if ANDROID
using MaskApp.App.Platforms.Android;
#endif
#if IOS
using MaskApp.App.Platforms.iOS;
#endif
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Home;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.React;
using MaskApp.Core.Features.Rave;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;
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
        builder.Services.AddTransient<BuiltInsPage>();
        builder.Services.AddTransient<BuiltInDetailPage>();
        builder.Services.AddSingleton<BuiltInsViewModel>();
        builder.Services.AddTransient<FaceStudioPage>();
        builder.Services.AddTransient<FaceStudioViewModel>();
        builder.Services.AddTransient<GalleryPage>();
        builder.Services.AddTransient<GalleryViewModel>();
        builder.Services.AddTransient<LibraryAddPage>();
        builder.Services.AddTransient<PagesPage>();
        builder.Services.AddTransient<PagesViewModel>();
        builder.Services.AddTransient<PageAddItemPage>();
        builder.Services.AddTransient<PageAddItemViewModel>();
        builder.Services.AddTransient<MaskControlViewModel>();
        builder.Services.AddTransient<TextPage>();
        builder.Services.AddTransient<TextUploadViewModel>();
        builder.Services.AddTransient<ReactPage>();
        builder.Services.AddTransient<ReactViewModel>();
        builder.Services.AddTransient<RavePage>();
        builder.Services.AddTransient<RaveViewModel>();
        builder.Services.AddSingleton<QuickActionCatalog>();
        builder.Services.AddSingleton<IQuickActionTextSettingsStore, JsonQuickActionTextSettingsStore>();
        builder.Services.AddTransient<IQuickActionDispatcher, QuickActionDispatcher>();
        builder.Services.AddSingleton<ITextPresetStore, JsonTextPresetStore>();
        builder.Services.AddTransient<ITextPresetDispatcher, TextPresetDispatcher>();
        builder.Services.AddSingleton<IFacePatternStore, JsonFacePatternStore>();
        builder.Services.AddSingleton<IBuiltInAssetArchiveStore, JsonBuiltInAssetArchiveStore>();
        builder.Services.AddSingleton<IBleAutoConnectSettingsStore, JsonBleAutoConnectSettingsStore>();
        builder.Services.AddSingleton<IGalleryLayoutStore, JsonGalleryLayoutStore>();

#if IOS
        builder.Services.AddSingleton<IMotionPreference, IosMotionPreference>();
        builder.Services.AddSingleton<IosBleAdapter>();
        builder.Services.AddSingleton<IBleScanner>(sp => sp.GetRequiredService<IosBleAdapter>());
        builder.Services.AddSingleton<IBleDeviceConnection>(sp => sp.GetRequiredService<IosBleAdapter>());
        builder.Services.AddSingleton<IMaskCommandTransport>(sp => sp.GetRequiredService<IosBleAdapter>());
        builder.Services.AddSingleton<ITextUploadTransport>(sp =>
            new SerializedTextUploadTransport(sp.GetRequiredService<IosBleAdapter>()));
        builder.Services.AddSingleton<IFaceUploadTransport>(sp =>
            new SerializedFaceUploadTransport(sp.GetRequiredService<IosBleAdapter>()));
        builder.Services.AddSingleton<IFaceImageDecoder, IosFaceImageDecoder>();
#elif ANDROID
        builder.Services.AddSingleton<IMotionPreference, AndroidMotionPreference>();
        builder.Services.AddSingleton<AndroidBleAdapter>();
        builder.Services.AddSingleton<IBleScanner>(sp => sp.GetRequiredService<AndroidBleAdapter>());
        builder.Services.AddSingleton<IBleDeviceConnection>(sp => sp.GetRequiredService<AndroidBleAdapter>());
        builder.Services.AddSingleton<IMaskCommandTransport>(sp => sp.GetRequiredService<AndroidBleAdapter>());
        builder.Services.AddSingleton<ITextUploadTransport>(sp =>
            new SerializedTextUploadTransport(sp.GetRequiredService<AndroidBleAdapter>()));
        builder.Services.AddSingleton<IFaceUploadTransport>(sp =>
            new SerializedFaceUploadTransport(sp.GetRequiredService<AndroidBleAdapter>()));
        builder.Services.AddSingleton<IFaceImageDecoder, AndroidFaceImageDecoder>();
#else
        builder.Services.AddSingleton<UnavailableBleAdapter>();
        builder.Services.AddSingleton<IBleScanner>(sp => sp.GetRequiredService<UnavailableBleAdapter>());
        builder.Services.AddSingleton<IBleDeviceConnection>(sp => sp.GetRequiredService<UnavailableBleAdapter>());
        builder.Services.AddSingleton<SimulatedMaskCommandTransport>();
        builder.Services.AddSingleton<IMaskCommandTransport>(sp => sp.GetRequiredService<SimulatedMaskCommandTransport>());
        builder.Services.AddSingleton<SimulatedTextUploadTransport>();
        builder.Services.AddSingleton<ITextUploadTransport>(sp =>
            new SerializedTextUploadTransport(sp.GetRequiredService<SimulatedTextUploadTransport>()));
        builder.Services.AddSingleton<SimulatedFaceUploadTransport>();
        builder.Services.AddSingleton<IFaceUploadTransport>(sp =>
            new SerializedFaceUploadTransport(sp.GetRequiredService<SimulatedFaceUploadTransport>()));
        builder.Services.AddSingleton<IFaceImageDecoder, UnavailableFaceImageDecoder>();
#endif
        builder.Services.AddSingleton<DiySlotPlaybackCoordinator>();
        builder.Services.AddSingleton<BleAutoConnectCoordinator>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
