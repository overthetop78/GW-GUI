using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Watara.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Interop.Services;

internal sealed class EnvironmentCallbackHost : IDisposable
{
    private const uint SetAudioBufferStatusCallback = 62;
    private const uint SetMinimumAudioLatency = 63;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void AudioBufferStatusCallback([MarshalAs(UnmanagedType.I1)] bool active, uint occupancy, [MarshalAs(UnmanagedType.I1)] bool underrunLikely);
    private AudioBufferStatusCallback? _audioBufferStatus;
    internal int MinimumAudioLatencyMilliseconds { get; private set; }
    internal void PublishAudioBufferStatus(AudioOutputBufferStatus? status)
    {
        if (_audioBufferStatus is not { } callback) return;
        var active = status is { IsActive: true, CapacityFrames: > 0, SampleRate: > 0 };
        var occupancy = active ? (uint)Math.Clamp((long)status!.Value.BufferedFrames * 100 / status.Value.CapacityFrames, 0, 100) : 0;
        var underrun = active && status!.Value.BufferedFrames < Math.Ceiling(status.Value.SampleRate / Math.Max(1, FramesPerSecond));
        callback(active, occupancy, underrun);
    }

    private enum NativePixelFormat { Rgb1555 = 0, Xrgb8888 = 1, Rgb565 = 2 }
    private const int EnglishLanguage = 0;
    private const int FrenchLanguage = 2;
    private readonly VideoCallbackHost _video;
    private readonly AudioCallbackHost _audio;
    private readonly OptionCallbackHost _options;
    private readonly IEmulationVirtualFileSystem _files;
    private readonly Dictionary<uint, IEmulationNativeString> _directories = [];
    private readonly ExternalCoreApi.LogCallback _log;
    private bool _disposed;
    internal EnvironmentCallbackHost(VideoCallbackHost video, AudioCallbackHost audio, IReadOnlyDictionary<string, string> options, string systemDirectory, string contentDirectory, string saveDirectory, string assetsDirectory)
    {
        _video = video;
        _audio = audio;
        _options = new OptionCallbackHost(options);
        _files = CommonCoreHostFunctions.NativeResources.CreateVirtualFileSystem();
        Callback = OnEnvironment;
        _log = OnLog;
        var completed = false;
        try
        {
            foreach (var directory in new[] { systemDirectory, contentDirectory, saveDirectory, assetsDirectory }) Directory.CreateDirectory(directory);
            _directories.Add(ExternalCoreApiConstants.GetSystemDirectory, CommonCoreHostFunctions.NativeResources.CreateString(Path.GetFullPath(systemDirectory)));
            _directories.Add(ExternalCoreApiConstants.GetContentDirectory, CommonCoreHostFunctions.NativeResources.CreateString(Path.GetFullPath(contentDirectory)));
            _directories.Add(ExternalCoreApiConstants.GetSaveDirectory, CommonCoreHostFunctions.NativeResources.CreateString(Path.GetFullPath(saveDirectory)));
            completed = true;
        }
        finally { if (!completed) Dispose(); }
    }
    internal ExternalCoreApi.EnvironmentCallback Callback { get; }
    internal IReadOnlyList<CoreOption> Options => _options.Options;
    internal List<string> Diagnostics { get; } = [];
    internal bool SupportsNoGame { get; private set; }
    internal double FramesPerSecond { get; private set; }
    internal Exception? Failure { get; private set; }
    internal void SetOption(string key, string value) => _options.SetValue(key, value);

    internal void ApplyAvInfo(ExternalCoreApi.SystemAvInfo info)
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
                case SetAudioBufferStatusCallback:
                    var callback = data == nint.Zero ? nint.Zero : Marshal.ReadIntPtr(data);
                    _audioBufferStatus = GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.OptionalDelegate<AudioBufferStatusCallback>(callback);
                    return true;
                case SetMinimumAudioLatency:
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
        _disposed = true;
        _audioBufferStatus = null;
        MinimumAudioLatencyMilliseconds = 0;
        try { _files.Dispose(); }
        finally
        {
            try { _options.Dispose(); }
            finally { foreach (var directory in _directories.Values) directory.Dispose(); _directories.Clear(); Diagnostics.Clear(); Failure = null; }
        }
    }
}
