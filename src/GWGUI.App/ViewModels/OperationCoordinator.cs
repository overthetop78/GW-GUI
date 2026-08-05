namespace GWGUI.App.ViewModels;

public sealed record OperationOutcome<T>(bool HasResult, T? Result, Exception? Error);

public sealed class OperationCoordinator
{
    private readonly object _gate = new();
    private CancellationTokenSource? _cancellation;

    public bool IsRunning
    {
        get { lock (_gate) return _cancellation is not null; }
    }

    public void RequestCancellation()
    {
        lock (_gate) _cancellation?.Cancel();
    }

    public async Task<OperationOutcome<T>> RunAsync<T>(Func<CancellationToken, Task<T>> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        CancellationTokenSource cancellation;
        lock (_gate)
        {
            if (_cancellation is not null) return new(false, default, new InvalidOperationException("An operation is already running."));
            _cancellation = cancellation = new CancellationTokenSource();
        }

        try { return new(true, await operation(cancellation.Token).ConfigureAwait(false), null); }
        catch (Exception exception) { return new(false, default, exception); }
        finally
        {
            lock (_gate)
            {
                if (ReferenceEquals(_cancellation, cancellation)) _cancellation = null;
            }
            cancellation.Dispose();
        }
    }
}
