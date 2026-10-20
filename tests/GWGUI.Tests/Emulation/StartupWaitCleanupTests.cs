using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class StartupWaitCleanupTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SuccessfulStartDoesNotStopEvenWithAlreadyCanceledToken(bool canceled)
    {
        using var cancellation = new CancellationTokenSource();
        if (canceled) cancellation.Cancel();
        var stops = 0;

        await ExternalCoreLifecycleFunctions.WaitForStartOrStopAsync(
            Task.CompletedTask, cancellation.Token,
            () => { stops++; return ValueTask.CompletedTask; });

        Assert.Equal(0, stops);
    }

    [Fact]
    public async Task FailedStartStopsOnceAndPreservesOriginalFailure()
    {
        var failure = new InvalidOperationException("start");
        var stops = 0;

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ExternalCoreLifecycleFunctions.WaitForStartOrStopAsync(
                Task.FromException(failure), CancellationToken.None,
                () => { stops++; return ValueTask.CompletedTask; }).AsTask());

        Assert.Same(failure, actual);
        Assert.Equal(1, stops);
    }

    [Fact]
    public async Task CanceledWaitStopsOnceBeforeCancellationIsPropagated()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stops = 0;
        try
        {
            var actual = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                ExternalCoreLifecycleFunctions.WaitForStartOrStopAsync(
                    start.Task, cancellation.Token,
                    () => { stops++; return ValueTask.CompletedTask; }).AsTask());

            Assert.Equal(cancellation.Token, actual.CancellationToken);
            Assert.Equal(1, stops);
        }
        finally { start.TrySetResult(); }
    }

    [Fact]
    public async Task FailedStartCannotReturnUntilStopCompletes()
    {
        var failure = new InvalidOperationException("start");
        var cleanup = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stops = 0;
        var pending = ExternalCoreLifecycleFunctions.WaitForStartOrStopAsync(
            Task.FromException(failure), CancellationToken.None,
            () => { stops++; return new ValueTask(cleanup.Task); }).AsTask();
        try
        {
            Assert.Equal(1, stops);
            Assert.False(pending.IsCompleted);
        }
        finally
        {
            cleanup.TrySetResult();
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => pending);
            Assert.Same(failure, actual);
        }
    }

    [Fact]
    public async Task StopFailureTakesPrecedenceAsInTheOriginalFinally()
    {
        var cleanupFailure = new ApplicationException("stop");
        var actual = await Assert.ThrowsAsync<ApplicationException>(() =>
            ExternalCoreLifecycleFunctions.WaitForStartOrStopAsync(
                Task.FromException(new InvalidOperationException("start")), CancellationToken.None,
                () => ValueTask.FromException(cleanupFailure)).AsTask());

        Assert.Same(cleanupFailure, actual);
    }
}
