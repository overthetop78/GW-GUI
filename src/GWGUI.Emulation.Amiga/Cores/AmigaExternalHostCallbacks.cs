using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Cores;

internal sealed class AmigaExternalHostCallbacks : IDisposable
{
    private readonly Dictionary<string, string> _options = new(StringComparer.Ordinal);
    private readonly Dictionary<string, nint> _nativeStrings = new(StringComparer.Ordinal);
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private EmulationPixelFormat _pixelFormat = EmulationPixelFormat.Rgb565;
    private long _videoSequence;
    private long _audioSequence;
    private bool _disposed;

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
                case AmigaExternalApi.SetDiskControl:
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
        // Les mappages Amiga complets sont raccordés après le premier démarrage vidéo.
        return 0;
    }

    public void Dispose()
    {
        if (_disposed) return;
        foreach (var pointer in _nativeStrings.Values) Marshal.FreeCoTaskMem(pointer);
        _nativeStrings.Clear();
        _disposed = true;
    }
}
