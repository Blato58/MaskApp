using MaskApp.Core.Features.Home;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Home;

public sealed class HomeViewModelTests
{
    [Fact]
    public void TitleAndSummary_AreControlRoomFocused()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("Control Room", viewModel.AppTitle);
        Assert.Contains("brightness", viewModel.Summary);
        Assert.DoesNotContain("migration", viewModel.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("roadmap", viewModel.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ExposesTransportStatusAndRecoveryHint()
    {
        var commandTransport = new FakeCommandTransport(MaskCommandTransportState.Disconnected, "Connect to a mask.");
        var textTransport = new FakeTextTransport
        {
            IsReady = false,
            State = TextUploadTransportState.Disconnected,
            StatusText = "Text upload disconnected."
        };

        var viewModel = CreateViewModel(commandTransport, textTransport);

        Assert.Equal(MaskCommandTransportState.Disconnected, viewModel.CommandTransportState);
        Assert.Equal("Connect to a mask.", viewModel.CommandTransportStatusText);
        Assert.Equal(TextUploadTransportState.Disconnected, viewModel.TextTransportState);
        Assert.Equal("Text upload disconnected.", viewModel.TextTransportStatusText);
        Assert.Equal("Connect to send", viewModel.RecoveryHint);
    }

    [Fact]
    public async Task BlackoutCommand_DispatchesQuickAction()
    {
        var dispatcher = new RecordingQuickActionDispatcher();
        var viewModel = CreateViewModel(dispatcher: dispatcher);

        await viewModel.BlackoutCommand.ExecuteAsync();

        Assert.Equal(QuickActionId.Blackout, dispatcher.LastActionId);
        Assert.Null(dispatcher.LastRequest);
        Assert.Equal("Sent, confirm on mask", viewModel.LastActionStatus);
        Assert.Equal("Blackout", viewModel.CurrentLookText);
    }

    [Fact]
    public async Task ApplyBrightnessCommand_SendsSelectedBrightness()
    {
        var dispatcher = new RecordingQuickActionDispatcher();
        var viewModel = CreateViewModel(dispatcher: dispatcher);
        viewModel.Brightness = 84;

        await viewModel.ApplyBrightnessCommand.ExecuteAsync();

        Assert.Equal(QuickActionId.SetBrightness, dispatcher.LastActionId);
        Assert.Equal(84, dispatcher.LastRequest?.Brightness);
        Assert.Equal("84%", viewModel.BrightnessText);
    }

    [Fact]
    public async Task FavoriteReactionCommand_UsesDispatcherAndMovesToRecent()
    {
        var dispatcher = new RecordingQuickActionDispatcher();
        var viewModel = CreateViewModel(dispatcher: dispatcher);
        var favorite = viewModel.FavoriteReactions.Single(action => action.Label == "LOL");

        Assert.Equal(QuickActionId.Lol, favorite.Id);

        await favorite.TriggerCommand.ExecuteAsync();

        Assert.Equal(QuickActionId.Lol, dispatcher.LastActionId);
        Assert.Equal("LOL", viewModel.RecentReactions[0].Label);
        Assert.Equal(4, viewModel.RecentReactions.Count);
    }

    [Fact]
    public void TransportStateChanges_UpdateReadinessAndCommandStates()
    {
        var commandTransport = new FakeCommandTransport(MaskCommandTransportState.Disconnected, "Disconnected.");
        var textTransport = new FakeTextTransport
        {
            IsReady = false,
            State = TextUploadTransportState.Disconnected,
            StatusText = "Disconnected."
        };
        var viewModel = CreateViewModel(commandTransport, textTransport);

        commandTransport.RaiseStateChanged(MaskCommandTransportState.Ready, "Control ready.");
        textTransport.RaiseStateChanged(
            TextUploadTransportState.CompatibilityReady,
            "Write-only ready.",
            supportsAcknowledgements: false,
            isReady: true);

        Assert.True(viewModel.CanUseControlCommands);
        Assert.True(viewModel.CanUseTextReactions);
        Assert.Equal("Control ready.", viewModel.CommandTransportStatusText);
        Assert.Equal("Write-only ready.", viewModel.TextTransportStatusText);
        Assert.Contains("ready", viewModel.RecoveryHint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("write-only", viewModel.TextAcknowledgementText, StringComparison.OrdinalIgnoreCase);
    }

    private static HomeViewModel CreateViewModel(
        FakeCommandTransport? commandTransport = null,
        FakeTextTransport? textTransport = null,
        RecordingQuickActionDispatcher? dispatcher = null)
    {
        return new HomeViewModel(
            new QuickActionCatalog(),
            dispatcher ?? new RecordingQuickActionDispatcher(),
            commandTransport ?? new FakeCommandTransport(),
            textTransport ?? new FakeTextTransport());
    }

    private sealed class RecordingQuickActionDispatcher : IQuickActionDispatcher
    {
        public QuickActionId? LastActionId { get; private set; }

        public QuickActionRequest? LastRequest { get; private set; }

        public Task<QuickActionResult> TriggerAsync(
            QuickActionId actionId,
            QuickActionRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            LastActionId = actionId;
            LastRequest = request;
            return Task.FromResult(QuickActionResult.Sent(actionId, "Sent."));
        }
    }

    private sealed class FakeCommandTransport : IMaskCommandTransport
    {
        public FakeCommandTransport(
            MaskCommandTransportState state = MaskCommandTransportState.Ready,
            string statusText = "Control ready.")
        {
            TransportState = state;
            TransportStatusText = statusText;
        }

        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged;

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState { get; private set; }

        public string TransportStatusText { get; private set; }

        public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Sent."));

        public void RaiseStateChanged(MaskCommandTransportState state, string message)
        {
            TransportState = state;
            TransportStatusText = message;
            TransportStateChanged?.Invoke(this, new MaskCommandTransportStateChangedEventArgs(state, message));
        }
    }

    private sealed class FakeTextTransport : ITextUploadTransport
    {
        public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged;

        public string TransportDisplayName => "Fake";

        public bool IsSimulated => true;

        public bool IsReady { get; set; } = true;

        public bool SupportsAcknowledgements { get; set; } = true;

        public TextUploadTransportState State { get; set; } = TextUploadTransportState.Ready;

        public string StatusText { get; set; } = "Text ready.";

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TextUploadResult.Success("Uploaded.", package.Frames.Count));

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
