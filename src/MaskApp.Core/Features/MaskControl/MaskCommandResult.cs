namespace MaskApp.Core.Features.MaskControl;

public sealed record MaskCommandResult(bool Succeeded, string Message)
{
    public static MaskCommandResult Success(string message) => new(true, message);

    public static MaskCommandResult Failure(string message) => new(false, message);
}
