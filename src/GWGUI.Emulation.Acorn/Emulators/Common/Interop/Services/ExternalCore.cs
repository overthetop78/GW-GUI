using CommonEmulatorCatalog = GWGUI.Emulation.Acorn.Common.Dictionaries.EmulatorCatalog;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Acorn.Emulators.Common.Functions;
using GWGUI.Emulation.Acorn.Emulators.Common.Contracts;
using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore : IEmulatorCore
{
    private readonly string _corePath;
    private readonly ExternalCoreInfo _info;
    private IEmulationCoreLibrary? _library;
    private ExternalCoreExports? _exports;
    private ExternalHostCallbacks? _callbacks;
    private LoadedContent? _content;
    private MachineConfiguration? _configuration;
    private string? _sessionDirectory;
    private bool _nativeInitialized;
    private bool _gameLoaded;
    private bool _supportsSaveStates;
    private bool _disposed;
    private readonly List<SessionMedia> _sessionMedia = [];

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
    public bool SupportsSaveStates => _gameLoaded && _supportsSaveStates;
    public double FramesPerSecond => _callbacks?.FramesPerSecond ?? default;
    public int SampleRate => _callbacks?.SampleRate ?? default;
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
        if (!ConfigurationFunctions.SupportsRuntime(configuration.Model))
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreNotFound, ErrorMessages.CoreFileMissing);
        var completed = false;
        try
        {
            _configuration = configuration;
            _sessionDirectory = Path.GetFullPath(sessionDirectory);
            _library = CommonCoreHostFunctions.NativeResources.CreateLibrary(_corePath);
            _exports = CoreFunctions.ResolveExports(_library);
            _callbacks = new ExternalHostCallbacks(Emulator,
                Path.Combine(_sessionDirectory, CoreDirectoryConstants.SystemDirectoryName),
                Path.Combine(_sessionDirectory, CoreDirectoryConstants.ContentDirectoryName),
                saveDirectory ?? Path.Combine(_sessionDirectory, CoreDirectoryConstants.SavesDirectoryName),
                Path.Combine(_sessionDirectory, CoreDirectoryConstants.AssetsDirectoryName), configuration.Options);
            CoreFunctions.InstallCallbacks(_exports, _callbacks);
            _exports.Initialize();
            _nativeInitialized = true;
            _callbacks.ValidateConfiguredOptions();
            var media = configuration.Media.FirstOrDefault(item => item.IsInserted);
            if (media is not null)
            {
                var prepared = SessionMediaFunctions.Prepare(media, _sessionDirectory, _info.Extensions);
                _sessionMedia.Add(prepared);
                _content = ContentFunctions.Create(prepared.RuntimePath, _info.NeedsFullPath, _info.Extensions);
            }
            if (_content is null && !_callbacks.SupportsNoGame)
                throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentRequired, ErrorMessages.ContentRequired);
            if (!_exports.LoadGame(_content?.GameInfo ?? nint.Zero))
                throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
            _gameLoaded = true;
            var stateSize = _exports.GetSerializedSize();
            _supportsSaveStates = stateSize > nuint.Zero && stateSize <= (nuint)Array.MaxLength;
            ControllerPortFunctions.Configure(_exports, _callbacks, configuration);
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

    public void RunFrame() => RequireExports().Run();
    public void HardReset()
    {
        RequireExports().Reset();
        // b2 recreates the machine on reset; reattach the session disk afterwards.
        if (_sessionMedia.LastOrDefault() is { } media)
            RequireCallbacks().DiskControl.Insert(media.RuntimePath);
    }
    public void SetInput(EmulationInputSnapshot snapshot) => RequireCallbacks().Input = snapshot;
    public void SetOption(string key, string value) => RequireCallbacks().SetOption(key, value);
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        ControllerPortFunctions.ConfigurePort(RequireExports(), RequireCallbacks(), configuration, port, peripheral);
        var bindings = (configuration.Input.Controllers ?? []).Where(binding => binding.Port != port).ToList();
        var previous = configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port);
        bindings.Add(previous is null ? new ControllerBinding(port, peripheral) : previous with { Peripheral = peripheral });
        _configuration = configuration with { Input = configuration.Input with { Controllers = bindings } };
        RequireCallbacks().ConfigureInput(_configuration.Input);
    }
}
