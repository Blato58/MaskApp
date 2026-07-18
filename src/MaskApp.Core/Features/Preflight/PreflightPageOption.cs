namespace MaskApp.Core.Features.Preflight;

public sealed record PreflightPageOption(string PageId, string Title)
{
    public string DisplayName => $"{Title} Page";
}
