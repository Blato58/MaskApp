using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class JsonFacePatternStoreCoreTests
{
    [Theory]
    [InlineData(FacePatternStoreState.LegacySchemaVersion)]
    [InlineData(FacePatternStoreState.PreviousSchemaVersion)]
    public async Task LoadAsync_MigratesEarlierSchemaAndCustomDrawing(int schemaVersion)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"maskapp-faces-{Guid.NewGuid():N}.json");
        var legacyPixels = Enumerable.Repeat(FacePixel.Off, 36 * 12).ToArray();
        legacyPixels[(6 * 36) + 18] = new FacePixel(true, new FaceColor(0x52, 0xE3, 0xFF));
        var legacyPattern = new FacePattern
        {
            Id = "legacy-custom",
            DisplayName = "Legacy Custom",
            Source = FacePatternSource.Custom,
            Pixels = legacyPixels
        };
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        try
        {
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(new
            {
                schemaVersion,
                seedVersion = 2,
                patterns = new[] { legacyPattern }
            }, options));

            var state = await new JsonFacePatternStoreCore(filePath).LoadAsync();

            Assert.False(state.UsedFallback);
            Assert.Equal(FacePatternStoreState.CurrentSchemaVersion, state.SchemaVersion);
            var migrated = Assert.Single(state.Patterns, pattern => pattern.Id == legacyPattern.Id);
            Assert.Equal(FacePattern.PixelCount, migrated.Pixels.Length);
            Assert.Contains(migrated.Pixels, pixel => pixel.IsLit && pixel.Color == new FaceColor(0x52, 0xE3, 0xFF));
        }
        finally
        {
            File.Delete(filePath);
            File.Delete($"{filePath}.tmp");
        }
    }

    [Fact]
    public async Task SaveAndLoadAsync_PreservesSlotInstallationLedger()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"maskapp-faces-{Guid.NewGuid():N}.json");
        var pattern = FacePatternFactory.CreateBlank("Installed", 19)
            .WithPixel(2, 3, new FacePixel(true, new FaceColor(0x12, 0x34, 0x56)));
        var timestamp = DateTimeOffset.UtcNow;
        var store = new JsonFacePatternStoreCore(filePath);

        try
        {
            var state = new FacePatternStoreState { Patterns = [pattern] }
                .MarkSlotInstalled(19, FaceContentFingerprint.Compute(pattern), pattern.Id, timestamp);

            await store.SaveAsync(state);
            var loaded = await store.LoadAsync();

            var installation = loaded.GetSlotInstallation(19);
            Assert.NotNull(installation);
            Assert.Equal(FaceContentFingerprint.Compute(pattern), installation.ContentFingerprint);
            Assert.Equal(pattern.Id, installation.SourceId);
            Assert.Equal(timestamp, installation.InstalledAt);
        }
        finally
        {
            File.Delete(filePath);
            File.Delete($"{filePath}.tmp");
        }
    }
}
