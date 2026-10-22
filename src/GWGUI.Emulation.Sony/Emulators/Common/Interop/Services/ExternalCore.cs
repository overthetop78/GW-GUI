using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using SemanticControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
using SemanticPortConstants = GWGUI.Emulation.Sony.Common.Constants.ControllerPortConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore : IEmulatorCore
{
    private readonly string _corePath;
    private readonly CoreDefinition _definition;
    private IEmulationCoreLibrary? _library;
    private ExternalHostCallbacks? _host;
    private ExternalCoreApi.VoidCall? _deinitialize;
    private ExternalCoreApi.VoidCall? _unloadGame;
    private ExternalCoreApi.VoidCall? _run;
    private ExternalCoreApi.VoidCall? _reset;
    private ExternalCoreApi.SetControllerPortDevice? _setControllerPortDevice;
    private ExternalCoreApi.GetSerializedSize? _getSerializedSize;
    private ExternalCoreApi.Serialize? _serialize;
    private ExternalCoreApi.Serialize? _unserialize;
    private MachineConfiguration? _configuration;
    private string? _sessionDirectory;
    private string? _saveDirectory;
    private bool _gameLoaded;
    private bool _initialized;
    private bool _disposing;
    private IReadOnlyList<CoreControllerPort>? _controllerPorts;
    private IReadOnlyList<(string Source, string Native, int Size)> _firmwareFiles = [];

    internal ExternalCore(CoreDefinition definition, string corePath)
    {
        _definition = definition;
        _corePath = corePath;
    }

    public VideoFrame? LatestVideoFrame => _host?.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _host?.LatestAudioChunk;
    public IReadOnlyList<CoreOption> Options => _host?.OptionCatalog ?? [];
    public IReadOnlyList<string> Diagnostics => _host?.Diagnostics ?? [];
    public IReadOnlyDictionary<int, bool> LedStates => _host?.LedStates ?? new Dictionary<int, bool>();
    public bool ShutdownRequested => _host?.ShutdownRequested ?? false;
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback => (_host?.Feedback.Snapshot() ?? [])
        .Select(state => state with { Port = _controllerPorts?.FirstOrDefault(port => port.NativeDevice == ExternalHostCallbacksConstants.JoypadDevice && port.NativePort == state.Port)?.ConfigurationPort ?? state.Port }).ToArray();
    public string CoreName { get; private set; } = string.Empty;
    public string CoreVersion { get; private set; } = string.Empty;
    public IReadOnlySet<string> SupportedContentExtensions { get; private set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public string CoreSha256 { get; private set; } = string.Empty;
    public double FramesPerSecond => _host?.FramesPerSecond ?? ExternalCoreConstants.FallbackFramesPerSecond;
    public int SampleRate => _host?.SampleRate ?? ExternalCoreConstants.FallbackSampleRate;
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
        configuration = _definition.PrepareRuntimeConfiguration?.Invoke(configuration) ?? configuration;
        saveDirectory = _definition.ResolveSaveDirectory?.Invoke(configuration, saveDirectory) ?? saveDirectory;
        _configuration = configuration;
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        _saveMemoryReady = false;
        var media = _definition.ResolveContentMedia?.Invoke(configuration) ?? ResolveConfiguredMedia(configuration);
        var contentPlan = _definition.CreateContentLoadPlan?.Invoke(configuration);
        foreach (var item in media)
            if (!File.Exists(item.Path))
                throw new FileNotFoundException(Error(CoreExceptionsResourceConstants.MediaNotFound), item.Path);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var stream = File.OpenRead(sourceCorePath))
            CoreSha256 = CommonCoreHostFunctions.StateFiles.HashStream(stream);

        saveDirectory = Path.GetFullPath(saveDirectory ?? Path.Combine(sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
        var coreSaveDirectory = Path.Combine(saveDirectory, _definition.Id);
        var (systemDirectory, contentDirectory) = CoreDirectoryFunctions.NativeDataDirectories(_definition, sessionDirectory, saveDirectory);
        systemDirectory = _definition.ResolveSystemDirectory?.Invoke(sourceCorePath) ?? systemDirectory;
        _saveMemoryRegions = (contentPlan?.SaveMemoryRegions ?? [new CoreSaveMemoryRegion(SaveMemoryConstants.SaveRamId, SaveMemoryConstants.FileName)])
                .Select(region => (region.Id, Path.Combine(coreSaveDirectory, region.FileName))).ToArray();
        Directory.CreateDirectory(systemDirectory);
        FirmwareConfigurationFunctions.CopySelectedFiles(configuration, new Engine().Adapter(configuration), systemDirectory);
        _definition.PrepareSystemFiles?.Invoke(configuration, systemDirectory);
        _firmwareFiles = _definition.FirmwarePersistence is { } persistence
            ? FirmwareRuntimeFunctions.Prepare(configuration, _definition.Id, persistence, systemDirectory, saveDirectory)
            : [];
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.CoreDirectory);
        Directory.CreateDirectory(isolatedCoreDirectory);
        var isolatedCorePath = Path.Combine(isolatedCoreDirectory, _definition.LibraryFileName);
        File.Copy(sourceCorePath, isolatedCorePath, true);
        var contentPath = PrepareContentPath(media, contentDirectory);
        var nativeSaveDirectory = PrepareMemoryCards(configuration, saveDirectory, systemDirectory);
        _controllerPorts = _definition.CreateControllerPorts?.Invoke(configuration);
        var nativeSystemDirectory = _definition.ResolveNativeSystemDirectory?.Invoke(configuration, systemDirectory);
        Func<string, (string Path, bool IsReadOnly)>? pathResolver = nativeSystemDirectory is null || nativeSystemDirectory == systemDirectory || _definition.ResolveVirtualPath is null
            ? null : path => _definition.ResolveVirtualPath(configuration, systemDirectory, path, _host?.CheatsEnabled);
        _host = new ExternalHostCallbacks(systemDirectory, contentDirectory, nativeSaveDirectory, configuration.Options ?? new Dictionary<string, string>(),
            _definition.StartupFailureDiagnostics, _controllerPorts, _definition.Id, nativeSystemDirectory, pathResolver);

        try
        {
            _library = CommonCoreHostFunctions.NativeResources.CreateLibrary(isolatedCorePath);
            var apiVersion = Export<ExternalCoreApi.GetApiVersion>(ExternalCoreConstants.RetroApiVersion)();
            if (apiVersion != ExternalCoreInteropConstants.ApiVersion)
                throw new NotSupportedException(Error(CoreExceptionsResourceConstants.UnsupportedApiVersion, apiVersion));
            Export<ExternalCoreApi.GetSystemInfo>(ExternalCoreConstants.RetroGetSystemInfo)(out var info);
            var libraryName = Marshal.PtrToStringUTF8(info.LibraryName);
            if (!CoreIdentityFunctions.Accepts(_definition, libraryName))
                throw new InvalidDataException(Error(CoreExceptionsResourceConstants.LibraryIdentityMismatch, libraryName ?? string.Empty));
            if (!info.NeedFullPath && _definition.FullContentPathsRequired is { } fullPathError)
                throw new InvalidDataException(fullPathError());
            CoreName = libraryName!;
            CoreVersion = Marshal.PtrToStringUTF8(info.LibraryVersion) ?? string.Empty;
            SupportedContentExtensions = CommonCoreHostFunctions.NativeResources.ParseExtensions(info.ValidExtensions);
            ValidateExtension(contentPath);
            foreach (var path in contentPlan?.Paths ?? []) ValidateExtension(path);

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
            _getMemoryData = Export<ExternalCoreApi.GetMemoryData>(ExternalCoreConstants.RetroGetMemoryData);
            _getMemorySize = Export<ExternalCoreApi.GetMemorySize>(ExternalCoreConstants.RetroGetMemorySize);
            Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroInit)();
            _initialized = true;
            _host.ValidateConfiguredOptions();
            _setControllerPortDevice = Export<ExternalCoreApi.SetControllerPortDevice>(ExternalCoreConstants.RetroSetControllerPortDevice);
            ApplyConfiguredControllers(requireAdvertised: false);

            var loadGame = Export<ExternalCoreApi.LoadGame>(ExternalCoreConstants.RetroLoadGame);
            if (contentPlan is not null)
                _gameLoaded = LoadSubsystem(contentPlan, info.NeedFullPath);
            else if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.StartWithoutMediaUnsupported));
                _gameLoaded = loadGame(0);
            }
            else _gameLoaded = LoadGame(loadGame, contentPath, info.NeedFullPath);
            _host.DiagnosticMonitor.ThrowIfFailed();
            if (!_gameLoaded) throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.ContentRefused));
            _host.ValidateConfiguredOptions();
            ApplyConfiguredControllers(requireAdvertised: true);
            RestoreSaveMemory();
            _saveMemoryReady = true;
            Export<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreConstants.RetroGetSystemAvInfo)(out var av);
            _host.ApplyInitialAvInfo(av);
            _host.HardwareRender.Reset();
            _host.DiagnosticMonitor.ThrowIfFailed();
        }
        catch (Exception error)
        {
            var startupError = CoreDiagnosticMonitor.WithNativeDiagnostics(error, _host?.Diagnostics ?? []);
            try { Dispose(); }
            catch (Exception cleanupError) { throw new AggregateException(startupError, cleanupError); }
            if (!ReferenceEquals(startupError, error)) throw startupError;
            throw;
        }
    }

    internal static IReadOnlyList<MediaConfiguration> ResolveConfiguredMedia(MachineConfiguration configuration) => (configuration.Media ?? []).Where(item => item.IsInserted).OrderBy(item => item.MountOrder).ToArray();

    internal static string? PrepareContentPath(IReadOnlyList<MediaConfiguration> media, string contentDirectory)
    {
        if (media.Count == 0) return null;
        return Path.GetFullPath(media[0].Path);
    }

    public void RunFrame()
    {
        (_run ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized)))();
        _host?.DiagnosticMonitor.ThrowIfFailed();
    }
    public void HardReset()
    {
        var configuration = _configuration ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized));
        var sessionDirectory = _sessionDirectory ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized));
        var saveDirectory = _saveDirectory;
        Dispose();
        Initialize(configuration, sessionDirectory, saveDirectory);
    }
    public void SoftReset() => (_reset ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized)))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    public void SetControllerPortDevice(int port, PeripheralCategory type)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized));
        var host = _host ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized));
        var setter = _setControllerPortDevice ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized));
        SemanticControllerFunctions.Validate(configuration, port, type);
        var route = _controllerPorts?.Single(item => item.ConfigurationPort == port);
        if (_gameLoaded && route is { CanChangeWhileRunning: false } && SemanticControllerFunctions.Resolve(configuration, port) != type)
            throw new NotSupportedException();
        var nativePort = route?.NativePort ?? (uint)port;
        if (route is null || route.NativeDevice == ExternalHostCallbacksConstants.JoypadDevice)
        {
            var nativeId = ControllerPortFunctions.Resolve(type, checked((int)nativePort), host.ControllerPorts);
            setter(nativePort, nativeId);
            host.Feedback.ResetPort(checked((int)nativePort));
        }
        host.SetControllerType(port, type);
        _configuration = SemanticControllerFunctions.WithControllerType(configuration, port, type);
    }

    private void ApplyConfiguredControllers(bool requireAdvertised)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized));
        for (var port = SemanticPortConstants.MinimumControllerPort;
            port < SemanticControllerFunctions.PortCount(configuration); port++)
        {
            var type = SemanticControllerFunctions.Resolve(configuration, port);
            if (!requireAdvertised && !_host!.ControllerPorts.Any()
                && type is not (PeripheralCategory.Joystick or PeripheralCategory.DanceMat or PeripheralCategory.ArcadeStick or PeripheralCategory.None))
                continue;
            SetControllerPortDevice(port, type);
        }
        if (configuration.EmulatorId == Emulators.Rpcs3.Constants.CoreConstants.Id)
            for (var port = SemanticControllerFunctions.PortCount(configuration); port < 7; port++)
            {
                _setControllerPortDevice!((uint)port, 0u);
                _host!.SetControllerType(port, PeripheralCategory.None);
            }
    }
    public void InsertMedia(string path) => (_host ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized))).DiskControl.Insert(path);
    public void EjectMedia() => (_host ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized))).DiskControl.Eject();
    public void SelectDisk(int index) => (_host ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized))).DiskControl.Select(index);

    public byte[] SaveState()
    {
        var size = (_getSerializedSize ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized)))();
        if (size == ExternalCoreInteropConstants.EmptyNativeSize || size > SavedStateConstants.MaximumStateSize)
            throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.InvalidStateSize, size));
        var state = new byte[(int)size];
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            if (!_serialize!(buffer, size))
                throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.StateSaveFailed));
            Marshal.Copy(buffer, state, BufferConstants.FirstBufferIndex, state.Length);
        }
        finally { Marshal.FreeHGlobal(buffer); }
        return state;
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (state.IsEmpty) throw new ArgumentException(Error(CoreExceptionsResourceConstants.StateEmpty), nameof(state));
        var bytes = state.ToArray();
        var buffer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, buffer, bytes.Length);
            if (!_unserialize!(buffer, (nuint)bytes.Length))
                throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.StateRestoreFailed));
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    public void SetOption(string key, string value) => (_host ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotInitialized))).SetOption(key, value);

    public void Stop()
    {
        if (!_gameLoaded) return;
        _gameLoaded = false;
        try { PersistSaveMemory(); }
        finally
        {
            try
            {
                try { _host?.HardwareRender.NotifyDestroy(); }
                finally { _unloadGame?.Invoke(); }
            }
            finally
            {
                try { PersistMemoryCards(); }
                finally
                {
                    _saveMemoryReady = false;
                    _memoryCardFiles.Clear();
                }
            }
        }
    }

    private void ValidateExtension(string? path)
    {
        if (path is null) return;
        var extension = Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix);
        if (!SupportedContentExtensions.Contains(extension))
            throw new InvalidDataException(Error(CoreExceptionsResourceConstants.UnsupportedContentExtension, extension));
    }

    private bool LoadGame(ExternalCoreApi.LoadGame loadGame, string path, bool needFullPath)
    {
        var nativePath = AllocateContentPath(path);
        var game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
        try
        {
            var content = needFullPath ? null : File.ReadAllBytes(path);
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo
            {
                Path = nativePath.Pointer,
                Data = content is null ? nint.Zero : AllocateContent(content),
                Size = content is null ? ExternalCoreInteropConstants.EmptyNativeSize : (nuint)content.Length
            }, game, false);
            return loadGame(game);
        }
        finally { Marshal.FreeHGlobal(game); }
    }

    private string Error(string key, params object[] arguments) => CoreExceptions.ForEmulator(_definition.Id, key, arguments);

    private T Export<T>(string name) where T : Delegate => (_library ?? throw new InvalidOperationException(Error(CoreExceptionsResourceConstants.CoreNotLoaded))).Resolve<T>(name);

    private string ResolveCorePath(string path)
    {
        if (!Path.IsPathFullyQualified(path))
            throw new ArgumentException(Error(CoreExceptionsResourceConstants.CorePathNotAbsolute), nameof(path));
        if (!File.Exists(path)) throw new FileNotFoundException(Error(CoreExceptionsResourceConstants.CoreNotFound), path);
        return path;
    }

    public void Dispose()
    {
        if (_disposing) return;
        _disposing = true;
        try
        {
            var callbacks = _host;
            try { _host?.HardwareRender.NotifyDestroy(); }
            finally
            {

                try { Stop(); }
                finally
                {
                    try
                    {
                        if (_initialized) _deinitialize?.Invoke();
                    }
                    finally
                    {
                        _initialized = false;
                        _deinitialize = null;
                        _unloadGame = null;
                        _run = null;
                        _reset = null;
                        _setControllerPortDevice = null;
                        _getSerializedSize = null;
                        _serialize = null;
                        _unserialize = null;
                        _getMemoryData = null;
                        _getMemorySize = null;
                        try { FirmwareRuntimeFunctions.Persist(_firmwareFiles); }
                        finally
                        {
                            _firmwareFiles = [];
                            try { _host?.Dispose(); }
                            finally
                            {
                                _host = null;
                                try { _library?.Dispose(); }
                                finally
                                {
                                    GC.KeepAlive(callbacks);
                                    _library = null;
                                    ReleaseContentBuffers();
                                }
                            }
                        }
                    }
                }
            }

        }
        finally { _disposing = false; }
    }
}
