using MaskApp.Core.Features.Audio;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Tests.Features.Audio;

public sealed class AudioVisualizationDiagnosticTests
{
    [Fact]
    public async Task SuccessfulWrites_StillRequirePhysicalConfirmation()
    {
        var transport = new SimulatedAudioVisualizationTransport();
        var diagnostic = new AudioVisualizationDiagnostic(transport);

        var result = await diagnostic.RunAsync(new AudioVisualizationDiagnosticOptions
        {
            PacketInterval = TimeSpan.Zero,
            Repetitions = 1
        });

        Assert.Equal(AudioVisualizationEvidenceStatus.PendingPhysicalConfirmation, result.Evidence.Status);
        Assert.Equal(5, result.Evidence.PacketsAttempted);
        Assert.Equal(5, result.Evidence.PacketsSent);
        Assert.False(result.Evidence.EnablesLiveMicrophone);
        Assert.Contains("without an ACK", result.Evidence.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PhysicalConfirmation_DoesNotPromoteSimulatorEvidenceToMaskProof()
    {
        var pending = new AudioVisualizationEvidence
        {
            Status = AudioVisualizationEvidenceStatus.PendingPhysicalConfirmation,
            CharacteristicObserved = true,
            IsSimulated = true,
            PacketsAttempted = 5,
            PacketsSent = 5,
            RequestedCadenceHz = 8
        };

        var confirmed = AudioVisualizationDiagnostic.ConfirmPhysicalResult(pending, passed: true);

        Assert.Equal(AudioVisualizationEvidenceStatus.Passed, confirmed.Status);
        Assert.False(confirmed.EnablesLiveMicrophone);
    }

    [Fact]
    public void PassedStatus_WithoutACompleteFiniteWriteSequence_DoesNotEnableMicrophone()
    {
        var incomplete = new AudioVisualizationEvidence
        {
            Status = AudioVisualizationEvidenceStatus.Passed,
            CharacteristicObserved = true,
            IsSimulated = false,
            PacketsAttempted = 5,
            PacketsSent = 4,
            FailedWrites = 1
        };

        Assert.False(incomplete.EnablesLiveMicrophone);
    }

    [Fact]
    public async Task VisualCancellation_StopsFiniteDiagnosticBeforeAnotherPacketCanQueue()
    {
        var transport = new SignalingTransport();
        var cancellationSource = new FakeVisualWorkCancellationSource();
        using var diagnostic = new AudioVisualizationDiagnostic(transport, cancellationSource);
        var run = diagnostic.RunAsync(new AudioVisualizationDiagnosticOptions
        {
            PacketInterval = TimeSpan.FromSeconds(1),
            Repetitions = 5
        });
        await transport.FirstPacketSent.Task.WaitAsync(TimeSpan.FromSeconds(1));

        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
        Assert.Equal(1, transport.PacketsSent);
    }

    private sealed class SignalingTransport : IAudioVisualizationTransport
    {
        public event EventHandler<AudioVisualizationTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public bool IsReady => true;

        public bool IsSimulated => false;

        public AudioVisualizationTransportState State => AudioVisualizationTransportState.Ready;

        public string StatusText => "Ready.";

        public TaskCompletionSource FirstPacketSent { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int PacketsSent { get; private set; }

        public Task<AudioVisualizationSendResult> SendAsync(
            AudioVisualizationPacket packet,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PacketsSent++;
            FirstPacketSent.TrySetResult();
            return Task.FromResult(AudioVisualizationSendResult.Success("Sent."));
        }
    }

    private sealed class FakeVisualWorkCancellationSource : IVisualWorkCancellationSource
    {
        public event EventHandler<VisualWorkCancelledEventArgs>? VisualWorkCancelled;

        public void Cancel() =>
            VisualWorkCancelled?.Invoke(
                this,
                new VisualWorkCancelledEventArgs(
                    VisualWorkCancellationReason.Blackout,
                    "Blackout requested."));
    }
}
