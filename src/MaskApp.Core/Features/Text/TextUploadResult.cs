namespace MaskApp.Core.Features.Text;

public sealed record TextUploadResult(bool Succeeded, string Message, int FramesSent)
{
    public static TextUploadResult Success(string message, int framesSent) => new(true, message, framesSent);

    public static TextUploadResult Failure(string message, int framesSent) => new(false, message, framesSent);
}
