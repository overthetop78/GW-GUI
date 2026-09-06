using GWGUI.App.Views.Controls.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Services.Maintenance;
using GWGUI.App.Services.Operations;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Logging;
using GWGUI.Infrastructure.Processes;
using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.MaintenanceViews;
internal static class ToolOperationScenarios
{
    public static async Task Outcome(bool clean, int exit)
    {
        var context = new Context(); var running = context.Execute(clean); await context.WaitUntilStarted(running);
        Assert.True(context.Operation.IsRunning); Assert.Equal("Common.Stop", context.Button(clean).Content);
        context.Operation.Report(new(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, "synthetic measurement"));
        if (exit < 0) context.Pending.SetException(new IOException("synthetic disconnect"));
        else context.Pending.SetResult(new(exit, false, TimeSpan.Zero, []));
        await running;
        Assert.Equal(clean ? "clean" : "erase", Assert.Single(context.Commands).Verb);
        Assert.Contains("virtual-device", context.Commands[0].Arguments); Assert.Contains("B", context.Commands[0].Arguments);
        Assert.Equal(exit == 0 ? "Status.Success" : "Status.Error", context.Model.OperationText);
        Assert.False(context.Operation.IsRunning); Assert.Equal(Visibility.Collapsed, context.Model.TimerVisibility);
        Assert.Equal("Common.Execute", context.Button(clean).Content); Assert.Contains("synthetic measurement", context.Output.Text);
        Assert.Equal(exit < 0 ? 1 : 0, context.Errors.Count);
    }
    internal sealed class Context
    {
        public ToolsTabSection View { get; } = new();
        public MainWindowViewModel Model { get; } = new("synthetic", "synthetic");
        public TextBox Output { get; } = new();
        public MaintenanceToolsController Controller { get; }
        public OperationRuntimeController Operation { get; }
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<GwExecutionResult> Pending { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public List<GwCommand> Commands { get; } = [];
        public List<Exception> Errors { get; } = [];
        public List<string> Messages { get; } = [];
        public List<string> Previews { get; } = [];
        public CancellationToken Token { get; private set; }
        public int StopPrompts { get; private set; }
        public bool AcceptStop { get; set; }
        public Context()
        {
            var log = new ConsoleLogSession("virtual-log", () => new OperationLogSettings { Enabled = false });
            var progress = new OperationProgressController(Model, new TrackProgressStrip(), new TrackProgressStrip(), (key, _) => key);
            Operation = new(Dispatcher.CurrentDispatcher, Model, progress, Output, log, (key, _) => key, (error, _) => Errors.Add(error));
            var runner = ControlledDependencies.Simulate<IGreaseweazleRunner>((method, args) => {
                Assert.Equal("RunAsync", method.Name); Commands.Add(Assert.IsType<GwCommand>(args[0])); Token = (CancellationToken)args[2]!; Started.TrySetResult(); return Pending.Task;
            });
            Controller = new(View, () => new AppSettings { GwExecutablePath = "virtual-tool" }, new GwCommandBuilder(), () => "virtual-device", () => "B", () => true, Previews.Add, (key, _) => key,
                Operation, ControlledDependencies.Simulate<IMessageDialogService>((method, args) => {
                    Assert.Equal("Show", method.Name); Assert.Equal("Advanced.Invalid", args[0]); Messages.Add((string)args[0]!);
                    return GWGUI.App.Enums.Services.Dialogs.UserDialogResult.Ok;
                }), () => true, () => { StopPrompts++; if (AcceptStop) Operation.RequestCancellation(); }, log, runner, Output, path => path == "virtual-tool", (error, _) => Errors.Add(error));
        }
        public Task Execute(bool clean) => clean ? Controller.ExecuteCleanAsync() : Controller.ExecuteEraseAsync();
        public Button Button(bool clean) => clean ? View.CleanExecuteButton : View.EraseExecuteButton;
        public async Task WaitUntilStarted(Task running)
        {
            await Task.WhenAny(Started.Task, running);
            if (!Started.Task.IsCompleted) { await running; Assert.Fail("Tool returned before calling the runner."); }
        }
    }
    public static void Commands()
    {
        var section = new ToolsTabSection(); var controller = ToolSelectionScenarios.Controller(section);
        section.EraseTracksEnabled.IsChecked = true; section.EraseTracksValue.Text = " c=1-3:h=0 ";
        section.EraseRevsEnabled.IsChecked = false; section.EraseExpertArguments.Text = "--fake-index 0.2";
        Assert.Equal(new[] { "--device", "virtual-device", "--drive", "B", "--tracks", "c=1-3:h=0", "--fake-index", "0.2" }, controller.BuildErase().Arguments);
        section.CleanCylindersEnabled.IsChecked = true; section.CleanCylindersValue.Text = "40";
        section.CleanPassesEnabled.IsChecked = true; section.CleanPassesValue.Text = "3";
        section.CleanLingerEnabled.IsChecked = false;
        Assert.Equal(new[] { "--device", "virtual-device", "--drive", "B", "--cylinders", "40", "--passes", "3" }, controller.BuildClean().Arguments);
        var erase = 0; var clean = 0;
        section.EraseRequested += (sender, _) => { Assert.Same(section, sender); erase++; };
        section.CleanRequested += (sender, _) => { Assert.Same(section, sender); clean++; };
        section.EraseExecuteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        section.CleanExecuteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(1, erase); Assert.Equal(1, clean);
    }
}
