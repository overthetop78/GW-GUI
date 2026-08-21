using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Common;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Amiga.Cores;

internal sealed class AmigaExternalCore : IAmigaCore
{
    private static readonly IReadOnlyDictionary<string, string> KnownKickstartNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["0b8442c311caa54fb12ec88eaaa9facf"] = "kick31034.A1000",
            ["1fa1f93d3d7b51271dd1356b8b2b45a9"] = "kick32034.A1000",
            ["85ad74194e87c08904327de1a9443b7a"] = "kick33180.A500",
            ["82a21c1890cae844b3df741f2762d48d"] = "kick34005.A500",
            ["dc10d7bdd1b6f450773dfb558477c230"] = "kick37175.A500",
            ["465646c9b6729f77eea5314d1f057951"] = "kick37350.A600",
            ["e40a5dfb3d017ba8779faba30cbd1c8e"] = "kick40063.A600",
            ["b7cc148386aa631136f510cd29e42fc3"] = "kick39106.A1200",
            ["646773759326fbac3b2311fd8c8793ee"] = "kick40068.A1200",
            ["9b8bdd5a3fd32c2a5a6f5b1aefc799a5"] = "kick39106.A4000",
            ["9bdedde6a4f33555b4a270c8ca53297d"] = "kick40068.A4000",
            ["f2f241bf094168cfb9e7805dc2856433"] = "kick40060.CD32",
            ["5f8924d013dd57a89cf349f4cdedc6b1"] = "kick40060.CD32"
        };
    private readonly string? _corePath;
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

    internal AmigaExternalCore(string? corePath = null) => _corePath = corePath;

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
    internal uint Region => (_getRegion ?? throw new InvalidOperationException("The Amiga core is not initialized."))();
    internal nuint GetMemorySize(uint id) =>
        (_getMemorySize ?? throw new InvalidOperationException("The Amiga core is not initialized."))(id);
    internal nint GetMemoryData(uint id) =>
        (_getMemoryData ?? throw new InvalidOperationException("The Amiga core is not initialized."))(id);

    public void Initialize(AmigaMachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration.KickstartPath);
        if (!File.Exists(configuration.KickstartPath))
            throw new FileNotFoundException("The configured Amiga Kickstart was not found.", configuration.KickstartPath);
        var media = ResolveConfiguredMedia(configuration);
        foreach (var item in media)
            if (!File.Exists(item.Path) && !Directory.Exists(item.Path))
                throw new FileNotFoundException("The configured Amiga media image or directory was not found.", item.Path);
        if (configuration.ExtendedRomPath is not null && !File.Exists(configuration.ExtendedRomPath))
            throw new FileNotFoundException("The configured Amiga extended ROM was not found.", configuration.ExtendedRomPath);
        if (configuration.RomKeyPath is not null && !File.Exists(configuration.RomKeyPath))
            throw new FileNotFoundException("The configured Amiga ROM key was not found.", configuration.RomKeyPath);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var coreStream = File.OpenRead(sourceCorePath)) CoreSha256 = Convert.ToHexString(SHA256.HashData(coreStream));
        var systemDirectory = Path.Combine(sessionDirectory, "System");
        var contentPath = PrepareContentPath(configuration, sessionDirectory, media);
        var contentDirectory = contentPath is null
            ? Path.Combine(sessionDirectory, "Content")
            : Path.GetDirectoryName(contentPath)!;
        saveDirectory = Path.GetFullPath(saveDirectory ?? Path.Combine(sessionDirectory, "Saves"));
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, "Core");
        Directory.CreateDirectory(isolatedCoreDirectory);
        var corePath = Path.Combine(isolatedCoreDirectory, "puae_libretro.dll");
        File.Copy(sourceCorePath, corePath, true);

        // PUAE discovers firmware in the frontend system directory. The
        // puae_kickstart option selects a discovered ROM; it does not accept an
        // arbitrary absolute file path.
        var sessionKickstartPath = Path.Combine(systemDirectory,
            ResolveKickstartFileName(configuration.Model, configuration.KickstartPath));
        File.Copy(configuration.KickstartPath, sessionKickstartPath, true);

        if (configuration.ExtendedRomPath is not null)
        {
            var extendedName = ResolveExtendedRomFileName(configuration.Model, configuration.ExtendedRomPath);
            File.Copy(configuration.ExtendedRomPath, Path.Combine(systemDirectory, extendedName), true);
        }
        if (configuration.RomKeyPath is not null)
            File.Copy(configuration.RomKeyPath, Path.Combine(systemDirectory, "rom.key"), true);

        var backendModel = AmigaModelCatalog.BackendModelFor(configuration.Model);
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            ["puae_model"] = backendModel,
            ["puae_kickstart"] = "auto",
            ["puae_mapper_select"] = "SWITCH_JOYMOUSE"
        };
        var floppyCount = media.Count(item => item.Category == AmigaMediaCategory.Floppy);
        if (floppyCount > 1)
            options["puae_floppy_multidrive"] = configuration.MountFloppiesInSeparateDrives ? "enabled" : "disabled";
        if (floppyCount > 0 && media.Where(item => item.Category == AmigaMediaCategory.Floppy).All(item => item.IsReadOnly))
            options["puae_floppy_write_protection"] = "enabled";
        _host = new AmigaExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory, options);

        try
        {
            _library = new ExternalCoreLibrary(corePath);
            var apiVersion = Export<ExternalCoreApi.GetApiVersion>("retro_api_version")();
            if (apiVersion != 1) throw new NotSupportedException($"The Amiga core uses unsupported API version {apiVersion}.");
            Export<ExternalCoreApi.GetSystemInfo>("retro_get_system_info")(out var systemInfo);
            var libraryName = Marshal.PtrToStringUTF8(systemInfo.LibraryName);
            if (!string.Equals(libraryName, "PUAE", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"The selected native library identifies itself as '{libraryName}', not PUAE.");
            if (!systemInfo.NeedFullPath)
                throw new InvalidDataException("The Amiga core does not request full content paths as required by this host.");
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
                    throw new InvalidDataException($"The Amiga core does not support '.{extension}' content.");
            }
            Export<ExternalCoreApi.SetEnvironment>("retro_set_environment")(_host.Environment);
            Export<ExternalCoreApi.SetVideo>("retro_set_video_refresh")(_host.Video);
            Export<ExternalCoreApi.SetAudioSample>("retro_set_audio_sample")(_host.AudioSample);
            Export<ExternalCoreApi.SetAudioBatch>("retro_set_audio_sample_batch")(_host.AudioBatch);
            Export<ExternalCoreApi.SetInputPoll>("retro_set_input_poll")(_host.InputPoll);
            Export<ExternalCoreApi.SetInputState>("retro_set_input_state")(_host.InputState);

            _deinitialize = Export<ExternalCoreApi.VoidCall>("retro_deinit");
            _unloadGame = Export<ExternalCoreApi.VoidCall>("retro_unload_game");
            _run = Export<ExternalCoreApi.VoidCall>("retro_run");
            _reset = Export<ExternalCoreApi.VoidCall>("retro_reset");
            _getSerializedSize = Export<ExternalCoreApi.GetSerializedSize>("retro_serialize_size");
            _serialize = Export<ExternalCoreApi.Serialize>("retro_serialize");
            _unserialize = Export<ExternalCoreApi.Serialize>("retro_unserialize");
            _getRegion = Export<ExternalCoreApi.GetRegion>("retro_get_region");
            _getMemoryData = Export<ExternalCoreApi.GetMemoryData>("retro_get_memory_data");
            _getMemorySize = Export<ExternalCoreApi.GetMemorySize>("retro_get_memory_size");
            Export<ExternalCoreApi.VoidCall>("retro_init")();
            _initialized = true;
            _host.ValidateConfiguredOptions();
            var setController = Export<ExternalCoreApi.SetControllerPortDevice>("retro_set_controller_port_device");
            var defaultController = configuration.Model.Equals("CD32", StringComparison.OrdinalIgnoreCase)
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

            ExternalCoreApi.LoadGame loadGame = Export<ExternalCoreApi.LoadGame>("retro_load_game");
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException("The Amiga core does not support starting without media.");
                _gameLoaded = loadGame(0);
            }
            else
            {
                using var path = new ExternalCoreUtf8String(contentPath);
                var game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
                try
                {
                    Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = path.Pointer }, game, false);
                    _gameLoaded = loadGame(game);
                }
                finally
                {
                    Marshal.FreeHGlobal(game);
                }
            }

            if (!_gameLoaded) throw new InvalidOperationException("The Amiga core refused the configured content.");
            Export<ExternalCoreApi.GetSystemAvInfo>("retro_get_system_av_info")(out var av);
            _host.ApplyInitialAvInfo(av);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void RunFrame() => (_run ?? throw new InvalidOperationException("The Amiga core is not initialized."))();

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
            "A1000" => "kick32034.A1000",
            "A500PLUS" => "kick37175.A500",
            "A600" => "kick40063.A600",
            "A1200" or "A1200OG" => "kick40068.A1200",
            "A3000" or "A4000" => "kick40068.A4000",
            "CDTV" => "kick34005.A500",
            "CD32" or "CD32FR" => "kick40060.CD32",
            _ => "kick34005.A500"
        };
    }

    internal static string ResolveExtendedRomFileName(string model, string sourcePath) =>
        model.ToUpperInvariant() switch
        {
            "CD32" or "CD32FR" => "kick40060.CD32.ext",
            "CDTV" => "kick34005.CDTV",
            _ => Path.GetFileName(sourcePath)
        };

    private static string ResolveKickstartSuffix(string model, int version, int revision) => (version, revision) switch
    {
        (31 or 32, 34) => "A1000",
        (33, 180) or (34, 5) or (37, 175) => "A500",
        (37, 350) or (40, 63) => "A600",
        (40, 60) => "CD32",
        (39, 106) or (40, 68) when model.Equals("A3000", StringComparison.OrdinalIgnoreCase)
            || model.Equals("A4000", StringComparison.OrdinalIgnoreCase) => "A4000",
        (39, 106) or (40, 68) => "A1200",
        _ => model.ToUpperInvariant() switch
        {
            "A1000" => "A1000",
            "A600" => "A600",
            "A1200" or "A1200OG" => "A1200",
            "A3000" or "A4000" => "A4000",
            "CD32" or "CD32FR" => "CD32",
            _ => "A500"
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
        if (media.Count > 64) throw new ArgumentOutOfRangeException(nameof(configuration), "An Amiga playlist cannot contain more than 64 media images.");
        var contentDirectory = Path.Combine(sessionDirectory, "Content");
        Directory.CreateDirectory(contentDirectory);
        var multidrive = configuration.MountFloppiesInSeparateDrives && media.All(item => item.Category == AmigaMediaCategory.Floppy);
        var playlist = Path.Combine(contentDirectory,
            multidrive ? "GW GUI media (MD).m3u" : "GW GUI media.m3u");
        var lines = media.Select(item =>
        {
            var label = item.Label;
            if (label?.IndexOfAny(['|', '\r', '\n']) >= 0) throw new InvalidDataException("An Amiga disk label cannot contain a pipe or a line break.");
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
        ".hdf" or ".hdz" => AmigaMediaCategory.HardDrive,
        ".cue" or ".ccd" or ".chd" or ".nrg" or ".mds" or ".iso" => AmigaMediaCategory.CompactDisc,
        ".lha" or ".slave" or ".info" => AmigaMediaCategory.WhdLoad,
        ".uae" => AmigaMediaCategory.Configuration,
            _ => AmigaMediaCategory.Floppy
        };
    public void HardReset() => (_reset ?? throw new InvalidOperationException("The Amiga core is not initialized."))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    public void InsertMedia(string path) => (_host ?? throw new InvalidOperationException("The Amiga core is not initialized."))
        .DiskControl.Insert(path);
    public void EjectMedia() => (_host ?? throw new InvalidOperationException("The Amiga core is not initialized."))
        .DiskControl.Eject();
    public void SelectDisk(int index) => (_host ?? throw new InvalidOperationException("The Amiga core is not initialized."))
        .DiskControl.Select(index);

    public byte[] SaveState()
    {
        var size = (_getSerializedSize ?? throw new InvalidOperationException("The Amiga core is not initialized."))();
        if (size == 0 || size > int.MaxValue) throw new InvalidOperationException($"The Amiga core returned invalid state size {size}.");
        var state = new byte[(int)size];
        unsafe
        {
            fixed (byte* pointer = state)
                if (!_serialize!((nint)pointer, size)) throw new InvalidOperationException("The Amiga state could not be saved.");
        }
        return state;
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (state.IsEmpty) throw new ArgumentException("The Amiga state is empty.", nameof(state));
        unsafe
        {
            fixed (byte* pointer = state)
                if (!_unserialize!((nint)pointer, (nuint)state.Length)) throw new InvalidOperationException("The Amiga state could not be restored.");
        }
    }

    public void SetOption(string key, string value) =>
        (_host ?? throw new InvalidOperationException("The Amiga core is not initialized.")).SetOption(key, value);

    public void Stop()
    {
        if (_gameLoaded) _unloadGame?.Invoke();
        _gameLoaded = false;
    }

    private T Export<T>(string name) where T : Delegate =>
        (_library ?? throw new InvalidOperationException("The Amiga core is not loaded.")).Resolve<T>(name);

    internal static uint ControllerDevice(IReadOnlyList<IReadOnlyList<AmigaControllerDevice>> ports,
        int port, AmigaControllerType controller)
    {
        if (controller == AmigaControllerType.None) return 0;
        var requestedName = controller switch
        {
            AmigaControllerType.Automatic => "Automatic",
            AmigaControllerType.RetroPad => "RetroPad",
            AmigaControllerType.Cd32Pad => "CD32 Pad",
            AmigaControllerType.AnalogJoystick => "Analog Joystick",
            AmigaControllerType.Joystick => "Joystick",
            AmigaControllerType.Keyboard => "Keyboard",
            _ => throw new ArgumentOutOfRangeException(nameof(controller))
        };
        var devices = port < ports.Count ? ports[port] : [];
        var selected = devices.FirstOrDefault(device => device.Name.Equals(requestedName, StringComparison.OrdinalIgnoreCase));
        if (selected is not null) return selected.Id;
        if (controller == AmigaControllerType.Automatic)
            return devices.FirstOrDefault(device => device.Name.Equals("RetroPad", StringComparison.OrdinalIgnoreCase))?.Id ?? 1;
        throw new InvalidDataException($"Controller '{requestedName}' is not supported on Amiga port {port + 1}.");
    }

    private static string ResolveCorePath(string? configuredPath)
    {
        if (configuredPath is not null)
        {
            if (!Path.IsPathFullyQualified(configuredPath))
                throw new ArgumentException("The Amiga core path must be absolute.", nameof(configuredPath));
            if (!File.Exists(configuredPath))
                throw new FileNotFoundException("AmigaCoreNotFound: the configured Amiga core was not found.", configuredPath);
            return configuredPath;
        }
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Emulation", "puae_libretro.dll"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "ppua", "puae_libretro.dll")),
            Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "artifacts", "ppua", "puae_libretro.dll"))
        };
        return candidates.FirstOrDefault(path => path is not null && File.Exists(path))
            ?? throw new FileNotFoundException("The temporary Amiga core puae_libretro.dll was not found.", candidates[1]);
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
