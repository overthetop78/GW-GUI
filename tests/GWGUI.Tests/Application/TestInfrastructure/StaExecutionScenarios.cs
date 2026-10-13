using System.Windows.Threading;

namespace GWGUI.Tests.Application.TestInfrastructure;

// One dispatcher for all WPF scenarios; it never starts the GW GUI application.
public sealed class StaExecutionScenarios : IDisposable
{
    private readonly Thread thread;
    private readonly Dispatcher dispatcher;
    private readonly ResourceApplication application;

    public StaExecutionScenarios()
    {
        var ready = new TaskCompletionSource<(Dispatcher Dispatcher, ResourceApplication Application)>(TaskCreationOptions.RunContinuationsAsynchronously);
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
                ready.SetResult((current, resources));
                Dispatcher.Run();
            }
            catch (Exception exception) { ready.TrySetException(exception); }
        }) { IsBackground = true, Name = "GWGUI test dispatcher" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        (dispatcher, application) = ready.Task.GetAwaiter().GetResult();
        TestResourceCleanup.Register(application);
    }

    public Task Run(Action scenario) => RunAsync(() =>
    {
        scenario();
        return Task.CompletedTask;
    });

    public Task RunAsync(Func<Task> scenario) => dispatcher.InvokeAsync(async () =>
    {
        try { await scenario(); }
        finally
        {
            TestResourceCleanup.ReleaseWindows(application);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            foreach (System.Windows.Window window in application.Windows)
                Assert.Equal(IntPtr.Zero, new System.Windows.Interop.WindowInteropHelper(window).Handle);
        }
    }).Task.Unwrap().WaitAsync(TimeSpan.FromSeconds(30));

    public void Dispose() => TestResourceCleanup.ReleaseFixture(application, thread, dispatcher);

    private sealed class ResourceApplication : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e) { }
    }
}

[CollectionDefinition("WPF", DisableParallelization = true)]
public sealed class WpfCollection : ICollectionFixture<StaExecutionScenarios>;
