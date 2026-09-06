using GWGUI.App.Controllers.MainWindow;
using GWGUI.App.Contracts.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Conversion;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Operations;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Controls.Write;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.App.Views.Dialogs.Conversion;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.Domain.Settings.Logging;
using GWGUI.Infrastructure.Processes;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace GWGUI.Tests.Interface.ConversionViews;
internal static class ConversionOperationScenarios
{
    public static async Task PartialBatch(bool firstFails)
    {
        var context = new Context();
        context.SecondPending = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        var running = context.Controller.ExecuteAsync(); await context.WaitUntilStarted(running);
        foreach (var line in new[] { "Converting c=0-1:h=0-1", "T0.0: done" })
            context.Operation.Report(new(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, line));
        Assert.Equal(50, context.Model.Face0ProgressValue);
        context.Pending.SetResult(new(firstFails ? 5 : 0, false, TimeSpan.Zero, []));
        await Task.WhenAny(context.SecondStarted.Task, running); Assert.True(context.SecondStarted.Task.IsCompleted);
        Assert.True(context.Operation.IsRunning); Assert.Equal(2, context.Commands.Count);
        foreach (var line in new[] { "Converting c=0-0:h=1", "T0.1: done" })
            context.Operation.Report(new(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, line));
        Assert.Equal(Visibility.Collapsed, context.Model.Face0ProgressVisibility);
        Assert.Equal(100, context.Model.Face1ProgressValue);
        context.SecondPending.SetResult(new(firstFails ? 0 : 5, false, TimeSpan.Zero, [])); await running;
        var summary = Assert.Single(context.Presentations, item => item.Key == "Conversion.Summary");
        Assert.Equal(new object[] { 1, 1 }, summary.Arguments);
        var failed = Assert.Single(context.Presentations, item => item.Key == "Conversion.Failures");
        var failedPath = context.Commands[firstFails ? 0 : 1].Arguments.Last();
        var successfulPath = context.Commands[firstFails ? 1 : 0].Arguments.Last();
        Assert.Contains(Path.GetFileName(failedPath), Assert.IsType<string>(Assert.Single(failed.Arguments)));
        Assert.DoesNotContain(Path.GetFileName(successfulPath), Assert.IsType<string>(Assert.Single(failed.Arguments)));
        Assert.Equal("Status.Error", context.Model.OperationText); Assert.False(context.Operation.IsRunning);
    }
    public static async Task Outcome(int exit)
    {
        var context = new Context(); await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        var running = context.Controller.ExecuteAsync(); await context.WaitUntilStarted(running);
        Assert.True(context.Operation.IsRunning); Assert.Equal(Visibility.Visible, context.Model.TimerVisibility);
        Assert.Equal(LocExtension.Get("Common.Stop"), context.View.ExecuteActionButton.Content);
        foreach (var line in new[] { "Converting c=0-1:h=0-1", "T0.0: done" })
            context.Operation.Report(new(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, line));
        Assert.Contains("T0.0: done", context.Output.Text);
        if (exit < 0) context.Pending.SetException(new IOException("synthetic conversion failure"));
        else context.Pending.SetResult(new(exit, false, TimeSpan.Zero, []));
        await running;
        Assert.Equal(exit < 0 ? 1 : 2, context.Commands.Count);
        Assert.Equal(exit == 0 ? "Status.Success" : "Status.Error", context.Model.OperationText);
        Assert.False(context.Operation.IsRunning); Assert.Equal(Visibility.Collapsed, context.Model.TimerVisibility);
        Assert.Equal(LocExtension.Get("Common.Execute"), context.View.ExecuteActionButton.Content);
        if (exit >= 0) { Assert.Contains("Conversion.Summary", context.Output.Text); Assert.Contains("disk.ima", context.Output.Text); Assert.Contains("disk.img", context.Output.Text); }
        if (exit > 0) Assert.Contains("Conversion.Failures", context.Output.Text);
        Assert.Equal(exit < 0 ? 1 : 0, context.Errors.Count);
    }
    public static async Task Cancel()
    {
        var context = new Context(); await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        var running = context.Controller.ExecuteAsync(); await context.WaitUntilStarted(running);
        await context.Controller.ExecuteAsync(); Assert.True(context.Token.IsCancellationRequested);
        context.Pending.SetResult(new(0, true, TimeSpan.Zero, [])); await running;
        Assert.Single(context.Commands); Assert.Equal("Status.Cancelled", context.Model.OperationText); Assert.False(context.Operation.IsRunning);
    }
    internal sealed class Context
    {
        public ConversionTabSection View { get; } = new();
        public MainWindowViewModel Model { get; } = new("synthetic", "synthetic");
        public TextBox Output { get; } = new();
        public ConversionTabController Controller { get; }
        public OperationRuntimeController Operation { get; }
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<GwExecutionResult> Pending { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<GwExecutionResult>? SecondPending { get; set; }
        public TaskCompletionSource SecondStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public List<(string Key, object[] Arguments)> Presentations { get; } = [];
        public CancellationToken Token { get; private set; }
        public List<GwCommand> Commands { get; } = [];
        public List<Exception> Errors { get; } = [];
        public AppSettings Settings { get; } = new() { GwExecutablePath = "virtual-tool" };
        public int ConflictPrompts { get; private set; }
        public Context(bool conflicts = false, ConversionConflictChoice? choice = null)
        {
            var settings = Settings; settings.Engines.Conversion = OperationEngine.GreaseweazleHostTools;
            View.DataContext = Model; Model.Conversion.SourcePath = "virtual-source.scp"; Model.Conversion.OutputName = "disk"; Model.Conversion.AddTags = false;
            Model.Conversion.SetFormat("ibm.160", true, new HashSet<string> { ".ima", ".img" });
            var catalog = new BuiltInImageFormatCatalog(key => key);
            var dialogs = ControlledDependencies.Reject<IMessageDialogService>(); var files = ControlledDependencies.Reject<IFileDialogService>();
            var business = ControlledDependencies.Simulate<IBusinessDialogService>((method, args) => {
                Assert.Equal("ResolveConversionConflicts", method.Name); ConflictPrompts++;
                var outputs = Assert.IsAssignableFrom<IReadOnlyList<ConversionOutput>>(args[0]); Assert.Equal(2, outputs.Count);
                return choice is null ? null : outputs.Select(output => new ConversionConflictDecision(output, choice.Value)).ToArray();
            });
            var definitions = new DiskDefinitionsController(new ReadAdvancedSection(), new WriteAdvancedSection(), View.AdvancedBlock, () => settings, null!, files, dialogs, () => { }, _ => { }, _ => { }, _ => { }, () => { }, () => { }, () => { }, (key, _) => key);
            var log = new ConsoleLogSession("virtual-log", () => new OperationLogSettings { Enabled = false });
            var progress = new OperationProgressController(Model, new TrackProgressStrip(), new TrackProgressStrip(), (key, _) => key);
            Operation = new(Dispatcher.CurrentDispatcher, Model, progress, Output, log,
                (key, values) => { Presentations.Add((key, values)); return key; }, (error, _) => Errors.Add(error));
            var runner = ControlledDependencies.Simulate<IGreaseweazleRunner>((method, args) => {
                Assert.Equal("RunAsync", method.Name); var command = Assert.IsType<GwCommand>(args[0]); Commands.Add(command);
                Assert.Equal("convert", command.Verb); Assert.Equal("virtual-tool", command.ExecutablePath); Assert.Contains("virtual-source.scp", command.Arguments);
                Token = (CancellationToken)args[2]!; Started.TrySetResult();
                if (Commands.Count == 1) return Pending.Task;
                SecondStarted.TrySetResult();
                return SecondPending?.Task ?? Task.FromResult(new GwExecutionResult(0, false, TimeSpan.Zero, []));
            });
            Controller = new(new Window(), View, Model, null!, new ConversionFormatPresenter(), () => catalog, null!, () => settings, new GwCommandBuilder(), runner, files, business, dialogs,
                definitions, Operation, log, null!, new TextBox { Text = "virtual-folder" }, new TextBox(), Output, () => 0, _ => { }, null!, Operation.RequestCancellation, (_, _) => throw new InvalidOperationException(), () => { }, Dispatcher.CurrentDispatcher,
                path => path is "virtual-tool" or "virtual-source.scp" || conflicts && path is not null && !path.Contains("(2)"));
        }
        public async Task WaitUntilStarted(Task running)
        {
            await Task.WhenAny(Started.Task, running);
            if (!Started.Task.IsCompleted) { await running; Assert.Fail("Conversion returned before calling the runner."); }
        }
    }
}
