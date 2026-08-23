using GWGUI.App;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Threading;

namespace GWGUI.Tests;

internal static class WpfTestHost
{
    private const int StartupTimeoutMilliseconds = 30000;
    private static readonly Dispatcher UiDispatcher;

    static WpfTestHost()
    {
        using var ready = new ManualResetEventSlim();
        Exception? startupFailure = null;
        Dispatcher? dispatcher = null;
        var thread = new Thread(() =>
        {
            try
            {
                var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
                app.InitializeComponent();
                dispatcher = Dispatcher.CurrentDispatcher;
            }
            catch (Exception error)
            {
                startupFailure = error;
            }
            finally
            {
                ready.Set();
            }

            if (startupFailure is null) Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = nameof(WpfTestHost)
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!ready.Wait(StartupTimeoutMilliseconds))
            throw new TimeoutException("The WPF test host did not start in time.");
        if (startupFailure is not null)
            ExceptionDispatchInfo.Capture(startupFailure).Throw();
        UiDispatcher = dispatcher ?? throw new InvalidOperationException("The WPF test dispatcher is unavailable.");
    }

    internal static void Run(Action action) => UiDispatcher.Invoke(action);

    internal static void RunAsync(Func<Task> action) =>
        UiDispatcher.InvokeAsync(action).Task.Unwrap().GetAwaiter().GetResult();

}
