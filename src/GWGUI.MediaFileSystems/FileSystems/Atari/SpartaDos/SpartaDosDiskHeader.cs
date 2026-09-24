namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

internal sealed record SpartaDosDiskHeader(
    int RootDirectoryMapSector,
    int TotalSectors,
    int FreeSectors,
    int BitmapSectorCount,
    int FirstBitmapSector,
    int FirstFileAllocationSector,
    int FirstDirectoryAllocationSector,
    string VolumeName,
    int TrackCount,
    bool DoubleSided,
    int SectorSize,
    byte Version,
    byte VolumeSequence,
    byte VolumeRandom,
    int BootFileMapSector,
    bool WriteProtected);
