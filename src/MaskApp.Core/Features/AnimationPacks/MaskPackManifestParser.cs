using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskApp.Core.Features.AnimationPacks;

public static class MaskPackManifestParser
{
    public const int RequiredWidth = 44;
    public const int RequiredHeight = 58;
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

        if (manifest.SchemaVersion <= 0)
        {
            errors.Add("schemaVersion must be positive.");
        }

        if (string.IsNullOrWhiteSpace(manifest.PackName))
        {
            errors.Add("packName is required.");
        }

        if (manifest.TargetDisplay.Width != RequiredWidth)
        {
            errors.Add($"targetDisplay.width must be {RequiredWidth}.");
        }

        if (manifest.TargetDisplay.Height != RequiredHeight)
        {
            errors.Add($"targetDisplay.height must be {RequiredHeight}.");
        }

        if (manifest.Assets.Length == 0)
        {
            errors.Add("At least one asset is required.");
        }

        foreach (var asset in manifest.Assets)
        {
            ValidateAsset(asset, errors, warnings);
        }

        return new MaskPackValidationResult(manifest, errors, warnings);
    }

    private static void ValidateAsset(MaskPackAsset asset, List<string> errors, List<string> warnings)
    {
        var label = string.IsNullOrWhiteSpace(asset.Id) ? "<missing asset id>" : asset.Id;

        if (string.IsNullOrWhiteSpace(asset.Id))
        {
            errors.Add("Asset id is required.");
        }

        if (string.IsNullOrWhiteSpace(asset.Name))
        {
            errors.Add($"Asset {label} name is required.");
        }

        if (asset.Frames.Length == 0)
        {
            errors.Add($"Asset {label} must include at least one frame.");
            return;
        }

        if (asset.FrameDurationMs <= 0)
        {
            errors.Add($"Asset {label} frameDurationMs must be positive.");
        }

        if (asset.Frames.Length > ReasonableFrameCount)
        {
            errors.Add($"Asset {label} has too many frames for the safe metadata format.");
        }
        else if (asset.Frames.Length > HighFrameCountWarningThreshold)
        {
            warnings.Add($"Asset {label} has a high frame count; stock-firmware playback is unverified.");
        }

        if (asset.Type == MaskPackAssetType.StaticImage && asset.Frames.Length > 1)
        {
            warnings.Add($"Asset {label} is a static image with multiple frames; only the first frame is expected to matter.");
        }

        for (var i = 0; i < asset.Frames.Length; i++)
        {
            var frame = asset.Frames[i];
            if (string.IsNullOrWhiteSpace(frame.Path))
            {
                errors.Add($"Asset {label} frame {i} path is required.");
            }

            if (frame.DurationMs is <= 0)
            {
                errors.Add($"Asset {label} frame {i} durationMs must be positive when provided.");
            }
        }
    }
}
