using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;
using System.Collections.Concurrent;

namespace GWGUI.Emulation.Amiga;

internal sealed class AmigaMachine : IAmigaMachine
{
    private readonly object _gate = new();
    private readonly IAmigaCore _core;
    private readonly string _sessionDirectory;
    private IAudioOutput? _audioOutput;
    private CancellationTokenSource? _stop;
    private Task? _runLoop;
    private bool _pauseRequested;
    private bool _disposed;
    private readonly ConcurrentQueue<Action> _commands = new();
    private TaskCompletionSource? _started;
    private string? _currentDiskPath;

    internal AmigaMachine(Guid id, AmigaMachineConfiguration configuration,
        IAmigaCore core, string sessionDirectory, IAudioOutput? audioOutput = null)
    {
        Id = id;
        Configuration = configuration;
        _core = core;
        _sessionDirectory = sessionDirectory;
        _audioOutput = audioOutput;
        _currentDiskPath = configuration.InitialDiskPath;
    }

    public Guid Id { get; }
    public AmigaMachineConfiguration Configuration { get; }
    public EmulationMachineState State { get; private set; } = EmulationMachineState.Created;
    public VideoFrame? LatestVideoFrame => _core.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _core.LatestAudioChunk;
    public IReadOnlyList<AmigaCoreOption> AvailableOptions => _core.Options;
    public event EventHandler<VideoFrame>? VideoFrameReady;
    public event EventHandler<AudioChunk>? AudioChunkReady;

    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        Task started;
        lock (_gate)
        {
            ThrowIfDisposed();
            if (State is not EmulationMachineState.Created and not EmulationMachineState.Stopped)
                throw new InvalidOperationException($"Cannot start an Amiga machine in state {State}.");
            State = EmulationMachineState.Starting;
            _stop = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _runLoop = Task.Factory.StartNew(() => Run(_stop.Token), CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
            started = _started.Task;
        }
        await started.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public ValueTask PauseAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (State != EmulationMachineState.Running) return ValueTask.CompletedTask;
            _pauseRequested = true;
            State = EmulationMachineState.Paused;
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask ResumeAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (State != EmulationMachineState.Paused) return ValueTask.CompletedTask;
            _pauseRequested = false;
            State = EmulationMachineState.Running;
            Monitor.PulseAll(_gate);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask HardResetAsync(CancellationToken cancellationToken = default) =>
        QueueCommand(_core.HardReset, cancellationToken);

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        Task? loop;
        lock (_gate)
        {
            if (State is EmulationMachineState.Stopped or EmulationMachineState.Created) return;
            State = EmulationMachineState.Stopping;
            _stop?.Cancel();
            _pauseRequested = false;
            Monitor.PulseAll(_gate);
            loop = _runLoop;
        }
        if (loop is not null) await loop.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public void SetInput(EmulationInputSnapshot snapshot) => _core.SetInput(snapshot);

    public ValueTask InsertFloppyAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() => { _core.InsertFloppy(path); _currentDiskPath = Path.GetFullPath(path); }, cancellationToken);

    public ValueTask EjectFloppyAsync(CancellationToken cancellationToken = default) =>
        QueueCommand(_core.EjectFloppy, cancellationToken);

    public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var header = new AmigaSavedStateHeader(1, Configuration.Model, _core.CoreSha256,
                AmigaStateStore.HashFile(Configuration.KickstartPath),
                _currentDiskPath is null ? null : AmigaStateStore.HashFile(_currentDiskPath),
                Configuration.Options);
            AmigaStateStore.Write(path, header, _core.SaveState());
        }, cancellationToken);

    public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var saved = AmigaStateStore.Read(path);
            if (saved.Header.FormatVersion != 1 || saved.Header.Model != Configuration.Model
                || saved.Header.CoreSha256 != _core.CoreSha256
                || saved.Header.KickstartSha256 != AmigaStateStore.HashFile(Configuration.KickstartPath))
                throw new InvalidDataException("The Amiga state does not match the running machine.");
            _core.LoadState(saved.State);
        }, cancellationToken);

    public ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default) =>
        QueueCommand(() => _core.SetOption(key, value), cancellationToken);

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
            throw new InvalidOperationException("The Amiga machine must be running before changing a floppy.");
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _commands.Enqueue(() =>
        {
            try { action(); completion.SetResult(); }
            catch (Exception error) { completion.SetException(error); }
        });
        lock (_gate) Monitor.PulseAll(_gate);
        return new ValueTask(completion.Task.WaitAsync(cancellationToken));
    }

    private void Run(CancellationToken cancellationToken)
    {
        try
        {
            _core.Initialize(Configuration, _sessionDirectory);
            if (_audioOutput is not null)
            {
                try { _audioOutput.Start(_core.SampleRate); }
                catch { _audioOutput.Dispose(); _audioOutput = null; }
            }
            lock (_gate)
            {
                State = EmulationMachineState.Running;
                _started?.TrySetResult();
            }
            var frameDuration = TimeSpan.FromSeconds(1 / _core.FramesPerSecond);
            var nextFrame = TimeProvider.System.GetTimestamp();
            long videoSequence = 0;
            long audioSequence = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                lock (_gate)
                    while (_pauseRequested && _commands.IsEmpty && !cancellationToken.IsCancellationRequested)
                        Monitor.Wait(_gate, TimeSpan.FromMilliseconds(100));
                if (cancellationToken.IsCancellationRequested) break;

                while (_commands.TryDequeue(out var command)) command();

                _core.RunFrame();
                if (_core.LatestVideoFrame is { } video && video.Sequence != videoSequence)
                {
                    videoSequence = video.Sequence;
                    VideoFrameReady?.Invoke(this, video);
                }
                if (_core.LatestAudioChunk is { } audio && audio.Sequence != audioSequence)
                {
                    audioSequence = audio.Sequence;
                    _audioOutput?.Write(audio.InterleavedStereo.Span);
                    AudioChunkReady?.Invoke(this, audio);
                }

                nextFrame += (long)(frameDuration.TotalSeconds * TimeProvider.System.TimestampFrequency);
                var remaining = TimeProvider.System.GetElapsedTime(TimeProvider.System.GetTimestamp(), nextFrame);
                if (remaining > TimeSpan.Zero) Thread.Sleep(remaining);
                else nextFrame = TimeProvider.System.GetTimestamp();
            }
            _core.Stop();
            _audioOutput?.Stop();
            lock (_gate) State = EmulationMachineState.Stopped;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _started?.TrySetCanceled(cancellationToken);
            lock (_gate) State = EmulationMachineState.Stopped;
        }
        catch (Exception error)
        {
            _started?.TrySetException(error);
            lock (_gate) State = EmulationMachineState.Faulted;
            throw;
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
        _disposed = true;
    }
}
