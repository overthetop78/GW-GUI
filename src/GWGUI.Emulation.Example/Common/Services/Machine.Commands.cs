using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return EmulationMachineCommand.Enqueue(_commands, _gate, action, cancellationToken, validateInsideGate: () =>
        {
            if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
                throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
        });
    }

    private void Run(CancellationToken cancellationToken)
    {
        GWGUI.Emulation.Functions.ExternalCoreLifecycleFunctions.RunCore(
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
                        released = ExternalCoreLifecycleFunctions.TryDisposeCore(_core, ReportFailure);
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
        _audio.Reset();
        _core.HardReset();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        var disposeUnstartedCore = _thread is null;
        try
        {
            await GWGUI.Emulation.Functions.ExternalCoreLifecycleFunctions.DisposeMachineAsync(
                () => StopAsync(),
                () => { if (disposeUnstartedCore) _core.Dispose(); },
                () => _audio.Dispose(),
                () => _stopSource?.Dispose(),
                () => { _disposed = true; if (disposeUnstartedCore) MachineFunctions.DeleteSessionDirectory(_sessionDirectory); }).ConfigureAwait(false);
        }
        finally { _disposed = true; }
    }
}
