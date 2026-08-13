namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Définit les secteurs système des volumes 1581.</summary>
public static class Commodore1581DosLayout
{
    /// <summary>Piste de l'en-tête et des BAM.</summary>
    public const int HeaderTrack = 40;
    /// <summary>Secteur d'en-tête.</summary>
    public const int HeaderSector = 0;
    /// <summary>Premier secteur de répertoire utilisé en repli.</summary>
    public const int DirectorySector = 3;
    /// <summary>Offset du nom du volume.</summary>
    public const int VolumeNameOffset = 4;
    /// <summary>Marqueur d'en-tête DOS.</summary>
    public const byte HeaderSignature = 0x44;
    /// <summary>Premier secteur BAM.</summary>
    public const int FirstBamSector = 1;
    /// <summary>Second secteur BAM.</summary>
    public const int SecondBamSector = 2;
    /// <summary>Offset de la première entrée BAM.</summary>
    public const int BamEntriesOffset = 16;
    /// <summary>Taille d'une entrée BAM.</summary>
    public const int BamEntrySize = 6;
    /// <summary>Nombre d'entrées BAM par secteur.</summary>
    public const int BamEntryCount = 40;
    /// <summary>Offset de l'identifiant de disque dans l'en-tête.</summary>
    public const int DiskIdOffset = 22;
    /// <summary>Offset du type DOS dans l'en-tête.</summary>
    public const int DosTypeOffset = 25;
    /// <summary>Type DOS standard 3D.</summary>
    public static ReadOnlySpan<byte> DosType => "3D"u8;
    /// <summary>Offset de l'identifiant de disque dans un secteur BAM.</summary>
    public const int BamDiskIdOffset = 4;
}
