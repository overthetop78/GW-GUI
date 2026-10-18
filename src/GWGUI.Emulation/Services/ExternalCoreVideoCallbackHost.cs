using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalCoreVideoCallbackHost : IDisposable
{
    private readonly long _started = Stopwatch.GetTimestamp();
    private long _sequence;
    private bool _disposed;
    internal ExternalCoreVideoCallbackHost() => Callback = OnVideo;
    internal ExternalCoreApi.VideoCallback Callback { get; }
    internal VideoFrame? LatestFrame { get; private set; }
    internal EmulationPixelFormat PixelFormat { get; set; } = EmulationPixelFormat.Xrgb8888;
    internal float AspectRatio { get; set; }
    internal Exception? Failure { get; private set; }

    private void OnVideo(nint data, uint width, uint height, nuint pitch)
    {
        if (_disposed) return;
        try
        {
            var timestamp = Stopwatch.GetElapsedTime(_started);
            if (data == nint.Zero)
            {
                if (LatestFrame is { } previous) LatestFrame = previous with { Sequence = ++_sequence, Timestamp = timestamp };
                return;
            }
            if (width == 0 || height == 0 || pitch == 0) return;
            var bytesPerPixel = PixelFormat == EmulationPixelFormat.Xrgb8888 ? sizeof(uint) : sizeof(ushort);
            if (pitch < checked((nuint)width * (nuint)bytesPerPixel)) throw new InvalidOperationException("Native video pitch is shorter than its image row.");
            var length = checked((int)(pitch * height));
            if (length > EmulationHostProtocolConstants.VideoSlotCapacity) throw new InvalidOperationException("Native video frame exceeds the host transport capacity.");
            var pixels = GC.AllocateUninitializedArray<byte>(length);
            Marshal.Copy(data, pixels, 0, length);
            LatestFrame = new VideoFrame(pixels, checked((int)width), checked((int)height), checked((int)pitch), PixelFormat, AspectRatio, ++_sequence, timestamp);
        }
        catch (Exception error) { Failure ??= error; }
    }

    public void Dispose()
    {
        _disposed = true;
        LatestFrame = null;
        Failure = null;
    }
}
