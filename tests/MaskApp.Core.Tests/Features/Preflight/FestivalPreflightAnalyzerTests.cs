using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Profiles;
using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.Preflight;

public sealed class FestivalPreflightAnalyzerTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-07-17T12:00:00Z");

    [Fact]
    public void Analyze_AcknowledgedPreparedFace_IsShowReady()
    {
        var face = FacePatternFactory.CreateBlank("Prepared", preferredSlot: 7);
        var fingerprint = FaceContentFingerprint.Compute(face);
        var item = CreateFaceItem(face);
        var request = CreateRequest(
            [item],
            [CreatePage("page-a", item.Id)],
            CreateProfile(new MaskPreparedSlot
            {
                Slot = 7,
                ContentFingerprint = fingerprint,
                SourceId = item.Id,
                InstalledAt = Now,
                Verification = MaskPreparedSlotVerification.Acknowledged
            }));

        var report = new FestivalPreflightAnalyzer().Analyze(request);

        Assert.Equal(FestivalPreflightStatus.ShowReady, report.Status);
        Assert.Equal("SHOW READY", report.StatusText);
        Assert.Empty(report.Issues);
        Assert.Equal(PreflightActionClassification.Prepared, Assert.Single(report.Actions).Classification);
    }

    [Fact]
    public void Analyze_NoActiveProfile_IsNotReadyWithRecoveryAction()
    {
        var face = FacePatternFactory.CreateBlank("Needs mask", preferredSlot: 7);
        var item = CreateFaceItem(face);
        var request = CreateRequest([item], [CreatePage("page-a", item.Id)], profile: null);

        var report = new FestivalPreflightAnalyzer().Analyze(request);

        Assert.Equal(FestivalPreflightStatus.NotReady, report.Status);
        var issue = Assert.Single(report.Issues, item => item.Code == "mask-profile-missing");
        Assert.Equal(PreflightIssueSeverity.Blocking, issue.Severity);
        Assert.Contains("connect", issue.RecoveryAction, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_DisconnectedActiveProfile_CannotReportShowReady()
    {
        var face = FacePatternFactory.CreateBlank("Prepared", preferredSlot: 7);
        var item = CreateFaceItem(face);
        var request = CreateRequest(
            [item],
            [CreatePage("page-a", item.Id)],
            CreateProfile(new MaskPreparedSlot
            {
                Slot = 7,
                ContentFingerprint = FaceContentFingerprint.Compute(face),
                SourceId = item.Id,
                InstalledAt = Now,
                Verification = MaskPreparedSlotVerification.Acknowledged
            })) with
        {
            ConnectionState = BleConnectionState.Disconnected
        };

        var report = new FestivalPreflightAnalyzer().Analyze(request);

        Assert.Equal(FestivalPreflightStatus.NotReady, report.Status);
        var issue = Assert.Single(report.Issues, candidate => candidate.Code == "mask-disconnected");
        Assert.Equal(PreflightIssueSeverity.Blocking, issue.Severity);
        Assert.Contains("Device", issue.RecoveryAction, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_DeniedBluetoothPermission_IsNotReadyWithRecoveryAction()
    {
        var face = FacePatternFactory.CreateBlank("Prepared", preferredSlot: 7);
        var item = CreateFaceItem(face);
        var request = CreateRequest(
            [item],
            [CreatePage("page-a", item.Id)],
            CreateProfile(new MaskPreparedSlot
            {
                Slot = 7,
                ContentFingerprint = FaceContentFingerprint.Compute(face),
                SourceId = item.Id,
                InstalledAt = Now,
                Verification = MaskPreparedSlotVerification.Acknowledged
            })) with
        {
            RuntimeSnapshot = PreflightRuntimeSnapshot.GrantedForTests with
            {
                BluetoothAccess = PreflightRuntimeAccessStatus.Denied,
                BluetoothDetail = "Bluetooth permission is not granted."
            }
        };

        var report = new FestivalPreflightAnalyzer().Analyze(request);

        Assert.Equal(FestivalPreflightStatus.NotReady, report.Status);
        var issue = Assert.Single(report.Issues, candidate => candidate.Code == "bluetooth-permission-denied");
        Assert.Contains("settings", issue.RecoveryAction, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_MicrophonePermission_IsCheckedOnlyWhenScopeRequiresIt()
    {
        var face = FacePatternFactory.CreateBlank("Prepared", preferredSlot: 7);
        var item = CreateFaceItem(face);
        var request = CreateRequest(
            [item],
            [CreatePage("page-a", item.Id)],
            CreateProfile(new MaskPreparedSlot
            {
                Slot = 7,
                ContentFingerprint = FaceContentFingerprint.Compute(face),
                SourceId = item.Id,
                InstalledAt = Now,
                Verification = MaskPreparedSlotVerification.Acknowledged
            })) with
        {
            RuntimeSnapshot = PreflightRuntimeSnapshot.GrantedForTests with
            {
                MicrophoneAccess = PreflightRuntimeAccessStatus.Denied,
                MicrophoneDetail = "Microphone permission is not granted."
            }
        };

        var showReport = new FestivalPreflightAnalyzer().Analyze(request);
        var audioReport = new FestivalPreflightAnalyzer().Analyze(request with
        {
            RequiredRuntimePermissions =
                PreflightRuntimeRequirement.Bluetooth | PreflightRuntimeRequirement.Microphone
        });

        Assert.DoesNotContain(showReport.Issues, issue => issue.Code.StartsWith("microphone-", StringComparison.Ordinal));
        Assert.Contains(audioReport.Issues, issue => issue.Code == "microphone-permission-denied");
    }

    [Fact]
    public void Analyze_SelectedPages_TraversesEverySelectedPageAndNoOthers()
    {
        var face = FacePatternFactory.CreateBlank("Prepared", preferredSlot: 7);
        var item = CreateFaceItem(face);
        var request = CreateRequest(
            [item],
            [
                CreatePage("page-a", item.Id),
                CreatePage("page-b", item.Id),
                CreatePage("page-c", item.Id)
            ],
            CreateProfile(new MaskPreparedSlot
            {
                Slot = 7,
                ContentFingerprint = FaceContentFingerprint.Compute(face),
                SourceId = item.Id,
                InstalledAt = Now,
                Verification = MaskPreparedSlotVerification.Acknowledged
            })) with
        {
            SelectedPageIds = ["page-a", "page-c"]
        };

        var report = new FestivalPreflightAnalyzer().Analyze(request);

        Assert.Equal(["page-a", "page-c"], report.Actions.Select(action => action.PageId));
    }

    [Fact]
    public void Analyze_TextThatMustUpload_IsDegradedNotFalselyReady()
    {
        var preset = new TextPreset
        {
            Id = new TextPresetId("test"),
            InputText = "TEST",
            MaskText = "TEST",
            DisplayName = "Test caption"
        };
        var item = new GalleryItem
        {
            Id = "text:test",
            Type = GalleryItemType.TextPreset,
            Title = "Test caption",
            TextPreset = preset
        };

        var report = new FestivalPreflightAnalyzer().Analyze(CreateRequest(
            [item],
            [CreatePage("page-a", item.Id)],
            CreateProfile()));

        Assert.Equal(FestivalPreflightStatus.Degraded, report.Status);
        Assert.Equal(PreflightActionClassification.UploadRequired, Assert.Single(report.Actions).Classification);
        Assert.Contains(report.Issues, issue => issue.Code == "content-upload-required");
    }

    [Fact]
    public void Analyze_SelectedPage_DoesNotTraverseUnselectedMissingContent()
    {
        var face = FacePatternFactory.CreateBlank("Prepared", preferredSlot: 7);
        var item = CreateFaceItem(face);
        var profile = CreateProfile(new MaskPreparedSlot
        {
            Slot = 7,
            ContentFingerprint = FaceContentFingerprint.Compute(face),
            SourceId = item.Id,
            InstalledAt = Now,
            Verification = MaskPreparedSlotVerification.Acknowledged
        });
        var selectedPage = CreatePage("selected", item.Id);
        var unselectedPage = CreatePage("unselected", "missing:item");
        var request = CreateRequest([item], [selectedPage, unselectedPage], profile) with
        {
            SelectedPageIds = [selectedPage.PageId]
        };

        var report = new FestivalPreflightAnalyzer().Analyze(request);

        Assert.Equal(FestivalPreflightStatus.ShowReady, report.Status);
        Assert.Single(report.Actions);
        Assert.DoesNotContain(report.Issues, issue => issue.Code == "missing-gallery-item");
    }

    [Fact]
    public void Analyze_UnsafeAnimation_IsBlockedUntilExactRevisionIsAcknowledged()
    {
        var animation = AppBuiltInAnimationCatalog.CreateBuiltIns()[0];
        Assert.Equal(300, animation.FrameDurationMilliseconds);
        var performanceAnimation = new PerformanceAnimationBuilder().FromAppBuiltIn(animation);
        var safetyAssessment = new FlashSafetyAnalyzer().Analyze(performanceAnimation);
        Assert.False(safetyAssessment.IsSafeByDefault);
        var item = new GalleryItem
        {
            Id = $"app-animation:{animation.Id}",
            Type = GalleryItemType.AppBuiltInAnimation,
            Title = animation.DisplayName,
            AppAnimation = animation
        };
        var preparedSlots = performanceAnimation.StoredFrames.Select(frame => new MaskPreparedSlot
        {
            Slot = frame.Slot,
            ContentFingerprint = frame.ContentFingerprint,
            SourceId = item.Id,
            InstalledAt = Now,
            Verification = MaskPreparedSlotVerification.Acknowledged
        }).ToArray();
        var request = CreateRequest(
            [item],
            [CreatePage("page-a", item.Id)],
            CreateProfile(preparedSlots) with { SustainableCadenceHz = 20 });

        var blocked = new FestivalPreflightAnalyzer().Analyze(request);
        var acknowledged = new FestivalPreflightAnalyzer().Analyze(request with
        {
            FlashSafetyAcknowledgements = new FlashSafetyAcknowledgementState
            {
                Acknowledgements =
                [
                    new FlashSafetyAcknowledgement
                    {
                        ContentId = safetyAssessment.ContentId,
                        RevisionHash = safetyAssessment.RevisionHash,
                        AcknowledgedAt = Now,
                        Warning = FlashSafetyAcknowledgementService.RequiredWarning
                    }
                ]
            }
        });

        Assert.Equal(FestivalPreflightStatus.NotReady, blocked.Status);
        var blockedIssue = Assert.Single(blocked.Issues, issue => issue.Code == "flash-safety-blocked");
        Assert.Contains(animation.DisplayName, blockedIssue.Message, StringComparison.Ordinal);
        Assert.Contains("Playback blocked", blockedIssue.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("explicitly acknowledge", blockedIssue.RecoveryAction, StringComparison.OrdinalIgnoreCase);
        Assert.Single(blocked.FlashSafetyResults);
        Assert.Equal(FestivalPreflightStatus.Degraded, acknowledged.Status);
        Assert.DoesNotContain(acknowledged.Issues, issue => issue.Code == "flash-safety-blocked");
        Assert.Contains(acknowledged.Issues, issue => issue.Code == "flash-safety-overridden");
        Assert.Equal(
            FlashSafetyStatus.AcknowledgedOverride,
            Assert.Single(acknowledged.FlashSafetyResults).Decision.Status);
    }

    [Fact]
    public void Analyze_PreparedCustomAnimation_UsesProductionFramesSafetyAndCadence()
    {
        var black = FacePatternFactory.CreateBlank("Black", 1);
        var white = new FacePattern
        {
            Id = "white",
            DisplayName = "White",
            Pixels = Enumerable.Repeat(
                new FacePixel(true, new FaceColor(255, 255, 255)),
                FacePattern.PixelCount).ToArray()
        }.Normalize();
        var project = new AnimationProject
        {
            Id = "custom-safe",
            DisplayName = "Custom safe",
            Frames =
            [
                new AnimationProjectFrame { Id = "black", Pattern = black, Duration = TimeSpan.FromMilliseconds(500) },
                new AnimationProjectFrame { Id = "white", Pattern = white, Duration = TimeSpan.FromMilliseconds(500) }
            ]
        };
        var animation = new AnimationProjectCompiler().Compile(project).Animation!;
        var item = new GalleryItem
        {
            Id = "animation:custom-safe",
            Type = GalleryItemType.CustomAnimation,
            Title = "Custom safe",
            PerformanceAnimation = animation,
            AnimationProject = project
        };
        var prepared = animation.StoredFrames.Select(frame => new MaskPreparedSlot
        {
            Slot = frame.Slot,
            ContentFingerprint = frame.ContentFingerprint,
            SourceId = item.Id,
            InstalledAt = Now,
            Verification = MaskPreparedSlotVerification.Acknowledged
        }).ToArray();

        var report = new FestivalPreflightAnalyzer().Analyze(CreateRequest(
            [item],
            [CreatePage("page-a", item.Id)],
            CreateProfile(prepared) with { SustainableCadenceHz = 10 }));

        Assert.Equal(FestivalPreflightStatus.ShowReady, report.Status);
        Assert.Equal(PreflightActionClassification.Prepared, Assert.Single(report.Actions).Classification);
        Assert.Equal(animation.StoredFrames.Count, report.SlotAllocations.Count);
        Assert.Equal(FlashSafetyStatus.Safe, Assert.Single(report.FlashSafetyResults).Decision.Status);
    }

    private static FestivalPreflightRequest CreateRequest(
        IReadOnlyList<GalleryItem> catalog,
        IReadOnlyList<GalleryPageLayout> pages,
        MaskProfile? profile) =>
        new()
        {
            Catalog = catalog,
            Layout = new GalleryLayoutState { Pages = pages },
            ActiveProfile = profile,
            ConnectionState = BleConnectionState.Connected,
            RuntimeSnapshot = PreflightRuntimeSnapshot.GrantedForTests,
            EvaluatedAt = Now
        };

    private static GalleryPageLayout CreatePage(string pageId, string itemId) =>
        new()
        {
            PageId = pageId,
            Title = pageId,
            Items =
            [
                new GalleryPageItemLayout
                {
                    SlotId = $"slot-{pageId}",
                    GalleryItemId = itemId,
                    Label = itemId
                }
            ]
        };

    private static GalleryItem CreateFaceItem(FacePattern face) =>
        new()
        {
            Id = $"face:{face.Id}",
            Type = GalleryItemType.CustomStaticFace,
            Title = face.DisplayName,
            FacePattern = face
        };

    private static MaskProfile CreateProfile(params MaskPreparedSlot[] slots) =>
        new()
        {
            ProfileId = "mask-test",
            DisplayName = "Test Mask",
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
            AverageCommandLatencyMilliseconds = 35,
            SustainableCadenceHz = 12
        };
}
