using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskApp.Core.Features.AnimationPacks;

public static class MaskPackManifestParser
{
    public const int RequiredWidth = 44;
    public const int RequiredHeight = 58;
    public const int ArtWidth = 46;
    public const int TextWidth = 44;
    public const int MaxContentEntries = 256;
    public const int ReasonableFrameCount = 120;
    public const int HighFrameCountWarningThreshold = 24;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static MaskPackValidationResult ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return MaskPackValidationResult.Failed("Manifest JSON is empty.");
        }

        try
        {
            var manifest = JsonSerializer.Deserialize<MaskPackManifest>(json, SerializerOptions);
            return Validate(manifest);
        }
        catch (JsonException ex)
        {
            return MaskPackValidationResult.Failed($"Manifest JSON is invalid: {ex.Message}");
        }
    }

    public static string ToJson(MaskPackManifest manifest) =>
        JsonSerializer.Serialize(manifest, SerializerOptions);

    public static MaskPackValidationResult Validate(MaskPackManifest? manifest)
    {
        if (manifest is null)
        {
            return MaskPackValidationResult.Failed("Manifest is missing.");
        }

        var errors = new List<string>();
        var warnings = new List<string>();

        if (manifest.SchemaVersion is not (1 or MaskPackManifest.CurrentSchemaVersion))
        {
            errors.Add($"schemaVersion must be 1 or {MaskPackManifest.CurrentSchemaVersion}.");
        }

        if (string.IsNullOrWhiteSpace(manifest.PackName) || manifest.PackName.Length > 160)
        {
            errors.Add("packName is required and must be at most 160 characters.");
        }

        if ((manifest.Author?.Length ?? 0) > 160)
        {
            errors.Add("author must be at most 160 characters.");
        }

        if ((manifest.Source?.Length ?? 0) > 240)
        {
            errors.Add("source must be at most 240 characters.");
        }

        if (manifest.SchemaVersion == 1)
        {
            ValidateV1(manifest, errors, warnings);
        }
        else
        {
            ValidateV2(manifest, errors);
        }

        return new MaskPackValidationResult(manifest, errors, warnings);
    }

    private static void ValidateV1(
        MaskPackManifest manifest,
        List<string> errors,
        List<string> warnings)
    {
        if (manifest.TargetDisplay is null)
        {
            errors.Add("targetDisplay is required.");
            return;
        }

        if (manifest.TargetDisplay.Width != RequiredWidth)
        {
            errors.Add($"targetDisplay.width must be {RequiredWidth}.");
        }

        if (manifest.TargetDisplay.Height != RequiredHeight)
        {
            errors.Add($"targetDisplay.height must be {RequiredHeight}.");
        }

        var assets = manifest.Assets ?? [];
        if (assets.Length is < 1 or > MaxContentEntries)
        {
            errors.Add($"assets must contain between 1 and {MaxContentEntries} entries.");
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var asset in assets)
        {
            if (asset is null)
            {
                errors.Add("Asset entries cannot be null.");
                continue;
            }

            ValidateAsset(asset, errors, warnings);
            if (!string.IsNullOrWhiteSpace(asset.Id) && !ids.Add(asset.Id))
            {
                errors.Add($"Duplicate asset id {asset.Id}.");
            }
        }
    }

    private static void ValidateV2(MaskPackManifest manifest, List<string> errors)
    {
        if (manifest.ArtDisplay is null
            || manifest.ArtDisplay.Width != ArtWidth || manifest.ArtDisplay.Height != RequiredHeight)
        {
            errors.Add($"artDisplay must be {ArtWidth}x{RequiredHeight} for physical DIY art.");
        }

        if (manifest.TextDisplay is null
            || manifest.TextDisplay.Width != TextWidth || manifest.TextDisplay.Height != RequiredHeight)
        {
            errors.Add($"textDisplay must be {TextWidth}x{RequiredHeight} for the verified text region.");
        }

        var contents = manifest.Contents ?? [];
        if (contents.Length is < 1 or > MaxContentEntries)
        {
            errors.Add($"contents must contain between 1 and {MaxContentEntries} entries.");
            return;
        }

        var keys = new HashSet<string>(StringComparer.Ordinal);
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var content in contents)
        {
            if (content is null)
            {
                errors.Add("Content entries cannot be null.");
                continue;
            }

            var label = string.IsNullOrWhiteSpace(content.Id) ? "<missing content id>" : content.Id;
            if (string.IsNullOrWhiteSpace(content.Id) || content.Id.Length > 128)
            {
                errors.Add("Every content id is required and must be at most 128 characters.");
            }

            if (string.IsNullOrWhiteSpace(content.Name) || content.Name.Length > 160)
            {
                errors.Add($"Content {label} name is required and must be at most 160 characters.");
            }

            if (string.IsNullOrWhiteSpace(content.Path) || content.Path.Length > 240)
            {
                errors.Add($"Content {label} path is required and must be at most 240 characters.");
            }

            if (content.Sha256 is null || content.Sha256.Length != 64 || !content.Sha256.All(Uri.IsHexDigit))
            {
                errors.Add($"Content {label} sha256 must be a 64-character hexadecimal digest.");
            }

            if (content.FormatVersion != 1)
            {
                errors.Add($"Content {label} formatVersion {content.FormatVersion} is unsupported.");
            }

            if (!Enum.IsDefined(content.Type))
            {
                errors.Add($"Content {label} type {content.Type} is unsupported.");
            }

            if (!keys.Add($"{content.Type}:{content.Id}"))
            {
                errors.Add($"Duplicate content key {content.Type}:{content.Id}.");
            }

            if (!string.IsNullOrWhiteSpace(content.Path) && !paths.Add(content.Path))
            {
                errors.Add($"Duplicate content path {content.Path}.");
            }
        }
    }

    private static void ValidateAsset(MaskPackAsset asset, List<string> errors, List<string> warnings)
    {
        var label = string.IsNullOrWhiteSpace(asset.Id) ? "<missing asset id>" : asset.Id;

        if (string.IsNullOrWhiteSpace(asset.Id) || asset.Id.Length > 128)
        {
            errors.Add("Asset id is required and must be at most 128 characters.");
        }

        if (string.IsNullOrWhiteSpace(asset.Name) || asset.Name.Length > 160)
        {
            errors.Add($"Asset {label} name is required and must be at most 160 characters.");
        }

        if (!Enum.IsDefined(asset.Type))
        {
            errors.Add($"Asset {label} type {asset.Type} is unsupported.");
        }

        if ((asset.Tags?.Length ?? 0) > 32
            || (asset.Tags ?? []).Any(tag => tag is null || tag.Length > 80))
        {
            errors.Add($"Asset {label} tags exceed the safe count or length limit.");
        }

        if ((asset.Notes?.Length ?? 0) > 2000)
        {
            errors.Add($"Asset {label} notes exceed the 2000-character limit.");
        }

        var frames = asset.Frames ?? [];
        if (frames.Length == 0)
        {
            errors.Add($"Asset {label} must include at least one frame.");
            return;
        }

        if (asset.FrameDurationMs <= 0)
        {
            errors.Add($"Asset {label} frameDurationMs must be positive.");
        }

        if (frames.Length > ReasonableFrameCount)
        {
            errors.Add($"Asset {label} has too many frames for the safe metadata format.");
        }
        else if (frames.Length > HighFrameCountWarningThreshold)
        {
            warnings.Add($"Asset {label} has a high frame count; stock-firmware playback is unverified.");
        }

        if (asset.Type == MaskPackAssetType.StaticImage && frames.Length > 1)
        {
            warnings.Add($"Asset {label} is a static image with multiple frames; only the first frame is expected to matter.");
        }

        for (var i = 0; i < frames.Length; i++)
        {
            var frame = frames[i];
            if (frame is null)
            {
                errors.Add($"Asset {label} frame {i} cannot be null.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(frame.Path) || frame.Path.Length > 240)
            {
                errors.Add($"Asset {label} frame {i} path is required and must be at most 240 characters.");
            }

            if (frame.DurationMs is <= 0)
            {
                errors.Add($"Asset {label} frame {i} durationMs must be positive when provided.");
            }
        }
    }
}
