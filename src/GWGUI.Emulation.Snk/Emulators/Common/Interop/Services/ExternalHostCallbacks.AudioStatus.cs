using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NativeAudioBufferStatusCallback([MarshalAs(UnmanagedType.I1)] bool active, uint occupancy, [MarshalAs(UnmanagedType.I1)] bool underrunLikely);

    private NativeAudioBufferStatusCallback? _audioBufferStatusCallback;
    internal AudioOutputBufferStatus? OutputAudioStatus { get; set; }
    internal int MinimumAudioLatencyMilliseconds { get; private set; }

    private bool CaptureAudioBufferStatus(nint data)
    {
        var pointer = data == nint.Zero ? nint.Zero : Marshal.ReadIntPtr(data);
        _audioBufferStatusCallback = pointer == nint.Zero ? null : Marshal.GetDelegateForFunctionPointer<NativeAudioBufferStatusCallback>(pointer);
        return true;
    }

    private bool SetMinimumAudioLatency(nint data)
    {
        if (data == nint.Zero) return false;
        var latency = Marshal.ReadInt32(data);
        if (latency < EnvironmentConstants.MinimumAudioBufferValue || latency > EnvironmentConstants.MaximumAudioLatencyMilliseconds) return false;
        MinimumAudioLatencyMilliseconds = latency;
        return true;
    }

    internal void NotifyAudioBufferStatus()
    {
        if (_audioBufferStatusCallback is not { } callback) return;
        if (OutputAudioStatus is not { IsActive: true, CapacityFrames: > 0, SampleRate: > 0 } status)
        {
            callback(false, EnvironmentConstants.MinimumAudioBufferValue, false);
            return;
        }
        var buffered = Math.Clamp(status.BufferedFrames, EnvironmentConstants.MinimumAudioBufferValue, status.CapacityFrames);
        var occupancy = checked((uint)((long)buffered * EnvironmentConstants.MaximumAudioOccupancyPercent / status.CapacityFrames));
        var underrunLikely = FramesPerSecond > EnvironmentConstants.MinimumAudioBufferValue && buffered < Math.Ceiling(status.SampleRate / FramesPerSecond);
        callback(true, occupancy, underrunLikely);
    }
}
