using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class MachineResourceDisposalTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task AttemptsAllOrderedReleasesEvenWhenOneFails(int failingStep)
    {
        var calls = new List<int>();
        var failure = new InvalidOperationException("release");
        void Release(int step)
        {
            calls.Add(step);
            if (step == failingStep) throw failure;
        }
        var pending = ExternalCoreLifecycleFunctions.DisposeMachineAsync(
            () => { Release(0); return ValueTask.CompletedTask; },
            () => Release(1), () => Release(2), () => Release(3), () => Release(4)).AsTask();

        if (failingStep < 0) await pending;
        else Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => pending));
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, calls);
    }

    [Fact]
    public async Task LastFailureTakesPrecedenceWithoutSkippingCleanup()
    {
        var calls = new List<int>();
        var cleanupFailure = new ApplicationException("cleanup");
        var actual = await Assert.ThrowsAsync<ApplicationException>(() =>
            ExternalCoreLifecycleFunctions.DisposeMachineAsync(
                () => ValueTask.FromException(new InvalidOperationException("stop")),
                () => { calls.Add(1); throw new InvalidOperationException("first"); },
                () => calls.Add(2),
                () => { calls.Add(3); throw new InvalidOperationException("third"); },
                () => { calls.Add(4); throw cleanupFailure; }).AsTask());

        Assert.Same(cleanupFailure, actual);
        Assert.Equal(new[] { 1, 2, 3, 4 }, calls);
    }

    [Fact]
    public async Task DoesNotReleaseResourcesUntilStopHasFinished()
    {
        var stop = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = new List<int>();
        var pending = ExternalCoreLifecycleFunctions.DisposeMachineAsync(
            () => new ValueTask(stop.Task), () => calls.Add(1), () => calls.Add(2),
            () => calls.Add(3), () => calls.Add(4)).AsTask();
        try
        {
            Assert.False(pending.IsCompleted);
            Assert.Empty(calls);
        }
        finally
        {
            stop.TrySetResult();
            await pending;
        }
        Assert.Equal(new[] { 1, 2, 3, 4 }, calls);
    }
}
