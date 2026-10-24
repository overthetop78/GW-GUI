using CommonControllerFunctions = GWGUI.Emulation.Commodore.Common.Functions.ControllerFunctions;
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
    /// <summary>Exécute le changement de périphérique sur le thread propriétaire du cœur.</summary>
    /// <param name="port">Indice du port déjà converti depuis le contrat public.</param>
    /// <param name="type">Type interne accepté par le modèle et son adaptation.</param>
    /// <remarks>La configuration est publiée uniquement après la réussite du cœur. Une erreur conserve
    /// les anciennes associations et suit la traduction des erreurs déjà utilisée par les commandes.</remarks>
    private void SetControllerPortDevice(int port, PeripheralCategory type)
    {
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
                StartAudio();
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
                    () => !cancellationToken.IsCancellationRequested && !_core.ShutdownRequested, cancellationToken, MachineConstants.PausedWorkerPollMilliseconds);
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
        => CommonCoreHostFunctions.MachineExecution.FailPendingCommands(_commands, error);

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

    private void StartAudio()
    {
        _audio.TrySetMinimumLatency(_core.MinimumAudioLatencyMilliseconds);
        _audio.Start(_core.SampleRate);
    }
}
