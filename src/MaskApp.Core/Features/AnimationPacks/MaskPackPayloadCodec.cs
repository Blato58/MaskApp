using System.Text.Json;
using System.Text.Json.Serialization;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.AnimationPacks;

public static class MaskPackPayloadCodec
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static byte[] SerializeFace(FacePattern source)
    {
        var face = source.Normalize();
        return Serialize(new FaceDocument
        {
            Id = face.Id,
            DisplayName = face.DisplayName,
            Emotion = face.Emotion,
            PreferredSlot = face.PreferredSlot,
            IsFavorite = face.IsFavorite,
            Pixels = PackPixels(face)
        });
    }

    public static FacePattern DeserializeFace(ReadOnlySpan<byte> payload)
    {
        var document = Deserialize<FaceDocument>(payload);
        if (string.IsNullOrWhiteSpace(document.Id) || document.Id.Length > 128
            || string.IsNullOrWhiteSpace(document.DisplayName) || document.DisplayName.Length > 160
            || !Enum.IsDefined(document.Emotion)
            || document.PreferredSlot is < FacePattern.MinSlot or > FacePattern.MaxSlot)
        {
            throw new InvalidDataException("Face payload has an invalid id, name, or preferred slot.");
        }

        return new FacePattern
        {
            Id = document.Id,
            DisplayName = document.DisplayName,
            Emotion = document.Emotion,
            Source = FacePatternSource.ImportedPhoto,
            PreferredSlot = document.PreferredSlot,
            IsFavorite = document.IsFavorite,
            Pixels = UnpackPixels(document.Pixels)
        }.Normalize();
    }

    public static byte[] SerializeAnimation(AnimationProject source)
    {
        var animation = source.Normalize();
        return Serialize(new AnimationDocument
        {
            Id = animation.Id,
            DisplayName = animation.DisplayName,
            LoopMode = animation.LoopMode,
            FiniteLoopCount = animation.FiniteLoopCount,
            Bpm = animation.Bpm,
            IsFavorite = animation.IsFavorite,
            Frames = animation.Frames.Select(frame => new AnimationFrameDocument
            {
                Id = frame.Id,
                DurationMilliseconds = frame.Duration.TotalMilliseconds,
                Pixels = PackPixels(frame.Pattern)
            }).ToArray()
        });
    }

    public static AnimationProject DeserializeAnimation(ReadOnlySpan<byte> payload)
    {
        var document = Deserialize<AnimationDocument>(payload);
        if (document.Frames is null || document.Frames.Length is < 1 or > AnimationProject.MaxSourceFrames)
        {
            throw new InvalidDataException("Animation payload frame count is outside the safe limit.");
        }

        if (string.IsNullOrWhiteSpace(document.Id)
            || document.Id.Length > 128
            || string.IsNullOrWhiteSpace(document.DisplayName)
            || document.DisplayName.Length > 160
            || !Enum.IsDefined(document.LoopMode)
            || document.Frames.Any(frame => frame is null || string.IsNullOrWhiteSpace(frame.Id))
            || document.Frames.Any(frame => frame.Id.Length > 128)
            || document.Frames.Select(frame => frame.Id).Distinct(StringComparer.Ordinal).Count() != document.Frames.Length)
        {
            throw new InvalidDataException("Animation payload requires stable unique ids for the project and every frame.");
        }

        return new AnimationProject
        {
            Id = document.Id,
            DisplayName = document.DisplayName,
            Source = AnimationProjectSource.MaskPackImport,
            LoopMode = document.LoopMode,
            FiniteLoopCount = document.FiniteLoopCount,
            Bpm = document.Bpm,
            IsFavorite = document.IsFavorite,
            Frames = document.Frames.Select((frame, index) => new AnimationProjectFrame
            {
                Id = frame.Id,
                Duration = TimeSpan.FromMilliseconds(frame.DurationMilliseconds),
                Pattern = new FacePattern
                {
                    Id = $"maskpack-frame-{index + 1}",
                    DisplayName = $"Frame {index + 1}",
                    Pixels = UnpackPixels(frame.Pixels)
                }
            }).ToArray()
        }.Normalize();
    }

    public static byte[] SerializeTextPreset(TextPreset source)
    {
        var preset = source.Normalize(DateTimeOffset.UnixEpoch) with
        {
            CreatedAt = default,
            UpdatedAt = default,
            LastSentAt = null,
            LastSendStatus = string.Empty,
            IsSeed = false
        };
        return Serialize(preset);
    }

    public static TextPreset DeserializeTextPreset(ReadOnlySpan<byte> payload)
    {
        var preset = Deserialize<TextPreset>(payload);
        if (string.IsNullOrWhiteSpace(preset.Id.Value) || preset.Id.Value.Length > 128
            || string.IsNullOrWhiteSpace(preset.DisplayName) || preset.DisplayName.Length > 160
            || preset.InputText is null || preset.MaskText is null
            || preset.Tags is null || preset.Tags.Count > 32
            || preset.Tags.Any(tag => tag is null || tag.Length > 80)
            || preset.Style is null || preset.Visibility is null
            || !Enum.IsDefined(preset.Category)
            || !Enum.IsDefined(preset.Style.LayoutMode)
            || !Enum.IsDefined(preset.Style.DisplayMode)
            || !Enum.IsDefined(preset.Style.SendProfile))
        {
            throw new InvalidDataException("Text preset payload has invalid required fields or collection limits.");
        }

        return preset with
        {
            CreatedAt = default,
            UpdatedAt = default,
            LastSentAt = null,
            LastSendStatus = string.Empty,
            IsSeed = false
        };
    }

    public static byte[] SerializePage(GalleryPageLayout source)
    {
        var normalized = source.Normalize(source.SortIndex);
        var page = normalized with
        {
            Items = normalized.Items.Select(item => item with
            {
                FastMaskSlot = null,
                FastContentFingerprint = string.Empty,
                FastPreparedAt = null
            }).ToArray()
        };
        return Serialize(page);
    }

    public static GalleryPageLayout DeserializePage(ReadOnlySpan<byte> payload)
    {
        var raw = Deserialize<GalleryPageLayout>(payload);
        if (raw.Items is null || raw.Items.Count > 64
            || string.IsNullOrWhiteSpace(raw.PageId) || raw.PageId.Length > 128
            || string.IsNullOrWhiteSpace(raw.Title) || raw.Title.Length > 160
            || raw.Items.Any(item => item is null || string.IsNullOrWhiteSpace(item.SlotId) || string.IsNullOrWhiteSpace(item.GalleryItemId))
            || raw.Items.Any(item => item.SlotId.Length > 128 || item.GalleryItemId.Length > 200
                                     || item.Label is null || item.Label.Length > 160)
            || raw.Items.Select(item => item.SlotId).Distinct(StringComparer.Ordinal).Count() != raw.Items.Count)
        {
            throw new InvalidDataException("Page payload requires a stable Page id and unique shortcut ids with content references.");
        }

        var page = raw.Normalize(0);
        return page with
        {
            Items = page.Items.Select(item => item with
            {
                FastMaskSlot = null,
                FastContentFingerprint = string.Empty,
                FastPreparedAt = null
            }).ToArray()
        };
    }

    public static byte[] SerializeScene(PerformanceScene source)
    {
        var scene = source.Normalize(DateTimeOffset.UnixEpoch) with
        {
            CreatedAt = default,
            UpdatedAt = default
        };
        return Serialize(scene);
    }

    public static PerformanceScene DeserializeScene(ReadOnlySpan<byte> payload)
    {
        var scene = Deserialize<PerformanceScene>(payload);
        if (scene.Steps is null
            || string.IsNullOrWhiteSpace(scene.Id) || scene.Id.Length > 128
            || string.IsNullOrWhiteSpace(scene.DisplayName) || scene.DisplayName.Length > 160
            || !Enum.IsDefined(scene.FailurePolicy)
            || scene.Steps.Any(step => step is null || string.IsNullOrWhiteSpace(step.Id) || step.Id.Length > 128
                                       || !Enum.IsDefined(step.Kind)
                                       || (step.GalleryItemId?.Length ?? 0) > 200))
        {
            throw new InvalidDataException("Scene payload requires stable ids for the Scene and every step.");
        }

        return scene.Normalize();
    }

    public static byte[] SerializeSetlist(PerformanceSetlist source)
    {
        var setlist = source.Normalize(DateTimeOffset.UnixEpoch) with
        {
            CreatedAt = default,
            UpdatedAt = default
        };
        return Serialize(setlist);
    }

    public static PerformanceSetlist DeserializeSetlist(ReadOnlySpan<byte> payload)
    {
        var setlist = Deserialize<PerformanceSetlist>(payload);
        if (setlist.Cues is null
            || string.IsNullOrWhiteSpace(setlist.Id) || setlist.Id.Length > 128
            || string.IsNullOrWhiteSpace(setlist.DisplayName) || setlist.DisplayName.Length > 160
            || setlist.Cues.Any(cue => cue is null || string.IsNullOrWhiteSpace(cue.Id) || cue.Id.Length > 128
                                       || string.IsNullOrWhiteSpace(cue.SceneId) || cue.SceneId.Length > 128
                                       || cue.Label is null || cue.Label.Length > 160))
        {
            throw new InvalidDataException("Setlist payload requires stable ids for the setlist and every cue.");
        }

        return setlist.Normalize();
    }

    public static byte[] SerializeAppearance(MaskPackAppearanceSettings source) => Serialize(source);

    public static MaskPackAppearanceSettings DeserializeAppearance(ReadOnlySpan<byte> payload)
    {
        var appearance = Deserialize<MaskPackAppearanceSettings>(payload);
        var order = appearance.GalleryOrder;
        if (order is null || order.ItemOrders is null || order.GroupOrders is null
            || order.ItemOrders.Count > 2048 || order.GroupOrders.Count > 256
            || order.ItemOrders.Any(item => item is null || string.IsNullOrWhiteSpace(item.ItemId) || item.ItemId.Length > 200)
            || order.GroupOrders.Any(group => group is null || string.IsNullOrWhiteSpace(group.GroupKey) || group.GroupKey.Length > 200)
            || order.ItemOrders.Select(item => item.ItemId).Distinct(StringComparer.Ordinal).Count() != order.ItemOrders.Count
            || order.GroupOrders.Select(group => group.GroupKey).Distinct(StringComparer.Ordinal).Count() != order.GroupOrders.Count)
        {
            throw new InvalidDataException("Appearance payload has invalid or excessive Library ordering data.");
        }

        return appearance;
    }

    private static byte[] Serialize<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);

    private static T Deserialize<T>(ReadOnlySpan<byte> payload)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payload, SerializerOptions)
                ?? throw new InvalidDataException($"MaskPack {typeof(T).Name} payload is empty.");
        }
        catch (Exception exception) when (
            exception is JsonException or ArgumentException or FormatException or OverflowException)
        {
            throw new InvalidDataException($"MaskPack {typeof(T).Name} payload is invalid.", exception);
        }
    }

    private static string PackPixels(FacePattern source)
    {
        var pattern = source.Normalize();
        var bytes = new byte[FacePattern.PixelCount * 4];
        for (var index = 0; index < pattern.Pixels.Length; index++)
        {
            var pixel = pattern.Pixels[index].Normalize();
            var offset = index * 4;
            bytes[offset] = pixel.IsLit ? (byte)1 : (byte)0;
            bytes[offset + 1] = pixel.Color.Red;
            bytes[offset + 2] = pixel.Color.Green;
            bytes[offset + 3] = pixel.Color.Blue;
        }

        return Convert.ToBase64String(bytes);
    }

    private static FacePixel[] UnpackPixels(string encoded)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(encoded ?? string.Empty);
        }
        catch (FormatException exception)
        {
            throw new InvalidDataException("MaskPack pixel payload is not valid base64.", exception);
        }
        if (bytes.Length != FacePattern.PixelCount * 4)
        {
            throw new InvalidDataException(
                $"MaskPack DIY art must contain exactly {FacePattern.Width}x{FacePattern.Height} pixels.");
        }

        var pixels = new FacePixel[FacePattern.PixelCount];
        for (var index = 0; index < pixels.Length; index++)
        {
            var offset = index * 4;
            if (bytes[offset] is not (0 or 1))
            {
                throw new InvalidDataException("MaskPack pixel state must be 0 or 1.");
            }

            pixels[index] = bytes[offset] == 1
                ? new FacePixel(true, new FaceColor(bytes[offset + 1], bytes[offset + 2], bytes[offset + 3]))
                : FacePixel.Off;
        }

        return pixels;
    }

    private sealed class FaceDocument
    {
        public int FormatVersion { get; init; } = 1;
        public string Id { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public FaceEmotion Emotion { get; init; }
        public int PreferredSlot { get; init; } = 7;
        public bool IsFavorite { get; init; }
        public string Pixels { get; init; } = string.Empty;
    }

    private sealed class AnimationDocument
    {
        public int FormatVersion { get; init; } = 1;
        public string Id { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public AnimationLoopMode LoopMode { get; init; }
        public int FiniteLoopCount { get; init; } = 1;
        public double? Bpm { get; init; }
        public bool IsFavorite { get; init; }
        public AnimationFrameDocument[] Frames { get; init; } = [];
    }

    private sealed class AnimationFrameDocument
    {
        public string Id { get; init; } = string.Empty;
        public double DurationMilliseconds { get; init; }
        public string Pixels { get; init; } = string.Empty;
    }
}
