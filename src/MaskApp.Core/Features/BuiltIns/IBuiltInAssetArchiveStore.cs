namespace MaskApp.Core.Features.BuiltIns;

public interface IBuiltInAssetArchiveStore
{
    Task<BuiltInAssetArchive> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(BuiltInAssetArchive archive, CancellationToken cancellationToken = default);
}
