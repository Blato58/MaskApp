using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.Faces;

public sealed class JsonFacePatternStoreCoreTests
{
    [Fact]
    public async Task LoadAsync_MigratesLegacySchemaAndCustomDrawing()
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
                schemaVersion = FacePatternStoreState.LegacySchemaVersion,
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
}
