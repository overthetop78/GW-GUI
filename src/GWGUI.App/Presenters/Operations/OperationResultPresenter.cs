using GWGUI.Domain.Commands.Execution;
using GWGUI.App.Contracts.ViewModels.Operations;
using GWGUI.App.Enums.ViewModels.Operations;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Services.Logging;

namespace GWGUI.App.Presenters.Operations;

public sealed class OperationResultPresenter
{
    public OperationResultPresentation Present(OperationOutcome<GwExecutionResult> outcome)
    {
        if (outcome.Error is { } error) return Error(error);
        var result = outcome.Result!;
        var state = result.WasCancelled ? OperationResultState.Cancelled : result.IsSuccess ? OperationResultState.Success : OperationResultState.Error;
        var statusKey = state switch
        {
            OperationResultState.Success => "Operation.Succeeded",
            OperationResultState.Cancelled => "Operation.Cancelled",
            _ => "Operation.ExitCode"
        };
        var statusArguments = state == OperationResultState.Error ? new object[] { result.ExitCode } : [];
        return new(state,
        [
            new(statusKey, statusArguments, true),
            new("Operation.Finished", [result.ExitCode, result.Duration.ToString("g")], true)
        ]);
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

    private static OperationResultPresentation Error(Exception error)
    {
        ErrorLog.Write(error, "Running Greaseweazle operation");
        var detail = ExceptionDescriptionFunctions.Describe(error);
        return new(OperationResultState.Error, [new("Error.Unexpected", [detail])]);
    }
}
