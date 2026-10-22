using CommonCoreHostFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreHostFunctions;
using AudioFunctions = GWGUI.Emulation.Commodore.Common.Functions.AudioFunctions;
using System.IO;
using GWGUI.Emulation;
using System.Collections.Concurrent;

namespace GWGUI.Emulation.Commodore.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    private IReadOnlyDictionary<string, string> FirmwareHashes() => (Configuration.FirmwarePaths ?? new Dictionary<string, string>()).ToDictionary(pair => pair.Key, pair => StateStore.HashFile(pair.Value), StringComparer.Ordinal);

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken, EmulationMessageCategory category = EmulationMessageCategory.Machine, EmulationMessageCode code = EmulationMessageCode.MachineStartFailed)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
            throw MachineExceptions.MachineNotRunning();
        return EmulationMachineCommand.Enqueue(_commands, _gate, () =>
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
                _audio.Start(_core.SampleRate);
                lock (_gate)
                {
                    State = EmulationMachineState.Running;
                    _started?.TrySetResult();
                }
                var nextFrame = TimeProvider.System.GetTimestamp();
                long videoSequence = MachineConstants.InitialVideoSequence;

                Action<Machine, AudioChunk> publishAudio = static (machine, audio) =>
                {
                    machine._audio.Write(audio);
                    machine.AudioChunkReady?.Invoke(machine, audio);
                };

                EmulationFrameTimer.RunLoop(_gate, _commands, () => _pauseRequested,
                    () => {
                        _core.RunFrame();
                        EmulationFrameOutputFunctions.PublishOutputs(this, ref videoSequence,
                            static machine => machine._core.LatestVideoFrame,
                            static machine => machine._core.TryDequeueAudio(out var audio) ? audio : null,
                            static (machine, video) => machine.VideoFrameReady?.Invoke(machine, video),
                            publishAudio);

                        nextFrame = EmulationFrameTimer.WaitForNextFrame(nextFrame, _core.FramesPerSecond, TimeProvider.System, Thread.Sleep);
                        return true;
                    },
                    () => !cancellationToken.IsCancellationRequested, cancellationToken, MachineConstants.PausedWorkerPollMilliseconds);
            },
            error => () =>
            {
                error = _startErrorTranslator?.Invoke(error) ?? CommonCoreHostFunctions.Messages.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, new EmulationMachineMessageContext(Configuration.Model));
                if (_core.Diagnostics.Count > BufferConstants.EmptyCollectionCount) error.Data[MachineConstants.DiagnosticsDataKey] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(MachineConstants.MaximumDiagnosticEntries));
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
        => EmulationMachineCommand.FailPending(_commands, error);

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        try
        {
            await CommonCoreHostFunctions.MachineExecution.DisposeMachineAsync(
                () => StopAsync(),
                () => _core.Dispose(),
                () => _audio.Dispose(),
                () => _stop?.Dispose(),
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
