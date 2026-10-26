namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Registres, états et cadences du contrôleur double densité WD1770.</summary>
internal static class Wd1770Constants
{
    internal const byte Busy = 1, DataRequest = 2, LostData = 4, CrcError = 8, NotFound = 16, WriteProtected = 64, Motor = 128;
    internal const int CpuClock = 2_000_000, DoubleDensityBytesPerSecond = 31_250, SingleDensityBytesPerSecond = 15_625;
    internal const int RevolutionsPerSecond = 5, SectorSizeBase = 128, MaximumSectorSizeCode = 7;
    internal static IReadOnlyList<int> StepMilliseconds { get; } = Array.AsReadOnly<int>([6, 12, 20, 30]);
}
