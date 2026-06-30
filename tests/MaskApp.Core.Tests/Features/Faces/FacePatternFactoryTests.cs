using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class FacePatternFactoryTests
{
    [Fact]
    public void CreateBuiltIns_ReturnsSixDeterministicSmileyEmotions()
    {
        var faces = FacePatternFactory.CreateBuiltIns();

        Assert.Equal(6, faces.Count);
        Assert.Equal(
            [FaceEmotion.Happy, FaceEmotion.Sad, FaceEmotion.Angry, FaceEmotion.Surprised, FaceEmotion.Meh, FaceEmotion.Wink],
            faces.Select(face => face.Emotion).ToArray());
        Assert.All(faces, face =>
        {
            Assert.True(face.IsBuiltIn);
            Assert.Equal(FacePattern.PixelCount, face.Pixels.Length);
            Assert.InRange(face.PreferredSlot, 1, 6);
            Assert.Contains(face.Pixels, pixel => pixel.IsLit);
        });
        Assert.Equal(faces.Count, faces.Select(face => Convert.ToHexString(FaceUploadProtocol.PackLedBits(face))).Distinct().Count());
    }

    [Fact]
    public void CreateBlank_ProducesEditableEmptyFace()
    {
        var face = FacePatternFactory.CreateBlank("Draft", preferredSlot: 12);

        Assert.Equal("Draft", face.DisplayName);
        Assert.Equal(FacePatternSource.Custom, face.Source);
        Assert.Equal(12, face.PreferredSlot);
        Assert.Equal(FacePattern.PixelCount, face.Pixels.Length);
        Assert.DoesNotContain(face.Pixels, pixel => pixel.IsLit);
    }
}
