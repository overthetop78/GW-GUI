using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace GWGUI.MediaAudit.TestInfrastructure;

internal static class WpfResourceCleanup
{
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(10);

    public static void Release()
    {
        var application = Application.Current;
        if (application is not null)
            Release(application);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static void Release(Application application)
    {
        var dispatcher = application.Dispatcher;
        using var shutdownFinished = new ManualResetEventSlim(dispatcher.HasShutdownFinished);
        EventHandler onShutdownFinished = (_, _) => shutdownFinished.Set();
        dispatcher.ShutdownFinished += onShutdownFinished;

        try
        {
            if (!dispatcher.HasShutdownFinished)
            {
                if (dispatcher.CheckAccess())
                    CloseAndShutdown(application, dispatcher);
                else
                {
                    var operation = dispatcher.InvokeAsync(
                        () => CloseAndShutdown(application, dispatcher),
                        DispatcherPriority.Send);
                    if (!operation.Task.Wait(ShutdownTimeout))
                        throw new TimeoutException("The WPF cleanup operation did not complete.");
                    operation.Task.GetAwaiter().GetResult();
                }
            }

            if (!dispatcher.HasShutdownFinished && !dispatcher.CheckAccess()
                && !shutdownFinished.Wait(ShutdownTimeout))
                throw new TimeoutException("The WPF dispatcher did not finish its shutdown.");
        }
        finally
        {
            dispatcher.ShutdownFinished -= onShutdownFinished;
        }
    }

    private static void CloseAndShutdown(Application application, Dispatcher dispatcher)
    {
        foreach (Window window in application.Windows.Cast<Window>().ToArray())
        {
            window.Owner = null;
            if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
                window.Close();
            window.DataContext = null;
            window.Content = null;
        }

        application.Shutdown();
        if (!dispatcher.HasShutdownStarted)
            dispatcher.InvokeShutdown();
    }
}
