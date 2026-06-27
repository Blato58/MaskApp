using MaskApp.Core.Features.BuiltIns;

namespace MaskApp.Core.Tests.Features.BuiltIns;

public sealed class BuiltInAssetArchiveTests
{
    [Theory]
    [InlineData(BuiltInAssetType.StaticImage, 0x69, true)]
    [InlineData(BuiltInAssetType.StaticImage, 0x6A, false)]
    [InlineData(BuiltInAssetType.Animation, 0x45, true)]
    [InlineData(BuiltInAssetType.Animation, 0x46, false)]
    public void IsInSafeRange_UsesProtocolDocumentedSafeMaximums(
        BuiltInAssetType type,
        int id,
        bool expected)
    {
        Assert.Equal(expected, BuiltInAssetRange.IsInSafeRange(type, id));
    }

    [Fact]
    public void GetOrCreate_DefaultRecord_FormatsHexAndName()
    {
        var archive = BuiltInAssetArchive.Empty;

        var record = archive.GetOrCreate(BuiltInAssetType.Animation, 3);

        Assert.Equal(3, record.Id);
        Assert.Equal("0x03", record.HexId);
        Assert.Equal("Animation 3", record.DisplayName);
        Assert.Equal(BuiltInAssetStatus.Untested, record.Status);
        Assert.Equal("Never sent", record.LastSendStatus);
    }

    [Fact]
    public void Upsert_ReplacesMatchingTypeAndIdAndNormalizesTags()
    {
        var original = new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 2)
        {
            DisplayName = "Smile",
            Tags = [" face ", "#face", "good"],
            Status = BuiltInAssetStatus.Working
        };
        var updated = original with
        {
            DisplayName = "Big smile",
            Tags = ["favorite", "FACE"],
            IsFavorite = true
        };

        var archive = new BuiltInAssetArchive([original]).Upsert(updated);

        var record = Assert.Single(archive.Records);
        Assert.Equal("Big smile", record.DisplayName);
        Assert.Equal(["favorite", "FACE"], record.Tags);
        Assert.True(record.IsFavorite);
    }

    [Fact]
    public void FavoriteOrTestedRecords_IncludesFavoritesStatusesAndSendTests()
    {
        var testedAt = DateTimeOffset.Parse("2026-06-27T12:00:00+00:00");
        var archive = new BuiltInAssetArchive(
        [
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 1),
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 2) { IsFavorite = true },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 3) { Status = BuiltInAssetStatus.Weird },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 4) { LastTestedAt = testedAt }
        ]);

        var records = archive.FavoriteOrTestedRecords();

        Assert.Equal([2, 3, 4], records.Select(record => record.Id));
    }
}
