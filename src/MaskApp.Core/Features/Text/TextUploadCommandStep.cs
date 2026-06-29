using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Text;

public sealed record TextUploadCommandStep(
    MaskCommand Command,
    bool FailSoft);
