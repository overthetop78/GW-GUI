using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreHostFunctions;
using System.IO;

using System.Runtime.InteropServices;
using GWGUI.Emulation;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;

internal sealed class ExternalCore : IEmulatorCore
{
    /// <inheritdoc/>
    public bool SupportsPointerCapture => true;
    /// <inheritdoc/>
    public string Emulator => _configuration?.Core.ToString() ?? string.Empty;

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
    /// <remarks>Le backend possède une seule entrée native de reset ; son traitement existant conserve les montages et les réglages.</remarks>
    public void SoftReset() => HardReset();

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
    private bool _gameLoaded;
    private bool _initialized;
    private ExternalCoreApi.GetSerializedSize? _getSerializedSize;
    private ExternalCoreApi.Serialize? _serialize;
    private ExternalCoreApi.Serialize? _unserialize;
    private ExternalCoreApi.GetRegion? _getRegion;
    private ExternalCoreApi.GetMemoryData? _getMemoryData;
    private ExternalCoreApi.GetMemorySize? _getMemorySize;
    private string? _conversionDirectory;
    private MachineFactory? _adapter;
    private MachineConfiguration? _configuration;

    internal ExternalCore(string corePath) => _corePath = corePath;

    public VideoFrame? LatestVideoFrame => _host?.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _host?.LatestAudioChunk;
    public bool TryDequeueAudio(out AudioChunk? chunk)
    {
        if (_host is not null) return _host.TryDequeueAudio(out chunk);
        chunk = null;
        return false;
    }
    public IReadOnlyList<CoreOption> Options => _host?.OptionCatalog ?? [];
    public IReadOnlyList<string> Diagnostics => _host?.Diagnostics ?? [];
    public IReadOnlyDictionary<int, bool> LedStates => _host?.LedStates ?? new Dictionary<int, bool>();
    public string CoreName { get; private set; } = string.Empty;
    public string CoreVersion { get; private set; } = string.Empty;
    public IReadOnlySet<string> SupportedContentExtensions { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public string CoreSha256 { get; private set; } = string.Empty;
    public double FramesPerSecond => _host?.FramesPerSecond ?? VideoConstants.DefaultFramesPerSecond;
    public int SampleRate => _host?.SampleRate ?? AudioConstants.DefaultSampleRate;
    public int DiskCount => _host?.DiskControl.ImageCount ?? BufferConstants.EmptyCollectionCount;
    public int CurrentDiskIndex => _host?.DiskControl.CurrentIndex ?? MediaConstants.NoSelectedDiskIndex;
    internal nuint GetMemorySize(uint id) => (_getMemorySize ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))(id);
    internal nint GetMemoryData(uint id) => (_getMemoryData ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))(id);

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        _configuration = configuration;
        _conversionDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.ConvertedMedia);
        ConfigurationValidationFunctions.ValidateForSave(configuration);
        foreach (var slot in EmulatorCatalog.CreateAdapter(configuration.Core).GetFirmwareSlots(configuration))
            if (slot.IsRequired && string.IsNullOrWhiteSpace(configuration.FirmwarePath(slot.FieldId)))
                throw new FileNotFoundException(slot.FieldId);
        var media = ResolveConfiguredMedia(configuration);
        var sourceCorePath = ResolveCorePath(_corePath);
        using (var coreStream = File.OpenRead(sourceCorePath)) CoreSha256 = CommonCoreHostFunctions.StateFiles.HashStream(coreStream);
        var adapter = (MachineFactory)EmulatorCatalog.CreateAdapter(configuration.Core);
        _adapter = adapter;
        var systemDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName);
        var contentPath = adapter.PrepareContent(configuration, sessionDirectory, media);
        var contentDirectory = contentPath is null ? Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName) : Path.GetDirectoryName(contentPath)!;
        saveDirectory = Path.GetFullPath(saveDirectory ?? Path.Combine(sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.Core);
        Directory.CreateDirectory(isolatedCoreDirectory);
        var corePath = Path.Combine(isolatedCoreDirectory, adapter.CoreDefinition.LibraryFile);
        File.Copy(sourceCorePath, corePath, true);

        ((IEmulatorFirmwareAdapter)adapter).PrepareFirmware(configuration, systemDirectory);

        var options = adapter.NativeOptions(configuration, media);
        _host = new ExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory, options, adapter.FirmwareOverrideOption, adapter.KeyboardOverrides(configuration), adapter.KeyboardExpansions(configuration), adapter.KeyboardToggleExpansions(configuration));

        try
        {
            _library = CommonCoreHostFunctions.NativeResources.CreateLibrary(corePath);
            var apiVersion = Export<ExternalCoreApi.GetApiVersion>(ExternalCoreConstants.RetroApiVersion)();
            if (apiVersion != ExternalCoreInteropConstants.ApiVersion)
                throw new NotSupportedException(CoreExceptions.UnsupportedApiVersion(apiVersion));
            Export<ExternalCoreApi.GetSystemInfo>(ExternalCoreConstants.RetroGetSystemInfo)(out var systemInfo);
            var libraryName = Marshal.PtrToStringUTF8(systemInfo.LibraryName);
            if (!string.Equals(libraryName, adapter.CoreDefinition.LibraryName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(CoreExceptions.LibraryIdentityMismatch(libraryName));
            if (!systemInfo.NeedFullPath)
                throw new InvalidDataException(CoreExceptions.FullContentPathsRequired());
            CoreName = libraryName!;
            CoreVersion = Marshal.PtrToStringUTF8(systemInfo.LibraryVersion) ?? string.Empty;
            var declaredExtensions = CommonCoreHostFunctions.NativeResources.ParseExtensions(systemInfo.ValidExtensions);
            SupportedContentExtensions = ((IEmulatorContentFormats)adapter).ResolveContentExtensions(declaredExtensions, systemInfo.NeedFullPath);
            if (contentPath is not null && !Directory.Exists(contentPath))
            {
                var extension = Path.GetExtension(contentPath).TrimStart(MediaConstants.ExtensionPrefix);
                if (extension.Length == BufferConstants.EmptyCollectionCount
                    || !SupportedContentExtensions.Contains(extension))
                {
                    contentPath = adapter.ConvertMediaPath(contentPath, _conversionDirectory);
                    if (!SupportedContentExtensions.Contains(Path.GetExtension(contentPath).TrimStart(MediaConstants.ExtensionPrefix)))
                        throw new InvalidDataException(CoreExceptions.UnsupportedContentExtension(extension));
                }
            }
            Export<ExternalCoreApi.SetEnvironment>(ExternalCoreConstants.RetroSetEnvironment)(_host.Environment);
            Export<ExternalCoreApi.SetVideo>(ExternalCoreConstants.RetroSetVideoRefresh)(_host.Video);
            Export<ExternalCoreApi.SetAudioSample>(ExternalCoreConstants.RetroSetAudioSample)(_host.AudioSample);
            Export<ExternalCoreApi.SetAudioBatch>(ExternalCoreConstants.RetroSetAudioSampleBatch)(_host.AudioBatch);
            Export<ExternalCoreApi.SetInputPoll>(ExternalCoreConstants.RetroSetInputPoll)(_host.InputPoll);
            Export<ExternalCoreApi.SetInputState>(ExternalCoreConstants.RetroSetInputState)(_host.InputState);

            _deinitialize = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroDeinit);
            _unloadGame = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroUnloadGame);
            _run = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroRun);
            _reset = Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroReset);
            _getSerializedSize = Export<ExternalCoreApi.GetSerializedSize>(ExternalCoreConstants.RetroSerializeSize);
            _serialize = Export<ExternalCoreApi.Serialize>(ExternalCoreConstants.RetroSerialize);
            _unserialize = Export<ExternalCoreApi.Serialize>(ExternalCoreConstants.RetroUnserialize);
            _getRegion = Export<ExternalCoreApi.GetRegion>(ExternalCoreConstants.RetroGetRegion);
            _getMemoryData = Export<ExternalCoreApi.GetMemoryData>(ExternalCoreConstants.RetroGetMemoryData);
            _getMemorySize = Export<ExternalCoreApi.GetMemorySize>(ExternalCoreConstants.RetroGetMemorySize);
            Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroInit)();
            _initialized = true;
            _host.ValidateConfiguredOptions();
            var setController = Export<ExternalCoreApi.SetControllerPortDevice>(ExternalCoreConstants.RetroSetControllerPortDevice);
            var model = ModelCatalog.Get(configuration.Model);
            var defaultController = ControllerCatalog.Default(model);
            for (var port = ControllerPortConstants.MinimumControllerPort;
                 port < adapter.ControllerPortCount(configuration); port++)
            {
                var controller = configuration.Controllers is { } controllers && port < controllers.Count ? controllers[port] : configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == port)?.Type ?? defaultController;
                if (controller == PeripheralCategory.Automatic) controller = defaultController;
                setController((uint)port, adapter.ControllerDevice(_host.ControllerPorts, port, controller));
            }

            ExternalCoreApi.LoadGame loadGame = Export<ExternalCoreApi.LoadGame>(ExternalCoreConstants.RetroLoadGame);
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException(CoreExceptions.StartWithoutMediaUnsupported());
                _gameLoaded = loadGame(nint.Zero);
            }
            else
            {
                _gameLoaded = LoadGame(loadGame, contentPath);
                if (!_gameLoaded)
                {
                    var converted = adapter.ConvertMediaPath(contentPath, _conversionDirectory);
                    if (!string.Equals(converted, contentPath, StringComparison.Ordinal))
                        _gameLoaded = LoadGame(loadGame, converted);
                }
            }

            if (!_gameLoaded) throw new InvalidOperationException(CoreExceptions.ContentRefused());
            Export<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreConstants.RetroGetSystemAvInfo)(out var av);
            _host.ApplyInitialAvInfo(av);
            Region = CommonCoreHostFunctions.Protocol.ReadRegion(unchecked((int)_getRegion!()), allowAllDefinedStandards: false);
            _supportsSaveStates = CommonCoreHostFunctions.MachineExecution.IsSupportedStateSize(_getSerializedSize!(), (nuint)SavedStateConstants.MaximumStateSize);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void RunFrame()
    {
        _host?.NotifyAudioBufferStatus();
        (_run ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))();
    }

    internal static IReadOnlyList<MediaConfiguration> ResolveConfiguredMedia(MachineConfiguration configuration)
    {
        if (configuration.Media is { Count: > BufferConstants.EmptyCollectionCount }) return configuration.Media;
        if (configuration.Floppies is { Count: > BufferConstants.EmptyCollectionCount })
            return configuration.Floppies.Select(floppy => new MediaConfiguration(floppy.Path, MediaCategory.Floppy, floppy.Label, floppy.IsReadOnly)).ToArray();
        return configuration.InitialDiskPath is null ? [] : [new MediaConfiguration(configuration.InitialDiskPath, InferMediaCategory(configuration, configuration.InitialDiskPath))];
    }

    internal static MediaCategory InferMediaCategory(MachineConfiguration configuration, string path) => ((MachineFactory)EmulatorCatalog.CreateAdapter(configuration.Core)).InferMediaCategory(configuration, path);
    public void HardReset() => (_reset ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    private void InsertMediaPath(string path)
    {
        var diskControl = (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized())).DiskControl;
        try { diskControl.Insert(path); }
        catch (Exception) when (_adapter is not null)
        {
            var converted = _adapter.ConvertMediaPath(path, _conversionDirectory ?? throw new InvalidOperationException());
            if (string.Equals(converted, path, StringComparison.Ordinal)) throw;
            diskControl.Insert(converted);
        }
    }
    private void EjectMediaPath() => (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized())).DiskControl.Eject();
    public void SelectDisk(int index) => (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized())).DiskControl.Select(index);

    private static bool LoadGame(ExternalCoreApi.LoadGame loadGame, string path)
    {
        using var nativePath = CommonCoreHostFunctions.NativeResources.CreateString(path);
        var game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = nativePath.Pointer }, game, false);
            return loadGame(game);
        }
        finally
        {
            Marshal.FreeHGlobal(game);
        }
    }

    /// <inheritdoc/>
    public void InsertMedia(MediaConfiguration media)
    {
        var index = media.SlotIndex ?? 0;
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(media));
        if (index < DiskCount && index != CurrentDiskIndex) SelectDisk(index);
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
        if (port < 0 || port >= (_adapter ?? throw new InvalidOperationException()).ControllerPortCount(configuration) || !Enum.IsDefined(peripheral)) throw new ArgumentOutOfRangeException(nameof(port));
        var device = (_adapter ?? throw new InvalidOperationException()).ControllerDevice(host.ControllerPorts, port, peripheral == PeripheralCategory.Automatic ? ControllerCatalog.Default(ModelCatalog.Get(configuration.Model)) : peripheral);
        Export<ExternalCoreApi.SetControllerPortDevice>(ExternalCoreConstants.RetroSetControllerPortDevice)((uint)port, device);
        host.Feedback.ResetPort(port);
    }

    public byte[] SaveState()
    {
        var size = (_getSerializedSize ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))();
        if (size == ExternalCoreInteropConstants.EmptyNativeSize
            || size > SavedStateConstants.MaximumStateSize)
            throw new InvalidOperationException(CoreExceptions.InvalidStateSize(size));
        var state = new byte[(int)size];
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            if (!_serialize!(buffer, size))
                throw new InvalidOperationException(CoreExceptions.StateSaveFailed());
            Marshal.Copy(buffer, state, BufferConstants.FirstBufferIndex, state.Length);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
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
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void SetOption(string key, string value) => (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized())).SetOption(key, value);

    public void Stop()
    {
        if (_gameLoaded) _unloadGame?.Invoke();
        _gameLoaded = false;
    }

    private T Export<T>(string name) where T : Delegate => (_library ?? throw new InvalidOperationException(CoreExceptions.CoreNotLoaded())).Resolve<T>(name);

    internal static uint ControllerDevice(IReadOnlyList<IReadOnlyList<ControllerDevice>> ports, int port, PeripheralCategory controller)
    {
        if (controller == PeripheralCategory.None) return ExternalCoreConstants.NoControllerDevice;
        var requestedName = controller switch
        {
            PeripheralCategory.Automatic => ExternalCoreConstants.Automatic,
            PeripheralCategory.RetroPad => ExternalCoreConstants.RetroPad,
            PeripheralCategory.Cd32Pad => ExternalCoreConstants.CD32Pad,
            PeripheralCategory.AnalogJoystick => ExternalCoreConstants.AnalogJoystick,
            PeripheralCategory.Joystick => ExternalCoreConstants.Joystick,
            PeripheralCategory.Keyboard => ExternalCoreConstants.Keyboard,
            _ => throw new ArgumentOutOfRangeException(nameof(controller))
        };
        var devices = port < ports.Count ? ports[port] : [];
        var selected = devices.FirstOrDefault(device => device.Name.Equals(requestedName, StringComparison.OrdinalIgnoreCase));
        if (selected is not null) return selected.Id;
        if (controller == PeripheralCategory.Automatic)
            return devices.FirstOrDefault(device => device.Name.Equals(ExternalCoreConstants.RetroPad, StringComparison.OrdinalIgnoreCase))?.Id ?? ExternalCoreConstants.DefaultJoypadDevice;
        throw new InvalidDataException(CoreExceptions.UnsupportedController(requestedName, port + ExternalCoreConstants.DisplayPortNumberOffset));
    }

    private static string ResolveCorePath(string configuredPath)
    {
        if (!Path.IsPathFullyQualified(configuredPath))
            throw new ArgumentException(CoreExceptions.CorePathNotAbsolute(), nameof(configuredPath));
        if (!File.Exists(configuredPath))
            throw new FileNotFoundException(CoreExceptions.CoreNotFound(), configuredPath);
        return configuredPath;
    }

    public void Dispose()
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
            _getRegion = null;
            _getMemoryData = null;
            _getMemorySize = null;
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
