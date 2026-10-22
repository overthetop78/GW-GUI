using System.Reflection;
using System.Runtime.CompilerServices;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class MachineThreadOwnershipTests
{
    private static readonly Type[] Machines =
    [
        typeof(GWGUI.Emulation.Acorn.Common.Services.Machine),
        typeof(GWGUI.Emulation.Amstrad.Common.Services.Machine),
        typeof(GWGUI.Emulation.Apple.Common.Services.Machine),
        typeof(GWGUI.Emulation.Atari.Common.Services.Machine),
        typeof(GWGUI.Emulation.Bandai.Common.Services.Machine),
        typeof(GWGUI.Emulation.Coleco.Common.Services.Machine),
        typeof(GWGUI.Emulation.Commodore.Common.Services.Machine),
        typeof(GWGUI.Emulation.Epoch.Common.Services.Machine),
        typeof(GWGUI.Emulation.Fairchild.Common.Services.Machine),
        typeof(GWGUI.Emulation.Mattel.Common.Services.Machine),
        typeof(GWGUI.Emulation.Microsoft.Common.Services.Machine),
        typeof(GWGUI.Emulation.Nec.Common.Services.Machine),
        typeof(GWGUI.Emulation.Nintendo.Common.Services.Machine),
        typeof(GWGUI.Emulation.Panasonic.Common.Services.Machine),
        typeof(GWGUI.Emulation.Philips.Common.Services.Machine),
        typeof(GWGUI.Emulation.Sega.Common.Services.Machine),
        typeof(GWGUI.Emulation.Sharp.Common.Services.Machine),
        typeof(GWGUI.Emulation.Sinclair.Common.Services.Machine),
        typeof(GWGUI.Emulation.Snk.Common.Services.Machine),
        typeof(GWGUI.Emulation.Sony.Common.Services.Machine),
        typeof(GWGUI.Emulation.Thomson.Common.Services.Machine),
        typeof(GWGUI.Emulation.Watara.Common.Services.Machine),
        typeof(GWGUI.Emulation.Wenquxing.Common.Services.Machine),
    ];

    public static TheoryData<Type, EmulationMachineState, bool, bool> StopCases
    {
        get
        {
            var cases = new TheoryData<Type, EmulationMachineState, bool, bool>();
            foreach (var machine in Machines)
            {
            foreach (var state in new[] { EmulationMachineState.Running, EmulationMachineState.Stopped, EmulationMachineState.Faulted })
            foreach (var failLoop in new[] { false, true })
                cases.Add(machine, state, failLoop, false);
                cases.Add(machine, EmulationMachineState.Running, false, true);
            }
            return cases;
        }
    }

    public static TheoryData<Type> MachineTypes
    {
        get
        {
            var cases = new TheoryData<Type>();
            foreach (var machine in Machines) cases.Add(machine);
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(StopCases))]
    public async Task CancelledStopWaitsForItsOwnedWorkerToExit(Type machineType, EmulationMachineState state, bool failLoop, bool cancelThrows)
    {
        var machine = CreateMachine(machineType, state);
        using var stopSource = new CancellationTokenSource();
        using var callerCancellation = new CancellationTokenSource();
        using var releaseWorker = new ManualResetEventSlim();
        using var workerReady = new ManualResetEventSlim();
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelError = new InvalidOperationException("Managed cancellation callback failed");
        using var registration = cancelThrows ? stopSource.Token.Register(() => throw cancelError) : default;
        var loopError = failLoop ? new InvalidOperationException("Managed loop failed") : null;
        Set(machine, machineType.GetField("_stopSource", BindingFlags.Instance | BindingFlags.NonPublic) is not null ? "_stopSource" : "_stop", stopSource);
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
        }) { IsBackground = true, Name = "GW GUI common ownership regression" };
        // The original implementation has no field; its cancelled wait must fail the behavioral assertion first.
        var ownedThread = machineType.GetField("_thread", BindingFlags.Instance | BindingFlags.NonPublic);
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
            if (cancelThrows) Assert.Contains(cancelError, (await Assert.ThrowsAsync<AggregateException>(() => stopping)).InnerExceptions);
            else if (loopError is null) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stopping);
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
                catch (Exception error) when (error is OperationCanceledException || ReferenceEquals(error, loopError) || error is AggregateException aggregate && aggregate.InnerExceptions.Contains(cancelError)) { }
            }
        }
    }

    [Theory]
    [MemberData(nameof(MachineTypes))]
    public async Task CreatedMachineWithoutAWorkerStillStopsImmediately(Type machineType)
    {
        var machine = CreateMachine(machineType, EmulationMachineState.Created);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await machine.StopAsync(cancellation.Token);
        Assert.Equal(EmulationMachineState.Created, ((IEmulatedMachine)machine).State);
    }

    private static IEmulationLifecycle CreateMachine(Type machineType, EmulationMachineState state)
    {
        // StopAsync needs only synchronization state; no constructor, core, output or media is acquired.
        var machine = (IEmulationLifecycle)RuntimeHelpers.GetUninitializedObject(machineType);
        Set(machine, "_gate", new object());
        Set(machine, "<State>k__BackingField", state);
        return machine;
    }

    private static void Set(object machine, string name, object value) =>
        machine.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(machine, value);
}
