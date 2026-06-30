using MaskApp.Core.Features.Faces;

namespace MaskApp.App.Infrastructure.Media;

public interface IFaceImageDecoder
{
    Task<FaceSampleImage?> DecodeAsync(Stream stream, CancellationToken cancellationToken = default);
}
