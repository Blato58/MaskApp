using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Scenes;

namespace MaskApp.Core.Tests.Features.Scenes;

public sealed class SceneValidatorTests
{
    [Fact]
    public void Validate_ExpandsFiniteRepeatWithoutHiddenLoop()
    {
        var scene = new PerformanceScene
        {
            Id = "scene-repeat",
            Steps =
            [
                Step("a", SceneStepKind.Brightness),
                Step("b", SceneStepKind.Wait) with { Duration = TimeSpan.FromMilliseconds(100) },
                Step("repeat", SceneStepKind.Repeat) with { RepeatFromStepId = "a", RepeatCount = 3 }
            ]
        };

        var result = new SceneValidator().Validate(scene, new Dictionary<string, GalleryItem>());

        Assert.True(result.IsValid);
        Assert.Equal(6, result.ExpandedStepCount);
        Assert.Equal(
            ["a", "b", "a", "b", "a", "b"],
            result.ExpandedSteps.Select(step => step.Id).ToArray());
    }

    [Fact]
    public void Validate_RejectsNestedRepeatAndForwardTarget()
    {
        var nested = new PerformanceScene
        {
            Id = "scene-invalid-repeat",
            Steps =
            [
                Step("a", SceneStepKind.Brightness),
                Step("r1", SceneStepKind.Repeat) with { RepeatFromStepId = "a", RepeatCount = 2 },
                Step("r2", SceneStepKind.Repeat) with { RepeatFromStepId = "a", RepeatCount = 2 }
            ]
        };
        var forward = nested with
        {
            Id = "scene-forward",
            Steps =
            [
                Step("repeat", SceneStepKind.Repeat) with { RepeatFromStepId = "later", RepeatCount = 2 },
                Step("later", SceneStepKind.Brightness)
            ]
        };

        var nestedResult = new SceneValidator().Validate(nested, new Dictionary<string, GalleryItem>());
        var forwardResult = new SceneValidator().Validate(forward, new Dictionary<string, GalleryItem>());

        Assert.False(nestedResult.IsValid);
        Assert.Contains(nestedResult.Issues, issue => issue.Code == "nested-repeat");
        Assert.False(forwardResult.IsValid);
        Assert.Contains(forwardResult.Issues, issue => issue.Code == "repeat-target-invalid");
    }

    [Fact]
    public void Validate_RequiresCompatibleSendableLibraryContent()
    {
        var catalog = new Dictionary<string, GalleryItem>(StringComparer.Ordinal)
        {
            ["text:one"] = new GalleryItem
            {
                Id = "text:one",
                Type = GalleryItemType.TextPreset,
                Title = "Caption"
            },
            ["face:blocked"] = new GalleryItem
            {
                Id = "face:blocked",
                Type = GalleryItemType.CustomStaticFace,
                Title = "Broken face",
                CanSend = false
            }
        };
        var scene = new PerformanceScene
        {
            Id = "scene-content",
            Steps =
            [
                Step("wrong", SceneStepKind.Face) with { GalleryItemId = "text:one" },
                Step("blocked", SceneStepKind.Face) with { GalleryItemId = "face:blocked" },
                Step("missing", SceneStepKind.Animation) with { GalleryItemId = "animation:missing" }
            ]
        };

        var result = new SceneValidator().Validate(scene, catalog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "content-type-mismatch");
        Assert.Contains(result.Issues, issue => issue.Code == "content-unavailable");
        Assert.Contains(result.Issues, issue => issue.Code == "content-missing");
    }

    [Fact]
    public void ValidateSetlist_ReportsMissingScene()
    {
        var setlist = new PerformanceSetlist
        {
            Id = "set",
            Cues = [new PerformanceSetlistCue { Id = "cue", SceneId = "missing" }]
        };

        var issues = new SceneValidator().ValidateSetlist(
            setlist,
            new Dictionary<string, PerformanceScene>());

        Assert.Contains(issues, issue => issue.Code == "setlist-scene-missing");
    }

    private static PerformanceSceneStep Step(string id, SceneStepKind kind) => new()
    {
        Id = id,
        Kind = kind,
        Value = 60
    };
}
