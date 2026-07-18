using MaskApp.Core.Features.Animations;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonFlashSafetyAcknowledgementStore : JsonFlashSafetyAcknowledgementStoreCore
{
    public JsonFlashSafetyAcknowledgementStore()
        : base(Path.Combine(FileSystem.AppDataDirectory, "flash-safety-acknowledgements.json"))
    {
    }

    public JsonFlashSafetyAcknowledgementStore(string filePath)
        : base(filePath)
    {
    }
}
