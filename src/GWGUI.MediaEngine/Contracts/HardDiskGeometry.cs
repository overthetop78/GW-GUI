namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes an explicitly known CHS geometry without deriving it from image length.</summary>
public sealed class HardDiskGeometry
{
    public HardDiskGeometry(long cylinders, int heads, int sectorsPerTrack, int sectorSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cylinders);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(heads);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sectorsPerTrack);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sectorSize);

        Cylinders = cylinders;
        Heads = heads;
        SectorsPerTrack = sectorsPerTrack;
        SectorSize = sectorSize;
        LogicalSectorCount = checked(checked(cylinders * heads) * sectorsPerTrack);
        Capacity = checked(LogicalSectorCount * sectorSize);
    }

    public long Cylinders { get; }

    public int Heads { get; }

    public int SectorsPerTrack { get; }

    public int SectorSize { get; }

    public long LogicalSectorCount { get; }

    public long Capacity { get; }
}
