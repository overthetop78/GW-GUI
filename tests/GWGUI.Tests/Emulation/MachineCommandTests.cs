using System.Collections.Concurrent;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Tests.Emulation;

public sealed class MachineCommandTests
{
    [Fact]
    public async Task EnqueuedActionCompletesItsWaitAfterExecution()
    {
        var queue = new ConcurrentQueue<EmulationMachineCommand>();
        var calls = 0;
        var wait = EmulationMachineCommand.Enqueue(queue, new object(), () => calls++, CancellationToken.None);
        Assert.False(wait.IsCompleted);
        Assert.True(queue.TryDequeue(out var command));
        Assert.NotNull(command);
        Assert.True(command.Completion.Task.CreationOptions.HasFlag(TaskCreationOptions.RunContinuationsAsynchronously));
        command.Execute();
        await wait;
        Assert.Equal(1, calls);
        Assert.True(command.Completion.Task.IsCompletedSuccessfully);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothExceptionPoliciesFaultTheCompletionWithTheOriginalException(bool rethrow)
    {
        var error = new InvalidOperationException("command failure");
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new EmulationMachineCommand(() => throw error, completion);
        if (rethrow)
            Assert.Same(error, Assert.Throws<InvalidOperationException>(() => command.Execute(rethrow: true)));
        else
            command.Execute();
        Assert.True(completion.Task.IsFaulted);
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => completion.Task.GetAwaiter().GetResult()));
    }

    [Fact]
    public void CancellingTheWaitDoesNotRemoveOrCancelTheQueuedAction()
    {
        var queue = new ConcurrentQueue<EmulationMachineCommand>();
        using var stop = new CancellationTokenSource();
        var calls = 0;
        var wait = EmulationMachineCommand.Enqueue(queue, new object(), () => calls++, stop.Token);
        stop.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => wait.GetAwaiter().GetResult());
        Assert.True(queue.TryDequeue(out var command));
        Assert.NotNull(command);
        command.Execute();
        Assert.Equal(1, calls);
        Assert.True(command.Completion.Task.IsCompletedSuccessfully);
    }

    [Fact]
    public void AtomicValidationRunsInsideTheGateAndFailureDoesNotEnqueue()
    {
        var queue = new ConcurrentQueue<EmulationMachineCommand>();
        var gate = new object();
        var error = new InvalidOperationException("machine stopped");
        var validatedInsideGate = false;
        var observed = Assert.Throws<InvalidOperationException>(() =>
            EmulationMachineCommand.Enqueue(queue, gate, () => throw new InvalidOperationException(),
                CancellationToken.None, validateInsideGate: () =>
                {
                    validatedInsideGate = Monitor.IsEntered(gate);
                    throw error;
                }));
        Assert.True(validatedInsideGate);
        Assert.Same(error, observed);
        Assert.Empty(queue);
        Assert.False(Monitor.IsEntered(gate));
    }

    [Fact]
    public void FailingPendingCommandsDoesNotExecuteThemAndFaultsRatherThanCancels()
    {
        var queue = new ConcurrentQueue<EmulationMachineCommand>();
        var gate = new object();
        var calls = 0;
        var first = EmulationMachineCommand.Enqueue(queue, gate, () => calls++, CancellationToken.None);
        var second = EmulationMachineCommand.Enqueue(queue, gate, () => calls++, CancellationToken.None);
        var error = new OperationCanceledException("machine stopped");
        EmulationMachineCommand.FailPending(queue, error);
        Assert.Empty(queue);
        Assert.Equal(0, calls);
        Assert.True(first.AsTask().IsFaulted);
        Assert.True(second.AsTask().IsFaulted);
        Assert.Same(error, Assert.Throws<OperationCanceledException>(() => first.GetAwaiter().GetResult()));
        Assert.Same(error, Assert.Throws<OperationCanceledException>(() => second.GetAwaiter().GetResult()));
    }
}
