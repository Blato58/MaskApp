using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Animations;

public sealed record AppBuiltInAnimationFrame
{
    public int Slot { get; init; }

    public FacePattern Pattern { get; init; } = new();

    public AppBuiltInAnimationFrame Normalize()
    {
        if (Slot is < FacePattern.MinSlot or > FacePattern.MaxSlot)
        {
            throw new ArgumentOutOfRangeException(nameof(Slot), Slot, "Animation frame slot must be between 1 and 20.");
        }

        return this with
        {
            Pattern = Pattern.Normalize() with { PreferredSlot = Slot }
        };
    }
}
