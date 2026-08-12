namespace GWGUI.MediaEngine.FileSystems.Fat;

/// <summary>Regroupe la géométrie validée lue dans un BPB FAT.</summary>
public readonly record struct FatBpbGeometry(int SectorSize, int TotalSectors, int Cylinders, int Heads, int SectorsPerTrack);
