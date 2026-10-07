using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Functions;

internal static class NativeAudioFunctions
{
    internal static short[] CopyBatch(nint data, int frameCount)
    {
        var samples = GC.AllocateUninitializedArray<short>(
            checked(frameCount * AudioConstants.StereoChannelCount));
        Marshal.Copy(data, samples, BufferConstants.FirstBufferIndex, samples.Length);
        return samples;
    }

}
