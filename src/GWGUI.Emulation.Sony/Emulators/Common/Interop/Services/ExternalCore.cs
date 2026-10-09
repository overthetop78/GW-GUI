using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using SemanticControllerFunctions = GWGUI.Emulation.Sony.Common.Machines.Common.Functions.ControllerFunctions;
using SemanticPortConstants = GWGUI.Emulation.Sony.Common.Constants.ControllerPortConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore : IEmulatorCore
{
    private readonly string _corePath;
    private readonly CoreDefinition _definition;
    private ExternalCoreLibrary? _library;
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
    private nint _contentBuffer;
    private bool _gameLoaded;
    private bool _initialized;

    internal ExternalCore(CoreDefinition definition, string corePath)
    {
        _definition = definition;
        _corePath = corePath;
    }

    public VideoFrame? LatestVideoFrame => _host?.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _host?.LatestAudioChunk;
    public IReadOnlyList<CoreOption> Options => _host?.OptionCatalog ?? [];
    public IReadOnlyList<string> Diagnostics => _host?.Diagnostics ?? [];
    public IReadOnlyDictionary<int, bool> LedStates => _host?.LedStates
        ?? new Dictionary<int, bool>();
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

    public void Initialize(MachineConfiguration configuration, string sessionDirectory,
        string? saveDirectory = null)
    {
        _configuration = configuration;
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        _saveMemoryReady = false;
        var media = ResolveConfiguredMedia(configuration);
        foreach (var item in media)
            if (!File.Exists(item.Path))
                throw new FileNotFoundException(CoreExceptions.MediaNotFound(), item.Path);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var stream = File.OpenRead(sourceCorePath))
            CoreSha256 = Convert.ToHexString(SHA256.HashData(stream));

        var contentDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName);
        saveDirectory = Path.GetFullPath(saveDirectory
            ?? Path.Combine(sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
        var coreSaveDirectory = Path.Combine(saveDirectory, _definition.Id);
        var systemDirectory = Path.Combine(coreSaveDirectory, CoreDirectoryConstants.SystemDirectoryName);
        _saveMemoryPath = Path.Combine(coreSaveDirectory, SaveMemoryConstants.FileName);
        Directory.CreateDirectory(systemDirectory);
        FirmwareConfigurationFunctions.CopySelectedFiles(configuration,
            new Engine().Adapter(configuration), systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.CoreDirectory);
        Directory.CreateDirectory(isolatedCoreDirectory);
        var isolatedCorePath = Path.Combine(isolatedCoreDirectory, _definition.LibraryFileName);
        File.Copy(sourceCorePath, isolatedCorePath, true);
        var contentPath = PrepareContentPath(media, contentDirectory);
        _host = new ExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory,
            configuration.Options ?? new Dictionary<string, string>());

        try
        {
            _library = new ExternalCoreLibrary(isolatedCorePath);
            var apiVersion = Export<ExternalCoreApi.GetApiVersion>(
                ExternalCoreConstants.RetroApiVersion)();
            if (apiVersion != ExternalCoreInteropConstants.ApiVersion)
                throw new NotSupportedException(CoreExceptions.UnsupportedApiVersion(apiVersion));
            Export<ExternalCoreApi.GetSystemInfo>(ExternalCoreConstants.RetroGetSystemInfo)(out var info);
            var libraryName = Marshal.PtrToStringUTF8(info.LibraryName);
            if (!string.Equals(libraryName, _definition.LibraryName,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(CoreExceptions.LibraryIdentityMismatch(libraryName));
            CoreName = libraryName!;
            CoreVersion = Marshal.PtrToStringUTF8(info.LibraryVersion) ?? string.Empty;
            SupportedContentExtensions = (Marshal.PtrToStringUTF8(info.ValidExtensions) ?? string.Empty)
                .Split(MediaConstants.SupportedExtensionSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            ValidateExtension(contentPath);

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
            _setControllerPortDevice = Export<ExternalCoreApi.SetControllerPortDevice>(
                ExternalCoreConstants.RetroSetControllerPortDevice);
            ApplyConfiguredControllers(requireAdvertised: false);

            var loadGame = Export<ExternalCoreApi.LoadGame>(ExternalCoreConstants.RetroLoadGame);
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException(CoreExceptions.StartWithoutMediaUnsupported());
                _gameLoaded = loadGame(0);
            }
            else _gameLoaded = LoadGame(loadGame, contentPath, info.NeedFullPath);
            if (!_gameLoaded) throw new InvalidOperationException(CoreExceptions.ContentRefused());
            _host.ValidateConfiguredOptions();
            ApplyConfiguredControllers(requireAdvertised: true);
            RestoreSaveMemory();
            _saveMemoryReady = true;
            Export<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreConstants.RetroGetSystemAvInfo)(out var av);
            _host.ApplyInitialAvInfo(av);
            _host.ResetHardwareContext();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    internal static IReadOnlyList<MediaConfiguration> ResolveConfiguredMedia(
        MachineConfiguration configuration) => (configuration.Media ?? [])
        .Where(item => item.IsInserted).OrderBy(item => item.MountOrder).ToArray();

    internal static string? PrepareContentPath(IReadOnlyList<MediaConfiguration> media,
        string contentDirectory)
    {
        if (media.Count == 0) return null;
        return Path.GetFullPath(media[0].Path);
    }

    public void RunFrame() => (_run
        ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))();
    public void HardReset()
    {
        var configuration = _configuration
            ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized());
        var sessionDirectory = _sessionDirectory
            ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized());
        var saveDirectory = _saveDirectory;
        Dispose();
        Initialize(configuration, sessionDirectory, saveDirectory);
    }
    public void SoftReset() => (_reset
        ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    public void SetControllerPortDevice(int port, ControllerType type)
    {
        var configuration = _configuration
            ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized());
        var host = _host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized());
        var setter = _setControllerPortDevice
            ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized());
        SemanticControllerFunctions.Validate(configuration, port, type);
        var nativeId = ControllerPortFunctions.Resolve(type, port, host.ControllerPorts);
        setter((uint)port, nativeId);
        host.SetControllerType(port, type);
        _configuration = SemanticControllerFunctions.WithControllerType(configuration, port, type);
    }

    private void ApplyConfiguredControllers(bool requireAdvertised)
    {
        var configuration = _configuration
            ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized());
        for (var port = SemanticPortConstants.MinimumControllerPort;
            port < SemanticControllerFunctions.PortCount(configuration); port++)
        {
            var type = SemanticControllerFunctions.Resolve(configuration, port);
            if (!requireAdvertised && !_host!.ControllerPorts.Any()
                && type is not (ControllerType.Joystick or ControllerType.None))
                continue;
            SetControllerPortDevice(port, type);
        }
    }
    public void InsertMedia(string path) => (_host
        ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))
        .DiskControl.Insert(path);
    public void EjectMedia() => (_host
        ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))
        .DiskControl.Eject();
    public void SelectDisk(int index) => (_host
        ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))
        .DiskControl.Select(index);

    public byte[] SaveState()
    {
        var size = (_getSerializedSize
            ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))();
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

    public void SetOption(string key, string value) => (_host
        ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))
        .SetOption(key, value);

    public void Stop()
    {
        if (!_gameLoaded) return;
        try { PersistSaveMemory(); }
        finally
        {
            try { _unloadGame?.Invoke(); }
            finally
            {
                _gameLoaded = false;
                _saveMemoryReady = false;
            }
        }
    }

    private void ValidateExtension(string? path)
    {
        if (path is null) return;
        var extension = Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix);
        if (!SupportedContentExtensions.Contains(extension))
            throw new InvalidDataException(CoreExceptions.UnsupportedContentExtension(extension));
    }

    private bool LoadGame(ExternalCoreApi.LoadGame loadGame, string path, bool needFullPath)
    {
        using var nativePath = new ExternalCoreUtf8String(path);
        var game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
        try
        {
            var content = needFullPath ? null : File.ReadAllBytes(path);
            if (content is not null)
            {
                _contentBuffer = Marshal.AllocHGlobal(content.Length);
                Marshal.Copy(content, BufferConstants.FirstBufferIndex, _contentBuffer, content.Length);
            }
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo
            {
                Path = nativePath.Pointer,
                Data = _contentBuffer,
                Size = content is null ? ExternalCoreInteropConstants.EmptyNativeSize : (nuint)content.Length
            }, game, false);
            return loadGame(game);
        }
        finally { Marshal.FreeHGlobal(game); }
    }

    private T Export<T>(string name) where T : Delegate => (_library
        ?? throw new InvalidOperationException(CoreExceptions.CoreNotLoaded())).Resolve<T>(name);

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
                    try { _host?.Dispose(); }
                    finally
                    {
                        _host = null;
                        try { _library?.Dispose(); }
                        finally
                        {
                            _library = null;
                            if (_contentBuffer != nint.Zero)
                            {
                                Marshal.FreeHGlobal(_contentBuffer);
                                _contentBuffer = nint.Zero;
                            }
                        }
                    }
                }
            }
        }
    }
}
