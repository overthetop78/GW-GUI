using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime,
    IEmulationCassetteTransport
{
private MachineConfiguration CurrentConfiguration() =>
        Configuration with { Media = _mountedMedia.ToArray() };

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
            throw new InvalidOperationException(MachineConstants.InvalidStateMessage);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _commands.Enqueue(new MachineCommand(action, completion));
        lock (_gate) Monitor.PulseAll(_gate);
        return new ValueTask(completion.Task.WaitAsync(cancellationToken));
    }

    private ValueTask QueueCommand<T>(Action<T> action, T value, CancellationToken cancellationToken) =>
        QueueCommand(() => action(value), cancellationToken);

    private void Run(CancellationToken cancellationToken)
    {
        var initialized = false;
        try
        {
            _core.Initialize(Configuration, _sessionDirectory, _saveDirectory);
            initialized = true;
            StartAudio();
            lock (_gate)
            {
                State = EmulationMachineState.Running;
                _started?.TrySetResult();
            }
            var nextFrame = Stopwatch.GetTimestamp();
            using var frameTimer = new FrameTimer(cancellationToken);
            long videoSequence = default;
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (_gate)
                    while (_pauseRequested && _commands.IsEmpty && !cancellationToken.IsCancellationRequested)
                        Monitor.Wait(_gate, TimeSpan.FromMilliseconds(MachineConstants.PauseWaitMilliseconds));
                if (cancellationToken.IsCancellationRequested) break;
                while (_commands.TryDequeue(out var command)) command.Execute();
                lock (_gate)
                    if (_pauseRequested) continue;
                _core.SetInput(_cassetteInput.NextFrame());
                _core.RunFrame();
                PublishOutputs(ref videoSequence);
                nextFrame = MachineFunctions.NextFrameTimestamp(nextFrame, _core.FramesPerSecond);
                frameTimer.WaitUntil(nextFrame, cancellationToken);
                if (nextFrame < Stopwatch.GetTimestamp()) nextFrame = Stopwatch.GetTimestamp();
            }
        }
        catch (Exception error)
        {
            error = MessageFunctions.Translate(error, Configuration);
            if (_core.Diagnostics.Count > MachineConstants.EmptyCount)
                error.Data[MachineConstants.DiagnosticDataKey] = string.Join(Environment.NewLine,
                    _core.Diagnostics.TakeLast(MachineConstants.DiagnosticTailCount));
            _started?.TrySetException(error);
            FailPendingCommands(error);
            lock (_gate) State = EmulationMachineState.Faulted;
        }
        finally
        {
            if (initialized) MachineFunctions.TryReleaseInput(_core);
            if (initialized)
                try { _core.Stop(); } catch (Exception) { }
            try { _core.Dispose(); } catch (Exception) { }
            try { _audio.Stop(); } catch (Exception) { }
            MachineFunctions.DeleteSessionDirectory(_sessionDirectory);
            FailPendingCommands(new OperationCanceledException(MachineConstants.StoppedMessage));
            lock (_gate)
                if (State != EmulationMachineState.Faulted) State = EmulationMachineState.Stopped;
        }
    }

    private void StartAudio()
    {
        _audio.Start(_core.SampleRate);
    }

    private void PublishOutputs(ref long videoSequence)
    {
        if (_core.LatestVideoFrame is { } video && video.Sequence != videoSequence)
        {
            videoSequence = video.Sequence;
            VideoFrameReady?.Invoke(this, video);
        }
        while (_core.TryDequeueAudio(out var audio) && audio is not null)
        {
            _audio.Write(audio);
            AudioChunkReady?.Invoke(this, audio);
        }
    }

    private void FailPendingCommands(Exception error)
    {
        while (_commands.TryDequeue(out var command)) command.Completion.TrySetException(error);
    }

    private void ResetCore(string resetType)
    {
        _audio.Reset();
        if (Configuration.Core == Emulator.Hatari)
            _core.SetOption(MachineValues.HatariResetType, resetType);
        _core.HardReset();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        var disposeUnstartedCore = State == EmulationMachineState.Created;
        await StopAsync().ConfigureAwait(false);
        _stopSource?.Dispose();
        if (disposeUnstartedCore) _core.Dispose();
        _audio.Dispose();
        MachineFunctions.DeleteSessionDirectory(_sessionDirectory);
        _disposed = true;
    }
}
