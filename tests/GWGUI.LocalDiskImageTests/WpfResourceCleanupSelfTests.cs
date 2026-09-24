using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using GWGUI.MediaAudit.TestInfrastructure;

namespace GWGUI.MediaAudit;

internal static class WpfResourceCleanupSelfTests
{
    public static void Run()
    {
        Dispatcher? dispatcher = null;
        Exception? failure = null;
        IntPtr createdHandle = IntPtr.Zero;
        IntPtr releasedHandle = IntPtr.Zero;
        using var initialized = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            TestApplication? application = null;
            Window? window = null;
            try
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                initialized.Set();
                application = new TestApplication { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                window = new Window
                {
                    Width = 1,
                    Height = 1,
                    ShowActivated = false,
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.ToolWindow
                };
                window.Show();
                createdHandle = new WindowInteropHelper(window).Handle;
            }
            catch (Exception error)
            {
                failure = error;
            }
            finally
            {
                try
                {
                    if (application is not null)
                        WpfResourceCleanup.Release(application);
                    else if (dispatcher is not null && !dispatcher.HasShutdownStarted)
                        dispatcher.InvokeShutdown();
                }
                catch (Exception cleanupError)
                {
                    failure = failure is null
                        ? cleanupError
                        : new AggregateException(failure, cleanupError);
                }

                if (window is not null)
                    releasedHandle = new WindowInteropHelper(window).Handle;
            }
        }) { IsBackground = true, Name = "WPF resource cleanup self-test" };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!initialized.Wait(TimeSpan.FromSeconds(10)))
            throw new TimeoutException("The WPF cleanup self-test did not initialize its dispatcher.");
        WpfResourceCleanup.WaitForShutdown(thread, dispatcher);

        if (failure is not null)
            throw new InvalidOperationException("The WPF cleanup self-test failed.", failure);
        if (createdHandle == IntPtr.Zero)
            throw new InvalidOperationException("The WPF cleanup self-test did not create a native window handle.");
        if (releasedHandle != IntPtr.Zero)
            throw new InvalidOperationException("The WPF cleanup self-test left a native window handle alive.");
    }

    private sealed class TestApplication : Application
    {
        protected override void OnStartup(StartupEventArgs e) { }
    }
}
