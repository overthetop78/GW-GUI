namespace GWGUI.MediaEngine.Containers.TeleDisk;

[Flags]
internal enum Td0SectorFlags : byte
{
    None = 0,
    DataCrcError = 0x02,
    DataUnavailable = 0x10,
    DataSkipped = 0x20,
    UnavailableMask = DataUnavailable | DataSkipped
}
