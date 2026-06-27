using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Rave;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Rave;

public sealed class RaveViewModelTests
{
    [Fact]
    public void Actions_ExposeManualRaveFastMvpDeck()
    {
        var viewModel = CreateViewModel();

        var labels = viewModel.Actions.Select(action => action.Label).ToArray();

        Assert.Equal(
            [
                "DROP",
                "WHEEL UP",
                "RELOAD",
                "BOH",
                "PULL UP",
                "RUN IT BACK",
                "BASS FACE",
                "HYDRATE",
                "WATER?",
                "ALL GOOD?",
                "NICE MOVES",
                "VIBE CHECK",
                "NO THOUGHTS",
                "WHERE WATER?",
                "I LIVE HERE",
                "TOO MUCH BASS"
            ],
            labels);
        Assert.Contains("no detector", viewModel.ModeStatusText);
        Assert.Contains("no microphone", viewModel.ModeStatusText);
        Assert.Contains("no AI", viewModel.ModeStatusText);
    }

    [Fact]
    public async Task ManualAction_SendsShortCaptionThroughTextTransport()
    {
        var dispatcher = new RecordingQuickActionDispatcher();
        var viewModel = CreateViewModel(dispatcher: dispatcher);

        await viewModel.Actions.Single(action => action.Label == "DROP").SendCommand.ExecuteAsync();

        Assert.Equal(QuickActionId.Drop, dispatcher.LastActionId);
        Assert.Equal("DROP", viewModel.LastActionText);
        Assert.Contains("Sent.", viewModel.SendStatusText);
        Assert.Equal("sent", viewModel.LastPayloadText);
    }

    [Fact]
    public async Task ManualAction_UsesWriteOnlyCompatibilityWhenAcknowledgementsAreUnavailable()
    {
        var dispatcher = new RecordingQuickActionDispatcher();
        var textTransport = new CapturingTextUploadTransport(supportsAcknowledgements: false);
        var viewModel = CreateViewModel(dispatcher: dispatcher, textTransport: textTransport);

        await viewModel.Actions.Single(action => action.Label == "WHEEL UP").SendCommand.ExecuteAsync();

        Assert.Equal(QuickActionId.WheelUp, dispatcher.LastActionId);
        Assert.Contains("without ACK", viewModel.SendStatusText);
    }

    [Fact]
    public async Task BlackoutAndRestore_SendBrightnessCommands()
    {
        var dispatcher = new RecordingQuickActionDispatcher();
        var viewModel = CreateViewModel(dispatcher: dispatcher);
        viewModel.BrightnessCap = 72;

        await viewModel.BlackoutCommand.ExecuteAsync();
        await viewModel.RestoreCommand.ExecuteAsync();

        Assert.Equal([QuickActionId.Blackout, QuickActionId.SetBrightness], dispatcher.ActionIds);
        Assert.Equal(72, dispatcher.Requests[1]?.Brightness);
    }

    [Fact]
    public async Task CommandFallbackAction_UsesCommandOnlyQuickAction()
    {
        var dispatcher = new RecordingQuickActionDispatcher();
        var viewModel = CreateViewModel(dispatcher: dispatcher);

        await viewModel.CommandFallbackActions.Single(action => action.Id == QuickActionId.TestAnimation1)
            .SendCommand.ExecuteAsync();

        Assert.Equal(QuickActionId.TestAnimation1, dispatcher.LastActionId);
        Assert.Equal("Test Anim 1", viewModel.LastActionText);
        Assert.Contains("Needs real-mask test", viewModel.SendStatusText);
    }

    [Fact]
    public async Task ManualAction_WriteOnlyFailure_DoesNotReportSent()
    {
        var dispatcher = new RecordingQuickActionDispatcher(QuickActionResult.Failed(QuickActionId.WheelUp, "Upload failed."));
        var textTransport = new CapturingTextUploadTransport(supportsAcknowledgements: false);
        var viewModel = CreateViewModel(dispatcher: dispatcher, textTransport: textTransport);

        await viewModel.Actions.Single(action => action.Label == "WHEEL UP").SendCommand.ExecuteAsync();

        Assert.Equal("Upload failed.", viewModel.SendStatusText);
        Assert.DoesNotContain("Sent without ACK", viewModel.SendStatusText);
    }

    [Fact]
    public void FestivalLock_HidesSecondaryControlsOnly()
    {
        var viewModel = CreateViewModel();

        viewModel.FestivalLock = true;

        Assert.False(viewModel.ShowSecondaryControls);
        Assert.Contains("BLACKOUT", viewModel.FestivalLockStatusText);
        Assert.Equal(4, viewModel.CommandFallbackActions.Count);
    }

    [Fact]
    public async Task NotReadyTextTransport_DoesNotSendManualCaption()
    {
        var textTransport = new CapturingTextUploadTransport(isReady: false);
        var dispatcher = new RecordingQuickActionDispatcher();
        var viewModel = CreateViewModel(dispatcher: dispatcher, textTransport: textTransport);

        await viewModel.Actions.Single(action => action.Label == "BOH").SendCommand.ExecuteAsync();

        Assert.Null(dispatcher.LastActionId);
        Assert.Equal("Text disconnected.", viewModel.SendStatusText);
    }

    private static RaveViewModel CreateViewModel(
        RecordingQuickActionDispatcher? dispatcher = null,
        IMaskCommandTransport? maskTransport = null,
        ITextUploadTransport? textTransport = null)
    {
        return new RaveViewModel(
            new QuickActionCatalog(),
            dispatcher ?? new RecordingQuickActionDispatcher(),
            maskTransport ?? new SimulatedMaskCommandTransport(),
            textTransport ?? new SimulatedTextUploadTransport());
    }

    private sealed class RecordingQuickActionDispatcher : IQuickActionDispatcher
    {
        private readonly QuickActionResult? fixedResult;
        private readonly List<QuickActionId> actionIds = [];
        private readonly List<QuickActionRequest?> requests = [];

        public RecordingQuickActionDispatcher(QuickActionResult? fixedResult = null)
        {
            this.fixedResult = fixedResult;
        }

        public QuickActionId? LastActionId { get; private set; }

        public IReadOnlyList<QuickActionId> ActionIds => actionIds;

        public IReadOnlyList<QuickActionRequest?> Requests => requests;

        public Task<QuickActionResult> TriggerAsync(
            QuickActionId actionId,
            QuickActionRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            LastActionId = actionId;
            actionIds.Add(actionId);
            requests.Add(request);
            return Task.FromResult(fixedResult ?? QuickActionResult.Sent(actionId, "Sent."));
        }
    }

    private sealed class CapturingTextUploadTransport : ITextUploadTransport
    {
        public CapturingTextUploadTransport(bool supportsAcknowledgements = true, bool isReady = true)
        {
            SupportsAcknowledgements = supportsAcknowledgements;
            IsReady = isReady;
            State = isReady
                ? supportsAcknowledgements ? TextUploadTransportState.Ready : TextUploadTransportState.CompatibilityReady
                : TextUploadTransportState.Disconnected;
        }

        public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged;

        public string TransportDisplayName => "Capture";

        public bool IsSimulated => false;

        public bool IsReady { get; }

        public bool SupportsAcknowledgements { get; }

        public TextUploadTransportState State { get; }

        public string StatusText => IsReady ? "Text ready." : "Text disconnected.";

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TextUploadResult.Success($"Uploaded {package.Frames.Count} frame(s).", package.Frames.Count));
        }

        public void RaiseStateChanged(TextUploadTransportState state, string message)
        {
            StateChanged?.Invoke(this, new TextUploadTransportStateChangedEventArgs(state, message, SupportsAcknowledgements, IsReady));
        }
    }
}
