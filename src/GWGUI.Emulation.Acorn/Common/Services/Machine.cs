using CommonControllerFunctions = GWGUI.Emulation.Acorn.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Acorn.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime, IEmulationControllerFeedbackSource
{
    /// <summary>Standard vidéo annoncé par le cœur courant, ou null si inconnu.</summary>
    public EmulationVideoStandard? Region => _core.Region;
    /// <summary>Trames audio en attente selon le dernier statut du cœur.</summary>
    public int BufferedAudioFrames => _core.BufferedAudioFrames;
    /// <summary>Dépassements du tampon audio signalés par le cœur.</summary>
    public long AudioOverrunCount => _core.AudioOverrunCount;
    /// <summary>Manques de données audio réellement comptés par le cœur.</summary>
    public long AudioUnderrunCount => _core.AudioUnderrunCount;

    /// <summary>Entrées actuelles de session, empruntées au propriétaire de la configuration.</summary>
    private InputConfiguration? CurrentInputConfiguration => _inputConfiguration.Input;
    /// <summary>Autorise les retours uniquement pendant une exécution active, avant destruction et hors suspension média.</summary>
    private bool CanPublishControllerOutputs => !_disposed && State == EmulationMachineState.Running;
    /// <summary>Configuration de session courante utilisée par les services Common.</summary>
    private MachineConfiguration CurrentConfiguration() => Configuration with { Input = _inputConfiguration.Input };

    /// <summary>Politique de redémarrage du backend empruntée à la fabrique.</summary>
    private readonly IEmulatorRuntimePolicy? _runtimePolicy;
    private readonly object _gate = new();
    private readonly IEmulatorCore _core;
    /// <summary>Projecteur Common emprunté à la fabrique ; aucune ressource de cœur ne lui est transférée.</summary>
    private readonly IEmulatorInputMapper? _inputMapper;
    /// <summary>Facette média empruntée à l'adaptateur déjà fourni, sans recréer ses fabriques lors des lectures de statut.</summary>
    /// <remarks>La facette partage la durée de vie de l'adaptateur d'entrées ; la machine ne la détruit pas.
    /// Son absence conserve la projection de repli du module, notamment pour une machine construite sans adaptateur.</remarks>
    private readonly IEmulatorMediaCapabilities? _mediaCapabilities;
    private readonly string _sessionDirectory;
    private readonly string? _saveDirectory;
    private readonly ConcurrentQueue<MachineCommand> _commands = new();
    private readonly AudioOutputController _audio;
    private readonly IEmulationInputAccumulator _input = CommonControllerFunctions.CreateInputAccumulator();
    private Thread? _thread;
    private readonly List<MediaConfiguration> _mountedMedia;
    /// <summary>Configuration des entrées de session actualisée après acceptation des changements par le cœur.</summary>
    private MachineConfiguration _inputConfiguration;
    private CancellationTokenSource? _stopSource;
    private Task? _runLoop;
    private TaskCompletionSource? _started;
    private bool _pauseRequested;
    private bool _disposed;

    internal Machine(Guid id, MachineConfiguration configuration, IEmulatorCore core, string sessionDirectory, IAudioOutput? audioOutput = null, string? saveDirectory = null, Func<IAudioOutput?>? audioOutputFactory = null, IEmulatorInputMapper? inputMapper = null, IEmulatorRuntimePolicy? runtimePolicy = null)
    {
        Id = id;
        _runtimePolicy = runtimePolicy;
        Configuration = configuration;
        _inputConfiguration = configuration;
        _mountedMedia = configuration.Media.Where(item => item.IsInserted).ToList();
        _core = core;
        _inputMapper = inputMapper;
        _mediaCapabilities = _inputMapper as IEmulatorMediaCapabilities;
        _sessionDirectory = sessionDirectory;
        _audio = new AudioOutputController(audioOutput, audioOutputFactory);
        _audio.SetMuted(!configuration.AudioEnabled);
        if (configuration.Options.TryGetValue(VideoAudioSettingsConstants.AudioVolumeOption, out var volume)
            && int.TryParse(volume, NumberStyles.Integer, CultureInfo.InvariantCulture, out var volumePercent))
            _audio.SetVolume(volumePercent / 100f);
        _saveDirectory = saveDirectory;
    }

    /// <summary>Projette un instantané avec le contrat de la fabrique ou le traitement Common initial.</summary>
    private EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot) => InputSnapshotFunctions.Apply(snapshot, _inputConfiguration, _inputMapper, false);

    public Guid Id { get; }
    public MachineConfiguration Configuration { get; }
    public IEmulationLifecycle Lifecycle => this;
    public IEmulationInput Input => this;
    public IEmulationMedia Media => this;
    public IEmulationVideo Video => this;
    public IEmulationAudio Audio => this;
    public IEmulationSavedStates SavedStates => this;
    public IEmulationRuntime Runtime => this;
    bool IEmulationInput.SupportsPointerCapture => false;
    bool IEmulationInput.CapturePointerOnClick => false;
    IReadOnlyDictionary<string, string> IEmulationInput.KeyboardBindings => Configuration.Input?.KeyboardBindings ?? Configuration.Input?.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal) ?? new Dictionary<string, string>();
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
    public RuntimeStatus RuntimeStatus => RuntimeFunctions.Status(Configuration, _core, _mediaCapabilities);
    public event EventHandler<VideoFrame>? VideoFrameReady;
    public event EventHandler<AudioChunk>? AudioChunkReady;
    IReadOnlyList<EmulationMedia> IEmulationMedia.MountedMedia => _mountedMedia.Select(EmulationMediaConversionFunctions.ToCommon).OfType<EmulationMedia>().ToArray();
    ValueTask IEmulationMedia.InsertAsync(EmulationMedia media, CancellationToken cancellationToken) => InsertMediaAsync(EmulationMediaConversionFunctions.ToModule(media, _mountedMedia), cancellationToken);
    ValueTask IEmulationMedia.EjectAsync(EmulationMediaSlot slot, CancellationToken cancellationToken) => EjectMediaAsync(slot, cancellationToken);
    ValueTask IEmulationMedia.SelectDiskAsync(EmulationMediaSlot slot, int index, CancellationToken cancellationToken) => SelectDiskAsync(index, cancellationToken);
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
    IReadOnlyDictionary<EmulationMediaSlot, bool> IEmulationRuntime.MediaActivity => RuntimeStatus.MediaActivity;
    IReadOnlyList<EmulationOption> IEmulationRuntime.AvailableOptions => AvailableOptions
        .Select(option => option with { Name = option.ResourceKey ?? option.Name, Description = option.ExplanationResourceKey ?? option.Description, Values = option.Values.Select(value => value with { Label = value.ResourceKey ?? value.InvariantDisplayValue ?? value.Label }).ToArray(), RequiresRestart = RuntimeOptionFunctions.RequiresRestart(Configuration.Core, option.Key) || _runtimePolicy?.RequiresOptionRestart(Configuration, option.Key) == true })
        .ToArray();
    VideoFrame? IEmulationVideo.LatestFrame => LatestVideoFrame;
    double IEmulationVideo.FramesPerSecond => _core.FramesPerSecond;
    event EventHandler<VideoFrame>? IEmulationVideo.FrameReady
    {
        add => VideoFrameReady += value;
        remove => VideoFrameReady -= value;
    }
    bool IEmulationSavedStates.IsSupported => SupportsSaveStates;
    ValueTask IEmulationSavedStates.SaveAsync(string path, CancellationToken cancellationToken) => SaveStateAsync(path, cancellationToken);
    ValueTask IEmulationSavedStates.LoadAsync(string path, CancellationToken cancellationToken) => LoadStateAsync(path, cancellationToken);
    void IEmulationInput.SetControllerPortDevice(int port, EmulationPeripheralCategory peripheral) => SetControllerPortDevice(port, CommonControllerFunctions.FromCommon(peripheral, CurrentConfiguration(), port));
    /// <inheritdoc/>
    public HostProcessState HostProcessState => _core.HostProcessState;
    /// <inheritdoc/>
    public int? HostProcessId => _core.HostProcessId;
}
