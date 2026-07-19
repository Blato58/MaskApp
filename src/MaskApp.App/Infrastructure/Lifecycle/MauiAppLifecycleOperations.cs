using MaskApp.Core.Features.AnimationPacks;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Audio;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Experience;
using MaskApp.Core.Features.Lifecycle;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.WatchRemote;

namespace MaskApp.App.Infrastructure.Lifecycle;

public sealed class MauiAppLifecycleOperations(IServiceProvider services) : IAppLifecycleOperations
{
    public async Task RecoverInterruptedImportAsync(CancellationToken cancellationToken)
    {
        await services.GetRequiredService<MaskPackArchiveService>()
            .RecoverInterruptedImportAsync(cancellationToken);
    }

    public async Task StartWatchRemoteAsync(CancellationToken cancellationToken)
    {
        var connectivity = services.GetRequiredService<IWatchConnectivityService>();
        await connectivity.StartAsync(cancellationToken);
        var state = await services.GetRequiredService<IWatchRemoteStateProvider>()
            .GetStateAsync(cancellationToken);
        await connectivity.PublishStateAsync(state, cancellationToken);
    }

    public void SetWatchForeground(bool isForeground) =>
        services.GetRequiredService<WatchRemoteExecutionSession>().SetForeground(isForeground);

    public void CancelSceneExecution() =>
        services.GetRequiredService<ISceneExecutionControl>().RequestCancel();

    public void CancelAudioDiagnostic() =>
        services.GetRequiredService<AudioVisualizationDiagnostic>().CancelActiveTest();

    public async Task StartForegroundAutoConnectAsync(CancellationToken cancellationToken)
    {
        var settings = await services.GetRequiredService<IAppExperienceSettingsStore>()
            .LoadAsync(cancellationToken);
        if (!settings.OnboardingCompleted)
        {
            return;
        }

        await services.GetRequiredService<BleAutoConnectCoordinator>()
            .StartForegroundAutoConnectAsync(cancellationToken);
    }

    public Task StopForegroundAutoConnectAsync(CancellationToken cancellationToken) =>
        services.GetRequiredService<BleAutoConnectCoordinator>()
            .StopForegroundAutoConnectAsync(cancellationToken);

    public Task StopAudioVisualizerAsync(CancellationToken cancellationToken) =>
        services.GetRequiredService<AudioVisualizerEngine>().StopAsync(cancellationToken);

    public async Task HandOffAnimationForBackgroundAsync(CancellationToken cancellationToken)
    {
        await services.GetRequiredService<PerformanceAnimationEngine>()
            .HandOffToMaskForBackgroundAsync(cancellationToken);
    }

    public async Task ResumeAnimationFromBackgroundAsync(CancellationToken cancellationToken)
    {
        await services.GetRequiredService<PerformanceAnimationEngine>()
            .ResumeFromBackgroundAsync(cancellationToken);
    }

    public async Task PublishWatchRemoteStateAsync(CancellationToken cancellationToken)
    {
        var state = await services.GetRequiredService<IWatchRemoteStateProvider>()
            .GetStateAsync(cancellationToken);
        await services.GetRequiredService<IWatchConnectivityService>()
            .PublishStateAsync(state, cancellationToken);
    }
}
