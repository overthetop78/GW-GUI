using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Acorn.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    private MachineConfiguration CurrentConfiguration() => Configuration with { Media = _mountedMedia.ToArray() };

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_gate)
        {
            if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
                throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
            _commands.Enqueue(new MachineCommand(action, completion));
            Monitor.PulseAll(_gate);
        }
        return new ValueTask(completion.Task.WaitAsync(cancellationToken));
    }

    private ValueTask QueueCommand<T>(Action<T> action, T value, CancellationToken cancellationToken) => QueueCommand(() => action(value), cancellationToken);

    private void Run(CancellationToken cancellationToken)
    {
        var initialized = false;
        try
        {
            _core.Initialize(Configuration, _sessionDirectory, _saveDirectory);
            initialized = true;
            StartAudio();
            lock (_gate)
            {
                State = EmulationMachineState.Running;
                _started?.TrySetResult();
            }
            var nextFrame = Stopwatch.GetTimestamp();
            using var frameTimer = new FrameTimer(cancellationToken);
            long videoSequence = default;
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (_gate)
                    while (_pauseRequested && _commands.IsEmpty && !cancellationToken.IsCancellationRequested)
                        Monitor.Wait(_gate, TimeSpan.FromMilliseconds(MachineConstants.PauseWaitMilliseconds));
                if (cancellationToken.IsCancellationRequested) break;
                while (_commands.TryDequeue(out var command)) command.Execute();
                lock (_gate)
                    if (_pauseRequested) continue;
                _core.SetInput(_input.Consume());
                _core.RunFrame();
                PublishOutputs(ref videoSequence);
                nextFrame = MachineFunctions.NextFrameTimestamp(nextFrame, _core.FramesPerSecond);
                frameTimer.WaitUntil(nextFrame, cancellationToken);
                if (nextFrame < Stopwatch.GetTimestamp()) nextFrame = Stopwatch.GetTimestamp();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _started?.TrySetCanceled(cancellationToken);
        }
        catch (Exception error)
        {
            ReportFailure(error);
        }
        finally
        {
            var released = false;
            try
            {
                try
                {
                    if (initialized) MachineFunctions.TryReleaseInput(_core);
                    if (initialized) _core.Stop();
                }
                catch (Exception error) { ReportFailure(error); }
                finally
                {
                    try { _core.Dispose(); released = true; }
                    catch (Exception error) { ReportFailure(error); }
                }
            }
            finally
            {
                try { _audio.Stop(); }
                finally
                {
                    if (released) MachineFunctions.DeleteSessionDirectory(_sessionDirectory);
                    lock (_gate)
                    {
                        FailPendingCommands(new OperationCanceledException(ErrorMessages.MachineStopped));
                        if (State != EmulationMachineState.Faulted) State = EmulationMachineState.Stopped;
                    }
                }
            }
        }
    }

    private void ReportFailure(Exception error)
    {
        error = MessageFunctions.Translate(error, Configuration);
        if (_core.Diagnostics.Count > MachineConstants.EmptyCount)
            error.Data[MachineConstants.DiagnosticDataKey] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(MachineConstants.DiagnosticTailCount));
        _started?.TrySetException(error);
        lock (_gate)
        {
            State = EmulationMachineState.Faulted;
            FailPendingCommands(error);
        }
    }

    private void StartAudio()
    {
        _audio.Start(_core.SampleRate);
    }

    private void PublishOutputs(ref long videoSequence)
    {
        if (_core.LatestVideoFrame is { } video && video.Sequence != videoSequence)
        {
            videoSequence = video.Sequence;
            VideoFrameReady?.Invoke(this, video);
        }
        while (_core.TryDequeueAudio(out var audio) && audio is not null)
        {
            _audio.Write(audio);
            AudioChunkReady?.Invoke(this, audio);
        }
    }

    private void FailPendingCommands(Exception error)
    {
        while (_commands.TryDequeue(out var command)) command.Completion.TrySetException(error);
    }

    private void ResetCore()
    {
        _audio.Reset();
        _core.HardReset();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        var disposeUnstartedCore = _thread is null;
        try { await StopAsync().ConfigureAwait(false); }
        finally
        {
            try
            {
                try { if (disposeUnstartedCore) _core.Dispose(); }
                finally { _audio.Dispose(); }
            }
            finally
            {
                _stopSource?.Dispose();
                _disposed = true;
                if (disposeUnstartedCore) MachineFunctions.DeleteSessionDirectory(_sessionDirectory);
            }
        }
    }
}
