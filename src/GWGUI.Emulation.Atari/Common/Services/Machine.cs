using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime,
    IEmulationCassetteTransport
{
    private readonly object _gate = new();
    private readonly IEmulatorCore _core;
    private readonly string _sessionDirectory;
    private readonly string? _saveDirectory;
    private readonly ConcurrentQueue<MachineCommand> _commands = new();
    private readonly AudioOutputController _audio;
    private readonly CassetteInputController _cassetteInput;
    private readonly List<MediaConfiguration> _mountedMedia;
    private CancellationTokenSource? _stopSource;
    private Task? _runLoop;
    private TaskCompletionSource? _started;
    private bool _pauseRequested;
    private bool _disposed;

    internal Machine(Guid id, MachineConfiguration configuration, IEmulatorCore core,
        string sessionDirectory, IAudioOutput? audioOutput = null, string? saveDirectory = null,
        Func<IAudioOutput?>? audioOutputFactory = null)
    {
        Id = id;
        Configuration = configuration;
        _mountedMedia = configuration.Media.Where(item => item.IsInserted).ToList();
        _core = core;
        _sessionDirectory = sessionDirectory;
        _audio = new AudioOutputController(audioOutput, audioOutputFactory);
        _cassetteInput = new CassetteInputController(configuration);
        _audio.SetMuted(!configuration.AudioEnabled);
        if (configuration.Options.TryGetValue(ConfigurationOptionConstants.AudioVolume, out var volume)
            && int.TryParse(volume, NumberStyles.Integer, CultureInfo.InvariantCulture, out var volumePercent))
            _audio.SetVolume(volumePercent / 100f);
        _saveDirectory = saveDirectory;
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
    public IEmulationCassetteTransport? CassetteTransport =>
        Configuration.Core == Emulator.Atari800 ? this : null;
    bool IEmulationInput.SupportsPointerCapture =>
        CompatibilityCatalog.Get(Configuration.Model).VisibleTabs.Contains(SettingsTab.Mouse);
    bool IEmulationInput.CapturePointerOnClick => Configuration.Input?.CaptureMouse ?? true;
    IReadOnlyDictionary<string, string> IEmulationInput.KeyboardBindings =>
        Configuration.Input?.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(),
            StringComparer.Ordinal) ?? new Dictionary<string, string>();
    bool IEmulationInput.SupportsControllerPointerSwitch => false;
    bool IEmulationInput.ControllerPointerMode => false;
    public EmulationMachineState State { get; private set; } = EmulationMachineState.Created;
    public VideoFrame? LatestVideoFrame => _core.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _core.LatestAudioChunk;
    public IReadOnlyList<CoreOption> AvailableOptions => _core.Options;
    public IReadOnlyDictionary<int, bool> LedStates => _core.LedStates;
    public string CoreName => _core.CoreName;
    public string CoreVersion => _core.CoreVersion;
    public IReadOnlySet<string> SupportedContentExtensions => _core.SupportedContentExtensions;
    public bool SupportsSaveStates => _core.SupportsSaveStates;
    public bool IsAudioMuted => _audio.IsMuted;
    public float AudioVolume => _audio.Volume;
    public RuntimeStatus RuntimeStatus => RuntimeFunctions.Status(Configuration, _core);
    public event EventHandler<VideoFrame>? VideoFrameReady;
    public event EventHandler<AudioChunk>? AudioChunkReady;
    EmulationCassetteState IEmulationCassetteTransport.State => CassetteStateFunctions.From(
        _mountedMedia.Any(media => media.Category == MediaCategory.Cassette && media.IsInserted),
        ((IEmulationRuntime)this).MediaActivity.GetValueOrDefault(EmulationMediaSlot.Cassette0));
    EmulationCassetteCommand? IEmulationCassetteTransport.ActiveOperation =>
        ((IEmulationCassetteTransport)this).State == EmulationCassetteState.Playing
            ? EmulationCassetteCommand.Play
            : null;
    IReadOnlySet<EmulationCassetteCommand> IEmulationCassetteTransport.AvailableCommands =>
        CassetteInputController.AvailableCommands;
    IReadOnlyList<EmulationMedia> IEmulationMedia.MountedMedia => _mountedMedia
        .Select(EmulationMediaConversionFunctions.ToCommon).OfType<EmulationMedia>().ToArray();
    ValueTask IEmulationMedia.InsertAsync(EmulationMedia media, CancellationToken cancellationToken) =>
        InsertMediaAsync(EmulationMediaConversionFunctions.ToAtari(media, _mountedMedia), cancellationToken);
    ValueTask IEmulationMedia.EjectAsync(EmulationMediaSlot slot, CancellationToken cancellationToken) =>
        EjectMediaAsync(slot, cancellationToken);
    ValueTask IEmulationMedia.SelectDiskAsync(EmulationMediaSlot slot, int index,
        CancellationToken cancellationToken) => SelectDiskAsync(index, cancellationToken);
    AudioChunk? IEmulationAudio.LatestChunk => LatestAudioChunk;
    int IEmulationAudio.SampleRate => _core.SampleRate;
    bool IEmulationAudio.IsMuted => IsAudioMuted;
    float IEmulationAudio.Volume => AudioVolume;
    event EventHandler<AudioChunk>? IEmulationAudio.ChunkReady
    {
        add => AudioChunkReady += value;
        remove => AudioChunkReady -= value;
    }
    void IEmulationAudio.SetMuted(bool muted) => SetAudioMuted(muted);
    void IEmulationAudio.SetVolume(float volume) => SetAudioVolume(volume);
    void IEmulationAudio.SetOutputFactory(Func<IAudioOutput?>? factory) => SetAudioOutputFactory(factory);
    string IEmulationRuntime.EmulatorName => CoreName;
    string IEmulationRuntime.EmulatorVersion => CoreVersion;
    IReadOnlySet<string> IEmulationRuntime.SupportedContentExtensions => SupportedContentExtensions;
    IReadOnlyDictionary<EmulationMediaSlot, bool> IEmulationRuntime.MediaActivity =>
        EmulationMediaActivityFunctions.FromRuntimeStatus(RuntimeStatus);
    IReadOnlyList<EmulationOption> IEmulationRuntime.AvailableOptions => AvailableOptions
        .Select(option => new EmulationOption(
            option.Key,
            option.Name,
            option.Description,
            option.Category,
            option.DefaultValue,
            option.CurrentValue,
            option.Values.Select(value => new EmulationOptionValue(value.Value, value.Label)).ToArray(),
            option.IsVisible,
            RuntimeOptionFunctions.RequiresRestart(Configuration.Core, option.Key)))
        .ToArray();
    VideoFrame? IEmulationVideo.LatestFrame => LatestVideoFrame;
    double IEmulationVideo.FramesPerSecond => _core.FramesPerSecond;
    event EventHandler<VideoFrame>? IEmulationVideo.FrameReady
    {
        add => VideoFrameReady += value;
        remove => VideoFrameReady -= value;
    }
    bool IEmulationSavedStates.IsSupported => SupportsSaveStates;
    ValueTask IEmulationSavedStates.SaveAsync(string path, CancellationToken cancellationToken) =>
        SaveStateAsync(path, cancellationToken);
    ValueTask IEmulationSavedStates.LoadAsync(string path, CancellationToken cancellationToken) =>
        LoadStateAsync(path, cancellationToken);
    void IEmulationInput.SetControllerPortDevice(int port, EmulationPeripheralCategory peripheral) =>
        SetControllerPortDevice(port, EmulationPeripheralConversionFunctions.ToAtari(peripheral));
}
