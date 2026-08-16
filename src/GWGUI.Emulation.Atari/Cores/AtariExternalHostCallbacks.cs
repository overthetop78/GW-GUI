using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

internal sealed class AtariExternalHostCallbacks : IDisposable
{
    private readonly Dictionary<string, string> _optionValues;
    private readonly Dictionary<string, ExternalCoreUtf8String> _nativeOptionValues = new(StringComparer.Ordinal);
    private readonly List<AtariCoreOption> _options = [];
    private readonly ConcurrentQueue<AudioChunk> _audio = new();
    private readonly Dictionary<int, bool> _ledStates = [];
    private readonly ExternalCoreUtf8String _systemDirectory;
    private readonly ExternalCoreUtf8String _contentDirectory;
    private readonly ExternalCoreUtf8String _saveDirectory;
    private long _videoSequence;
    private long _audioSequence;
    private EmulationPixelFormat _pixelFormat = EmulationPixelFormat.Xrgb8888;
    private bool _disposed;

    internal AtariExternalHostCallbacks(string systemDirectory, string contentDirectory, string saveDirectory,
        IReadOnlyDictionary<string, string> configuredOptions)
    {
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        _systemDirectory = new ExternalCoreUtf8String(Path.GetFullPath(systemDirectory));
        _contentDirectory = new ExternalCoreUtf8String(Path.GetFullPath(contentDirectory));
        _saveDirectory = new ExternalCoreUtf8String(Path.GetFullPath(saveDirectory));
        _optionValues = new Dictionary<string, string>(configuredOptions, StringComparer.Ordinal);
        Environment = OnEnvironment;
        Video = OnVideo;
        AudioSample = OnAudioSample;
        AudioBatch = OnAudioBatch;
        InputPoll = OnInputPoll;
        InputState = OnInputState;
        SetLedState = OnSetLedState;
        Log = OnLog;
    }

    internal ExternalCoreApi.EnvironmentCallback Environment { get; }
    internal ExternalCoreApi.VideoCallback Video { get; }
    internal ExternalCoreApi.AudioSampleCallback AudioSample { get; }
    internal ExternalCoreApi.AudioBatchCallback AudioBatch { get; }
    internal ExternalCoreApi.InputPollCallback InputPoll { get; }
    internal ExternalCoreApi.InputStateCallback InputState { get; }
    internal ExternalCoreApi.SetLedState SetLedState { get; }
    internal ExternalCoreApi.LogCallback Log { get; }
    internal EmulationInputSnapshot Input { get; set; } = EmulationInputSnapshot.Empty;
    internal VideoFrame? LatestVideoFrame { get; private set; }
    internal AudioChunk? LatestAudioChunk { get; private set; }
    internal IReadOnlyList<AtariCoreOption> Options => _options;
    internal List<string> Diagnostics { get; } = [];
    internal IReadOnlyDictionary<int, bool> LedStates => _ledStates;
    internal bool SupportsNoGame { get; private set; }
    internal double FramesPerSecond { get; private set; }
    internal int SampleRate { get; private set; }
    internal float AspectRatio { get; private set; }

    internal bool TryDequeueAudio(out AudioChunk? chunk) => _audio.TryDequeue(out chunk);

    internal void ApplySystemAvInfo(ExternalCoreApi.SystemAvInfo info)
    {
        FramesPerSecond = info.Timing.FramesPerSecond;
        SampleRate = checked((int)Math.Round(info.Timing.SampleRate));
        AspectRatio = info.Geometry.AspectRatio;
    }

    internal void SetOption(string key, string value)
    {
        var option = _options.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.Ordinal));
        if (option is null || option.Values.All(item => !string.Equals(item.Value, value, StringComparison.Ordinal)))
            throw new AtariEmulationException(AtariErrorKind.Option, AtariErrorCode.OptionInvalid,
                AtariCoreFunctions.CreateInvalidOptionValueMessage(key, value));
        _optionValues[key] = value;
        ReplaceNativeOptionValue(key, value);
        var index = _options.IndexOf(option);
        _options[index] = option with { CurrentValue = value };
    }

    private bool OnEnvironment(uint command, nint data)
    {
        switch (command)
        {
            case ExternalCoreApiConstants.GetSystemDirectory:
                return AtariCoreFunctions.WritePointer(data, _systemDirectory.Pointer);
            case ExternalCoreApiConstants.GetContentDirectory:
                return AtariCoreFunctions.WritePointer(data, _contentDirectory.Pointer);
            case ExternalCoreApiConstants.GetSaveDirectory:
                return AtariCoreFunctions.WritePointer(data, _saveDirectory.Pointer);
            case ExternalCoreApiConstants.SetPixelFormat:
                return SetPixelFormat(data);
            case ExternalCoreApiConstants.GetVariable:
                return GetVariable(data);
            case ExternalCoreApiConstants.SetVariables:
                return ReadLegacyOptions(data);
            case ExternalCoreApiConstants.GetVariableUpdate:
                return AtariCoreFunctions.WriteBoolean(data, false);
            case ExternalCoreApiConstants.SetSupportNoGame:
                SupportsNoGame = data != 0 && Marshal.ReadByte(data) != 0;
                return data != 0;
            case ExternalCoreApiConstants.GetCanDuplicateFrames:
            case ExternalCoreApiConstants.GetInputBitmasks:
                return AtariCoreFunctions.WriteBoolean(data, true);
            case ExternalCoreApiConstants.GetCoreOptionsVersion:
                return AtariCoreFunctions.WriteInteger(data, AtariConstants.LegacyCoreOptionsVersion);
            case ExternalCoreApiConstants.GetMessageInterfaceVersion:
                return AtariCoreFunctions.WriteInteger(data, AtariConstants.MessageInterfaceVersion);
            case ExternalCoreApiConstants.SetSystemAvInfo:
                return SetSystemAvInfo(data);
            case ExternalCoreApiConstants.SetGeometry:
                return SetGeometry(data);
            case ExternalCoreApiConstants.SetMessage:
                return ReadMessage(data);
            case ExternalCoreApiConstants.SetMessageExtended:
                return ReadExtendedMessage(data);
            case ExternalCoreApiConstants.GetLogInterface:
                return SetLogInterface(data);
            case ExternalCoreApiConstants.GetLedInterface:
                return SetLedInterface(data);
            case ExternalCoreApiConstants.SetInputDescriptors:
            case ExternalCoreApiConstants.SetKeyboardCallback:
            case ExternalCoreApiConstants.SetControllerInfo:
            case ExternalCoreApiConstants.SetMemoryMaps:
            case ExternalCoreApiConstants.SetSupportAchievements:
                return true;
            case ExternalCoreApiConstants.SetDiskControl:
            case ExternalCoreApiConstants.SetDiskControlExtended:
            case ExternalCoreApiConstants.GetVfsInterface:
            case ExternalCoreApiConstants.SetCoreOptionsV2:
            case ExternalCoreApiConstants.SetCoreOptionsV2International:
            case ExternalCoreApiConstants.SetCoreOptionsDisplay:
            case ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback:
            case ExternalCoreApiConstants.SetFastForwardingOverride:
                return false;
            default:
                return false;
        }
    }

    private bool SetPixelFormat(nint data)
    {
        if (data == 0) return false;
        switch (Marshal.ReadInt32(data))
        {
            case AtariConstants.PixelFormatXrgb8888:
                _pixelFormat = EmulationPixelFormat.Xrgb8888;
                return true;
            case AtariConstants.PixelFormatRgb565:
                _pixelFormat = EmulationPixelFormat.Rgb565;
                return true;
            case AtariConstants.PixelFormat0Rgb1555:
            default:
                return false;
        }
    }

    private bool GetVariable(nint data)
    {
        if (data == 0) return true;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = Marshal.PtrToStringUTF8(variable.Key);
        variable.Value = key is not null && _optionValues.TryGetValue(key, out var value)
            ? GetNativeOptionValue(key, value)
            : 0;
        Marshal.StructureToPtr(variable, data, false);
        return true;
    }

    private bool ReadLegacyOptions(nint data)
    {
        if (data == 0) return false;
        _options.Clear();
        var size = Marshal.SizeOf<ExternalCoreApi.Variable>();
        for (var index = 0; index < AtariConstants.MaximumCoreOptionCount; index++)
        {
            var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data + index * size);
            if (variable.Key == 0) return true;
            var key = Marshal.PtrToStringUTF8(variable.Key);
            var definition = Marshal.PtrToStringUTF8(variable.Value);
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(definition)) continue;
            var separator = definition.IndexOf(AtariConstants.LegacyOptionNameSeparator);
            var name = separator < 0 ? key : definition[..separator].Trim();
            var values = (separator < 0
                    ? definition
                    : definition[(separator + AtariConstants.LegacyOptionValueStartOffset)..])
                .Split(AtariConstants.LegacyOptionValueSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (values.Length == 0) continue;
            var selected = _optionValues.TryGetValue(key, out var configured) && values.Contains(configured, StringComparer.Ordinal)
                ? configured : values[0];
            _optionValues[key] = selected;
            ReplaceNativeOptionValue(key, selected);
            _options.Add(new AtariCoreOption(key, name, null, null, values[0], selected,
                values.Select(value => new AtariCoreOptionValue(value, value)).ToArray()));
        }
        return false;
    }

    private bool SetSystemAvInfo(nint data)
    {
        if (data == 0) return false;
        var info = Marshal.PtrToStructure<ExternalCoreApi.SystemAvInfo>(data);
        ApplySystemAvInfo(info);
        return true;
    }

    private bool SetGeometry(nint data)
    {
        if (data == 0) return false;
        AspectRatio = Marshal.PtrToStructure<ExternalCoreApi.Geometry>(data).AspectRatio;
        return true;
    }

    private bool ReadMessage(nint data)
    {
        if (data == 0) return false;
        var message = Marshal.PtrToStructure<ExternalCoreApi.Message>(data);
        AddDiagnostic(message.Text);
        return true;
    }

    private bool ReadExtendedMessage(nint data)
    {
        if (data == 0) return false;
        var message = Marshal.PtrToStructure<ExternalCoreApi.MessageExtended>(data);
        AddDiagnostic(message.Text);
        return true;
    }

    private void AddDiagnostic(nint textPointer)
    {
        var text = Marshal.PtrToStringUTF8(textPointer);
        if (!string.IsNullOrWhiteSpace(text)) Diagnostics.Add(text);
    }

    private bool SetLogInterface(nint data)
    {
        if (data == 0) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.LogInterface
        {
            Log = Marshal.GetFunctionPointerForDelegate(Log)
        }, data, false);
        return true;
    }

    private bool SetLedInterface(nint data)
    {
        if (data == 0) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.LedInterface
        {
            SetLedState = Marshal.GetFunctionPointerForDelegate(SetLedState)
        }, data, false);
        return true;
    }

    private void OnVideo(nint data, uint width, uint height, nuint pitch)
    {
        if (data == 0 || width == 0 || height == 0 || pitch == 0) return;
        var length = checked((int)(pitch * height));
        if (length > EmulationHostProtocolConstants.VideoSlotCapacity) return;
        var pixels = GC.AllocateUninitializedArray<byte>(length);
        Marshal.Copy(data, pixels, AtariConstants.FirstBufferIndex, length);
        LatestVideoFrame = new VideoFrame(pixels, checked((int)width), checked((int)height), checked((int)pitch),
            _pixelFormat, AspectRatio, ++_videoSequence, TimeSpan.Zero);
    }

    private void OnAudioSample(short left, short right)
    {
        AddAudio(new short[] { left, right }, AtariConstants.SingleAudioFrameCount);
    }

    private nuint OnAudioBatch(nint data, nuint frames)
    {
        if (data == 0 || frames == 0 || frames > AtariConstants.MaximumAudioFramesPerBatch) return 0;
        var frameCount = checked((int)frames);
        var samples = GC.AllocateUninitializedArray<short>(checked(frameCount * AtariConstants.StereoChannelCount));
        Marshal.Copy(data, samples, AtariConstants.FirstBufferIndex, samples.Length);
        AddAudio(samples, frameCount);
        return frames;
    }

    private void AddAudio(ReadOnlyMemory<short> samples, int frameCount)
    {
        var chunk = new AudioChunk(samples, SampleRate, frameCount, ++_audioSequence, TimeSpan.Zero);
        LatestAudioChunk = chunk;
        _audio.Enqueue(chunk);
    }

    private void OnInputPoll() { }
    private short OnInputState(uint port, uint device, uint index, uint id) => (short)AtariConstants.NoInputState;
    private void OnSetLedState(int led, int state) => _ledStates[led] = state != 0;

    private void OnLog(int level, nint format)
    {
        var text = Marshal.PtrToStringUTF8(format);
        if (!string.IsNullOrWhiteSpace(text)) Diagnostics.Add(text.Trim());
    }

    private nint GetNativeOptionValue(string key, string value)
    {
        if (!_nativeOptionValues.TryGetValue(key, out var native))
        {
            native = new ExternalCoreUtf8String(value);
            _nativeOptionValues.Add(key, native);
        }
        return native.Pointer;
    }

    private void ReplaceNativeOptionValue(string key, string value)
    {
        if (_nativeOptionValues.Remove(key, out var previous)) previous.Dispose();
        _nativeOptionValues.Add(key, new ExternalCoreUtf8String(value));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var value in _nativeOptionValues.Values) value.Dispose();
        _nativeOptionValues.Clear();
        _systemDirectory.Dispose();
        _contentDirectory.Dispose();
        _saveDirectory.Dispose();
    }
}
