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
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.AnimationPacks;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Stage;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;
using MaskApp.Core.Features.Experience;
using MaskApp.Core.Features.Live;
using MaskApp.Core.Features.Shows;
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
        builder.Services.AddSingleton<ConnectViewModel>();
        builder.Services.AddTransient<DevicePage>();
        builder.Services.AddTransient<DevicePickerPage>();
        builder.Services.AddTransient<DiagnosticsPage>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<BuiltInsPage>();
        builder.Services.AddTransient<StockContentDetailPage>();
        builder.Services.AddSingleton<BuiltInsViewModel>();
        builder.Services.AddTransient<FaceStudioPage>();
        builder.Services.AddTransient<FaceStudioViewModel>();
        builder.Services.AddTransient<AnimationStudioPage>();
        builder.Services.AddTransient<AnimationStudioViewModel>();
        builder.Services.AddTransient<MaskPackPage>();
        builder.Services.AddTransient<MaskPackViewModel>();
        builder.Services.AddSingleton<SceneStudioViewModel>();
        builder.Services.AddSingleton<ShowsViewModel>();
        builder.Services.AddTransient<ShowsPage>();
        builder.Services.AddTransient<ShowBuilderPage>();
        builder.Services.AddTransient<SceneEditorPage>();
        builder.Services.AddTransient<GalleryViewModel>();
        builder.Services.AddTransient<LibraryPage>();
        builder.Services.AddSingleton<PagesViewModel>();
        builder.Services.AddSingleton<LiveViewModel>();
        builder.Services.AddTransient<LivePage>();
        builder.Services.AddTransient<DeckEditorPage>();
        builder.Services.AddTransient<MaskControlViewModel>();
        builder.Services.AddTransient<TextPage>();
        builder.Services.AddTransient<TextUploadViewModel>();
        builder.Services.AddTransient<FestivalPreflightPage>();
        builder.Services.AddSingleton<FestivalPreflightViewModel>();
        builder.Services.AddTransient<StageModePage>();
        builder.Services.AddTransient<StageModeViewModel>();
        builder.Services.AddTransient<PagesStageShowSource>();
        builder.Services.AddTransient<IStageShowSource, PerformanceStageShowSource>();
        builder.Services.AddSingleton<IStageReadinessProvider, PreflightStageReadinessProvider>();
        builder.Services.AddSingleton<MauiStageDeviceFeedback>();
        builder.Services.AddSingleton<IStageDeviceFeedback>(sp => sp.GetRequiredService<MauiStageDeviceFeedback>());
        builder.Services.AddSingleton<IStageDisplayControl, MauiStageDisplayControl>();
        builder.Services.AddSingleton<DiySlotAllocator>();
        builder.Services.AddSingleton<FlashSafetyAnalyzer>();
        builder.Services.AddSingleton<AnimationLoadAnalyzer>();
        builder.Services.AddSingleton<FestivalPreflightAnalyzer>();
        builder.Services.AddSingleton<PreflightStatusSession>();
        builder.Services.AddSingleton<FestivalShowPreparationService>();
        builder.Services.AddSingleton<QuickActionCatalog>();
        builder.Services.AddSingleton<IQuickActionTextSettingsStore, JsonQuickActionTextSettingsStore>();
        builder.Services.AddTransient<IQuickActionDispatcher, QuickActionDispatcher>();
        builder.Services.AddSingleton<ITextPresetStore, JsonTextPresetStore>();
        builder.Services.AddTransient<ITextPresetDispatcher, TextPresetDispatcher>();
        builder.Services.AddSingleton<JsonFacePatternStore>();
        builder.Services.AddSingleton<JsonMaskProfileStore>();
        builder.Services.AddSingleton<JsonFlashSafetyAcknowledgementStore>();
        builder.Services.AddSingleton<JsonAnimationProjectStore>();
        builder.Services.AddSingleton<JsonSceneShowStore>();
        builder.Services.AddSingleton<JsonMaskPackImportJournalStore>();
        builder.Services.AddSingleton<IMaskPackImportJournalStore>(sp =>
            sp.GetRequiredService<JsonMaskPackImportJournalStore>());
        builder.Services.AddSingleton<IAnimationProjectStore>(sp =>
            sp.GetRequiredService<JsonAnimationProjectStore>());
        builder.Services.AddSingleton<ISceneShowStore>(sp =>
            sp.GetRequiredService<JsonSceneShowStore>());
        builder.Services.AddSingleton<IFlashSafetyAcknowledgementStore>(sp =>
            sp.GetRequiredService<JsonFlashSafetyAcknowledgementStore>());
        builder.Services.AddSingleton<FlashSafetyAcknowledgementService>();
        builder.Services.AddSingleton<IMaskProfileStore>(sp => sp.GetRequiredService<JsonMaskProfileStore>());
        builder.Services.AddSingleton<MaskProfileSession>(sp =>
            new MaskProfileSession(
                sp.GetRequiredService<IMaskProfileStore>(),
                sp.GetRequiredService<JsonFacePatternStore>()));
        builder.Services.AddSingleton<IFacePatternStore>(sp =>
            new ProfiledFacePatternStore(
                sp.GetRequiredService<JsonFacePatternStore>(),
                sp.GetRequiredService<MaskProfileSession>()));
        builder.Services.AddSingleton<IBuiltInAssetArchiveStore, JsonBuiltInAssetArchiveStore>();
        builder.Services.AddSingleton<IBleAutoConnectSettingsStore, JsonBleAutoConnectSettingsStore>();
        builder.Services.AddSingleton<IGalleryLayoutStore, JsonGalleryLayoutStore>();
        builder.Services.AddSingleton<IAppExperienceSettingsStore, JsonAppExperienceSettingsStore>();
        builder.Services.AddSingleton<ContentCatalogQuery>();
        builder.Services.AddSingleton<SceneValidator>();
        builder.Services.AddSingleton<MaskPackArchiveService>(sp => new MaskPackArchiveService(
            sp.GetRequiredService<ITextPresetStore>(),
            sp.GetRequiredService<JsonFacePatternStore>(),
            sp.GetRequiredService<IAnimationProjectStore>(),
            sp.GetRequiredService<IGalleryLayoutStore>(),
            sp.GetRequiredService<ISceneShowStore>(),
            sp.GetRequiredService<IMaskPackImportJournalStore>()));
        builder.Services.AddSingleton<SceneReadinessEvaluator>();
        builder.Services.AddSingleton<ISceneCatalogSource, GallerySceneCatalogSource>();
        builder.Services.AddSingleton<ISceneItemDispatcher, GallerySceneItemDispatcher>();

#if IOS
        builder.Services.AddSingleton<IosMotionPreference>();
        builder.Services.AddSingleton<ExperienceMotionPreference>(sp => new ExperienceMotionPreference(
            sp.GetRequiredService<IosMotionPreference>(),
            sp.GetRequiredService<IAppExperienceSettingsStore>()));
        builder.Services.AddSingleton<IMotionPreference>(sp => sp.GetRequiredService<ExperienceMotionPreference>());
        builder.Services.AddSingleton<IosBleAdapter>();
        builder.Services.AddSingleton<IBleScanner>(sp => sp.GetRequiredService<IosBleAdapter>());
        builder.Services.AddSingleton<ProfiledBleDeviceConnection>(sp =>
        {
            var adapter = sp.GetRequiredService<IosBleAdapter>();
            return new ProfiledBleDeviceConnection(
                adapter,
                sp.GetRequiredService<MaskProfileSession>(),
                adapter,
                adapter,
                adapter);
        });
        builder.Services.AddSingleton<IBleDeviceConnection>(sp =>
            sp.GetRequiredService<ProfiledBleDeviceConnection>());
        builder.Services.AddSingleton<MaskBleScheduler>(sp =>
        {
            var adapter = sp.GetRequiredService<IosBleAdapter>();
            return new MaskBleScheduler(
                adapter,
                adapter,
                adapter,
                sp.GetRequiredService<ProfiledBleDeviceConnection>());
        });
        builder.Services.AddSingleton<IMaskCommandTransport>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<IMaskEmergencyControl>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<ITextUploadTransport>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<IFaceUploadTransport>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<IFaceImageDecoder, IosFaceImageDecoder>();
        builder.Services.AddSingleton<IAnimationMediaDecoder, IosAnimationMediaDecoder>();
#elif ANDROID
        builder.Services.AddSingleton<AndroidMotionPreference>();
        builder.Services.AddSingleton<ExperienceMotionPreference>(sp => new ExperienceMotionPreference(
            sp.GetRequiredService<AndroidMotionPreference>(),
            sp.GetRequiredService<IAppExperienceSettingsStore>()));
        builder.Services.AddSingleton<IMotionPreference>(sp => sp.GetRequiredService<ExperienceMotionPreference>());
        builder.Services.AddSingleton<AndroidBleAdapter>();
        builder.Services.AddSingleton<IBleScanner>(sp => sp.GetRequiredService<AndroidBleAdapter>());
        builder.Services.AddSingleton<ProfiledBleDeviceConnection>(sp =>
        {
            var adapter = sp.GetRequiredService<AndroidBleAdapter>();
            return new ProfiledBleDeviceConnection(
                adapter,
                sp.GetRequiredService<MaskProfileSession>(),
                adapter,
                adapter,
                adapter);
        });
        builder.Services.AddSingleton<IBleDeviceConnection>(sp =>
            sp.GetRequiredService<ProfiledBleDeviceConnection>());
        builder.Services.AddSingleton<MaskBleScheduler>(sp =>
        {
            var adapter = sp.GetRequiredService<AndroidBleAdapter>();
            return new MaskBleScheduler(
                adapter,
                adapter,
                adapter,
                sp.GetRequiredService<ProfiledBleDeviceConnection>());
        });
        builder.Services.AddSingleton<IMaskCommandTransport>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<IMaskEmergencyControl>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<ITextUploadTransport>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<IFaceUploadTransport>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<IFaceImageDecoder, AndroidFaceImageDecoder>();
        builder.Services.AddSingleton<IAnimationMediaDecoder, AndroidAnimationMediaDecoder>();
#else
        builder.Services.AddSingleton<UnavailableBleAdapter>();
        builder.Services.AddSingleton<IBleScanner>(sp => sp.GetRequiredService<UnavailableBleAdapter>());
        builder.Services.AddSingleton<SimulatedMaskCommandTransport>();
        builder.Services.AddSingleton<SimulatedTextUploadTransport>();
        builder.Services.AddSingleton<SimulatedFaceUploadTransport>();
        builder.Services.AddSingleton<ProfiledBleDeviceConnection>(sp =>
            new ProfiledBleDeviceConnection(
                sp.GetRequiredService<UnavailableBleAdapter>(),
                sp.GetRequiredService<MaskProfileSession>(),
                sp.GetRequiredService<SimulatedMaskCommandTransport>(),
                sp.GetRequiredService<SimulatedTextUploadTransport>(),
                sp.GetRequiredService<SimulatedFaceUploadTransport>()));
        builder.Services.AddSingleton<IBleDeviceConnection>(sp =>
            sp.GetRequiredService<ProfiledBleDeviceConnection>());
        builder.Services.AddSingleton<MaskBleScheduler>(sp =>
            new MaskBleScheduler(
                sp.GetRequiredService<SimulatedMaskCommandTransport>(),
                sp.GetRequiredService<SimulatedTextUploadTransport>(),
                sp.GetRequiredService<SimulatedFaceUploadTransport>(),
                sp.GetRequiredService<ProfiledBleDeviceConnection>()));
        builder.Services.AddSingleton<IMaskCommandTransport>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<IMaskEmergencyControl>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<ITextUploadTransport>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<IFaceUploadTransport>(sp => sp.GetRequiredService<MaskBleScheduler>());
        builder.Services.AddSingleton<IFaceImageDecoder, UnavailableFaceImageDecoder>();
        builder.Services.AddSingleton<IAnimationMediaDecoder, UnavailableAnimationMediaDecoder>();
#endif
        builder.Services.AddSingleton<IAnimationClock, MonotonicAnimationClock>();
        builder.Services.AddSingleton<PerformanceAnimationBuilder>();
        builder.Services.AddSingleton<AnimationProjectCompiler>();
        builder.Services.AddSingleton<AnimationMediaImportLimits>();
        builder.Services.AddSingleton<AnimationMediaImportService>();
        builder.Services.AddSingleton<PerformanceAnimationEngine>(sp =>
            new PerformanceAnimationEngine(
                sp.GetRequiredService<IMaskCommandTransport>(),
                sp.GetRequiredService<IMaskEmergencyControl>(),
                sp.GetRequiredService<IAnimationClock>(),
                sp.GetRequiredService<FlashSafetyAnalyzer>(),
                sp.GetRequiredService<IFlashSafetyAcknowledgementStore>()));
        builder.Services.AddSingleton<DiySlotPlaybackCoordinator>(sp =>
            new DiySlotPlaybackCoordinator(
                sp.GetRequiredService<IFacePatternStore>(),
                sp.GetRequiredService<IFaceUploadTransport>(),
                sp.GetRequiredService<IMaskCommandTransport>(),
                sp.GetRequiredService<PerformanceAnimationEngine>(),
                sp.GetRequiredService<PerformanceAnimationBuilder>()));
        builder.Services.AddSingleton<SceneExecutionEngine>();
        builder.Services.AddSingleton<ISceneExecutionControl>(sp =>
            sp.GetRequiredService<SceneExecutionEngine>());
        builder.Services.AddSingleton<SetlistCoordinator>();
        builder.Services.AddSingleton<BleAutoConnectCoordinator>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
