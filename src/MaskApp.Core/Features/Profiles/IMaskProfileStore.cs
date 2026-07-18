namespace MaskApp.Core.Features.Profiles;

public interface IMaskProfileStore
{
    Task<MaskProfileStoreState> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(MaskProfileStoreState state, CancellationToken cancellationToken = default);
}
