using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks : IDisposable
{
    private static readonly IReadOnlyDictionary<uint, EmulationKey> KeyboardMap = CreateKeyboardMap();
    internal HardwareRenderSession HardwareRender { get; } = new();
    internal CoreDiagnosticMonitor DiagnosticMonitor { get; }
    private readonly Dictionary<string, string> _options = new(StringComparer.Ordinal);
    private readonly HashSet<string> _configuredOptionKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ExternalCoreUtf8String> _nativeStrings = new(StringComparer.Ordinal);
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private EmulationPixelFormat _pixelFormat = EmulationPixelFormat.Rgb565;
    private float _aspectRatio;
    private long _videoSequence;
    private long _audioSequence;
    private bool _disposed;
    private readonly string? _emulatorId;
    private readonly bool _legacyCallbacks;
    private readonly bool _pspCallbacks;
    private readonly ExternalCoreVirtualFileSystem? _virtualFileSystem;
    private volatile bool _shutdownRequested;
    internal bool ShutdownRequested => _shutdownRequested;
    internal NativeControllerFeedback Feedback { get; } = new();
    private int _optionsUpdated;
    private readonly object _inputGate = new();
    private readonly int[] _controllerTypes = Enumerable.Repeat((int)ControllerType.None, GWGUI.Emulation.Sony.Common.Constants.ControllerPortConstants.MaximumControllerPortCount).ToArray();
    private readonly IReadOnlyList<CoreControllerPort>? _controllerPorts;
    private readonly InputAccumulator _input = new();
    private EmulationInputSnapshot _polledInput = EmulationInputSnapshot.Empty;
    private int _pointerX = ExternalHostCallbacksConstants.PointerCoordinateCenter;
    private int _pointerY = ExternalHostCallbacksConstants.PointerCoordinateCenter;
    private IReadOnlySet<EmulationKey> _previousKeys = new HashSet<EmulationKey>();
    private ExternalCoreApi.KeyboardEvent? _keyboardEvent;
    private ExternalCoreApi.UpdateCoreOptionsDisplay? _updateOptionsDisplay;
    private bool _suppressFullscreenChord;
    private readonly Dictionary<string, bool> _optionVisibility = new(StringComparer.Ordinal);
    internal ExternalDiskControl DiskControl { get; }

    internal ExternalHostCallbacks(string systemDirectory, string contentDirectory, string saveDirectory, IReadOnlyDictionary<string, string>? options, IReadOnlyList<string>? startupFailureDiagnostics = null, IReadOnlyList<CoreControllerPort>? controllerPorts = null,
        string? emulatorId = null, string? nativeSystemDirectory = null, Func<string, (string Path, bool IsReadOnly)>? pathResolver = null)
    {
        _emulatorId = emulatorId;
        _pspCallbacks = emulatorId == Emulators.Ppsspp.Constants.PpssppConstants.Id;
        _legacyCallbacks = _pspCallbacks || emulatorId == Emulators.Pcsx2.Constants.Pcsx2Constants.Id;
        DiskControl = new ExternalDiskControl(emulatorId);
        _controllerPorts = controllerPorts;
        DiagnosticMonitor = new CoreDiagnosticMonitor(startupFailureDiagnostics);
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        SystemDirectory = nativeSystemDirectory ?? Path.GetFullPath(systemDirectory);
        ContentDirectory = Path.GetFullPath(contentDirectory);
        SaveDirectory = Path.GetFullPath(saveDirectory);
        if (options is not null)
            foreach (var option in options)
            {
                _options[option.Key] = option.Value;
                _configuredOptionKeys.Add(option.Key);
            }

        Environment = HandleEnvironment;
        Video = HandleVideo;
        AudioSample = HandleAudioSample;
        AudioBatch = HandleAudioBatch;
        MultichannelAudioBatch = HandleMultichannelAudioBatch;
        InputPoll = HandleInputPoll;
        InputState = HandleInputState;
        Log = HandleLog;
        Led = HandleLed;
        if (emulatorId == Emulators.Pcsx2.Constants.Pcsx2Constants.Id)
            _virtualFileSystem = new ExternalCoreVirtualFileSystem(readObserver: null, pathResolver: pathResolver);
    }

    internal string SystemDirectory { get; }
    internal bool CheatsEnabled => _options.GetValueOrDefault(Emulators.Pcsx2.Constants.OptionConstants.EnableCheatsKey) == EmulationSettingConstants.Enabled;
    internal string ContentDirectory { get; }
    internal string SaveDirectory { get; }
    internal VideoFrame? LatestVideoFrame { get; private set; }
    internal AudioChunk? LatestAudioChunk { get; private set; }
    private readonly GWGUI.Emulation.Services.EmulationAudioBuffer _audioBuffer = new();
    private readonly ConcurrentQueue<string> _diagnostics = new();
    private readonly ConcurrentDictionary<int, bool> _ledStates = new();
    private readonly ConcurrentDictionary<int, long> _ledActivityUntil = new();
    private readonly HashSet<uint> _unknownEnvironmentCommands = [];
    internal EmulationInputSnapshot Input
    {
        set { lock (_inputGate) _input.Update(value); }
    }
    internal ExternalCoreApi.EnvironmentCallback Environment { get; }
    internal void SetControllerType(int port, ControllerType type)
    {
        if (port < GWGUI.Emulation.Sony.Common.Constants.ControllerPortConstants.MinimumControllerPort
            || port >= _controllerTypes.Length)
            throw new ArgumentOutOfRangeException(nameof(port), port, null);
        Volatile.Write(ref _controllerTypes[port], (int)type);
    }
    internal ExternalCoreApi.VideoCallback Video { get; }
    internal ExternalCoreApi.AudioSampleCallback AudioSample { get; }
    internal ExternalCoreApi.AudioBatchCallback AudioBatch { get; }
    internal MultichannelAudioApi.BatchInt16 MultichannelAudioBatch { get; }
    internal ExternalCoreApi.InputPollCallback InputPoll { get; }
    internal ExternalCoreApi.InputStateCallback InputState { get; }
    internal NativeLogFunctions.WindowsX64Callback Log { get; }
    internal ExternalCoreApi.SetLedState Led { get; }
    internal int SampleRate { get; set; } = ExternalCoreConstants.FallbackSampleRate;
    internal double FramesPerSecond { get; private set; } = ExternalCoreConstants.FallbackFramesPerSecond;
    internal bool SupportsNoGame { get; private set; }
    internal IReadOnlyList<IReadOnlyList<ControllerDevice>> ControllerPorts { get; private set; } = [];
    internal IReadOnlyList<CoreOption> OptionCatalog { get; private set; } = [];
    internal IReadOnlyList<string> Diagnostics => _diagnostics.ToArray();
    internal IReadOnlyDictionary<int, bool> LedStates
    {
        get
        {
            var now = Stopwatch.GetTimestamp();
            return _ledStates.Keys.Concat(_ledActivityUntil.Keys).Distinct().ToDictionary(key => key, key => _ledStates.GetValueOrDefault(key) || _ledActivityUntil.GetValueOrDefault(key) > now);
        }
    }
    internal int BufferedAudioFrames => _audioBuffer.BufferedFrames;
    internal long AudioOverrunCount => _audioBuffer.NativeOverrunCount;

    internal void SetOption(string key, string value)
    {
        if (!OptionCatalog.Any(option => option.Key.Equals(key, StringComparison.Ordinal)))
            throw new ArgumentOutOfRangeException(nameof(key), key, CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.UnknownCoreOption));
        var option = OptionCatalog.First(item => item.Key.Equals(key, StringComparison.Ordinal));
        if (option.Values.Count > 0 && !option.Values.Any(item => item.Value.Equals(value, StringComparison.Ordinal)))
            throw new ArgumentOutOfRangeException(nameof(value), value, CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.InvalidOptionValue, value, key));
        _options[key] = value;
        OptionCatalog = CoreOptionMetadataFunctions.WithCurrentValues(OptionCatalog, _options);
        Interlocked.Exchange(ref _optionsUpdated, 1);
        _updateOptionsDisplay?.Invoke();
    }

    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            try { HardwareRender.Dispose(); }
            finally
            {
                foreach (var pointer in _nativeStrings.Values)
                {
                    GWGUI.Emulation.Functions.EmulationHostProtocolFunctions.DisposeTransportIgnoringErrors(pointer);
                }
            }
        }
        finally
        {
            try
            {
                Feedback.Dispose();
                _nativeStrings.Clear();
                _keyboardEvent = null;
                _updateOptionsDisplay = null;
                LatestVideoFrame = null;
                _audioBuffer.ClearNative(() => LatestAudioChunk = null);
                lock (_inputGate)
                {
                    _input.Reset();
                    _polledInput = EmulationInputSnapshot.Empty;
                    _previousKeys = new HashSet<EmulationKey>();
                }
                _disposed = true;
            }
            finally { _virtualFileSystem?.Dispose(); }
        }
    }
}
