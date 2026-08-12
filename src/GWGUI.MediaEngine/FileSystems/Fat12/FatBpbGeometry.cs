namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Regroupe la géométrie validée lue dans un BPB FAT.</summary>
public readonly record struct FatBpbGeometry
{
    /// <summary>Crée une géométrie BPB validée.</summary>
    public FatBpbGeometry(int sectorSize, int totalSectors, int cylinders, int heads, int sectorsPerTrack)
    {
        SectorSize = sectorSize;
        TotalSectors = totalSectors;
        Cylinders = cylinders;
        Heads = heads;
        SectorsPerTrack = sectorsPerTrack;
    }

    /// <summary>Taille d'un secteur en octets.</summary>
    public int SectorSize { get; }
    /// <summary>Nombre total de secteurs.</summary>
    public int TotalSectors { get; }
    /// <summary>Nombre de cylindres.</summary>
    public int Cylinders { get; }
    /// <summary>Nombre de têtes.</summary>
    public int Heads { get; }
    /// <summary>Nombre de secteurs par piste.</summary>
    public int SectorsPerTrack { get; }
}
