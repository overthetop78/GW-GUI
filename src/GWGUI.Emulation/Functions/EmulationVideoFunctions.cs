using System.Globalization;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Functions;

public static class EmulationVideoFunctions
{
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
