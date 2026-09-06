using GWGUI.App.Controllers.MainWindow;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Operations;
using GWGUI.App.Services.DiskImages;
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
namespace GWGUI.Tests.Interface.WriteViews;
internal static class WriteOperationScenarios
{
    public static async Task Outcome(int exit)
    {
        var context = new Context(); await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        var running = context.Controller.ExecuteAsync(); await context.WaitUntilStarted(running);
        Assert.True(context.Operation.IsRunning); Assert.Equal(Visibility.Visible, context.Model.TimerVisibility);
        Assert.Equal(LocExtension.Get("Common.Stop"), context.View.ExecuteActionButton.Content);
        var verification = exit == 0 ? "verification matches" : "verification mismatch";
        foreach (var line in new[] { "Writing c=0-1:h=0-1", "T0.0: done", "T0.1: done", verification })
            context.Operation.Report(new(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, line));
        Assert.Equal(Visibility.Visible, context.Model.Face0ProgressVisibility);
        Assert.Equal(Visibility.Visible, context.Model.Face1ProgressVisibility);
        Assert.Equal(50, context.Model.Face0ProgressValue); Assert.Equal(50, context.Model.Face1ProgressValue);
        if (exit < 0) context.Pending.SetException(new IOException("synthetic write failure"));
        else context.Pending.SetResult(new(exit, false, TimeSpan.Zero, []));
        await running;
        Assert.False(context.Operation.IsRunning); Assert.Equal(Visibility.Collapsed, context.Model.TimerVisibility);
        Assert.Equal(exit == 0 ? "Status.Success" : "Status.Error", context.Model.OperationText);
        Assert.Contains(verification, context.Output.Text);
        Assert.Equal(LocExtension.Get("Common.Execute"), context.View.ExecuteActionButton.Content);
        Assert.Equal(1, context.Confirmations); Assert.Equal(1, context.Calls);
        Assert.Equal("virtual-source.img", context.Model.Write.SourcePath);
        Assert.Equal(exit < 0 ? 1 : 0, context.Errors.Count);
    }
    public static async Task Cancel()
    {
        var context = new Context(); await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        var running = context.Controller.ExecuteAsync(); await context.WaitUntilStarted(running);
        await context.Controller.ExecuteAsync(); Assert.True(context.Token.IsCancellationRequested); Assert.Equal(1, context.Confirmations);
        context.Pending.SetResult(new(0, true, TimeSpan.Zero, [])); await running;
        Assert.Equal("Status.Cancelled", context.Model.OperationText); Assert.Equal(1, context.Calls);
        Assert.False(context.Operation.IsRunning); Assert.Equal(LocExtension.Get("Common.Execute"), context.View.ExecuteActionButton.Content);
    }
    internal sealed class Context(bool accept = true)
    {
        public WriteTabSection View { get; } = new();
        public MainWindowViewModel Model { get; } = new("synthetic", "synthetic");
        public TextBox Output { get; } = new();
        public WriteTabController Controller { get; private set; } = null!;
        public OperationRuntimeController Operation { get; private set; } = null!;
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<GwExecutionResult> Pending { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public CancellationToken Token { get; private set; }
        public int Calls { get; private set; }
        public int Confirmations { get; private set; }
        public List<Exception> Errors { get; } = [];
        public Context() : this(true) { Initialize(); }
        public static Context Refusing() { var context = new Context(false); context.Initialize(); return context; }
        public async Task WaitUntilStarted(Task running)
        {
            await Task.WhenAny(Started.Task, running);
            if (!Started.Task.IsCompleted) { await running; Assert.Fail("Write returned before calling the runner."); }
        }
        private void Initialize()
        {
            var settings = new AppSettings { GwExecutablePath = "virtual-tool" }; settings.Engines.PhysicalWrite = OperationEngine.GreaseweazleHostTools;
            View.DataContext = Model; Model.Write.SourcePath = "virtual-source.img";
            var format = new DiskFormat("test.one", "family", "one", [new(".img", "image", true)]);
            View.FormatBlock.FormatCombo.ItemsSource = new[] { format }; View.FormatBlock.FormatCombo.SelectedIndex = 0;
            var dialogs = ControlledDependencies.Simulate<IMessageDialogService>((method, args) => {
                Assert.Equal("Show", method.Name); Assert.Equal(LocExtension.Get("Write.ConfirmTitle"), args[1]);
                Assert.Equal(UserDialogButtons.OkCancel, args[2]); Assert.Equal(UserDialogIcon.Warning, args[3]);
                Assert.Contains("virtual-source.img", (string)args[0]!); Confirmations++; return accept ? UserDialogResult.Ok : UserDialogResult.Cancel;
            });
            var files = ControlledDependencies.Reject<IFileDialogService>();
            var definitions = new DiskDefinitionsController(new ReadAdvancedSection(), View.AdvancedBlock, new ConversionAdvancedSection(), () => settings, null!, files, dialogs, () => { }, _ => { }, _ => { }, _ => { }, () => { }, () => { }, () => { }, (key, _) => key);
            var log = new ConsoleLogSession("virtual-log", () => new OperationLogSettings { Enabled = false });
            var progress = new OperationProgressController(Model, new TrackProgressStrip(), new TrackProgressStrip(), (key, _) => key);
            Operation = new(Dispatcher.CurrentDispatcher, Model, progress, Output, log, (key, _) => key, (error, _) => Errors.Add(error));
            var runner = ControlledDependencies.Simulate<IGreaseweazleRunner>((method, args) => {
                Assert.Equal("RunAsync", method.Name); Calls++; var command = Assert.IsType<GwCommand>(args[0]);
                Assert.Equal("write", command.Verb); Assert.Equal("virtual-tool", command.ExecutablePath);
                Assert.Contains("virtual-source.img", command.Arguments); Assert.Contains("controller", command.Arguments); Assert.Contains("B", command.Arguments);
                Token = (CancellationToken)args[2]!; Started.TrySetResult(); return Pending.Task;
            });
            Controller = new(View, Model, null!, () => ControlledDependencies.Reject<IImageFormatCatalog>(), null!, () => settings, new GwCommandBuilder(), files,
                dialogs, definitions, Operation, progress, log, runner, null!, new TextBox(), new TextBox(), Output,
                () => 1, _ => { }, () => "controller", () => "B", () => true, () => null, Operation.RequestCancellation,
                _ => throw new InvalidOperationException(), (_, _) => throw new InvalidOperationException(), _ => throw new InvalidOperationException(), (_, _) => throw new InvalidOperationException(), () => { },
                path => path is "virtual-tool" or "virtual-source.img");
        }
    }
}
