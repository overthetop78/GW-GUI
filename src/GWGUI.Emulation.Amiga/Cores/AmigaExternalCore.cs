using System.Runtime.InteropServices;
using GWGUI.Emulation;

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

    internal AmigaExternalCore(string? corePath = null) => _corePath = corePath;

    public VideoFrame? LatestVideoFrame => _host?.LatestVideoFrame;
    public AudioChunk? LatestAudioChunk => _host?.LatestAudioChunk;
    public double FramesPerSecond { get; private set; } = 50;
    public int SampleRate { get; private set; } = 44100;

    public void Initialize(AmigaMachineConfiguration configuration, string sessionDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration.KickstartPath);
        if (!File.Exists(configuration.KickstartPath))
            throw new FileNotFoundException("The configured Amiga Kickstart was not found.", configuration.KickstartPath);
        if (configuration.InitialDiskPath is not null && !File.Exists(configuration.InitialDiskPath))
            throw new FileNotFoundException("The configured Amiga disk image was not found.", configuration.InitialDiskPath);

        var sourceCorePath = ResolveCorePath(_corePath);
        var systemDirectory = Path.Combine(sessionDirectory, "System");
        var contentDirectory = configuration.InitialDiskPath is null
            ? Path.Combine(sessionDirectory, "Content")
            : Path.GetDirectoryName(Path.GetFullPath(configuration.InitialDiskPath))!;
        var saveDirectory = Path.Combine(sessionDirectory, "Saves");
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        var isolatedCoreDirectory = Path.Combine(sessionDirectory, "Core");
        Directory.CreateDirectory(isolatedCoreDirectory);
        var corePath = Path.Combine(isolatedCoreDirectory, "puae_libretro.dll");
        File.Copy(sourceCorePath, corePath, true);

        var kickstartFileName = "kickstart.rom";
        var sessionKickstart = Path.Combine(systemDirectory, kickstartFileName);
        File.Copy(configuration.KickstartPath, sessionKickstart, true);

        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            ["puae_model"] = configuration.Model,
            ["puae_kickstart"] = kickstartFileName
        };
        _host = new AmigaExternalHostCallbacks(systemDirectory, contentDirectory, saveDirectory, options);

        try
        {
            _library = NativeLibrary.Load(corePath);
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
            Export<AmigaExternalApi.VoidCall>("retro_init")();

            AmigaExternalApi.LoadGame loadGame = Export<AmigaExternalApi.LoadGame>("retro_load_game");
            if (configuration.InitialDiskPath is null)
            {
                _gameLoaded = loadGame(0);
            }
            else
            {
                var path = Marshal.StringToCoTaskMemUTF8(Path.GetFullPath(configuration.InitialDiskPath));
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
    public void HardReset() => (_reset ?? throw new InvalidOperationException("The Amiga core is not initialized."))();
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_host is not null) _host.Input = snapshot;
    }
    public void InsertFloppy(string path) => (_host ?? throw new InvalidOperationException("The Amiga core is not initialized."))
        .DiskControl.Insert(path);
    public void EjectFloppy() => (_host ?? throw new InvalidOperationException("The Amiga core is not initialized."))
        .DiskControl.Eject();

    public void Stop()
    {
        if (_gameLoaded) _unloadGame?.Invoke();
        _gameLoaded = false;
    }

    private T Export<T>(string name) where T : Delegate =>
        Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(_library, name));

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
            _host?.Dispose();
            _host = null;
            if (_library != 0) NativeLibrary.Free(_library);
            _library = 0;
        }
    }
}
