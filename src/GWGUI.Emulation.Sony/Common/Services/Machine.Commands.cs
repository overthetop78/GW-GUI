using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using AudioFunctions = GWGUI.Emulation.Sony.Common.Functions.AudioFunctions;
using CommonControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;
using System.Collections.Concurrent;
using System.IO;

namespace GWGUI.Emulation.Sony.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken, EmulationMessageCategory category = EmulationMessageCategory.Machine, EmulationMessageCode code = EmulationMessageCode.MachineStartFailed)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
            throw MachineExceptions.MachineNotRunning();
        return CommonCoreHostFunctions.MachineExecution.EnqueueCommand(_commands, _gate, () =>
        {
            try
            {
                action();
            }
            catch (Exception error)
            {
                throw CommonCoreHostFunctions.Messages.Translate(error, category, code, new EmulationMachineMessageContext(Configuration.Model));
            }
        }, cancellationToken);
    }

    private void SetControllerPortDevice(int port, PeripheralCategory type)
    {
        CommonControllerFunctions.Validate(Configuration, port, type);
        QueueCommand(() =>
        {
            var updated = CommonControllerFunctions.WithControllerType(Configuration, port, type);
            _core.SetControllerPortDevice(port, type);
            Configuration = updated;
        }, CancellationToken.None).AsTask().GetAwaiter().GetResult();
    }
    private void Run(CancellationToken cancellationToken)
    {
        CommonCoreHostFunctions.MachineExecution.RunCore(
            () => _core.Initialize(Configuration, _sessionDirectory, _saveDirectory),
            () =>
            {
                _audio.Start(_core.SampleRate);
                lock (_gate)
                {
                    State = EmulationMachineState.Running;
                    _started?.TrySetResult();
                }
                var nextFrame = TimeProvider.System.GetTimestamp();
                long videoSequence = 0;

                Action<Machine, AudioChunk> publishAudio = static (machine, audio) =>
                {
                    machine._audio.Write(audio);
                    machine.AudioChunkReady?.Invoke(machine, audio);
                };

                CommonCoreHostFunctions.MachineExecution.RunLoop(_gate, _commands, () => _pauseRequested,
                    () => {
                        if (_core is IEmulatorAudioBufferFeedback feedback)
                            feedback.SetAudioBufferStatus(_audio.BufferStatus);
                        _core.RunFrame();
                        if (_core is IEmulatorAudioBufferFeedback latency)
                            _audio.TrySetMinimumLatency(latency.MinimumAudioLatencyMilliseconds);
                        if (_core.ShutdownRequested)
                            return false;
                        CommonCoreHostFunctions.MachineExecution.PublishOutputs(this, ref videoSequence,
                            static machine => machine._core.LatestVideoFrame,
                            static machine => machine._core.TryDequeueAudio(out var audio) ? audio : null,
                            static (machine, video) => machine.VideoFrameReady?.Invoke(machine, video),
                            publishAudio);

                        nextFrame = CommonCoreHostFunctions.MachineExecution.WaitForNextFrame(nextFrame, _core.FramesPerSecond, TimeProvider.System, Thread.Sleep);
                        return true;
                    },
                    () => !cancellationToken.IsCancellationRequested && !_core.ShutdownRequested, cancellationToken, 100);
            },
            error => () =>
            {
                error = _startErrorTranslator?.Invoke(error) ?? CommonCoreHostFunctions.Messages.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, new EmulationMachineMessageContext(Configuration.Model));
                if (_core.Diagnostics.Count > 0) error.Data[MachineConstants.Diagnostics] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(100));
                _started?.TrySetException(error);
                FailPendingCommands(error);
                lock (_gate) State = EmulationMachineState.Faulted;
            },
            initialized => {
                if (initialized)
                {
                    try { _core.Stop(); }
                    catch (Exception) { }
                }
                try { _audio.Stop(); }
                catch (Exception) { }
                FailPendingCommands(new OperationCanceledException());
                lock (_gate)
                    if (State != EmulationMachineState.Faulted) State = EmulationMachineState.Stopped;
            });
    }
    private void FailPendingCommands(Exception error)
        => CommonCoreHostFunctions.MachineExecution.FailPendingCommands(_commands, error);

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            await CommonCoreHostFunctions.MachineExecution.DisposeMachineAsync(
                () => StopAsync(),
                () => { try { _stop?.Dispose(); } finally { _stop = null; } },
                () => _core.Dispose(),
                () => _audio.Dispose(),
                DeleteSessionDirectory).ConfigureAwait(false);
        }
        finally { _disposed = true; }
    }

    private void DeleteSessionDirectory()
    {
        if (_deleteSession is not null) { _deleteSession(_sessionDirectory); return; }
        EmulationSdk.SessionDirectories.DeleteSessionDirectory(_sessionDirectory);
    }
}
