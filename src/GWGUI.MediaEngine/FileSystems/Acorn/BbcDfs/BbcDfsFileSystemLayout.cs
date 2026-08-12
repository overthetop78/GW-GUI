namespace GWGUI.MediaEngine.FileSystems.Acorn.BbcDfs;

/// <summary>Définit la disposition et les masques du catalogue BBC DFS.</summary>
internal static class BbcDfsFileSystemLayout
{
    /// <summary>Taille d'un secteur.</summary>
    public const int SectorSize = 256;
    /// <summary>Secteur contenant les noms.</summary>
    public const int NamesSector = 0;
    /// <summary>Secteur contenant les métadonnées.</summary>
    public const int MetadataSector = 1;
    /// <summary>Nombre maximal d'entrées.</summary>
    public const int MaximumEntryCount = 31;
    /// <summary>Taille de chaque partie d'entrée.</summary>
    public const int EntryPartSize = 8;
    /// <summary>Offset du titre dans le secteur des noms.</summary>
    public const int TitleFirstOffset = 0;
    /// <summary>Longueur de la première partie du titre.</summary>
    public const int TitleFirstLength = 8;
    /// <summary>Longueur de la seconde partie du titre.</summary>
    public const int TitleSecondLength = 4;
    /// <summary>Offset du compteur d'entrées.</summary>
    public const int EntryCountOffset = 5;
    /// <summary>Offset des bits hauts du nombre de secteurs.</summary>
    public const int TotalSectorsHighOffset = 6;
    /// <summary>Offset de l'octet bas du nombre de secteurs.</summary>
    public const int TotalSectorsLowOffset = 7;
    /// <summary>Masque des bits hauts du nombre de secteurs.</summary>
    public const byte TotalSectorsHighMask = 3;
    /// <summary>Offset de la première entrée.</summary>
    public const int FirstEntryOffset = 8;
    /// <summary>Longueur du nom de fichier.</summary>
    public const int LeafNameLength = 7;
    /// <summary>Offset du caractère de répertoire.</summary>
    public const int DirectoryOffset = 7;
    /// <summary>Masque des caractères ASCII.</summary>
    public const byte CharacterMask = 0x7f;
    /// <summary>Bit de verrouillage dans le caractère de répertoire.</summary>
    public const byte LockedBit = 0x80;
    /// <summary>Offset de la longueur dans les métadonnées.</summary>
    public const int LengthOffset = 4;
    /// <summary>Offset de l'octet compacté.</summary>
    public const int PackedOffset = 6;
    /// <summary>Offset du secteur initial.</summary>
    public const int StartSectorOffset = 7;
    /// <summary>Masque haut de la longueur.</summary>
    public const byte LengthHighMask = 0x30;
    /// <summary>Décalage haut de la longueur.</summary>
    public const int LengthHighShift = 12;
    /// <summary>Masque haut du secteur initial.</summary>
    public const byte StartSectorHighMask = 3;
    /// <summary>Masque haut de l'adresse de chargement.</summary>
    public const byte LoadHighMask = 0x0c;
    /// <summary>Décalage haut de l'adresse de chargement.</summary>
    public const int LoadHighShift = 14;
    /// <summary>Masque haut de l'adresse d'exécution.</summary>
    public const byte ExecuteHighMask = 0xc0;
    /// <summary>Décalage haut de l'adresse d'exécution.</summary>
    public const int ExecuteHighShift = 10;
}
