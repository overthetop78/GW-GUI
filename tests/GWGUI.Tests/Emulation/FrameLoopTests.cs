using System.Collections.Concurrent;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Services;

namespace GWGUI.Tests.Emulation;

public sealed class FrameLoopTests
{
    [Fact]
    public void InitiallyCancelledLoopDoesNoWork()
    {
        using var stop = new CancellationTokenSource();
        stop.Cancel();
        EmulationFrameTimer.RunLoop(0, new object(), new(),
            () => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            () => throw new InvalidOperationException(), (_, _) => throw new InvalidOperationException(),
            (_, _) => throw new InvalidOperationException(), stop.Token, 100);
    }

    [Fact]
    public void CommandUnpausesBeforeFrameAndTiming()
    {
        using var stop = new CancellationTokenSource();
        var gate = new object();
        var paused = true;
        var events = new List<string>();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var commands = new ConcurrentQueue<EmulationMachineCommand>();
        commands.Enqueue(new(() => { events.Add("command"); paused = false; }, completion));
        EmulationFrameTimer.RunLoop(123, gate, commands,
            () => { Assert.True(Monitor.IsEntered(gate)); return paused; },
            () => events.Add("frame"),
            () => { events.Add("frequency"); return 50; },
            (timestamp, frequency) =>
            {
                Assert.Equal(123, timestamp);
                Assert.Equal(50, frequency);
                events.Add("timestamp");
                return 456;
            },
            (timestamp, token) =>
            {
                Assert.Equal(456, timestamp);
                Assert.Equal(stop.Token, token);
                events.Add("wait");
                stop.Cancel();
            }, stop.Token, 100);
        Assert.Equal(new[] { "command", "frame", "frequency", "timestamp", "wait" }, events);
        Assert.True(completion.Task.IsCompletedSuccessfully);
        Assert.False(Monitor.IsEntered(gate));
    }

    [Fact]
    public void CommandCanCancelWhilePausedWithoutRunningAFrame()
    {
        using var stop = new CancellationTokenSource();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var commands = new ConcurrentQueue<EmulationMachineCommand>();
        commands.Enqueue(new(stop.Cancel, completion));
        EmulationFrameTimer.RunLoop(0, new object(), commands, () => true,
            () => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            (_, _) => throw new InvalidOperationException(), (_, _) => throw new InvalidOperationException(),
            stop.Token, 100);
        Assert.True(completion.Task.IsCompletedSuccessfully);
        Assert.Empty(commands);
    }

    [Fact]
    public void CommandFailureFaultsItsCompletionAndEscapesBeforeFrame()
    {
        var error = new InvalidOperationException("command failed");
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var commands = new ConcurrentQueue<EmulationMachineCommand>();
        commands.Enqueue(new(() => throw error, completion));
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            EmulationFrameTimer.RunLoop(0, new object(), commands, () => false,
                () => throw new NotSupportedException(), () => throw new NotSupportedException(),
                (_, _) => throw new NotSupportedException(), (_, _) => throw new NotSupportedException(),
                CancellationToken.None, 100)));
        Assert.True(completion.Task.IsFaulted);
        Assert.Same(error, Assert.Single(completion.Task.Exception!.InnerExceptions));
    }

    [Fact]
    public void FrameFailureEscapesBeforeTiming()
    {
        var error = new InvalidOperationException("frame failed");
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            EmulationFrameTimer.RunLoop(0, new object(), new(), () => false,
                () => throw error, () => throw new NotSupportedException(),
                (_, _) => throw new NotSupportedException(), (_, _) => throw new NotSupportedException(),
                CancellationToken.None, 100)));
    }

    [Fact]
    public void HistoricalOwnerGuardStopsBeforePauseOrFrame()
    {
        EmulationFrameTimer.RunLoop(new object(), new(),
            () => throw new InvalidOperationException("pause must not run"),
            () => throw new InvalidOperationException("frame must not run"),
            () => false, CancellationToken.None, 100);
    }

    [Fact]
    public void HistoricalCommandFailureStaysInItsCompletionAndTheFrameRuns()
    {
        var gate = new object();
        var error = new InvalidOperationException("command failed");
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var commands = new ConcurrentQueue<EmulationMachineCommand>();
        commands.Enqueue(new(() => { Assert.False(Monitor.IsEntered(gate)); throw error; }, completion));
        var frames = 0;
        EmulationFrameTimer.RunLoop(gate, commands,
            () => { Assert.True(Monitor.IsEntered(gate)); return false; },
            () => { Assert.False(Monitor.IsEntered(gate)); frames++; return false; },
            () => true, CancellationToken.None, 100);
        Assert.Equal(1, frames);
        Assert.True(completion.Task.IsFaulted);
        Assert.Same(error, Assert.Single(completion.Task.Exception!.InnerExceptions));
        Assert.Empty(commands);
    }

    [Fact]
    public void HistoricalCommandUnpausesBeforeTheFrame()
    {
        var gate = new object();
        var paused = true;
        var events = new List<string>();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var commands = new ConcurrentQueue<EmulationMachineCommand>();
        commands.Enqueue(new(() => { events.Add("command"); paused = false; }, completion));
        EmulationFrameTimer.RunLoop(gate, commands, () => paused,
            () => { events.Add("frame"); return false; }, () => true, CancellationToken.None, 100);
        Assert.Equal(new[] { "command", "frame" }, events);
        Assert.True(completion.Task.IsCompletedSuccessfully);
    }

    [Fact]
    public void HistoricalFrameStopDoesNotRecheckTheOwnerGuard()
    {
        var guardCalls = 0;
        var frames = 0;
        EmulationFrameTimer.RunLoop(new object(), new(), () => false,
            () => { frames++; return false; },
            () => { Assert.Equal(1, ++guardCalls); return true; }, CancellationToken.None, 100);
        Assert.Equal(1, frames);
        Assert.Equal(1, guardCalls);
    }

    [Fact]
    public void HistoricalFrameFailureEscapesWithoutAnotherGuardCall()
    {
        var error = new InvalidOperationException("frame failed");
        var guardCalls = 0;
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            EmulationFrameTimer.RunLoop(new object(), new(), () => false,
                () => throw error, () => { guardCalls++; return true; }, CancellationToken.None, 100)));
        Assert.Equal(1, guardCalls);
    }
}
