namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Registres et codes du contrôleur Intel 8271, indépendants de son adresse sur une carte.</summary>
internal static class Intel8271Constants
{
    internal const byte Busy = 0x80, ResultFull = 0x10, Interrupt = 0x08, NonDma = 0x04;
    internal const byte LateDma = 0x0a, DataCrc = 0x0e, NotReady = 0x10, WriteProtected = 0x12, NotFound = 0x18;
    internal const byte ReadSingle = 0x12, ReadMultiple = 0x13, ReadIncludingDeletedSingle = 0x16, ReadIncludingDeletedMultiple = 0x17;
    internal const byte WriteSingle = 0x0a, WriteMultiple = 0x0b, ReadId = 0x1b, VerifySingle = 0x1e, VerifyMultiple = 0x1f;
    internal const byte Format = 0x23, Seek = 0x29, DriveStatus = 0x2c, Specify = 0x35, WriteSpecial = 0x3a, ReadSpecial = 0x3d;
    internal const byte Track0 = 0x12, Track1 = 0x1a, Mode = 0x17, DriveOutput = 0x23;
    internal const int Drive0Select = 0x40, Drive1Select = 0x80, SideSelect = 0x20;
    internal const int ParameterCapacity = 5, FormatIdSize = 4, FmBytesPerSecond = 15625;
    internal const byte FormatFill = 0xe5;
}
