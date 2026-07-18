using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using MaskApp.Core.Features.AnimationPacks;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Tests.Features.AnimationPacks;

public sealed class MaskPackArchiveSecurityAndMigrationTests
{
    [Fact]
    public async Task Inspect_V1Archive_MigratesStaticArtAndAnimationTimingTo46x58()
    {
        var red = CreateRgbaPng(44, 58, 0xEE, 0x11, 0x22);
        var blue = CreateRgbaPng(44, 58, 0x11, 0x33, 0xEE);
        var manifest = new MaskPackManifest
        {
            SchemaVersion = 1,
            PackName = "Legacy Show",
            Author = "Legacy Artist",
            TargetDisplay = new MaskPackDisplayGeometry { Width = 44, Height = 58 },
            Assets =
            [
                new MaskPackAsset
                {
                    Id = "legacy-face",
                    Type = MaskPackAssetType.StaticImage,
                    Name = "Legacy Face",
                    FrameDurationMs = 250,
                    Frames = [new MaskPackFrame { Path = "frames/face.png" }]
                },
                new MaskPackAsset
                {
                    Id = "legacy-animation",
                    Type = MaskPackAssetType.Animation,
                    Name = "Legacy Animation",
                    FrameDurationMs = 100,
                    Loop = true,
                    Frames =
                    [
                        new MaskPackFrame { Path = "frames/red.png", DurationMs = 80 },
                        new MaskPackFrame { Path = "frames/blue.png", DurationMs = 160 }
                    ]
                }
            ]
        };
        await using var archive = Archive(
            manifest,
            ("frames/face.png", red),
            ("frames/red.png", red),
            ("frames/blue.png", blue));
        var faceStore = new InMemoryFacePatternStore();
        var animationStore = new InMemoryAnimationProjectStore();
        var service = MaskPackArchiveRoundTripTests.CreateService(
            new MaskApp.Core.Features.TextPresets.InMemoryTextPresetStore(),
            faceStore,
            animationStore,
            new MaskApp.Core.Features.Gallery.InMemoryGalleryLayoutStore(),
            new MaskApp.Core.Features.Scenes.InMemorySceneShowStore());

        var inspection = await service.InspectAsync(archive);

        Assert.True(inspection.IsValid, string.Join(" ", inspection.Errors));
        Assert.True(inspection.MigratedFromV1);
        Assert.Contains(inspection.Warnings, warning => warning.Contains("one off column on each side", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(inspection.Warnings, warning => warning.Contains("no payload hashes", StringComparison.OrdinalIgnoreCase));
        var face = Assert.Single(inspection.Package!.Faces).Value;
        Assert.False(face.GetPixel(0, 0).IsLit);
        Assert.True(face.GetPixel(1, 0).IsLit);
        Assert.True(face.GetPixel(44, 57).IsLit);
        Assert.False(face.GetPixel(45, 57).IsLit);
        var animation = Assert.Single(inspection.Package.Animations).Value;
        Assert.Equal([80d, 160d], animation.Frames.Select(frame => frame.Duration.TotalMilliseconds));
        Assert.True(animation.Frames[0].Pattern.GetPixel(1, 0).IsLit);
        Assert.Equal(0xEE, animation.Frames[0].Pattern.GetPixel(1, 0).Color.Red);
        Assert.Equal(0xEE, animation.Frames[1].Pattern.GetPixel(1, 0).Color.Blue);

        var imported = await service.ImportAsync(new MaskPackImportRequest { Inspection = inspection });

        Assert.True(imported.Succeeded, imported.Message);
        Assert.Single((await faceStore.LoadAsync()).Patterns, item => item.Id == "legacy-face");
        Assert.Single((await animationStore.LoadAsync()).Projects, item => item.Id == "legacy-animation");
    }

    [Fact]
    public async Task Inspect_PathTraversalEntry_IsRejectedWithoutExtraction()
    {
        var face = MaskPackArchiveRoundTripTests.CreateFace("face", "Face", 0, new FaceColor(1, 2, 3));
        var bytes = MaskPackPayloadCodec.SerializeFace(face);
        var manifest = V2Manifest(new MaskPackContentEntry
        {
            Id = face.Id,
            Type = MaskPackContentType.Face,
            Name = face.DisplayName,
            Path = "../escape.json",
            Sha256 = Hash(bytes)
        });
        await using var archive = Archive(manifest, ("../escape.json", bytes));

        var inspection = await DefaultService().InspectAsync(archive);

        Assert.False(inspection.IsValid);
        Assert.Contains(inspection.Errors, error => error.Contains("unsafe path", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Inspect_CaseInsensitiveDuplicatePath_IsRejected()
    {
        var manifest = V2Manifest(new MaskPackContentEntry
        {
            Id = "face",
            Type = MaskPackContentType.Face,
            Name = "Face",
            Path = "content/face.json",
            Sha256 = new string('0', 64)
        });
        await using var archive = Archive(
            manifest,
            ("content/face.json", [1]),
            ("CONTENT/FACE.JSON", [2]));

        var inspection = await DefaultService().InspectAsync(archive);

        Assert.False(inspection.IsValid);
        Assert.Contains(inspection.Errors, error => error.Contains("duplicate path", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Inspect_UnlistedFileAndHashMismatch_AreRejected()
    {
        var face = MaskPackArchiveRoundTripTests.CreateFace("face", "Face", 0, new FaceColor(1, 2, 3));
        var bytes = MaskPackPayloadCodec.SerializeFace(face);
        var entry = new MaskPackContentEntry
        {
            Id = face.Id,
            Type = MaskPackContentType.Face,
            Name = face.DisplayName,
            Path = "content/face.json",
            Sha256 = new string('0', 64)
        };
        await using var hashArchive = Archive(V2Manifest(entry), (entry.Path, bytes));
        await using var unlistedArchive = Archive(
            V2Manifest(entry with { Sha256 = Hash(bytes) }),
            (entry.Path, bytes),
            ("content/unlisted.json", [1, 2, 3]));

        var hashInspection = await DefaultService().InspectAsync(hashArchive);
        var unlistedInspection = await DefaultService().InspectAsync(unlistedArchive);

        Assert.Contains(hashInspection.Errors, error => error.Contains("hash mismatch", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(unlistedInspection.Errors, error => error.Contains("unlisted entries", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(MaskPackContentType.Animation, "animation", "{\"id\":\"animation\",\"displayName\":\"Animation\",\"frames\":null}")]
    [InlineData(MaskPackContentType.Page, "page", "{\"pageId\":\"page\",\"title\":\"Page\",\"items\":null}")]
    [InlineData(MaskPackContentType.Appearance, "appearance", "{\"galleryOrder\":null}")]
    public async Task Inspect_NullRequiredPayloadCollections_AreRejectedAsInvalidData(
        MaskPackContentType type,
        string id,
        string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var entry = new MaskPackContentEntry
        {
            Id = id,
            Type = type,
            Name = "Invalid payload",
            Path = $"content/{id}.json",
            Sha256 = Hash(bytes)
        };
        await using var archive = Archive(V2Manifest(entry), (entry.Path, bytes));

        var inspection = await DefaultService().InspectAsync(archive);

        Assert.False(inspection.IsValid);
        Assert.NotEmpty(inspection.Errors);
    }

    [Fact]
    public async Task Inspect_CorruptZipAndDeclaredOversizedArchive_AreRejected()
    {
        await using var corrupt = new MemoryStream([1, 2, 3, 4, 5]);
        await using var oversized = new DeclaredLengthStream(MaskPackArchiveService.MaxArchiveBytes + 1);

        var corruptInspection = await DefaultService().InspectAsync(corrupt);
        var oversizedInspection = await DefaultService().InspectAsync(oversized);

        Assert.False(corruptInspection.IsValid);
        Assert.Contains(oversizedInspection.Errors, error => error.Contains("32 MB limit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Inspect_ExcessiveEntryCount_IsRejectedBeforeManifestDecode()
    {
        var result = new MemoryStream();
        using (var zip = new ZipArchive(result, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (var index = 0; index <= MaskPackArchiveService.MaxArchiveEntries; index++)
            {
                MaskPackConflictAndRecoveryTests.Write(zip, $"content/{index:000}.json", [1]);
            }
        }

        result.Position = 0;
        await using var archive = result;

        var inspection = await DefaultService().InspectAsync(archive);

        Assert.False(inspection.IsValid);
        Assert.Contains(inspection.Errors, error => error.Contains("between 1 and 300 files", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Inspect_EntryLargerThanEightMegabytes_IsRejectedBeforeDecode()
    {
        var payload = new byte[MaskPackArchiveService.MaxEntryBytes + 1];
        var entry = new MaskPackContentEntry
        {
            Id = "oversized",
            Type = MaskPackContentType.Face,
            Name = "Oversized",
            Path = "content/oversized.json",
            Sha256 = Hash(payload)
        };
        await using var archive = Archive(V2Manifest(entry), (entry.Path, payload));

        var inspection = await DefaultService().InspectAsync(archive);

        Assert.False(inspection.IsValid);
        Assert.Contains(inspection.Errors, error => error.Contains("8 MB limit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Inspect_ExtremeCompressionRatio_IsRejectedAsZipBomb()
    {
        var payload = new byte[2 * 1024 * 1024];
        var entry = new MaskPackContentEntry
        {
            Id = "compressed",
            Type = MaskPackContentType.Face,
            Name = "Compressed",
            Path = "content/compressed.json",
            Sha256 = Hash(payload)
        };
        await using var archive = Archive(V2Manifest(entry), (entry.Path, payload));

        var inspection = await DefaultService().InspectAsync(archive);

        Assert.False(inspection.IsValid);
        Assert.Contains(inspection.Errors, error => error.Contains("unsafe compression ratio", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void LegacyPng_WrongDimensionsAndCorruptCrc_AreRejected()
    {
        var wrongSize = CreateRgbaPng(43, 58, 1, 2, 3);
        var corruptCrc = CreateRgbaPng(44, 58, 1, 2, 3);
        corruptCrc[^1] ^= 0xFF;

        Assert.Throws<InvalidDataException>(() =>
            LegacyMaskPackPngDecoder.Decode44x58(wrongSize, "wrong", "Wrong", 7));
        var exception = Assert.Throws<InvalidDataException>(() =>
            LegacyMaskPackPngDecoder.Decode44x58(corruptCrc, "crc", "CRC", 7));
        Assert.Contains("checksum", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static MaskPackArchiveService DefaultService() =>
        MaskPackArchiveRoundTripTests.CreateService(
            new MaskApp.Core.Features.TextPresets.InMemoryTextPresetStore(),
            new InMemoryFacePatternStore(),
            new MaskApp.Core.Features.Animations.InMemoryAnimationProjectStore(),
            new MaskApp.Core.Features.Gallery.InMemoryGalleryLayoutStore(),
            new MaskApp.Core.Features.Scenes.InMemorySceneShowStore());

    private static MaskPackManifest V2Manifest(params MaskPackContentEntry[] contents) => new()
    {
        SchemaVersion = 2,
        PackName = "Security Test",
        ArtDisplay = new MaskPackDisplayGeometry { Width = 46, Height = 58 },
        TextDisplay = new MaskPackDisplayGeometry { Width = 44, Height = 58 },
        Contents = contents
    };

    private static MemoryStream Archive(
        MaskPackManifest manifest,
        params (string Path, byte[] Bytes)[] files)
    {
        var result = new MemoryStream();
        using (var zip = new ZipArchive(result, ZipArchiveMode.Create, leaveOpen: true))
        {
            MaskPackConflictAndRecoveryTests.Write(
                zip,
                "manifest.json",
                Encoding.UTF8.GetBytes(MaskPackManifestParser.ToJson(manifest)));
            foreach (var file in files)
            {
                MaskPackConflictAndRecoveryTests.Write(zip, file.Path, file.Bytes);
            }
        }

        result.Position = 0;
        return result;
    }

    private static string Hash(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static byte[] CreateRgbaPng(int width, int height, byte red, byte green, byte blue)
    {
        var raw = new byte[checked(height * ((width * 4) + 1))];
        var offset = 0;
        for (var row = 0; row < height; row++)
        {
            raw[offset++] = 0;
            for (var column = 0; column < width; column++)
            {
                raw[offset++] = red;
                raw[offset++] = green;
                raw[offset++] = blue;
                raw[offset++] = 0xFF;
            }
        }

        byte[] compressed;
        using (var buffer = new MemoryStream())
        {
            using (var zlib = new ZLibStream(buffer, CompressionLevel.Optimal, leaveOpen: true))
            {
                zlib.Write(raw);
            }

            compressed = buffer.ToArray();
        }

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        var header = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0, 4), checked((uint)width));
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), checked((uint)height));
        header[8] = 8;
        header[9] = 6;
        WritePngChunk(png, "IHDR"u8, header);
        WritePngChunk(png, "IDAT"u8, compressed);
        WritePngChunk(png, "IEND"u8, []);
        return png.ToArray();
    }

    private static void WritePngChunk(Stream destination, ReadOnlySpan<byte> type, byte[] data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, checked((uint)data.Length));
        destination.Write(length);
        destination.Write(type);
        destination.Write(data);
        var crcInput = new byte[type.Length + data.Length];
        type.CopyTo(crcInput);
        data.CopyTo(crcInput, type.Length);
        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32(crcInput));
        destination.Write(crc);
    }

    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        var crc = uint.MaxValue;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) == 0 ? crc >> 1 : (crc >> 1) ^ 0xEDB88320u;
            }
        }

        return crc ^ uint.MaxValue;
    }

    private sealed class DeclaredLengthStream(long length) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => length;
        public override long Position { get; set; }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => 0;
        public override long Seek(long offset, SeekOrigin origin) => Position;
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
