namespace MaskApp.Core.Features.AnimationPacks;

public sealed record MaskPackValidationResult(
    MaskPackManifest? Manifest,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public bool IsValid => Errors.Count == 0;

    public static MaskPackValidationResult Failed(params string[] errors) =>
        new(null, errors, []);
}
