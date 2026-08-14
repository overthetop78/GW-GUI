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
    private AmigaExternalApi.GetSerializedSize? _getSerializedSize;
    private AmigaExternalApi.Serialize? _serialize;
    private AmigaExternalApi.Serialize? _unserialize;

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
    public string CoreSha256 { get; private set; } = string.Empty;
    public double FramesPerSecond { get; private set; } = 50;
    public int SampleRate { get; private set; } = 44100;
    public int DiskCount => _host?.DiskControl.ImageCount ?? 0;
    public int CurrentDiskIndex => _host?.DiskControl.CurrentIndex ?? -1;

    public void Initialize(AmigaMachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration.KickstartPath);
        if (!File.Exists(configuration.KickstartPath))
            throw new FileNotFoundException("The configured Amiga Kickstart was not found.", configuration.KickstartPath);
        var floppyPaths = configuration.Floppies is { Count: > 0 }
            ? configuration.Floppies.Select(floppy => floppy.Path).ToArray()
            : configuration.InitialDiskPath is null ? [] : new[] { configuration.InitialDiskPath };
        foreach (var floppyPath in floppyPaths)
            if (!File.Exists(floppyPath)) throw new FileNotFoundException("The configured Amiga disk image was not found.", floppyPath);
        if (configuration.ExtendedRomPath is not null && !File.Exists(configuration.ExtendedRomPath))
            throw new FileNotFoundException("The configured Amiga extended ROM was not found.", configuration.ExtendedRomPath);
        if (configuration.RomKeyPath is not null && !File.Exists(configuration.RomKeyPath))
            throw new FileNotFoundException("The configured Amiga ROM key was not found.", configuration.RomKeyPath);

        var sourceCorePath = ResolveCorePath(_corePath);
        using (var coreStream = File.OpenRead(sourceCorePath)) CoreSha256 = Convert.ToHexString(SHA256.HashData(coreStream));
        var systemDirectory = Path.Combine(sessionDirectory, "System");
        var contentPath = PrepareContentPath(configuration, sessionDirectory, floppyPaths);
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

        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            ["puae_model"] = configuration.Model,
            ["puae_kickstart"] = Path.GetFullPath(configuration.KickstartPath)
        };
        if (floppyPaths.Length > 1)
            options["puae_floppy_multidrive"] = configuration.MountFloppiesInSeparateDrives ? "enabled" : "disabled";
        _host = new AmigaExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory, options);

        try
        {
            _library = NativeLibrary.Load(corePath);
            var apiVersion = Export<AmigaExternalApi.GetApiVersion>("retro_api_version")();
            if (apiVersion != 1) throw new NotSupportedException($"The Amiga core uses unsupported API version {apiVersion}.");
            Export<AmigaExternalApi.GetSystemInfo>("retro_get_system_info")(out var systemInfo);
            var libraryName = Marshal.PtrToStringUTF8(systemInfo.LibraryName);
            if (!string.Equals(libraryName, "PUAE", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"The selected native library identifies itself as '{libraryName}', not PUAE.");
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
            Export<AmigaExternalApi.VoidCall>("retro_init")();
            var setController = Export<AmigaExternalApi.SetControllerPortDevice>("retro_set_controller_port_device");
            for (var port = 0; port < 6; port++)
            {
                var controller = configuration.Controllers is { } controllers && port < controllers.Count ? controllers[port]
                    : configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == port)?.Type
                      ?? (port < 2 ? AmigaControllerType.Automatic : AmigaControllerType.None);
                setController((uint)port, ControllerDevice(controller));
            }

            AmigaExternalApi.LoadGame loadGame = Export<AmigaExternalApi.LoadGame>("retro_load_game");
            if (contentPath is null)
            {
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
            FramesPerSecond = av.Timing.FramesPerSecond;
            SampleRate = checked((int)Math.Round(av.Timing.SampleRate));
            _host.SampleRate = SampleRate;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void RunFrame() => (_run ?? throw new InvalidOperationException("The Amiga core is not initialized."))();

    internal static string? PrepareContentPath(AmigaMachineConfiguration configuration, string sessionDirectory,
        IReadOnlyList<string>? resolvedFloppyPaths = null)
    {
        var paths = resolvedFloppyPaths ?? (configuration.Floppies is { Count: > 0 }
            ? configuration.Floppies.Select(floppy => floppy.Path).ToArray()
            : configuration.InitialDiskPath is null ? [] : new[] { configuration.InitialDiskPath });
        if (paths.Count == 0) return null;
        if (paths.Count == 1) return Path.GetFullPath(paths[0]);
        if (paths.Count > 64) throw new ArgumentOutOfRangeException(nameof(configuration), "An Amiga playlist cannot contain more than 64 disks.");
        var contentDirectory = Path.Combine(sessionDirectory, "Content");
        Directory.CreateDirectory(contentDirectory);
        var playlist = Path.Combine(contentDirectory,
            configuration.MountFloppiesInSeparateDrives ? "GW GUI disks (MD).m3u" : "GW GUI disks.m3u");
        var lines = paths.Select((path, index) =>
        {
            var label = configuration.Floppies?.ElementAtOrDefault(index)?.Label;
            if (label?.IndexOfAny(['|', '\r', '\n']) >= 0) throw new InvalidDataException("An Amiga disk label cannot contain a pipe or a line break.");
            var fullPath = Path.GetFullPath(path);
            return string.IsNullOrWhiteSpace(label) ? fullPath : $"{fullPath}|{label}";
        });
        File.WriteAllLines(playlist, lines, new System.Text.UTF8Encoding(false));
        return playlist;
    }
    public void HardReset() => (_reset ?? throw new InvalidOperationException("The Amiga core is not initialized."))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    public void InsertFloppy(string path) => (_host ?? throw new InvalidOperationException("The Amiga core is not initialized."))
        .DiskControl.Insert(path);
    public void EjectFloppy() => (_host ?? throw new InvalidOperationException("The Amiga core is not initialized."))
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

    private static uint ControllerDevice(AmigaControllerType controller) => controller switch
    {
        AmigaControllerType.Automatic => 1,
        AmigaControllerType.RetroPad => (1u << 8) | 1,
        AmigaControllerType.Cd32Pad => (2u << 8) | 5,
        AmigaControllerType.AnalogJoystick => (3u << 8) | 5,
        AmigaControllerType.Joystick => (1u << 8) | 5,
        AmigaControllerType.Keyboard => (1u << 8) | 3,
        AmigaControllerType.None => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(controller))
    };

    private static string ResolveCorePath(string? configuredPath)
    {
        var candidates = new[]
        {
            configuredPath,
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
            _deinitialize?.Invoke();
            _deinitialize = null;
            _unloadGame = null;
            _run = null;
            _reset = null;
            _getSerializedSize = null;
            _serialize = null;
            _unserialize = null;
            _host?.Dispose();
            _host = null;
            if (_library != 0) NativeLibrary.Free(_library);
            _library = 0;
        }
    }
}
