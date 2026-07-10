namespace MaskApp.Core.Features.Faces;

internal static class FaceUploadOperationLock
{
    internal static SemaphoreSlim Gate { get; } = new(1, 1);
}
