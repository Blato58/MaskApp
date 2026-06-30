using MaskApp.Core.Features.TextPresets;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonTextPresetStore : JsonTextPresetStoreCore
{
    public JsonTextPresetStore()
        : base(Path.Combine(FileSystem.AppDataDirectory, "text-presets.json"))
    {
    }

    public JsonTextPresetStore(string filePath)
        : base(filePath)
    {
    }
}
