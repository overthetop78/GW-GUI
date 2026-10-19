using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Mattel.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    private MachineConfiguration CurrentConfiguration() => Configuration with { Media = _mountedMedia.ToArray(), Input = _inputConfiguration };

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return EmulationMachineCommand.Enqueue(_commands, _gate, action, cancellationToken, validateInsideGate: () =>
        {
            if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
                throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
        });
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
            EmulationFrameTimer.RunLoop(nextFrame, _gate, _commands, () => _pauseRequested,
                () => {
                    _core.SetInput(_input.Consume());
                    _core.RunFrame();
                    PublishOutputs(ref videoSequence);
                },
                () => _core.FramesPerSecond, MachineFunctions.NextFrameTimestamp, frameTimer.WaitUntil,
                cancellationToken, MachineConstants.PauseWaitMilliseconds);
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
                    if (initialized && !_mediaSuspended) MachineFunctions.TryReleaseInput(_core);
                    if (initialized && !_mediaSuspended) _core.Stop();
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
        => EmulationFrameOutputFunctions.PublishOutputs(this, ref videoSequence,
            static machine => machine._core.LatestVideoFrame,
            static machine => machine._core.TryDequeueAudio(out var audio) ? audio : null,
            static (machine, video) => machine.VideoFrameReady?.Invoke(machine, video),
            static (machine, audio) =>
            {
                machine._audio.Write(audio);
                machine.AudioChunkReady?.Invoke(machine, audio);
            });

    private void FailPendingCommands(Exception error)
        => EmulationMachineCommand.FailPending(_commands, error);

    private void ResetCore()
    {
        if (_mediaSuspended) return;
        _audio.Reset();
        _core.HardReset();
    }

    private bool NeedsMediaRestart(EmulationMediaSlot slot, string? path = null) => _requiresMediaRestart(slot, path);

    private void SuspendForMedia()
    {
        _resumeAfterMediaInsert = !_pauseRequested;
        _audio.Stop();
        try { _core.Stop(); }
        finally { _core.Dispose(); }
        _mediaSuspended = true;
        lock (_gate)
        {
            _pauseRequested = true;
            State = EmulationMachineState.Paused;
        }
    }

    private void RestartCore(MachineConfiguration configuration)
    {
        ConfigurationFunctions.Validate(configuration);
        var factory = _coreFactory ?? throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
        _audio.Stop();
        var resumeAfterInsert = _mediaSuspended && _resumeAfterMediaInsert;
        try { if (!_mediaSuspended) _core.Stop(); }
        finally { _core.Dispose(); }
        var replacement = factory();
        _core = replacement;
        var completed = false;
        try
        {
            replacement.Initialize(configuration, _sessionDirectory, _saveDirectory);
            replacement.SetInput(_pauseRequested ? EmulationInputSnapshot.Empty : _input.Consume());
            StartAudio();
            if (_pauseRequested) _audio.Pause();
            _mediaSuspended = false;
            if (resumeAfterInsert)
            {
                lock (_gate)
                {
                    _pauseRequested = false;
                    State = EmulationMachineState.Running;
                }
                _audio.Resume();
            }
            _resumeAfterMediaInsert = false;
            completed = true;
        }
        finally { if (!completed) replacement.Dispose(); }
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
