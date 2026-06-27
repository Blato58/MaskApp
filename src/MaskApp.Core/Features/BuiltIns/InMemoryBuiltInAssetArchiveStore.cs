namespace MaskApp.Core.Features.BuiltIns;

public sealed class InMemoryBuiltInAssetArchiveStore : IBuiltInAssetArchiveStore
{
    private BuiltInAssetArchive archive;

    public InMemoryBuiltInAssetArchiveStore()
        : this(BuiltInAssetArchive.Empty)
    {
    }

    public InMemoryBuiltInAssetArchiveStore(BuiltInAssetArchive archive)
    {
        this.archive = archive;
    }

    public Task<BuiltInAssetArchive> LoadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(archive);

    public Task SaveAsync(BuiltInAssetArchive archive, CancellationToken cancellationToken = default)
    {
        this.archive = archive;
        return Task.CompletedTask;
    }
}
