using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Xunit.Sdk;

[assembly: GWGUI.Tests.Application.TestInfrastructure.TestResourceCleanupAttribute]

namespace GWGUI.Tests.Application.TestInfrastructure;

public sealed class TestResourceCleanupAttribute : BeforeAfterTestAttribute
{
    public override void After(MethodInfo methodUnderTest) => TestResourceCleanup.AfterTest();
}

internal static class TestResourceCleanup
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
    private static System.Windows.Application? ownedApplication;

    internal static void Register(System.Windows.Application application) => ownedApplication = application;

    internal static void AfterTest()
    {
        try
        {
            var application = ownedApplication;
            if (application is not null) OnDispatcher(application.Dispatcher, () => ReleaseWindows(application));
        }
        finally { Collect(); }
    }

    internal static void ReleaseWindows(System.Windows.Application application)
    {
        application.Dispatcher.VerifyAccess();
        Exception? failure = null;
        foreach (Window window in application.Windows.Cast<Window>().ToArray())
        {
            Run(() => window.Owner = null, ref failure);
            try
            {
                if (new WindowInteropHelper(window).Handle != IntPtr.Zero) Run(window.Close, ref failure);
            }
            finally
            {
                Run(() => window.DataContext = null, ref failure);
                Run(() => window.Content = null, ref failure);
                Run(() => window.Resources.MergedDictionaries.Clear(), ref failure);
                Run(() => window.Resources.Clear(), ref failure);
                Run(() =>
                {
                    if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
                        throw new InvalidOperationException("A test window remains alive after cleanup.");
                }, ref failure);
            }
        }
        Rethrow(failure);
    }

    internal static void ReleaseFixture(System.Windows.Application application, Thread thread, Dispatcher dispatcher)
    {
        Exception? failure = null;
        try
        {
            OnDispatcher(dispatcher, () =>
            {
                Exception? shutdownFailure = null;
                try
                {
                    Run(() => ReleaseWindows(application), ref shutdownFailure);
                    Run(() => application.MainWindow = null, ref shutdownFailure);
                    Run(() => application.Resources.MergedDictionaries.Clear(), ref shutdownFailure);
                    Run(() => application.Resources.Clear(), ref shutdownFailure);
                }
                finally
                {
                    Run(application.Shutdown, ref shutdownFailure);
                    if (!dispatcher.HasShutdownStarted) Run(dispatcher.InvokeShutdown, ref shutdownFailure);
                }
                Rethrow(shutdownFailure);
            });
        }
        catch (Exception error) { failure = error; }
        finally
        {
            try
            {
                if (!dispatcher.HasShutdownStarted && !dispatcher.HasShutdownFinished) dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                if (!thread.Join(Timeout)) throw new TimeoutException("The test dispatcher thread did not terminate.");
                if (thread.IsAlive || !dispatcher.HasShutdownFinished) throw new InvalidOperationException("The test dispatcher remains alive.");
            }
            catch (Exception error) { failure ??= error; }
            finally
            {
                if (ReferenceEquals(ownedApplication, application)) ownedApplication = null;
                Collect();
            }
        }
        Rethrow(failure);
    }

    private static void OnDispatcher(Dispatcher dispatcher, Action action)
    {
        if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished) return;
        if (dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action, DispatcherPriority.ApplicationIdle, CancellationToken.None, Timeout);
    }

    private static void Run(Action action, ref Exception? failure)
    {
        try { action(); }
        catch (Exception error) { failure ??= error; }
    }

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static void Rethrow(Exception? failure)
    {
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
