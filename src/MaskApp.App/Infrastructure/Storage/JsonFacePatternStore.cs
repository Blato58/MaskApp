using MaskApp.Core.Features.Faces;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonFacePatternStore : JsonFacePatternStoreCore
{
    public JsonFacePatternStore()
        : base(Path.Combine(FileSystem.AppDataDirectory, "faces.json"))
    {
    }

    public JsonFacePatternStore(string filePath)
        : base(filePath)
    {
    }
}
