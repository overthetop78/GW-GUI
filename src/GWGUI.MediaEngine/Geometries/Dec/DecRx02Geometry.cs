namespace GWGUI.MediaEngine.Geometries.Dec;

/// <summary>Définit la géométrie physique et logique d'une disquette DEC RX02.</summary>
public static class DecRx02Geometry
{
    /// <summary>Nombre de pistes physiques.</summary>
    public const int TrackCount = 77;
    /// <summary>Nombre de faces.</summary>
    public const int HeadCount = 1;
    /// <summary>Nombre de secteurs physiques par piste.</summary>
    public const int PhysicalSectorsPerTrack = 26;
    /// <summary>Taille d'un secteur physique en octets.</summary>
    public const int PhysicalSectorSize = 256;
    /// <summary>Taille d'un bloc logique RT-11 en octets.</summary>
    public const int LogicalBlockSize = 512;
    /// <summary>Nombre de secteurs physiques réunis dans un bloc logique.</summary>
    public const int PhysicalSectorsPerLogicalBlock = LogicalBlockSize / PhysicalSectorSize;
    /// <summary>Nombre total de secteurs physiques.</summary>
    public const int PhysicalSectorCount = TrackCount * PhysicalSectorsPerTrack;
    /// <summary>Nombre total de blocs logiques.</summary>
    public const int LogicalBlockCount = PhysicalSectorCount / PhysicalSectorsPerLogicalBlock;
    /// <summary>Nombre de blocs logiques par piste.</summary>
    public const int LogicalBlocksPerTrack = PhysicalSectorsPerTrack / PhysicalSectorsPerLogicalBlock;
    /// <summary>Capacité totale d'un dump RX02 complet en octets.</summary>
    public const int Capacity = PhysicalSectorCount * PhysicalSectorSize;
}
