namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Interface hôte Winchester Acorn et codes de résultat SCSI/SASI.</summary>
internal static class AcornScsiConstants
{
    internal const int UnitCount = 4, SectorSize = 256, SectorsPerTrack = 33, DescriptorLength = 33, CpuClock = 2_000_000;
    internal const ushort Start = 0xfc40, End = 0xfc44;
    internal const byte CheckCondition = 2, NotReady = 4, ReadError = 0x11, WriteError = 0x0c, InvalidCommand = 0x20, InvalidAddress = 0x21, InvalidParameter = 0x24, WriteProtected = 0x27;
}
