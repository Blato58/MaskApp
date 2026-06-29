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
        var textTransport = new FakeTextUploadTransport();
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            new FakeCommandTransport(),
            textTransport);

        var result = await dispatcher.TriggerAsync(QuickActionId.Lol);

        Assert.True(result.Succeeded);
        Assert.Equal("LOL", textTransport.LastPackage?.Text);
        Assert.Equal(44, textTransport.LastPackage?.ColumnCount);
        Assert.Equal((byte)2, textTransport.LastPackage!.ModeCommand.Plaintext.Span[5]);
        Assert.Equal((byte)100, textTransport.LastPackage.SpeedCommand.Plaintext.Span[6]);
        Assert.False(textTransport.LastOptions?.AckRequired);
        Assert.True(textTransport.LastOptions?.CompatibilityWriteOnly);
        Assert.Equal(TimeSpan.FromMilliseconds(20), textTransport.LastOptions?.InterFrameDelay);
        Assert.Equal("Sent, confirm on mask", result.Message);
    }

    [Fact]
    public async Task TriggerAsync_VibeCheck_UsesTwoLineLayoutAndBlackBlankColumns()
    {
        var textTransport = new FakeTextUploadTransport();
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            new FakeCommandTransport(),
            textTransport);

        var result = await dispatcher.TriggerAsync(QuickActionId.VibeCheck);

        Assert.True(result.Succeeded);
        var package = Assert.IsType<TextUploadPackage>(textTransport.LastPackage);
        Assert.Equal("VIBE CHECK", package.Text);
        Assert.Equal(44, package.ColumnCount);
        Assert.Equal((byte)2, package.ModeCommand.Plaintext.Span[5]);
        Assert.Equal((byte)100, package.SpeedCommand.Plaintext.Span[6]);
        Assert.Equal(TimeSpan.FromMilliseconds(20), textTransport.LastOptions?.InterFrameDelay);
        Assert.True(HasLitPixelInRows(package.LedData, startRow: 0, endRow: 6));
        Assert.True(HasLitPixelInRows(package.LedData, startRow: 9, endRow: 15));
        Assert.Equal(new TextLedColor(0, 0, 0), ReadPayloadColor(package, column: 0));
        Assert.Equal(new TextLedColor(0, 0, 0), ReadPayloadColor(package, column: package.ColumnCount - 1));
    }

    [Fact]
    public async Task TriggerAsync_TextReaction_UsesReliableAckWhenConfigured()
    {
        var textTransport = new FakeTextUploadTransport();
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            new FakeCommandTransport(),
            textTransport,
            new InMemoryQuickActionTextSettingsStore(new QuickActionTextSettings
            {
                DisplayMode = QuickCaptionDisplayMode.ScrollLeftToRight,
                Speed = 44,
                SendMode = QuickCaptionSendMode.ReliableAcknowledgement
            }));

        var result = await dispatcher.TriggerAsync(QuickActionId.Drop);

        Assert.True(result.Succeeded);
        Assert.Equal((byte)4, textTransport.LastPackage!.ModeCommand.Plaintext.Span[5]);
        Assert.Equal(44, textTransport.LastPackage.ColumnCount);
        Assert.Equal((byte)44, textTransport.LastPackage.SpeedCommand.Plaintext.Span[6]);
        Assert.True(textTransport.LastOptions?.AckRequired);
        Assert.False(textTransport.LastOptions?.CompatibilityWriteOnly);
    }

    [Fact]
    public async Task TriggerAsync_TextReaction_ReliableAckReportsNotReadyWhenAckUnavailable()
    {
        var textTransport = new FakeTextUploadTransport
        {
            SupportsAcknowledgements = false
        };
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            new FakeCommandTransport(),
            textTransport,
            new InMemoryQuickActionTextSettingsStore(new QuickActionTextSettings
            {
                SendMode = QuickCaptionSendMode.ReliableAcknowledgement
            }));

        var result = await dispatcher.TriggerAsync(QuickActionId.Lol);

        Assert.False(result.Succeeded);
        Assert.Equal("Text not ready", result.Message);
        Assert.Equal("ack unavailable", result.Status);
        Assert.False(textTransport.WasCalled);
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

    [Theory]
    [InlineData(QuickActionId.TestImage1, MaskCommandKind.Image, 1)]
    [InlineData(QuickActionId.TestAnimation2, MaskCommandKind.Animation, 2)]
    public async Task TriggerAsync_BuiltInFallback_SendsCommandTransport(
        QuickActionId actionId,
        MaskCommandKind expectedKind,
        int expectedId)
    {
        var commandTransport = new FakeCommandTransport();
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            commandTransport,
            new SimulatedTextUploadTransport());

        var result = await dispatcher.TriggerAsync(actionId);

        Assert.True(result.Succeeded);
        Assert.NotNull(commandTransport.LastCommand);
        Assert.Equal(expectedKind, commandTransport.LastCommand.Kind);
        Assert.Equal(expectedId, commandTransport.LastCommand.Plaintext.Span[5]);
    }

    [Fact]
    public async Task TriggerAsync_BuiltInFallback_ReportsUnavailableCommandTransport()
    {
        var commandTransport = new FakeCommandTransport
        {
            TransportState = MaskCommandTransportState.Disconnected,
            TransportStatusText = "Connect first."
        };
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            commandTransport,
            new SimulatedTextUploadTransport());

        var result = await dispatcher.TriggerAsync(QuickActionId.TestImage1);

        Assert.False(result.Succeeded);
        Assert.Equal("command transport not ready", result.Status);
        Assert.Equal("Connect first.", result.Message);
        Assert.Null(commandTransport.LastCommand);
    }

    [Fact]
    public async Task TriggerAsync_TextReaction_LongCaptionIsCappedToFixedWidth()
    {
        var textTransport = new FakeTextUploadTransport();
        var catalog = new QuickActionCatalog();
        var dispatcher = new QuickActionDispatcher(
            catalog,
            new FakeCommandTransport(),
            textTransport);

        var result = await dispatcher.TriggerAsync(QuickActionId.TooMuchBass);

        Assert.True(result.Succeeded);
        Assert.Equal(44, textTransport.LastPackage?.ColumnCount);
        Assert.True(textTransport.LastPackage!.Text.Length < catalog.Get(QuickActionId.TooMuchBass).Caption!.Length);
    }

    [Fact]
    public async Task TriggerAsync_TextReaction_UploadExceptionReturnsFailedResult()
    {
        var textTransport = new FakeTextUploadTransport
        {
            ExceptionToThrow = new InvalidOperationException("write failed")
        };
        var dispatcher = new QuickActionDispatcher(
            new QuickActionCatalog(),
            new FakeCommandTransport(),
            textTransport);

        var result = await dispatcher.TriggerAsync(QuickActionId.Drop);

        Assert.False(result.Succeeded);
        Assert.Equal("Failed: write failed", result.Message);
        Assert.True(textTransport.WasCalled);
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

        public string TransportStatusText { get; init; } = "Fake ready.";

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

        public bool IsReady { get; init; } = true;

        public bool SupportsAcknowledgements { get; init; } = true;

        public TextUploadTransportState State => IsReady ? TextUploadTransportState.Ready : TextUploadTransportState.Disconnected;

        public string StatusText { get; init; } = "Ready.";

        public bool WasCalled { get; private set; }

        public TextUploadPackage? LastPackage { get; private set; }

        public TextUploadOptions? LastOptions { get; private set; }

        public Exception? ExceptionToThrow { get; init; }

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            LastPackage = package;
            LastOptions = options;
            if (ExceptionToThrow is not null)
            {
                return Task.FromException<TextUploadResult>(ExceptionToThrow);
            }

            return Task.FromResult(TextUploadResult.Success("Uploaded.", package.Frames.Count));
        }
    }

    private static bool HasLitPixelInRows(byte[] ledData, int startRow, int endRow)
    {
        for (var column = 0; column < ledData.Length / 2; column++)
        {
            var offset = column * 2;
            var columnBits = (ledData[offset] << 8) | ledData[offset + 1];
            for (var row = startRow; row <= endRow; row++)
            {
                if ((columnBits & (1 << (15 - row))) != 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static TextLedColor ReadPayloadColor(TextUploadPackage package, int column)
    {
        var colorOffset = package.LedData.Length + (column * 3);
        return new TextLedColor(
            package.Payload[colorOffset],
            package.Payload[colorOffset + 1],
            package.Payload[colorOffset + 2]);
    }
}
