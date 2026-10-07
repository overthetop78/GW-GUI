namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Enums;

public enum ClockFrequency
{
    PalHalfBase = 3_546_895,
    NtscHalfBase = 3_579_545,
    PalBase = 7_093_790,
    NtscBase = 7_159_090,
    Pal68EC020 = 14_187_580,
    Ntsc68EC020 = 14_318_180,
    Workstation = 25_000_000
}

public enum CpuClockMultiplier
{
    HalfBaseClock = 1, BaseClock = 2, DoubleBaseClock = 4, QuadrupleBaseClock = 8, EightfoldBaseClock = 16
}
