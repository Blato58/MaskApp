using Microsoft.Extensions.DependencyInjection;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.AnimationPacks;
#if IOS
using UIKit;
#endif

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
            window.Created += (_, _) => _ = StartStartupServicesAsync();
            window.Activated += (_, _) => _ = StartForegroundAutoConnectAsync();
            window.Stopped += (_, _) => _ = HandOffAnimationForBackgroundAsync();
            window.Resumed += (_, _) => _ = ResumeAnimationFromBackgroundAsync();
            return window;
        }
        catch (Exception ex)
        {
            return new Window(StartupErrorPageFactory.Create("Startup failed", ex));
        }
    }

    private async Task StartStartupServicesAsync()
    {
        try
        {
            await services.GetRequiredService<MaskPackArchiveService>().RecoverInterruptedImportAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                   or InvalidDataException or InvalidOperationException)
        {
        }

        await StartForegroundAutoConnectAsync();
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

    private async Task HandOffAnimationForBackgroundAsync()
    {
#if IOS
        var iosApplication = UIApplication.SharedApplication;
        var iosBackgroundTaskId = UIApplication.BackgroundTaskInvalid;
        var iosBackgroundTaskEndRequested = 0;

        void EndIosBackgroundTask()
        {
            Interlocked.Exchange(ref iosBackgroundTaskEndRequested, 1);
            var taskId = Interlocked.Exchange(
                ref iosBackgroundTaskId,
                UIApplication.BackgroundTaskInvalid);
            if (taskId != UIApplication.BackgroundTaskInvalid)
            {
                iosApplication.EndBackgroundTask(taskId);
            }
        }

        iosBackgroundTaskId = iosApplication.BeginBackgroundTask(
            "Mask animation handoff",
            EndIosBackgroundTask);
        if (Volatile.Read(ref iosBackgroundTaskEndRequested) != 0)
        {
            EndIosBackgroundTask();
        }
#endif
        try
        {
            await services.GetRequiredService<PerformanceAnimationEngine>()
                .HandOffToMaskForBackgroundAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Animation background handoff failed: {exception}");
        }
#if IOS
        finally
        {
            EndIosBackgroundTask();
        }
#endif
    }

    private async Task ResumeAnimationFromBackgroundAsync()
    {
        try
        {
            await services.GetRequiredService<PerformanceAnimationEngine>()
                .ResumeFromBackgroundAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Animation foreground resume failed: {exception}");
        }
    }
}
