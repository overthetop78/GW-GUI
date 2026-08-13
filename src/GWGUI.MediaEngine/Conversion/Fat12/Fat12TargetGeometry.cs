namespace GWGUI.MediaEngine.Conversion.Fat12;

public readonly record struct Fat12TargetGeometry(
    string FormatId,
    int SectorSize,
    int Cylinders,
    int Heads,
    int SectorsPerTrack)
{
    public int TotalSectors => checked(Cylinders * Heads * SectorsPerTrack);

    public int Capacity => checked(TotalSectors * SectorSize);
}
