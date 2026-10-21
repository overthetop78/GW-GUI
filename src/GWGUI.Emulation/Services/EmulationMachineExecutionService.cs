using System.Collections.Concurrent;

namespace GWGUI.Emulation.Services;

/// <summary>Relais interne vers les opérations de durée de vie, de commandes et de publication existantes.</summary>
internal sealed class EmulationMachineExecutionService : IEmulationMachineExecution
{
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
    public void PublishOutputs<TMachine>(TMachine machine, ref long videoSequence, Func<TMachine, VideoFrame?> latestVideo, Func<TMachine, AudioChunk?> dequeueAudio, Action<TMachine, VideoFrame> publishVideo, Action<TMachine, AudioChunk> publishAudio)
        => EmulationFrameOutputFunctions.PublishOutputs(machine, ref videoSequence, latestVideo, dequeueAudio, publishVideo, publishAudio);

    /// <inheritdoc/>
    public ValueTask EnqueueCommand(ConcurrentQueue<EmulationMachineCommand> commands, object gate, Action action, CancellationToken cancellationToken, Action? validateInsideGate = null)
        => EmulationMachineCommand.Enqueue(commands, gate, action, cancellationToken, validateInsideGate);

    /// <inheritdoc/>
    public void FailPendingCommands(ConcurrentQueue<EmulationMachineCommand> commands, Exception error)
        => EmulationMachineCommand.FailPending(commands, error);
}
