using MaskApp.Core.Features.Animations;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonAnimationProjectStore : JsonAnimationProjectStoreCore
{
    public JsonAnimationProjectStore()
        : base(Path.Combine(FileSystem.AppDataDirectory, "animation-projects.json"))
    {
    }

    public JsonAnimationProjectStore(string filePath)
        : base(filePath)
    {
    }
}
