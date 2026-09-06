using System.Windows.Threading;

namespace GWGUI.Tests.Application.TestInfrastructure;

// One dispatcher for all WPF scenarios; it never starts the GW GUI application.
public sealed class StaExecutionScenarios : IDisposable
{
    private readonly Thread thread;
    private readonly Dispatcher dispatcher;

    public StaExecutionScenarios()
    {
        var ready = new TaskCompletionSource<Dispatcher>(TaskCreationOptions.RunContinuationsAsynchronously);
        thread = new Thread(() =>
        {
            try
            {
                var current = Dispatcher.CurrentDispatcher;
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(current));
                var resources = new ResourceApplication { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };
                resources.Resources = new System.Windows.ResourceDictionary
                {
                    Source = new Uri("/gwgui.app;component/Resources/ApplicationStyles.xaml", UriKind.Relative)
                };
                ready.SetResult(current);
                Dispatcher.Run();
            }
            catch (Exception exception) { ready.TrySetException(exception); }
        }) { IsBackground = true, Name = "GWGUI test dispatcher" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        dispatcher = ready.Task.GetAwaiter().GetResult();
    }

    public Task Run(Action scenario) => dispatcher.InvokeAsync(() =>
    {
        scenario();
        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            Assert.Equal(IntPtr.Zero, new System.Windows.Interop.WindowInteropHelper(window).Handle);
    }).Task.WaitAsync(TimeSpan.FromSeconds(30));

    public Task RunAsync(Func<Task> scenario) => dispatcher.InvokeAsync(async () =>
    {
        await scenario();
        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            Assert.Equal(IntPtr.Zero, new System.Windows.Interop.WindowInteropHelper(window).Handle);
    }).Task.Unwrap().WaitAsync(TimeSpan.FromSeconds(30));

    public void Dispose()
    {
        dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
        if (!thread.Join(TimeSpan.FromSeconds(5))) throw new TimeoutException("WPF test dispatcher did not stop.");
    }

    private sealed class ResourceApplication : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e) { }
    }
}

[CollectionDefinition("WPF", DisableParallelization = true)]
public sealed class WpfCollection : ICollectionFixture<StaExecutionScenarios>;
