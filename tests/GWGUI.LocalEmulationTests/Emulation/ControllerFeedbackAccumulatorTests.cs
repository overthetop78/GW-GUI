using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Interop;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GWGUI.Tests.Emulation;

public sealed class ControllerFeedbackAccumulatorTests
{
    [Fact]
    public void EnvironmentCallbackPublishesEffectsAndStopsAfterDisposal()
    {
        var type = typeof(EmulationSdk).Assembly.GetType("GWGUI.Emulation.Services.ExternalCoreEnvironmentCallbackHost", throwOnError: true)!;
        // Ce propriétaire est limité au stockage et à une table de répertoires vide : aucun fichier ni cœur natif n’est ouvert.
        var host = (IEmulationEnvironmentCallbackHost)RuntimeHelpers.GetUninitializedObject(type);
        using var states = EmulationSdk.EnvironmentProcessing.CreateControllerFeedbackAccumulator(2);
        var directories = type.GetField("_directories", BindingFlags.Instance | BindingFlags.NonPublic)!;
        directories.SetValue(host, Activator.CreateInstance(directories.FieldType));
        type.GetField("<Diagnostics>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(host, new List<string>());
        type.GetField("_feedback", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(host, states);
        ExternalCoreApi.SetRumbleState callback = states.SetState;
        type.GetField("_setRumbleState", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(host, callback);
        var receive = type.GetMethod("OnEnvironment", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.RumbleInterface>());
        try
        {
            Assert.False(Assert.IsType<bool>(receive.Invoke(host, [ExternalCoreApiConstants.GetRumbleInterface, nint.Zero])));
            Assert.True(Assert.IsType<bool>(receive.Invoke(host, [ExternalCoreApiConstants.GetRumbleInterface, pointer])));
            var table = Marshal.PtrToStructure<ExternalCoreApi.RumbleInterface>(pointer);
            var native = Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.SetRumbleState>(table.SetState);
            Assert.True(native(1, 1, ushort.MaxValue));
            Assert.Equal(new ControllerFeedbackState(1, EmulationControllerFeedbackCommands.Weak, 1f), Assert.Single(host.ControllerFeedback));
            host.ResetControllerFeedback(1);
            Assert.Empty(host.ControllerFeedback);
            host.Dispose();
            Assert.False(native(0, 0, ushort.MaxValue));
            Assert.Empty(host.ControllerFeedback);
            Assert.False(Assert.IsType<bool>(receive.Invoke(host, [ExternalCoreApiConstants.GetRumbleInterface, pointer])));
        }
        finally
        {
            try { host.Dispose(); }
            finally { Marshal.FreeHGlobal(pointer); GC.KeepAlive(callback); }
        }
    }

    [Fact]
    public void MotorsAndPortsKeepIndependentNormalizedSnapshots()
    {
        using var feedback = EmulationSdk.EnvironmentProcessing.CreateControllerFeedbackAccumulator(2);
        Assert.True(feedback.SetState(0, 0, ushort.MaxValue));
        Assert.True(feedback.SetState(0, 1, 32768));
        Assert.True(feedback.SetState(1, 0, 1));
        var before = feedback.Snapshot();
        Assert.Equal(new[]
        {
            new ControllerFeedbackState(0, EmulationControllerFeedbackCommands.Strong, 1f),
            new ControllerFeedbackState(0, EmulationControllerFeedbackCommands.Weak, 32768f / ushort.MaxValue),
            new ControllerFeedbackState(1, EmulationControllerFeedbackCommands.Strong, 1f / ushort.MaxValue)
        }, before);
        Assert.Equal(before, feedback.Snapshot());
        Assert.True(feedback.SetState(0, 0, 0));
        Assert.Equal(3, before.Count);
        Assert.Equal(2, feedback.Snapshot().Count);
        feedback.ResetPort(0);
        Assert.Equal(1, Assert.Single(feedback.Snapshot()).Port);
        feedback.ResetPort(1);
        Assert.Empty(feedback.Snapshot());
    }

    [Fact]
    public void InvalidIndexesAndLateCallbacksDoNotActivateEffects()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EmulationSdk.EnvironmentProcessing.CreateControllerFeedbackAccumulator(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmulationSdk.EnvironmentProcessing.CreateControllerFeedbackAccumulator(-1));
        using var feedback = EmulationSdk.EnvironmentProcessing.CreateControllerFeedbackAccumulator(2);
        Assert.False(feedback.SetState(2, 0, 1));
        Assert.False(feedback.SetState(uint.MaxValue, 0, 1));
        Assert.False(feedback.SetState(0, 2, 1));
        Assert.False(feedback.SetState(0, uint.MaxValue, 1));
        feedback.ResetPort(-1);
        feedback.ResetPort(2);
        Assert.Empty(feedback.Snapshot());
        Assert.True(feedback.SetState(1, 1, ushort.MaxValue));
        feedback.Dispose();
        Assert.Empty(feedback.Snapshot());
        Assert.False(feedback.SetState(1, 1, ushort.MaxValue));
        feedback.ResetPort(0);
        Assert.False(feedback.SetState(0, 0, 1));
    }

    [Fact]
    public void ConcurrentUpdatesProduceValidSnapshots()
    {
        using var feedback = EmulationSdk.EnvironmentProcessing.CreateControllerFeedbackAccumulator(4);
        Parallel.For(0, 1000, index =>
        {
            Assert.True(feedback.SetState((uint)(index % 4), (uint)(index % 2), (ushort)(index + 1)));
            foreach (var state in feedback.Snapshot()) Assert.InRange(state.Strength, 0f, 1f);
        });
        var snapshot = feedback.Snapshot();
        Assert.Equal(4, snapshot.Count);
        Assert.Equal(snapshot.Count, snapshot.Select(state => (state.Port, state.CommandId)).Distinct().Count());
    }
}
