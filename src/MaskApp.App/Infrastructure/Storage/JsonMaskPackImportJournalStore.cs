using MaskApp.Core.Features.AnimationPacks;

namespace MaskApp.App.Infrastructure.Storage;

public sealed class JsonMaskPackImportJournalStore : JsonMaskPackImportJournalStoreCore
{
    public JsonMaskPackImportJournalStore()
        : base(Path.Combine(FileSystem.AppDataDirectory, "maskpack-import-journal.json"))
    {
    }

    public JsonMaskPackImportJournalStore(string filePath)
        : base(filePath)
    {
    }
}
