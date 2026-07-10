using System.Security.Cryptography;

namespace MaskApp.Core.Features.Faces;

public static class FaceContentFingerprint
{
    public static string Compute(FacePattern pattern) =>
        Convert.ToHexString(SHA256.HashData(FaceUploadProtocol.BuildPayload(pattern)));
}
