using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Faces;

public sealed record FaceUploadPackage(
    FacePattern Pattern,
    int Slot,
    byte[] Payload,
    IReadOnlyList<FaceUploadFrame> Frames,
    MaskCommand StartCommand,
    MaskCommand FinishCommand,
    MaskCommand PlayCommand);
