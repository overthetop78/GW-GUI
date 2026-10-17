using System.Runtime.InteropServices;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class CoreVideoFunctions
{
    internal static VideoFrame? Read(nint data, uint width, uint height, nuint pitch,
        HardwareRenderSession hardwareRender, EmulationPixelFormat pixelFormat, float aspectRatio,
        ref long sequence, TimeSpan elapsed)
    {
        if (data == nint.Zero || width == ExternalCoreInteropConstants.EmptyFrameDimension
            || height == ExternalCoreInteropConstants.EmptyFrameDimension) return null;
        if (data == HardwareRenderConstants.HardwareFramebuffer)
        {
            if (!hardwareRender.TryReadFramebuffer(checked((int)width), checked((int)height), out var frame)) return null;
            return new VideoFrame(frame, checked((int)width), checked((int)height),
                checked((int)width * HardwareRenderConstants.PixelByteCount), EmulationPixelFormat.Xrgb8888,
                aspectRatio > 0 ? aspectRatio : width / (float)height, ++sequence, elapsed);
        }
        var byteCount = checked((int)(pitch * height));
        var pixels = new byte[byteCount];
        Marshal.Copy(data, pixels, BufferConstants.FirstBufferIndex, byteCount);
        return new VideoFrame(pixels, checked((int)width), checked((int)height), checked((int)pitch),
            pixelFormat, aspectRatio > 0 ? aspectRatio : width / (float)height, ++sequence, elapsed);
    }
}
