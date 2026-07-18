namespace MaskApp.Core.Features.Preflight;

public sealed record FestivalShowPreparationResult(
    bool Succeeded,
    string Message,
    int UploadedSlotCount,
    int ReusedSlotCount)
{
    public static FestivalShowPreparationResult Success(
        string message,
        int uploadedSlotCount,
        int reusedSlotCount) =>
        new(true, message, uploadedSlotCount, reusedSlotCount);

    public static FestivalShowPreparationResult Failure(
        string message,
        int uploadedSlotCount,
        int reusedSlotCount) =>
        new(false, message, uploadedSlotCount, reusedSlotCount);
}
