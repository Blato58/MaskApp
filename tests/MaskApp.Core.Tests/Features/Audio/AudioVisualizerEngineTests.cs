using MaskApp.Core.Features.Audio;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Profiles;

namespace MaskApp.Core.Tests.Features.Audio;

public sealed class AudioVisualizerEngineTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task Start_BlocksMicrophoneUntilPhysicalEvidencePasses()
    {
        var session = await CreateSessionAsync();
        var capture = new FakeCaptureService();
        var transport = new RecordingAudioTransport();
        await using var engine = new AudioVisualizerEngine(capture, transport, session);

        var result = await engine.StartAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(AudioVisualizerEngineState.Blocked, engine.State);
        Assert.Equal(0, capture.StartCount);
        Assert.Empty(transport.Packets);
    }

    [Fact]
    public async Task PassedPerMaskEvidence_AllowsBoundedForegroundFrames_AndExplicitStop()
    {
        var session = await CreateSessionAsync();
        await session.RecordAudioVisualizationEvidenceAsync(new AudioVisualizationEvidence
        {
            Status = AudioVisualizationEvidenceStatus.Passed,
            Framing = AudioVisualizationFraming.LegacyAndroidLength,
            PackingMode = AudioVisualizationPackingMode.PaletteA,
            CharacteristicObserved = true,
            IsSimulated = false,
            PacketsAttempted = 5,
            PacketsSent = 5,
            RequestedCadenceHz = 10,
            ObservedWriteCadenceHz = 10,
            StatusText = "Physical test passed."
        });
        var capture = new FakeCaptureService();
        var transport = new RecordingAudioTransport();
        await using var engine = new AudioVisualizerEngine(capture, transport, session);
        engine.UpdateSettings(new AudioVisualizerSettings { Smoothing = 0, Sensitivity = 2 });

        var start = await engine.StartAsync();
        capture.Emit(CreateSine(512, 16_000, 220, 0.8), 16_000);
        await WaitUntilAsync(() => transport.Packets.Count > 0);
        await engine.StopAsync();

        Assert.True(start.Succeeded);
        Assert.Equal(AudioVisualizerEngineState.Stopped, engine.State);
        Assert.Equal(1, capture.StartCount);
        Assert.Equal(1, capture.StopCount);
        Assert.NotEmpty(transport.Packets);
        Assert.All(transport.Packets, packet => Assert.Equal(16, packet.EncryptedPayload.Length));
    }

    [Fact]
    public async Task CaptureInterruption_StopsWithoutAutomaticResume()
    {
        var session = await CreateSessionAsync();
        await session.RecordAudioVisualizationEvidenceAsync(new AudioVisualizationEvidence
        {
            Status = AudioVisualizationEvidenceStatus.Passed,
            CharacteristicObserved = true,
            IsSimulated = false,
            PacketsAttempted = 5,
            PacketsSent = 5,
            RequestedCadenceHz = 8,
            StatusText = "Physical test passed."
        });
        var capture = new FakeCaptureService();
        var transport = new RecordingAudioTransport();
        await using var engine = new AudioVisualizerEngine(capture, transport, session);
        Assert.True((await engine.StartAsync()).Succeeded);

        capture.Interrupt("Headset route changed.");
        await WaitUntilAsync(() => engine.State == AudioVisualizerEngineState.Stopped);

        Assert.Equal(1, capture.StartCount);
        Assert.Equal(1, capture.StopCount);
        Assert.Contains("not resume automatically", engine.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Stop_CancelsActiveNoiseCalibrationWithoutWaitingForItsTimeout()
    {
        var session = await CreateSessionAsync();
        var capture = new FakeCaptureService();
        await using var engine = new AudioVisualizerEngine(capture, new RecordingAudioTransport(), session);

        var calibration = engine.CalibrateAsync();
        await WaitUntilAsync(() => capture.State == AudioCaptureState.Capturing);
        var stop = engine.StopAsync();

        var result = await calibration.WaitAsync(TimeSpan.FromSeconds(1));
        await stop.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.False(result.Succeeded);
        Assert.Equal(AudioVisualizerEngineState.Stopped, engine.State);
        Assert.Equal(1, capture.StopCount);
        Assert.Contains("stopped", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Stop_CancelsMicrophoneStartBeforePermissionBoundaryCanComplete()
    {
        var session = await CreateSessionAsync();
        await session.RecordAudioVisualizationEvidenceAsync(new AudioVisualizationEvidence
        {
            Status = AudioVisualizationEvidenceStatus.Passed,
            CharacteristicObserved = true,
            IsSimulated = false,
            PacketsAttempted = 5,
            PacketsSent = 5,
            RequestedCadenceHz = 8,
            StatusText = "Physical test passed."
        });
        var capture = new FakeCaptureService
        {
            StartRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        await using var engine = new AudioVisualizerEngine(capture, new RecordingAudioTransport(), session);

        var start = engine.StartAsync();
        await capture.StartEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        var stop = engine.StopAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => start);
        await stop.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal(AudioVisualizerEngineState.Stopped, engine.State);
        Assert.Equal(1, capture.StopCount);
    }

    private static async Task<MaskProfileSession> CreateSessionAsync()
    {
        var session = new MaskProfileSession(new InMemoryMaskProfileStore());
        await session.ActivateAsync(new DiscoveredMaskDevice("device-a", "Test Mask", -42));
        return session;
    }

    private static float[] CreateSine(
        int length,
        int sampleRate,
        double frequency,
        double amplitude) =>
        Enumerable.Range(0, length)
            .Select(index => (float)(Math.Sin(2 * Math.PI * frequency * index / sampleRate) * amplitude))
            .ToArray();

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TestTimeout);
        while (!condition())
        {
            await Task.Delay(5, timeout.Token);
        }
    }

    private sealed class FakeCaptureService : IAudioCaptureService
    {
        public event EventHandler<AudioSamplesAvailableEventArgs>? SamplesAvailable;

        public event EventHandler<AudioCaptureStateChangedEventArgs>? StateChanged;

        public AudioCaptureState State { get; private set; }

        public string StatusText { get; private set; } = "Stopped.";

        public int StartCount { get; private set; }

        public int StopCount { get; private set; }

        public TaskCompletionSource StartEntered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource? StartRelease { get; init; }

        public async Task<AudioCaptureStartResult> StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StartCount++;
            StartEntered.TrySetResult();
            if (StartRelease is not null)
            {
                await StartRelease.Task.WaitAsync(cancellationToken);
            }

            State = AudioCaptureState.Capturing;
            StatusText = "Capturing.";
            StateChanged?.Invoke(this, new AudioCaptureStateChangedEventArgs(State, StatusText));
            return AudioCaptureStartResult.Success(StatusText);
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StopCount++;
            State = AudioCaptureState.Stopped;
            StatusText = "Stopped.";
            StateChanged?.Invoke(this, new AudioCaptureStateChangedEventArgs(State, StatusText));
            return Task.CompletedTask;
        }

        public void Emit(float[] samples, int sampleRate) =>
            SamplesAvailable?.Invoke(
                this,
                new AudioSamplesAvailableEventArgs(samples, sampleRate, DateTimeOffset.UtcNow));

        public void Interrupt(string message)
        {
            State = AudioCaptureState.Interrupted;
            StatusText = message;
            StateChanged?.Invoke(this, new AudioCaptureStateChangedEventArgs(State, StatusText));
        }
    }

    private sealed class RecordingAudioTransport : IAudioVisualizationTransport
    {
        private readonly List<AudioVisualizationPacket> packets = [];

        public event EventHandler<AudioVisualizationTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public bool IsReady => true;

        public bool IsSimulated => false;

        public AudioVisualizationTransportState State => AudioVisualizationTransportState.Ready;

        public string StatusText => "Ready.";

        public IReadOnlyList<AudioVisualizationPacket> Packets
        {
            get
            {
                lock (packets)
                {
                    return packets.ToArray();
                }
            }
        }

        public Task<AudioVisualizationSendResult> SendAsync(
            AudioVisualizationPacket packet,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (packets)
            {
                packets.Add(packet);
            }
            return Task.FromResult(AudioVisualizationSendResult.Success("Sent."));
        }
    }
}
