using GWGUI.Domain.Commands.Execution;
using GWGUI.App.Contracts.ViewModels.Operations;
using GWGUI.App.Enums.ViewModels.Operations;
using GWGUI.App.Presenters.Operations;
using GWGUI.App.ViewModels.Main;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GWGUI.Infrastructure.Processes;

namespace GWGUI.App.Services.Operations;

public sealed class OperationRuntimeController
{
    private readonly OperationCoordinator _coordinator = new();
    private readonly OperationResultPresenter _resultPresenter = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly Dispatcher _dispatcher;
    private readonly MainWindowViewModel _viewModel;
    private readonly OperationProgressController _progress;
    private readonly TextBox _output;
    private readonly ConsoleLogSession _consoleLog;
    private readonly Func<string, object[], string> _localize;

    public OperationRuntimeController(
        Dispatcher dispatcher,
        MainWindowViewModel viewModel,
        OperationProgressController progress,
        TextBox output,
        ConsoleLogSession consoleLog,
        Func<string, object[], string> localize)
    {
        _dispatcher = dispatcher;
        _viewModel = viewModel;
        _progress = progress;
        _output = output;
        _consoleLog = consoleLog;
        _localize = localize;
        _timer.Tick += (_, _) => UpdateElapsedTime();
    }

    public bool IsRunning => _coordinator.IsRunning;
    public void RequestCancellation() => _coordinator.RequestCancellation();
    public Task WaitForCompletionAsync() => _coordinator.WaitForCompletionAsync();
    public Task<OperationOutcome<T>> RunAsync<T>(Func<CancellationToken, Task<T>> operation) => _coordinator.RunAsync(operation);
    public OperationResultPresentation Present(OperationOutcome<GwExecutionResult> outcome) => _resultPresenter.Present(outcome);
    public OperationResultPresentation Present(OperationOutcome<GwBatchExecutionResult> outcome) => _resultPresenter.Present(outcome);

    public void Begin()
    {
        if (!_stopwatch.IsRunning)
        {
            _stopwatch.Restart();
            _timer.Start();
            _viewModel.TimerVisibility = Visibility.Visible;
            UpdateElapsedTime();
        }
        _progress.Begin();
    }

    public Task RenderPendingAsync() => _dispatcher.InvokeAsync(static () => { }, DispatcherPriority.Render).Task;

    public void Report(GwOutputLine line)
    {
        AppendText(line.Text + Environment.NewLine);
        _progress.Accept(line.Text);
    }

    public Task FlushPendingAsync() => _dispatcher.InvokeAsync(static () => { }, DispatcherPriority.ContextIdle).Task;

    public void End()
    {
        _stopwatch.Stop();
        _timer.Stop();
        UpdateElapsedTime();
        _viewModel.TimerVisibility = Visibility.Collapsed;
        _progress.End();
    }

    public void Apply(OperationResultPresentation presentation)
    {
        switch (presentation.State)
        {
            case OperationResultState.Success: SetState("Status.Success", Color.FromRgb(63, 171, 91)); break;
            case OperationResultState.Cancelled: SetState("Status.Cancelled", Color.FromRgb(220, 148, 45)); break;
            default: SetState("Status.Error", Color.FromRgb(210, 66, 66)); break;
        }
        foreach (var message in presentation.Messages)
        {
            if (message.StartOnNewLine) AppendText(Environment.NewLine);
            AppendText(_localize(message.ResourceKey, message.Arguments.ToArray()));
        }
    }

    public void AppendText(string text)
    {
        _output.AppendText(text);
        _output.ScrollToEnd();
        _ = _consoleLog.AppendTextAsync(text);
    }

    public void SetState(string resourceKey, Color color) => _progress.SetState(resourceKey, color);

    private void UpdateElapsedTime()
    {
        var elapsed = _stopwatch.Elapsed;
        _viewModel.ElapsedText = $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }
}
