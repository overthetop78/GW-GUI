using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace GWGUI.MediaAudit.TestInfrastructure;

internal static class WpfResourceCleanup
{
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(10);

    public static void Release()
    {
        Exception? failure = null;
        try
        {
            if (Application.Current is { } application)
                Release(application);
        }
        catch (Exception error)
        {
            failure = error;
        }
        finally
        {
            Collect();
        }

        Rethrow(failure);
    }

    public static void Release(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        var dispatcher = application.Dispatcher;
        if (dispatcher.HasShutdownFinished)
            return;

        if (dispatcher.CheckAccess())
        {
            CloseAndShutdown(application, dispatcher);
            return;
        }

        using var shutdownFinished = new ManualResetEventSlim(dispatcher.HasShutdownFinished);
        EventHandler onShutdownFinished = (_, _) => shutdownFinished.Set();
        dispatcher.ShutdownFinished += onShutdownFinished;
        try
        {
            var operation = dispatcher.InvokeAsync(
                () => CloseAndShutdown(application, dispatcher),
                DispatcherPriority.Send);
            if (!operation.Task.Wait(ShutdownTimeout))
                throw new TimeoutException("The WPF cleanup operation did not complete.");
            operation.Task.GetAwaiter().GetResult();

            if (!dispatcher.HasShutdownFinished && !shutdownFinished.Wait(ShutdownTimeout))
                throw new TimeoutException("The WPF dispatcher did not finish its shutdown.");
        }
        finally
        {
            dispatcher.ShutdownFinished -= onShutdownFinished;
        }
    }

    public static void WaitForShutdown(Thread thread, Dispatcher? dispatcher)
    {
        ArgumentNullException.ThrowIfNull(thread);
        if (!thread.Join(ShutdownTimeout))
            throw new TimeoutException($"The WPF thread '{thread.Name}' did not terminate.");
        if (thread.IsAlive)
            throw new InvalidOperationException($"The WPF thread '{thread.Name}' is still alive.");
        if (dispatcher is not null && !dispatcher.HasShutdownFinished)
            throw new InvalidOperationException($"The WPF dispatcher for '{thread.Name}' did not finish its shutdown.");

        Collect();
    }

    private static void CloseAndShutdown(Application application, Dispatcher dispatcher)
    {
        dispatcher.VerifyAccess();
        List<Exception>? failures = null;

        foreach (Window window in application.Windows.Cast<Window>().ToArray())
            TryCleanup(() => CloseAndDetach(window), ref failures);

        TryCleanup(() => application.MainWindow = null, ref failures);
        TryCleanup(() => ClearResources(application.Resources), ref failures);
        TryCleanup(application.Shutdown, ref failures);
        if (!dispatcher.HasShutdownStarted)
            TryCleanup(dispatcher.InvokeShutdown, ref failures);

        if (failures is { Count: > 0 })
            throw new AggregateException("One or more WPF resources could not be released.", failures);
    }

    private static void CloseAndDetach(Window window)
    {
        List<Exception>? failures = null;
        TryCleanup(() => window.Owner = null, ref failures);
        try
        {
            if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
                TryCleanup(window.Close, ref failures);
        }
        finally
        {
            TryCleanup(() => window.DataContext = null, ref failures);
            TryCleanup(() => window.Content = null, ref failures);
            TryCleanup(() => ClearResources(window.Resources), ref failures);
        }

        if (failures is { Count: > 0 })
            throw new AggregateException($"The WPF window '{window.GetType().FullName}' could not be fully released.", failures);
    }

    private static void ClearResources(ResourceDictionary resources)
    {
        resources.MergedDictionaries.Clear();
        resources.Clear();
    }

    private static void TryCleanup(Action action, ref List<Exception>? failures)
    {
        try
        {
            action();
        }
        catch (Exception error)
        {
            (failures ??= []).Add(error);
        }
    }

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static void Rethrow(Exception? failure)
    {
        if (failure is not null)
            ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
