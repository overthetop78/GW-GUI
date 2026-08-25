using System.Runtime.InteropServices;
using GWGUI.Emulation;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Amiga.Services;

internal sealed class AmigaExternalCore : IAmigaCore
{
    private static readonly IReadOnlyDictionary<string, string> KnownKickstartNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [AmigaExternalCoreConstants.Hash0B8442C311CA] = AmigaExternalCoreConstants.Kick31034A1000,
            [AmigaExternalCoreConstants.Hash1FA1F93D3D7B] = AmigaExternalCoreConstants.Kick32034A1000,
            [AmigaExternalCoreConstants.Hash85AD74194E87] = AmigaExternalCoreConstants.Kick33180A500,
            [AmigaExternalCoreConstants.Hash82A21C1890CA] = AmigaExternalCoreConstants.Kick34005A500,
            [AmigaExternalCoreConstants.HashDC10D7BDD1B6] = AmigaExternalCoreConstants.Kick37175A500,
            [AmigaExternalCoreConstants.Hash465646C9B672] = AmigaExternalCoreConstants.Kick37350A600,
            [AmigaExternalCoreConstants.HashE40A5DFB3D01] = AmigaExternalCoreConstants.Kick40063A600,
            [AmigaExternalCoreConstants.HashB7CC148386AA] = AmigaExternalCoreConstants.Kick39106A1200,
            [AmigaExternalCoreConstants.Hash646773759326] = AmigaExternalCoreConstants.Kick40068A1200,
            [AmigaExternalCoreConstants.Hash9B8BDD5A3FD3] = AmigaExternalCoreConstants.Kick39106A4000,
            [AmigaExternalCoreConstants.Hash9BDEDDE6A4F3] = AmigaExternalCoreConstants.Kick40068A4000,
            [AmigaExternalCoreConstants.HashF2F241BF0941] = AmigaExternalCoreConstants.Kick40060CD32,
            [AmigaExternalCoreConstants.Hash5F8924D013DD] = AmigaExternalCoreConstants.Kick40060CD32
        };
    private readonly string _corePath;
    private ExternalCoreLibrary? _library;
    private AmigaExternalHostCallbacks? _host;
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

    internal AmigaExternalCore(string corePath) => _corePath = corePath;

    public VideoFrame? LatestVideoFrame => _host?.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _host?.LatestAudioChunk;
    public bool TryDequeueAudio(out AudioChunk? chunk)
    {
        if (_host is not null) return _host.TryDequeueAudio(out chunk);
        chunk = null;
        return false;
    }
    public IReadOnlyList<AmigaCoreOption> Options => _host?.OptionCatalog ?? [];
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
    internal uint Region => (_getRegion ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized))();
    internal nuint GetMemorySize(uint id) =>
        (_getMemorySize ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized))(id);
    internal nint GetMemoryData(uint id) =>
        (_getMemoryData ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized))(id);

    public void Initialize(AmigaMachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        _conversionDirectory = Path.Combine(sessionDirectory, AmigaExternalCoreConstants.ConvertedMedia);
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration.KickstartPath);
        if (!File.Exists(configuration.KickstartPath))
            throw new FileNotFoundException(AmigaExternalCoreConstants.TheConfiguredAmigaKickstartWasNotFound, configuration.KickstartPath);
        var media = ResolveConfiguredMedia(configuration);
        foreach (var item in media)
            if (!File.Exists(item.Path) && !Directory.Exists(item.Path))
                throw new FileNotFoundException(AmigaExternalCoreConstants.TheConfiguredAmigaMediaImageOrDirectoryWasNotFound, item.Path);
        if (!string.IsNullOrWhiteSpace(configuration.ExtendedRomPath) && !File.Exists(configuration.ExtendedRomPath))
            throw new FileNotFoundException(AmigaExternalCoreConstants.TheConfiguredAmigaExtendedROMWasNotFound, configuration.ExtendedRomPath);
        if (!string.IsNullOrWhiteSpace(configuration.RomKeyPath) && !File.Exists(configuration.RomKeyPath))
            throw new FileNotFoundException(AmigaExternalCoreConstants.TheConfiguredAmigaROMKeyWasNotFound, configuration.RomKeyPath);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var coreStream = File.OpenRead(sourceCorePath)) CoreSha256 = Convert.ToHexString(SHA256.HashData(coreStream));
        var systemDirectory = Path.Combine(sessionDirectory, AmigaExternalCoreConstants.System);
        var contentPath = PrepareContentPath(configuration, sessionDirectory, media);
        var contentDirectory = contentPath is null
            ? Path.Combine(sessionDirectory, AmigaExternalCoreConstants.Content)
            : Path.GetDirectoryName(contentPath)!;
        saveDirectory = Path.GetFullPath(saveDirectory ?? Path.Combine(sessionDirectory, AmigaExternalCoreConstants.Saves));
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, AmigaExternalCoreConstants.Core);
        Directory.CreateDirectory(isolatedCoreDirectory);
        var corePath = Path.Combine(isolatedCoreDirectory, AmigaExternalCoreConstants.OptionLibretroDll);
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
            File.Copy(configuration.RomKeyPath, Path.Combine(systemDirectory, AmigaExternalCoreConstants.RomKey), true);

        var backendModel = AmigaModelCatalog.BackendModelFor(configuration.Model);
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [AmigaExternalCoreConstants.OptionModel] = backendModel,
            [AmigaExternalCoreConstants.OptionKickstart] = AmigaExternalCoreConstants.Auto,
            [AmigaExternalCoreConstants.OptionMapperSelect] = AmigaExternalCoreConstants.SWITCHJOYMOUSE
        };
        var floppyCount = media.Count(item => item.Category == AmigaMediaCategory.Floppy);
        if (floppyCount > 1)
            options[AmigaExternalCoreConstants.OptionFloppyMultidrive] = configuration.MountFloppiesInSeparateDrives ? AmigaExternalCoreConstants.Enabled : AmigaExternalCoreConstants.Disabled;
        if (floppyCount > 0 && media.Where(item => item.Category == AmigaMediaCategory.Floppy).All(item => item.IsReadOnly))
            options[AmigaExternalCoreConstants.OptionFloppyWriteProtection] = AmigaExternalCoreConstants.Enabled;
        _host = new AmigaExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory, options);

        try
        {
            _library = new ExternalCoreLibrary(corePath);
            var apiVersion = Export<ExternalCoreApi.GetApiVersion>(AmigaExternalCoreConstants.RetroApiVersion)();
            if (apiVersion != 1) throw new NotSupportedException($"The Amiga core uses unsupported API version {apiVersion}.");
            Export<ExternalCoreApi.GetSystemInfo>(AmigaExternalCoreConstants.RetroGetSystemInfo)(out var systemInfo);
            var libraryName = Marshal.PtrToStringUTF8(systemInfo.LibraryName);
            if (!string.Equals(libraryName, AmigaExternalCoreConstants.PUAE, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"The selected native library identifies itself as '{libraryName}', not PUAE.");
            if (!systemInfo.NeedFullPath)
                throw new InvalidDataException(AmigaExternalCoreConstants.TheAmigaCoreDoesNotRequestFullContentPathsAsRequiredByThisHost);
            CoreName = libraryName!;
            CoreVersion = Marshal.PtrToStringUTF8(systemInfo.LibraryVersion) ?? string.Empty;
            SupportedContentExtensions = (Marshal.PtrToStringUTF8(systemInfo.ValidExtensions) ?? string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(extension => extension.TrimStart('.'))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (contentPath is not null && !Directory.Exists(contentPath))
            {
                var extension = Path.GetExtension(contentPath).TrimStart('.');
                if (extension.Length == 0 || !SupportedContentExtensions.Contains(extension))
                {
                    if (!extension.Equals(AmigaExternalCoreConstants.Scp, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException($"The Amiga core does not support '.{extension}' content.");
                    contentPath = ConvertScp(contentPath);
                }
            }
            Export<ExternalCoreApi.SetEnvironment>(AmigaExternalCoreConstants.RetroSetEnvironment)(_host.Environment);
            Export<ExternalCoreApi.SetVideo>(AmigaExternalCoreConstants.RetroSetVideoRefresh)(_host.Video);
            Export<ExternalCoreApi.SetAudioSample>(AmigaExternalCoreConstants.RetroSetAudioSample)(_host.AudioSample);
            Export<ExternalCoreApi.SetAudioBatch>(AmigaExternalCoreConstants.RetroSetAudioSampleBatch)(_host.AudioBatch);
            Export<ExternalCoreApi.SetInputPoll>(AmigaExternalCoreConstants.RetroSetInputPoll)(_host.InputPoll);
            Export<ExternalCoreApi.SetInputState>(AmigaExternalCoreConstants.RetroSetInputState)(_host.InputState);

            _deinitialize = Export<ExternalCoreApi.VoidCall>(AmigaExternalCoreConstants.RetroDeinit);
            _unloadGame = Export<ExternalCoreApi.VoidCall>(AmigaExternalCoreConstants.RetroUnloadGame);
            _run = Export<ExternalCoreApi.VoidCall>(AmigaExternalCoreConstants.RetroRun);
            _reset = Export<ExternalCoreApi.VoidCall>(AmigaExternalCoreConstants.RetroReset);
            _getSerializedSize = Export<ExternalCoreApi.GetSerializedSize>(AmigaExternalCoreConstants.RetroSerializeSize);
            _serialize = Export<ExternalCoreApi.Serialize>(AmigaExternalCoreConstants.RetroSerialize);
            _unserialize = Export<ExternalCoreApi.Serialize>(AmigaExternalCoreConstants.RetroUnserialize);
            _getRegion = Export<ExternalCoreApi.GetRegion>(AmigaExternalCoreConstants.RetroGetRegion);
            _getMemoryData = Export<ExternalCoreApi.GetMemoryData>(AmigaExternalCoreConstants.RetroGetMemoryData);
            _getMemorySize = Export<ExternalCoreApi.GetMemorySize>(AmigaExternalCoreConstants.RetroGetMemorySize);
            Export<ExternalCoreApi.VoidCall>(AmigaExternalCoreConstants.RetroInit)();
            _initialized = true;
            _host.ValidateConfiguredOptions();
            var setController = Export<ExternalCoreApi.SetControllerPortDevice>(AmigaExternalCoreConstants.RetroSetControllerPortDevice);
            var defaultController = configuration.Model.Equals(AmigaExternalCoreConstants.CD32, StringComparison.OrdinalIgnoreCase)
                ? AmigaControllerType.Cd32Pad
                : AmigaControllerType.Joystick;
            for (var port = 0; port < 4; port++)
            {
                var controller = port >= 2 && configuration.Input?.ParallelJoystickAdapterEnabled != true
                    ? AmigaControllerType.None
                    : configuration.Controllers is { } controllers && port < controllers.Count ? controllers[port]
                    : configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == port)?.Type
                      ?? (port < 2 ? defaultController : AmigaControllerType.None);
                if (controller == AmigaControllerType.Automatic)
                    controller = port < 2 ? defaultController : AmigaControllerType.None;
                setController((uint)port, ControllerDevice(_host.ControllerPorts, port, controller));
            }

            ExternalCoreApi.LoadGame loadGame = Export<ExternalCoreApi.LoadGame>(AmigaExternalCoreConstants.RetroLoadGame);
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreDoesNotSupportStartingWithoutMedia);
                _gameLoaded = loadGame(0);
            }
            else
            {
                _gameLoaded = LoadGame(loadGame, contentPath);
                if (!_gameLoaded && IsScp(contentPath))
                    _gameLoaded = LoadGame(loadGame, ConvertScp(contentPath));
            }

            if (!_gameLoaded) throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreRefusedTheConfiguredContent);
            Export<ExternalCoreApi.GetSystemAvInfo>(AmigaExternalCoreConstants.RetroGetSystemAvInfo)(out var av);
            _host.ApplyInitialAvInfo(av);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void RunFrame() => (_run ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized))();

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
            AmigaExternalCoreConstants.A1000 => AmigaExternalCoreConstants.Kick32034A1000,
            AmigaExternalCoreConstants.A500PLUS => AmigaExternalCoreConstants.Kick37175A500,
            AmigaExternalCoreConstants.A600 => AmigaExternalCoreConstants.Kick40063A600,
            AmigaExternalCoreConstants.A1200 or AmigaExternalCoreConstants.A1200OG => AmigaExternalCoreConstants.Kick40068A1200,
            AmigaExternalCoreConstants.A3000 or AmigaExternalCoreConstants.A4000 => AmigaExternalCoreConstants.Kick40068A4000,
            AmigaExternalCoreConstants.CDTV => AmigaExternalCoreConstants.Kick34005A500,
            AmigaExternalCoreConstants.CD32 or AmigaExternalCoreConstants.CD32FR => AmigaExternalCoreConstants.Kick40060CD32,
            _ => AmigaExternalCoreConstants.Kick34005A500
        };
    }

    internal static string ResolveExtendedRomFileName(string model, string sourcePath) =>
        model.ToUpperInvariant() switch
        {
            AmigaExternalCoreConstants.CD32 or AmigaExternalCoreConstants.CD32FR => AmigaExternalCoreConstants.Kick40060CD32Ext,
            AmigaExternalCoreConstants.CDTV => AmigaExternalCoreConstants.Kick34005CDTV,
            _ => Path.GetFileName(sourcePath)
        };

    private static string ResolveKickstartSuffix(string model, int version, int revision) => (version, revision) switch
    {
        (31 or 32, 34) => AmigaExternalCoreConstants.A1000,
        (33, 180) or (34, 5) or (37, 175) => AmigaExternalCoreConstants.A500,
        (37, 350) or (40, 63) => AmigaExternalCoreConstants.A600,
        (40, 60) => AmigaExternalCoreConstants.CD32,
        (39, 106) or (40, 68) when model.Equals(AmigaExternalCoreConstants.A3000, StringComparison.OrdinalIgnoreCase)
            || model.Equals(AmigaExternalCoreConstants.A4000, StringComparison.OrdinalIgnoreCase) => AmigaExternalCoreConstants.A4000,
        (39, 106) or (40, 68) => AmigaExternalCoreConstants.A1200,
        _ => model.ToUpperInvariant() switch
        {
            AmigaExternalCoreConstants.A1000 => AmigaExternalCoreConstants.A1000,
            AmigaExternalCoreConstants.A600 => AmigaExternalCoreConstants.A600,
            AmigaExternalCoreConstants.A1200 or AmigaExternalCoreConstants.A1200OG => AmigaExternalCoreConstants.A1200,
            AmigaExternalCoreConstants.A3000 or AmigaExternalCoreConstants.A4000 => AmigaExternalCoreConstants.A4000,
            AmigaExternalCoreConstants.CD32 or AmigaExternalCoreConstants.CD32FR => AmigaExternalCoreConstants.CD32,
            _ => AmigaExternalCoreConstants.A500
        }
    };

    internal static IReadOnlyList<AmigaMediaConfiguration> ResolveConfiguredMedia(AmigaMachineConfiguration configuration)
    {
        if (configuration.Media is { Count: > 0 }) return configuration.Media;
        if (configuration.Floppies is { Count: > 0 })
            return configuration.Floppies.Select(floppy => new AmigaMediaConfiguration(
                floppy.Path, AmigaMediaCategory.Floppy, floppy.Label, floppy.IsReadOnly)).ToArray();
        return configuration.InitialDiskPath is null ? []
            : [new AmigaMediaConfiguration(configuration.InitialDiskPath, InferMediaCategory(configuration.InitialDiskPath))];
    }

    internal static string? PrepareContentPath(AmigaMachineConfiguration configuration, string sessionDirectory,
        IReadOnlyList<AmigaMediaConfiguration>? resolvedMedia = null)
    {
        var media = resolvedMedia ?? ResolveConfiguredMedia(configuration);
        if (media.Count == 0) return null;
        if (media.Count == 1) return Path.GetFullPath(media[0].Path);
        if (media.Count > 64) throw new ArgumentOutOfRangeException(nameof(configuration), AmigaExternalCoreConstants.AnAmigaPlaylistCannotContainMoreThan64MediaImages);
        var contentDirectory = Path.Combine(sessionDirectory, AmigaExternalCoreConstants.Content);
        Directory.CreateDirectory(contentDirectory);
        var multidrive = configuration.MountFloppiesInSeparateDrives && media.All(item => item.Category == AmigaMediaCategory.Floppy);
        var playlist = Path.Combine(contentDirectory,
            multidrive ? AmigaExternalCoreConstants.GWGUIMediaMDM3u : AmigaExternalCoreConstants.GWGUIMediaM3u);
        var lines = media.Select(item =>
        {
            var label = item.Label;
            if (label?.IndexOfAny(['|', '\r', '\n']) >= 0) throw new InvalidDataException(AmigaExternalCoreConstants.AnAmigaDiskLabelCannotContainAPipeOrALineBreak);
            var fullPath = Path.GetFullPath(item.Path);
            return string.IsNullOrWhiteSpace(label) ? fullPath : $"{fullPath}|{label}";
        });
        File.WriteAllLines(playlist, lines, new System.Text.UTF8Encoding(false));
        return playlist;
    }

    internal static AmigaMediaCategory InferMediaCategory(string path) => Directory.Exists(path)
        ? AmigaMediaCategory.HardDrive
        : Path.GetExtension(path).ToLowerInvariant() switch
    {
        AmigaExternalCoreConstants.Hdf or AmigaExternalCoreConstants.Hdz => AmigaMediaCategory.HardDrive,
        AmigaExternalCoreConstants.Cue or AmigaExternalCoreConstants.Ccd or AmigaExternalCoreConstants.Chd or AmigaExternalCoreConstants.Nrg or AmigaExternalCoreConstants.Mds or AmigaExternalCoreConstants.Iso => AmigaMediaCategory.CompactDisc,
        AmigaExternalCoreConstants.Lha or AmigaExternalCoreConstants.Slave or AmigaExternalCoreConstants.Info => AmigaMediaCategory.WhdLoad,
        AmigaExternalCoreConstants.Uae => AmigaMediaCategory.Configuration,
            _ => AmigaMediaCategory.Floppy
        };
    public void HardReset() => (_reset ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    public void InsertMedia(string path)
    {
        var diskControl = (_host ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized))
            .DiskControl;
        try { diskControl.Insert(path); }
        catch when (IsScp(path)) { diskControl.Insert(ConvertScp(path)); }
    }
    public void EjectMedia() => (_host ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized))
        .DiskControl.Eject();
    public void SelectDisk(int index) => (_host ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized))
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

    private string ConvertScp(string path) => AmigaRuntimeMediaFunctions
        .ConvertScpPathAsync(path, _conversionDirectory ?? throw new InvalidOperationException())
        .GetAwaiter().GetResult();

    private static bool IsScp(string path) =>
        Path.GetExtension(path).Equals(AmigaExternalCoreConstants.Scp2, StringComparison.OrdinalIgnoreCase);

    public byte[] SaveState()
    {
        var size = (_getSerializedSize ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized))();
        if (size == 0 || size > int.MaxValue) throw new InvalidOperationException($"The Amiga core returned invalid state size {size}.");
        var state = new byte[(int)size];
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            if (!_serialize!(buffer, size))
                throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaStateCouldNotBeSaved);
            Marshal.Copy(buffer, state, 0, state.Length);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
        return state;
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (state.IsEmpty) throw new ArgumentException(AmigaExternalCoreConstants.TheAmigaStateIsEmpty, nameof(state));
        var bytes = state.ToArray();
        var buffer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, buffer, bytes.Length);
            if (!_unserialize!(buffer, (nuint)bytes.Length))
                throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaStateCouldNotBeRestored);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void SetOption(string key, string value) =>
        (_host ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotInitialized)).SetOption(key, value);

    public void Stop()
    {
        if (_gameLoaded) _unloadGame?.Invoke();
        _gameLoaded = false;
    }

    private T Export<T>(string name) where T : Delegate =>
        (_library ?? throw new InvalidOperationException(AmigaExternalCoreConstants.TheAmigaCoreIsNotLoaded)).Resolve<T>(name);

    internal static uint ControllerDevice(IReadOnlyList<IReadOnlyList<AmigaControllerDevice>> ports,
        int port, AmigaControllerType controller)
    {
        if (controller == AmigaControllerType.None) return 0;
        var requestedName = controller switch
        {
            AmigaControllerType.Automatic => AmigaExternalCoreConstants.Automatic,
            AmigaControllerType.RetroPad => AmigaExternalCoreConstants.RetroPad,
            AmigaControllerType.Cd32Pad => AmigaExternalCoreConstants.CD32Pad,
            AmigaControllerType.AnalogJoystick => AmigaExternalCoreConstants.AnalogJoystick,
            AmigaControllerType.Joystick => AmigaExternalCoreConstants.Joystick,
            AmigaControllerType.Keyboard => AmigaExternalCoreConstants.Keyboard,
            _ => throw new ArgumentOutOfRangeException(nameof(controller))
        };
        var devices = port < ports.Count ? ports[port] : [];
        var selected = devices.FirstOrDefault(device => device.Name.Equals(requestedName, StringComparison.OrdinalIgnoreCase));
        if (selected is not null) return selected.Id;
        if (controller == AmigaControllerType.Automatic)
            return devices.FirstOrDefault(device => device.Name.Equals(AmigaExternalCoreConstants.RetroPad, StringComparison.OrdinalIgnoreCase))?.Id ?? 1;
        throw new InvalidDataException($"Controller '{requestedName}' is not supported on Amiga port {port + 1}.");
    }

    private static string ResolveCorePath(string configuredPath)
    {
        if (!Path.IsPathFullyQualified(configuredPath))
            throw new ArgumentException(AmigaExternalCoreConstants.TheAmigaCorePathMustBeAbsolute, nameof(configuredPath));
        if (!File.Exists(configuredPath))
            throw new FileNotFoundException(AmigaExternalCoreConstants.AmigaCoreNotFoundTheConfiguredAmigaCoreWasNotFound, configuredPath);
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
