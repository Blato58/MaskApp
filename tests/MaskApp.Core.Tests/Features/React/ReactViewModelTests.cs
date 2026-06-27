using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.React;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.React;

public sealed class ReactViewModelTests
{
    [Fact]
    public async Task SendAsync_Reaction_TriggersDispatcherAndUpdatesStatus()
    {
        var dispatcher = new FakeQuickActionDispatcher(QuickActionResult.Sent(QuickActionId.Lol, "Uploaded."));
        var viewModel = new ReactViewModel(
            new QuickActionCatalog(),
            dispatcher,
            new FakeCommandTransport(),
            new FakeTextUploadTransport());
        var card = viewModel.Groups.Single(group => group.Category == QuickActionCategory.Meme)
            .Cards.Single(card => card.Id == QuickActionId.Lol);

        await viewModel.SendAsync(card);

        Assert.Equal(QuickActionId.Lol, dispatcher.LastActionId);
        Assert.Equal("LOL", viewModel.LastActionText);
        Assert.Equal("Sent LOL. Uploaded.", viewModel.StatusText);
    }

    [Fact]
    public void TextReactionCommand_IsDisabledWhenTextTransportIsNotReady()
    {
        var viewModel = new ReactViewModel(
            new QuickActionCatalog(),
            new FakeQuickActionDispatcher(QuickActionResult.Sent(QuickActionId.Lol, "Uploaded.")),
            new FakeCommandTransport(),
            new FakeTextUploadTransport(IsReady: false, StatusText: "Connect first."));
        var card = viewModel.Groups.Single(group => group.Category == QuickActionCategory.Meme)
            .Cards.Single(card => card.Id == QuickActionId.Lol);

        Assert.False(card.SendCommand.CanExecute(null));
        Assert.Contains("Text reactions unavailable: Connect first.", viewModel.TextReadinessText);
    }

    [Fact]
    public void BlackoutCommand_StaysAvailableWhenTextTransportIsNotReady()
    {
        var viewModel = new ReactViewModel(
            new QuickActionCatalog(),
            new FakeQuickActionDispatcher(QuickActionResult.Sent(QuickActionId.Blackout, "Sent.")),
            new FakeCommandTransport(),
            new FakeTextUploadTransport(IsReady: false, StatusText: "Connect first."));
        var blackout = viewModel.PinnedCards.Single(card => card.Id == QuickActionId.Blackout);
        var random = viewModel.PinnedCards.Single(card => card.Id == QuickActionId.RandomReaction);

        Assert.True(blackout.SendCommand.CanExecute(null));
        Assert.False(random.SendCommand.CanExecute(null));
        Assert.Contains("BLACKOUT ready.", viewModel.ReadinessText);
    }

    [Fact]
    public void BuiltInFallbackCommand_StaysAvailableWhenTextTransportIsNotReady()
    {
        var viewModel = new ReactViewModel(
            new QuickActionCatalog(),
            new FakeQuickActionDispatcher(QuickActionResult.Sent(QuickActionId.TestImage1, "Sent image.")),
            new FakeCommandTransport(),
            new FakeTextUploadTransport(IsReady: false, StatusText: "Connect text first."));
        var card = viewModel.Groups.Single(group => group.Category == QuickActionCategory.BuiltIn)
            .Cards.Single(card => card.Id == QuickActionId.TestImage1);

        Assert.True(card.SendCommand.CanExecute(null));
        Assert.Contains("Needs real-mask test", card.Description);
    }

    private sealed class FakeQuickActionDispatcher : IQuickActionDispatcher
    {
        private readonly QuickActionResult result;

        public FakeQuickActionDispatcher(QuickActionResult result)
        {
            this.result = result;
        }

        public QuickActionId? LastActionId { get; private set; }

        public Task<QuickActionResult> TriggerAsync(
            QuickActionId actionId,
            QuickActionRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            LastActionId = actionId;
            return Task.FromResult(result);
        }
    }

    private sealed class FakeCommandTransport : IMaskCommandTransport
    {
        public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Fake commands";

        public bool IsSimulated => true;

        public MaskCommandTransportState TransportState => MaskCommandTransportState.Ready;

        public string TransportStatusText => "Command ready.";

        public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(MaskCommandResult.Success("Sent."));
    }

    private sealed class FakeTextUploadTransport : ITextUploadTransport
    {
        public FakeTextUploadTransport(
            bool IsReady = true,
            string StatusText = "Text ready.",
            bool SupportsAcknowledgements = true)
        {
            this.IsReady = IsReady;
            this.StatusText = StatusText;
            this.SupportsAcknowledgements = SupportsAcknowledgements;
        }

        public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Fake text";

        public bool IsSimulated => true;

        public bool IsReady { get; }

        public bool SupportsAcknowledgements { get; }

        public TextUploadTransportState State => IsReady ? TextUploadTransportState.Ready : TextUploadTransportState.Disconnected;

        public string StatusText { get; }

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TextUploadResult.Success("Uploaded.", package.Frames.Count));
    }
}
