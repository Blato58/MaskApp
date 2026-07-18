using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.Preflight;

public sealed class ScenePreflightTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-07-17T12:00:00Z");

    [Fact]
    public void Analyze_SceneTraversesRepeatedDependenciesAndDeduplicatesPhysicalSlot()
    {
        var face = FacePatternFactory.CreateBlank("Scene face", 6);
        var faceItem = new GalleryItem
        {
            Id = $"face:{face.Id}",
            Type = GalleryItemType.CustomStaticFace,
            Title = face.DisplayName,
            FacePattern = face
        };
        var scene = new PerformanceScene
        {
            Id = "scene-repeat",
            DisplayName = "Repeated face",
            Steps =
            [
                new PerformanceSceneStep { Id = "face", Kind = SceneStepKind.Face, GalleryItemId = faceItem.Id },
                new PerformanceSceneStep
                {
                    Id = "repeat",
                    Kind = SceneStepKind.Repeat,
                    RepeatFromStepId = "face",
                    RepeatCount = 3
                }
            ]
        };
        var sceneItem = new GalleryItem
        {
            Id = $"scene:{scene.Id}",
            Type = GalleryItemType.Scene,
            Title = scene.DisplayName,
            Scene = scene
        };
        var report = new FestivalPreflightAnalyzer().Analyze(Request(
            [faceItem, sceneItem],
            sceneItem.Id,
            Profile()));

        Assert.Equal(FestivalPreflightStatus.Degraded, report.Status);
        Assert.Equal(PreflightActionClassification.UploadRequired, Assert.Single(report.Actions).Classification);
        Assert.Equal(3, report.SlotAllocations.Count);
        Assert.Single(report.SlotAllocations.Select(allocation => allocation.AssignedSlot).Distinct());
    }

    [Fact]
    public void Analyze_PreparedSceneIsPrepared_AndLiveTextKeepsSceneDegraded()
    {
        var face = FacePatternFactory.CreateBlank("Prepared", 5);
        var faceItem = new GalleryItem
        {
            Id = $"face:{face.Id}",
            Type = GalleryItemType.CustomStaticFace,
            Title = face.DisplayName,
            FacePattern = face
        };
        var textItem = new GalleryItem
        {
            Id = "text:caption",
            Type = GalleryItemType.TextPreset,
            Title = "Caption",
            TextPreset = new TextPreset
            {
                Id = new TextPresetId("caption"),
                DisplayName = "Caption",
                InputText = "HELLO",
                MaskText = "HELLO"
            }
        };
        var faceScene = Scene("scene-face", new PerformanceSceneStep
        {
            Id = "face",
            Kind = SceneStepKind.Face,
            GalleryItemId = faceItem.Id
        });
        var textScene = Scene("scene-text", new PerformanceSceneStep
        {
            Id = "text",
            Kind = SceneStepKind.Text,
            GalleryItemId = textItem.Id
        });
        var preparedSlot = new MaskPreparedSlot
        {
            Slot = face.PreferredSlot,
            ContentFingerprint = FaceContentFingerprint.Compute(face),
            SourceId = faceItem.Id,
            InstalledAt = Now,
            Verification = MaskPreparedSlotVerification.Acknowledged
        };

        var preparedReport = new FestivalPreflightAnalyzer().Analyze(Request(
            [faceItem, SceneItem(faceScene)],
            $"scene:{faceScene.Id}",
            Profile(preparedSlot)));
        var textReport = new FestivalPreflightAnalyzer().Analyze(Request(
            [textItem, SceneItem(textScene)],
            $"scene:{textScene.Id}",
            Profile()));

        Assert.Equal(FestivalPreflightStatus.ShowReady, preparedReport.Status);
        Assert.Equal(PreflightActionClassification.Prepared, Assert.Single(preparedReport.Actions).Classification);
        Assert.Equal(FestivalPreflightStatus.Degraded, textReport.Status);
        Assert.Equal(PreflightActionClassification.UploadRequired, Assert.Single(textReport.Actions).Classification);
        Assert.Contains(textReport.Issues, issue => issue.Code == "content-upload-required");
    }

    private static PerformanceScene Scene(string id, params PerformanceSceneStep[] steps) => new()
    {
        Id = id,
        DisplayName = id,
        Steps = steps
    };

    private static GalleryItem SceneItem(PerformanceScene scene) => new()
    {
        Id = $"scene:{scene.Id}",
        Type = GalleryItemType.Scene,
        Title = scene.DisplayName,
        Scene = scene
    };

    private static FestivalPreflightRequest Request(
        IReadOnlyList<GalleryItem> catalog,
        string itemId,
        MaskProfile profile) => new()
    {
        Catalog = catalog,
        Layout = new GalleryLayoutState
        {
            Pages =
            [
                new GalleryPageLayout
                {
                    PageId = "page",
                    Title = "Show",
                    Items = [new GalleryPageItemLayout { SlotId = "slot", GalleryItemId = itemId }]
                }
            ]
        },
        ActiveProfile = profile,
        ConnectionState = BleConnectionState.Connected,
        EvaluatedAt = Now
    };

    private static MaskProfile Profile(params MaskPreparedSlot[] slots) => new()
    {
        ProfileId = "mask",
        DisplayName = "Mask",
        FirstSeenAt = Now.AddHours(-1),
        LastSeenAt = Now,
        Capabilities = new MaskCapabilitySnapshot
        {
            CommandWriteAvailable = true,
            TextUploadAvailable = true,
            FaceUploadAvailable = true,
            AcknowledgementMode = MaskAcknowledgementMode.Acknowledged,
            DiySlotCapacity = 20,
            TransportName = "Fake",
            ObservedAt = Now
        },
        PreparedSlots = slots,
        AverageCommandLatencyMilliseconds = 30,
        SustainableCadenceHz = 12
    };
}
