using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;
using System.Collections.Concurrent;

namespace GWGUI.Emulation.Amiga;

internal sealed class AmigaMachine : IAmigaMachine
{
    private readonly object _gate = new();
    private readonly IAmigaCore _core;
    private readonly string _sessionDirectory;
    private readonly string? _saveDirectory;
    private IAudioOutput? _audioOutput;
    private CancellationTokenSource? _stop;
    private Task? _runLoop;
    private bool _pauseRequested;
    private bool _disposed;
    private readonly ConcurrentQueue<PendingCommand> _commands = new();
    private TaskCompletionSource? _started;
    private string? _currentDiskPath;
    private readonly List<string> _mediaPaths;
    private readonly Dictionary<string, string> _currentOptions;

    internal AmigaMachine(Guid id, AmigaMachineConfiguration configuration,
        IAmigaCore core, string sessionDirectory, IAudioOutput? audioOutput = null, string? saveDirectory = null)
    {
        Id = id;
        Configuration = configuration;
        _core = core;
        _sessionDirectory = sessionDirectory;
        _saveDirectory = saveDirectory;
        _audioOutput = audioOutput;
        _mediaPaths = AmigaExternalCore.ResolveConfiguredMedia(configuration)
            .Select(item => Path.GetFullPath(item.Path)).ToList();
        _currentDiskPath = _mediaPaths.FirstOrDefault();
        _currentOptions = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
    }

    public Guid Id { get; }
    public AmigaMachineConfiguration Configuration { get; }
    public Exception? Fault { get; private set; }
    public EmulationMachineState State { get; private set; } = EmulationMachineState.Created;
    public VideoFrame? LatestVideoFrame => _core.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _core.LatestAudioChunk;
    public IReadOnlyList<AmigaCoreOption> AvailableOptions => _core.Options;
    public IReadOnlyList<string> Diagnostics => _core.Diagnostics;
    public string CoreName => _core.CoreName;
    public string CoreVersion => _core.CoreVersion;
    public IReadOnlySet<string> SupportedContentExtensions => _core.SupportedContentExtensions;
    public int DiskCount => _core.DiskCount;
    public int CurrentDiskIndex => _core.CurrentDiskIndex;
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
            FlushAudio();
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
        QueueCommand(() => { FlushAudio(); _core.HardReset(); }, cancellationToken);

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        Task? loop;
        lock (_gate)
        {
            if (State is EmulationMachineState.Stopped or EmulationMachineState.Created) return;
            loop = _runLoop;
            if (State != EmulationMachineState.Faulted)
            {
                State = EmulationMachineState.Stopping;
                _stop?.Cancel();
                _pauseRequested = false;
                Monitor.PulseAll(_gate);
            }
        }
        if (loop is not null) await loop.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public void SetInput(EmulationInputSnapshot snapshot) => _core.SetInput(snapshot);

    public ValueTask InsertMediaAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var fullPath = Path.GetFullPath(path);
            _core.InsertMedia(fullPath);
            var index = Math.Max(0, _core.CurrentDiskIndex);
            if (index < _mediaPaths.Count) _mediaPaths[index] = fullPath;
            else _mediaPaths.Add(fullPath);
            _currentDiskPath = fullPath;
        }, cancellationToken);

    public ValueTask EjectMediaAsync(CancellationToken cancellationToken = default) =>
        QueueCommand(() => { _core.EjectMedia(); _currentDiskPath = null; }, cancellationToken);

    public ValueTask InsertFloppyAsync(string path, CancellationToken cancellationToken = default) =>
        InsertMediaAsync(path, cancellationToken);

    public ValueTask EjectFloppyAsync(CancellationToken cancellationToken = default) =>
        EjectMediaAsync(cancellationToken);

    public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            _core.SelectDisk(index);
            if (index < _mediaPaths.Count) _currentDiskPath = _mediaPaths[index];
        }, cancellationToken);

    public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var state = _core.SaveState();
            var header = new AmigaSavedStateHeader(3, Configuration.Model, _core.CoreSha256,
                AmigaStateStore.HashFile(Configuration.KickstartPath),
                _currentDiskPath is null ? null : AmigaStateStore.HashPath(_currentDiskPath),
                new Dictionary<string, string>(_currentOptions, StringComparer.Ordinal),
                HashOptionalFile(Configuration.ExtendedRomPath), HashOptionalFile(Configuration.RomKeyPath),
                AmigaStateStore.HashBytes(state), _mediaPaths.Select(AmigaStateStore.HashPath).ToArray());
            AmigaStateStore.Write(path, header, state);
        }, cancellationToken);

    public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var saved = AmigaStateStore.Read(path);
            if (saved.Header.FormatVersion is < 1 or > 3 || saved.Header.Model != Configuration.Model
                || saved.Header.CoreSha256 != _core.CoreSha256
                || saved.Header.KickstartSha256 != AmigaStateStore.HashFile(Configuration.KickstartPath))
                throw new InvalidDataException("The Amiga state does not match the running machine.");
            if (saved.Header.FormatVersion >= 2
                && (saved.Header.ExtendedRomSha256 != HashOptionalFile(Configuration.ExtendedRomPath)
                    || saved.Header.RomKeySha256 != HashOptionalFile(Configuration.RomKeyPath)
                    || saved.Header.MediaSha256 != HashOptionalPath(_currentDiskPath)
                    || !OptionsEqual(saved.Header.Options, _currentOptions)))
                throw new InvalidDataException("The Amiga state firmware, media or options do not match the running machine.");
            if (saved.Header.FormatVersion >= 3
                && !(saved.Header.MediaSha256s ?? []).SequenceEqual(_mediaPaths.Select(AmigaStateStore.HashPath), StringComparer.OrdinalIgnoreCase))
                throw new InvalidDataException("The Amiga state media list does not match the running machine.");
            _core.LoadState(saved.State);
        }, cancellationToken);

    public ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default) =>
        QueueCommand(() => { _core.SetOption(key, value); _currentOptions[key] = value; }, cancellationToken);

    private static string? HashOptionalFile(string? path) => path is null ? null : AmigaStateStore.HashFile(path);
    private static string? HashOptionalPath(string? path) => path is null ? null : AmigaStateStore.HashPath(path);

    private static bool OptionsEqual(IReadOnlyDictionary<string, string>? left, IReadOnlyDictionary<string, string> right)
    {
        if ((left?.Count ?? 0) != right.Count) return false;
        return left is null ? right.Count == 0 : left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);
    }

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
            throw new InvalidOperationException("The Amiga machine must be running before changing a floppy.");
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
                    if (_audioOutput is not null)
                    {
                        try { _audioOutput.Write(audio.InterleavedStereo.Span); }
                        catch { _audioOutput.Dispose(); _audioOutput = null; }
                    }
                    AudioChunkReady?.Invoke(this, audio);
                }

                nextFrame += (long)(frameDuration.TotalSeconds * TimeProvider.System.TimestampFrequency);
                var remaining = TimeProvider.System.GetElapsedTime(TimeProvider.System.GetTimestamp(), nextFrame);
                if (remaining > TimeSpan.Zero) Thread.Sleep(remaining);
                else nextFrame = TimeProvider.System.GetTimestamp();
            }
        }
        catch (Exception error)
        {
            if (_core.Diagnostics.Count > 0) error.Data["AmigaDiagnostics"] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(100));
            Fault = error;
            _started?.TrySetException(error);
            FailPendingCommands(error);
            lock (_gate) State = EmulationMachineState.Faulted;
        }
        finally
        {
            if (initialized)
            {
                try { _core.Stop(); }
                catch (Exception error) { Fault ??= error; }
            }
            try { _audioOutput?.Stop(); }
            catch (Exception error) { Fault ??= error; }
            FailPendingCommands(new OperationCanceledException("The Amiga machine stopped."));
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
