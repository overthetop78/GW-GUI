using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;

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
    private readonly Dictionary<string, nint> _nativeStrings = new(StringComparer.Ordinal);
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private EmulationPixelFormat _pixelFormat = EmulationPixelFormat.Rgb565;
    private long _videoSequence;
    private long _audioSequence;
    private bool _disposed;
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
                _options[option.Key] = option.Value;

        Environment = HandleEnvironment;
        Video = HandleVideo;
        AudioSample = HandleAudioSample;
        AudioBatch = HandleAudioBatch;
        InputPoll = HandleInputPoll;
        InputState = HandleInputState;
        Log = HandleLog;
    }

    internal string SystemDirectory { get; }
    internal string ContentDirectory { get; }
    internal string SaveDirectory { get; }
    internal VideoFrame? LatestVideoFrame { get; private set; }
    internal AudioChunk? LatestAudioChunk { get; private set; }
    internal EmulationInputSnapshot Input { get; set; } = EmulationInputSnapshot.Empty;
    internal AmigaExternalApi.EnvironmentCallback Environment { get; }
    internal AmigaExternalApi.VideoCallback Video { get; }
    internal AmigaExternalApi.AudioSampleCallback AudioSample { get; }
    internal AmigaExternalApi.AudioBatchCallback AudioBatch { get; }
    internal AmigaExternalApi.InputPollCallback InputPoll { get; }
    internal AmigaExternalApi.InputStateCallback InputState { get; }
    internal AmigaExternalApi.LogCallback Log { get; }
    internal int SampleRate { get; set; } = 44100;

    internal void SetOption(string key, string value) => _options[key] = value;

    private bool HandleEnvironment(uint command, nint data)
    {
        try
        {
            switch (command)
            {
                case AmigaExternalApi.GetSystemDirectory:
                    Marshal.WriteIntPtr(data, NativeString(SystemDirectory));
                    return true;
                case AmigaExternalApi.GetContentDirectory:
                    Marshal.WriteIntPtr(data, NativeString(ContentDirectory));
                    return true;
                case AmigaExternalApi.GetSaveDirectory:
                    Marshal.WriteIntPtr(data, NativeString(SaveDirectory));
                    return true;
                case AmigaExternalApi.GetCanDuplicateFrames:
                case AmigaExternalApi.GetInputBitmasks:
                    if (data != 0) Marshal.WriteByte(data, 1);
                    return true;
                case AmigaExternalApi.SetPixelFormat:
                    _pixelFormat = Marshal.ReadInt32(data) switch
                    {
                        1 => EmulationPixelFormat.Xrgb8888,
                        2 => EmulationPixelFormat.Rgb565,
                        var value => throw new NotSupportedException($"Pixel format {value} is not supported.")
                    };
                    return true;
                case AmigaExternalApi.GetCoreOptionsVersion:
                    if (data != 0) Marshal.WriteInt32(data, 2);
                    return true;
                case AmigaExternalApi.SetCoreOptionsV2:
                case AmigaExternalApi.SetCoreOptionsV2International:
                    return true;
                case AmigaExternalApi.SetVariables:
                    RegisterLegacyOptions(data);
                    return true;
                case AmigaExternalApi.GetVariable:
                    return ReturnOption(data);
                case AmigaExternalApi.GetVariableUpdate:
                    if (data != 0) Marshal.WriteByte(data, 0);
                    return true;
                case AmigaExternalApi.GetDiskControlVersion:
                    if (data != 0) Marshal.WriteInt32(data, 0);
                    return true;
                case AmigaExternalApi.SetInputDescriptors:
                case AmigaExternalApi.SetKeyboardCallback:
                    return true;
                case AmigaExternalApi.SetDiskControl:
                    DiskControl.Capture(data);
                    return true;
                case AmigaExternalApi.SetDiskControlExtended:
                case AmigaExternalApi.SetControllerInfo:
                case AmigaExternalApi.SetMemoryMaps:
                case AmigaExternalApi.SetSupportNoGame:
                case AmigaExternalApi.SetSupportAchievements:
                case AmigaExternalApi.SetCoreOptionsDisplay:
                case AmigaExternalApi.SetCoreOptionsUpdateDisplayCallback:
                case AmigaExternalApi.SetFastForwardingOverride:
                    return true;
                case AmigaExternalApi.SetGeometry:
                case AmigaExternalApi.SetSystemAvInfo:
                    return true;
                case AmigaExternalApi.GetLogInterface:
                    Marshal.StructureToPtr(new AmigaExternalApi.LogInterface
                    {
                        Log = Marshal.GetFunctionPointerForDelegate(Log)
                    }, data, false);
                    return true;
                case AmigaExternalApi.GetVfsInterface:
                case AmigaExternalApi.GetLedInterface:
                    return false;
                default:
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
        var size = Marshal.SizeOf<AmigaExternalApi.Variable>();
        for (var current = data; current != 0; current += size)
        {
            var variable = Marshal.PtrToStructure<AmigaExternalApi.Variable>(current);
            if (variable.Key == 0) break;
            var key = Marshal.PtrToStringUTF8(variable.Key)!;
            if (_options.ContainsKey(key)) continue;
            var definition = Marshal.PtrToStringUTF8(variable.Value);
            var values = definition?.Split(';', 2).ElementAtOrDefault(1)?.Trim();
            var defaultValue = values?.Split('|', 2)[0];
            if (!string.IsNullOrWhiteSpace(defaultValue)) _options[key] = defaultValue;
        }
    }

    private bool ReturnOption(nint data)
    {
        var variable = Marshal.PtrToStructure<AmigaExternalApi.Variable>(data);
        var key = Marshal.PtrToStringUTF8(variable.Key);
        if (key is not null && _options.TryGetValue(key, out var value))
        {
            variable.Value = NativeString(value);
            Marshal.StructureToPtr(variable, data, false);
            return true;
        }

        variable.Value = 0;
        Marshal.StructureToPtr(variable, data, false);
        return false;
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
            checked((int)pitch), _pixelFormat, width / (float)height, ++_videoSequence, _clock.Elapsed);
    }

    private void HandleAudioSample(short left, short right)
    {
        LatestAudioChunk = new AudioChunk(new[] { left, right }, SampleRate, 1,
            ++_audioSequence, _clock.Elapsed);
    }

    private nuint HandleAudioBatch(nint data, nuint frames)
    {
        if (data == 0 || frames == 0) return frames;
        var samples = new short[checked((int)frames * 2)];
        Marshal.Copy(data, samples, 0, samples.Length);
        LatestAudioChunk = new AudioChunk(samples, SampleRate, checked((int)frames),
            ++_audioSequence, _clock.Elapsed);
        return frames;
    }

    private static void HandleInputPoll() { }

    private static void HandleLog(int level, nint format)
    {
        // Le callback consomme correctement l'appel variadique natif. Les journaux structurés
        // seront raccordés au diagnostic de l'application sans interpréter ici les arguments C.
    }

    private short HandleInputState(uint port, uint device, uint index, uint id)
    {
        var input = Input;
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
