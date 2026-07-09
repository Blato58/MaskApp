using MaskApp.Core.Features.BuiltIns;

namespace MaskApp.Core.Tests.Features.BuiltIns;

public sealed class BuiltInAssetArchiveTests
{
    [Theory]
    [InlineData(BuiltInAssetType.StaticImage, 69, true)]
    [InlineData(BuiltInAssetType.StaticImage, 70, false)]
    [InlineData(BuiltInAssetType.Animation, 3, true)]
    [InlineData(BuiltInAssetType.Animation, 4, false)]
    [InlineData(BuiltInAssetType.Animation, 5, true)]
    [InlineData(BuiltInAssetType.Animation, 45, true)]
    [InlineData(BuiltInAssetType.Animation, 46, false)]
    public void IsInSafeRange_UsesAndroidCatalogIds(
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
        Assert.Equal("Animation 03", record.DisplayName);
        Assert.Equal(BuiltInAssetStatus.Untested, record.Status);
        Assert.Equal("Never sent", record.LastSendStatus);
    }

    [Fact]
    public void Normalize_FillsBlankAndOldGeneratedNamesFromCatalog()
    {
        var archive = new BuiltInAssetArchive(
        [
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 7) { DisplayName = " " },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 5) { DisplayName = "Animation 5" }
        ]);

        Assert.Equal("Face 07", archive.GetOrCreate(BuiltInAssetType.StaticImage, 7).DisplayName);
        Assert.Equal("Animation 05", archive.GetOrCreate(BuiltInAssetType.Animation, 5).DisplayName);
    }

    [Fact]
    public void Normalize_PreservesCustomNamesAndUnknownIds()
    {
        var archive = new BuiltInAssetArchive(
        [
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 4)
            {
                DisplayName = "My weird skipped slot",
                IsFavorite = true
            }
        ]);

        var record = Assert.Single(archive.Records);
        Assert.Equal(4, record.Id);
        Assert.Equal("My weird skipped slot", record.DisplayName);
        Assert.False(BuiltInAssetCatalog.IsKnown(record.Type, record.Id));
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

    [Fact]
    public void FavoriteDeckRecords_IncludesFavoritesAndWorkingOnly()
    {
        var archive = new BuiltInAssetArchive(
        [
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 1),
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 2) { Status = BuiltInAssetStatus.Working },
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 3) { Status = BuiltInAssetStatus.Bad },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 4) { Status = BuiltInAssetStatus.Untested, IsFavorite = true },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 5) { Status = BuiltInAssetStatus.Weird }
        ]);

        var records = archive.FavoriteDeckRecords();

        Assert.Equal([4, 2], records.Select(record => record.Id));
    }

    [Fact]
    public void FavoriteDeckRecords_SortsFavoritesThenWorkingThenRecent()
    {
        var older = DateTimeOffset.Parse("2026-06-27T10:00:00+00:00");
        var newer = DateTimeOffset.Parse("2026-06-27T12:00:00+00:00");
        var archive = new BuiltInAssetArchive(
        [
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 1) { Status = BuiltInAssetStatus.Working, LastTestedAt = older },
            new BuiltInAssetRecord(BuiltInAssetType.StaticImage, 2) { Status = BuiltInAssetStatus.Working, LastTestedAt = newer },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 3) { IsFavorite = true, LastUpdatedAt = older },
            new BuiltInAssetRecord(BuiltInAssetType.Animation, 4) { IsFavorite = true, LastUpdatedAt = newer }
        ]);

        var records = archive.FavoriteDeckRecords();

        Assert.Equal([4, 3, 2, 1], records.Select(record => record.Id));
    }

    [Fact]
    public void FavoriteDeckRecords_EmptyArchive_ReturnsEmpty()
    {
        Assert.Empty(BuiltInAssetArchive.Empty.FavoriteDeckRecords());
    }
}
