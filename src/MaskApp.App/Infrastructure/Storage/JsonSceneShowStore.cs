using MaskApp.Core.Features.Scenes;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonSceneShowStore : JsonSceneShowStoreCore
{
    public JsonSceneShowStore()
        : base(Path.Combine(FileSystem.AppDataDirectory, "scenes-and-setlists.json"))
    {
    }

    public JsonSceneShowStore(string filePath)
        : base(filePath)
    {
    }
}
