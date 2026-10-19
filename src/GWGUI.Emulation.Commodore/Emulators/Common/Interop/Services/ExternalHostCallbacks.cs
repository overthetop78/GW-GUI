using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;
internal sealed partial class ExternalHostCallbacks : IDisposable
{
    private readonly IReadOnlyDictionary<uint, EmulationKey> _keyboardMap;
    private readonly IReadOnlyDictionary<EmulationKey, IReadOnlyList<EmulationKey>> _keyboardExpansions;
    private readonly IReadOnlyDictionary<EmulationKey, IReadOnlyList<EmulationKey>> _keyboardToggleExpansions;
    private readonly HashSet<EmulationKey> _activeKeyboardToggles = new();
    private readonly HashSet<EmulationKey> _previousKeyboardToggleSources = new();
    private readonly string? _pathOption;
    private readonly Dictionary<string, string> _options = new(StringComparer.Ordinal);
    private readonly HashSet<string> _configuredOptionKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ExternalCoreUtf8String> _nativeStrings = new(StringComparer.Ordinal);
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private EmulationPixelFormat _pixelFormat = EmulationPixelFormat.Rgb565;
    private float _aspectRatio;
    private long _videoSequence;
    private long _audioSequence;
    private bool _disposed;
    private int _optionsUpdated;
    private readonly object _inputGate = new();
    private EmulationInputSnapshot _pendingInput = EmulationInputSnapshot.Empty;
    private EmulationInputSnapshot _polledInput = EmulationInputSnapshot.Empty;
    private IReadOnlySet<EmulationKey> _previousKeys = new HashSet<EmulationKey>();
    private ExternalCoreApi.KeyboardEvent? _keyboardEvent;
    private ExternalCoreApi.UpdateCoreOptionsDisplay? _updateOptionsDisplay;
    private readonly Dictionary<string, bool> _optionVisibility = new(StringComparer.Ordinal);
    internal ExternalDiskControl DiskControl { get; } = new();

    internal ExternalHostCallbacks(
        string systemDirectory,
        string contentDirectory,
        string saveDirectory,
        IReadOnlyDictionary<string, string>? options,
        string? pathOption = null,
        IReadOnlyDictionary<uint, EmulationKey?>? keyboardOverrides = null,
        IReadOnlyDictionary<EmulationKey, IReadOnlyList<EmulationKey>>? keyboardExpansions = null,
        IReadOnlyDictionary<EmulationKey, IReadOnlyList<EmulationKey>>? keyboardToggleExpansions = null
    )
    {
        _keyboardMap = CreateKeyboardMap(keyboardOverrides);
        _keyboardExpansions = keyboardExpansions ?? new Dictionary<EmulationKey, IReadOnlyList<EmulationKey>>();
        _keyboardToggleExpansions = keyboardToggleExpansions ?? new Dictionary<EmulationKey, IReadOnlyList<EmulationKey>>();
        _pathOption = pathOption;
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        SystemDirectory = Path.GetFullPath(systemDirectory);
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
        InputPoll = HandleInputPoll;
        InputState = HandleInputState;
        Log = HandleLog;
        Led = HandleLed;
    }

    internal string SystemDirectory { get; }
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
        set
        {
            lock (_inputGate)
            {
                var pointer = value.Pointer with
                {
                    DeltaX = EmulationInputMappingFunctions.SaturatingAdd(_pendingInput.Pointer.DeltaX, value.Pointer.DeltaX),
                    DeltaY = EmulationInputMappingFunctions.SaturatingAdd(_pendingInput.Pointer.DeltaY, value.Pointer.DeltaY),
                    Wheel = EmulationInputMappingFunctions.SaturatingAdd(_pendingInput.Pointer.Wheel, value.Pointer.Wheel)
                };
                _pendingInput = value with
                {
                    Pointer = pointer
                };
            }
        }
    }

    internal ExternalCoreApi.EnvironmentCallback Environment { get; }
    internal ExternalCoreApi.VideoCallback Video { get; }
    internal ExternalCoreApi.AudioSampleCallback AudioSample { get; }
    internal ExternalCoreApi.AudioBatchCallback AudioBatch { get; }
    internal ExternalCoreApi.InputPollCallback InputPoll { get; }
    internal ExternalCoreApi.InputStateCallback InputState { get; }
    internal ExternalCoreApi.LogCallback Log { get; }
    internal ExternalCoreApi.SetLedState Led { get; }
    internal int SampleRate { get; set; } = ExternalHostCallbacksConstants.DefaultSampleRate;
    internal double FramesPerSecond { get; private set; } = ExternalHostCallbacksConstants.DefaultFrameRate;
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
            throw new ArgumentOutOfRangeException(nameof(key), key, CoreExceptions.UnknownCoreOption());
        var option = OptionCatalog.First(item => item.Key.Equals(key, StringComparison.Ordinal));
        if (option.Values.Count > BufferConstants.EmptyCollectionCount && !option.Values.Any(item => item.Value.Equals(value, StringComparison.Ordinal)))
            throw new ArgumentOutOfRangeException(nameof(value), value, CoreExceptions.InvalidOptionValue(value, key));
        _options[key] = value;
        Interlocked.Exchange(ref _optionsUpdated, ExternalHostCallbacksConstants.OptionsChangedState);
        _updateOptionsDisplay?.Invoke();
    }

    public void Dispose()
    {
        if (_disposed) return;
        Exception? failure = null;
        try
        {
            foreach (var text in _nativeStrings.Values)
            {
                try { text.Dispose(); }
                catch (Exception error) { failure ??= error; }
            }
        }
        finally
        {
            _nativeStrings.Clear();
            lock (_inputGate)
            {
                _activeKeyboardToggles.Clear();
                _previousKeyboardToggleSources.Clear();
                _previousKeys = new HashSet<EmulationKey>();
            }
            _disposed = true;
        }
        if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }
}