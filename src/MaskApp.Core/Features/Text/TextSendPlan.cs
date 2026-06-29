namespace MaskApp.Core.Features.Text;

public sealed record TextSendPlan(
    TextSendProfile Profile,
    TextUploadPackage Package,
    TextUploadOptions Options,
    TextSendLayoutMetadata Layout,
    IReadOnlyList<string> Warnings,
    string Summary);
