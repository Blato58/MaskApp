using MaskApp.Core.Features.Faces;

namespace MaskApp.App.Infrastructure.Media;

public sealed class UnavailableFaceImageDecoder : IFaceImageDecoder
{
    public Task<FaceSampleImage?> DecodeAsync(Stream stream, CancellationToken cancellationToken = default) =>
        Task.FromResult<FaceSampleImage?>(null);
}
