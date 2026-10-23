using CommonEmulatorCatalog = GWGUI.Emulation.Coleco.Common.Dictionaries.EmulatorCatalog;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Coleco.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Coleco.Emulators.Common.Constants;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants;
using System.IO;
using System.Security.Cryptography;
using GearOptions = GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants.OptionConstants;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore : IEmulatorCore
{
    /// <inheritdoc/>
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback => _callbacks?.Feedback.Snapshot() ?? [];

    private readonly string _corePath;
    private readonly ExternalCoreInfo _info;
    private IEmulationCoreLibrary? _library;
    private ExternalCoreExports? _exports;
    private ExternalHostCallbacks? _callbacks;
    private LoadedContent? _content;
    private LoadedSubsystem? _subsystem;
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
        var completed = false;
        try
        {
            _configuration = configuration;
            _sessionDirectory = Path.GetFullPath(sessionDirectory);
            _saveDirectory = Path.GetFullPath(saveDirectory ?? configuration.Folders.Saves ?? Path.Combine(_sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
            var systemDirectory = Path.Combine(_sessionDirectory, CoreDirectoryConstants.SystemDirectoryName);
            Directory.CreateDirectory(systemDirectory);
            FirmwareRuntimeFunctions.Prepare(configuration, systemDirectory);
            _library = CommonCoreHostFunctions.NativeResources.CreateLibrary(_corePath);
            _exports = CoreFunctions.ResolveExports(_library);
            var runtimeOptions = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
            if (configuration.Core == Emulator.BlueMSX)
                runtimeOptions[GWGUI.Emulation.Coleco.Emulators.BlueMSX.Constants.OptionConstants.OptionMsxtype] = CoreModelConstants.ColecoVision;
            if (configuration.Core == Emulator.GearColeco)
            {
                runtimeOptions[GearOptions.OptionCartridgeHardware] = configuration.Model == MachineModel.Adam ? MediaRuntimeConstants.AdamHardware : MediaRuntimeConstants.ColecoVisionHardware;
                var media = configuration.Media.Where(value => value.IsInserted && value.Category != MediaCategory.Cartridge).ToArray();
                if (media.Select(value => value.IsReadOnly).Distinct().Count() > 1)
                    throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
                runtimeOptions[GearOptions.OptionAdamWritableMedia] = media.Any(value => !value.IsReadOnly) ? MediaRuntimeConstants.SaveWorkingCopy : MediaRuntimeConstants.Disabled;
            }
            _callbacks = new ExternalHostCallbacks(Emulator,
                systemDirectory,
                Path.Combine(_sessionDirectory, CoreDirectoryConstants.ContentDirectoryName),
                _saveDirectory,
                Path.Combine(_sessionDirectory, CoreDirectoryConstants.AssetsDirectoryName), runtimeOptions);
            CoreFunctions.InstallCallbacks(_exports, _callbacks);
            _exports.Initialize();
            _nativeInitialized = true;
            _callbacks.ValidateConfiguredOptions();
            PrepareInitialMedia(configuration);
            if (_content is null && _subsystem is null && !_callbacks.SupportsNoGame)
                throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentRequired, ErrorMessages.ContentRequired);
            var loaded = _subsystem is not null ? _exports.LoadGameSpecial!(GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants.SubsystemConstants.Adam, _subsystem.GameInfos, _subsystem.Count) : _exports.LoadGame(_content?.GameInfo ?? nint.Zero);
            if (!loaded)
                throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
            _gameLoaded = true;
            RestoreBatteryMemory();
            var stateSize = _exports.GetSerializedSize();
            _supportsSaveStates = CommonCoreHostFunctions.MachineExecution.IsSupportedStateSize(stateSize, SavedStateConstants.MaximumStateSize);
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
        ControllerPortFunctions.Configure(RequireExports(), RequireCallbacks(), next);
        _configuration = next;
        _callbacks?.Feedback.ResetPort(port);
    }
}
