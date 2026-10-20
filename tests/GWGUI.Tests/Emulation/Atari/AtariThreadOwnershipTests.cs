using System.Reflection;
using System.Runtime.CompilerServices;
using GWGUI.Emulation.Enums;
using AtariMachine = GWGUI.Emulation.Atari.Common.Services.Machine;

namespace GWGUI.Tests.Emulation.Atari;

public sealed class AtariThreadOwnershipTests
{
    [Theory]
    [InlineData(EmulationMachineState.Running, false)]
    [InlineData(EmulationMachineState.Running, true)]
    [InlineData(EmulationMachineState.Stopped, false)]
    [InlineData(EmulationMachineState.Stopped, true)]
    [InlineData(EmulationMachineState.Faulted, false)]
    [InlineData(EmulationMachineState.Faulted, true)]
    public async Task CancelledStopWaitsForItsOwnedWorkerToExit(EmulationMachineState state, bool failLoop)
    {
        var machine = CreateMachine(state);
        using var stopSource = new CancellationTokenSource();
        using var callerCancellation = new CancellationTokenSource();
        using var releaseWorker = new ManualResetEventSlim();
        using var workerReady = new ManualResetEventSlim();
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var loopError = failLoop ? new InvalidOperationException("Managed loop failed") : null;
        Set(machine, "_stopSource", stopSource);
        Set(machine, "_runLoop", completed.Task);
        var worker = new Thread(() =>
        {
            try
            {
                workerReady.Set();
                releaseWorker.Wait();
            }
            finally
            {
                if (loopError is null) completed.TrySetResult();
                else completed.TrySetException(loopError);
            }
        }) { IsBackground = true, Name = "GW GUI Atari ownership regression" };
        // The original implementation has no field; its cancelled wait must fail the behavioral assertion first.
        var ownedThread = typeof(AtariMachine).GetField("_thread", BindingFlags.Instance | BindingFlags.NonPublic);
        ownedThread?.SetValue(machine, worker);
        Task? stopping = null;
        var workerStarted = false;
        try
        {
            worker.Start();
            workerStarted = true;
            Assert.True(workerReady.Wait(TimeSpan.FromSeconds(5)));
            callerCancellation.Cancel();
            stopping = machine.StopAsync(callerCancellation.Token).AsTask();

            Assert.False(stopping.IsCompleted);
            Assert.True(worker.IsAlive);
            releaseWorker.Set();
            if (loopError is null) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stopping);
            else Assert.Same(loopError, await Assert.ThrowsAsync<InvalidOperationException>(() => stopping));
            Assert.False(worker.IsAlive);
            Assert.NotNull(ownedThread);
        }
        finally
        {
            releaseWorker.Set();
            if (workerStarted) worker.Join();
            if (stopping is not null)
            {
                try { await stopping; }
                catch (Exception error) when (error is OperationCanceledException || ReferenceEquals(error, loopError)) { }
            }
        }
    }

    [Fact]
    public async Task CreatedMachineWithoutAWorkerStillStopsImmediately()
    {
        var machine = CreateMachine(EmulationMachineState.Created);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await machine.StopAsync(cancellation.Token);
        Assert.Equal(EmulationMachineState.Created, machine.State);
    }

    private static AtariMachine CreateMachine(EmulationMachineState state)
    {
        // StopAsync needs only synchronization state; no constructor, core, output or media is acquired.
        var machine = (AtariMachine)RuntimeHelpers.GetUninitializedObject(typeof(AtariMachine));
        Set(machine, "_gate", new object());
        Set(machine, "<State>k__BackingField", state);
        return machine;
    }

    private static void Set(AtariMachine machine, string name, object value) =>
        typeof(AtariMachine).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(machine, value);
}
