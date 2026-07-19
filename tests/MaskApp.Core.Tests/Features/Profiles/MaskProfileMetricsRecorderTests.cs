using MaskApp.Core.Features.Audio;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Profiles;

public sealed class MaskProfileMetricsRecorderTests
{
    [Fact]
    public async Task SuccessfulScheduledOperation_RecordsLatencyForActiveMask()
    {
        var store = new InMemoryMaskProfileStore();
        var session = new MaskProfileSession(store);
        await session.ActivateAsync(new DiscoveredMaskDevice("device-a", "Mask A", -40));
        await using var scheduler = new MaskBleScheduler(
            new SimulatedMaskCommandTransport(),
            new SimulatedTextUploadTransport(),
            new SimulatedFaceUploadTransport());
        using var recorder = new MaskProfileMetricsRecorder(scheduler, session);

        var result = await scheduler.SendAsync(MaskCommandBuilder.Brightness(50));
        await WaitUntilAsync(() => scheduler.GetSnapshot().TotalCompleted == 1);
        await recorder.UpdateTask.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.Succeeded);
        var profile = await session.GetActiveProfileAsync();
        Assert.True(profile!.AverageCommandLatencyMilliseconds > 0);
        Assert.Equal(string.Empty, recorder.LastError);
    }

    [Fact]
    public async Task StaleProfileMeasurement_IsNotWrittenToNewActiveMask()
    {
        var store = new InMemoryMaskProfileStore();
        var session = new MaskProfileSession(store);
        var first = await session.ActivateAsync(new DiscoveredMaskDevice("device-a", "Mask A", -40));
        await session.ActivateAsync(new DiscoveredMaskDevice("device-b", "Mask B", -45));

        var result = await session.RecordCommandLatencyForProfileAsync(
            first.ProfileId,
            TimeSpan.FromMilliseconds(25));

        Assert.Null(result);
        Assert.Null((await session.GetActiveProfileAsync())!.AverageCommandLatencyMilliseconds);
    }

    [Fact]
    public async Task PhysicallyConfirmedAudioEvidence_RecordsSustainableCadence()
    {
        var session = new MaskProfileSession(new InMemoryMaskProfileStore());
        await session.ActivateAsync(new DiscoveredMaskDevice("device-a", "Mask A", -40));
        var evidence = new AudioVisualizationEvidence
        {
            Status = AudioVisualizationEvidenceStatus.Passed,
            CharacteristicObserved = true,
            IsSimulated = false,
            PacketsAttempted = 10,
            PacketsSent = 10,
            RequestedCadenceHz = 8,
            ObservedWriteCadenceHz = 7.5,
            TestedAt = DateTimeOffset.UtcNow,
            StatusText = "Physical sequence passed."
        };

        var profile = await session.RecordAudioVisualizationEvidenceAsync(evidence);

        Assert.Equal(7.5, profile!.SustainableCadenceHz);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!condition())
        {
            await Task.Delay(10, timeout.Token);
        }
    }
}
