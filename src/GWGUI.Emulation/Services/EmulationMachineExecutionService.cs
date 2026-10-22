using System.Collections.Concurrent;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

/// <summary>Relais interne vers les opérations de durée de vie, de commandes et de publication existantes.</summary>
internal sealed class EmulationMachineExecutionService : IEmulationMachineExecution
{
    /// <inheritdoc/>
    public IEmulationFrameTimer CreateFrameTimer(CancellationToken cancellationToken) => new EmulationFrameTimer(cancellationToken);

    /// <inheritdoc/>
    public long WaitForNextFrame(long nextFrame, double framesPerSecond, TimeProvider clock, Action<TimeSpan> sleep) => EmulationFrameTimer.WaitForNextFrame(nextFrame, framesPerSecond, clock, sleep);

    public Dictionary<EmulationMediaSlot, bool> FromLedStates(IReadOnlyDictionary<int, bool> leds) => EmulationMediaRules.FromLedStates(leds);
    public IReadOnlyDictionary<EmulationMediaSlot, bool> FromLedStatesWithCd(IReadOnlyDictionary<int, bool> leds) => EmulationMediaRules.FromLedStatesWithCd(leds);

    public byte[] SerializeState(Func<ExternalCoreExports> requireExports, bool supported, nuint maximumSize, Func<Exception> unavailable, Func<Exception> saveFailed) => ExternalCoreLifecycleFunctions.SerializeState(requireExports, supported, maximumSize, unavailable, saveFailed);
    public void UnserializeState(Func<ExternalCoreExports> requireExports, bool supported, nuint maximumSize, ReadOnlySpan<byte> state, Func<Exception> invalidSize, Func<Exception> loadFailed) => ExternalCoreLifecycleFunctions.UnserializeState(requireExports, supported, maximumSize, state, invalidSize, loadFailed);
    public bool IsSupportedStateSize(nuint size, nuint maximumStateSize) => ExternalCoreLifecycleFunctions.IsSupportedStateSize(size, maximumStateSize);
    public void DisposeCallbackResources(IDisposable? systemDirectory, IDisposable? contentDirectory, IDisposable? saveDirectory, IDisposable? assetsDirectory, IDisposable? virtualFileSystem, IDisposable? videoBuffers, IDisposable optionHost) => ExternalCoreLifecycleFunctions.DisposeCallbackResources(systemDirectory, contentDirectory, saveDirectory, assetsDirectory, virtualFileSystem, videoBuffers, optionHost);
    public void Cleanup(ExternalCoreExports? exports, bool gameLoaded, bool initialized, Action disposeCallbacks, Action disposeLibrary) => ExternalCoreLifecycleFunctions.Cleanup(exports, gameLoaded, initialized, disposeCallbacks, disposeLibrary);

    /// <inheritdoc/>
    public void StartThread(Action<CancellationToken> run, string name, CancellationToken cancellationToken, ref CancellationTokenSource? stop, out TaskCompletionSource started, out Task? loop, ref Thread? thread)
        => ExternalCoreLifecycleFunctions.StartThread(run, name, cancellationToken, ref stop, out started, out loop, ref thread);

    /// <inheritdoc/>
    public void RunCore(Action initialize, Action run, Func<Exception, Action> selectFailureHandler, Action<bool> cleanup)
        => ExternalCoreLifecycleFunctions.RunCore(initialize, run, selectFailureHandler, cleanup);

    /// <inheritdoc/>
    public ValueTask DisposeMachineAsync(Func<ValueTask> stop, Action disposeFirst, Action disposeSecond, Action disposeThird, Action cleanup)
        => ExternalCoreLifecycleFunctions.DisposeMachineAsync(stop, disposeFirst, disposeSecond, disposeThird, cleanup);

    /// <inheritdoc/>
    public ValueTask WaitForStartOrStopAsync(Task started, CancellationToken cancellationToken, Func<ValueTask> stop)
        => ExternalCoreLifecycleFunctions.WaitForStartOrStopAsync(started, cancellationToken, stop);

    /// <inheritdoc/>
    public ValueTask WaitForThreadAsync(Task? loop, Thread? thread)
        => ExternalCoreLifecycleFunctions.WaitForThreadAsync(loop, thread);

    /// <inheritdoc/>
    public bool TryDisposeCore(IDisposable core, Action<Exception> reportFailure)
        => ExternalCoreLifecycleFunctions.TryDisposeCore(core, reportFailure);

    /// <inheritdoc/>
    public void RunLoop(long nextFrame, object gate, ConcurrentQueue<EmulationMachineCommand> commands, Func<bool> isPaused, Action runFrame, Func<double> framesPerSecond, Func<long, double, long> nextFrameTimestamp, Action<long, CancellationToken> waitUntil, CancellationToken cancellationToken, int pauseWaitMilliseconds)
        => EmulationFrameTimer.RunLoop(nextFrame, gate, commands, isPaused, runFrame, framesPerSecond, nextFrameTimestamp, waitUntil, cancellationToken, pauseWaitMilliseconds);

    /// <inheritdoc/>
    public void RunLoop(object gate, ConcurrentQueue<EmulationMachineCommand> commands, Func<bool> isPaused, Func<bool> runFrame, Func<bool> keepRunning, CancellationToken cancellationToken, int pauseWaitMilliseconds)
        => EmulationFrameTimer.RunLoop(gate, commands, isPaused, runFrame, keepRunning, cancellationToken, pauseWaitMilliseconds);

    /// <inheritdoc/>
    public void PublishOutputs<TMachine>(TMachine machine, ref long videoSequence, Func<TMachine, VideoFrame?> latestVideo, Func<TMachine, AudioChunk?> dequeueAudio, Action<TMachine, VideoFrame> publishVideo, Action<TMachine, AudioChunk> publishAudio)
        => EmulationFrameOutputFunctions.PublishOutputs(machine, ref videoSequence, latestVideo, dequeueAudio, publishVideo, publishAudio);

    /// <inheritdoc/>
    public ValueTask EnqueueCommand(ConcurrentQueue<EmulationMachineCommand> commands, object gate, Action action, CancellationToken cancellationToken, Action? validateInsideGate = null)
        => EmulationMachineCommand.Enqueue(commands, gate, action, cancellationToken, validateInsideGate);

    /// <inheritdoc/>
    public void FailPendingCommands(ConcurrentQueue<EmulationMachineCommand> commands, Exception error)
        => EmulationMachineCommand.FailPending(commands, error);
}
