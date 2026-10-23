using CommonCoreHostFunctions = GWGUI.Emulation.Thomson.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Thomson.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput, IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    private MachineConfiguration CurrentConfiguration() => Configuration with { Media = _mountedMedia.ToArray(), Input = _inputConfiguration.Input };

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return CommonCoreHostFunctions.MachineExecution.EnqueueCommand(_commands, _gate, action, cancellationToken, validateInsideGate: () =>
        {
            if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
                throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
        });
    }

    private void Run(CancellationToken cancellationToken)
    {
        CommonCoreHostFunctions.MachineExecution.RunCore(
            () => _core.Initialize(Configuration, _sessionDirectory, _saveDirectory),
            () =>
            {
                StartAudio();
                lock (_gate)
                {
                    State = EmulationMachineState.Running;
                    _started?.TrySetResult();
                }
                var nextFrame = Stopwatch.GetTimestamp();
                using var frameTimer = CommonCoreHostFunctions.MachineExecution.CreateFrameTimer(cancellationToken);
                long videoSequence = default;
                CommonCoreHostFunctions.MachineExecution.RunLoop(nextFrame, _gate, _commands, () => _pauseRequested,
                    () => {
                        _core.SetInput(_input.Consume());
                        _core.SetAudioBufferStatus(_audio.BufferStatus);
                        _core.RunFrame();
                        if (_core.ShutdownRequested) return;
                        _audio.TrySetMinimumLatency(_core.MinimumAudioLatencyMilliseconds);
                        PublishOutputs(ref videoSequence);
                    },
                    () => _core.FramesPerSecond, MachineFunctions.NextFrameTimestamp, frameTimer.WaitUntil,
                    cancellationToken, MachineConstants.PauseWaitMilliseconds, () => !_core.ShutdownRequested);
            },
            error =>
            {
                if (error is OperationCanceledException && cancellationToken.IsCancellationRequested)
                    return () =>
                    {
                        _started?.TrySetCanceled(cancellationToken);
                    };
                return () =>
                    {
                        ReportFailure(error);
                    };
            },
            initialized => {
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
                        released = CommonCoreHostFunctions.MachineExecution.TryDisposeCore(_core, ReportFailure);
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
            });
    }

    private void ReportFailure(Exception error)
    {
        error = MessageFunctions.Translate(error, Configuration);
        if (_core.Diagnostics.Count > MachineConstants.EmptyCount) error.Data[MachineConstants.DiagnosticDataKey] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(MachineConstants.DiagnosticTailCount));
        _started?.TrySetException(error);
        lock (_gate)
        {
            State = EmulationMachineState.Faulted;
            FailPendingCommands(error);
        }
    }

    private void StartAudio()
    {
        _audio.TrySetMinimumLatency(_core.MinimumAudioLatencyMilliseconds);
        _audio.Start(_core.SampleRate);
    }

    private void PublishOutputs(ref long videoSequence)
        => CommonCoreHostFunctions.MachineExecution.PublishOutputs(this, ref videoSequence,
            static machine => machine._core.LatestVideoFrame,
            static machine => machine._core.TryDequeueAudio(out var audio) ? audio : null,
            static (machine, video) => machine.VideoFrameReady?.Invoke(machine, video),
            static (machine, audio) =>
            {
                machine._audio.Write(audio);
                machine.AudioChunkReady?.Invoke(machine, audio);
            });

    private void FailPendingCommands(Exception error)
        => CommonCoreHostFunctions.MachineExecution.FailPendingCommands(_commands, error);

    private void ResetCore(bool hardReset = true)
    {
        if (_mediaSuspended) return;
        _audio.Reset();
        if (_requiresResetRestart) RestartCore(CurrentConfiguration()); else if (hardReset) _core.HardReset(); else CommonCoreHostFunctions.MachineExecution.SoftReset(_runtimePolicy?.SoftResetKeys ?? new HashSet<EmulationKey>(), _core.SetInput, _core.RunFrame, _core.SoftReset);
    }

    private bool NeedsMediaRestart(EmulationMediaSlot slot, string? path = null) =>
        _requiresMediaRestart?.Invoke(slot, path) ?? _runtimePolicy?.RequiresMediaRestart(Configuration, slot, path) ?? false;

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
        try
        {
            await CommonCoreHostFunctions.MachineExecution.DisposeMachineAsync(
                () => StopAsync(),
                () => { if (disposeUnstartedCore) _core.Dispose(); },
                () => _audio.Dispose(),
                () => _stopSource?.Dispose(),
                () => { _disposed = true; if (disposeUnstartedCore) MachineFunctions.DeleteSessionDirectory(_sessionDirectory); }).ConfigureAwait(false);
        }
        finally { _disposed = true; }
    }
}
