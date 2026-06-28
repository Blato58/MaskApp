using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Tests.Features.Connect;

public sealed class AsyncRelayCommandTests
{
    [Fact]
    public async Task ExecuteAsync_CatchesExceptionAndResetsCanExecute()
    {
        var exceptionHandled = false;
        var command = new AsyncRelayCommand(
            _ => throw new InvalidOperationException("boom"),
            onError: _ => exceptionHandled = true);

        await command.ExecuteAsync();

        Assert.True(exceptionHandled);
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public async Task ExecuteAsync_DisablesCommandWhileRunning()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new AsyncRelayCommand(_ => gate.Task);

        var runTask = command.ExecuteAsync();

        Assert.False(command.CanExecute(null));
        gate.SetResult();
        await runTask;
        Assert.True(command.CanExecute(null));
    }
}
