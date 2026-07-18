using MaskApp.Core.Features.AnimationPacks;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.AnimationPacks;

public sealed class MaskPackPayloadCodecTests
{
    [Fact]
    public void SupportedPayloads_RoundTripAndStripVolatileDeviceState()
    {
        var face = CreateFace("face-codec", "Codec Face", 0);
        var animation = new AnimationProject
        {
            Id = "animation-codec",
            DisplayName = "Codec Animation",
            LoopMode = AnimationLoopMode.Finite,
            FiniteLoopCount = 3,
            Bpm = 128,
            Frames =
            [
                new AnimationProjectFrame { Id = "one", Duration = TimeSpan.FromMilliseconds(80), Pattern = face },
                new AnimationProjectFrame { Id = "two", Duration = TimeSpan.FromMilliseconds(140), Pattern = CreateFace("frame-two", "Frame Two", 1) }
            ]
        }.Normalize(DateTimeOffset.UnixEpoch);
        var text = new TextPreset
        {
            Id = new TextPresetId("text-codec"),
            DisplayName = "Codec Text",
            InputText = "HELLO",
            MaskText = "HELLO",
            IsSeed = true,
            LastSentAt = DateTimeOffset.UtcNow,
            LastSendStatus = "Sent",
            Style = TextPresetStyle.Default with { IsBold = true }
        };
        var page = new GalleryPageLayout
        {
            PageId = "page-codec",
            Title = "Codec Page",
            Items =
            [
                new GalleryPageItemLayout
                {
                    SlotId = "slot-codec",
                    GalleryItemId = "face:face-codec",
                    Label = "Face",
                    FastMaskSlot = 9,
                    FastContentFingerprint = "device-specific",
                    FastPreparedAt = DateTimeOffset.UtcNow
                }
            ]
        };
        var scene = new PerformanceScene
        {
            Id = "scene-codec",
            DisplayName = "Codec Scene",
            Steps = [new PerformanceSceneStep { Id = "step-one", Kind = SceneStepKind.Face, GalleryItemId = "face:face-codec" }]
        };
        var setlist = new PerformanceSetlist
        {
            Id = "setlist-codec",
            DisplayName = "Codec Setlist",
            Cues = [new PerformanceSetlistCue { Id = "cue-one", Label = "Open", SceneId = "scene-codec" }]
        };

        var decodedFace = MaskPackPayloadCodec.DeserializeFace(MaskPackPayloadCodec.SerializeFace(face));
        var decodedAnimation = MaskPackPayloadCodec.DeserializeAnimation(MaskPackPayloadCodec.SerializeAnimation(animation));
        var decodedText = MaskPackPayloadCodec.DeserializeTextPreset(MaskPackPayloadCodec.SerializeTextPreset(text));
        var decodedPage = MaskPackPayloadCodec.DeserializePage(MaskPackPayloadCodec.SerializePage(page));
        var decodedScene = MaskPackPayloadCodec.DeserializeScene(MaskPackPayloadCodec.SerializeScene(scene));
        var decodedSetlist = MaskPackPayloadCodec.DeserializeSetlist(MaskPackPayloadCodec.SerializeSetlist(setlist));

        Assert.Equal(face.Id, decodedFace.Id);
        Assert.Equal(face.GetPixel(0, 0), decodedFace.GetPixel(0, 0));
        Assert.Equal([80d, 140d], decodedAnimation.Frames.Select(frame => frame.Duration.TotalMilliseconds));
        Assert.Equal(AnimationProjectSource.MaskPackImport, decodedAnimation.Source);
        Assert.False(decodedText.IsSeed);
        Assert.Null(decodedText.LastSentAt);
        Assert.Empty(decodedText.LastSendStatus);
        Assert.Null(decodedPage.Items[0].FastMaskSlot);
        Assert.Empty(decodedPage.Items[0].FastContentFingerprint);
        Assert.Null(decodedPage.Items[0].FastPreparedAt);
        Assert.Equal("face:face-codec", decodedScene.Steps[0].GalleryItemId);
        Assert.Equal("scene-codec", decodedSetlist.Cues[0].SceneId);
    }

    [Fact]
    public void DeserializeFace_InvalidPixelState_IsRejected()
    {
        var face = CreateFace("face-invalid", "Invalid", 0);
        var json = System.Text.Encoding.UTF8.GetString(MaskPackPayloadCodec.SerializeFace(face));
        var packed = Convert.FromBase64String(
            System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("pixels").GetString()!);
        packed[0] = 2;
        using var document = System.Text.Json.JsonDocument.Parse(json);
        var replacement = json.Replace(
            document.RootElement.GetProperty("pixels").GetString()!,
            Convert.ToBase64String(packed),
            StringComparison.Ordinal);

        Assert.Throws<InvalidDataException>(() =>
            MaskPackPayloadCodec.DeserializeFace(System.Text.Encoding.UTF8.GetBytes(replacement)));
    }

    private static FacePattern CreateFace(string id, string name, int litIndex)
    {
        var pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray();
        pixels[litIndex] = new FacePixel(true, new FaceColor(0x12, 0x34, 0x56));
        return new FacePattern
        {
            Id = id,
            DisplayName = name,
            Source = FacePatternSource.Custom,
            Pixels = pixels
        }.Normalize(DateTimeOffset.UnixEpoch);
    }
}
