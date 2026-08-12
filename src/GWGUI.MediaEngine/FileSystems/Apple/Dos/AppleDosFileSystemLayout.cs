namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Définit la géométrie et les champs binaires d'Apple DOS 3.2 et 3.3.</summary>
public static class AppleDosFileSystemLayout
{
    /// <summary>Taille d'un secteur, en octets.</summary>
    public const int SectorSize = 256;
    /// <summary>Nombre de pistes d'une disquette Apple DOS.</summary>
    public const int TrackCount = 35;
    /// <summary>Nombre de secteurs par piste d'Apple DOS 3.2.</summary>
    public const int Dos32SectorsPerTrack = 13;
    /// <summary>Nombre de secteurs par piste d'Apple DOS 3.3.</summary>
    public const int Dos33SectorsPerTrack = 16;
    /// <summary>Piste contenant le VTOC.</summary>
    public const int VtocTrack = 17;
    /// <summary>Offset de la piste du premier catalogue dans le VTOC.</summary>
    public const int VtocCatalogTrackOffset = 1;
    /// <summary>Offset du secteur du premier catalogue dans le VTOC.</summary>
    public const int VtocCatalogSectorOffset = 2;
    /// <summary>Offset du numéro de volume.</summary>
    public const int VtocVolumeNumberOffset = 6;
    /// <summary>Offset du nombre de pistes.</summary>
    public const int VtocTrackCountOffset = 0x34;
    /// <summary>Offset du nombre de secteurs par piste.</summary>
    public const int VtocSectorsPerTrackOffset = 0x35;
    /// <summary>Offset de la taille sectorielle.</summary>
    public const int VtocSectorSizeOffset = 0x36;
    /// <summary>Offset du bitmap des secteurs libres.</summary>
    public const int VtocFreeBitmapOffset = 0x38;
    /// <summary>Taille du bitmap d'une piste.</summary>
    public const int VtocTrackBitmapSize = 4;
    /// <summary>Offset de la première entrée d'un secteur de catalogue.</summary>
    public const int CatalogFirstEntryOffset = 0x0b;
    /// <summary>Taille d'une entrée de catalogue.</summary>
    public const int CatalogEntrySize = 35;
    /// <summary>Nombre d'entrées par secteur de catalogue.</summary>
    public const int CatalogEntriesPerSector = 7;
    /// <summary>Offset de la piste de liste T/S dans une entrée.</summary>
    public const int EntryTrackOffset = 0;
    /// <summary>Offset du secteur de liste T/S dans une entrée.</summary>
    public const int EntrySectorOffset = 1;
    /// <summary>Offset du type de fichier dans une entrée.</summary>
    public const int EntryTypeOffset = 2;
    /// <summary>Offset du nom dans une entrée.</summary>
    public const int EntryNameOffset = 3;
    /// <summary>Longueur du nom dans une entrée.</summary>
    public const int EntryNameLength = 30;
    /// <summary>Offset du nombre de secteurs déclaré.</summary>
    public const int EntrySectorCountOffset = 33;
    /// <summary>Offset du premier couple piste/secteur d'une liste T/S.</summary>
    public const int TrackSectorPairsOffset = 0x0c;
    /// <summary>Taille d'un couple piste/secteur.</summary>
    public const int TrackSectorPairSize = 2;
    /// <summary>Nombre maximal de couples piste/secteur.</summary>
    public const int TrackSectorPairCount = 122;
    /// <summary>Masque retirant le bit fort des noms et types.</summary>
    public const byte ValueMask = 0x7f;
    /// <summary>Marqueur d'une entrée supprimée.</summary>
    public const byte DeletedEntryMarker = 0xff;
    /// <summary>Marqueur d'une entrée inutilisée et fin des entrées du secteur.</summary>
    public const byte UnusedEntryMarker = 0;
    /// <summary>Masque du bit de verrouillage dans le type brut.</summary>
    public const byte LockedMask = 0x80;
    /// <summary>Offset de la piste suivante dans un catalogue ou une liste T/S.</summary>
    public const int NextTrackOffset = 1;
    /// <summary>Offset du secteur suivant dans un catalogue ou une liste T/S.</summary>
    public const int NextSectorOffset = 2;

    /// <summary>Valide une coordonnée physique Apple DOS.</summary>
    public static bool IsValidAddress(int track, int sector, int tracks, int sectorsPerTrack) => track >= 0 && track < tracks && sector >= 0 && sector < sectorsPerTrack;

    /// <summary>Valide les pointeurs et la géométrie déclarés par un VTOC.</summary>
    public static bool IsValidVtoc(ReadOnlySpan<byte> vtoc, int trackCount, int sectorsPerTrack)
    {
        if (vtoc.Length < VtocSectorSizeOffset + sizeof(ushort)) return false;
        var declaredSectorSize = vtoc[VtocSectorSizeOffset] | vtoc[VtocSectorSizeOffset + 1] << 8;
        return vtoc[VtocCatalogTrackOffset] is > 0 && vtoc[VtocCatalogTrackOffset] < trackCount && vtoc[VtocCatalogSectorOffset] < sectorsPerTrack && vtoc[VtocSectorsPerTrackOffset] == sectorsPerTrack && declaredSectorSize == SectorSize;
    }
}
