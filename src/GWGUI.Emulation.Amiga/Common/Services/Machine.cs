using GWGUI.Emulation;
using System.Collections.Concurrent;

namespace GWGUI.Emulation.Amiga.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    private readonly object _gate = new();
    private readonly IEmulatorCore _core;
    private readonly string _sessionDirectory;
    private readonly string? _saveDirectory;
    private readonly Action<string>? _deleteSession;
    private readonly Func<Exception, Exception>? _startErrorTranslator;
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

    internal Machine(Guid id, MachineConfiguration configuration,
        IEmulatorCore core, IReadOnlyList<MediaConfiguration> resolvedMedia,
        string sessionDirectory, IAudioOutput? audioOutput = null, string? saveDirectory = null,
        Action<string>? deleteSession = null, Func<Exception, Exception>? startErrorTranslator = null)
    {
        Id = id;
        Configuration = configuration;
        _core = core;
        _sessionDirectory = sessionDirectory;
        _saveDirectory = saveDirectory;
        _deleteSession = deleteSession;
        _startErrorTranslator = startErrorTranslator;
        _audioOutput = audioOutput;
        _mediaPaths = resolvedMedia
            .Select(item => Path.GetFullPath(item.Path)).ToList();
        _mountedCommonMedia = EmulationMediaConversionFunctions
            .ToCommon(resolvedMedia).ToList();
        _currentDiskPath = _mediaPaths.FirstOrDefault();
        _currentOptions = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
    }

    public Guid Id { get; }
    public MachineConfiguration Configuration { get; }
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
    public IReadOnlyList<CoreOption> AvailableOptions => _core.Options;
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
}
