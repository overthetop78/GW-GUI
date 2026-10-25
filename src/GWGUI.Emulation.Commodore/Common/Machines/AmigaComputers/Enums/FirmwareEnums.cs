namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Enums;

public enum KickstartVersion
{
    _1_0, _1_1, _1_2, _1_3, _2_0, _2_02, _2_04, _2_05, _3_0, _3_1
}

internal enum NativeKickstartVersion
{
    Kickstart11Ntsc = 31, Kickstart11Pal = 32, Kickstart12 = 33, Kickstart13 = 34,
    Kickstart20 = 36, Kickstart204 = 37, Kickstart30 = 39, Kickstart31 = 40
}
internal enum NativeKickstartRevision
{
    Kickstart11 = 34, Kickstart12 = 180, Kickstart13 = 5, Kickstart204 = 175,
    Kickstart205A600 = 350, Kickstart30 = 106, Kickstart31Cd32 = 60,
    Kickstart31A600 = 63, Kickstart31A1200 = 68, Kickstart31A4000 = 70
}
