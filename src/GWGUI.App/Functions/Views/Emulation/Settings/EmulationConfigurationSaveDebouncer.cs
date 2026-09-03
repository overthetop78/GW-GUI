namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal sealed class EmulationConfigurationSaveDebouncer : IDisposable
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(160);
    private readonly object _gate = new();
    private CancellationTokenSource? _pending;

    internal void Schedule(Func<Task> save, Action<Exception> failed)
    {
        CancellationTokenSource current;
        lock (_gate)
        {
            _pending?.Cancel();
            _pending?.Dispose();
            current = new CancellationTokenSource();
            _pending = current;
        }
        _ = RunAsync(current, save, failed);
    }

    private static async Task RunAsync(CancellationTokenSource current,
        Func<Task> save, Action<Exception> failed)
    {
        try
        {
            await Task.Delay(Delay, current.Token);
            await save();
        }
        catch (OperationCanceledException) when (current.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            failed(error);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _pending?.Cancel();
            _pending?.Dispose();
            _pending = null;
        }
    }
}
