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
        var transport = new FakeTextUploadTransport(false, "Text upload unavailable.");
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = "A"
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.Equal("Text upload unavailable.", viewModel.StatusText);
        Assert.False(transport.WasCalled);
    }

    private sealed class FakeTextUploadTransport : ITextUploadTransport
    {
        public FakeTextUploadTransport(bool isReady, string statusText)
        {
            IsReady = isReady;
            StatusText = statusText;
        }

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => false;

        public bool IsReady { get; }

        public string StatusText { get; }

        public bool WasCalled { get; private set; }

        public Task<TextUploadResult> UploadAsync(TextUploadPackage package, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(TextUploadResult.Success("Uploaded.", package.Frames.Count));
        }
    }
}
