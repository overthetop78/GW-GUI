using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Emulation.Amiga;

internal sealed class AmigaMachine : IAmigaMachine
{
    private readonly object _gate = new();
    private readonly IAmigaCore _core;
    private readonly string _sessionDirectory;
    private CancellationTokenSource? _stop;
    private Task? _runLoop;
    private bool _pauseRequested;
    private bool _disposed;

    internal AmigaMachine(Guid id, AmigaMachineConfiguration configuration,
        IAmigaCore core, string sessionDirectory)
    {
        Id = id;
        Configuration = configuration;
        _core = core;
        _sessionDirectory = sessionDirectory;
    }

    public Guid Id { get; }
    public AmigaMachineConfiguration Configuration { get; }
    public EmulationMachineState State { get; private set; } = EmulationMachineState.Created;
    public VideoFrame? LatestVideoFrame => _core.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _core.LatestAudioChunk;
    public event EventHandler<VideoFrame>? VideoFrameReady;
    public event EventHandler<AudioChunk>? AudioChunkReady;

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            if (State is not EmulationMachineState.Created and not EmulationMachineState.Stopped)
                throw new InvalidOperationException($"Cannot start an Amiga machine in state {State}.");
            State = EmulationMachineState.Starting;
            _stop = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _runLoop = Task.Factory.StartNew(() => Run(_stop.Token), CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        return ValueTask.CompletedTask;
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

    public ValueTask HardResetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _core.HardReset();
        return ValueTask.CompletedTask;
    }

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

    private void Run(CancellationToken cancellationToken)
    {
        try
        {
            _core.Initialize(Configuration, _sessionDirectory);
            lock (_gate) State = EmulationMachineState.Running;
            var frameDuration = TimeSpan.FromSeconds(1 / _core.FramesPerSecond);
            var nextFrame = TimeProvider.System.GetTimestamp();
            long videoSequence = 0;
            long audioSequence = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                lock (_gate)
                    while (_pauseRequested && !cancellationToken.IsCancellationRequested)
                        Monitor.Wait(_gate, TimeSpan.FromMilliseconds(100));
                if (cancellationToken.IsCancellationRequested) break;

                _core.RunFrame();
                if (_core.LatestVideoFrame is { } video && video.Sequence != videoSequence)
                {
                    videoSequence = video.Sequence;
                    VideoFrameReady?.Invoke(this, video);
                }
                if (_core.LatestAudioChunk is { } audio && audio.Sequence != audioSequence)
                {
                    audioSequence = audio.Sequence;
                    AudioChunkReady?.Invoke(this, audio);
                }

                nextFrame += (long)(frameDuration.TotalSeconds * TimeProvider.System.TimestampFrequency);
                var remaining = TimeProvider.System.GetElapsedTime(TimeProvider.System.GetTimestamp(), nextFrame);
                if (remaining > TimeSpan.Zero) Thread.Sleep(remaining);
                else nextFrame = TimeProvider.System.GetTimestamp();
            }
            _core.Stop();
            lock (_gate) State = EmulationMachineState.Stopped;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            lock (_gate) State = EmulationMachineState.Stopped;
        }
        catch
        {
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
        _disposed = true;
    }
}
