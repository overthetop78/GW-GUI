using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

internal sealed partial class AmigaExternalHostCallbacks : IDisposable
{
    private static readonly IReadOnlyDictionary<uint, EmulationKey> KeyboardMap = CreateKeyboardMap();
    private readonly Dictionary<string, string> _options = new(StringComparer.Ordinal);
    private readonly HashSet<string> _configuredOptionKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, nint> _nativeStrings = new(StringComparer.Ordinal);
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
    internal AmigaExternalDiskControl DiskControl { get; } = new();

    internal AmigaExternalHostCallbacks(string systemDirectory, string contentDirectory,
        string saveDirectory, IReadOnlyDictionary<string, string>? options)
    {
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
    private readonly Queue<AudioChunk> _audioChunks = new();
    private readonly object _audioGate = new();
    private int _bufferedAudioFrames;
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
                    DeltaX = SaturatingAdd(_pendingInput.Pointer.DeltaX, value.Pointer.DeltaX),
                    DeltaY = SaturatingAdd(_pendingInput.Pointer.DeltaY, value.Pointer.DeltaY),
                    Wheel = SaturatingAdd(_pendingInput.Pointer.Wheel, value.Pointer.Wheel)
                };
                _pendingInput = value with { Pointer = pointer };
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
    internal int SampleRate { get; set; } = 44100;
    internal double FramesPerSecond { get; private set; } = 50;
    internal bool SupportsNoGame { get; private set; }
    internal IReadOnlyList<IReadOnlyList<AmigaControllerDevice>> ControllerPorts { get; private set; } = [];
    internal IReadOnlyList<AmigaCoreOption> OptionCatalog { get; private set; } = [];
    internal IReadOnlyList<string> Diagnostics => _diagnostics.ToArray();
    internal IReadOnlyDictionary<int, bool> LedStates
    {
        get
        {
            var now = Stopwatch.GetTimestamp();
            return _ledStates.Keys.Concat(_ledActivityUntil.Keys).Distinct()
                .ToDictionary(key => key, key => _ledStates.GetValueOrDefault(key)
                    || _ledActivityUntil.GetValueOrDefault(key) > now);
        }
    }
    internal int BufferedAudioFrames { get { lock (_audioGate) return _bufferedAudioFrames; } }
    internal long AudioOverrunCount { get; private set; }

    internal void SetOption(string key, string value)
    {
        if (!OptionCatalog.Any(option => option.Key.Equals(key, StringComparison.Ordinal)))
            throw new ArgumentOutOfRangeException(nameof(key), key, AmigaExternalHostCallbacksConstants.UnknownAmigaCoreOption);
        var option = OptionCatalog.First(item => item.Key.Equals(key, StringComparison.Ordinal));
        if (option.Values.Count > 0 && !option.Values.Any(item => item.Value.Equals(value, StringComparison.Ordinal)))
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Invalid value for Amiga option {key}.");
        _options[key] = value;
        Interlocked.Exchange(ref _optionsUpdated, 1);
        _updateOptionsDisplay?.Invoke();
    }

    public void Dispose()
    {
        if (_disposed) return;
        foreach (var pointer in _nativeStrings.Values) Marshal.FreeCoTaskMem(pointer);
        _nativeStrings.Clear();
        _disposed = true;
    }
}
