using CommonEmulatorCatalog = GWGUI.Emulation.Panasonic.Common.Dictionaries.EmulatorCatalog;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Panasonic.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Contracts;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Functions;
using NativeFirmware = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.FirmwareRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Panasonic.Emulators.Opera.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Services;
internal sealed class ExternalCore : IEmulatorCore, IEmulationCoreCallbacks, IEmulatorAudioBufferFeedback
{
    /// <inheritdoc/>
    /// <remarks>Le backend possède une seule entrée native de reset ; son traitement existant conserve les montages et les réglages.</remarks>
    public void SoftReset() => HardReset();

    /// <inheritdoc/>
    public bool ShutdownRequested => _environment?.ShutdownRequested ?? false;

    /// <inheritdoc/>
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback => _environment?.ControllerFeedback ?? [];

    ExternalCoreApi.EnvironmentCallback IEmulationCoreCallbacks.Environment => _environment!.Callback;
    ExternalCoreApi.VideoCallback IEmulationCoreCallbacks.Video => _video!.Callback;
    ExternalCoreApi.AudioSampleCallback IEmulationCoreCallbacks.AudioSample => _audio!.SampleCallback;
    ExternalCoreApi.AudioBatchCallback IEmulationCoreCallbacks.AudioBatch => _audio!.BatchCallback;
    ExternalCoreApi.InputPollCallback IEmulationCoreCallbacks.InputPoll => _input!.PollCallback;
    ExternalCoreApi.InputStateCallback IEmulationCoreCallbacks.InputState => _input!.StateCallback;

    private readonly string _path;
    private readonly ExternalCoreInfo _info;
    private IEmulationCoreLibrary? _library;
    private ExternalCoreExports? _exports;
    private VideoCallbackHost? _video;
    private AudioCallbackHost? _audio;
    private IEmulationEnvironmentCallbackHost? _environment;
    private InputCallbackHost? _input;
    private MachineConfiguration? _configuration;
    private MediaConfiguration? _content;
    private bool _initialized, _loaded, _disposed;
    private IReadOnlyList<string> _lastDiagnostics = [];
    internal ExternalCore(string path)
    {
        _path = Path.GetFullPath(path);
        _info = CoreProbeFunctions.Inspect(_path);
        var formats = CommonEmulatorCatalog.CreateAdapters().Single(adapter => adapter.EmulatorKey == Emulator.ToString());
        _info = _info with { Extensions = formats.ResolveContentExtensions(_info.Extensions, _info.NeedsFullPath) };
        using var stream = File.OpenRead(_path);
        CoreSha256 = CommonCoreHostFunctions.StateFiles.HashStream(stream);
    }

    public Emulator Emulator => Emulator.Opera;
    public int MinimumAudioLatencyMilliseconds => _environment?.MinimumAudioLatencyMilliseconds ?? 0;
    public void SetAudioBufferStatus(AudioOutputBufferStatus? status) => _environment?.PublishAudioBufferStatus(status);
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

    public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
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
            new GWGUI.Emulation.Panasonic.Common.Services.Engine().SelectedAdapter(configuration)!.PrepareFirmware(configuration, system);
            _configuration = configuration;
            _library = CommonCoreHostFunctions.NativeResources.CreateLibrary(_path);
            _exports = CoreFunctions.ResolveExports(_library);
            _video = CommonCoreHostFunctions.NativeResources.CreateVideoCallbackHost();
            _audio = CommonCoreHostFunctions.NativeResources.CreateAudioCallbackHost();
            _environment = CommonCoreHostFunctions.NativeResources.CreateEnvironmentCallbackHost(_video, _audio, NativeOptions.Prepare(configuration), system, Path.Combine(sessionDirectory, "Content"), saveDirectory ?? Path.Combine(sessionDirectory, "Saves"), Path.Combine(sessionDirectory, "Assets"), OptionCallbackHost.Create);
            _input = new InputCallbackHost();
            CommonCoreHostFunctions.EnvironmentProcessing.InstallCallbacks(_exports, this);
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
                throw CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Media, EmulationMessageCode.RequiredMediaMissing, null);
            using var filename = CommonCoreHostFunctions.NativeResources.CreateString(path);
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
            throw CommonCoreHostFunctions.Messages.CreateError(content is null ? EmulationMessageCategory.Firmware : EmulationMessageCategory.Media, content is null ? EmulationMessageCode.FirmwareIncompatible : EmulationMessageCode.MediaUnsupported, null);
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

    public void SetInput(EmulationInputSnapshot snapshot)
    {
        (_input ?? throw new InvalidOperationException()).Update(snapshot, _video?.LatestFrame?.Width ?? 320, _video?.LatestFrame?.Height ?? 240);
        _environment?.SetInput(snapshot);
    }
    public void SetOption(string key, string value)
    {
        (_environment ?? throw new InvalidOperationException()).SetOption(key, value);
        ThrowCallbackFailure();
    }

    public void SetControllerPortDevice(int port, PeripheralCategory peripheral)
    {
        if (_input is null || !InputSettingsFunctions.SupportsPeripheral(_configuration!, port, peripheral))
            throw new ArgumentException(nameof(peripheral));
        _input.SetPeripheral(port, peripheral);
        RequireExports().SetControllerPortDevice(checked((uint)port), (uint)InputCallbackHost.Device(peripheral));
        _environment?.ResetControllerFeedback(port);
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
        var exports = _exports;
        var library = _library;
        var environment = _environment;
        var input = _input;
        var video = _video;
        var audio = _audio;
        var content = _content;
        var loaded = _loaded;
        var initialized = _initialized;
        _lastDiagnostics = environment?.Diagnostics.ToArray() ?? _lastDiagnostics;
        _exports = null;
        _library = null;
        _environment = null;
        _input = null;
        _video = null;
        _audio = null;
        _content = null;
        _configuration = null;
        _loaded = false;
        _initialized = false;
        try
        {
            try { input?.Release(); }
            finally { CommonCoreHostFunctions.MachineExecution.Cleanup(exports, loaded, initialized, () => environment?.Dispose(), () => library?.Dispose()); }
        }
        finally
        {
            GC.KeepAlive(environment);
            GC.KeepAlive(input);
            GC.KeepAlive(video);
            GC.KeepAlive(audio);
            GC.KeepAlive(content);
            try { video?.Dispose(); }
            finally { audio?.Dispose(); }
        }
    }
    private static EmulationMessageException UnsupportedMedia() => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Media, EmulationMessageCode.MediaUnsupported, null);
    private static EmulationMessageException InvalidState() => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateInvalid, null);
}