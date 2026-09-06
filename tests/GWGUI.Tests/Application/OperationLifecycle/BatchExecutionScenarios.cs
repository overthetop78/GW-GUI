using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
namespace GWGUI.Tests.Application.OperationLifecycle;
internal static class BatchExecutionScenarios
{
    private sealed class Runner(Queue<GwExecutionResult> responses):IGreaseweazleRunner
    {
        public List<string> Calls { get; }=[];
        public bool IsRunning=>false;
        public Task<GwExecutionResult> RunAsync(GwCommand command,IProgress<GwOutputLine>? output=null,CancellationToken cancellationToken=default)
        { Calls.Add(command.Verb); return Task.FromResult(responses.Dequeue()); }
    }
    public static async Task Run(bool cancel)
    {
        var runner=new Runner(new Queue<GwExecutionResult>([
            new(0,false,TimeSpan.Zero,[]),new(1,cancel,TimeSpan.Zero,[]),new(0,false,TimeSpan.Zero,[])]));
        var starting=new List<string>();
        var items=new[]{"one","two","three"}.Select(x=>new GwBatchItem(x,new("virtual",x,[]))).ToArray();
        var result=await new GwBatchExecutor(runner).RunAsync(items,itemStarting:i=>starting.Add(i.Label));
        Assert.Equal(cancel?new[]{"one","two"}:new[]{"one","two","three"},runner.Calls);
        Assert.Equal(runner.Calls,starting);
        Assert.Equal(cancel?1:2,result.SuccessfulCount);
        Assert.Equal(cancel?Array.Empty<string>():new[]{"two"},result.FailedLabels);
        Assert.Equal(cancel,result.WasCancelled);
        using var cancellation=new CancellationTokenSource();
        cancellation.Cancel();
        var empty=await new GwBatchExecutor(runner).RunAsync(items,cancellationToken:cancellation.Token);
        Assert.Empty(empty.Items);
        Assert.True(empty.WasCancelled);
    }
}
