using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Acorn.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{

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
                        if (_core is IEmulatorAudioBufferFeedback feedback)
                            feedback.SetAudioBufferStatus(_audio.BufferStatus);
                        _core.RunFrame();
                        if (_core.ShutdownRequested) return;
                        if (_core is IEmulatorAudioBufferFeedback latency)
                            _audio.TrySetMinimumLatency(latency.MinimumAudioLatencyMilliseconds);
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
                        if (initialized) MachineFunctions.TryReleaseInput(_core);
                        if (initialized) _core.Stop();
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
        if (_core is IEmulatorAudioBufferFeedback latency)
            _audio.TrySetMinimumLatency(latency.MinimumAudioLatencyMilliseconds);
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

    private void ResetCore()
    {
        _audio.Reset();
        _core.HardReset();
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
