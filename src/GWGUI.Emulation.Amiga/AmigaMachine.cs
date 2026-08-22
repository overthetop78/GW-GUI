using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;
using System.Collections.Concurrent;

namespace GWGUI.Emulation.Amiga;

internal sealed class AmigaMachine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    private readonly object _gate = new();
    private readonly IAmigaCore _core;
    private readonly string _sessionDirectory;
    private readonly string? _saveDirectory;
    private IAudioOutput? _audioOutput;
    private CancellationTokenSource? _stop;
    private Task? _runLoop;
    private bool _pauseRequested;
    private volatile bool _audioMuted;
    private float _audioVolume = 1f;
    private bool _disposed;
    private readonly ConcurrentQueue<PendingCommand> _commands = new();
    private TaskCompletionSource? _started;
    private string? _currentDiskPath;
    private readonly List<string> _mediaPaths;
    private readonly List<EmulationMedia> _mountedCommonMedia;
    private readonly Dictionary<string, string> _currentOptions;
    private EmulationInputSnapshot _lastPhysicalInput = EmulationInputSnapshot.Empty;
    private bool _controllerPointerSwitchPressed;
    private bool _controllerPointerMode;

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
        _mountedCommonMedia = EmulationMediaConversionFunctions
            .ToCommon(AmigaExternalCore.ResolveConfiguredMedia(configuration)).ToList();
        _currentDiskPath = _mediaPaths.FirstOrDefault();
        _currentOptions = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
    }

    public Guid Id { get; }
    public AmigaMachineConfiguration Configuration { get; }
    public IEmulationLifecycle Lifecycle => this;
    public IEmulationInput Input => this;
    public IEmulationMedia Media => this;
    public IEmulationVideo Video => this;
    public IEmulationAudio Audio => this;
    public IEmulationSavedStates SavedStates => this;
    public IEmulationRuntime Runtime => this;
    bool IEmulationInput.SupportsPointerCapture => true;
    bool IEmulationInput.CapturePointerOnClick => Configuration.Input?.CaptureMouse ?? true;
    IReadOnlyDictionary<string, string> IEmulationInput.KeyboardBindings =>
        Configuration.Input?.KeyboardBindings ?? new Dictionary<string, string>();
    bool IEmulationInput.SupportsControllerPointerSwitch => true;
    bool IEmulationInput.ControllerPointerMode => _controllerPointerMode;
    public EmulationMachineState State { get; private set; } = EmulationMachineState.Created;
    public VideoFrame? LatestVideoFrame => _core.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _core.LatestAudioChunk;
    public IReadOnlyList<AmigaCoreOption> AvailableOptions => _core.Options;
    public IReadOnlyDictionary<int, bool> LedStates => _core.LedStates;
    public string CoreName => _core.CoreName;
    public string CoreVersion => _core.CoreVersion;
    public IReadOnlySet<string> SupportedContentExtensions => _core.SupportedContentExtensions;
    public int DiskCount => _core.DiskCount;
    public int CurrentDiskIndex => _core.CurrentDiskIndex;
    public bool IsAudioMuted => _audioMuted;
    public event EventHandler<VideoFrame>? VideoFrameReady;
    public event EventHandler<AudioChunk>? AudioChunkReady;
    IReadOnlyList<EmulationMedia> IEmulationMedia.MountedMedia => _mountedCommonMedia.ToArray();
    async ValueTask IEmulationMedia.InsertAsync(EmulationMedia media, CancellationToken cancellationToken)
    {
        if (media.Slot.Index < DiskCount)
            await SelectDiskAsync(media.Slot.Index, cancellationToken).ConfigureAwait(false);
        await InsertMediaAsync(media.Path, cancellationToken).ConfigureAwait(false);
        var inserted = media with { IsInserted = true };
        _mountedCommonMedia.RemoveAll(item => item.Slot == inserted.Slot);
        _mountedCommonMedia.Add(inserted);
    }
    async ValueTask IEmulationMedia.EjectAsync(EmulationMediaSlot slot, CancellationToken cancellationToken)
    {
        await SelectDiskAsync(slot.Index, cancellationToken).ConfigureAwait(false);
        await EjectMediaAsync(cancellationToken).ConfigureAwait(false);
        var mountedIndex = _mountedCommonMedia.FindIndex(item => item.Slot == slot);
        if (mountedIndex >= 0)
            _mountedCommonMedia[mountedIndex] = _mountedCommonMedia[mountedIndex] with { IsInserted = false };
    }
    ValueTask IEmulationMedia.SelectDiskAsync(EmulationMediaSlot slot, int index,
        CancellationToken cancellationToken) => SelectDiskAsync(index, cancellationToken);
    AudioChunk? IEmulationAudio.LatestChunk => LatestAudioChunk;
    int IEmulationAudio.SampleRate => _core.SampleRate;
    bool IEmulationAudio.IsMuted => IsAudioMuted;
    float IEmulationAudio.Volume => _audioVolume;
    event EventHandler<AudioChunk>? IEmulationAudio.ChunkReady
    {
        add => AudioChunkReady += value;
        remove => AudioChunkReady -= value;
    }
    void IEmulationAudio.SetMuted(bool muted) => SetAudioMuted(muted);
    void IEmulationAudio.SetVolume(float volume) => _audioVolume = Math.Clamp(volume, 0f, 1f);
    void IEmulationAudio.SetOutputFactory(Func<IAudioOutput?>? factory) => ReplaceAudioOutput(factory);
    string IEmulationRuntime.EmulatorName => CoreName;
    string IEmulationRuntime.EmulatorVersion => CoreVersion;
    IReadOnlySet<string> IEmulationRuntime.SupportedContentExtensions => SupportedContentExtensions;
    IReadOnlyDictionary<EmulationMediaSlot, bool> IEmulationRuntime.MediaActivity =>
        EmulationMediaActivityFunctions.FromLedStates(_core.LedStates);
    IReadOnlyList<EmulationOption> IEmulationRuntime.AvailableOptions => AvailableOptions
        .Select(option => new EmulationOption(
            option.Key,
            option.Name,
            option.Description,
            option.Category,
            option.DefaultValue,
            _currentOptions.GetValueOrDefault(option.Key, option.DefaultValue),
            option.Values.Select(value => new EmulationOptionValue(value.Value, value.Label)).ToArray(),
            option.IsVisible))
        .ToArray();
    VideoFrame? IEmulationVideo.LatestFrame => LatestVideoFrame;
    double IEmulationVideo.FramesPerSecond => _core.FramesPerSecond;
    event EventHandler<VideoFrame>? IEmulationVideo.FrameReady
    {
        add => VideoFrameReady += value;
        remove => VideoFrameReady -= value;
    }
    bool IEmulationSavedStates.IsSupported => true;
    ValueTask IEmulationSavedStates.SaveAsync(string path, CancellationToken cancellationToken) =>
        SaveStateAsync(path, cancellationToken);
    ValueTask IEmulationSavedStates.LoadAsync(string path, CancellationToken cancellationToken) =>
        LoadStateAsync(path, cancellationToken);
    void IEmulationInput.SetControllerPortDevice(int port, EmulationPeripheralCategory peripheral) =>
        throw new NotSupportedException();
    ValueTask<bool> IEmulationInput.SwitchControllerPointerAsync(CancellationToken cancellationToken) =>
        SwitchControllerPointerAsync(cancellationToken);

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
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try { Run(_stop.Token); }
                finally { completion.TrySetResult(); }
            })
            {
                IsBackground = true,
                Name = $"GWGUI Amiga {Id:N}"
            };
            _runLoop = completion.Task;
            thread.Start();
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

    public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            FlushAudio();
            var resetKeys = new HashSet<EmulationKey>
                { EmulationKey.LeftControl, EmulationKey.LeftAmiga, EmulationKey.RightAmiga };
            _core.SetInput(EmulationInputSnapshot.Empty with { Keys = resetKeys });
            _core.RunFrame();
            _core.SetInput(EmulationInputSnapshot.Empty);
        }, cancellationToken);

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

    public void SetInput(EmulationInputSnapshot snapshot)
    {
        _lastPhysicalInput = snapshot;
        _core.SetInput(AmigaInputSnapshotFunctions.Apply(snapshot, Configuration.Input,
            _controllerPointerSwitchPressed));
    }

    private async ValueTask<bool> SwitchControllerPointerAsync(CancellationToken cancellationToken)
    {
        _controllerPointerSwitchPressed = true;
        SetInput(_lastPhysicalInput);
        await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        _controllerPointerSwitchPressed = false;
        _controllerPointerMode = !_controllerPointerMode;
        SetInput(_lastPhysicalInput);
        return _controllerPointerMode;
    }

    public void SetAudioMuted(bool muted)
    {
        _audioMuted = muted;
        if (muted) FlushAudio();
    }

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
            if (_core.Diagnostics.Count > 0) error.Data["AmigaDiagnostics"] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(100));
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
