using System.Windows.Input;
using System.Diagnostics;

namespace MaskApp.Core.Features.Connect;

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<CancellationToken, Task> execute;
    private readonly Func<bool>? canExecute;
    private readonly Action<Exception>? onError;
    private bool isRunning;

    public AsyncRelayCommand(
        Func<CancellationToken, Task> execute,
        Func<bool>? canExecute = null,
        Action<Exception>? onError = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
        this.onError = onError;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !isRunning && (canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        await ExecuteInternalAsync(parameter, CancellationToken.None, respectCanExecute: true);
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default) =>
        ExecuteInternalAsync(null, cancellationToken, respectCanExecute: false);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private async Task ExecuteInternalAsync(object? parameter, CancellationToken cancellationToken, bool respectCanExecute)
    {
        if (respectCanExecute && !CanExecute(parameter))
        {
            return;
        }

        isRunning = true;
        RaiseCanExecuteChanged();

        try
        {
            await execute(cancellationToken);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            Debug.WriteLine($"Async command failed: {ex}");
        }
        finally
        {
            isRunning = false;
            RaiseCanExecuteChanged();
        }
    }
}
