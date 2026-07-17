using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class FacePatternFactoryTests
{
    [Fact]
    public void CreateBuiltIns_ReturnsOriginalSmileysAndTwentySevenPixelCharacters()
    {
        var faces = FacePatternFactory.CreateBuiltIns();

        Assert.Equal(33, faces.Count);
        Assert.Equal(
            [FaceEmotion.Happy, FaceEmotion.Sad, FaceEmotion.Angry, FaceEmotion.Surprised, FaceEmotion.Meh, FaceEmotion.Wink],
            faces.Take(6).Select(face => face.Emotion).ToArray());
        Assert.Equal(27, faces.Count(face => face.Id.StartsWith("built-in-face-", StringComparison.Ordinal)));
        Assert.Contains(faces, face => face.DisplayName == "Pixel Cat");
        Assert.Contains(faces, face => face.DisplayName == "Rave DJ");
        Assert.Contains(faces, face => face.DisplayName == "Three-Eyed Monster");
        Assert.All(faces, face =>
        {
            Assert.True(face.IsBuiltIn);
            Assert.Equal(FacePattern.PixelCount, face.Pixels.Length);
            Assert.InRange(face.PreferredSlot, 1, 7);
            Assert.Contains(face.Pixels, pixel => pixel.IsLit);
        });
        Assert.Equal(faces.Count, faces.Select(face => face.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(faces.Count, faces.Select(face => Convert.ToHexString(FaceUploadProtocol.BuildPayload(face))).Distinct().Count());
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

    [Fact]
    public void CreateBuiltIns_UsesFullCanvasWithLayeredColorDetail()
    {
        var faces = FacePatternFactory.CreateBuiltIns();

        Assert.All(faces, face =>
        {
            var litPixels = face.Pixels
                .Select((pixel, index) => (pixel, index))
                .Where(item => item.pixel.IsLit)
                .ToArray();
            var columns = litPixels.Select(item => item.index % FacePattern.Width).ToArray();
            var rows = litPixels.Select(item => item.index / FacePattern.Width).ToArray();

            Assert.True(litPixels.Length >= 600, $"{face.DisplayName} does not have enough drawn detail.");
            Assert.True(columns.Max() - columns.Min() >= 36, $"{face.DisplayName} does not use enough canvas width.");
            Assert.True(rows.Max() - rows.Min() >= 48, $"{face.DisplayName} does not use enough canvas height.");
            Assert.True(
                litPixels.Select(item => item.pixel.Color).Distinct().Count() >= 4,
                $"{face.DisplayName} does not have enough color layers.");
        });
    }

    [Fact]
    public void Normalize_AddsNewSeedFacesAndPreservesExistingFaceState()
    {
        var originalBuiltIns = FacePatternFactory.CreateBuiltIns().Take(6).ToArray();
        var customizedBuiltIn = originalBuiltIns[0] with
        {
            IsFavorite = false,
            PreferredSlot = 12,
            LastUploadStatus = "Uploaded",
            LastUploadedAt = DateTimeOffset.UnixEpoch.AddDays(1)
        };
        var customFace = FacePatternFactory.CreateBlank("My Drawing", preferredSlot: 15);
        var legacyState = new FacePatternStoreState
        {
            SeedVersion = 1,
            Patterns = [customizedBuiltIn, .. originalBuiltIns.Skip(1), customFace]
        };

        var normalized = legacyState.Normalize();

        Assert.Equal(FacePatternStoreState.CurrentSeedVersion, normalized.SeedVersion);
        Assert.Equal(34, normalized.Patterns.Count);
        Assert.Contains(normalized.Patterns, face => face.Id == customFace.Id);
        var migratedBuiltIn = normalized.Patterns.Single(face => face.Id == customizedBuiltIn.Id);
        Assert.False(migratedBuiltIn.IsFavorite);
        Assert.Equal(12, migratedBuiltIn.PreferredSlot);
        Assert.Equal("Uploaded", migratedBuiltIn.LastUploadStatus);
        Assert.Equal(customizedBuiltIn.LastUploadedAt, migratedBuiltIn.LastUploadedAt);
    }
}
