using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace GWGUI.App.Services.Input.GameInput;

internal sealed class GameInputWorker
{
    internal const string ThreadName = "GWGUI GameInput";

    private readonly BlockingCollection<Action> _queue = new();
    private readonly int _threadId;
    private readonly Thread _thread;

    internal GameInputWorker()
    {
        using var ready = new ManualResetEventSlim();
        var threadId = 0;
        _thread = new Thread(() =>
        {
            threadId = Environment.CurrentManagedThreadId;
            ready.Set();
            foreach (var action in _queue.GetConsumingEnumerable()) action();
        })
        {
            IsBackground = true,
            Name = ThreadName
        };
        _thread.SetApartmentState(ApartmentState.MTA);
        _thread.Start();
        ready.Wait();
        _threadId = threadId;
    }

    internal void Post(Action action) => _queue.Add(action);

    internal void Invoke(Action action) => Invoke(() =>
    {
        action();
        return true;
    });

    internal bool TryInvoke(Action action, TimeSpan timeout)
    {
        if (Environment.CurrentManagedThreadId == _threadId)
        {
            action();
            return true;
        }
        var completed = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _queue.Add(() =>
        {
            try
            {
                action();
                completed.TrySetResult();
            }
            catch (Exception exception)
            {
                completed.TrySetException(exception);
            }
        });
        if (!completed.Task.Wait(timeout)) return false;
        completed.Task.GetAwaiter().GetResult();
        return true;
    }

    internal bool Stop(Action action, TimeSpan timeout)
    {
        if (_queue.IsAddingCompleted)
            return !_thread.IsAlive || _thread.Join(timeout);

        if (Environment.CurrentManagedThreadId == _threadId)
        {
            try { action(); }
            finally { _queue.CompleteAdding(); }
            return true;
        }

        var completed = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _queue.Add(() =>
        {
            try
            {
                action();
                completed.TrySetResult();
            }
            catch (Exception exception)
            {
                completed.TrySetException(exception);
            }
            finally
            {
                _queue.CompleteAdding();
            }
        });

        Exception? failure = null;
        var actionCompleted = false;
        try
        {
            actionCompleted = completed.Task.Wait(timeout);
            if (actionCompleted) completed.Task.GetAwaiter().GetResult();
        }
        catch (Exception error)
        {
            failure = error;
        }

        var threadStopped = _thread.Join(timeout);
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
        return actionCompleted && threadStopped;
    }

    internal T Invoke<T>(Func<T> action)
    {
        if (Environment.CurrentManagedThreadId == _threadId) return action();
        using var completed = new ManualResetEventSlim();
        T? result = default;
        Exception? failure = null;
        _queue.Add(() =>
        {
            try { result = action(); }
            catch (Exception exception) { failure = exception; }
            finally { completed.Set(); }
        });
        completed.Wait();
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
        return result!;
    }
}
