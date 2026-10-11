namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants;

internal static class ClockConstants
{
    internal const int PalHalfBaseFrequencyHz = 3546895;
    internal const int NtscHalfBaseFrequencyHz = 3579545;
    internal const int PalBaseFrequencyHz = 7093790;
    internal const int NtscBaseFrequencyHz = 7159090;
    internal const int Pal68EC020FrequencyHz = 14187580;
    internal const int Ntsc68EC020FrequencyHz = 14318180;
    internal const int WorkstationFrequencyHz = 25000000;
    internal const double HertzPerMegahertz = 1_000_000;
    internal const double ComparisonToleranceMhz = 0.0001;
    internal const double NominalSpeedRatio = 1;
    internal const double HalfSpeedRatio = 0.5;
    internal const double DoubleSpeedRatio = 2;
    internal const double QuadrupleSpeedRatio = 4;
    internal const double EightfoldSpeedRatio = 8;
    internal static IReadOnlyList<CpuClockMultiplier> CycleExactMultipliers { get; } = Enum.GetValues<CpuClockMultiplier>();
}
