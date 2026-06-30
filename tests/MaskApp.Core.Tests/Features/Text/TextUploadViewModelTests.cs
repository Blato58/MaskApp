using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.TextPresets;
using System.Collections.ObjectModel;

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
        Assert.IsNotType<ObservableCollection<TextPreviewCell>>(viewModel.PreviewCells);
        Assert.Equal(TextLayoutMode.FixedWidthCentered, viewModel.SelectedLayoutMode.LayoutMode);
        Assert.Equal(2, viewModel.SelectedAnimationMode.Mode);
        Assert.Equal(50, viewModel.Speed);
        Assert.Equal("Centered 44 columns, Blink, Speed 50, White", viewModel.ProfileSummary);
    }

    [Fact]
    public async Task SendCommand_UploadsPackageThroughSimulator()
    {
        var transport = new SimulatedTextUploadTransport();
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = "A",
            Speed = 25,
            SelectedLayoutMode = new TextLayoutModeOption("Scroll / variable width", TextLayoutMode.VariableWidth),
            SelectedAnimationMode = new TextAnimationModeOption("Scroll right-to-left", 3)
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.NotNull(transport.LastPackage);
        Assert.Equal("A", transport.LastPackage.Text);
        Assert.Equal(2, viewModel.FrameCount);
        Assert.Contains("Composer Scroll", viewModel.StatusText);
        Assert.NotEqual("None", viewModel.LastPayloadHex);
    }

    [Fact]
    public async Task SendCommand_CenteredLayoutUsesComposerCenteredPlan()
    {
        var transport = new FakeTextUploadTransport
        {
            IsReady = true,
            SupportsAcknowledgements = true,
            State = TextUploadTransportState.Ready
        };
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = "TEST",
            SelectedLayoutMode = new TextLayoutModeOption("Centered 44-column", TextLayoutMode.FixedWidthCentered),
            SelectedAnimationMode = new TextAnimationModeOption("Blink", 2),
            Speed = 100
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.True(transport.WasCalled);
        Assert.Equal("Centered 44 columns, Blink, Speed 100, White", viewModel.ProfileSummary);
        Assert.Equal(44, transport.LastPackage?.ColumnCount);
        Assert.Equal((byte)2, transport.LastPackage!.ModeCommand.Plaintext.Span[5]);
    }

    [Fact]
    public async Task InitializeAsync_DefaultsSelectedColorFromGlobalSettings()
    {
        var viewModel = new TextUploadViewModel(
            new SimulatedTextUploadTransport(),
            new InMemoryQuickActionTextSettingsStore(new QuickActionTextSettings
            {
                ForegroundPreset = QuickCaptionForegroundPreset.Purple
            }));

        await viewModel.InitializeAsync();

        Assert.Equal("Purple", viewModel.SelectedColor.Name);
        Assert.Contains("Purple", viewModel.ProfileSummary);
    }

    [Fact]
    public async Task InitializeAsync_DoesNotOverrideManualColorSelection()
    {
        var viewModel = new TextUploadViewModel(
            new SimulatedTextUploadTransport(),
            new InMemoryQuickActionTextSettingsStore(new QuickActionTextSettings
            {
                ForegroundPreset = QuickCaptionForegroundPreset.Pink
            }));
        var cyan = viewModel.TextColorOptions.Single(option => option.Name == "Cyan");

        viewModel.SelectColor(cyan);
        await viewModel.InitializeAsync();

        Assert.Equal("Cyan", viewModel.SelectedColor.Name);
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
    public void Text_ClampsToMaxLengthAndReportsCharacterCount()
    {
        var viewModel = new TextUploadViewModel(new SimulatedTextUploadTransport());
        var overLimitText = new string('A', TextUploadViewModel.MaxTextLength + 8);

        viewModel.Text = overLimitText;

        Assert.Equal(TextUploadViewModel.MaxTextLength, viewModel.Text.Length);
        Assert.Equal("64/64", viewModel.CharacterCountText);
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

    [Fact]
    public async Task Text_ReplacesPreviewListAfterDebounce()
    {
        var viewModel = new TextUploadViewModel(new SimulatedTextUploadTransport());
        var originalPreview = viewModel.PreviewCells;

        viewModel.Text = "DROP";
        var previewBeforeDebounce = viewModel.PreviewCells;

        await Task.Delay(250);

        Assert.Same(originalPreview, previewBeforeDebounce);
        Assert.NotSame(originalPreview, viewModel.PreviewCells);
        Assert.Equal(48 * 16, viewModel.PreviewCells.Count);
    }

    [Fact]
    public async Task SendCommand_ReportsUploadExceptionAsStatus()
    {
        var transport = new FakeTextUploadTransport
        {
            IsReady = true,
            SupportsAcknowledgements = true,
            State = TextUploadTransportState.Ready,
            ExceptionToThrow = new InvalidOperationException("BLE write failed")
        };
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = "DROP"
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.False(viewModel.IsSending);
        Assert.Equal("Failed: BLE write failed", viewModel.StatusText);
    }

    [Fact]
    public async Task SendCommand_ReportsCancellationAsStatus()
    {
        var transport = new FakeTextUploadTransport
        {
            IsReady = true,
            SupportsAcknowledgements = true,
            State = TextUploadTransportState.Ready,
            ExceptionToThrow = new OperationCanceledException()
        };
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = "DROP"
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.False(viewModel.IsSending);
        Assert.Equal("Text send cancelled.", viewModel.StatusText);
    }

    [Fact]
    public async Task SendCommand_TruncatesDiagnosticPayloadHex()
    {
        var transport = new FakeTextUploadTransport
        {
            IsReady = true,
            SupportsAcknowledgements = true,
            State = TextUploadTransportState.Ready
        };
        var viewModel = new TextUploadViewModel(transport)
        {
            Text = new string('W', TextUploadViewModel.MaxTextLength),
            SelectedLayoutMode = new TextLayoutModeOption("Scroll / variable width", TextLayoutMode.VariableWidth)
        };

        await viewModel.SendCommand.ExecuteAsync();

        Assert.EndsWith("...", viewModel.LastPayloadHex);
        Assert.True(viewModel.LastPayloadHex.Length <= 1027);
    }

    [Fact]
    public async Task SaveAsPresetCommand_CreatesPresetWithMaskSafeTextAndStyle()
    {
        var store = new InMemoryTextPresetStore(new TextPresetStoreState());
        var viewModel = new TextUploadViewModel(
            new SimulatedTextUploadTransport(),
            textPresetStore: store)
        {
            Text = "ČAU",
            PresetName = "Pozdrav",
            SelectedPresetCategory = TextPresetCategory.CzechBasic,
            SelectedPresetSendProfile = TextPresetSendProfile.StableFlash,
            Speed = 33
        };
        viewModel.SelectColor(viewModel.TextColorOptions.Single(option => option.Name == "Green"));

        await viewModel.SaveAsPresetCommand.ExecuteAsync();

        var state = await store.LoadAsync();
        var preset = state.Presets.Single(item => item.DisplayName == "Pozdrav");
        Assert.Equal("ČAU", preset.InputText);
        Assert.Equal("CAU", preset.MaskText);
        Assert.Equal(TextPresetCategory.CzechBasic, preset.Category);
        Assert.Equal(TextPresetSendProfile.StableFlash, preset.Style.SendProfile);
        Assert.Equal(33, preset.Style.Speed);
        Assert.Equal("Mask-safe: CAU", viewModel.MaskSafeTextWarning);
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

        public TextUploadPackage? LastPackage { get; private set; }

        public Exception? ExceptionToThrow { get; init; }

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow is not null)
            {
                return Task.FromException<TextUploadResult>(ExceptionToThrow);
            }

            WasCalled = true;
            LastOptions = options;
            LastPackage = package;
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
