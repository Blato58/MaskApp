using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Animations;

public sealed class AnimationProjectStoreTests
{
    [Fact]
    public async Task JsonStore_RoundTripsCompactFramesAndMetadata()
    {
        var path = CreateTempPath();
        try
        {
            var store = new JsonAnimationProjectStoreCore(path);
            var project = AnimationProjectCompilerTests.CreateProject(
                new AnimationProjectFrame
                {
                    Id = "frame-a",
                    Pattern = AnimationProjectCompilerTests.CreateSolidPattern("White"),
                    Duration = TimeSpan.FromMilliseconds(125)
                }) with
            {
                Source = AnimationProjectSource.GifImport,
                LoopMode = AnimationLoopMode.Finite,
                FiniteLoopCount = 4,
                Bpm = 128
            };

            await store.SaveAsync(new AnimationProjectStoreState { Projects = [project] });
            var loaded = await store.LoadAsync();

            var actual = Assert.Single(loaded.Projects);
            Assert.Equal(AnimationProjectSource.GifImport, actual.Source);
            Assert.Equal(AnimationLoopMode.Finite, actual.LoopMode);
            Assert.Equal(4, actual.FiniteLoopCount);
            Assert.Equal(128, actual.Bpm);
            Assert.Equal(TimeSpan.FromMilliseconds(125), Assert.Single(actual.Frames).Duration);
            Assert.Equal(
                FaceContentFingerprint.Compute(project.Frames[0].Pattern),
                FaceContentFingerprint.Compute(actual.Frames[0].Pattern));
        }
        finally
        {
            DeleteTempFiles(path);
        }
    }

    [Fact]
    public async Task JsonStore_CorruptDataLoadsFallback_AndCannotBeOverwritten()
    {
        var path = CreateTempPath();
        try
        {
            await File.WriteAllTextAsync(path, "{ not json");
            var store = new JsonAnimationProjectStoreCore(path);

            var fallback = await store.LoadAsync();
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => store.SaveAsync(fallback));

            Assert.True(fallback.UsedFallback);
            Assert.Empty(fallback.Projects);
            Assert.Contains("cannot be overwritten", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("{ not json", await File.ReadAllTextAsync(path));
        }
        finally
        {
            DeleteTempFiles(path);
        }
    }

    [Fact]
    public async Task JsonStore_RejectsMalformedPixelPayloadWithoutPartiallyLoading()
    {
        var path = CreateTempPath();
        try
        {
            await File.WriteAllTextAsync(path, """
                {"schemaVersion":1,"projects":[{"id":"bad","displayName":"Bad","frames":[{"id":"f","durationMilliseconds":75,"pixels":"AA=="}]}]}
                """);

            var loaded = await new JsonAnimationProjectStoreCore(path).LoadAsync();

            Assert.True(loaded.UsedFallback);
            Assert.Empty(loaded.Projects);
        }
        finally
        {
            DeleteTempFiles(path);
        }
    }

    private static string CreateTempPath()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"maskapp-animation-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "animations.json");
    }

    private static void DeleteTempFiles(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
