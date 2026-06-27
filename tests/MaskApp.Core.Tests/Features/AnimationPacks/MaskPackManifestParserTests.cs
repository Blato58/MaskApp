using MaskApp.Core.Features.AnimationPacks;

namespace MaskApp.Core.Tests.Features.AnimationPacks;

public sealed class MaskPackManifestParserTests
{
    [Fact]
    public void ParseJson_ValidStaticImageManifest_ReturnsManifest()
    {
        var json = """
            {
              "schemaVersion": 1,
              "packName": "Festival Faces",
              "author": "MaskApp",
              "source": "manual",
              "targetDisplay": { "width": 44, "height": 58 },
              "assets": [
                {
                  "id": "smile",
                  "type": "staticImage",
                  "name": "Smile",
                  "tags": [ "happy" ],
                  "notes": "One frame static face.",
                  "frames": [ { "path": "frames/frame-000.png" } ],
                  "frameDurationMs": 250,
                  "loop": false
                }
              ]
            }
            """;

        var result = MaskPackManifestParser.ParseJson(json);

        Assert.True(result.IsValid);
        Assert.Empty(result.Warnings);
        Assert.Equal("Festival Faces", result.Manifest?.PackName);
        Assert.Equal(MaskPackAssetType.StaticImage, result.Manifest?.Assets[0].Type);
    }

    [Fact]
    public void ParseJson_InvalidGeometryAndDuration_ReturnsErrors()
    {
        var json = """
            {
              "schemaVersion": 1,
              "packName": "Bad Pack",
              "targetDisplay": { "width": 45, "height": 58 },
              "assets": [
                {
                  "id": "bad",
                  "type": "animation",
                  "name": "Bad",
                  "frames": [ { "path": "frames/frame-000.png", "durationMs": 0 } ],
                  "frameDurationMs": 0
                }
              ]
            }
            """;

        var result = MaskPackManifestParser.ParseJson(json);

        Assert.False(result.IsValid);
        Assert.Contains("targetDisplay.width must be 44.", result.Errors);
        Assert.Contains("Asset bad frameDurationMs must be positive.", result.Errors);
        Assert.Contains("Asset bad frame 0 durationMs must be positive when provided.", result.Errors);
    }

    [Fact]
    public void Validate_HighFrameCount_WarnsButDoesNotReject()
    {
        var manifest = new MaskPackManifest
        {
            SchemaVersion = 1,
            PackName = "Long Loop",
            TargetDisplay = new MaskPackDisplayGeometry { Width = 44, Height = 58 },
            Assets =
            [
                new MaskPackAsset
                {
                    Id = "loop",
                    Type = MaskPackAssetType.Animation,
                    Name = "Loop",
                    FrameDurationMs = 100,
                    Frames = Enumerable.Range(0, 25)
                        .Select(index => new MaskPackFrame { Path = $"frames/frame-{index:000}.png" })
                        .ToArray()
                }
            ]
        };

        var result = MaskPackManifestParser.Validate(manifest);

        Assert.True(result.IsValid);
        Assert.Contains(result.Warnings, warning => warning.Contains("high frame count"));
    }
}
