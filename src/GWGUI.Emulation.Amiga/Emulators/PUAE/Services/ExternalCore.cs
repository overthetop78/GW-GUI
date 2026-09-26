using GWGUI.Emulation.Amiga.Emulators.PUAE.Exceptions;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Contracts;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Factories;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Functions;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

using System.Runtime.InteropServices;
using GWGUI.Emulation;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

internal sealed class ExternalCore : IEmulatorCore
{
    private static readonly IReadOnlyDictionary<string, string> KnownKickstartNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [FirmwareCatalogConstants.Hash0B8442C311CA] = ExternalCoreConstants.Kick31034A1000,
            [FirmwareCatalogConstants.Hash1FA1F93D3D7B] = ExternalCoreConstants.Kick32034A1000,
            [FirmwareCatalogConstants.Hash85AD74194E87] = ExternalCoreConstants.Kick33180A500,
            [FirmwareCatalogConstants.Hash82A21C1890CA] = ExternalCoreConstants.Kick34005A500,
            [FirmwareCatalogConstants.HashDC10D7BDD1B6] = ExternalCoreConstants.Kick37175A500,
            [FirmwareCatalogConstants.Hash465646C9B672] = ExternalCoreConstants.Kick37350A600,
            [FirmwareCatalogConstants.HashE40A5DFB3D01] = ExternalCoreConstants.Kick40063A600,
            [FirmwareCatalogConstants.HashB7CC148386AA] = ExternalCoreConstants.Kick39106A1200,
            [FirmwareCatalogConstants.Hash646773759326] = ExternalCoreConstants.Kick40068A1200,
            [FirmwareCatalogConstants.Hash9B8BDD5A3FD3] = ExternalCoreConstants.Kick39106A4000,
            [FirmwareCatalogConstants.Hash9BDEDDE6A4F3] = ExternalCoreConstants.Kick40068A4000,
            [FirmwareCatalogConstants.HashF2F241BF0941] = ExternalCoreConstants.Kick40060CD32,
            [FirmwareCatalogConstants.Hash5F8924D013DD] = ExternalCoreConstants.Kick40060CD32
        };
    private readonly string _corePath;
    private ExternalCoreLibrary? _library;
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
    public double FramesPerSecond => _host?.FramesPerSecond ?? 50;
    public int SampleRate => _host?.SampleRate ?? 44100;
    public int DiskCount => _host?.DiskControl.ImageCount ?? 0;
    public int CurrentDiskIndex => _host?.DiskControl.CurrentIndex ?? -1;
    internal uint Region => (_getRegion ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized()))();
    internal nuint GetMemorySize(uint id) =>
        (_getMemorySize ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized()))(id);
    internal nint GetMemoryData(uint id) =>
        (_getMemoryData ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized()))(id);

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        _conversionDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.ConvertedMedia);
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration.KickstartPath);
        if (!File.Exists(configuration.KickstartPath))
            throw new FileNotFoundException(PuaeExceptions.KickstartNotFound(), configuration.KickstartPath);
        var media = ResolveConfiguredMedia(configuration);
        foreach (var disk in media.Where(item => item.Category == MediaCategory.HardDrive && !Directory.Exists(item.Path)))
        {
            var format = HardDiskFormats.All.FirstOrDefault(item => string.Equals(item.Extension,
                Path.GetExtension(disk.Path), StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException(PuaeExceptions.UnsupportedHardDiskExtension());
            GWGUI.Emulation.HardDisks.HardDiskImageValidation.ValidateExisting(disk.Path, format);
        }
        foreach (var item in media)
            if (!File.Exists(item.Path) && !Directory.Exists(item.Path))
                throw new FileNotFoundException(PuaeExceptions.MediaNotFound(), item.Path);
        if (!string.IsNullOrWhiteSpace(configuration.ExtendedRomPath) && !File.Exists(configuration.ExtendedRomPath))
            throw new FileNotFoundException(PuaeExceptions.ExtendedRomNotFound(), configuration.ExtendedRomPath);
        if (!string.IsNullOrWhiteSpace(configuration.RomKeyPath) && !File.Exists(configuration.RomKeyPath))
            throw new FileNotFoundException(PuaeExceptions.RomKeyNotFound(), configuration.RomKeyPath);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var coreStream = File.OpenRead(sourceCorePath)) CoreSha256 = Convert.ToHexString(SHA256.HashData(coreStream));
        var systemDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName);
        var contentPath = PrepareContentPath(configuration, sessionDirectory, media);
        var contentDirectory = contentPath is null
            ? Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName)
            : Path.GetDirectoryName(contentPath)!;
        saveDirectory = Path.GetFullPath(saveDirectory
            ?? Path.Combine(sessionDirectory, CoreDirectoryConstants.SavesDirectoryName));
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, ExternalCoreConstants.Core);
        Directory.CreateDirectory(isolatedCoreDirectory);
        var corePath = Path.Combine(isolatedCoreDirectory, ExternalCoreConstants.OptionLibretroDll);
        File.Copy(sourceCorePath, corePath, true);

        // PUAE discovers firmware in the frontend system directory. The
        // puae_kickstart option selects a discovered ROM; it does not accept an
        // arbitrary absolute file path.
        var sessionKickstartPath = Path.Combine(systemDirectory,
            ResolveKickstartFileName(configuration.Model, configuration.KickstartPath));
        File.Copy(configuration.KickstartPath, sessionKickstartPath, true);

        if (!string.IsNullOrWhiteSpace(configuration.ExtendedRomPath))
        {
            var extendedName = ResolveExtendedRomFileName(configuration.Model, configuration.ExtendedRomPath);
            File.Copy(configuration.ExtendedRomPath, Path.Combine(systemDirectory, extendedName), true);
        }
        if (!string.IsNullOrWhiteSpace(configuration.RomKeyPath))
            File.Copy(configuration.RomKeyPath, Path.Combine(systemDirectory, ExternalCoreConstants.RomKey), true);

        var backendModel = ModelCatalog.BackendModelFor(configuration.Model);
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [PuaeOptionConstants.Model] = backendModel,
            [ExternalCoreConstants.OptionKickstart] = ExternalCoreConstants.Auto,
            [ExternalCoreConstants.OptionMapperMouseToggle] = ExternalCoreConstants.RightControl
        };
        var floppyCount = media.Count(item => item.Category == MediaCategory.Floppy);
        if (floppyCount > 1)
            options[PuaeOptionConstants.FloppyMultidrive] = configuration.MountFloppiesInSeparateDrives
                ? SettingsDescriptionFunctionsConstants.Enabled : SettingsDescriptionFunctionsConstants.Disabled;
        if (floppyCount > 0 && media.Where(item => item.Category == MediaCategory.Floppy).All(item => item.IsReadOnly))
            options[PuaeOptionConstants.FloppyWriteProtection] = SettingsDescriptionFunctionsConstants.Enabled;
        _host = new ExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory, options);

        try
        {
            _library = new ExternalCoreLibrary(corePath);
            var apiVersion = Export<ExternalCoreApi.GetApiVersion>(ExternalCoreConstants.RetroApiVersion)();
            if (apiVersion != ExternalCoreInteropConstants.ApiVersion)
                throw new NotSupportedException(PuaeExceptions.UnsupportedApiVersion(apiVersion));
            Export<ExternalCoreApi.GetSystemInfo>(ExternalCoreConstants.RetroGetSystemInfo)(out var systemInfo);
            var libraryName = Marshal.PtrToStringUTF8(systemInfo.LibraryName);
            if (!string.Equals(libraryName, PuaeConstants.DisplayName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(PuaeExceptions.LibraryIdentityMismatch(libraryName));
            if (!systemInfo.NeedFullPath)
                throw new InvalidDataException(PuaeExceptions.FullContentPathsRequired());
            CoreName = libraryName!;
            CoreVersion = Marshal.PtrToStringUTF8(systemInfo.LibraryVersion) ?? string.Empty;
            SupportedContentExtensions = (Marshal.PtrToStringUTF8(systemInfo.ValidExtensions) ?? string.Empty)
                .Split(MediaConstants.SupportedExtensionSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (contentPath is not null && !Directory.Exists(contentPath))
            {
                var extension = Path.GetExtension(contentPath).TrimStart(MediaConstants.ExtensionPrefix);
                if (extension.Length == BufferConstants.EmptyCollectionCount
                    || !SupportedContentExtensions.Contains(extension))
                {
                    if (!extension.Equals(ExternalCoreConstants.Scp, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException(PuaeExceptions.UnsupportedContentExtension(extension));
                    contentPath = ConvertScp(contentPath);
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
            var defaultController = configuration.Model.Equals(ExternalCoreConstants.CD32, StringComparison.OrdinalIgnoreCase)
                ? ControllerType.Cd32Pad
                : ControllerType.Joystick;
            for (var port = ControllerPortConstants.MinimumControllerPort;
                 port < ControllerPortConstants.MaximumControllerPortCount; port++)
            {
                var controller = port >= 2 && configuration.Input?.ParallelJoystickAdapterEnabled != true
                    ? ControllerType.None
                    : configuration.Controllers is { } controllers && port < controllers.Count ? controllers[port]
                    : configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == port)?.Type
                      ?? (port < 2 ? defaultController : ControllerType.None);
                if (controller == ControllerType.Automatic)
                    controller = port < 2 ? defaultController : ControllerType.None;
                setController((uint)port, ControllerDevice(_host.ControllerPorts, port, controller));
            }

            ExternalCoreApi.LoadGame loadGame = Export<ExternalCoreApi.LoadGame>(ExternalCoreConstants.RetroLoadGame);
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException(PuaeExceptions.StartWithoutMediaUnsupported());
                _gameLoaded = loadGame(0);
            }
            else
            {
                _gameLoaded = LoadGame(loadGame, contentPath);
                if (!_gameLoaded && IsScp(contentPath))
                    _gameLoaded = LoadGame(loadGame, ConvertScp(contentPath));
            }

            if (!_gameLoaded) throw new InvalidOperationException(PuaeExceptions.ContentRefused());
            Export<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreConstants.RetroGetSystemAvInfo)(out var av);
            _host.ApplyInitialAvInfo(av);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void RunFrame() => (_run ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized()))();

    internal static string ResolveKickstartFileName(string model, string sourcePath)
    {
        using var stream = File.OpenRead(sourcePath);
        var md5 = Convert.ToHexString(MD5.HashData(stream));
        if (KnownKickstartNames.TryGetValue(md5, out var knownName)) return knownName;

        stream.Position = 0;
        Span<byte> header = stackalloc byte[16];
        if (stream.Read(header) == header.Length)
        {
            var version = (header[12] << 8) | header[13];
            var revision = (header[14] << 8) | header[15];
            var suffix = ResolveKickstartSuffix(model, version, revision);
            if (version is >= 29 and <= 50 && revision is <= 999)
                return $"kick{version}{revision:D3}.{suffix}";
        }

        return model.ToUpperInvariant() switch
        {
            ExternalCoreConstants.A1000 => ExternalCoreConstants.Kick32034A1000,
            ExternalCoreConstants.A500PLUS => ExternalCoreConstants.Kick37175A500,
            ExternalCoreConstants.A600 => ExternalCoreConstants.Kick40063A600,
            ExternalCoreConstants.A1200 or ExternalCoreConstants.A1200OG => ExternalCoreConstants.Kick40068A1200,
            ExternalCoreConstants.A3000 or ExternalCoreConstants.A4000 => ExternalCoreConstants.Kick40068A4000,
            ExternalCoreConstants.CDTV => ExternalCoreConstants.Kick34005A500,
            ExternalCoreConstants.CD32 or ExternalCoreConstants.CD32FR => ExternalCoreConstants.Kick40060CD32,
            _ => ExternalCoreConstants.Kick34005A500
        };
    }

    internal static string ResolveExtendedRomFileName(string model, string sourcePath) =>
        model.ToUpperInvariant() switch
        {
            ExternalCoreConstants.CD32 or ExternalCoreConstants.CD32FR => ExternalCoreConstants.Kick40060CD32Ext,
            ExternalCoreConstants.CDTV => ExternalCoreConstants.Kick34005CDTV,
            _ => Path.GetFileName(sourcePath)
        };

    private static string ResolveKickstartSuffix(string model, int version, int revision) => (version, revision) switch
    {
        (31 or 32, 34) => ExternalCoreConstants.A1000,
        (33, 180) or (34, 5) or (37, 175) => ExternalCoreConstants.A500,
        (37, 350) or (40, 63) => ExternalCoreConstants.A600,
        (40, 60) => ExternalCoreConstants.CD32,
        (39, 106) or (40, 68) when model.Equals(ExternalCoreConstants.A3000, StringComparison.OrdinalIgnoreCase)
            || model.Equals(ExternalCoreConstants.A4000, StringComparison.OrdinalIgnoreCase) => ExternalCoreConstants.A4000,
        (39, 106) or (40, 68) => ExternalCoreConstants.A1200,
        _ => model.ToUpperInvariant() switch
        {
            ExternalCoreConstants.A1000 => ExternalCoreConstants.A1000,
            ExternalCoreConstants.A600 => ExternalCoreConstants.A600,
            ExternalCoreConstants.A1200 or ExternalCoreConstants.A1200OG => ExternalCoreConstants.A1200,
            ExternalCoreConstants.A3000 or ExternalCoreConstants.A4000 => ExternalCoreConstants.A4000,
            ExternalCoreConstants.CD32 or ExternalCoreConstants.CD32FR => ExternalCoreConstants.CD32,
            _ => ExternalCoreConstants.A500
        }
    };

    internal static IReadOnlyList<MediaConfiguration> ResolveConfiguredMedia(MachineConfiguration configuration)
    {
        if (configuration.Media is { Count: > 0 }) return configuration.Media;
        if (configuration.Floppies is { Count: > 0 })
            return configuration.Floppies.Select(floppy => new MediaConfiguration(
                floppy.Path, MediaCategory.Floppy, floppy.Label, floppy.IsReadOnly)).ToArray();
        return configuration.InitialDiskPath is null ? []
            : [new MediaConfiguration(configuration.InitialDiskPath, InferMediaCategory(configuration.InitialDiskPath))];
    }

    internal static string? PrepareContentPath(MachineConfiguration configuration, string sessionDirectory,
        IReadOnlyList<MediaConfiguration>? resolvedMedia = null)
    {
        var media = resolvedMedia ?? ResolveConfiguredMedia(configuration);
        if (media.Count == BufferConstants.EmptyCollectionCount) return null;
        if (media.Count == 1) return Path.GetFullPath(media[0].Path);
        if (media.Count > 64) throw new ArgumentOutOfRangeException(nameof(configuration), PuaeExceptions.PlaylistLimitExceeded());
        var contentDirectory = Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName);
        Directory.CreateDirectory(contentDirectory);
        var multidrive = configuration.MountFloppiesInSeparateDrives && media.All(item => item.Category == MediaCategory.Floppy);
        var playlist = Path.Combine(contentDirectory,
            multidrive ? ExternalCoreConstants.GWGUIMediaMDM3u : ExternalCoreConstants.GWGUIMediaM3u);
        var lines = media.Select(item =>
        {
            var label = item.Label;
            if (label?.IndexOfAny(['|', '\r', '\n']) >= 0) throw new InvalidDataException(PuaeExceptions.DiskLabelInvalid());
            var fullPath = Path.GetFullPath(item.Path);
            return string.IsNullOrWhiteSpace(label) ? fullPath : $"{fullPath}|{label}";
        });
        File.WriteAllLines(playlist, lines, new System.Text.UTF8Encoding(false));
        return playlist;
    }

    internal static MediaCategory InferMediaCategory(string path) => Directory.Exists(path)
        ? MediaCategory.HardDrive
        : Path.GetExtension(path).ToLowerInvariant() switch
    {
        StorageSettingsFunctionsConstants.Hdf or StorageSettingsFunctionsConstants.Hdz => MediaCategory.HardDrive,
        StorageSettingsFunctionsConstants.Cue or StorageSettingsFunctionsConstants.Ccd or StorageSettingsFunctionsConstants.Chd or StorageSettingsFunctionsConstants.Nrg or StorageSettingsFunctionsConstants.Mds or StorageSettingsFunctionsConstants.Iso => MediaCategory.CompactDisc,
        ExternalCoreConstants.Lha or ExternalCoreConstants.Slave or ExternalCoreConstants.Info => MediaCategory.WhdLoad,
        ExternalCoreConstants.Uae => MediaCategory.Configuration,
            _ => MediaCategory.Floppy
        };
    public void HardReset() => (_reset ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized()))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    public void InsertMedia(string path)
    {
        var diskControl = (_host ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized()))
            .DiskControl;
        try { diskControl.Insert(path); }
        catch when (IsScp(path)) { diskControl.Insert(ConvertScp(path)); }
    }
    public void EjectMedia() => (_host ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized()))
        .DiskControl.Eject();
    public void SelectDisk(int index) => (_host ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized()))
        .DiskControl.Select(index);

    private static bool LoadGame(ExternalCoreApi.LoadGame loadGame, string path)
    {
        using var nativePath = new ExternalCoreUtf8String(path);
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

    private string ConvertScp(string path) => RuntimeMediaFunctions
        .ConvertScpPathAsync(path, _conversionDirectory ?? throw new InvalidOperationException())
        .GetAwaiter().GetResult();

    private static bool IsScp(string path) =>
        Path.GetExtension(path).Equals(StorageSettingsFunctionsConstants.Scp, StringComparison.OrdinalIgnoreCase);

    public byte[] SaveState()
    {
        var size = (_getSerializedSize ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized()))();
        if (size == ExternalCoreInteropConstants.EmptyNativeSize
            || size > SavedStateConstants.MaximumStateSize)
            throw new InvalidOperationException(PuaeExceptions.InvalidStateSize(size));
        var state = new byte[(int)size];
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            if (!_serialize!(buffer, size))
                throw new InvalidOperationException(PuaeExceptions.StateSaveFailed());
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
        if (state.IsEmpty) throw new ArgumentException(PuaeExceptions.StateEmpty(), nameof(state));
        var bytes = state.ToArray();
        var buffer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, buffer, bytes.Length);
            if (!_unserialize!(buffer, (nuint)bytes.Length))
                throw new InvalidOperationException(PuaeExceptions.StateRestoreFailed());
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void SetOption(string key, string value) =>
        (_host ?? throw new InvalidOperationException(PuaeExceptions.CoreNotInitialized())).SetOption(key, value);

    public void Stop()
    {
        if (_gameLoaded) _unloadGame?.Invoke();
        _gameLoaded = false;
    }

    private T Export<T>(string name) where T : Delegate =>
        (_library ?? throw new InvalidOperationException(PuaeExceptions.CoreNotLoaded())).Resolve<T>(name);

    internal static uint ControllerDevice(IReadOnlyList<IReadOnlyList<ControllerDevice>> ports,
        int port, ControllerType controller)
    {
        if (controller == ControllerType.None) return 0;
        var requestedName = controller switch
        {
            ControllerType.Automatic => ExternalCoreConstants.Automatic,
            ControllerType.RetroPad => ExternalCoreConstants.RetroPad,
            ControllerType.Cd32Pad => ExternalCoreConstants.CD32Pad,
            ControllerType.AnalogJoystick => ExternalCoreConstants.AnalogJoystick,
            ControllerType.Joystick => ExternalCoreConstants.Joystick,
            ControllerType.Keyboard => ExternalCoreConstants.Keyboard,
            _ => throw new ArgumentOutOfRangeException(nameof(controller))
        };
        var devices = port < ports.Count ? ports[port] : [];
        var selected = devices.FirstOrDefault(device => device.Name.Equals(requestedName, StringComparison.OrdinalIgnoreCase));
        if (selected is not null) return selected.Id;
        if (controller == ControllerType.Automatic)
            return devices.FirstOrDefault(device => device.Name.Equals(ExternalCoreConstants.RetroPad, StringComparison.OrdinalIgnoreCase))?.Id ?? 1;
        throw new InvalidDataException(PuaeExceptions.UnsupportedController(requestedName, port + 1));
    }

    private static string ResolveCorePath(string configuredPath)
    {
        if (!Path.IsPathFullyQualified(configuredPath))
            throw new ArgumentException(PuaeExceptions.CorePathNotAbsolute(), nameof(configuredPath));
        if (!File.Exists(configuredPath))
            throw new FileNotFoundException(PuaeExceptions.CoreNotFound(), configuredPath);
        return configuredPath;
    }

    public void Dispose()
    {
        try { Stop(); }
        finally
        {
            if (_initialized) _deinitialize?.Invoke();
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
            _host?.Dispose();
            _host = null;
            _library?.Dispose();
            _library = null;
        }
    }
}
