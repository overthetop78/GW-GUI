namespace GWGUI.MediaFileSystems.FileSystems.Fat12;

public readonly record struct Fat12TargetGeometry(
    string FormatId,
    int SectorSize,
    int Cylinders,
    int Heads,
    int SectorsPerTrack,
    bool IsMsx)
{
    public int TotalSectors => checked(Cylinders * Heads * SectorsPerTrack);

    public int Capacity => checked(TotalSectors * SectorSize);
}
