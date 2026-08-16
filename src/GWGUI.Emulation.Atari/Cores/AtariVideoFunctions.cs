using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariVideoFunctions
{
    internal static int FrameLength(uint height, nuint pitch) => checked((int)(pitch * height));

    internal static void CopyRows(nint source, byte[] destination, int height, int pitch)
    {
        for (var row = AtariVideoConstants.FirstRow; row < height; row++)
            Marshal.Copy(nint.Add(source, checked(row * pitch)), destination,
                checked(row * pitch), pitch);
    }

    internal static TimeSpan Timestamp(long startTimestamp) => Stopwatch.GetElapsedTime(startTimestamp);
}
