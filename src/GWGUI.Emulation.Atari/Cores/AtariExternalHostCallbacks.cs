using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

internal sealed class AtariExternalHostCallbacks : IDisposable
{
    private readonly AtariCoreOptionHost _optionHost;
    private readonly AtariVideoBufferSet _videoBuffers = new();
    private readonly AtariInputFrameStore _input = new();
    private readonly long _videoStartTimestamp = Stopwatch.GetTimestamp();
    private readonly AtariAudioBuffer _audio = new();
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
    private readonly AtariDiskControl _diskControl = new();

    internal AtariExternalHostCallbacks(string systemDirectory, string contentDirectory, string saveDirectory,
        string assetsDirectory,
        IReadOnlyDictionary<string, string> configuredOptions)
    {
        Directory.CreateDirectory(systemDirectory);
        Directory.CreateDirectory(contentDirectory);
        Directory.CreateDirectory(saveDirectory);
        Directory.CreateDirectory(assetsDirectory);
        _systemDirectory = new ExternalCoreUtf8String(Path.GetFullPath(systemDirectory));
        _contentDirectory = new ExternalCoreUtf8String(Path.GetFullPath(contentDirectory));
        _saveDirectory = new ExternalCoreUtf8String(Path.GetFullPath(saveDirectory));
        _assetsDirectory = new ExternalCoreUtf8String(Path.GetFullPath(assetsDirectory));
        _optionHost = new AtariCoreOptionHost(configuredOptions);
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
    internal EmulationInputSnapshot Input { set => _input.Update(value); }
    internal VideoFrame? LatestVideoFrame { get; private set; }
    internal AudioChunk? LatestAudioChunk { get; private set; }
    internal IReadOnlyList<AtariCoreOption> Options => _optionHost.Catalog;
    internal IReadOnlyList<AtariCoreOptionCategory> OptionCategories => _optionHost.Categories;
    internal IReadOnlyDictionary<string, string> OptionDocumentValues => _optionHost.DocumentValues;
    internal List<string> Diagnostics { get; } = [];
    internal IReadOnlyDictionary<int, bool> LedStates => _ledStates;
    internal IReadOnlyList<AtariInputDescriptor> InputDescriptors { get; private set; } = [];
    internal IReadOnlyList<AtariControllerPort> ControllerPorts { get; private set; } = [];
    internal IReadOnlyList<AtariMemoryDescriptor> MemoryDescriptors { get; private set; } = [];
    internal nint KeyboardCallbackPointer { get; private set; }
    internal uint Rotation { get; private set; } = AtariEnvironmentConstants.NoRotation;
    internal bool SupportsAchievements { get; private set; }
    internal uint PerformanceLevel { get; private set; }
    internal ExternalCoreApi.Geometry Geometry { get; private set; }
    internal ExternalCoreApi.SystemAvInfo SystemAvInfo { get; private set; }
    internal List<AtariEnvironmentMessage> Messages { get; } = [];
    internal List<AtariEnvironmentExtendedMessage> ExtendedMessages { get; } = [];
    internal IReadOnlySet<uint> EnvironmentCommands => _environmentCommands;
    internal bool SupportsNoGame { get; private set; }
    internal double FramesPerSecond { get; private set; }
    internal int SampleRate { get; private set; }
    internal int BufferedAudioFrames => _audio.BufferedFrames;
    internal long AudioOverrunCount => _audio.OverrunCount;
    internal long AudioUnderrunCount => _audio.UnderrunCount;
    internal float AspectRatio { get; private set; }
    internal AtariDiskControl DiskControl => _diskControl;

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

    private bool OnEnvironment(uint command, nint data)
    {
        _environmentCommands.Add(command);
        switch (command)
        {
            case ExternalCoreApiConstants.SetRotation:
                return SetRotation(data);
            case ExternalCoreApiConstants.GetOverscan:
                return AtariCoreFunctions.WriteBoolean(data, false);
            case ExternalCoreApiConstants.SetPerformanceLevel:
                return CapturePerformanceLevel(data);
            case ExternalCoreApiConstants.GetSystemDirectory:
                return AtariCoreFunctions.WritePointer(data, _systemDirectory.Pointer);
            case ExternalCoreApiConstants.GetContentDirectory:
                return AtariCoreFunctions.WritePointer(data, _assetsDirectory.Pointer);
            case ExternalCoreApiConstants.GetSaveDirectory:
                return AtariCoreFunctions.WritePointer(data, _saveDirectory.Pointer);
            case ExternalCoreApiConstants.SetPixelFormat:
                return SetPixelFormat(data);
            case ExternalCoreApiConstants.GetVariable:
                return _optionHost.ReturnValue(data);
            case ExternalCoreApiConstants.SetVariables:
                _optionHost.RegisterLegacyVariables(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.GetVariableUpdate:
                return _optionHost.GetAndClearUpdated(data);
            case ExternalCoreApiConstants.SetSupportNoGame:
                SupportsNoGame = data != nint.Zero && Marshal.ReadByte(data) != AtariConstants.NativeBooleanFalse;
                return data != nint.Zero;
            case ExternalCoreApiConstants.GetCanDuplicateFrames:
            case ExternalCoreApiConstants.GetInputBitmasks:
                return AtariCoreFunctions.WriteBoolean(data, true);
            case ExternalCoreApiConstants.GetCoreOptionsVersion:
                return AtariCoreFunctions.WriteInteger(data, AtariCoreOptionConstants.SupportedInterfaceVersion);
            case ExternalCoreApiConstants.SetCoreOptions:
                _optionHost.RegisterVersionOne(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetCoreOptionsInternational:
                _optionHost.RegisterVersionOneInternational(data);
                return data != nint.Zero;
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
            case ExternalCoreApiConstants.GetPerformanceInterface:
                return false;
            case ExternalCoreApiConstants.GetLedInterface:
                return SetLedInterface(data);
            case ExternalCoreApiConstants.GetRumbleInterface:
                return SetRumbleInterface(data);
            case ExternalCoreApiConstants.GetSensorInterface:
                return SetSensorInterface(data);
            case ExternalCoreApiConstants.GetInputDeviceCapabilities:
                return AtariCoreFunctions.WriteUnsignedLong(data,
                    AtariEnvironmentConstants.JoypadCapability | AtariEnvironmentConstants.MouseCapability |
                    AtariEnvironmentConstants.KeyboardCapability | AtariEnvironmentConstants.AnalogCapability);
            case ExternalCoreApiConstants.GetLanguage:
                return AtariCoreFunctions.WriteInteger(data, AtariEnvironmentFunctions.CurrentLanguage());
            case ExternalCoreApiConstants.GetFastForwarding:
                return AtariCoreFunctions.WriteBoolean(data, false);
            case ExternalCoreApiConstants.SetInputDescriptors:
                InputDescriptors = AtariEnvironmentFunctions.CopyInputDescriptors(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetKeyboardCallback:
                return CaptureKeyboardCallback(data);
            case ExternalCoreApiConstants.SetControllerInfo:
                ControllerPorts = AtariEnvironmentFunctions.CopyControllerPorts(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetMemoryMaps:
                MemoryDescriptors = AtariEnvironmentFunctions.CopyMemoryMap(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetSupportAchievements:
                SupportsAchievements = data != nint.Zero && Marshal.ReadByte(data) != AtariConstants.NativeBooleanFalse;
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetDiskControl:
                if (data == nint.Zero) return false;
                _diskControl.Capture(data);
                return true;
            case ExternalCoreApiConstants.SetDiskControlExtended:
                if (data == nint.Zero) return false;
                _diskControl.CaptureExtended(data);
                return true;
            case ExternalCoreApiConstants.GetDiskControlVersion:
                return AtariCoreFunctions.WriteInteger(data, AtariDiskControlConstants.InterfaceVersion);
            case ExternalCoreApiConstants.SetCoreOptionsV2:
                _optionHost.RegisterVersionTwo(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetCoreOptionsV2International:
                _optionHost.RegisterVersionTwoInternational(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetCoreOptionsDisplay:
                return _optionHost.ApplyVisibility(data);
            case ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback:
                return _optionHost.CaptureDisplayUpdate(data);
            case ExternalCoreApiConstants.SetVariable:
                return _optionHost.SetNativeValue(data);
            case ExternalCoreApiConstants.GetVfsInterface:
            case ExternalCoreApiConstants.GetMidiInterface:
            case ExternalCoreApiConstants.SetFastForwardingOverride:
            case ExternalCoreApiConstants.SetContentInfoOverride:
            case ExternalCoreApiConstants.SetNetworkPacketInterface:
            case ExternalCoreApiConstants.SetSerializationQuirks:
                return false;
            default:
                if (_unknownEnvironmentCommands.Add(command))
                    Diagnostics.Add(AtariEnvironmentFunctions.CreateUnknownCommandDiagnostic(command));
                return false;
        }
    }

    private bool SetRotation(nint data)
    {
        if (data == nint.Zero) return false;
        var rotation = unchecked((uint)Marshal.ReadInt32(data));
        if (rotation is < AtariEnvironmentConstants.FirstRotation or > AtariEnvironmentConstants.LastRotation)
            return false;
        Rotation = rotation;
        return true;
    }

    private bool CapturePerformanceLevel(nint data)
    {
        if (data == nint.Zero) return false;
        PerformanceLevel = unchecked((uint)Marshal.ReadInt32(data));
        return true;
    }

    private bool CaptureKeyboardCallback(nint data)
    {
        if (data == nint.Zero) return false;
        KeyboardCallbackPointer = Marshal.PtrToStructure<ExternalCoreApi.KeyboardCallback>(data).Callback;
        return true;
    }

    private bool SetPixelFormat(nint data)
    {
        if (data == nint.Zero) return false;
        switch (Marshal.ReadInt32(data))
        {
            case AtariConstants.PixelFormatXrgb8888:
                _pixelFormat = EmulationPixelFormat.Xrgb8888;
                return true;
            case AtariConstants.PixelFormatRgb565:
                _pixelFormat = EmulationPixelFormat.Rgb565;
                return true;
            case AtariConstants.PixelFormat0Rgb1555:
                _pixelFormat = EmulationPixelFormat.Rgb1555;
                return true;
            default:
                return false;
        }
    }

    private bool SetSystemAvInfo(nint data)
    {
        if (data == nint.Zero) return false;
        var info = Marshal.PtrToStructure<ExternalCoreApi.SystemAvInfo>(data);
        ApplySystemAvInfo(info);
        return true;
    }

    private bool SetGeometry(nint data)
    {
        if (data == nint.Zero) return false;
        Geometry = Marshal.PtrToStructure<ExternalCoreApi.Geometry>(data);
        AspectRatio = Geometry.AspectRatio;
        return true;
    }

    private bool ReadMessage(nint data)
    {
        if (data == nint.Zero) return false;
        var message = Marshal.PtrToStructure<ExternalCoreApi.Message>(data);
        var text = CopyDiagnostic(message.Text);
        if (text is not null) Messages.Add(new(text, message.Frames));
        return true;
    }

    private bool ReadExtendedMessage(nint data)
    {
        if (data == nint.Zero) return false;
        var message = Marshal.PtrToStructure<ExternalCoreApi.MessageExtended>(data);
        var text = CopyDiagnostic(message.Text);
        if (text is not null) ExtendedMessages.Add(new(text, message.DurationMilliseconds, message.Priority,
            message.Level, message.Target, message.Type, message.Progress));
        return true;
    }

    private string? CopyDiagnostic(nint textPointer)
    {
        var text = Marshal.PtrToStringUTF8(textPointer);
        if (!string.IsNullOrWhiteSpace(text)) Diagnostics.Add(text);
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private bool SetLogInterface(nint data)
    {
        if (data == nint.Zero) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.LogInterface
        {
            Log = Marshal.GetFunctionPointerForDelegate(Log)
        }, data, false);
        return true;
    }

    private bool SetLedInterface(nint data)
    {
        if (data == nint.Zero) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.LedInterface
        {
            SetLedState = Marshal.GetFunctionPointerForDelegate(SetLedState)
        }, data, false);
        return true;
    }

    private bool SetRumbleInterface(nint data)
    {
        if (data == nint.Zero) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.RumbleInterface
        {
            SetState = Marshal.GetFunctionPointerForDelegate(SetRumbleState)
        }, data, false);
        return true;
    }

    private bool SetSensorInterface(nint data)
    {
        if (data == nint.Zero) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.SensorInterface
        {
            SetState = Marshal.GetFunctionPointerForDelegate(SetSensorState),
            GetInput = Marshal.GetFunctionPointerForDelegate(GetSensorInput)
        }, data, false);
        return true;
    }

    private void OnVideo(nint data, uint width, uint height, nuint pitch)
    {
        if (data == nint.Zero)
        {
            if (LatestVideoFrame is { } previous)
                LatestVideoFrame = previous with
                {
                    Sequence = ++_videoSequence,
                    Timestamp = AtariVideoFunctions.Timestamp(_videoStartTimestamp)
                };
            return;
        }
        if (width == AtariConstants.EmptyFrameDimension ||
            height == AtariConstants.EmptyFrameDimension || pitch == AtariConstants.EmptyNativeSize) return;
        var length = AtariVideoFunctions.FrameLength(height, pitch);
        if (length > EmulationHostProtocolConstants.VideoSlotCapacity) return;
        var pixels = _videoBuffers.Rent(length);
        AtariVideoFunctions.CopyRows(data, pixels, checked((int)height), checked((int)pitch));
        LatestVideoFrame = new VideoFrame(pixels.AsMemory(AtariConstants.FirstBufferIndex, length),
            checked((int)width), checked((int)height), checked((int)pitch), _pixelFormat, AspectRatio,
            ++_videoSequence, AtariVideoFunctions.Timestamp(_videoStartTimestamp));
    }

    private void OnAudioSample(short left, short right)
    {
        AddAudio(AtariAudioFunctions.SingleFrame(left, right), AtariAudioConstants.SingleFrameCount);
    }

    private nuint OnAudioBatch(nint data, nuint frames)
    {
        if (data == nint.Zero || frames == AtariConstants.EmptyNativeSize ||
            frames > AtariAudioConstants.MaximumFramesPerBatch) return AtariConstants.EmptyNativeSize;
        var frameCount = checked((int)frames);
        var samples = AtariAudioFunctions.CopyBatch(data, frameCount);
        AddAudio(samples, frameCount);
        return frames;
    }

    private void AddAudio(ReadOnlyMemory<short> samples, int frameCount)
    {
        var chunk = new AudioChunk(samples, SampleRate, frameCount, ++_audioSequence, TimeSpan.Zero);
        LatestAudioChunk = chunk;
        _audio.Enqueue(chunk);
    }

    private void OnInputPoll() => _input.Poll();
    private short OnInputState(uint port, uint device, uint index, uint id) => _input.State(port, device, index, id);
    private void OnSetLedState(int led, int state) => _ledStates[led] = state != AtariConstants.InactiveState;
    private bool OnSetRumbleState(uint port, uint effect, ushort strength) => false;
    private bool OnSetSensorState(uint port, uint action, uint rate) => false;
    private float OnGetSensorInput(uint port, uint id) => AtariEnvironmentConstants.NoSensorInput;

    private void OnLog(int level, nint format)
    {
        var text = AtariEnvironmentFunctions.CopyNativeLogTemplate(format);
        if (!string.IsNullOrEmpty(text)) Diagnostics.Add(text);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _optionHost.Dispose();
        _videoBuffers.Dispose();
        _systemDirectory.Dispose();
        _contentDirectory.Dispose();
        _saveDirectory.Dispose();
        _assetsDirectory.Dispose();
    }
}
