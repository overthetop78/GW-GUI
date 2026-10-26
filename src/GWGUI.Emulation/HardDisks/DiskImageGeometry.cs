namespace GWGUI.Emulation.HardDisks;

/// <summary>Géométrie explicitement choisie pour une image, avec sa taille de secteur indépendante du BIOS.</summary>
public sealed record DiskImageGeometry
{
    public DiskImageGeometry(long cylinders, int heads, int sectorsPerTrack, int sectorSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cylinders); ArgumentOutOfRangeException.ThrowIfNegativeOrZero(heads);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sectorsPerTrack); ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sectorSize);
        Cylinders = cylinders; Heads = heads; SectorsPerTrack = sectorsPerTrack; SectorSize = sectorSize;
        Capacity = checked(checked(checked(cylinders * heads) * sectorsPerTrack) * sectorSize);
    }
    public long Cylinders { get; }
    public int Heads { get; }
    public int SectorsPerTrack { get; }
    public int SectorSize { get; }
    public long Capacity { get; }
}
