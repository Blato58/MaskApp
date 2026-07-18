namespace MaskApp.Core.Features.Profiles;

public sealed class MaskProfileChangedEventArgs : EventArgs
{
    public MaskProfileChangedEventArgs(MaskProfile profile)
    {
        Profile = profile;
    }

    public MaskProfile Profile { get; }
}
