namespace MaskApp.Core.Features.Faces;

public sealed record DiySlotPlaybackResult(
    bool Succeeded,
    string Message,
    int UploadedSlotCount,
    int ReusedSlotCount,
    bool PlayCommandSent)
{
    public static DiySlotPlaybackResult Failure(
        string message,
        int uploadedSlotCount = 0,
        int reusedSlotCount = 0) =>
        new(false, message, uploadedSlotCount, reusedSlotCount, false);

    public static DiySlotPlaybackResult Success(
        string message,
        int uploadedSlotCount,
        int reusedSlotCount,
        bool playCommandSent) =>
        new(true, message, uploadedSlotCount, reusedSlotCount, playCommandSent);
}
