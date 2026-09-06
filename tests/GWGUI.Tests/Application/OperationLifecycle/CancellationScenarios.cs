using GWGUI.App.Services.Operations;
namespace GWGUI.Tests.Application.OperationLifecycle;
internal static class CancellationScenarios
{
    public static async Task Completion(bool observesCancellation)
    {
        var coordinator=new OperationCoordinator();
        var pending=new TaskCompletionSource<int>(); CancellationToken token=default;
        var running=coordinator.RunAsync(async current=>{token=current; var result=await pending.Task; if(observesCancellation) current.ThrowIfCancellationRequested(); return result;});
        var completion=coordinator.WaitForCompletionAsync(); Assert.False(completion.IsCompleted);
        coordinator.RequestCancellation(); Assert.True(token.IsCancellationRequested); pending.SetResult(42);
        var outcome=await running; await completion;
        Assert.Equal(!observesCancellation,outcome.HasResult);
        if(observesCancellation) Assert.IsAssignableFrom<OperationCanceledException>(outcome.Error); else { Assert.Equal(42,outcome.Result); Assert.Null(outcome.Error); }
        coordinator.RequestCancellation(); Assert.False(coordinator.IsRunning);
        Assert.Equal(93,(await coordinator.RunAsync(current=>{Assert.False(current.IsCancellationRequested); return Task.FromResult(93);})).Result);
    }
    public static async Task Cancel()
    {
        var coordinator=new OperationCoordinator();
        coordinator.RequestCancellation();
        var gate=new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken captured=default;
        var running=coordinator.RunAsync(async token=>{
            captured=token;
            using var registration=token.Register(()=>gate.TrySetCanceled(token));
            return await gate.Task;
        });
        Assert.True(captured.CanBeCanceled);
        coordinator.RequestCancellation();
        var result=await running.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsAssignableFrom<OperationCanceledException>(result.Error);
        Assert.False(result.HasResult);
        Assert.False(coordinator.IsRunning);
        Assert.True(coordinator.WaitForCompletionAsync().IsCompleted);
        Assert.True((await coordinator.RunAsync(t=>Task.FromResult(!t.IsCancellationRequested))).Result);
    }
}
