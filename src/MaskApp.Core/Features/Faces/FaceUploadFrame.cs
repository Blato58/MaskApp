namespace MaskApp.Core.Features.Faces;

public sealed record FaceUploadFrame(int Index, ReadOnlyMemory<byte> Data);
