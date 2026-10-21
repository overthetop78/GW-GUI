using GWGUI.Emulation;
using System.Collections.Concurrent;
using System.IO;

namespace GWGUI.Emulation.Amstrad.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{

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
                throw EmulationErrorService.Translate(error, category, code, new EmulationMachineMessageContext(Configuration.Model));
            }
        }, cancellationToken);
    }
    private void Run(CancellationToken cancellationToken)
    {
        GWGUI.Emulation.Functions.ExternalCoreLifecycleFunctions.RunCore(
            () => _core.Initialize(Configuration, _sessionDirectory, _saveDirectory),
            () =>
            {
                var audioSampleRate = AudioConstants.UninitializedSampleRate;
                if (_audioOutput is not null)
                {
                    try { _audioOutput.Start(_core.SampleRate); audioSampleRate = _core.SampleRate; }
                    catch { _audioOutput.Dispose(); _audioOutput = null; }
                }
                lock (_gate)
                {
                    State = EmulationMachineState.Running;
                    _started?.TrySetResult();
                }
                var nextFrame = TimeProvider.System.GetTimestamp();
                long videoSequence = VideoConstants.InitialFrameSequence;

                Action<Machine, AudioChunk> publishAudio = (machine, audio) =>
                {
                    if (machine._audioOutput is not null && !machine._audioMuted)
                    {
                        try
                        {
                            if (audio.SampleRate != audioSampleRate)
                            {
                                machine._audioOutput.Stop();
                                machine._audioOutput.Start(audio.SampleRate);
                                audioSampleRate = audio.SampleRate;
                            }

                            machine.WriteAudio(audio.InterleavedSamples.Span);
                        }
                        catch
                        {
                            machine._audioOutput.Dispose();
                            machine._audioOutput = null;
                        }
                    }

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
                    () => !cancellationToken.IsCancellationRequested, cancellationToken, MachineConstants.CommandPollingIntervalMilliseconds);
            },
            error => () =>
            {
                error = _startErrorTranslator?.Invoke(error) ?? EmulationErrorService.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, new EmulationMachineMessageContext(Configuration.Model));
                if (_core.Diagnostics.Count > BufferConstants.EmptyCollectionCount) error.Data[MachineConstants.Diagnostics] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(MachineConstants.MaximumFailureDiagnosticCount));
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
                try { _audioOutput?.Stop(); }
                catch (Exception) { }
                FailPendingCommands(new OperationCanceledException());
                lock (_gate)
                    if (State != EmulationMachineState.Faulted) State = EmulationMachineState.Stopped;
            });
    }
    private void FailPendingCommands(Exception error)
        => EmulationMachineCommand.FailPending(_commands, error);

    private void FlushAudio()
    {
        if (_audioOutput is null) return;
        try { _audioOutput.Flush(); }
        catch { _audioOutput.Dispose(); _audioOutput = null; }
    }

    private void WriteAudio(ReadOnlySpan<short> samples)
    => EmulationSdk.AudioProcessing.WriteVolume(ref _audioOutput, ref _audioVolume, samples);

    private void ReplaceAudioOutput(Func<IAudioOutput?>? factory)
    {
        lock (_gate)
        {
            try { _audioOutput?.Stop(); }
            finally { _audioOutput?.Dispose(); }
            _audioOutput = factory?.Invoke();
            if (_audioOutput is not null && State is EmulationMachineState.Running or EmulationMachineState.Paused)
                _audioOutput.Start(_core.SampleRate);
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            await GWGUI.Emulation.Functions.ExternalCoreLifecycleFunctions.DisposeMachineAsync(
                () => StopAsync(),
                () => { try { _stop?.Dispose(); } finally { _stop = null; } },
                () => _core.Dispose(),
                () => { try { _audioOutput?.Dispose(); } finally { _audioOutput = null; } },
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
