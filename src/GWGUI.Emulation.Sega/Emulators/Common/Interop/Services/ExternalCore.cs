using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Sega.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

internal sealed class ExternalCore : IEmulatorCore
{
    /// <summary>Capacité native constatée après le chargement, bornée par les limites du format existant.</summary>
    private bool _supportsSaveStates;
    /// <inheritdoc/>
    public bool SupportsSaveStates => _gameLoaded && _supportsSaveStates;
    /// <inheritdoc/>
    public EmulationVideoStandard? Region { get; private set; }
    /// <inheritdoc/>
    public int BufferedAudioFrames => _host?.BufferedAudioFrames ?? 0;
    /// <inheritdoc/>
    public long AudioOverrunCount => _host?.AudioOverrunCount ?? 0;
    /// <inheritdoc/>
    public long AudioUnderrunCount => _host?.AudioUnderrunCount ?? 0;

    /// <inheritdoc/>
    public DiskStatus GetDiskStatus() => (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized())).DiskControl.GetStatus();

    /// <inheritdoc/>
    public bool ShutdownRequested => _host?.ShutdownRequested ?? false;

    /// <inheritdoc/>
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback => _host?.Feedback.Snapshot() ?? [];

    private readonly string _corePath;
    private IEmulationCoreLibrary? _library;
    private ExternalHostCallbacks? _host;
    private ExternalCoreApi.VoidCall? _deinitialize;
    private ExternalCoreApi.VoidCall? _unloadGame;
    private ExternalCoreApi.VoidCall? _run;
    private ExternalCoreApi.VoidCall? _reset;
    private ExternalCoreApi.GetSerializedSize? _getSerializedSize;
    private ExternalCoreApi.Serialize? _serialize;
    private ExternalCoreApi.Serialize? _unserialize;
    private MachineConfiguration? _configuration;
    private string? _sessionDirectory;
    private string? _saveDirectory;
    private bool _gameLoaded;
    private bool _initialized;
    private nint _contentBuffer;

    internal ExternalCore(string corePath) => _corePath = corePath;

    public VideoFrame? LatestVideoFrame => _host?.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _host?.LatestAudioChunk;
    public IReadOnlyList<CoreOption> Options => _host?.OptionCatalog ?? [];
    public IReadOnlyList<string> Diagnostics => _host?.Diagnostics ?? [];
    public IReadOnlyDictionary<int, bool> LedStates => _host?.LedStates ?? new Dictionary<int, bool>();
    public string CoreName { get; private set; } = string.Empty;
    public string CoreVersion { get; private set; } = string.Empty;
    public IReadOnlySet<string> SupportedContentExtensions { get; private set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public string CoreSha256 { get; private set; } = string.Empty;
    public double FramesPerSecond => _host?.FramesPerSecond ?? 50d;
    public int SampleRate => _host?.SampleRate ?? 44_100;
    public int DiskCount => _host?.DiskControl.ImageCount ?? 0;
    public int CurrentDiskIndex => _host?.DiskControl.CurrentIndex ?? -1;

    public bool TryDequeueAudio(out AudioChunk? chunk)
    {
        if (_host is not null) return _host.TryDequeueAudio(out chunk);
        chunk = null;
        return false;
    }

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        _configuration = configuration;
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        var media = ResolveConfiguredMedia(configuration);
        foreach (var item in media)
            if (!File.Exists(item.Path))
                throw new FileNotFoundException(CoreExceptions.MediaNotFound(), item.Path);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var stream = File.OpenRead(sourceCorePath))
            CoreSha256 = CommonCoreHostFunctions.StateFiles.HashStream(stream);

        var systemDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName);
        var contentDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName);
        saveDirectory = Path.GetFullPath(saveDirectory ?? Path.Combine(sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
        Directory.CreateDirectory(systemDirectory);
        new GWGUI.Emulation.Sega.Common.Services.Engine().SelectedAdapter(configuration)!.PrepareFirmware(configuration, systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.CoreDirectory);
        Directory.CreateDirectory(isolatedCoreDirectory);
        var definition = Dictionaries.CoreCatalog.Get(configuration.EmulatorId);
        var isolatedCorePath = Path.Combine(isolatedCoreDirectory, definition.LibraryFileName);
        File.Copy(sourceCorePath, isolatedCorePath, true);
        var contentPath = PrepareContentPath(media, contentDirectory, definition.SelectFirstInsertedMedia);
        if (contentPath is not null && definition.PrepareContent is { } prepareContent)
            contentPath = prepareContent(configuration, contentPath, contentDirectory);
        _host = new ExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory, Functions.FirmwareFunctions.RuntimeOptions(configuration, systemDirectory));

        var initializationCompleted = false;
        try
        {
            _library = CommonCoreHostFunctions.NativeResources.CreateLibrary(isolatedCorePath);
            var apiVersion = Export<ExternalCoreApi.GetApiVersion>(ExternalCoreConstants.RetroApiVersion)();
            if (apiVersion != ExternalCoreInteropConstants.ApiVersion)
                throw new NotSupportedException(CoreExceptions.UnsupportedApiVersion(apiVersion));
            Export<ExternalCoreApi.GetSystemInfo>(ExternalCoreConstants.RetroGetSystemInfo)(out var info);
            var libraryName = Marshal.PtrToStringUTF8(info.LibraryName);
            if (!string.Equals(libraryName, definition.LibraryName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(CoreExceptions.LibraryIdentityMismatch(libraryName));
            CoreName = libraryName!;
            CoreVersion = Marshal.PtrToStringUTF8(info.LibraryVersion) ?? string.Empty;
            var declaredExtensions = CommonCoreHostFunctions.NativeResources.ParseExtensions(info.ValidExtensions);
            SupportedContentExtensions = new Engine().SelectedAdapter(configuration)?.ResolveContentExtensions(declaredExtensions, info.NeedFullPath) ?? declaredExtensions;
            ValidateExtension(contentPath);

            Export<ExternalCoreApi.SetEnvironment>(ExternalCoreConstants.RetroSetEnvironment)(_host.Environment);
            _deinitialize = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroDeinit);
            _unloadGame = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroUnloadGame);
            _run = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroRun);
            _reset = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroReset);
            _getSerializedSize = Export<ExternalCoreApi.GetSerializedSize>(ExternalCoreConstants.RetroSerializeSize);
            _serialize = Export<ExternalCoreApi.Serialize>(ExternalCoreConstants.RetroSerialize);
            _unserialize = Export<ExternalCoreApi.Serialize>(ExternalCoreConstants.RetroUnserialize);
            Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroInit)();
            _initialized = true;
            Export<ExternalCoreApi.SetVideo>(ExternalCoreConstants.RetroSetVideoRefresh)(_host.Video);
            Export<ExternalCoreApi.SetAudioSample>(ExternalCoreConstants.RetroSetAudioSample)(_host.AudioSample);
            Export<ExternalCoreApi.SetAudioBatch>(ExternalCoreConstants.RetroSetAudioSampleBatch)(_host.AudioBatch);
            Export<ExternalCoreApi.SetInputPoll>(ExternalCoreConstants.RetroSetInputPoll)(_host.InputPoll);
            Export<ExternalCoreApi.SetInputState>(ExternalCoreConstants.RetroSetInputState)(_host.InputState);
            var loadGame = Export<ExternalCoreApi.LoadGame>(ExternalCoreConstants.RetroLoadGame);
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException(CoreExceptions.StartWithoutMediaUnsupported());
                _gameLoaded = loadGame(0);
            }
            else _gameLoaded = LoadGame(loadGame, contentPath, info.NeedFullPath);
            if (!_gameLoaded) throw new InvalidOperationException(CoreExceptions.ContentRefused());
            Export<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreConstants.RetroGetSystemAvInfo)(out var av);
            _host.ApplyInitialAvInfo(av);
            _host.ResetHardwareContext();
            var setController = Export<ExternalCoreApi.SetControllerPortDevice>(ExternalCoreConstants.RetroSetControllerPortDevice);
            for (var port = 0; port < ModelCatalog.Get(configuration.Model).ControllerPortCount; port++)
                setController((uint)port, ResolveControllerDevice(_host.ControllerPorts.ElementAtOrDefault(port) ?? [], configuration.Controllers?.ElementAtOrDefault(port) ?? ControllerCatalog.Default(ModelCatalog.Get(configuration.Model))));
            _host.ValidateConfiguredOptions();
            Region = CommonCoreHostFunctions.Protocol.ReadRegion(unchecked((int)Export<ExternalCoreApi.GetRegion>(ExternalCoreConstants.RetroGetRegion)()), allowAllDefinedStandards: false);
            _supportsSaveStates = CommonCoreHostFunctions.MachineExecution.IsSupportedStateSize(_getSerializedSize!(), (nuint)SavedStateConstants.MaximumStateSize);
            initializationCompleted = true;
        }
        finally
        {
            if (!initializationCompleted) Dispose();
        }
    }

    internal static IReadOnlyList<MediaConfiguration> ResolveConfiguredMedia(MachineConfiguration configuration) => (configuration.Media ?? []).Where(item => item.IsInserted).OrderBy(item => item.MountOrder).ToArray();

    internal static string? PrepareContentPath(IReadOnlyList<MediaConfiguration> media, string contentDirectory = "", bool selectFirst = false)
    {
        if (media.Count == 0) return null;
        if (media.Count > 1 && !selectFirst) throw new InvalidOperationException(CoreExceptions.ContentRefused());
        return Path.GetFullPath(media[0].Path);
    }

    internal static uint ResolveControllerDevice(IReadOnlyList<ControllerDevice> devices, PeripheralCategory type)
    {
        if (type == PeripheralCategory.None) return 0;
        if (devices.Count == 0)
            return type is PeripheralCategory.SegaXe1Ap or PeripheralCategory.SegaSaturnMissionStick or PeripheralCategory.SegaSaturnArcadeRacer
                or PeripheralCategory.SegaDreamcastFishingController or PeripheralCategory.SegaDreamcastMaracas
                ? 0 : ExternalHostCallbacksConstants.JoypadDevice;
        var exactName = type switch
        {
            PeripheralCategory.SegaSg1000Joystick or PeripheralCategory.SegaSg1000IiJoypad
                or PeripheralCategory.SegaMasterSystemController or PeripheralCategory.SegaGameGearController
                or PeripheralCategory.SegaControlStick =>
                ExternalCoreConstants.MasterSystemJoypadName,
            PeripheralCategory.SegaMegaDriveThreeButton or PeripheralCategory.SegaArcadePowerStick =>
                ExternalCoreConstants.MegaDriveThreeButtonName,
            PeripheralCategory.SegaMegaDriveSixButton or PeripheralCategory.SegaArcadePowerStickSixButton =>
                ExternalCoreConstants.MegaDriveSixButtonName,
            PeripheralCategory.SegaXe1Ap => ExternalCoreConstants.ControllerXe1Ap,
            PeripheralCategory.SegaSaturnThreeDControlPad => ExternalCoreConstants.ControllerThreeDPad,
            PeripheralCategory.SegaSaturnMissionStick => ExternalCoreConstants.ControllerMissionStick,
            PeripheralCategory.SegaSaturnArcadeRacer => ExternalCoreConstants.ControllerArcadeRacer,
            PeripheralCategory.SegaDreamcastFishingController => ExternalCoreConstants.ControllerFishing,
            PeripheralCategory.SegaDreamcastMaracas => ExternalCoreConstants.ControllerMaracas,
            _ => null
        };
        if (exactName is not null)
        {
            var exactDevice = devices.FirstOrDefault(device => device.Name.Equals(exactName, StringComparison.OrdinalIgnoreCase))
                ?? devices.FirstOrDefault(device => device.Name.Contains(exactName, StringComparison.OrdinalIgnoreCase)
                    && (type != PeripheralCategory.SegaSaturnMissionStick || device.Name.EndsWith(exactName, StringComparison.OrdinalIgnoreCase)));
            if (exactDevice is not null) return exactDevice.Id;
        }
        var aliases = ControllerAliases(type);
        return devices.FirstOrDefault(device => aliases.Any(alias => device.Name.Contains(alias, StringComparison.OrdinalIgnoreCase)
            && (type != PeripheralCategory.SegaSaturnMissionStick || device.Name.EndsWith(alias, StringComparison.OrdinalIgnoreCase))))?.Id ?? 0;
    }

    private static IReadOnlyList<string> ControllerAliases(PeripheralCategory type) => type switch
    {
        PeripheralCategory.SegaLightPhaser => [ExternalCoreConstants.ControllerLightPhaser, ExternalCoreConstants.ControllerLightgun, ExternalCoreConstants.ControllerLightGun],
        PeripheralCategory.SegaMegaMouse or PeripheralCategory.SegaSaturnShuttleMouse
            or PeripheralCategory.SegaDreamcastMouse => [ExternalCoreConstants.ControllerMouse],
        PeripheralCategory.SegaMenacer or PeripheralCategory.SegaSaturnVirtuaGun
            or PeripheralCategory.SegaDreamcastLightGun => [ExternalCoreConstants.ControllerMenacer, ExternalCoreConstants.ControllerLightGun, ExternalCoreConstants.ControllerLightgun],
        PeripheralCategory.SegaPaddleControl => [ExternalCoreConstants.ControllerPaddle],
        PeripheralCategory.SegaSportsPad => [ExternalCoreConstants.ControllerSports],
        PeripheralCategory.SegaSc3000Keyboard or PeripheralCategory.SegaDreamcastKeyboard => [ExternalCoreConstants.ControllerKeyboard],
        PeripheralCategory.SegaActivator => [ExternalCoreConstants.ControllerActivator],
        PeripheralCategory.SegaXe1Ap => [ExternalCoreConstants.ControllerXe1Ap],
        PeripheralCategory.SegaSaturnThreeDControlPad => [ExternalCoreConstants.ControllerThreeDPad],
        PeripheralCategory.SegaSaturnMissionStick => [ExternalCoreConstants.ControllerMissionStick],
        PeripheralCategory.SegaSaturnArcadeRacer => [ExternalCoreConstants.ControllerArcadeRacer],
        PeripheralCategory.SegaDreamcastFishingController => [ExternalCoreConstants.ControllerFishing],
        PeripheralCategory.SegaDreamcastMaracas => [ExternalCoreConstants.ControllerMaracas],
        _ => [ExternalCoreConstants.ControllerJoypad, ExternalCoreConstants.ControllerController, ExternalCoreConstants.ControllerGamepad]
    };

    public void RunFrame()
    {
        _host?.NotifyAudioBufferStatus();
        (_run ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))();
    }
    public void HardReset()
    {
        var configuration = _configuration ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized());
        var sessionDirectory = _sessionDirectory ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized());
        var saveDirectory = _saveDirectory;
        Dispose();
        Initialize(configuration, sessionDirectory, saveDirectory);
    }
    public void SoftReset() => (_reset ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    private void InsertMediaPath(string path) => (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized())).DiskControl.Insert(path);
    private void EjectMediaPath() => (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized())).DiskControl.Eject();
    public void SelectDisk(int index) => (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized())).DiskControl.Select(index);

    /// <inheritdoc/>
    public void InsertMedia(MediaConfiguration media)
    {
        var index = media.Slot.Index;
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(media));
        if (index < DiskCount) SelectDisk(index);
        InsertMediaPath(media.Path);
    }
    /// <inheritdoc/>
    public void EjectMedia(EmulationMediaSlot slot)
    {
        SelectDisk(slot.Index);
        EjectMediaPath();
    }
    /// <inheritdoc/>
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral)
    {
        var configuration = _configuration ?? throw new InvalidOperationException();
        var host = _host ?? throw new InvalidOperationException();
        if (port < 0 || port >= ModelCatalog.Get(configuration.Model).ControllerPortCount || !Enum.IsDefined(peripheral)) throw new ArgumentOutOfRangeException(nameof(port));
        var device = ResolveControllerDevice(host.ControllerPorts.ElementAtOrDefault(port) ?? [], peripheral == PeripheralCategory.Automatic ? ControllerCatalog.Default(ModelCatalog.Get(configuration.Model)) : peripheral);
        Export<ExternalCoreApi.SetControllerPortDevice>(ExternalCoreConstants.RetroSetControllerPortDevice)((uint)port, device);
        host.Feedback.ResetPort(port);
    }

    public byte[] SaveState()
    {
        var size = (_getSerializedSize ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))();
        if (size == ExternalCoreInteropConstants.EmptyNativeSize || size > SavedStateConstants.MaximumStateSize)
            throw new InvalidOperationException(CoreExceptions.InvalidStateSize(size));
        var state = new byte[(int)size];
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            if (!_serialize!(buffer, size))
                throw new InvalidOperationException(CoreExceptions.StateSaveFailed());
            Marshal.Copy(buffer, state, BufferConstants.FirstBufferIndex, state.Length);
        }
        finally { Marshal.FreeHGlobal(buffer); }
        return state;
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (state.IsEmpty) throw new ArgumentException(CoreExceptions.StateEmpty(), nameof(state));
        var bytes = state.ToArray();
        var buffer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, buffer, bytes.Length);
            if (!_unserialize!(buffer, (nuint)bytes.Length))
                throw new InvalidOperationException(CoreExceptions.StateRestoreFailed());
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    public void SetOption(string key, string value) => (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized())).SetOption(key, value);

    public void Stop()
    {
        if (_gameLoaded) _unloadGame?.Invoke();
        _gameLoaded = false;
    }

    private void ValidateExtension(string? path)
    {
        if (path is null) return;
        ValidateConfiguredExtensions(SupportedContentExtensions, [path]);
    }

    internal static void ValidateConfiguredExtensions(IReadOnlySet<string> extensions, IReadOnlyList<string> paths)
    {
        foreach (var path in paths)
        {
            var extension = Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix);
            if (!extensions.Contains(extension))
                throw new InvalidDataException(CoreExceptions.UnsupportedContentExtension(extension));
        }
    }

    private bool LoadGame(ExternalCoreApi.LoadGame loadGame, string path, bool needFullPath)
    {
        using var nativePath = CommonCoreHostFunctions.NativeResources.CreateString(path);
        var game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
        try
        {
            var content = _host!.NeedsContentPath(path, needFullPath) ? null : File.ReadAllBytes(path);
            if (content is not null)
            {
                _contentBuffer = Marshal.AllocHGlobal(content.Length);
                Marshal.Copy(content, BufferConstants.FirstBufferIndex, _contentBuffer, content.Length);
            }
            _host!.SetExtendedGameInfo(path, _contentBuffer, (nuint)(content?.Length ?? 0));
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = nativePath.Pointer,
                    Data = _contentBuffer, Size = (nuint)(content?.Length ?? 0) },
                game, false);
            return loadGame(game);
        }
        finally { Marshal.FreeHGlobal(game); }
    }

    private T Export<T>(string name) where T : Delegate => (_library ?? throw new InvalidOperationException(CoreExceptions.CoreNotLoaded())).Resolve<T>(name);

    private static string ResolveCorePath(string path)
    {
        if (!Path.IsPathFullyQualified(path))
            throw new ArgumentException(CoreExceptions.CorePathNotAbsolute(), nameof(path));
        if (!File.Exists(path)) throw new FileNotFoundException(CoreExceptions.CoreNotFound(), path);
        return path;
    }

    public void Dispose()
    {
        try { _host?.NotifyHardwareContextDestroy(); }
        finally
        {
            try { Stop(); }
            finally
            {
                try { if (_initialized) _deinitialize?.Invoke(); }
                finally
                {
                    _initialized = false;
                    _deinitialize = null;
                    _unloadGame = null;
                    _run = null;
                    _reset = null;
                    _getSerializedSize = null;
                    _serialize = null;
                    _unserialize = null;
                    try
                    {
                        if (_contentBuffer != nint.Zero)
                        {
                            Marshal.FreeHGlobal(_contentBuffer);
                            _contentBuffer = nint.Zero;
                        }
                    }
                    finally
                    {
                        try { _host?.Dispose(); }
                        finally
                        {
                            _host = null;
                            try { _library?.Dispose(); }
                            finally { _library = null; }
                        }
                    }
                }
            }
        }
    }
    /// <inheritdoc/>
    public int MinimumAudioLatencyMilliseconds => _host?.MinimumAudioLatencyMilliseconds ?? 0;
    /// <inheritdoc/>
    public void SetAudioBufferStatus(AudioOutputBufferStatus? status)
    {
        if (_host is { } callbacks) callbacks.OutputAudioStatus = status;
    }
    /// <inheritdoc/>
    public HostProcessState HostProcessState => HostProcessState.InProcess;
    /// <inheritdoc/>
    public int? HostProcessId => null;
    /// <summary>Contrôle la session et la validité de l’identité de lecteur reçue.</summary>
    private void ValidateMediaSaveSlot(EmulationMediaSlot slot)
    {
        if (_library is null) throw new InvalidOperationException();
        if (!Enum.IsDefined(slot.Category) || slot.Index < 0)
            throw new ArgumentOutOfRangeException(nameof(slot));
    }
    /// <inheritdoc/>
    /// <remarks>Ce backend conserve sa politique existante : contenus protégés ou écritures directes.
    /// Il ne possède aucune copie modifiable de média nécessitant une publication supplémentaire.</remarks>
    public void SaveMediaChanges(EmulationMediaSlot slot) => ValidateMediaSaveSlot(slot);
    /// <inheritdoc/>
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) { ValidateMediaSaveSlot(slot); return false; }
}
