using Microsoft.Extensions.DependencyInjection;
using MaskApp.App.Resources.Strings;
using MaskApp.Core.Features.Lifecycle;
using MaskApp.Core.Features.Profiles;
using System.Globalization;
#if IOS
using UIKit;
#endif

namespace MaskApp.App;

public partial class App : Application
{
    private readonly IServiceProvider services;
    private readonly MaskProfileMetricsRecorder profileMetricsRecorder;
    private readonly AppLifecycleCoordinator lifecycleCoordinator;

    public App(
        IServiceProvider services,
        MaskProfileMetricsRecorder profileMetricsRecorder,
        AppLifecycleCoordinator lifecycleCoordinator)
    {
        AppText.Culture = string.Equals(
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            "cs",
            StringComparison.OrdinalIgnoreCase)
            ? CultureInfo.GetCultureInfo("cs-CZ")
            : CultureInfo.GetCultureInfo("en");
        InitializeComponent();
        this.services = services;
        this.profileMetricsRecorder = profileMetricsRecorder;
        this.lifecycleCoordinator = lifecycleCoordinator;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            var window = new Window(services.GetRequiredService<AppShell>());
            window.Created += (_, _) => _ = lifecycleCoordinator.OnCreatedAsync();
            window.Activated += (_, _) => _ = lifecycleCoordinator.OnActivatedAsync();
            window.Stopped += (_, _) => _ = HandleStoppedAsync();
            window.Resumed += (_, _) => _ = lifecycleCoordinator.OnResumedAsync();
            return window;
        }
        catch (Exception ex)
        {
            return new Window(StartupErrorPageFactory.Create("Startup failed", ex));
        }
    }

    private async Task HandleStoppedAsync()
    {
        var backgroundWorkCancellationToken = CancellationToken.None;
#if IOS
        var iosApplication = UIApplication.SharedApplication;
        var iosBackgroundTaskId = UIApplication.BackgroundTaskInvalid;
        var iosBackgroundTaskEndRequested = 0;
        using var backgroundWorkCancellation = new CancellationTokenSource();
        backgroundWorkCancellationToken = backgroundWorkCancellation.Token;

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

        void ExpireIosBackgroundTask()
        {
            try
            {
                backgroundWorkCancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            EndIosBackgroundTask();
        }

        iosBackgroundTaskId = iosApplication.BeginBackgroundTask(
            "Mask animation handoff",
            ExpireIosBackgroundTask);
        if (Volatile.Read(ref iosBackgroundTaskEndRequested) != 0)
        {
            EndIosBackgroundTask();
        }
#endif
        try
        {
            await lifecycleCoordinator.OnStoppedAsync(backgroundWorkCancellationToken);
        }
        finally
        {
#if IOS
            EndIosBackgroundTask();
#endif
        }
    }
}
