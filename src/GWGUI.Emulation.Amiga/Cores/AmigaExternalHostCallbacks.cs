using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using GWGUI.Emulation;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Amiga.Cores;

internal sealed class AmigaExternalHostCallbacks : IDisposable
{
    private const uint KeyboardDevice = 3;
    private const uint MouseDevice = 2;
    private const uint JoypadDevice = 1;
    private const uint AnalogDevice = 5;
    private const uint JoypadMask = 256;
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
            throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown Amiga core option.");
        var option = OptionCatalog.First(item => item.Key.Equals(key, StringComparison.Ordinal));
        if (option.Values.Count > 0 && !option.Values.Any(item => item.Value.Equals(value, StringComparison.Ordinal)))
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Invalid value for Amiga option {key}.");
        _options[key] = value;
        Interlocked.Exchange(ref _optionsUpdated, 1);
        _updateOptionsDisplay?.Invoke();
    }

    private bool HandleEnvironment(uint command, nint data)
    {
        try
        {
            switch (command)
            {
                case ExternalCoreApiConstants.GetSystemDirectory:
                    Marshal.WriteIntPtr(data, NativeString(SystemDirectory));
                    return true;
                case ExternalCoreApiConstants.GetContentDirectory:
                    Marshal.WriteIntPtr(data, NativeString(ContentDirectory));
                    return true;
                case ExternalCoreApiConstants.GetSaveDirectory:
                    Marshal.WriteIntPtr(data, NativeString(SaveDirectory));
                    return true;
                case ExternalCoreApiConstants.GetCanDuplicateFrames:
                case ExternalCoreApiConstants.GetInputBitmasks:
                    if (data != 0) Marshal.WriteByte(data, 1);
                    return true;
                case ExternalCoreApiConstants.SetMessage:
                    return CaptureMessage(data, extended: false);
                case ExternalCoreApiConstants.SetPixelFormat:
                    _pixelFormat = Marshal.ReadInt32(data) switch
                    {
                        1 => EmulationPixelFormat.Xrgb8888,
                        2 => EmulationPixelFormat.Rgb565,
                        var value => throw new NotSupportedException($"Pixel format {value} is not supported.")
                    };
                    return true;
                case ExternalCoreApiConstants.GetCoreOptionsVersion:
                    if (data != 0) Marshal.WriteInt32(data, 2);
                    return true;
                case ExternalCoreApiConstants.SetCoreOptionsV2:
                    RegisterVersionTwoOptions(data);
                    return true;
                case ExternalCoreApiConstants.SetCoreOptionsV2International:
                    RegisterVersionTwoOptions(Marshal.ReadIntPtr(data));
                    return true;
                case ExternalCoreApiConstants.SetVariables:
                    RegisterLegacyOptions(data);
                    return true;
                case ExternalCoreApiConstants.GetVariable:
                    return ReturnOption(data);
                case ExternalCoreApiConstants.GetVariableUpdate:
                    if (data != 0) Marshal.WriteByte(data, Interlocked.Exchange(ref _optionsUpdated, 0) != 0 ? (byte)1 : (byte)0);
                    return true;
                case ExternalCoreApiConstants.GetDiskControlVersion:
                    if (data != 0) Marshal.WriteInt32(data, 1);
                    return true;
                case ExternalCoreApiConstants.SetInputDescriptors:
                    return true;
                case ExternalCoreApiConstants.SetKeyboardCallback:
                    var keyboard = Marshal.PtrToStructure<ExternalCoreApi.KeyboardCallback>(data);
                    _keyboardEvent = keyboard.Callback == 0 ? null : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.KeyboardEvent>(keyboard.Callback);
                    return true;
                case ExternalCoreApiConstants.SetDiskControl:
                    DiskControl.Capture(data);
                    return true;
                case ExternalCoreApiConstants.SetDiskControlExtended:
                    DiskControl.CaptureExtended(data);
                    return true;
                case ExternalCoreApiConstants.SetControllerInfo:
                    return CaptureControllerInfo(data);
                case ExternalCoreApiConstants.SetMemoryMaps:
                case ExternalCoreApiConstants.SetSupportAchievements:
                    return true;
                case ExternalCoreApiConstants.SetCoreOptionsDisplay:
                    return ApplyOptionVisibility(data);
                case ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback:
                    return CaptureOptionsDisplayCallback(data);
                case ExternalCoreApiConstants.SetSupportNoGame:
                    SupportsNoGame = data != 0 && Marshal.ReadByte(data) != 0;
                    return true;
                case ExternalCoreApiConstants.GetMessageInterfaceVersion:
                    if (data != 0) Marshal.WriteInt32(data, 1);
                    return data != 0;
                case ExternalCoreApiConstants.SetMessageExtended:
                    return CaptureMessage(data, extended: true);
                case ExternalCoreApiConstants.SetFastForwardingOverride:
                    return false;
                case ExternalCoreApiConstants.SetGeometry:
                    return ApplyGeometry(data);
                case ExternalCoreApiConstants.SetSystemAvInfo:
                    return ApplySystemAvInfo(data);
                case ExternalCoreApiConstants.GetLogInterface:
                    Marshal.StructureToPtr(new ExternalCoreApi.LogInterface
                    {
                        Log = Marshal.GetFunctionPointerForDelegate(Log)
                    }, data, false);
                    return true;
                case ExternalCoreApiConstants.GetVfsInterface:
                    return false;
                case ExternalCoreApiConstants.GetLedInterface:
                    if (data == 0) return false;
                    Marshal.StructureToPtr(new ExternalCoreApi.LedInterface
                    {
                        SetLedState = Marshal.GetFunctionPointerForDelegate(Led)
                    }, data, false);
                    return true;
                default:
                    if (_unknownEnvironmentCommands.Add(command)) AddDiagnostic($"Unsupported environment command: {command}");
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void RegisterLegacyOptions(nint data)
    {
        var size = Marshal.SizeOf<ExternalCoreApi.Variable>();
        var catalog = new List<AmigaCoreOption>();
        for (var current = data; current != 0; current += size)
        {
            var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(current);
            if (variable.Key == 0) break;
            var key = Marshal.PtrToStringUTF8(variable.Key)!;
            var definition = Marshal.PtrToStringUTF8(variable.Value);
            var parts = definition?.Split(';', 2) ?? [];
            var name = parts.ElementAtOrDefault(0)?.Trim() ?? key;
            var values = parts.ElementAtOrDefault(1)?.Trim().Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => new AmigaCoreOptionValue(value, value)).ToArray() ?? [];
            var defaultValue = values.FirstOrDefault()?.Value ?? string.Empty;
            if (!_options.ContainsKey(key) && defaultValue.Length > 0) _options[key] = defaultValue;
            catalog.Add(new AmigaCoreOption(key, name, null, null, defaultValue, values,
                !_optionVisibility.TryGetValue(key, out var visible) || visible));
        }
        OptionCatalog = catalog;
    }

    private void RegisterVersionTwoOptions(nint options)
    {
        if (options == 0) return;
        var definitions = Marshal.ReadIntPtr(options, IntPtr.Size);
        const int pointerFieldsBeforeValues = 6;
        const int maximumValues = 128;
        var definitionSize = (pointerFieldsBeforeValues + maximumValues * 2 + 1) * IntPtr.Size;
        var valuesOffset = pointerFieldsBeforeValues * IntPtr.Size;
        var defaultOffset = valuesOffset + maximumValues * 2 * IntPtr.Size;
        var catalog = new List<AmigaCoreOption>();

        for (var optionIndex = 0; optionIndex < 1024; optionIndex++)
        {
            var definition = definitions + optionIndex * definitionSize;
            var keyPointer = Marshal.ReadIntPtr(definition);
            if (keyPointer == 0) break;
            var key = Marshal.PtrToStringUTF8(keyPointer)!;
            var name = StringAt(definition, IntPtr.Size) ?? key;
            var description = StringAt(definition, 3 * IntPtr.Size);
            var category = StringAt(definition, 5 * IntPtr.Size);
            var defaultValue = StringAt(definition, defaultOffset) ?? string.Empty;
            var values = new List<AmigaCoreOptionValue>();
            for (var valueIndex = 0; valueIndex < maximumValues; valueIndex++)
            {
                var valueOffset = valuesOffset + valueIndex * 2 * IntPtr.Size;
                var value = StringAt(definition, valueOffset);
                if (value is null) break;
                values.Add(new AmigaCoreOptionValue(value, StringAt(definition, valueOffset + IntPtr.Size) ?? value));
            }
            catalog.Add(new AmigaCoreOption(key, name, description, category, defaultValue, values,
                !_optionVisibility.TryGetValue(key, out var visible) || visible));
            if (!_options.ContainsKey(key) && defaultValue.Length > 0) _options[key] = defaultValue;
        }
        OptionCatalog = catalog;
    }

    private bool ApplyOptionVisibility(nint data)
    {
        if (data == 0) return false;
        var display = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionDisplay>(data);
        var key = display.Key == 0 ? null : Marshal.PtrToStringUTF8(display.Key);
        if (string.IsNullOrWhiteSpace(key)) return false;
        _optionVisibility[key] = display.Visible;
        OptionCatalog = OptionCatalog.Select(option => option.Key.Equals(key, StringComparison.Ordinal)
            ? option with { IsVisible = display.Visible }
            : option).ToArray();
        return true;
    }

    private bool CaptureOptionsDisplayCallback(nint data)
    {
        if (data == 0) return false;
        var callback = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionsUpdateDisplayCallback>(data).Callback;
        _updateOptionsDisplay = callback == 0
            ? null
            : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.UpdateCoreOptionsDisplay>(callback);
        return true;
    }

    private static string? StringAt(nint structure, int offset)
    {
        var pointer = Marshal.ReadIntPtr(structure, offset);
        return pointer == 0 ? null : Marshal.PtrToStringUTF8(pointer);
    }

    private bool ReturnOption(nint data)
    {
        if (data == 0) return true;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = Marshal.PtrToStringUTF8(variable.Key);
        if (key is not null && _options.TryGetValue(key, out var value))
        {
            variable.Value = NativeString(value);
            Marshal.StructureToPtr(variable, data, false);
            return true;
        }

        if (data != 0)
        {
            variable.Value = 0;
            Marshal.StructureToPtr(variable, data, false);
        }
        return true;
    }

    internal void ValidateConfiguredOptions()
    {
        foreach (var key in _configuredOptionKeys)
        {
            if (key.Equals("puae_kickstart", StringComparison.Ordinal)) continue;
            var configuredValue = _options[key];
            var option = OptionCatalog.FirstOrDefault(item => item.Key.Equals(key, StringComparison.Ordinal));
            if (option is null || option.Values.Count == 0) continue;
            if (!option.Values.Any(value => value.Value.Equals(configuredValue, StringComparison.Ordinal)))
                throw new InvalidDataException($"Invalid value '{configuredValue}' for Amiga option '{key}'.");
        }
    }

    private bool CaptureMessage(nint data, bool extended)
    {
        if (data == 0) return false;
        var textPointer = Marshal.ReadIntPtr(data);
        var message = textPointer == 0 ? null : Marshal.PtrToStringUTF8(textPointer);
        if (!string.IsNullOrWhiteSpace(message)) AddDiagnostic($"[message{(extended ? "-extended" : string.Empty)}] {message}");
        return true;
    }

    private bool CaptureControllerInfo(nint data)
    {
        if (data == 0)
        {
            ControllerPorts = [];
            return true;
        }
        var ports = new List<IReadOnlyList<AmigaControllerDevice>>();
        var infoSize = Marshal.SizeOf<ExternalCoreApi.ControllerInfo>();
        var descriptionSize = Marshal.SizeOf<ExternalCoreApi.ControllerDescription>();
        for (var port = 0; port < 16; port++)
        {
            var info = Marshal.PtrToStructure<ExternalCoreApi.ControllerInfo>(data + port * infoSize);
            if (info.Types == 0 || info.Count == 0) break;
            if (info.Count > 64) return false;
            var devices = new List<AmigaControllerDevice>(checked((int)info.Count));
            for (var index = 0; index < info.Count; index++)
            {
                var description = Marshal.PtrToStructure<ExternalCoreApi.ControllerDescription>(
                    info.Types + checked((int)index) * descriptionSize);
                var name = description.Description == 0 ? null : Marshal.PtrToStringUTF8(description.Description);
                if (!string.IsNullOrWhiteSpace(name)) devices.Add(new AmigaControllerDevice(name, description.Id));
            }
            ports.Add(devices);
        }
        ControllerPorts = ports;
        return true;
    }

    private nint NativeString(string value)
    {
        if (_nativeStrings.TryGetValue(value, out var existing)) return existing;
        var pointer = Marshal.StringToCoTaskMemUTF8(value);
        _nativeStrings.Add(value, pointer);
        return pointer;
    }

    private void HandleVideo(nint data, uint width, uint height, nuint pitch)
    {
        if (data == 0 || width == 0 || height == 0) return;
        var byteCount = checked((int)(pitch * height));
        var pixels = new byte[byteCount];
        Marshal.Copy(data, pixels, 0, byteCount);
        LatestVideoFrame = new VideoFrame(pixels, checked((int)width), checked((int)height),
            checked((int)pitch), _pixelFormat, _aspectRatio > 0 ? _aspectRatio : width / (float)height,
            ++_videoSequence, _clock.Elapsed);
    }

    internal void ApplyInitialAvInfo(ExternalCoreApi.SystemAvInfo info)
    {
        ApplyGeometry(info.Geometry);
        if (double.IsFinite(info.Timing.FramesPerSecond) && info.Timing.FramesPerSecond > 0)
            FramesPerSecond = info.Timing.FramesPerSecond;
        if (double.IsFinite(info.Timing.SampleRate) && info.Timing.SampleRate is > 0 and <= int.MaxValue)
            SampleRate = checked((int)Math.Round(info.Timing.SampleRate));
    }

    private bool ApplyGeometry(nint data)
    {
        if (data == 0) return false;
        ApplyGeometry(Marshal.PtrToStructure<ExternalCoreApi.Geometry>(data));
        return true;
    }

    private void ApplyGeometry(ExternalCoreApi.Geometry geometry)
    {
        if (float.IsFinite(geometry.AspectRatio) && geometry.AspectRatio > 0)
            _aspectRatio = geometry.AspectRatio;
        else if (geometry.BaseHeight > 0)
            _aspectRatio = geometry.BaseWidth / (float)geometry.BaseHeight;
    }

    private bool ApplySystemAvInfo(nint data)
    {
        if (data == 0) return false;
        ApplyInitialAvInfo(Marshal.PtrToStructure<ExternalCoreApi.SystemAvInfo>(data));
        return true;
    }

    private void HandleAudioSample(short left, short right)
    {
        PublishAudio(new AudioChunk(new[] { left, right }, SampleRate, 1,
            ++_audioSequence, _clock.Elapsed));
    }

    private nuint HandleAudioBatch(nint data, nuint frames)
    {
        if (data == 0 || frames == 0) return frames;
        var samples = new short[checked((int)frames * 2)];
        Marshal.Copy(data, samples, 0, samples.Length);
        PublishAudio(new AudioChunk(samples, SampleRate, checked((int)frames),
            ++_audioSequence, _clock.Elapsed));
        return frames;
    }

    private void PublishAudio(AudioChunk chunk)
    {
        LatestAudioChunk = chunk;
        lock (_audioGate)
        {
            var maximumFrames = Math.Max(1, SampleRate / 5);
            if (chunk.FrameCount > maximumFrames)
            {
                var retainedSamples = chunk.InterleavedStereo.Slice((chunk.FrameCount - maximumFrames) * 2).ToArray();
                chunk = new AudioChunk(retainedSamples, chunk.SampleRate, maximumFrames, chunk.Sequence, chunk.Timestamp);
                AudioOverrunCount++;
            }
            _audioChunks.Enqueue(chunk);
            _bufferedAudioFrames += chunk.FrameCount;
            while (_bufferedAudioFrames > maximumFrames && _audioChunks.Count > 1)
            {
                _bufferedAudioFrames -= _audioChunks.Dequeue().FrameCount;
                AudioOverrunCount++;
            }
        }
    }

    internal bool TryDequeueAudio(out AudioChunk? chunk)
    {
        lock (_audioGate)
        {
            if (!_audioChunks.TryDequeue(out chunk)) return false;
            _bufferedAudioFrames -= chunk.FrameCount;
            return true;
        }
    }

    private void HandleInputPoll()
    {
        lock (_inputGate)
        {
            _polledInput = _pendingInput;
            _pendingInput = _pendingInput with
            {
                Pointer = _pendingInput.Pointer with { DeltaX = 0, DeltaY = 0, Wheel = 0, HorizontalWheel = 0 }
            };
        }
        PublishKeyboardTransitions(_polledInput.Keys);
    }

    private void PublishKeyboardTransitions(IReadOnlySet<EmulationKey> keys)
    {
        if (_keyboardEvent is null) return;
        var reverseMap = KeyboardMap.ToDictionary(pair => pair.Value, pair => pair.Key);
        var modifiers = (ushort)((keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift) ? 1 : 0)
            | (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl) ? 2 : 0)
            | (keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt) ? 4 : 0));
        foreach (var key in _previousKeys.Except(keys).OrderBy(key => IsModifier(key) ? 1 : 0))
            if (reverseMap.TryGetValue(key, out var code)) _keyboardEvent(false, code, 0, modifiers);
        foreach (var key in keys.Except(_previousKeys).OrderBy(key => IsModifier(key) ? 0 : 1))
            if (reverseMap.TryGetValue(key, out var code)) _keyboardEvent(true, code, CharacterFor(code, keys), modifiers);
        _previousKeys = new HashSet<EmulationKey>(keys);
    }

    private static uint CharacterFor(uint code, IReadOnlySet<EmulationKey> keys)
    {
        var shifted = keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);
        var caps = keys.Contains(EmulationKey.CapsLock);
        if (code is >= (uint)'a' and <= (uint)'z') return shifted ^ caps ? code - 32 : code;
        if (!shifted) return code is >= 32 and <= 126 ? code : 0;
        return code switch
        {
            (uint)'0' => ')', (uint)'1' => '!', (uint)'2' => '@', (uint)'3' => '#', (uint)'4' => '$',
            (uint)'5' => '%', (uint)'6' => '^', (uint)'7' => '&', (uint)'8' => '*', (uint)'9' => '(',
            (uint)'-' => '_', (uint)'=' => '+', (uint)'[' => '{', (uint)']' => '}', (uint)'\\' => '|',
            (uint)';' => ':', (uint)'\'' => '"', (uint)',' => '<', (uint)'.' => '>', (uint)'/' => '?',
            (uint)'`' => '~', _ => code is >= 32 and <= 126 ? code : 0
        };
    }

    private static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift
        or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt
        or EmulationKey.LeftAmiga or EmulationKey.RightAmiga;

    private void HandleLog(int level, nint format)
    {
        var message = format == 0 ? null : Marshal.PtrToStringUTF8(format);
        if (!string.IsNullOrWhiteSpace(message)) AddDiagnostic($"[{level}] {message.TrimEnd()}");
    }

    private void HandleLed(int led, int state)
    {
        if (led is not (>= 0 and < 256)) return;
        _ledStates[led] = state != 0;
        if (state != 0)
            _ledActivityUntil[led] = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 140 / 1000;
    }

    private void AddDiagnostic(string message)
    {
        _diagnostics.Enqueue(message);
        while (_diagnostics.Count > 500) _diagnostics.TryDequeue(out _);
    }

    private short HandleInputState(uint port, uint device, uint index, uint id)
    {
        var input = _polledInput;
        if (device == KeyboardDevice)
            return KeyboardMap.TryGetValue(id, out var key) && input.Keys.Contains(key) ? (short)1 : (short)0;

        if (device == MouseDevice && port == 0)
            return id switch
            {
                0 => ClampToShort(input.Pointer.DeltaX),
                1 => ClampToShort(input.Pointer.DeltaY),
                2 => Bool(input.Pointer.Left),
                3 => Bool(input.Pointer.Right),
                4 => Bool(input.Pointer.Wheel > 0),
                5 => Bool(input.Pointer.Wheel < 0),
                6 => Bool(input.Pointer.Middle),
                _ => 0
            };

        if (port >= input.Controllers.Count) return 0;
        var controller = input.Controllers[(int)port];
        if (device == JoypadDevice)
        {
            if (id == JoypadMask) return unchecked((short)(controller.Buttons & ushort.MaxValue));
            return id < 32 && (controller.Buttons & (1u << (int)id)) != 0 ? (short)1 : (short)0;
        }
        if (device == AnalogDevice)
            return (index, id) switch
            {
                (0, 0) => controller.LeftX,
                (0, 1) => controller.LeftY,
                (1, 0) => controller.RightX,
                (1, 1) => controller.RightY,
                _ => 0
            };
        return 0;
    }

    private static short Bool(bool value) => value ? (short)1 : (short)0;
    private static short ClampToShort(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    private static int SaturatingAdd(int left, int right) => (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);

    private static IReadOnlyDictionary<uint, EmulationKey> CreateKeyboardMap()
    {
        var map = new Dictionary<uint, EmulationKey>
        {
            [8] = EmulationKey.Backspace, [9] = EmulationKey.Tab, [13] = EmulationKey.Return,
            [27] = EmulationKey.Escape, [32] = EmulationKey.Space, [44] = EmulationKey.Comma,
            [45] = EmulationKey.Minus, [46] = EmulationKey.Period, [47] = EmulationKey.Slash,
            [59] = EmulationKey.Semicolon, [61] = EmulationKey.Equals, [91] = EmulationKey.LeftBracket,
            [92] = EmulationKey.Backslash, [93] = EmulationKey.RightBracket, [39] = EmulationKey.Quote,
            [96] = EmulationKey.Backquote, [127] = EmulationKey.Delete,
            [273] = EmulationKey.Up, [274] = EmulationKey.Down, [275] = EmulationKey.Right,
            [276] = EmulationKey.Left, [277] = EmulationKey.Insert, [278] = EmulationKey.Home,
            [279] = EmulationKey.End, [280] = EmulationKey.PageUp, [281] = EmulationKey.PageDown,
            [301] = EmulationKey.CapsLock, [303] = EmulationKey.RightShift, [304] = EmulationKey.LeftShift,
            [305] = EmulationKey.RightControl, [306] = EmulationKey.LeftControl,
            [307] = EmulationKey.RightAlt, [308] = EmulationKey.LeftAlt,
            [311] = EmulationKey.LeftAmiga, [312] = EmulationKey.RightAmiga, [315] = EmulationKey.Help
        };
        for (var index = 0; index < 26; index++) map[(uint)('a' + index)] = (EmulationKey)((int)EmulationKey.A + index);
        for (var index = 0; index < 10; index++) map[(uint)('0' + index)] = (EmulationKey)((int)EmulationKey.D0 + index);
        for (var index = 0; index < 10; index++) map[(uint)(282 + index)] = (EmulationKey)((int)EmulationKey.F1 + index);
        for (var index = 0; index < 10; index++) map[(uint)(256 + index)] = (EmulationKey)((int)EmulationKey.Numpad0 + index);
        map[266] = EmulationKey.NumpadPeriod; map[267] = EmulationKey.NumpadDivide;
        map[268] = EmulationKey.NumpadMultiply; map[269] = EmulationKey.NumpadMinus;
        map[270] = EmulationKey.NumpadPlus; map[271] = EmulationKey.NumpadEnter;
        return map;
    }

    public void Dispose()
    {
        if (_disposed) return;
        foreach (var pointer in _nativeStrings.Values) Marshal.FreeCoTaskMem(pointer);
        _nativeStrings.Clear();
        _disposed = true;
    }
}
