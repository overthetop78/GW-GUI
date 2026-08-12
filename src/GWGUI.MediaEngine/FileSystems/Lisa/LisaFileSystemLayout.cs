namespace GWGUI.MediaEngine.FileSystems.Lisa;

/// <summary>Définit les identifiants, marqueurs et dispositions du système de fichiers Lisa.</summary>
public static class LisaFileSystemLayout
{
    /// <summary>Identifiant du fichier MDDF.</summary>
    public const ushort MddfFileId = 0x0001;
    /// <summary>Identifiant du bitmap.</summary>
    public const ushort BitmapFileId = 0x0002;
    /// <summary>Identifiant des S-records.</summary>
    public const ushort SRecordsFileId = 0x0003;
    /// <summary>Identifiant du catalogue.</summary>
    public const ushort CatalogFileId = 0x0004;
    /// <summary>Identifiant d'une page libre.</summary>
    public const ushort FreePageFileId = 0x0000;
    /// <summary>Marqueur supérieur d'une page libre.</summary>
    public const ushort AlternateFreePageFileId = 0x7fff;
    /// <summary>Premier identifiant de fichier utilisateur.</summary>
    public const ushort FirstUserFileId = CatalogFileId + 1;
    /// <summary>Dernier identifiant de fichier utilisateur.</summary>
    public const ushort LastUserFileId = AlternateFreePageFileId - 1;
    /// <summary>Offset de l'octet fort de l'identifiant dans un tag.</summary>
    public const int TagFileIdHighOffset = 4;
    /// <summary>Offset de l'octet faible de l'identifiant dans un tag.</summary>
    public const int TagFileIdLowOffset = 5;
    /// <summary>Offset de l'octet fort du numéro de page dans un tag.</summary>
    public const int TagPageHighOffset = 6;
    /// <summary>Offset de l'octet faible du numéro de page dans un tag.</summary>
    public const int TagPageLowOffset = 7;
    /// <summary>Longueur minimale d'un tag exploitable.</summary>
    public const int MinimumTagLength = TagPageLowOffset + 1;
    /// <summary>Masque du numéro de page.</summary>
    public const int PageNumberMask = 0x07ff;
    /// <summary>Taille d'une entrée du catalogue tabulaire.</summary>
    public const int TableEntrySize = 54;
    /// <summary>Taille d'une entrée de catalogue haché ou B-tree.</summary>
    public const int TreeEntrySize = 64;
    /// <summary>Offset de départ des entrées hachées ou B-tree.</summary>
    public const int TreeEntriesOffset = 0x50;
    /// <summary>Offset du nom dans une entrée.</summary>
    public const int CatalogNameOffset = 1;
    /// <summary>Longueur maximale d'un nom de catalogue.</summary>
    public const int CatalogNameLength = 31;
    /// <summary>Offset de l'identifiant du fichier dans une entrée.</summary>
    public const int CatalogFileIdOffset = 36;
    /// <summary>Marqueurs de fichiers système réservés exclus des fichiers utilisateur.</summary>
    public static IReadOnlySet<ushort> ReservedFileIds { get; } = new HashSet<ushort> { 0x00aa, 0x00bb, 0xaaaa, 0xbbbb };
}
