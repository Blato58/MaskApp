using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.QuickActions;

public sealed class QuickActionDispatcherTests
{
    [Fact]
    public async Task TriggerAsync_Blackout_SendsDimBrightnessCommand()
    {
        var commandTransport = new FakeCommandTransport();
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            commandTransport,
            new SimulatedTextUploadTransport());

        var result = await dispatcher.TriggerAsync(QuickActionId.Blackout);

        Assert.True(result.Succeeded);
        Assert.Equal("sent", result.Status);
        Assert.NotNull(commandTransport.LastCommand);
        Assert.Equal(MaskCommandKind.Brightness, commandTransport.LastCommand.Kind);
        Assert.Equal(1, commandTransport.LastCommand.Plaintext.Span[6]);
    }

    [Fact]
    public async Task TriggerAsync_TextReaction_UsesTextUploadTransport()
    {
        var textTransport = new SimulatedTextUploadTransport();
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            new FakeCommandTransport(),
            textTransport);

        var result = await dispatcher.TriggerAsync(QuickActionId.Lol);

        Assert.True(result.Succeeded);
        Assert.Equal("LOL", textTransport.LastPackage?.Text);
    }

    [Fact]
    public async Task TriggerAsync_TextReaction_ReportsUnavailableTextTransport()
    {
        var textTransport = new FakeTextUploadTransport
        {
            IsReady = false,
            StatusText = "Connect first."
        };
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            new FakeCommandTransport(),
            textTransport);

        var result = await dispatcher.TriggerAsync(QuickActionId.Lol);

        Assert.False(result.Succeeded);
        Assert.Equal("text transport not ready", result.Status);
        Assert.False(textTransport.WasCalled);
    }

    private sealed class FakeCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState { get; init; } = MaskCommandTransportState.Ready;

        public string TransportStatusText => "Fake ready.";

        public MaskCommand? LastCommand { get; private set; }

        public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default)
        {
            LastCommand = command;
            return Task.FromResult(MaskCommandResult.Success($"Sent {command.DisplayName}."));
        }
    }

    private sealed class FakeTextUploadTransport : ITextUploadTransport
    {
        public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => true;

        public bool IsReady { get; init; }

        public bool SupportsAcknowledgements { get; init; } = true;

        public TextUploadTransportState State => IsReady ? TextUploadTransportState.Ready : TextUploadTransportState.Disconnected;

        public string StatusText { get; init; } = "Ready.";

        public bool WasCalled { get; private set; }

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(TextUploadResult.Success("Uploaded.", package.Frames.Count));
        }
    }
}
