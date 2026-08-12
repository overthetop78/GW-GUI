namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Décrit la disposition calculée d'un volume FAT12.</summary>
public sealed class Fat12Layout
{
    /// <summary>Nombre maximal de clusters d'un volume FAT12.</summary>
    public const int MaximumClusterCount = 4085;

    /// <summary>Crée et valide une disposition FAT12.</summary>
    public Fat12Layout(int reservedSectors, int sectorsPerFat, int rootStart, int rootSectors, int dataStart, int sectorsPerCluster, int clusterCount)
    {
        if (reservedSectors <= 0) throw new ArgumentOutOfRangeException(nameof(reservedSectors));
        if (sectorsPerFat <= 0) throw new ArgumentOutOfRangeException(nameof(sectorsPerFat));
        if (rootStart < reservedSectors || rootSectors <= 0 || dataStart < rootStart + rootSectors) throw new ArgumentOutOfRangeException(nameof(rootStart));
        if (sectorsPerCluster <= 0) throw new ArgumentOutOfRangeException(nameof(sectorsPerCluster));
        if (clusterCount is <= 0 or >= MaximumClusterCount) throw new ArgumentOutOfRangeException(nameof(clusterCount));
        ReservedSectors = reservedSectors;
        SectorsPerFat = sectorsPerFat;
        RootStart = rootStart;
        RootSectors = rootSectors;
        DataStart = dataStart;
        SectorsPerCluster = sectorsPerCluster;
        ClusterCount = clusterCount;
    }

    /// <summary>Nombre de secteurs réservés.</summary>
    public int ReservedSectors { get; }
    /// <summary>Nombre de secteurs occupés par une FAT.</summary>
    public int SectorsPerFat { get; }
    /// <summary>Premier secteur du répertoire racine.</summary>
    public int RootStart { get; }
    /// <summary>Nombre de secteurs du répertoire racine.</summary>
    public int RootSectors { get; }
    /// <summary>Premier secteur de la zone de données.</summary>
    public int DataStart { get; }
    /// <summary>Nombre de secteurs par cluster.</summary>
    public int SectorsPerCluster { get; }
    /// <summary>Nombre de clusters de données.</summary>
    public int ClusterCount { get; }
}
