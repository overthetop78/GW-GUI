using System.Collections.Concurrent;
using System.Diagnostics;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Emulation.Atari;

internal sealed class AtariMachine : IAtariMachine
{
    private readonly object _gate = new();
    private readonly IAtariCore _core;
    private readonly string _sessionDirectory;
    private readonly string? _saveDirectory;
    private readonly ConcurrentQueue<AtariMachineCommand> _commands = new();
    private readonly AtariAudioOutputController _audio;
    private CancellationTokenSource? _stopSource;
    private Task? _runLoop;
    private TaskCompletionSource? _started;
    private bool _pauseRequested;
    private bool _disposed;

    internal AtariMachine(Guid id, AtariMachineConfiguration configuration, IAtariCore core,
        string sessionDirectory, IAudioOutput? audioOutput = null, string? saveDirectory = null,
        Func<IAudioOutput?>? audioOutputFactory = null)
    {
        Id = id;
        Configuration = configuration;
        _core = core;
        _sessionDirectory = sessionDirectory;
        _audio = new AtariAudioOutputController(audioOutput, audioOutputFactory);
        _audio.SetMuted(!configuration.AudioEnabled);
        _saveDirectory = saveDirectory;
    }

    public Guid Id { get; }
    public AtariMachineConfiguration Configuration { get; }
    public Exception? Fault { get; private set; }
    public EmulationMachineState State { get; private set; } = EmulationMachineState.Created;
    public VideoFrame? LatestVideoFrame => _core.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _core.LatestAudioChunk;
    public IReadOnlyList<AtariCoreOption> AvailableOptions => _core.Options;
    public IReadOnlyList<string> Diagnostics => _core.Diagnostics;
    public IReadOnlyDictionary<int, bool> LedStates => _core.LedStates;
    public string CoreName => _core.CoreName;
    public string CoreVersion => _core.CoreVersion;
    public IReadOnlySet<string> SupportedContentExtensions => _core.SupportedContentExtensions;
    public bool IsAudioMuted => _audio.IsMuted;
    public float AudioVolume => _audio.Volume;
    public AtariRuntimeStatus RuntimeStatus => AtariRuntimeFunctions.Status(Configuration, _core, Fault);
    public event EventHandler<VideoFrame>? VideoFrameReady;
    public event EventHandler<AudioChunk>? AudioChunkReady;

    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        Task started;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (State != EmulationMachineState.Created)
                throw new InvalidOperationException(AtariMachineConstants.InvalidStartStateMessage);
            State = EmulationMachineState.Starting;
            _stopSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try { Run(_stopSource.Token); }
                finally { completed.TrySetResult(); }
            })
            {
                IsBackground = true,
                Name = AtariMachineFunctions.ThreadName(Id, Configuration.Core)
            };
            _runLoop = completed.Task;
            thread.Start();
            started = _started.Task;
        }
        await started.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public ValueTask PauseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (State != EmulationMachineState.Running) return ValueTask.CompletedTask;
            _pauseRequested = true;
            State = EmulationMachineState.Paused;
            _audio.Pause();
        }
        return QueueCommand(static () => { }, cancellationToken);
    }

    public ValueTask ResumeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (State != EmulationMachineState.Paused) return ValueTask.CompletedTask;
            _pauseRequested = false;
            State = EmulationMachineState.Running;
            _audio.Resume();
            Monitor.PulseAll(_gate);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) =>
        QueueCommand(ResetCore, cancellationToken);

    public ValueTask HardResetAsync(CancellationToken cancellationToken = default) =>
        QueueCommand(ResetCore, cancellationToken);

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        Task? loop;
        lock (_gate)
        {
            if (State is EmulationMachineState.Created or EmulationMachineState.Stopped) return;
            loop = _runLoop;
            if (State != EmulationMachineState.Faulted)
            {
                State = EmulationMachineState.Stopping;
                _stopSource?.Cancel();
                _pauseRequested = false;
                Monitor.PulseAll(_gate);
            }
        }
        if (loop is not null) await loop.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public void SetInput(EmulationInputSnapshot snapshot) =>
        QueueCommand(_core.SetInput, snapshot, CancellationToken.None).AsTask().GetAwaiter().GetResult();

    public void SetControllerPortDevice(int port, AtariPeripheralKind peripheral) =>
        QueueCommand(() => _core.SetControllerPortDevice(port, peripheral), CancellationToken.None)
            .AsTask().GetAwaiter().GetResult();

    public void SetAudioMuted(bool muted)
    {
        _audio.SetMuted(muted);
    }

    public void SetAudioVolume(float volume) => _audio.SetVolume(volume);

    public void SetAudioOutputFactory(Func<IAudioOutput?>? factory) => _audio.ReplaceFactory(factory);

    public ValueTask InsertMediaAsync(AtariMediaConfiguration media, CancellationToken cancellationToken = default) =>
        QueueCommand(() => _core.InsertMedia(media), cancellationToken);

    public ValueTask EjectMediaAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default) =>
        QueueCommand(() => _core.EjectMedia(slot), cancellationToken);

    public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) =>
        QueueCommand(() => _core.SelectDisk(index), cancellationToken);

    public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() => AtariMachineFunctions.SaveState(path, _core.SaveState()), cancellationToken);

    public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() => _core.LoadState(AtariMachineFunctions.LoadState(path)), cancellationToken);

    public ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default) =>
        QueueCommand(() => _core.SetOption(key, value), cancellationToken);

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
            throw new InvalidOperationException(AtariMachineConstants.InvalidStateMessage);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _commands.Enqueue(new AtariMachineCommand(action, completion));
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
            long videoSequence = default;
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (_gate)
                    while (_pauseRequested && _commands.IsEmpty && !cancellationToken.IsCancellationRequested)
                        Monitor.Wait(_gate, TimeSpan.FromMilliseconds(AtariMachineConstants.PauseWaitMilliseconds));
                if (cancellationToken.IsCancellationRequested) break;
                while (_commands.TryDequeue(out var command)) command.Execute();
                lock (_gate)
                    if (_pauseRequested) continue;
                _core.RunFrame();
                PublishOutputs(ref videoSequence);
                nextFrame = AtariMachineFunctions.NextFrameTimestamp(nextFrame, _core.FramesPerSecond);
                AtariMachineFunctions.WaitForFrame(nextFrame, cancellationToken);
                if (nextFrame < Stopwatch.GetTimestamp()) nextFrame = Stopwatch.GetTimestamp();
            }
        }
        catch (Exception error)
        {
            if (_core.Diagnostics.Count > AtariMachineConstants.EmptyCount)
                error.Data[AtariMachineConstants.DiagnosticDataKey] = string.Join(Environment.NewLine,
                    _core.Diagnostics.TakeLast(AtariMachineConstants.DiagnosticTailCount));
            Fault = error;
            _started?.TrySetException(error);
            FailPendingCommands(error);
            lock (_gate) State = EmulationMachineState.Faulted;
        }
        finally
        {
            if (initialized)
                try { _core.Stop(); } catch (Exception error) { Fault ??= error; }
            try { _core.Dispose(); } catch (Exception error) { Fault ??= error; }
            try { _audio.Stop(); } catch (Exception error) { Fault ??= error; }
            FailPendingCommands(new OperationCanceledException(AtariMachineConstants.StoppedMessage));
            lock (_gate)
                State = Fault is null ? EmulationMachineState.Stopped : EmulationMachineState.Faulted;
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

    private void ResetCore()
    {
        _audio.Reset();
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
        AtariMachineFunctions.DeleteSessionDirectory(_sessionDirectory);
        _disposed = true;
    }
}
