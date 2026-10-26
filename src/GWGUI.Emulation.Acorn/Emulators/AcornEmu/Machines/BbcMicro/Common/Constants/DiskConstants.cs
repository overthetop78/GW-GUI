namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants;

/// <summary>Décodage BBC des ROM latérales et des cartes disquette 8271 et Acorn WD1770.</summary>
internal static class DiskConstants
{
    internal const int RomBankSize = 16384, RomBankCount = 16, BasicBank = 15, DfsBank = 14, NetworkBank = 13;
    internal const ushort RomSelectStart = 0xfe30, RomSelectEnd = 0xfe40, ControllerStart = 0xfe80, ControllerEnd = 0xfea0;
    internal const int DriveCount = 2;
}
