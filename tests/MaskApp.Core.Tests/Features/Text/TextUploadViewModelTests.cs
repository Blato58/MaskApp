using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Text;

public sealed class TextUploadViewModelTests
{
    [Fact]
    public void Constructor_BuildsInitialPreviewAndDiagnostics()
    {
        var viewModel = new TextUploadViewModel(new SimulatedTextUploadTransport());

        Assert.Equal(48 * 16, viewModel.PreviewCells.Count);
        Assert.True(viewModel.ColumnCount > 0);
        Assert.Equal("Simulator (simulated)", viewModel.ActiveTransportText);
        Assert.True(viewModel.SupportsAcknowledgements);
        Assert.Equal(TextUploadTransportState.Simulated, viewModel.TransportState);
    }

    [Fact]
    public async Task SendCommand_UploadsPackageThroughSimulator()
    {
        var transport = new SimulatedTextUploadTransport();
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = "A",
            Speed = 25,
            SelectedAnimationMode = new TextAnimationModeOption("Scroll right", 2)
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.NotNull(transport.LastPackage);
        Assert.Equal("A", transport.LastPackage.Text);
        Assert.Equal(2, viewModel.FrameCount);
        Assert.Contains("Simulated text upload complete", viewModel.StatusText);
        Assert.NotEqual("None", viewModel.LastPayloadHex);
    }

    [Fact]
    public async Task SendCommand_RejectsEmptyText()
    {
        var transport = new SimulatedTextUploadTransport();
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = " "
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.Null(transport.LastPackage);
        Assert.Equal("Enter text before sending.", viewModel.StatusText);
    }

    [Fact]
    public async Task SendCommand_ReportsUnavailableTransport()
    {
        var transport = new FakeTextUploadTransport
        {
            IsReady = false,
            StatusText = "Text upload unavailable.",
            State = TextUploadTransportState.Disconnected
        };
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = "A"
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.Equal("Text upload unavailable.", viewModel.StatusText);
        Assert.False(transport.WasCalled);
    }

    [Fact]
    public void TransportStateChanged_RefreshesReadinessAndStatus()
    {
        var transport = new FakeTextUploadTransport
        {
            IsReady = false,
            SupportsAcknowledgements = false,
            State = TextUploadTransportState.Disconnected,
            StatusText = "Connect to a mask."
        };
        var viewModel = new TextUploadViewModel(transport);

        transport.RaiseStateChanged(
            TextUploadTransportState.Ready,
            "Text upload ready with ACK confirmation.",
            supportsAcknowledgements: true,
            isReady: true);

        Assert.True(viewModel.SupportsAcknowledgements);
        Assert.Equal(TextUploadTransportState.Ready, viewModel.TransportState);
        Assert.Equal("Text upload ready with ACK confirmation.", viewModel.StatusText);
        Assert.Contains("ACK required", viewModel.AcknowledgementModeText);
    }

    [Fact]
    public async Task SendCommand_UsesAckRequiredOptionsWhenAckIsSupported()
    {
        var transport = new FakeTextUploadTransport
        {
            IsReady = true,
            SupportsAcknowledgements = true,
            State = TextUploadTransportState.Ready,
            StatusText = "Text upload ready with ACK confirmation."
        };
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = "ACK"
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.True(transport.WasCalled);
        Assert.NotNull(transport.LastOptions);
        Assert.True(transport.LastOptions.AckRequired);
        Assert.False(transport.LastOptions.CompatibilityWriteOnly);
    }

    [Fact]
    public async Task SendCommand_UsesWriteOnlyCompatibilityWhenAckIsMissing()
    {
        var transport = new FakeTextUploadTransport
        {
            IsReady = true,
            SupportsAcknowledgements = false,
            State = TextUploadTransportState.CompatibilityReady,
            StatusText = "Text upload write-only compatibility ready."
        };
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = "NO ACK"
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.True(viewModel.UseCompatibilityWriteOnly);
        Assert.True(transport.WasCalled);
        Assert.NotNull(transport.LastOptions);
        Assert.False(transport.LastOptions.AckRequired);
        Assert.True(transport.LastOptions.CompatibilityWriteOnly);
    }

    private sealed class FakeTextUploadTransport : ITextUploadTransport
    {
        public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged;

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => false;

        public bool IsReady { get; set; }

        public bool SupportsAcknowledgements { get; set; } = true;

        public TextUploadTransportState State { get; set; } = TextUploadTransportState.Ready;

        public string StatusText { get; set; } = "Ready.";

        public bool WasCalled { get; private set; }

        public TextUploadOptions? LastOptions { get; private set; }

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            LastOptions = options;
            return Task.FromResult(TextUploadResult.Success("Uploaded.", package.Frames.Count));
        }

        public void RaiseStateChanged(
            TextUploadTransportState state,
            string message,
            bool supportsAcknowledgements,
            bool isReady)
        {
            State = state;
            StatusText = message;
            SupportsAcknowledgements = supportsAcknowledgements;
            IsReady = isReady;
            StateChanged?.Invoke(
                this,
                new TextUploadTransportStateChangedEventArgs(
                    state,
                    message,
                    supportsAcknowledgements,
                    isReady));
        }
    }
}
