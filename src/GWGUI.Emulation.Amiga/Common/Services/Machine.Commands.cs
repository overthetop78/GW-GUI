using GWGUI.Emulation;
using System.Collections.Concurrent;

namespace GWGUI.Emulation.Amiga.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
private static string? HashOptionalFile(string? path) => path is null ? null : StateStore.HashFile(path);
    private static string? HashOptionalPath(string? path) => path is null ? null : StateStore.HashPath(path);

    private static bool OptionsEqual(IReadOnlyDictionary<string, string>? left, IReadOnlyDictionary<string, string> right)
    {
        if ((left?.Count ?? 0) != right.Count) return false;
        return left is null ? right.Count == 0 : left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);
    }

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
            throw new InvalidOperationException(MachineConstants.TheAmigaMachineMustBeRunningBeforeChangingAFloppy);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _commands.Enqueue(new PendingCommand(action, completion));
        lock (_gate) Monitor.PulseAll(_gate);
        return new ValueTask(completion.Task.WaitAsync(cancellationToken));
    }

    private void Run(CancellationToken cancellationToken)
    {
        var initialized = false;
        try
        {
            _core.Initialize(Configuration, _sessionDirectory, _saveDirectory);
            initialized = true;
            var audioSampleRate = 0;
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
            long videoSequence = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                lock (_gate)
                    while (_pauseRequested && _commands.IsEmpty && !cancellationToken.IsCancellationRequested)
                        Monitor.Wait(_gate, TimeSpan.FromMilliseconds(100));
                if (cancellationToken.IsCancellationRequested) break;

                while (_commands.TryDequeue(out var command)) command.Execute();

                lock (_gate)
                    if (_pauseRequested) continue;

                _core.RunFrame();
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
                            if (audio.SampleRate != audioSampleRate)
                            {
                                _audioOutput.Stop();
                                _audioOutput.Start(audio.SampleRate);
                                audioSampleRate = audio.SampleRate;
                            }
                            WriteAudio(audio.InterleavedStereo.Span);
                        }
                        catch { _audioOutput.Dispose(); _audioOutput = null; }
                    }
                    AudioChunkReady?.Invoke(this, audio);
                }

                var frameDuration = TimeSpan.FromSeconds(1 / Math.Clamp(_core.FramesPerSecond, 1, 1000));
                nextFrame += (long)(frameDuration.TotalSeconds * TimeProvider.System.TimestampFrequency);
                var remaining = TimeProvider.System.GetElapsedTime(TimeProvider.System.GetTimestamp(), nextFrame);
                if (remaining > TimeSpan.Zero) Thread.Sleep(remaining);
                else nextFrame = TimeProvider.System.GetTimestamp();
            }
        }
        catch (Exception error)
        {
            if (_core.Diagnostics.Count > 0) error.Data[MachineConstants.AmigaDiagnostics] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(100));
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
            FailPendingCommands(new OperationCanceledException(MachineConstants.TheAmigaMachineStopped));
            lock (_gate)
                if (State != EmulationMachineState.Faulted) State = EmulationMachineState.Stopped;
        }
    }

    private void FailPendingCommands(Exception error)
    {
        while (_commands.TryDequeue(out var command)) command.Completion.TrySetException(error);
    }

    private void FlushAudio()
    {
        if (_audioOutput is null) return;
        try { _audioOutput.Flush(); }
        catch { _audioOutput.Dispose(); _audioOutput = null; }
    }

    private void WriteAudio(ReadOnlySpan<short> samples)
    {
        if (_audioOutput is null) return;
        if (_audioVolume >= 1f)
        {
            _audioOutput.Write(samples);
            return;
        }

        var scaled = new short[samples.Length];
        for (var index = 0; index < samples.Length; index++)
            scaled[index] = (short)Math.Clamp(samples[index] * _audioVolume, short.MinValue, short.MaxValue);
        _audioOutput.Write(scaled);
    }

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
        await StopAsync().ConfigureAwait(false);
        _stop?.Dispose();
        _core.Dispose();
        _audioOutput?.Dispose();
        DeleteSessionDirectory();
        _disposed = true;
    }

    private void DeleteSessionDirectory()
    {
        if (_deleteSession is not null) { _deleteSession(_sessionDirectory); return; }
        try
        {
            var path = Path.GetFullPath(_sessionDirectory);
            if (Directory.Exists(path) && !string.IsNullOrWhiteSpace(Path.GetFileName(path))) Directory.Delete(path, true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private sealed record PendingCommand(Action Action, TaskCompletionSource Completion)
    {
        public void Execute()
        {
            try { Action(); Completion.TrySetResult(); }
            catch (Exception error) { Completion.TrySetException(error); }
        }
    }
}
