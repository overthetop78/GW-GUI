using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks : IDisposable
{
    private readonly CoreOptionHost _optionHost;
    private readonly VideoBufferSet _videoBuffers = new();
    private readonly InputFrameStore _input = new();
    private readonly KeyboardState _keyboard = new();
    private ExternalCoreApi.KeyboardEvent? _keyboardEvent;
    private readonly DiskControl _diskControl = new();
    private readonly long _videoStartTimestamp = Stopwatch.GetTimestamp();
    private readonly AudioBuffer _audio = new();
    private readonly Dictionary<int, bool> _ledStates = [];
    private readonly HashSet<uint> _unknownEnvironmentCommands = [];
    private readonly HashSet<uint> _environmentCommands = [];
    private readonly ExternalCoreUtf8String _systemDirectory;
    private readonly ExternalCoreUtf8String _contentDirectory;
    private readonly ExternalCoreUtf8String _saveDirectory;
    private readonly ExternalCoreUtf8String _assetsDirectory;
    private long _videoSequence;
    private long _audioSequence;
    private EmulationPixelFormat _pixelFormat = EmulationPixelFormat.Xrgb8888;
    private bool _disposed;
    private readonly ExternalCoreVirtualFileSystem _virtualFileSystem;

    internal ExternalHostCallbacks(Emulator emulator, string systemDirectory, string contentDirectory, string saveDirectory, string assetsDirectory, IReadOnlyDictionary<string, string> configuredOptions)
    {
        if (!Enum.IsDefined(emulator)) throw new ArgumentOutOfRangeException(nameof(emulator));
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        Directory.CreateDirectory(assetsDirectory);
        ExternalCoreUtf8String? system = null;
        ExternalCoreUtf8String? content = null;
        ExternalCoreUtf8String? save = null;
        ExternalCoreUtf8String? assets = null;
        CoreOptionHost? optionHost = null;
        ExternalCoreVirtualFileSystem? virtualFileSystem = null;
        var completed = false;
        try
        {
            _systemDirectory = system = new ExternalCoreUtf8String(Path.GetFullPath(systemDirectory));
            _contentDirectory = content = new ExternalCoreUtf8String(Path.GetFullPath(contentDirectory));
            _saveDirectory = save = new ExternalCoreUtf8String(Path.GetFullPath(saveDirectory));
            _assetsDirectory = assets = new ExternalCoreUtf8String(Path.GetFullPath(assetsDirectory));
            _optionHost = optionHost = new CoreOptionHost(configuredOptions);
            _virtualFileSystem = virtualFileSystem = new ExternalCoreVirtualFileSystem();
            Environment = OnEnvironment;
            Video = OnVideo;
            AudioSample = OnAudioSample;
            AudioBatch = OnAudioBatch;
            InputPoll = OnInputPoll;
            InputState = OnInputState;
            SetLedState = OnSetLedState;
            SetRumbleState = OnSetRumbleState;
            SetSensorState = OnSetSensorState;
            GetSensorInput = OnGetSensorInput;
            Log = OnLog;
            completed = true;
        }
        finally
        {
            if (!completed)
            {
                using (system)
                using (content)
                using (save)
                using (assets)
                using (virtualFileSystem)
                using (_videoBuffers)
                    optionHost?.Dispose();
            }
        }
    }

    internal ExternalCoreApi.EnvironmentCallback Environment { get; }
    internal ExternalCoreApi.VideoCallback Video { get; }
    internal ExternalCoreApi.AudioSampleCallback AudioSample { get; }
    internal ExternalCoreApi.AudioBatchCallback AudioBatch { get; }
    internal ExternalCoreApi.InputPollCallback InputPoll { get; }
    internal ExternalCoreApi.InputStateCallback InputState { get; }
    internal ExternalCoreApi.SetLedState SetLedState { get; }
    internal ExternalCoreApi.SetRumbleState SetRumbleState { get; }
    internal ExternalCoreApi.SetSensorState SetSensorState { get; }
    internal ExternalCoreApi.GetSensorInput GetSensorInput { get; }
    internal ExternalCoreApi.LogCallback Log { get; }
    internal EmulationInputSnapshot Input
    {
        set => _input.Update(ControllerFunctions.FreezeMappedInput(value));
    }
    internal VideoFrame? LatestVideoFrame { get; private set; }
    internal AudioChunk? LatestAudioChunk { get; private set; }
    internal IReadOnlyList<CoreOption> Options => _optionHost.Catalog;
    internal IReadOnlyList<CoreOptionCategory> OptionCategories => _optionHost.Categories;
    internal IReadOnlyDictionary<string, string> OptionDocumentValues => _optionHost.DocumentValues;
    internal List<string> Diagnostics { get; } = [];
    internal IReadOnlyDictionary<int, bool> LedStates => _ledStates;
    internal IReadOnlyList<InputDescriptor> InputDescriptors { get; private set; } = [];
    internal IReadOnlyList<ControllerPort> ControllerPorts { get; private set; } = [];
    internal IReadOnlyList<MemoryDescriptor> MemoryDescriptors { get; private set; } = [];
    internal nint KeyboardCallbackPointer { get; private set; }
    internal DiskControl DiskControl => _diskControl;
    internal uint Rotation { get; private set; } = EnvironmentConstants.NoRotation;
    internal bool SupportsAchievements { get; private set; }
    internal uint PerformanceLevel { get; private set; }
    internal ExternalCoreApi.Geometry Geometry { get; private set; }
    internal ExternalCoreApi.SystemAvInfo SystemAvInfo { get; private set; }
    internal List<EnvironmentMessage> Messages { get; } = [];
    internal List<EnvironmentExtendedMessage> ExtendedMessages { get; } = [];
    internal IReadOnlySet<uint> EnvironmentCommands => _environmentCommands;
    internal bool SupportsNoGame { get; private set; }
    internal double FramesPerSecond { get; private set; }
    internal int SampleRate { get; private set; }
    internal int BufferedAudioFrames => _audio.BufferedFrames;
    internal long AudioOverrunCount => _audio.OverrunCount;
    internal long AudioUnderrunCount => _audio.UnderrunCount;
    internal float AspectRatio { get; private set; }

    internal bool TryDequeueAudio(out AudioChunk? chunk) => _audio.TryDequeue(out chunk);

    internal void ApplySystemAvInfo(ExternalCoreApi.SystemAvInfo info)
    {
        SystemAvInfo = info;
        Geometry = info.Geometry;
        FramesPerSecond = info.Timing.FramesPerSecond;
        SampleRate = checked((int)Math.Round(info.Timing.SampleRate));
        AspectRatio = info.Geometry.AspectRatio;
    }

    internal void SetOption(string key, string value) => _optionHost.SetValue(key, value);
    internal void ValidateConfiguredOptions() => _optionHost.ValidateConfiguredValues();
    internal void BeginFrame()
    {
    }
}
