using System.Globalization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Functions;

internal static class EmulationVideoFunctions
{
    internal static void ApplyGeometry(ExternalCoreApi.Geometry geometry, ref float aspectRatio)
    {
        if (float.IsFinite(geometry.AspectRatio) && geometry.AspectRatio > 0)
            aspectRatio = geometry.AspectRatio;
        else if (geometry.BaseHeight > 0)
            aspectRatio = geometry.BaseWidth / (float)geometry.BaseHeight;
    }

    internal static int FrameLength(uint height, nuint pitch) => checked((int)(pitch * height));

    internal static void CopyRows(nint source, byte[] destination, int height, int pitch)
    {
        for (var row = 0; row < height; row++)
            Marshal.Copy(nint.Add(source, checked(row * pitch)), destination, checked(row * pitch), pitch);
    }

    internal static byte[] CopyFramePixels(nint data, uint height, nuint pitch)
    {
        var byteCount = FrameLength(height, pitch);
        var pixels = new byte[byteCount];
        Marshal.Copy(data, pixels, 0, byteCount);
        return pixels;
    }

    internal static TimeSpan Timestamp(long startTimestamp) => Stopwatch.GetElapsedTime(startTimestamp);

    public static string FormatStandard(EmulationVideoStandard standard) => standard switch
    {
        EmulationVideoStandard.Ntsc => "NTSC",
        EmulationVideoStandard.Pal => "PAL",
        EmulationVideoStandard.Secam => "SECAM",
        _ => throw new ArgumentOutOfRangeException(nameof(standard))
    };

    public static string FormatFrequency(double frequencyHz)
    {
        if (!double.IsFinite(frequencyHz) || frequencyHz <= 0) throw new ArgumentOutOfRangeException(nameof(frequencyHz));
        return $"{frequencyHz.ToString("0.######", CultureInfo.InvariantCulture)} Hz";
    }

    public static string FormatTiming(EmulationVideoStandard standard, double frequencyHz) => $"{FormatStandard(standard)} ({FormatFrequency(frequencyHz)})";
}
