namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants;

internal static class ClockConstants
{
    internal const double HertzPerMegahertz = 1_000_000;
    internal const double ComparisonToleranceMhz = 0.0001;
    internal const double NominalSpeedRatio = 1;
    internal const double HalfSpeedRatio = 0.5;
    internal const double DoubleSpeedRatio = 2;
    internal const double QuadrupleSpeedRatio = 4;
    internal const double EightfoldSpeedRatio = 8;
    internal static IReadOnlyList<CpuClockMultiplier> CycleExactMultipliers { get; } = Enum.GetValues<CpuClockMultiplier>();
}
