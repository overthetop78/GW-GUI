using GWGUI.App.Services.Operations;
namespace GWGUI.Tests.Application.OperationLifecycle;
internal static class OperationStateScenarios
{
    public static async Task Concurrency()
    {
        var coordinator=new OperationCoordinator();
        var signal=new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        Assert.True(coordinator.WaitForCompletionAsync().IsCompleted);
        var running=coordinator.RunAsync(_=>signal.Task);
        Assert.True(coordinator.IsRunning);
        Assert.False(coordinator.WaitForCompletionAsync().IsCompleted);
        var second=await coordinator.RunAsync<int>(_=>throw new Exception("must not run"));
        Assert.False(second.HasResult);
        Assert.IsType<InvalidOperationException>(second.Error);
        signal.SetResult(42);
        var result=await running.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(result.HasResult);
        Assert.Equal(42,result.Result);
        Assert.Null(result.Error);
        Assert.False(coordinator.IsRunning);
        await coordinator.WaitForCompletionAsync().WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(7,(await coordinator.RunAsync(_=>Task.FromResult(7))).Result);
    }
    public static async Task Error()
    {
        var coordinator=new OperationCoordinator();
        var error=new IOException("synthetic");
        var result=await coordinator.RunAsync<int>(_=>throw error);
        Assert.Same(error,result.Error);
        Assert.False(result.HasResult);
        Assert.False(coordinator.IsRunning);
        Assert.True(coordinator.WaitForCompletionAsync().IsCompleted);
        Assert.True((await coordinator.RunAsync(_=>Task.FromResult(1))).HasResult);
    }
}
