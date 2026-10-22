using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Wenquxing.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Interop;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Contracts;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Functions;
using NativeFirmware = GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Functions.FirmwareRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Services;
internal sealed class ExternalCore : IEmulatorCore, IEmulationCoreCallbacks
{
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
    private EnvironmentCallbackHost? _environment;
    private InputCallbackHost? _input;
    private bool _initialized, _loaded, _disposed;
    private IReadOnlyList<string> _lastDiagnostics = [];
    internal ExternalCore(string path)
    {
        _path = Path.GetFullPath(path);
        _info = CoreProbeFunctions.Inspect(_path);
        using var stream = File.OpenRead(_path);
        CoreSha256 = CommonCoreHostFunctions.StateFiles.HashStream(stream);
    }

    public Emulator Emulator => Emulator.Wqxemu;
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
            NativeFirmware.Prepare(configuration, Path.Combine(sessionDirectory, "System"));
            _library = CommonCoreHostFunctions.NativeResources.CreateLibrary(_path);
            _exports = CoreFunctions.ResolveExports(_library);
            _video = CommonCoreHostFunctions.NativeResources.CreateVideoCallbackHost();
            _audio = CommonCoreHostFunctions.NativeResources.CreateAudioCallbackHost();
            _environment = CommonCoreHostFunctions.NativeResources.CreateEnvironmentCallbackHost(
                _video,
                _audio,
                NativeOptions.Prepare(configuration),
                Path.Combine(sessionDirectory, "System"),
                Path.Combine(sessionDirectory, "Content"),
                saveDirectory ?? Path.Combine(sessionDirectory, "Saves"),
                Path.Combine(sessionDirectory, "Assets"),
                NativeOptions.CreateCallbackHost
            );
            _input = new InputCallbackHost(configuration.Model, _environment);
            CommonCoreHostFunctions.EnvironmentProcessing.InstallCallbacks(_exports, this);
            _exports.Initialize();
            _initialized = true;
            if (!_environment.SupportsNoGame || !_exports.LoadGame(nint.Zero))
                throw FirmwareInvalid();
            _loaded = true;
            _exports.GetSystemAvInfo(out var info);
            _environment.ApplyAvInfo(info);
            Region = null;
            ThrowCallbackFailure();
            completed = true;
        }
        finally
        {
            if (!completed)
                Cleanup();
        }
    }

    public void RunFrame()
    {
        RequireExports().Run();
        ThrowCallbackFailure();
    }

    public void HardReset()
    {
        _input?.Release();
        RequireExports().Reset();
    }

    public void SetInput(EmulationInputSnapshot snapshot) => (_input ?? throw new InvalidOperationException()).Update(snapshot);
    public void SetOption(string key, string value)
    {
        if (key == GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Constants.RuntimeOptionConstants.Model)
            throw CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Emulator, EmulationMessageCode.OptionInvalid, null);
        (_environment ?? throw new InvalidOperationException()).SetOption(key, value);
    }

    public void SetControllerPortDevice(int port, EmulationPeripheralCategory peripheral)
    {
        if (port != 0 || peripheral != EmulationPeripheralCategory.Keyboard)
            throw new ArgumentException(nameof(peripheral));
    }

    public void InsertMedia(MediaConfiguration media) => throw MediaUnsupported();
    public void EjectMedia(EmulationMediaSlot slot) => throw MediaUnsupported();
    public void SelectDisk(int index) => throw MediaUnsupported();
    public DiskStatus GetDiskStatus() => new(0, 0, true, []);
    public byte[] SaveState()
    {
        var exports = RequireExports();
        var size = exports.GetSerializedSize();
        if (size == 0 || size > (nuint)Array.MaxLength)
            throw StateInvalid();
        var pointer = Marshal.AllocHGlobal(checked((int)size));
        try
        {
            if (!exports.Serialize(pointer, size))
                throw StateInvalid();
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
            throw StateInvalid();
        var pointer = Marshal.AllocHGlobal(state.Length);
        try
        {
            var bytes = state.ToArray();
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            if (!RequireExports().Unserialize(pointer, (nuint)bytes.Length))
                throw StateInvalid();
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
        var loaded = _loaded;
        var initialized = _initialized;
        _lastDiagnostics = environment?.Diagnostics.ToArray() ?? _lastDiagnostics;
        _exports = null;
        _library = null;
        _environment = null;
        _input = null;
        _video = null;
        _audio = null;
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
            try { video?.Dispose(); }
            finally { audio?.Dispose(); }
        }
    }
    private static EmulationMessageException FirmwareInvalid() => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Firmware, EmulationMessageCode.FirmwareIncompatible, null);
    private static EmulationMessageException MediaUnsupported() => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Media, EmulationMessageCode.MediaUnsupported, null);
    private static EmulationMessageException StateInvalid() => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateInvalid, null);
}