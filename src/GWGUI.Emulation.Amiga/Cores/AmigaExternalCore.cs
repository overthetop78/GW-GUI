using System.Runtime.InteropServices;
using GWGUI.Emulation;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Amiga.Cores;

internal sealed class AmigaExternalCore : IAmigaCore
{
    private readonly string? _corePath;
    private nint _library;
    private AmigaExternalHostCallbacks? _host;
    private AmigaExternalApi.VoidCall? _deinitialize;
    private AmigaExternalApi.VoidCall? _unloadGame;
    private AmigaExternalApi.VoidCall? _run;
    private AmigaExternalApi.VoidCall? _reset;
    private bool _gameLoaded;
    private bool _initialized;
    private AmigaExternalApi.GetSerializedSize? _getSerializedSize;
    private AmigaExternalApi.Serialize? _serialize;
    private AmigaExternalApi.Serialize? _unserialize;
    private AmigaExternalApi.GetRegion? _getRegion;
    private AmigaExternalApi.GetMemoryData? _getMemoryData;
    private AmigaExternalApi.GetMemorySize? _getMemorySize;

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

        if (configuration.ExtendedRomPath is not null)
        {
            var extendedName = configuration.Model is "CD32" or "CD32FR" ? "kick40060.CD32.ext"
                : configuration.Model == "CDTV" ? "kick34005.CDTV"
                : Path.GetFileName(configuration.ExtendedRomPath);
            File.Copy(configuration.ExtendedRomPath, Path.Combine(systemDirectory, extendedName), true);
        }
        if (configuration.RomKeyPath is not null)
            File.Copy(configuration.RomKeyPath, Path.Combine(systemDirectory, "rom.key"), true);

        var backendModel = AmigaModelCatalog.BackendModelFor(configuration.Model);
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            ["puae_model"] = backendModel,
            ["puae_kickstart"] = Path.GetFullPath(configuration.KickstartPath)
        };
        var floppyCount = media.Count(item => item.Kind == AmigaMediaKind.Floppy);
        if (floppyCount > 1)
            options["puae_floppy_multidrive"] = configuration.MountFloppiesInSeparateDrives ? "enabled" : "disabled";
        if (floppyCount > 0 && media.Where(item => item.Kind == AmigaMediaKind.Floppy).All(item => item.IsReadOnly))
            options["puae_floppy_write_protection"] = "enabled";
        _host = new AmigaExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory, options);

        try
        {
            _library = LoadNativeCore(corePath);
            var apiVersion = Export<AmigaExternalApi.GetApiVersion>("retro_api_version")();
            if (apiVersion != 1) throw new NotSupportedException($"The Amiga core uses unsupported API version {apiVersion}.");
            Export<AmigaExternalApi.GetSystemInfo>("retro_get_system_info")(out var systemInfo);
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
            Export<AmigaExternalApi.SetEnvironment>("retro_set_environment")(_host.Environment);
            Export<AmigaExternalApi.SetVideo>("retro_set_video_refresh")(_host.Video);
            Export<AmigaExternalApi.SetAudioSample>("retro_set_audio_sample")(_host.AudioSample);
            Export<AmigaExternalApi.SetAudioBatch>("retro_set_audio_sample_batch")(_host.AudioBatch);
            Export<AmigaExternalApi.SetInputPoll>("retro_set_input_poll")(_host.InputPoll);
            Export<AmigaExternalApi.SetInputState>("retro_set_input_state")(_host.InputState);

            _deinitialize = Export<AmigaExternalApi.VoidCall>("retro_deinit");
            _unloadGame = Export<AmigaExternalApi.VoidCall>("retro_unload_game");
            _run = Export<AmigaExternalApi.VoidCall>("retro_run");
            _reset = Export<AmigaExternalApi.VoidCall>("retro_reset");
            _getSerializedSize = Export<AmigaExternalApi.GetSerializedSize>("retro_serialize_size");
            _serialize = Export<AmigaExternalApi.Serialize>("retro_serialize");
            _unserialize = Export<AmigaExternalApi.Serialize>("retro_unserialize");
            _getRegion = Export<AmigaExternalApi.GetRegion>("retro_get_region");
            _getMemoryData = Export<AmigaExternalApi.GetMemoryData>("retro_get_memory_data");
            _getMemorySize = Export<AmigaExternalApi.GetMemorySize>("retro_get_memory_size");
            Export<AmigaExternalApi.VoidCall>("retro_init")();
            _initialized = true;
            _host.ValidateConfiguredOptions();
            var setController = Export<AmigaExternalApi.SetControllerPortDevice>("retro_set_controller_port_device");
            for (var port = 0; port < 4; port++)
            {
                var controller = configuration.Controllers is { } controllers && port < controllers.Count ? controllers[port]
                    : configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == port)?.Type
                      ?? (port < 2 ? AmigaControllerType.Automatic : AmigaControllerType.None);
                setController((uint)port, ControllerDevice(_host.ControllerPorts, port, controller));
            }

            AmigaExternalApi.LoadGame loadGame = Export<AmigaExternalApi.LoadGame>("retro_load_game");
            if (contentPath is null)
            {
                if (!_host.SupportsNoGame)
                    throw new InvalidOperationException("The Amiga core does not support starting without media.");
                _gameLoaded = loadGame(0);
            }
            else
            {
                var path = Marshal.StringToCoTaskMemUTF8(contentPath);
                var game = Marshal.AllocHGlobal(Marshal.SizeOf<AmigaExternalApi.GameInfo>());
                try
                {
                    Marshal.StructureToPtr(new AmigaExternalApi.GameInfo { Path = path }, game, false);
                    _gameLoaded = loadGame(game);
                }
                finally
                {
                    Marshal.FreeHGlobal(game);
                    Marshal.FreeCoTaskMem(path);
                }
            }

            if (!_gameLoaded) throw new InvalidOperationException("The Amiga core refused the configured content.");
            Export<AmigaExternalApi.GetSystemAvInfo>("retro_get_system_av_info")(out var av);
            _host.ApplyInitialAvInfo(av);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void RunFrame() => (_run ?? throw new InvalidOperationException("The Amiga core is not initialized."))();

    internal static IReadOnlyList<AmigaMediaConfiguration> ResolveConfiguredMedia(AmigaMachineConfiguration configuration)
    {
        if (configuration.Media is { Count: > 0 }) return configuration.Media;
        if (configuration.Floppies is { Count: > 0 })
            return configuration.Floppies.Select(floppy => new AmigaMediaConfiguration(
                floppy.Path, AmigaMediaKind.Floppy, floppy.Label, floppy.IsReadOnly)).ToArray();
        return configuration.InitialDiskPath is null ? []
            : [new AmigaMediaConfiguration(configuration.InitialDiskPath, InferMediaKind(configuration.InitialDiskPath))];
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
        var multidrive = configuration.MountFloppiesInSeparateDrives && media.All(item => item.Kind == AmigaMediaKind.Floppy);
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

    internal static AmigaMediaKind InferMediaKind(string path) => Directory.Exists(path)
        ? AmigaMediaKind.HardDrive
        : Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".hdf" or ".hdz" => AmigaMediaKind.HardDrive,
        ".cue" or ".ccd" or ".chd" or ".nrg" or ".mds" or ".iso" => AmigaMediaKind.CompactDisc,
        ".lha" or ".slave" or ".info" => AmigaMediaKind.WhdLoad,
        ".uae" => AmigaMediaKind.Configuration,
            _ => AmigaMediaKind.Floppy
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
        Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(_library, name));

    private static nint LoadNativeCore(string absolutePath)
    {
        if (!Path.IsPathFullyQualified(absolutePath))
            throw new ArgumentException("The Amiga core path must be absolute.", nameof(absolutePath));
        if (!File.Exists(absolutePath))
            throw new FileNotFoundException("AmigaCoreNotFound: the configured Amiga core was not found.", absolutePath);
        return NativeLibrary.Load(absolutePath);
    }

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
            if (_library != 0) NativeLibrary.Free(_library);
            _library = 0;
        }
    }
}
