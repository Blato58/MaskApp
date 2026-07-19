using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.Preflight;

public sealed class FestivalPreflightViewModelTests
{
    [Fact]
    public async Task RunSelectedPages_TraversesAllCheckedPagesAndPreservesSelectionAcrossRefresh()
    {
        var faceStore = new InMemoryFacePatternStore();
        var face = (await faceStore.LoadAsync()).Patterns.First();
        var layoutStore = new InMemoryGalleryLayoutStore(new GalleryLayoutState
        {
            Pages =
            [
                CreatePage("page-a", "A", face.Id),
                CreatePage("page-b", "B", face.Id),
                CreatePage("page-c", "C", face.Id)
            ]
        });
        var profileSession = new MaskProfileSession(new InMemoryMaskProfileStore());
        await profileSession.ActivateAsync(new DiscoveredMaskDevice("device-a", "Mask A", -40));
        await profileSession.ObserveCapabilitiesAsync(new MaskCapabilitySnapshot
        {
            CommandWriteAvailable = true,
            TextUploadAvailable = true,
            FaceUploadAvailable = true,
            AcknowledgementMode = MaskAcknowledgementMode.Acknowledged,
            DiySlotCapacity = FacePattern.MaxSlot,
            TransportName = "Fake",
            ObservedAt = DateTimeOffset.UtcNow
        });
        var connection = new ConnectedDeviceConnection();
        var commandTransport = new SimulatedMaskCommandTransport();
        var textTransport = new SimulatedTextUploadTransport();
        var faceTransport = new SimulatedFaceUploadTransport();
        await using var scheduler = new MaskBleScheduler(
            commandTransport,
            textTransport,
            faceTransport,
            connection);
        var acknowledgements = new InMemoryFlashSafetyAcknowledgementStore();
        var viewModel = new FestivalPreflightViewModel(
            new InMemoryTextPresetStore(),
            new InMemoryBuiltInAssetArchiveStore(),
            faceStore,
            layoutStore,
            profileSession,
            scheduler,
            new QuickActionCatalog(),
            new FestivalPreflightAnalyzer(),
            new FestivalShowPreparationService(faceStore, scheduler),
            acknowledgements,
            new FlashSafetyAcknowledgementService(acknowledgements),
            connection,
            new GrantedRuntimeStateProvider());

        await viewModel.InitializeAsync();
        viewModel.Pages[0].IsSelected = false;
        viewModel.Pages[1].IsSelected = true;
        viewModel.Pages[2].IsSelected = true;
        await viewModel.RunSelectedPagesAsync();

        Assert.Equal(["page-b", "page-c"], viewModel.CurrentReport!.Actions.Select(action => action.PageId));
        Assert.Equal("Selected Pages · 2", viewModel.ScopeText);

        await viewModel.InitializeAsync();
        Assert.Equal(
            ["page-b", "page-c"],
            viewModel.Pages.Where(page => page.IsSelected).Select(page => page.PageId));
    }

    [Fact]
    public void PageOption_CommunicatesSelectionWithoutColor()
    {
        var option = new PreflightPageOption("page-a", "Main");

        Assert.Contains("not selected", option.SelectionStatusText, StringComparison.OrdinalIgnoreCase);
        option.IsSelected = true;
        Assert.Contains("selected", option.SelectionStatusText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not selected", option.SelectionStatusText, StringComparison.OrdinalIgnoreCase);
    }

    private static GalleryPageLayout CreatePage(string id, string title, string faceId) => new()
    {
        PageId = id,
        Title = title,
        Items =
        [
            new GalleryPageItemLayout
            {
                SlotId = $"slot-{id}",
                GalleryItemId = $"face:{faceId}",
                Label = title
            }
        ]
    };

    private sealed class GrantedRuntimeStateProvider : IPreflightRuntimeStateProvider
    {
        public Task<PreflightRuntimeSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(PreflightRuntimeSnapshot.GrantedForTests);
        }
    }

    private sealed class ConnectedDeviceConnection : IBleDeviceConnection
    {
        public event EventHandler<BleConnectionStateChangedEventArgs>? ConnectionStateChanged
        {
            add { }
            remove { }
        }

        public BleConnectionState State => BleConnectionState.Connected;

        public Task ConnectAsync(
            DiscoveredMaskDevice device,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DisconnectAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
