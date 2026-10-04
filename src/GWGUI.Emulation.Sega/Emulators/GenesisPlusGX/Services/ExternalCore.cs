using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation.Sega.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Sega.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Contracts;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Constants;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Functions;

namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Services;

internal sealed class ExternalCore : IEmulatorCore
{
    private readonly string _corePath;
    private readonly string _expectedLibraryName;
    private readonly bool _stageSc3000;
    private ExternalCoreLibrary? _library;
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

    internal ExternalCore(string corePath, string? expectedLibraryName = null, bool stageSc3000 = true)
    {
        _corePath = corePath;
        _expectedLibraryName = string.IsNullOrWhiteSpace(expectedLibraryName)
            ? GenesisPlusGXConstants.LibraryName
            : expectedLibraryName;
        _stageSc3000 = stageSc3000;
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
        _sessionDirectory = Path.GetFullPath(sessionDirectory);
        _saveDirectory = saveDirectory is null ? null : Path.GetFullPath(saveDirectory);
        var media = ResolveConfiguredMedia(configuration);
        foreach (var item in media)
            if (!File.Exists(item.Path))
                throw new FileNotFoundException(ExternalCoreExceptions.MediaNotFound(), item.Path);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var stream = File.OpenRead(sourceCorePath))
            CoreSha256 = Convert.ToHexString(SHA256.HashData(stream));

        var systemDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName);
        var contentDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName);
        saveDirectory = Path.GetFullPath(saveDirectory
            ?? Path.Combine(sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        PrepareFirmware(configuration, systemDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.CoreDirectory);
        Directory.CreateDirectory(isolatedCoreDirectory);
        var isolatedCorePath = Path.Combine(isolatedCoreDirectory, ExternalCoreConstants.LibraryName);
        File.Copy(sourceCorePath, isolatedCorePath, true);
        var contentPath = _stageSc3000
            ? PrepareContentPath(media, contentDirectory, configuration.Model)
            : PrepareContentPath(media);
        _host = new ExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory,
            configuration.Options ?? new Dictionary<string, string>());

        try
        {
            _library = new ExternalCoreLibrary(isolatedCorePath);
            var apiVersion = Export<ExternalCoreApi.GetApiVersion>(
                ExternalCoreConstants.RetroApiVersion)();
            if (apiVersion != ExternalCoreInteropConstants.ApiVersion)
                throw new NotSupportedException(ExternalCoreExceptions.UnsupportedApiVersion(apiVersion));
            Export<ExternalCoreApi.GetSystemInfo>(ExternalCoreConstants.RetroGetSystemInfo)(out var info);
            var libraryName = Marshal.PtrToStringUTF8(info.LibraryName);
            if (!string.Equals(libraryName, _expectedLibraryName,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(ExternalCoreExceptions.LibraryIdentityMismatch(libraryName));
            if (!info.NeedFullPath)
                throw new InvalidDataException(ExternalCoreExceptions.FullContentPathsRequired());
            CoreName = libraryName!;
            CoreVersion = Marshal.PtrToStringUTF8(info.LibraryVersion) ?? string.Empty;
            SupportedContentExtensions = (Marshal.PtrToStringUTF8(info.ValidExtensions) ?? string.Empty)
                .Split(MediaConstants.SupportedExtensionSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            ValidateConfiguredExtensions(SupportedContentExtensions,
                contentPath is null ? [] : [contentPath]);
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
            Export<ExternalCoreApi.VoidCall>(ExternalCoreConstants.RetroInit)();
            _initialized = true;
            _host.FilterConfiguredOptions(GenesisPlusGXOptionFunctions.FilterToCoreOptions(
                configuration.Options, _host.OptionCatalog));
            _host.ValidateConfiguredOptions();
            var setController = Export<ExternalCoreApi.SetControllerPortDevice>(
                ExternalCoreConstants.RetroSetControllerPortDevice);
            ConfigureControllerPorts(configuration, setController);

            var loadGame = Export<ExternalCoreApi.LoadGame>(ExternalCoreConstants.RetroLoadGame);
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException(ExternalCoreExceptions.StartWithoutMediaUnsupported());
                _gameLoaded = loadGame(0);
            }
            else _gameLoaded = LoadGame(loadGame, contentPath);
            if (!_gameLoaded) throw new InvalidOperationException(ExternalCoreExceptions.ContentRefused());
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

    internal static string? PrepareContentPath(IReadOnlyList<MediaConfiguration> media)
    {
        if (media.Count == 0) return null;
        if (media.Count > 1)
            throw new InvalidOperationException(ExternalCoreExceptions.ContentRefused());
        return Path.GetFullPath(media[0].Path);
    }

    internal static string? PrepareContentPath(IReadOnlyList<MediaConfiguration> media,
        string contentDirectory, string model)
    {
        var path = PrepareContentPath(media);
        if (path is null || !model.Equals(ModelConstants.Sc3000, StringComparison.OrdinalIgnoreCase)
            || !Path.GetExtension(path).Equals(GenesisPlusGXConstants.Sc3000SourceExtension,
                StringComparison.OrdinalIgnoreCase)) return path;
        var stagedPath = Path.Combine(contentDirectory,
            Path.GetFileNameWithoutExtension(path) + GenesisPlusGXConstants.Sc3000CoreExtension);
        File.Copy(path, stagedPath, true);
        return stagedPath;
    }

    internal static void ValidateConfiguredExtensions(IReadOnlySet<string> supportedExtensions,
        IEnumerable<string?> paths)
    {
        foreach (var path in paths)
        {
            if (path is null) continue;
            var extension = Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix);
            if (extension.Length == 0 || !supportedExtensions.Contains(extension))
                throw new InvalidDataException(ExternalCoreExceptions.UnsupportedContentExtension(extension));
        }
    }

    private static void PrepareFirmware(MachineConfiguration configuration, string systemDirectory)
    {
        var firmwarePath = configuration.Options?.GetValueOrDefault(SettingsConstants.FirmwarePath);
        if (string.IsNullOrWhiteSpace(firmwarePath)) return;
        var sourcePath = Path.GetFullPath(firmwarePath);
        if (!File.Exists(sourcePath)) throw new FileNotFoundException(null, sourcePath);
        var firmware = FirmwareCatalog.Inspect(sourcePath);
        if (!firmware.IsKnown || firmware.ExpectedFileNames is not { Count: > 0 })
            throw new InvalidDataException(ExternalCoreExceptions.ContentRefused());
        foreach (var expectedName in firmware.ExpectedFileNames)
        {
            var targetPath = Path.Combine(systemDirectory, expectedName);
            if (!string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
                File.Copy(sourcePath, targetPath, true);
        }
    }

    private void ConfigureControllerPorts(MachineConfiguration configuration,
        ExternalCoreApi.SetControllerPortDevice setController)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var configured = configuration.Input?.ControllerBindings ?? [];
        for (var port = 0; port < model.ControllerPortCount; port++)
        {
            var type = configured.FirstOrDefault(item => item.Port == port)?.Type
                ?? ControllerCatalog.Default(model);
            setController((uint)port, ResolveControllerDevice(port, type));
        }
    }

    private uint ResolveControllerDevice(int port, ControllerType type)
    {
        var devices = _host?.ControllerPorts.ElementAtOrDefault(port) ?? [];
        return ResolveControllerDevice(devices, type);
    }

    internal static uint ResolveControllerDevice(IReadOnlyList<ControllerDevice> devices,
        ControllerType type)
    {
        if (type == ControllerType.None) return 0;
        var exactName = type switch
        {
            ControllerType.SegaSg1000Joystick or ControllerType.SegaSg1000IiJoypad
                or ControllerType.SegaMasterSystemController or ControllerType.SegaGameGearController
                or ControllerType.SegaControlStick =>
                ExternalCoreConstants.MasterSystemJoypadName,
            ControllerType.SegaMegaDriveThreeButton or ControllerType.SegaArcadePowerStick =>
                ExternalCoreConstants.MegaDriveThreeButtonName,
            ControllerType.SegaMegaDriveSixButton or ControllerType.SegaArcadePowerStickSixButton =>
                ExternalCoreConstants.MegaDriveSixButtonName,
            _ => null
        };
        if (exactName is not null)
        {
            var exactDevice = devices.FirstOrDefault(device =>
                device.Name.Contains(exactName, StringComparison.OrdinalIgnoreCase));
            if (exactDevice is not null) return exactDevice.Id;
        }
        var aliases = ControllerAliases(type);
        return devices.FirstOrDefault(device => aliases.Any(alias =>
            device.Name.Contains(alias, StringComparison.OrdinalIgnoreCase)))?.Id ?? 0;
    }

    private static IReadOnlyList<string> ControllerAliases(ControllerType type) => type switch
    {
        ControllerType.SegaLightPhaser => ["light phaser", "lightgun", "light gun"],
        ControllerType.SegaMegaMouse or ControllerType.SegaSaturnShuttleMouse
            or ControllerType.SegaDreamcastMouse => ["mouse"],
        ControllerType.SegaMenacer or ControllerType.SegaSaturnVirtuaGun
            or ControllerType.SegaDreamcastLightGun => ["menacer", "light gun", "lightgun"],
        ControllerType.SegaPaddleControl or ControllerType.SegaSportsPad => ["paddle", "sports"],
        ControllerType.SegaSc3000Keyboard or ControllerType.SegaDreamcastKeyboard => ["keyboard"],
        ControllerType.SegaActivator => ["activator"],
        _ => ["joypad", "controller", "gamepad"]
    };

    public void RunFrame() => (_run
        ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotInitialized()))();
    public void HardReset()
    {
        var configuration = _configuration
            ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotInitialized());
        var sessionDirectory = _sessionDirectory
            ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotInitialized());
        var saveDirectory = _saveDirectory;
        Dispose();
        Initialize(configuration, sessionDirectory, saveDirectory);
    }
    public void SoftReset() => (_reset
        ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotInitialized()))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    public void InsertMedia(string path) => (_host
        ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotInitialized()))
        .DiskControl.Insert(path);
    public void EjectMedia() => (_host
        ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotInitialized()))
        .DiskControl.Eject();
    public void SelectDisk(int index) => (_host
        ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotInitialized()))
        .DiskControl.Select(index);

    public byte[] SaveState()
    {
        var size = (_getSerializedSize
            ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotInitialized()))();
        if (size == ExternalCoreInteropConstants.EmptyNativeSize || size > SavedStateConstants.MaximumStateSize)
            throw new InvalidOperationException(ExternalCoreExceptions.InvalidStateSize(size));
        var state = new byte[(int)size];
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            if (!_serialize!(buffer, size))
                throw new InvalidOperationException(ExternalCoreExceptions.StateSaveFailed());
            Marshal.Copy(buffer, state, BufferConstants.FirstBufferIndex, state.Length);
        }
        finally { Marshal.FreeHGlobal(buffer); }
        return state;
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (state.IsEmpty) throw new ArgumentException(ExternalCoreExceptions.StateEmpty(), nameof(state));
        var bytes = state.ToArray();
        var buffer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, buffer, bytes.Length);
            if (!_unserialize!(buffer, (nuint)bytes.Length))
                throw new InvalidOperationException(ExternalCoreExceptions.StateRestoreFailed());
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    public void SetOption(string key, string value) => (_host
        ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotInitialized()))
        .SetOption(key, value);

    public void Stop()
    {
        if (_gameLoaded) _unloadGame?.Invoke();
        _gameLoaded = false;
    }

    private void ValidateExtension(string? path)
    {
        if (path is null) return;
        var extension = Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix);
        if (!SupportedContentExtensions.Contains(extension))
            throw new InvalidDataException(ExternalCoreExceptions.UnsupportedContentExtension(extension));
    }

    private static bool LoadGame(ExternalCoreApi.LoadGame loadGame, string path)
    {
        using var nativePath = new ExternalCoreUtf8String(path);
        var game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = nativePath.Pointer },
                game, false);
            return loadGame(game);
        }
        finally { Marshal.FreeHGlobal(game); }
    }

    private T Export<T>(string name) where T : Delegate => (_library
        ?? throw new InvalidOperationException(ExternalCoreExceptions.CoreNotLoaded())).Resolve<T>(name);

    private static string ResolveCorePath(string path)
    {
        if (!Path.IsPathFullyQualified(path))
            throw new ArgumentException(ExternalCoreExceptions.CorePathNotAbsolute(), nameof(path));
        if (!File.Exists(path)) throw new FileNotFoundException(ExternalCoreExceptions.CoreNotFound(), path);
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
                _initialized = false;
                _deinitialize = null;
                _unloadGame = null;
                _run = null;
                _reset = null;
                _getSerializedSize = null;
                _serialize = null;
                _unserialize = null;
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
