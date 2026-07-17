using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Tests.Features.MaskControl;

public sealed class MaskControlViewModelTests
{
    [Fact]
    public async Task ApplyBrightnessCommand_SendsClampedBrightnessAndUpdatesPreview()
    {
        var transport = new SimulatedMaskCommandTransport();
        var viewModel = new MaskControlViewModel(transport)
        {
            Brightness = 200
        };

        await viewModel.ApplyBrightnessCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(MaskCommandKind.Brightness, command.Kind);
        Assert.Equal(100, viewModel.Brightness);
        Assert.Equal(100, viewModel.PreviewBrightness);
        Assert.Equal(1d, viewModel.PreviewOpacity);
        Assert.False(viewModel.IsDimmed);
        Assert.Equal("Brightness: Brightness 100%", viewModel.LastCommandText);
        Assert.Equal(Convert.ToHexString(command.EncryptedPayload.Span), viewModel.LastPayloadHex);
        Assert.Equal("Simulated Brightness 100%.", viewModel.LastTransportStatusText);
    }

    [Fact]
    public async Task TogglePowerCommand_DimsThenRestoresPreviousBrightness()
    {
        var transport = new SimulatedMaskCommandTransport();
        var viewModel = new MaskControlViewModel(transport)
        {
            Brightness = 72
        };

        await viewModel.TogglePowerCommand.ExecuteAsync();
        await viewModel.TogglePowerCommand.ExecuteAsync();

        Assert.Equal(2, transport.SentCommands.Count);
        Assert.Equal(72, viewModel.Brightness);
        Assert.Equal(72, viewModel.PreviewBrightness);
        Assert.False(viewModel.IsDimmed);
        Assert.Equal("Dim", viewModel.PowerButtonText);
    }

    [Fact]
    public async Task BlackoutAndRestoreCommands_UsePreviousBrightness()
    {
        var transport = new SimulatedMaskCommandTransport();
        var viewModel = new MaskControlViewModel(transport)
        {
            Brightness = 74
        };

        await viewModel.BlackoutCommand.ExecuteAsync();
        await viewModel.RestoreBrightnessCommand.ExecuteAsync();

        Assert.Equal(2, transport.SentCommands.Count);
        Assert.Equal(1, transport.SentCommands[0].Plaintext.Span[6]);
        Assert.Equal(74, transport.SentCommands[1].Plaintext.Span[6]);
        Assert.Equal(74, viewModel.Brightness);
        Assert.False(viewModel.IsDimmed);
    }

    [Fact]
    public async Task BrightnessPresetCommands_SendPresetLevels()
    {
        var transport = new SimulatedMaskCommandTransport();
        var viewModel = new MaskControlViewModel(transport);

        await viewModel.SetBrightness25Command.ExecuteAsync();
        await viewModel.SetBrightness75Command.ExecuteAsync();

        Assert.Equal(2, transport.SentCommands.Count);
        Assert.Equal(25, transport.SentCommands[0].Plaintext.Span[6]);
        Assert.Equal(75, transport.SentCommands[1].Plaintext.Span[6]);
        Assert.Equal(75, viewModel.Brightness);
        Assert.Equal(75, viewModel.PreviewBrightness);
        Assert.False(viewModel.IsDimmed);
    }

    [Fact]
    public async Task ApplyPlaybackSpeedCommand_SendsSelectedSpeed()
    {
        var transport = new SimulatedMaskCommandTransport();
        var viewModel = new MaskControlViewModel(transport)
        {
            PlaybackSpeed = 40
        };

        await viewModel.ApplyPlaybackSpeedCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(MaskCommandKind.AnimationSpeed, command.Kind);
        Assert.Equal(40, command.Plaintext.Span[6]);
        Assert.Equal(40, viewModel.PlaybackSpeed);
        Assert.Equal("AnimationSpeed: Animation speed 40", viewModel.LastCommandText);
    }

    [Fact]
    public async Task PlaybackSpeedPresetCommands_SendPresetLevels()
    {
        var transport = new SimulatedMaskCommandTransport();
        var viewModel = new MaskControlViewModel(transport);

        await viewModel.SetPlaybackSpeed25Command.ExecuteAsync();
        await viewModel.SetPlaybackSpeed100Command.ExecuteAsync();

        Assert.Equal(2, transport.SentCommands.Count);
        Assert.Equal(25, transport.SentCommands[0].Plaintext.Span[6]);
        Assert.Equal(100, transport.SentCommands[1].Plaintext.Span[6]);
        Assert.Equal(100, viewModel.PlaybackSpeed);
    }

    [Fact]
    public async Task EffectPresetCommand_UpdatesCurrentEffectAfterSuccess()
    {
        var transport = new SimulatedMaskCommandTransport();
        var viewModel = new MaskControlViewModel(transport);
        var preset = viewModel.EffectPresets.Single(effect => effect.Name == "Animation 1");

        await preset.ApplyCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(MaskCommandKind.Animation, command.Kind);
        Assert.Equal("Animation 1", viewModel.CurrentEffectName);
    }

    [Fact]
    public void SimulatedTransport_IsReportedInDiagnostics()
    {
        var transport = new SimulatedMaskCommandTransport();
        var viewModel = new MaskControlViewModel(transport);

        Assert.Equal("Simulator (simulated)", viewModel.ActiveTransportText);
        Assert.Equal("None", viewModel.LastCommandText);
        Assert.Equal("None", viewModel.LastPayloadHex);
        Assert.Equal("Simulator ready.", viewModel.LastTransportStatusText);
    }

    [Fact]
    public async Task FailedCommand_DoesNotUpdatePreviewState()
    {
        var transport = new FakeMaskCommandTransport(MaskCommandResult.Failure("Write failed."));
        var viewModel = new MaskControlViewModel(transport)
        {
            Brightness = 80
        };

        await viewModel.ApplyBrightnessCommand.ExecuteAsync();

        Assert.Equal(60, viewModel.PreviewBrightness);
        Assert.Equal(MaskCommandTransportState.Failed, viewModel.TransportState);
        Assert.Equal("Write failed.", viewModel.StatusText);
        Assert.Equal("Write failed.", viewModel.TransportStatusText);
        Assert.Equal("Brightness: Brightness 80%", viewModel.LastCommandText);
        Assert.Equal(Convert.ToHexString(MaskCommandBuilder.Brightness(80).EncryptedPayload.Span), viewModel.LastPayloadHex);
        Assert.Equal("Write failed.", viewModel.LastTransportStatusText);
    }

    [Fact]
    public async Task NotReadyTransport_DoesNotSendCommand()
    {
        var transport = new FakeMaskCommandTransport(MaskCommandResult.Success("Should not send."), MaskCommandTransportState.Disconnected);
        var viewModel = new MaskControlViewModel(transport);

        await viewModel.ApplyBrightnessCommand.ExecuteAsync();

        Assert.Equal(0, transport.SentCommandCount);
        Assert.Equal("Mask controls are not ready.", viewModel.StatusText);
        Assert.Equal(60, viewModel.PreviewBrightness);
        Assert.Equal("None", viewModel.LastCommandText);
        Assert.Equal("None", viewModel.LastPayloadHex);
        Assert.Equal("Mask controls are not ready.", viewModel.LastTransportStatusText);
    }

    private sealed class FakeMaskCommandTransport : IMaskCommandTransport
    {
        private readonly MaskCommandResult result;

        public FakeMaskCommandTransport(MaskCommandResult result, MaskCommandTransportState state = MaskCommandTransportState.Ready)
        {
            this.result = result;
            TransportState = state;
        }

        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

        public string TransportDisplayName => "Fake transport";

        public bool IsSimulated => false;

        public MaskCommandTransportState TransportState { get; }

        public string TransportStatusText => TransportState == MaskCommandTransportState.Ready ? "Ready." : "Disconnected.";

        public int SentCommandCount { get; private set; }

        public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default)
        {
            SentCommandCount++;
            return Task.FromResult(result);
        }

        public void RaiseStateChanged(MaskCommandTransportState state, string message)
        {
            TransportStateChanged?.Invoke(this, new MaskCommandTransportStateChangedEventArgs(state, message));
        }
    }
}
