using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class ExternalCore : IEmulatorCore
{
    private readonly string _corePath;
    private readonly IEmulatorAdapter _adapter;
    private readonly IEmulatorMediaAdapter _mediaAdapter;
    private ExternalCoreInfo _info;
    private ExternalCoreLibrary? _library;
    private ExternalCoreExports? _exports;
    private ExternalHostCallbacks? _callbacks;
    private LoadedContent? _content;
    private bool _nativeInitialized;
    private bool _gameLoaded;
    private bool _supportsSaveStates;
    private bool _disposed;
    private readonly List<MediaConfiguration> _mountedMedia = [];
    private readonly List<SessionMedia> _sessionMedia = [];
    private string? _sessionDirectory;
    private EmulatorPreparedContent? _preparedContent;
    private MachineConfiguration? _configuration;
    private PreparedCartridge? _cartridge;

    internal ExternalCore(string absoluteCorePath, Emulator emulator)
    {
        _corePath = Path.GetFullPath(absoluteCorePath);
        Emulator = emulator;
        _adapter = EmulatorCatalog.CreateAdapter(emulator);
        _mediaAdapter = (IEmulatorMediaAdapter)_adapter;
        _info = ExternalCoreProbe.Inspect(absoluteCorePath, emulator);
        CoreSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(_corePath))).ToLowerInvariant();
    }

    public Emulator Emulator { get; }
    public VideoFrame? LatestVideoFrame => _callbacks?.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _callbacks?.LatestAudioChunk;
    public IReadOnlyList<CoreOption> Options => _callbacks?.Options ?? [];
    public IReadOnlyList<string> Diagnostics => _callbacks?.Diagnostics ?? [];
    public IReadOnlyDictionary<int, bool> LedStates => _callbacks?.LedStates ?? new Dictionary<int, bool>();
    public string CoreName => _info.LibraryName;
    public string CoreVersion => _info.LibraryVersion;
    public string CoreSha256 { get; }
    public IReadOnlySet<string> SupportedContentExtensions => _info.Extensions;
    public bool SupportsSaveStates => _supportsSaveStates;
    public double FramesPerSecond => _callbacks?.FramesPerSecond ?? default;
    public int SampleRate => _callbacks?.SampleRate ?? default;
    public RuntimeRegion? Region { get; private set; }
    public int BufferedAudioFrames => _callbacks?.BufferedAudioFrames ?? default;
    public long AudioOverrunCount => _callbacks?.AudioOverrunCount ?? default;
    public long AudioUnderrunCount => _callbacks?.AudioUnderrunCount ?? default;
    public HostProcessState HostProcessState => HostProcessState.InProcess;
    public int? HostProcessId => null;
    internal IReadOnlyList<MediaConfiguration> MountedMedia => _mountedMedia;

    public bool TryDequeueAudio(out AudioChunk? chunk)
    {
        chunk = null;
        return _callbacks?.TryDequeueAudio(out chunk) == true;
    }

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_library is not null)
            throw new InvalidOperationException(ErrorMessages.CoreAlreadyInitialized);

        try
        {
            var absoluteSession = Path.GetFullPath(sessionDirectory);
            _configuration = configuration;
            _sessionDirectory = absoluteSession;
            var systemDirectory = Path.Combine(absoluteSession, CommonConstants.SystemDirectoryName);
            var media = _mediaAdapter.SelectPrimaryMedia(configuration);
            _library = new ExternalCoreLibrary(_corePath);
            _exports = CoreFunctions.ResolveExports(_library);
            var configuredOptions = _mediaAdapter.GetConfiguredOptions(configuration);
            _callbacks = new ExternalHostCallbacks(Emulator,
                systemDirectory,
                Path.Combine(absoluteSession, CommonConstants.ContentDirectoryName),
                saveDirectory ?? Path.Combine(absoluteSession, CommonConstants.SavesDirectoryName),
                Path.Combine(absoluteSession, CommonConstants.AssetsDirectoryName),
                configuredOptions);
            CoreFunctions.InstallCallbacks(_exports, _callbacks);
            _exports.Initialize();
            _nativeInitialized = true;
            _info = CoreFunctions.ReadInitializedInfo(_exports, Emulator);
            _callbacks.ValidateConfiguredOptions();

            FirmwareRuntimeFunctions.PrepareSystemDirectory(configuration, systemDirectory);
            _preparedContent = _mediaAdapter.PrepareContent(configuration, media, absoluteSession, _info);
            media = _preparedContent?.Configuration;
            if (_preparedContent?.ActivityPaths is { } activityPaths)
                _callbacks.TrackOpticalMedia(activityPaths);
            var runtimeOptions = _preparedContent?.RuntimeOptions ?? configuration.Options;
            foreach (var option in runtimeOptions)
                if (!configuration.Options.TryGetValue(option.Key, out var configuredValue)
                    || !string.Equals(configuredValue, option.Value, StringComparison.Ordinal))
                    _callbacks.SetOption(option.Key, option.Value);

            if (media is not null)
            {
                _content = ContentFunctions.Create(_preparedContent!.RuntimePath,
                    _preparedContent.NeedsFullPath, _info.Extensions,
                    Emulator == Emulator.Atari800);
            }
            CoreLifecycleFunctions.Load(_exports, _callbacks, configuration,
                _content?.GameInfo ?? nint.Zero);
            _gameLoaded = true;
            if (_preparedContent?.BootMedia is { } bootFloppy)
            {
                _sessionMedia.Add(bootFloppy);
                _callbacks.DiskControl.Insert(bootFloppy.RuntimePath);
                _exports.Reset();
                var configuredFloppy = configuration.Media.First(item => item.Slot == EmulationMediaSlot.Floppy0);
                MediaRuntimeFunctions.Register(_mountedMedia, configuredFloppy);
            }
            _supportsSaveStates = StateFunctions.IsAvailable(_exports);
            Region = RuntimeFunctions.Region(_exports.GetRegion());
            _mediaAdapter.ValidatePreparedContent(_preparedContent, _callbacks.DiskControl.IsAvailable);
            if (media is not null)
            {
                MediaRuntimeFunctions.Register(_mountedMedia, media);
                if (_preparedContent?.SessionMedia is { } sessionMedia)
                    _sessionMedia.Add(sessionMedia);
            }
        }
        catch
        {
            DisposeNativeResources();
            throw;
        }
    }

    public void RunFrame()
    {
        RequireCallbacks().BeginFrame();
        RequireExports().Run();
    }
    public void HardReset() => RequireExports().Reset();
    public void SetInput(EmulationInputSnapshot snapshot) => RequireCallbacks().Input = snapshot;
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral)
    {
        var configuration = _configuration ??
            throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        ControllerPortFunctions.ConfigurePort(RequireExports(), RequireCallbacks(), configuration, port, peripheral);
    }
    public void SetOption(string key, string value) => RequireCallbacks().SetOption(key, value);
}
