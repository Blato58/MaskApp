using MaskApp.Core.Features.Profiles;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonMaskProfileStore : JsonMaskProfileStoreCore
{
    public JsonMaskProfileStore()
        : base(Path.Combine(FileSystem.AppDataDirectory, "mask-profiles.json"))
    {
    }

    public JsonMaskProfileStore(string filePath)
        : base(filePath)
    {
    }
}
