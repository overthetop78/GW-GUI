using GWGUI.Domain.Commands;
namespace GWGUI.Domain.Commands.Execution;

public sealed record GwBatchItem(string Label, GwCommand Command);
public sealed record GwBatchItemResult(GwBatchItem Item, GwExecutionResult Result);
public sealed record GwBatchExecutionResult(IReadOnlyList<GwBatchItemResult> Items, bool WasCancelled)
{
    public int SuccessfulCount => Items.Count(item => item.Result.IsSuccess);
    public IReadOnlyList<string> FailedLabels => Items.Where(item => !item.Result.IsSuccess && !item.Result.WasCancelled).Select(item => item.Item.Label).ToArray();
}

public sealed class GwBatchExecutor(IGreaseweazleRunner runner)
{
    public async Task<GwBatchExecutionResult> RunAsync(
        IReadOnlyList<GwBatchItem> items,
        IProgress<GwOutputLine>? output = null,
        Action<GwBatchItem>? itemStarting = null,
        CancellationToken cancellationToken = default)
    {
        var completed = new List<GwBatchItemResult>(items.Count);
        var cancelled = false;
        foreach (var item in items)
        {
            if (cancellationToken.IsCancellationRequested) { cancelled = true; break; }
            itemStarting?.Invoke(item);
            var result = await runner.RunAsync(item.Command, output, cancellationToken).ConfigureAwait(false);
            completed.Add(new(item, result));
            if (result.WasCancelled) { cancelled = true; break; }
        }
        return new(completed, cancelled || cancellationToken.IsCancellationRequested);
    }
}
