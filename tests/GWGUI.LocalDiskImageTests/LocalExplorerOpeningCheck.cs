using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.DiskImages.Exploration;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Contracts.Explorer;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaAudit.TestInfrastructure;

namespace GWGUI.MediaAudit;

/// <summary>Checks the real Explorer opening path for any image supplied on the command line.</summary>
internal static class LocalExplorerOpeningCheck
{
    public static async Task RunAsync(
        string path,
        bool requireFiles,
        string? expectedFormatId,
        int minimumFormats)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Dispatcher? dispatcher = null;
        var thread = new Thread(() =>
        {
            ResourceApplication? application = null;
            DiskImageCancellationScope? scope = null;
            Exception? failure = null;
            try
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
                application = new ResourceApplication { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                application.Resources = new ResourceDictionary
                {
                    Source = new Uri("/gwgui.app;component/Resources/ApplicationStyles.xaml", UriKind.Relative)
                };
                scope = new DiskImageCancellationScope();
                var engine = MediaEngineComposition.CreateDefault();
                var opening = new MediaOpeningAnalysisService(
                    engine.ReadingService,
                    DiskImageExplorer.CreateDefault(),
                    engine.Explorer);
                var explorer = new ExplorerSection();
                var visualizer = new VisualizerTabSection();
                var formats = new BuiltInImageFormatCatalog().Formats;
                explorer.SetFormats(formats, null);
                visualizer.Header.SetFormats(formats);
                Exception? presentedError = null;
                var controller = new ExplorerPresentationController(
                    explorer,
                    visualizer,
                    (source, format, scpProgress, progress, token, scpImageLoaded) =>
                        opening.AnalyzeAsync(source, format, scpProgress, progress, token,
                            includeFileSystems: true, scpImageLoaded: scpImageLoaded),
                    scope,
                    () => { },
                    _ => { },
                    _ => { },
                    (error, _, _, _) => presentedError = error,
                    (key, _) => key);

                var frame = new DispatcherFrame();
                GWGUI.MediaEngine.Images.Formats.Floppy.Scp.ScpImage? earlyFlux = null;
                var load = controller.LoadAsync(path, true, null, image => earlyFlux = image);
                _ = load.ContinueWith(
                    _ => dispatcher.BeginInvoke(() => frame.Continue = false),
                    TaskScheduler.Default);
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var timeoutRegistration = timeout.Token.Register(() =>
                {
                    scope.CancelAll();
                    if (!dispatcher.HasShutdownStarted)
                        dispatcher.BeginInvoke(() => frame.Continue = false);
                });
                Dispatcher.PushFrame(frame);
                if (!load.IsCompleted) throw new TimeoutException("Explorer opening exceeded 30 seconds.");
                var document = load.GetAwaiter().GetResult();
                if (presentedError is not null)
                    throw new InvalidOperationException("Explorer reported an opening error.", presentedError);
                if (document is null || controller.CurrentOpeningResult?.MediaExploration is null)
                    throw new InvalidOperationException("Explorer did not return the decoded image and its volumes.");
                if (document.ScpImage is not null && !ReferenceEquals(earlyFlux, document.ScpImage))
                    throw new InvalidOperationException(
                        "Explorer did not pass the loaded SCP flux to the visualizer before decoding.");
                var mediaResult = controller.CurrentOpeningResult.MediaExploration;
                if (expectedFormatId is not null &&
                    !mediaResult.Document.FormatId.Equals(expectedFormatId, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        $"Explorer returned format '{mediaResult.Document.FormatId}' instead of '{expectedFormatId}'.");
                var volumes = mediaResult.Volumes;
                var fileCount = volumes.SelectMany(volume => volume.FileSystem?.Entries ?? [])
                    .Sum(CountFiles);
                if (requireFiles && fileCount == 0)
                    throw new InvalidOperationException("Explorer did not return any file from the image.");
                var formatSelector = explorer.FindName("DetectedFormatSelector") as ComboBox
                    ?? throw new InvalidOperationException("Explorer format selector was not created.");
                if (formatSelector.Items.Count < minimumFormats ||
                    (minimumFormats > 1 && formatSelector.Visibility != Visibility.Visible))
                    throw new InvalidOperationException(
                        $"Explorer showed {formatSelector.Items.Count} format choice(s), expected at least {minimumFormats}.");
                if (formatSelector.Items.Count == 1 && formatSelector.Visibility != Visibility.Collapsed)
                    throw new InvalidOperationException(
                        "Explorer showed the detected-format selector for a single recognized format.");
                if (expectedFormatId is not null &&
                    (!string.Equals(explorer.ClassificationSelector.SelectedFormatId, expectedFormatId,
                        StringComparison.OrdinalIgnoreCase) ||
                     string.IsNullOrWhiteSpace(explorer.ClassificationSelector.SelectedMachine)))
                    throw new InvalidOperationException(
                        "Explorer did not connect the detected format to the machine and format selector.");
                if (minimumFormats > 1)
                {
                    var automatic = explorer.FindName("AutomaticDetection") as CheckBox
                        ?? throw new InvalidOperationException("Explorer automatic selection was not created.");
                    var choices = formatSelector.Items.Cast<GWGUI.App.ViewModels.Explorer.ExplorerFormatChoice>()
                        .ToArray();
                    var alternate = choices.First(choice =>
                        !string.Equals(choice.Id, mediaResult.Document.FormatId,
                            StringComparison.OrdinalIgnoreCase));
                    formatSelector.SelectedItem = alternate;
                    if (!string.Equals(explorer.SelectedFormatId, alternate.Id,
                            StringComparison.OrdinalIgnoreCase) || automatic.IsChecked != true)
                        throw new InvalidOperationException(
                            "Explorer's detected-format selector depends on automatic detection.");
                    formatSelector.SelectedItem = choices.First(choice =>
                        string.Equals(choice.Id, mediaResult.Document.FormatId,
                            StringComparison.OrdinalIgnoreCase));
                    if (!string.Equals(explorer.SelectedFormatId, mediaResult.Document.FormatId,
                            StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(
                            "Explorer's detected-format selector did not restore the original interpretation.");
                    var machineSelector = explorer.ClassificationSelector.FindName("Machine") as ComboBox
                        ?? throw new InvalidOperationException("Explorer machine selector was not created.");
                    machineSelector.SelectedItem = machineSelector.Items.Cast<object>()
                        .First(item => !ReferenceEquals(item, machineSelector.SelectedItem));
                    if (automatic.IsChecked != false)
                        throw new InvalidOperationException(
                            "Manual machine selection did not disable automatic detection.");
                    formatSelector.SelectedItem = alternate;
                    if (!string.Equals(explorer.SelectedFormatId, alternate.Id,
                            StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(
                            "Explorer's detected-format selector did not work after manual selection.");
                }
                Console.WriteLine($"Explorer opened {mediaResult.Document.FormatId} and returned {volumes.Count} volume(s), {fileCount} file(s), {formatSelector.Items.Count} format choice(s).");
            }
            catch (Exception error)
            {
                failure = error;
            }
            finally
            {
                try
                {
                    scope?.Dispose();
                }
                catch (Exception cleanupError)
                {
                    failure = Combine(failure, cleanupError);
                }
                try
                {
                    if (application is not null)
                        WpfResourceCleanup.Release(application);
                    else if (dispatcher is not null && !dispatcher.HasShutdownStarted)
                        dispatcher.InvokeShutdown();
                }
                catch (Exception cleanupError)
                {
                    failure = Combine(failure, cleanupError);
                }
                if (failure is null) finished.TrySetResult();
                else finished.TrySetException(failure);
            }
        }) { IsBackground = true, Name = "Local Explorer image check" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Exception? runFailure = null;
        try
        {
            await finished.Task;
        }
        catch (Exception error)
        {
            runFailure = error;
        }
        finally
        {
            try
            {
                WpfResourceCleanup.WaitForShutdown(thread, dispatcher);
            }
            catch (Exception cleanupError)
            {
                runFailure = Combine(runFailure, cleanupError);
            }
        }

        if (runFailure is not null)
            throw runFailure;
    }

    private static int CountFiles(FileSystemEntry entry) =>
        (entry.Kind == FileSystemEntryKind.File ? 1 : 0) + entry.Children.Sum(CountFiles);

    private static Exception Combine(Exception? first, Exception second) =>
        first is null ? second : new AggregateException(first, second);

    private sealed class ResourceApplication : Application
    {
        protected override void OnStartup(StartupEventArgs e) { }
    }
}
