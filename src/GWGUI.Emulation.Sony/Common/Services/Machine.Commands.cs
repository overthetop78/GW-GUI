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

    private void SetControllerPortDevice(int port, ControllerType type)
    {
        ControllerFunctions.Validate(Configuration, port, type);
        QueueCommand(() =>
        {
            var updated = ControllerFunctions.WithControllerType(Configuration, port, type);
            _core.SetControllerPortDevice(port, type);
            Configuration = updated;
        }, CancellationToken.None).AsTask().GetAwaiter().GetResult();
    }
    private void Run(CancellationToken cancellationToken)
    {
        var initialized = false;
        try
        {
            _core.Initialize(Configuration, _sessionDirectory, _saveDirectory);
            initialized = true;
            if (_audioOutput is not null)
            {
                try { StartAudioOutput(_core.SampleRate, EmulationAudioChannelLayout.Stereo); }
                catch { _audioOutput.Dispose(); _audioOutput = null; }
            }
            lock (_gate)
            {
                State = EmulationMachineState.Running;
                _started?.TrySetResult();
            }
            var nextFrame = TimeProvider.System.GetTimestamp();
            long videoSequence = 0;

            EmulationFrameTimer.RunLoop(_gate, _commands, () => _pauseRequested,
                () => {
                    _core.RunFrame();
                    if (_core.ShutdownRequested)
                        return false;
                    if (_core.LatestVideoFrame is { } video && video.Sequence != videoSequence)
                    {
                        videoSequence = video.Sequence;
                        VideoFrameReady?.Invoke(this, video);
                    }

                    while (_core.TryDequeueAudio(out var audio) && audio is not null)
                    {
                        if (_audioOutput is not null && !_audioMuted)
                        {
                            try
                            {
                                if (audio.SampleRate != _audioSampleRate || audio.ChannelLayout != _audioChannelLayout)
                                {
                                    _audioOutput.Stop();
                                    StartAudioOutput(audio.SampleRate, audio.ChannelLayout);
                                }

                                WriteAudio(audio.InterleavedSamples.Span);
                            }
                            catch
                            {
                                _audioOutput.Dispose();
                                _audioOutput = null;
                            }
                        }

                        AudioChunkReady?.Invoke(this, audio);
                    }

                    nextFrame = EmulationFrameTimer.WaitForNextFrame(nextFrame, _core.FramesPerSecond, TimeProvider.System, Thread.Sleep);
                    return true;
                },
                () => !cancellationToken.IsCancellationRequested && !_core.ShutdownRequested, cancellationToken, 100);
        }
        catch (Exception error)
        {
            error = _startErrorTranslator?.Invoke(error) ?? EmulationErrorService.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, new EmulationMachineMessageContext(Configuration.Model));
            if (_core.Diagnostics.Count > 0) error.Data[MachineConstants.Diagnostics] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(100));
            _started?.TrySetException(error);
            FailPendingCommands(error);
            lock (_gate) State = EmulationMachineState.Faulted;
        }
        finally
        {
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
        }
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
    => EmulationAudioFunctions.WriteVolume(ref _audioOutput, ref _audioVolume, samples);

    private void ReplaceAudioOutput(Func<IAudioOutput?>? factory)
    {
        lock (_gate)
        {
            try { _audioOutput?.Stop(); }
            finally { _audioOutput?.Dispose(); }
            _audioOutput = factory?.Invoke();
            if (_audioOutput is not null && State is EmulationMachineState.Running or EmulationMachineState.Paused)
                StartAudioOutput(_core.SampleRate, _audioChannelLayout);
        }
    }

    private void StartAudioOutput(int sampleRate, EmulationAudioChannelLayout layout)
    {
        if (_audioOutput is IMultichannelAudioOutput multichannel) multichannel.Start(sampleRate, layout);
        else if (layout == EmulationAudioChannelLayout.Stereo) _audioOutput?.Start(sampleRate);
        else throw new NotSupportedException(nameof(IMultichannelAudioOutput));
        _audioSampleRate = sampleRate;
        _audioChannelLayout = layout;
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try { await StopAsync().ConfigureAwait(false); }
        finally
        {
            try { _stop?.Dispose(); }
            finally
            {
                _stop = null;
                try { _core.Dispose(); }
                finally
                {
                    try { _audioOutput?.Dispose(); }
                    finally
                    {
                        _audioOutput = null;
                        DeleteSessionDirectory();
                    }
                }
            }
        }
    }

    private void DeleteSessionDirectory()
    {
        if (_deleteSession is not null) { _deleteSession(_sessionDirectory); return; }
        EmulationSessionDirectoryFunctions.DeleteSessionDirectory(_sessionDirectory);
    }
}
