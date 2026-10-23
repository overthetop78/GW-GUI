using CommonEmulatorCatalog = GWGUI.Emulation.Snk.Common.Dictionaries.EmulatorCatalog;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Snk.Emulators.Common.Functions;
using System.IO;
using System.Security.Cryptography;
namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore : IEmulatorCore
{
    private readonly string _corePath;
    private readonly ExternalCoreInfo _info;
    private IEmulationCoreLibrary? _library;
    private ExternalCoreExports? _exports;
    private ExternalHostCallbacks? _callbacks;
    private LoadedContent? _content;
    private LibretroMachineFactory? _persistentMediaAdapter;
    private object? _persistentMediaState;
    private MachineConfiguration? _configuration;
    private string? _sessionDirectory;
    private string? _saveDirectory;
    private bool _nativeInitialized;
    private bool _gameLoaded;
    private bool _disposed;
    private bool _supportsSaveStates;

    internal ExternalCore(string absoluteCorePath, Emulator emulator)
    {
        _corePath = Path.GetFullPath(absoluteCorePath);
        Emulator = emulator;
        _info = ExternalCoreProbe.Inspect(_corePath, emulator);
        var formats = CommonEmulatorCatalog.CreateAdapters().Single(adapter => adapter.EmulatorKey == emulator.ToString());
        _info = _info with { Extensions = formats.ResolveContentExtensions(_info.Extensions, _info.NeedsFullPath) };
        CoreSha256 = CommonCoreHostFunctions.StateFiles.HashBytes(File.ReadAllBytes(_corePath)).ToLowerInvariant();
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
    public void SetAudioBufferStatus(AudioOutputBufferStatus? status) { if (_callbacks is { } callbacks) callbacks.OutputAudioStatus = status; }
    public int MinimumAudioLatencyMilliseconds => _callbacks?.MinimumAudioLatencyMilliseconds ?? default;
    public EmulationVideoStandard? Region { get; private set; }
    public int BufferedAudioFrames => _callbacks?.BufferedAudioFrames ?? default;
    public long AudioOverrunCount => _callbacks?.AudioOverrunCount ?? default;
    public long AudioUnderrunCount => _callbacks?.AudioUnderrunCount ?? default;
    public HostProcessState HostProcessState => HostProcessState.InProcess;
    public int? HostProcessId => null;

    public bool TryDequeueAudio(out AudioChunk? chunk)
    {
        chunk = null;
        return _callbacks?.TryDequeueAudio(out chunk) == true;
    }

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_library is not null) throw new InvalidOperationException(ErrorMessages.CoreAlreadyInitialized);
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator) throw new ArgumentException(nameof(configuration));
        var completed = false;
        try
        {
            _configuration = configuration;
            _sessionDirectory = Path.GetFullPath(sessionDirectory);
            _saveDirectory = Path.GetFullPath(saveDirectory ?? Path.Combine(_sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
            var systemDirectory = Path.Combine(_sessionDirectory, CoreDirectoryConstants.SystemDirectoryName);
            Directory.CreateDirectory(systemDirectory);
            FirmwareRuntimeFunctions.Prepare(configuration, systemDirectory);
            _library = CommonCoreHostFunctions.NativeResources.CreateLibrary(_corePath);
            _exports = CoreFunctions.ResolveExports(_library);
            var runtimeOptions = NativeRuntimeOptionFunctions.Prepare(configuration);
            _callbacks = new ExternalHostCallbacks(configuration, systemDirectory, Path.Combine(_sessionDirectory, CoreDirectoryConstants.ContentDirectoryName), _saveDirectory, Path.Combine(_sessionDirectory, CoreDirectoryConstants.AssetsDirectoryName), runtimeOptions);
            CoreFunctions.InstallCallbacks(_exports, _callbacks);
            _exports.Initialize();
            _nativeInitialized = true;
            _callbacks.ValidateConfiguredOptions();
            PrepareInitialMedia(configuration);
            if (_content is null && !_callbacks.SupportsNoGame) throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentRequired, ErrorMessages.ContentRequired);
            ControllerPortFunctions.Configure(_exports, configuration);
            _persistentMediaAdapter = MediaAdapter();
            _persistentMediaState = _persistentMediaAdapter.PreparePersistentMedia(configuration, _saveDirectory, _content);
            var loaded = MediaAdapter().LoadContent(configuration, _exports, _content);
            if (!loaded) throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
            _gameLoaded = true;
            _callbacks.ValidateConfiguredOptions();
            _callbacks.ValidateDeferredOptions(configuration);
            MediaAdapter().CompleteMediaLoad(configuration, _sessionDirectory, _callbacks);
            var stateSize = _exports.GetSerializedSize();
            _supportsSaveStates = CommonCoreHostFunctions.MachineExecution.IsSupportedStateSize(stateSize, SavedStateConstants.MaximumStateSize);

            _exports.GetSystemAvInfo(out var avInfo);
            _callbacks.ApplySystemAvInfo(avInfo);
            Region = _exports.GetRegion() == ExternalCoreInteropConstants.NativeBooleanTrue ? EmulationVideoStandard.Pal : EmulationVideoStandard.Ntsc;
            completed = true;
        }
        finally
        {
            if (!completed) DisposeNativeResources();
        }
    }

    public void RunFrame()
    {
        var exports = RequireExports();
        RequireCallbacks().NotifyAudioBufferStatus();
        exports.Run();
        if (!_supportsSaveStates)
        {
            var size = exports.GetSerializedSize();
            _supportsSaveStates = CommonCoreHostFunctions.MachineExecution.IsSupportedStateSize(size, SavedStateConstants.MaximumStateSize);
        }
    }
    public void HardReset() => RequireExports().Reset();
    public void SetInput(EmulationInputSnapshot snapshot) => RequireCallbacks().Input = snapshot;
    public void SetOption(string key, string value) => RequireCallbacks().SetOption(key, value);
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        var bindings = (configuration.Input.Controllers ?? []).Where(binding => binding.Port != port).ToList();
        var previous = configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port);
        bindings.Add(previous is null ? new ControllerBinding(port, peripheral) : previous with { Peripheral = peripheral });
        var next = configuration with { Input = configuration.Input with { Controllers = bindings } };
        ConfigurationFunctions.Validate(next);
        var previousOptions = NativeRuntimeOptionFunctions.Prepare(configuration);
        foreach (var option in NativeRuntimeOptionFunctions.Prepare(next)) if (!previousOptions.TryGetValue(option.Key, out var previousValue) || previousValue != option.Value) RequireCallbacks().SetOption(option.Key, option.Value);
        ControllerPortFunctions.Configure(RequireExports(), next);
        _configuration = next;
    }
}
