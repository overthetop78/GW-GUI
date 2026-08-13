using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Décrit l'organisation des secteurs système d'un volume Commodore DOS.</summary>
public sealed class CommodoreDosLayout
{
    /// <summary>Disposition utilisée par les images D64 et D71.</summary>
    public static CommodoreDosLayout D64D71 { get; } = new(Commodore1541DosLayout.HeaderTrack, Commodore1541DosLayout.HeaderSector, Commodore1541DosLayout.HeaderTrack, Commodore1541DosLayout.DirectorySector, Commodore1541DosLayout.VolumeNameOffset, [Commodore1541DosLayout.HeaderSector], Commodore1541DosLayout.BamEntriesOffset, Commodore1541DosLayout.BamEntrySize, Geometries.Commodore.Commodore1541Geometry.StandardTrackCount, Commodore1541DosLayout.HeaderSignature);

    /// <summary>Disposition utilisée par les images D81.</summary>
    public static CommodoreDosLayout D81 { get; } = new(Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.HeaderSector, Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.DirectorySector, Commodore1581DosLayout.VolumeNameOffset, [Commodore1581DosLayout.FirstBamSector, Commodore1581DosLayout.SecondBamSector], Commodore1581DosLayout.BamEntriesOffset, Commodore1581DosLayout.BamEntrySize, Commodore1581DosLayout.BamEntryCount, Commodore1581DosLayout.HeaderSignature);

    /// <summary>Taille d'un secteur logique Commodore DOS.</summary>
    public const int SectorSize = 256;

    /// <summary>Offset de la piste suivante dans une chaîne de secteurs.</summary>
    public const int NextTrackOffset = 0;

    /// <summary>Offset du secteur suivant dans une chaîne de secteurs.</summary>
    public const int NextSectorOffset = 1;

    /// <summary>Offset de la première entrée dans un secteur de répertoire.</summary>
    public const int DirectoryEntriesOffset = 2;
    /// <summary>Nombre d'octets du lien placé au début d'un secteur chaîné.</summary>
    public const int LinkLength = DirectoryEntriesOffset;

    /// <summary>Nombre d'entrées contenues dans un secteur de répertoire.</summary>
    public const int DirectoryEntryCount = 8;

    /// <summary>Taille d'une entrée de répertoire.</summary>
    public const int DirectoryEntrySize = 32;

    /// <summary>Offset du type de fichier dans une entrée.</summary>
    public const int FileTypeOffset = 2;

    /// <summary>Offset de la première piste de données dans une entrée.</summary>
    public const int FirstDataTrackOffset = 3;

    /// <summary>Offset du premier secteur de données dans une entrée.</summary>
    public const int FirstDataSectorOffset = 4;

    /// <summary>Offset du nom dans une entrée.</summary>
    public const int FileNameOffset = 5;

    /// <summary>Longueur du nom d'un fichier ou d'un volume.</summary>
    public const int NameLength = 16;

    /// <summary>Offset du nombre de blocs déclaré dans une entrée.</summary>
    public const int DeclaredBlockCountOffset = 30;

    /// <summary>Offset de la piste du premier secteur latéral REL.</summary>
    public const int RelativeSideTrackOffset = 21;

    /// <summary>Offset du secteur du premier secteur latéral REL.</summary>
    public const int RelativeSideSectorOffset = 22;

    /// <summary>Offset de la longueur d'enregistrement REL.</summary>
    public const int RelativeRecordLengthOffset = 23;

    /// <summary>Décalage stockant la longueur d'enregistrement REL dans les attributs communs.</summary>
    public const int RelativeRecordLengthAttributeShift = 8;

    /// <summary>Nombre de pointeurs de données dans un secteur latéral REL.</summary>
    public const int RelativeDataPointersPerSideSector = 120;

    /// <summary>Nombre maximal de secteurs latéraux du format REL classique.</summary>
    public const int MaximumRelativeSideSectors = 6;

    /// <summary>Offset du numéro d'un secteur latéral REL.</summary>
    public const int RelativeSideNumberOffset = 2;

    /// <summary>Offset de la longueur d'enregistrement dans un secteur latéral REL.</summary>
    public const int RelativeSideRecordLengthOffset = 3;

    /// <summary>Offset de la table des secteurs latéraux REL.</summary>
    public const int RelativeSideTableOffset = 4;

    /// <summary>Offset des pointeurs de données REL.</summary>
    public const int RelativeDataPointersOffset = 16;

    /// <summary>Nombre maximal d'octets de données dans un secteur chaîné.</summary>
    public const int DataBytesPerSector = SectorSize - DirectoryEntriesOffset;

    /// <summary>Valeur maximale de piste visitée lors du contrôle d'un répertoire.</summary>
    public const int MaximumDirectoryChainLength = 64;

    private CommodoreDosLayout(int headerTrack, int headerSector, int directoryTrack, int directorySector, int volumeNameOffset, IReadOnlyList<int> bamSectors, int bamEntriesOffset, int bamEntrySize, int bamEntryCount, byte headerSignature)
    {
        HeaderTrack = headerTrack;
        HeaderSector = headerSector;
        DirectoryTrack = directoryTrack;
        DirectorySector = directorySector;
        VolumeNameOffset = volumeNameOffset;
        BamSectors = Array.AsReadOnly(bamSectors.ToArray());
        BamEntriesOffset = bamEntriesOffset;
        BamEntrySize = bamEntrySize;
        BamEntryCount = bamEntryCount;
        HeaderSignature = headerSignature;
    }

    /// <summary>Piste du secteur d'en-tête.</summary>
    public int HeaderTrack { get; }

    /// <summary>Numéro du secteur d'en-tête.</summary>
    public int HeaderSector { get; }

    /// <summary>Piste du premier secteur de répertoire par défaut.</summary>
    public int DirectoryTrack { get; }

    /// <summary>Numéro du premier secteur de répertoire par défaut.</summary>
    public int DirectorySector { get; }

    /// <summary>Offset du nom du volume dans le secteur d'en-tête.</summary>
    public int VolumeNameOffset { get; }

    /// <summary>Secteurs contenant le BAM.</summary>
    public IReadOnlyList<int> BamSectors { get; }

    /// <summary>Offset de la première entrée du BAM.</summary>
    public int BamEntriesOffset { get; }

    /// <summary>Taille d'une entrée du BAM.</summary>
    public int BamEntrySize { get; }

    /// <summary>Nombre d'entrées de BAM prises en charge.</summary>
    public int BamEntryCount { get; }

    /// <summary>Marqueur attendu dans le secteur d'en-tête.</summary>
    public byte HeaderSignature { get; }

    /// <summary>Résout la disposition correspondant à un identifiant de format.</summary>
    /// <param name="formatId">Identifiant du format d'image.</param>
    /// <returns>Disposition correspondante, ou <see langword="null"/> si le format est inconnu.</returns>
    public static CommodoreDosLayout? Resolve(string formatId) => formatId switch
    {
        DiskImageFormatIds.Commodore1541 or DiskImageFormatIds.Commodore1571 => D64D71,
        DiskImageFormatIds.Commodore1581 => D81,
        _ => null
    };
}
