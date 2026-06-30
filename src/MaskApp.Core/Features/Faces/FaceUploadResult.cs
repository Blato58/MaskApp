namespace MaskApp.Core.Features.Faces;

public sealed record FaceUploadResult(bool Succeeded, string Message, int FramesSent)
{
    public static FaceUploadResult Success(string message, int framesSent) => new(true, message, framesSent);

    public static FaceUploadResult Failure(string message, int framesSent) => new(false, message, framesSent);
}
