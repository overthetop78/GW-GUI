using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Dictionaries;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

internal sealed class ExternalCore : IEmulatorCore
{
    private readonly string _corePath;
    private readonly List<nint> _contentBuffers = [];
    private ExternalCoreLibrary? _library;
    private ExternalHostCallbacks? _host;
    private ExternalCoreApi.VoidCall? _deinitialize;
    private ExternalCoreApi.VoidCall? _unloadGame;
    private ExternalCoreApi.VoidCall? _run;
    private ExternalCoreApi.VoidCall? _reset;
    private ExternalCoreApi.GetSerializedSize? _getSerializedSize;
    private ExternalCoreApi.Serialize? _serialize;
    private ExternalCoreApi.Serialize? _unserialize;
    private ExternalCoreApi.GetMemoryData? _getMemoryData;
    private ExternalCoreApi.GetMemorySize? _getMemorySize;
    private bool _saveRamReady;
    private MachineConfiguration? _configuration;
    private string? _sessionDirectory;
    private string? _saveDirectory;
    private bool _gameLoaded;
    private bool _initialized;

    internal ExternalCore(string corePath) => _corePath = corePath;

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

    public void Initialize(MachineConfiguration configuration, string sessionDirectory,
        string? saveDirectory = null)
    {
        _configuration = configuration;
        var definition = CoreCatalog.Get(configuration.EmulatorId);
        var expectedLibraryName = definition.LibraryName;
        var isolatedLibraryName = definition.LibraryFileName;
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        var media = ResolveConfiguredMedia(configuration);
        foreach (var item in media)
            if (!File.Exists(item.Path))
                throw new FileNotFoundException(CoreExceptions.MediaNotFound(), item.Path);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var stream = File.OpenRead(sourceCorePath))
            CoreSha256 = Convert.ToHexString(SHA256.HashData(stream));

        var systemDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName);
        var contentDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName);
        saveDirectory = Path.GetFullPath(saveDirectory
            ?? Path.Combine(sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
        _saveDirectory = saveDirectory;
        Directory.CreateDirectory(systemDirectory);
        FirmwareFunctions.Prepare(configuration, systemDirectory);
        if (configuration.EmulatorId != GeargrafxConstants.Id
            && !string.IsNullOrWhiteSpace(configuration.FirmwarePath))
            File.Copy(configuration.FirmwarePath,
                Path.Combine(systemDirectory, FirmwareCatalogConstants.PcEngineSystemCardFileName), true);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.CoreDirectory);
        Directory.CreateDirectory(isolatedCoreDirectory);
        var isolatedCorePath = Path.Combine(isolatedCoreDirectory, isolatedLibraryName);
        File.Copy(sourceCorePath, isolatedCorePath, true);
        var contentPath = definition.PrepareContent is null
            ? PrepareContentPath(media, contentDirectory)
            : definition.PrepareContent(media, contentDirectory, systemDirectory);
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
            if (!string.Equals(libraryName, expectedLibraryName,
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
            var setController = Export<ExternalCoreApi.SetControllerPortDevice>(
                ExternalCoreConstants.RetroSetControllerPortDevice);
            for (var port = 0; port < ModelCatalog.Get(configuration.Model).ControllerPortCount; port++)
                setController((uint)port, configuration.Input?.ControllerBindings?
                    .Any(binding => binding.Port == port && ControllerCatalog.IsMouse(binding.Type)) == true
                    ? ExternalHostCallbacksConstants.MouseDevice : ExternalCoreConstants.JoypadDevice);

            var loadGame = Export<ExternalCoreApi.LoadGame>(ExternalCoreConstants.RetroLoadGame);
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new GWGUI.Emulation.Nec.Common.Exceptions.CoreMediaRequiredException(
                        CoreExceptions.StartWithoutMediaUnsupported());
                _gameLoaded = loadGame(0);
            }
            else if (definition.Subsystems?.TryGetValue(media.Count, out var subsystem) == true)
                _gameLoaded = LoadSpecial(subsystem, media);
            else _gameLoaded = LoadGame(loadGame, contentPath, info.NeedFullPath);
            if (!_gameLoaded) throw new InvalidOperationException(CoreExceptions.ContentRefused());
            if (configuration.EmulatorId == GeargrafxConstants.Id)
                GeargrafxSaveRamFunctions.Load(_getMemoryData, _getMemorySize, saveDirectory);
            else CoreSaveRamFunctions.Load(_getMemoryData, _getMemorySize, saveDirectory);
            _saveRamReady = true;
            Export<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreConstants.RetroGetSystemAvInfo)(out var av);
            _host.ApplyInitialAvInfo(av);
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
        return media.Count == 0 ? null : Path.GetFullPath(media[0].Path);
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
    public void InsertMedia(string path)
    {
        if (_configuration?.EmulatorId == GeargrafxConstants.Id)
            throw new NotSupportedException(CoreExceptions.MediaRefused());
        (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))
            .DiskControl.Insert(path);
    }
    public void EjectMedia()
    {
        EnsureDiskControlSupported();
        (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))
            .DiskControl.Eject();
    }
    public void SelectDisk(int index)
    {
        EnsureDiskControlSupported();
        (_host ?? throw new InvalidOperationException(CoreExceptions.CoreNotInitialized()))
            .DiskControl.Select(index);
    }

    private void EnsureDiskControlSupported()
    {
        if (_configuration?.EmulatorId != GeargrafxConstants.Id) return;
        var current = ResolveConfiguredMedia(_configuration).FirstOrDefault();
        if (current is null || !Path.GetExtension(current.Path).Equals(
                GeargrafxConstants.MmiExtension, StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException(CoreExceptions.DiskControlUnavailable());
    }

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
        try
        {
            if (_saveRamReady && _saveDirectory is { } directory
                && _getMemoryData is not null && _getMemorySize is not null)
            {
                if (_configuration?.EmulatorId == GeargrafxConstants.Id)
                    GeargrafxSaveRamFunctions.Save(_getMemoryData, _getMemorySize, directory);
                else CoreSaveRamFunctions.Save(_getMemoryData, _getMemorySize, directory);
            }
        }
        finally
        {
            try { _unloadGame?.Invoke(); }
            finally { _gameLoaded = false; _saveRamReady = false; }
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
        nint content = nint.Zero;
        try
        {
            var bytes = needFullPath ? [] : File.ReadAllBytes(path);
            if (!needFullPath)
            {
                content = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, content, bytes.Length);
            }
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo
                { Path = nativePath.Pointer, Data = content, Size = (nuint)bytes.Length },
                game, false);
            var loaded = loadGame(game);
            if (loaded && content != nint.Zero)
            {
                _contentBuffers.Add(content);
                content = nint.Zero;
            }
            return loaded;
        }
        finally
        {
            Marshal.FreeHGlobal(content);
            Marshal.FreeHGlobal(game);
        }
    }

    private bool LoadSpecial(uint type, IReadOnlyList<MediaConfiguration> media)
    {
        var paths = new List<ExternalCoreUtf8String>();
        var itemSize = Marshal.SizeOf<ExternalCoreApi.GameInfo>();
        var games = Marshal.AllocHGlobal(checked(itemSize * media.Count));
        try
        {
            for (var index = BufferConstants.FirstBufferIndex; index < media.Count; index++)
            {
                var path = new ExternalCoreUtf8String(Path.GetFullPath(media[index].Path));
                paths.Add(path);
                Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = path.Pointer },
                    games + index * itemSize, false);
            }
            return Export<SubsystemContracts.LoadGameSpecial>(ExternalCoreConstants.RetroLoadGameSpecial)
                (type, games, (nuint)media.Count);
        }
        finally
        {
            foreach (var path in paths) path.Dispose();
            Marshal.FreeHGlobal(games);
        }
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
        try { Stop(); }
        finally
        {
            try
            {
                if (_initialized) _deinitialize?.Invoke();
            }
            finally
            {
                foreach (var buffer in _contentBuffers) Marshal.FreeHGlobal(buffer);
                _contentBuffers.Clear();
                _initialized = false;
                _deinitialize = null;
                _unloadGame = null;
                _run = null;
                _reset = null;
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
                    finally { _library = null; }
                }
            }
        }
    }
}

