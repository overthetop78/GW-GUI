namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Enums;

public enum CpuModel
{
    ZilogZ80 = 80,
    Mos6502 = 6502,
    Mos6509 = 6509,
    Mos6510 = 6510,
    Mos7501 = 7501,
    Mos8502 = 8502,
    Wdc65816 = 65816,
    Motorola6809 = 6809,
    Motorola68000 = 68000,
    Motorola68010 = 68010,
    Motorola68020 = 68020,
    Motorola68030 = 68030,
    Motorola68040 = 68040,
    Motorola68060 = 68060
}

public enum FpuModel
{
    None,
    Integrated,
    Motorola68881,
    Motorola68882
}
