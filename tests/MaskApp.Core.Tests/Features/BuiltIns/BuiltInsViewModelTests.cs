using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Tests.Features.BuiltIns;

public sealed class BuiltInsViewModelTests
{
    [Fact]
    public async Task SendCommand_DefaultMode_SendsImageCommand()
    {
        var transport = new RecordingCommandTransport();
        var viewModel = new BuiltInsViewModel(transport)
        {
            CurrentId = 2
        };

        await viewModel.SendCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(MaskCommandKind.Image, command.Kind);
        Assert.Equal(2, command.Plaintext.Span[5]);
        Assert.Contains("Needs real-mask test", viewModel.StatusText);
    }

    [Fact]
    public async Task SendCommand_AnimationMode_SendsAnimationCommand()
    {
        var transport = new RecordingCommandTransport();
        var viewModel = new BuiltInsViewModel(transport);

        await viewModel.SelectAnimationCommand.ExecuteAsync();
        viewModel.CurrentId = 3;
        await viewModel.SendCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(MaskCommandKind.Animation, command.Kind);
        Assert.Equal(3, command.Plaintext.Span[5]);
        Assert.Equal("0x03", viewModel.CurrentHexId);
        Assert.Contains("ANIM", viewModel.RangeNote);
    }

    [Fact]
    public async Task BlackoutCommand_SendsLightOne()
    {
        var transport = new RecordingCommandTransport();
        var viewModel = new BuiltInsViewModel(transport);

        await viewModel.BlackoutCommand.ExecuteAsync();

        var command = Assert.Single(transport.SentCommands);
        Assert.Equal(MaskCommandKind.Brightness, command.Kind);
        Assert.Equal(1, command.Plaintext.Span[6]);
    }

    [Fact]
    public async Task SendCommand_NotReady_DoesNotSend()
    {
        var transport = new RecordingCommandTransport
        {
            TransportState = MaskCommandTransportState.Disconnected,
            TransportStatusText = "Connect first."
        };
        var viewModel = new BuiltInsViewModel(transport);

        await viewModel.SendCommand.ExecuteAsync();

        Assert.Empty(transport.SentCommands);
        Assert.Equal("Connect first.", viewModel.StatusText);
    }

    private sealed class RecordingCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

        public string TransportDisplayName => "Recorder";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState { get; init; } = MaskCommandTransportState.Ready;

        public string TransportStatusText { get; init; } = "Ready.";

        public List<MaskCommand> SentCommands { get; } = [];

        public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default)
        {
            SentCommands.Add(command);
            return Task.FromResult(MaskCommandResult.Success($"Sent {command.DisplayName}."));
        }

        public void RaiseStateChanged(MaskCommandTransportState state, string message)
        {
            TransportStateChanged?.Invoke(this, new MaskCommandTransportStateChangedEventArgs(state, message));
        }
    }
}
