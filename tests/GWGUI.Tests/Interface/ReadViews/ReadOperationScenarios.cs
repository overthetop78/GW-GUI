using GWGUI.App.Controllers.MainWindow;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Operations;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Controls.Write;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.Domain.Settings.Logging;
using GWGUI.Infrastructure.Processes;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Tests.Interface.VisualizerViews;
using GWGUI.Tests.Interface.ExplorerViews;
namespace GWGUI.Tests.Interface.ReadViews;
internal static class ReadOperationScenarios
{
    public static async Task Outcome(int exit)
    {
        using var context = new Context(); await Dispatcher.Yield(DispatcherPriority.ContextIdle); var running = context.Controller.ExecuteAsync();
        await Task.WhenAny(context.Started.Task, running);
        if (!context.Started.Task.IsCompleted) { await running; Assert.Fail("Read returned before calling the runner."); }
        Assert.True(context.Operation.IsRunning); Assert.Equal(Visibility.Visible, context.Model.TimerVisibility);
        Assert.Equal(LocExtension.Get("Common.Stop"), context.View.ExecuteActionButton.Content);
        context.Operation.Report(new(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, "synthetic progress"));
        foreach (var line in new[] { "Reading c=0-1:h=0-1", "T0.0: done", "T0.1: done" })
            context.Operation.Report(new(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, line));
        Assert.Equal(Visibility.Visible, context.Model.Face0ProgressVisibility);
        Assert.Equal(Visibility.Visible, context.Model.Face1ProgressVisibility);
        Assert.Equal(50, context.Model.Face0ProgressValue); Assert.Equal(50, context.Model.Face1ProgressValue);
        context.Operation.Report(new(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, "T1.0: done"));
        Assert.Equal(100, context.Model.Face0ProgressValue); Assert.Equal(50, context.Model.Face1ProgressValue);
        if (exit < 0) context.Pending.SetException(new IOException("synthetic read failure"));
        else context.Pending.SetResult(new(exit, false, TimeSpan.Zero, []));
        await running;
        Assert.False(context.Operation.IsRunning); Assert.Equal(Visibility.Collapsed, context.Model.TimerVisibility);
        Assert.Equal(exit == 0 ? "Status.Success" : "Status.Error", context.Model.OperationText);
        Assert.Contains("synthetic progress", context.Output.Text);
        Assert.Equal(LocExtension.Get("Common.Execute"), context.View.ExecuteActionButton.Content);
        Assert.Equal(exit == 0 ? "2" : "1", context.Model.Read.SequenceValue);
        Assert.Equal(0, context.Deletions);
        Assert.Equal(exit < 0 ? 1 : 0, context.Errors.Count);
    }
    public static async Task Cancel()
    {
        using var context = new Context(); await Dispatcher.Yield(DispatcherPriority.ContextIdle); var running = context.Controller.ExecuteAsync();
        await Task.WhenAny(context.Started.Task, running);
        if (!context.Started.Task.IsCompleted) { await running; Assert.Fail("Read returned before calling the runner."); }
        await context.Controller.ExecuteAsync(); Assert.True(context.Token.IsCancellationRequested); Assert.Equal(1, context.Calls);
        context.Pending.SetResult(new(0, true, TimeSpan.Zero, [])); await running;
        Assert.Equal("Status.Cancelled", context.Model.OperationText); Assert.Equal(1, context.Deletions);
        Assert.Equal("1", context.Model.Read.SequenceValue); Assert.False(context.Operation.IsRunning);
    }
    public static async Task CaptureSummary(bool failure)
    {
        using var context = new Context();
        context.View.ImageBlock.RawScpRadio.IsChecked = true;
        context.View.ImageBlock.KnownFormatRadio.IsChecked = false;
        var info = new ScpCaptureInfo(ExplorerDocumentScenarios.Document("header").ScpImage!.Header, 3, 1, 2, 2, true, 1234);
        context.CaptureInfo = _ => failure ? Task.FromException<ScpCaptureInfo>(new IOException("summary unavailable")) : Task.FromResult(info);
        await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        var running = context.Controller.ExecuteAsync();
        await Task.WhenAny(context.Started.Task, running);
        Assert.True(context.Started.Task.IsCompleted);
        context.Pending.SetResult(new(0, false, TimeSpan.Zero, [])); await running;
        Assert.Equal(Visibility.Visible, context.View.CompletionBlock.Visibility);
        Assert.EndsWith(".scp", context.Workspace.Controller.LastCapturedPath);
        Assert.Equal("Status.Success", context.Model.OperationText);
        Assert.False(context.Operation.IsRunning); Assert.Equal("2", context.Model.Read.SequenceValue);
        if (failure) { Assert.IsType<IOException>(Assert.Single(context.Errors)); Assert.NotEmpty(context.View.CompletionBlock.SummaryTextBlock.Text); }
        else {
            Assert.Empty(context.Errors);
            var checksum = LocExtension.Get("Visual.ChecksumValid");
            Assert.Equal(LocExtension.Get("Read.ScpBannerSummary", 3, 1, 2, 2, info.Header.Revolutions, 1234L, checksum), context.View.CompletionBlock.SummaryTextBlock.Text);
            Assert.Contains(LocExtension.Get("Read.ScpTracksSummary", 3, 1, 2, 2), context.Output.Text);
        }
    }
    internal sealed class Context : IDisposable
    {
        public VisualizerDocumentScenarios.Workspace Workspace { get; } = new();
        public HashSet<string> ExistingPaths { get; } = new(StringComparer.OrdinalIgnoreCase) { "virtual-tool" };
        public ReadConflictChoice? ConflictChoice { get; set; }
        public List<string> Conflicts { get; } = [];
        public List<string> ValidationMessages { get; } = [];
        public string ValidationKey { get; set; } = "Read.NameRequired";
        public AppSettings Settings { get; } = new() { GwExecutablePath = "virtual-tool" };
        public GWGUI.App.Contracts.Services.Hardware.HardwareChoice? Hardware { get; set; }
        public int NameSelections { get; private set; }
        public GwCommand? Command { get; private set; }
        public Func<string, Task<ScpCaptureInfo>> CaptureInfo { get; set; } = _ => throw new InvalidOperationException("Unexpected capture summary");
        public ReadTabSection View { get; } = new();
        public MainWindowViewModel Model { get; } = new("synthetic", "synthetic");
        public TextBox Output { get; } = new();
        public ReadTabController Controller { get; }
        public OperationRuntimeController Operation { get; }
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<GwExecutionResult> Pending { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public CancellationToken Token { get; private set; }
        public int Calls { get; private set; }
        public int Deletions { get; private set; }
        public List<Exception> Errors { get; } = [];
        public Context()
        {
            Workspace.Explore = (_, _, _) => Task.FromResult(ExplorerDocumentScenarios.Document("completed"));
            var settings = Settings; settings.Engines.PhysicalRead = OperationEngine.GreaseweazleHostTools;
            View.DataContext = Model; Model.Read.FileName = "disk"; Model.Read.Folder = "virtual-folder"; Model.Read.AutoNumber = true; Model.Read.SequenceValue = "1";
            var format = new DiskFormat("test.one", "family", "one", [new(".img", "image", true)]);
            View.ImageBlock.KnownFormatRadio.IsChecked = true; View.ImageBlock.RawScpRadio.IsChecked = false;
            View.ImageBlock.FormatCombo.ItemsSource = new[] { format }; View.ImageBlock.FormatCombo.SelectedIndex = 0;
            View.ImageBlock.ExtensionCombo.ItemsSource = format.Extensions; View.ImageBlock.ExtensionCombo.SelectedIndex = 0;
            var dialogs = ControlledDependencies.Simulate<IMessageDialogService>((method, args) => {
                Assert.Equal("Show", method.Name); Assert.Equal(LocExtension.Get(ValidationKey), args[0]);
                ValidationMessages.Add((string)args[0]!); return UserDialogResult.Ok;
            }); var files = ControlledDependencies.Reject<IFileDialogService>();
            var definitions = new DiskDefinitionsController(View.AdvancedBlock, new WriteAdvancedSection(), new ConversionAdvancedSection(), () => settings, null!, files, dialogs, () => { }, _ => { }, _ => { }, _ => { }, () => { }, () => { }, () => { }, (key, _) => key);
            var log = new ConsoleLogSession("virtual-log", () => new OperationLogSettings { Enabled = false });
            var progress = new OperationProgressController(Model, new TrackProgressStrip(), new TrackProgressStrip(), (key, _) => key);
            Operation = new(Dispatcher.CurrentDispatcher, Model, progress, Output, log, (key, _) => key, (error, _) => Errors.Add(error));
            var runner = ControlledDependencies.Simulate<IGreaseweazleRunner>((method, args) => {
                Assert.Equal("RunAsync", method.Name); Calls++; var command = Assert.IsType<GwCommand>(args[0]);
                Assert.Equal("read", command.Verb); Assert.Equal("virtual-tool", command.ExecutablePath); Command = command;
                Token = (CancellationToken)args[2]!; Started.TrySetResult(); return Pending.Task;
            });
            var catalog = ControlledDependencies.Simulate<IImageFormatCatalog>((method, _) => method.Name == "get_Formats"
                ? new[] { format } : throw new InvalidOperationException(method.Name));
            Controller = new(View, Model, null!, () => catalog, () => settings, new GwCommandBuilder(), files,
                ControlledDependencies.Simulate<IBusinessDialogService>((method, args) => {
                    Assert.Equal("ResolveReadConflict", method.Name); Conflicts.Add((string)args[0]!); return ConflictChoice;
                }), dialogs, definitions, Operation, progress, log, runner, Workspace.Controller, new TextBox(), Output,
                () => "controller", () => "B", () => true, () => Hardware, Operation.RequestCancellation, () => { }, () => { },
                path => path is not null && ExistingPaths.Contains(path), path => { Assert.EndsWith(".img", path); Deletions++; return null; },
                () => NameSelections++, path => CaptureInfo(path), (error, _) => Errors.Add(error));
        }
        public void Dispose() => Workspace.Dispose();
    }
}
