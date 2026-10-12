using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Contracts;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Functions;
using NativeFirmware = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.FirmwareRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Services;
internal sealed class ExternalCore : IEmulatorCore
{
    private readonly string _path;
    private readonly ExternalCoreInfo _info;
    private ExternalCoreLibrary? _library;
    private ExternalCoreExports? _exports;
    private VideoCallbackHost? _video;
    private AudioCallbackHost? _audio;
    private EnvironmentCallbackHost? _environment;
    private InputCallbackHost? _input;
    private MachineConfiguration? _configuration;
    private MediaConfiguration? _content;
    private bool _initialized, _loaded, _disposed;
    private IReadOnlyList<string> _lastDiagnostics = [];
    internal ExternalCore(string path)
    {
        _path = Path.GetFullPath(path);
        _info = CoreProbeFunctions.Inspect(_path);
        using var stream = File.OpenRead(_path);
        CoreSha256 = Convert.ToHexString(SHA256.HashData(stream));
    }

    public Emulator Emulator => Emulator.Opera;
    public VideoFrame? LatestVideoFrame => _video?.LatestFrame;
    public AudioChunk? LatestAudioChunk => _audio?.LatestChunk;
    public IReadOnlyList<CoreOption> Options => _environment?.Options ?? [];
    public IReadOnlyList<string> Diagnostics => _environment?.Diagnostics ?? _lastDiagnostics;
    public IReadOnlyDictionary<int, bool> LedStates { get; } = new Dictionary<int, bool>();
    public string CoreName => _info.LibraryName;
    public string CoreVersion => _info.LibraryVersion;
    public string CoreSha256 { get; }
    public IReadOnlySet<string> SupportedContentExtensions => _info.Extensions;
    public bool SupportsSaveStates => _loaded && _exports is { } exports && exports.GetSerializedSize()is var size && size > 0 && size <= (nuint)Array.MaxLength;
    public double FramesPerSecond => _environment?.FramesPerSecond ?? 0;
    public int SampleRate => _audio?.SampleRate ?? 0;
    public EmulationVideoStandard? Region { get; private set; }
    public int BufferedAudioFrames => _audio?.BufferedFrames ?? 0;
    public long AudioOverrunCount => _audio?.OverrunCount ?? 0;
    public long AudioUnderrunCount => _audio?.UnderrunCount ?? 0;
    public HostProcessState HostProcessState => HostProcessState.InProcess;
    public int? HostProcessId => null;

    public bool TryDequeueAudio(out AudioChunk? chunk)
    {
        chunk = null;
        return _audio?.TryDequeue(out chunk) == true;
    }

    public void Initialize(
        MachineConfiguration configuration,
        string sessionDirectory,
        string? saveDirectory = null
    )
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_library is not null)
            throw new InvalidOperationException();
        ConfigurationFunctions.Validate(configuration);
        if (configuration.Core != Emulator)
            throw new ArgumentException(nameof(configuration));
        var completed = false;
        try
        {
            var system = Path.Combine(sessionDirectory, "System");
            NativeFirmware.Prepare(configuration, system);
            _configuration = configuration;
            _library = new ExternalCoreLibrary(_path);
            _exports = CoreFunctions.ResolveExports(_library);
            _video = new VideoCallbackHost();
            _audio = new AudioCallbackHost();
            _environment = new EnvironmentCallbackHost(_video, _audio, NativeOptions.Prepare(configuration), system, Path.Combine(sessionDirectory, "Content"), saveDirectory ?? Path.Combine(sessionDirectory, "Saves"), Path.Combine(sessionDirectory, "Assets"));
            _input = new InputCallbackHost();
            _exports.SetEnvironment(_environment.Callback);
            _exports.SetVideo(_video.Callback);
            _exports.SetAudioSample(_audio.SampleCallback);
            _exports.SetAudioBatch(_audio.BatchCallback);
            _exports.SetInputPoll(_input.PollCallback);
            _exports.SetInputState(_input.StateCallback);
            _exports.Initialize();
            _initialized = true;
            LoadContent(configuration.Media.SingleOrDefault(media => media.IsInserted));
            ThrowCallbackFailure();
            completed = true;
        }
        finally
        {
            if (!completed)
                Cleanup();
        }
    }

    private void LoadContent(MediaConfiguration? content)
    {
        var exports = _initialized && _exports is { } active ? active : throw new InvalidOperationException();
        _input?.Release();
        if (_loaded)
        {
            try
            {
                exports.UnloadGame();
            }
            finally
            {
                _loaded = false;
                _content = null;
            }
        }

        var loaded = false;
        if (content is null)
        {
            if (_environment?.SupportsNoGame != true)
                throw UnsupportedMedia();
            loaded = exports.LoadGame(nint.Zero);
        }
        else
        {
            if (content.Category != EmulationMediaType.CompactDisc || content.Slot != EmulationMediaSlot.Cd0)
                throw UnsupportedMedia();
            var path = Path.GetFullPath(content.Path);
            if (!File.Exists(path))
                throw new EmulationMessageException(new EmulationMessage(EmulationMessageCategory.Media, EmulationMessageCode.RequiredMediaMissing, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog));
            using var filename = new ExternalCoreUtf8String(path);
            var info = new ExternalCoreApi.GameInfo
            {
                Path = filename.Pointer
            };
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
            try
            {
                Marshal.StructureToPtr(info, pointer, false);
                loaded = exports.LoadGame(pointer);
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        if (!loaded)
            throw new EmulationMessageException(new EmulationMessage(
                    content is null ? EmulationMessageCategory.Firmware : EmulationMessageCategory.Media,
                    content is null ? EmulationMessageCode.FirmwareIncompatible : EmulationMessageCode.MediaUnsupported,
                    EmulationMessageSeverity.Error,
                    EmulationMessageTarget.Dialog
                ));
        _loaded = true;
        _content = content;
        exports.GetSystemAvInfo(out var av);
        _environment!.ApplyAvInfo(av);
        RefreshRegion();
        for (var port = ControllerConstants.FirstPort; port < ControllerConstants.PortCount; port++)
            SetControllerPortDevice(port, InputSettingsFunctions.Peripheral(_configuration!.Input, port));
        ThrowCallbackFailure();
    }

    private void RefreshRegion() => Region = _exports?.GetRegion() switch
    {
        0 => EmulationVideoStandard.Ntsc,
        1 => EmulationVideoStandard.Pal,
        _ => null
    };
    public void RunFrame()
    {
        RequireExports().Run();
        RefreshRegion();
        ThrowCallbackFailure();
    }

    public void HardReset()
    {
        _input?.Release();
        RequireExports().Reset();
        RefreshRegion();
        ThrowCallbackFailure();
    }

    public void SetInput(EmulationInputSnapshot snapshot) => (_input ?? throw new InvalidOperationException()).Update(snapshot, _video?.LatestFrame?.Width ?? 320, _video?.LatestFrame?.Height ?? 240);
    public void SetOption(
        string key,
        string value
    )
    {
        (_environment ?? throw new InvalidOperationException()).SetOption(key, value);
        ThrowCallbackFailure();
    }

    public void SetControllerPortDevice(
        int port,
        PeripheralCategory peripheral
    )
    {
        if (_input is null || !InputSettingsFunctions.SupportsPeripheral(_configuration!, port, peripheral))
            throw new ArgumentException(nameof(peripheral));
        _input.SetPeripheral(port, peripheral);
        RequireExports().SetControllerPortDevice(checked((uint)port), (uint)InputCallbackHost.Device(peripheral));
    }

    public void InsertMedia(MediaConfiguration media) => LoadContent(media with { IsInserted = true });
    public void EjectMedia(EmulationMediaSlot slot)
    {
        if (slot != EmulationMediaSlot.Cd0)
            throw UnsupportedMedia();
        LoadContent(null);
    }

    public void SelectDisk(int index)
    {
        if (index != 0 || _content is null)
            throw UnsupportedMedia();
    }

    public DiskStatus GetDiskStatus() => _content is null ? new(0, 0, true, []) : new(1, 0, false, [new(0, _content.Path, _content.Label)]);
    public byte[] SaveState()
    {
        var exports = RequireExports();
        var size = exports.GetSerializedSize();
        if (size == 0 || size > (nuint)Array.MaxLength)
            throw InvalidState();
        var pointer = Marshal.AllocHGlobal(checked((int)size));
        try
        {
            if (!exports.Serialize(pointer, size))
                throw InvalidState();
            var result = GC.AllocateUninitializedArray<byte>((int)size);
            Marshal.Copy(pointer, result, 0, result.Length);
            return result;
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (state.IsEmpty)
            throw InvalidState();
        var pointer = Marshal.AllocHGlobal(state.Length);
        try
        {
            var bytes = state.ToArray();
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            if (!RequireExports().Unserialize(pointer, (nuint)bytes.Length))
                throw InvalidState();
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    public void Stop() => Cleanup();
    public void Dispose()
    {
        if (_disposed)
            return;
        try
        {
            Cleanup();
        }
        finally
        {
            _disposed = true;
        }
    }

    private ExternalCoreExports RequireExports() => _loaded && _exports is { } exports ? exports : throw new InvalidOperationException();
    private void ThrowCallbackFailure()
    {
        var error = _environment?.Failure ?? _video?.Failure ?? _audio?.Failure ?? _input?.Failure;
        if (error is not null)
            throw error;
    }

    private void Cleanup()
    {
        _lastDiagnostics = _environment?.Diagnostics.ToArray() ?? _lastDiagnostics;
        try
        {
            try
            {
                _input?.Release();
            }
            finally
            {
                CoreLifecycleFunctions.Cleanup(_exports, _loaded, _initialized, () => _environment?.Dispose(), () => _library?.Dispose());
            }
        }
        finally
        {
            try
            {
                _video?.Dispose();
            }
            finally
            {
                _audio?.Dispose();
                _exports = null;
                _library = null;
                _environment = null;
                _input = null;
                _video = null;
                _audio = null;
                _configuration = null;
                _content = null;
                _loaded = false;
                _initialized = false;
            }
        }
    }

    private static EmulationMessageException UnsupportedMedia() => new(new EmulationMessage(EmulationMessageCategory.Media, EmulationMessageCode.MediaUnsupported, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog));
    private static EmulationMessageException InvalidState() => new(new EmulationMessage(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateInvalid, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog));
}