using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalCoreEnvironmentCallbackHost : IEmulationEnvironmentCallbackHost
{
    private readonly IEmulationControllerFeedbackAccumulator _feedback = new EmulationControllerFeedbackAccumulator(EmulationHostProtocolConstants.MaximumInputControllerCount);
    private readonly ExternalCoreApi.SetRumbleState _setRumbleState;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void AudioBufferStatusCallback([MarshalAs(UnmanagedType.I1)] bool active, uint occupancy, [MarshalAs(UnmanagedType.I1)] bool underrunLikely);
    private static readonly IReadOnlyDictionary<EmulationKey, uint> KeyboardMap = ExternalCoreKeyboardFunctions.CreateKeyMap();
    private IReadOnlySet<EmulationKey> _previousKeys = new HashSet<EmulationKey>();
    private ExternalCoreApi.KeyboardEvent? _keyboard;
    private AudioBufferStatusCallback? _audioBufferStatus;
    private enum NativePixelFormat { Rgb1555 = 0, Xrgb8888 = 1, Rgb565 = 2 }
    private const int EnglishLanguage = 0;
    private const int FrenchLanguage = 2;
    private readonly IEmulationVideoCallbackHost _video;
    private readonly IEmulationAudioCallbackHost _audio;
    private readonly IEmulationVariableOptionHost<EmulationEnabledOption> _options;
    private readonly ExternalCoreVirtualFileSystem _files;
    private readonly Dictionary<uint, ExternalCoreUtf8String> _directories = [];
    private readonly ExternalCoreApi.LogCallback _log;
    private bool _disposed;
    internal ExternalCoreEnvironmentCallbackHost(IEmulationVideoCallbackHost video, IEmulationAudioCallbackHost audio, IReadOnlyDictionary<string, string> options, string systemDirectory, string contentDirectory, string saveDirectory, string assetsDirectory,
        Func<IReadOnlyDictionary<string, string>, IEmulationVariableOptionHost<EmulationEnabledOption>> createOptions)
    {
        _setRumbleState = _feedback.SetState;
        _video = video;
        _audio = audio;
        Callback = OnEnvironment;
        _log = OnLog;
        var completed = false;
        try
        {
            _options = createOptions(options);
            _files = new ExternalCoreVirtualFileSystem();
            foreach (var directory in new[] { systemDirectory, contentDirectory, saveDirectory, assetsDirectory }) Directory.CreateDirectory(directory);
            _directories.Add(ExternalCoreApiConstants.GetSystemDirectory, new ExternalCoreUtf8String(Path.GetFullPath(systemDirectory)));
            _directories.Add(ExternalCoreApiConstants.GetContentDirectory, new ExternalCoreUtf8String(Path.GetFullPath(contentDirectory)));
            _directories.Add(ExternalCoreApiConstants.GetSaveDirectory, new ExternalCoreUtf8String(Path.GetFullPath(saveDirectory)));
            completed = true;
        }
        finally { if (!completed) Dispose(); }
    }
    public ExternalCoreApi.EnvironmentCallback Callback { get; }
    public ExternalCoreApi.KeyboardEvent? Keyboard => _keyboard;
    public IReadOnlyList<EmulationEnabledOption> Options => _options.Options;
    public List<string> Diagnostics { get; } = [];
    public bool SupportsNoGame { get; private set; }
    public double FramesPerSecond { get; private set; }
    public Exception? Failure { get; private set; }
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback => _feedback.Snapshot();
    public void ResetControllerFeedback(int port) => _feedback.ResetPort(port);
    public void SetOption(string key, string value) => _options.SetValue(key, value);
    public int MinimumAudioLatencyMilliseconds { get; private set; }

    public void SetInput(EmulationInputSnapshot snapshot)
    {
        if (_keyboard is null)
        {
            _previousKeys = new HashSet<EmulationKey>();
            return;
        }
        ExternalCoreKeyboardFunctions.PublishTransitions(snapshot.Keys, KeyboardMap, ExternalCoreKeyboardFunctions.Modifiers(snapshot.Keys),
            ref _previousKeys, ref _keyboard,
            static key => key is EmulationKey.LeftShift or EmulationKey.RightShift or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt,
            ExternalCoreKeyboardFunctions.Character);
    }

    public void PublishAudioBufferStatus(AudioOutputBufferStatus? status)
    {
        if (_audioBufferStatus is not { } callback) return;
        var active = status is { IsActive: true, CapacityFrames: > 0, SampleRate: > 0 };
        var occupancy = active ? (uint)Math.Clamp((long)status!.Value.BufferedFrames * 100 / status.Value.CapacityFrames, 0, 100) : 0;
        var underrun = active && status!.Value.BufferedFrames < Math.Ceiling(status.Value.SampleRate / Math.Max(1, FramesPerSecond));
        callback(active, occupancy, underrun);
    }

    public void ApplyAvInfo(ExternalCoreApi.SystemAvInfo info)
    {
        FramesPerSecond = info.Timing.FramesPerSecond;
        _audio.SampleRate = checked((int)Math.Round(info.Timing.SampleRate));
        _video.AspectRatio = info.Geometry.AspectRatio;
    }

    private bool OnEnvironment(uint command, nint data)
    {
        if (_disposed) return false;
        try
        {
            if (_directories.TryGetValue(command, out var directory)) { if (data == nint.Zero) return false; return ExternalCoreEnvironmentFunctions.WritePointer(data, directory.Pointer); }
            switch (command)
            {
                case ExternalCoreApiConstants.GetRumbleInterface: return ExternalCoreEnvironmentFunctions.SetRumbleInterface(data, _setRumbleState);
                case ExternalCoreApiConstants.SetAudioBufferStatusCallback:
                    var audioCallback = data == nint.Zero ? nint.Zero : Marshal.ReadIntPtr(data);
                    _audioBufferStatus = ExternalCoreEnvironmentFunctions.OptionalDelegate<AudioBufferStatusCallback>(audioCallback);
                    return true;
                case ExternalCoreApiConstants.SetMinimumAudioLatency:
                    MinimumAudioLatencyMilliseconds = data == nint.Zero ? 0 : Math.Clamp(Marshal.ReadInt32(data), 0, 512);
                    return true;
                case ExternalCoreApiConstants.SetVariables: return _options.Register(data);
                case ExternalCoreApiConstants.GetVariable: return _options.ReturnValue(data);
                case ExternalCoreApiConstants.GetVariableUpdate: return _options.ReturnUpdated(data);
                case ExternalCoreApiConstants.SetSupportNoGame: if (data == nint.Zero) return false; SupportsNoGame = Marshal.ReadByte(data) != 0; return true;
                case ExternalCoreApiConstants.GetVfsInterface: return _files.Provide(data);
                case ExternalCoreApiConstants.SetPerformanceLevel: return data != nint.Zero;
                case ExternalCoreApiConstants.SetInputDescriptors: return data != nint.Zero;
                case ExternalCoreApiConstants.SetControllerInfo: return data != nint.Zero;
                case ExternalCoreApiConstants.GetCanDuplicateFrames: if (data == nint.Zero) return false; return ExternalCoreEnvironmentFunctions.WriteBoolean(data, true);
                case ExternalCoreApiConstants.GetLanguage: if (data == nint.Zero) return false; return ExternalCoreEnvironmentFunctions.WriteInteger(data, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr" ? FrenchLanguage : EnglishLanguage);
                case ExternalCoreApiConstants.SetKeyboardCallback: if (data == nint.Zero) return false; var pointer = Marshal.ReadIntPtr(data); _keyboard = ExternalCoreEnvironmentFunctions.OptionalDelegate<ExternalCoreApi.KeyboardEvent>(pointer); return true;
                case ExternalCoreApiConstants.GetLogInterface: return ExternalCoreEnvironmentFunctions.SetLogInterface(data, _log);
                case ExternalCoreApiConstants.SetMessage: if (data == nint.Zero) return false; ExternalCoreEnvironmentFunctions.CopyDiagnostic(Marshal.ReadIntPtr(data), Diagnostics.Add, allowWhitespace: true); return true;
                case ExternalCoreApiConstants.SetPixelFormat:
                    if (data == nint.Zero) return false;
                    var format = (NativePixelFormat)Marshal.ReadInt32(data);
                    if (!Enum.IsDefined(format)) return false;
                    _video.PixelFormat = format switch { NativePixelFormat.Rgb1555 => EmulationPixelFormat.Rgb1555, NativePixelFormat.Xrgb8888 => EmulationPixelFormat.Xrgb8888, NativePixelFormat.Rgb565 => EmulationPixelFormat.Rgb565, _ => throw new InvalidOperationException() };
                    return true;
                case ExternalCoreApiConstants.SetSystemAvInfo: if (data == nint.Zero) return false; ApplyAvInfo(Marshal.PtrToStructure<ExternalCoreApi.SystemAvInfo>(data)); return true;
                case ExternalCoreApiConstants.SetGeometry: if (data == nint.Zero) return false; _video.AspectRatio = Marshal.PtrToStructure<ExternalCoreApi.Geometry>(data).AspectRatio; return true;
                default: return false;
            }
        }
        catch (Exception error) { Failure ??= error; return false; }
    }

    private void OnLog(int level, nint text)
    {
        try { ExternalCoreEnvironmentFunctions.CopyDiagnostic(text, Diagnostics.Add, allowWhitespace: true); }
        catch (Exception error) { Failure ??= error; }
    }

    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            _disposed = true;
            _audioBufferStatus = null;
            MinimumAudioLatencyMilliseconds = 0;
            _previousKeys = new HashSet<EmulationKey>();
            try { _files?.Dispose(); }
            finally
            {
                try { _options?.Dispose(); }
                finally { foreach (var directory in _directories.Values) directory.Dispose(); _directories.Clear(); _keyboard = null; Diagnostics.Clear(); Failure = null; }
            }
        }
        finally
        {
            _feedback.Dispose();
        }
    }
}
