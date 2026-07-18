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

    [Fact]
    public void ParseJson_ValidV2Manifest_DistinguishesArtAndTextGeometry()
    {
        var json = """
            {
              "schemaVersion": 2,
              "packName": "Complete Show",
              "author": "MaskApp",
              "artDisplay": { "width": 46, "height": 58 },
              "textDisplay": { "width": 44, "height": 58 },
              "contents": [
                {
                  "id": "face-one",
                  "type": "face",
                  "name": "Face One",
                  "path": "content/face/face-one.json",
                  "sha256": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                  "formatVersion": 1
                }
              ]
            }
            """;

        var result = MaskPackManifestParser.ParseJson(json);

        Assert.True(result.IsValid);
        Assert.Equal(46, result.Manifest?.ArtDisplay.Width);
        Assert.Equal(44, result.Manifest?.TextDisplay.Width);
        Assert.Single(result.Manifest?.Contents ?? []);
    }

    [Theory]
    [InlineData(44, 58, 44, 58, "artDisplay must be 46x58")]
    [InlineData(46, 58, 46, 58, "textDisplay must be 44x58")]
    public void Validate_V2GeometryMismatch_IsRejected(
        int artWidth,
        int artHeight,
        int textWidth,
        int textHeight,
        string expected)
    {
        var manifest = new MaskPackManifest
        {
            SchemaVersion = 2,
            PackName = "Wrong geometry",
            ArtDisplay = new MaskPackDisplayGeometry { Width = artWidth, Height = artHeight },
            TextDisplay = new MaskPackDisplayGeometry { Width = textWidth, Height = textHeight },
            Contents =
            [
                new MaskPackContentEntry
                {
                    Id = "face",
                    Type = MaskPackContentType.Face,
                    Name = "Face",
                    Path = "content/face.json",
                    Sha256 = new string('a', 64)
                }
            ]
        };

        var result = MaskPackManifestParser.Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains(expected, StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_V2DuplicateTypedIdAndUnsupportedPayloadVersion_AreRejected()
    {
        var entry = new MaskPackContentEntry
        {
            Id = "same",
            Type = MaskPackContentType.Face,
            Name = "Same",
            Path = "content/one.json",
            Sha256 = new string('b', 64),
            FormatVersion = 2
        };
        var manifest = new MaskPackManifest
        {
            SchemaVersion = 2,
            PackName = "Bad entries",
            ArtDisplay = new MaskPackDisplayGeometry { Width = 46, Height = 58 },
            TextDisplay = new MaskPackDisplayGeometry { Width = 44, Height = 58 },
            Contents = [entry, entry with { Path = "content/two.json", FormatVersion = 1 }]
        };

        var result = MaskPackManifestParser.Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("formatVersion 2 is unsupported", StringComparison.Ordinal));
        Assert.Contains(result.Errors, error => error.Contains("Duplicate content key Face:same", StringComparison.Ordinal));
    }
}
