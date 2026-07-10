using MaskApp.Core.Features.Faces;

namespace MaskApp.Core.Features.Gallery;

public sealed record PageFastSlotSnapshot(
    FacePattern Pattern,
    string ContentFingerprint,
    string ContentDescription);
