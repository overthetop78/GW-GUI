using GWGUI.Domain.Commands;

namespace GWGUI.App.ViewModels;

public enum OperationResultState { Success, Error, Cancelled }

public sealed record OperationResultMessage(string ResourceKey, IReadOnlyList<object> Arguments, bool StartOnNewLine = false);

public sealed record OperationResultPresentation(OperationResultState State, IReadOnlyList<OperationResultMessage> Messages);

public sealed class OperationResultPresenter
{
    public OperationResultPresentation Present(OperationOutcome<GwExecutionResult> outcome)
    {
        if (outcome.Error is { } error) return Error(error);
        var result = outcome.Result!;
        var state = result.WasCancelled ? OperationResultState.Cancelled : result.IsSuccess ? OperationResultState.Success : OperationResultState.Error;
        return new(state, [new("Operation.Finished", [result.ExitCode, result.Duration.ToString("g")], true)]);
    }

    public OperationResultPresentation Present(OperationOutcome<GwBatchExecutionResult> outcome)
    {
        if (outcome.Error is { } error) return Error(error);
        var result = outcome.Result!;
        var state = result.WasCancelled ? OperationResultState.Cancelled : result.FailedLabels.Count == 0 ? OperationResultState.Success : OperationResultState.Error;
        var messages = new List<OperationResultMessage>
        {
            new("Conversion.Summary", [result.SuccessfulCount, result.FailedLabels.Count], true)
        };
        if (result.FailedLabels.Count > 0)
            messages.Add(new("Conversion.Failures", [string.Join(", ", result.FailedLabels)]));
        return new(state, messages);
    }

    private static OperationResultPresentation Error(Exception error) =>
        new(OperationResultState.Error, [new("Operation.Error", [error.Message])]);
}
