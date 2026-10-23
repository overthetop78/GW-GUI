using CommonCoreHostFunctions = GWGUI.Emulation.Sega.Common.Functions.CoreHostFunctions;
using AudioFunctions = GWGUI.Emulation.Sega.Common.Functions.AudioFunctions;
using GWGUI.Emulation;
using System.Collections.Concurrent;
using System.IO;

namespace GWGUI.Emulation.Sega.Common.Services;

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
                var nextFrame = TimeProvider.System.GetTimestamp();
                long videoSequence = 0;

                Action<Machine, AudioChunk> publishAudio = static (machine, audio) =>
                {
                    machine._audio.Write(audio);
                    machine.AudioChunkReady?.Invoke(machine, audio);
                };

                CommonCoreHostFunctions.MachineExecution.RunLoop(_gate, _commands, () => _pauseRequested,
                    () => {
                        _core.SetAudioBufferStatus(_audio.BufferStatus);
                        _core.RunFrame();
                        if (_core.ShutdownRequested) return false;
                        _audio.TrySetMinimumLatency(_core.MinimumAudioLatencyMilliseconds);
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
                if (initialized) TryReleaseInput();
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

    private void TryReleaseInput()
    {
        _lastPhysicalInput = EmulationInputSnapshot.Empty;
        _controllerPointerSwitchPressed = false;
        _controllerPointerMode = false;
        try { _core.SetInput(EmulationInputSnapshot.Empty); }
        catch (Exception) { }
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

    private void StartAudio()
    {
        _audio.TrySetMinimumLatency(_core.MinimumAudioLatencyMilliseconds);
        _audio.Start(_core.SampleRate);
    }
}
