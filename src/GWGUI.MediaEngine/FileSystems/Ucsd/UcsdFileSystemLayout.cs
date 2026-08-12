namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Définit la disposition du répertoire UCSD p-System.</summary>
public static class UcsdFileSystemLayout
{
    /// <summary>Taille d'un bloc.</summary>
    public const int BlockSize = 512;
    /// <summary>Premier bloc du répertoire.</summary>
    public const int DirectoryBlock = 2;
    /// <summary>Taille d'une entrée.</summary>
    public const int EntrySize = 26;
    /// <summary>Fin d'un répertoire court.</summary>
    public const int ShortDirectoryEnd = 6;
    /// <summary>Fin d'un répertoire long.</summary>
    public const int LongDirectoryEnd = 10;
    /// <summary>Nombre de blocs du répertoire court.</summary>
    public const int ShortDirectoryBlockCount = 4;
    /// <summary>Nombre de blocs du répertoire long.</summary>
    public const int LongDirectoryBlockCount = 8;
    /// <summary>Longueur maximale du nom du volume.</summary>
    public const int MaximumVolumeNameLength = 7;
    /// <summary>Nombre maximal de fichiers.</summary>
    public const int MaximumFileCount = 77;
    /// <summary>Premier caractère ASCII admis.</summary>
    public const byte MinimumNameCharacter = 0x20;
    /// <summary>Premier caractère ASCII exclu.</summary>
    public const byte MaximumNameCharacterExclusive = 0x7f;
    /// <summary>Offset de la fin de répertoire.</summary>
    public const int DirectoryEndOffset = 2;
    /// <summary>Offset du champ de nom du volume.</summary>
    public const int VolumeNameOffset = 6;
    /// <summary>Longueur du champ de nom du volume.</summary>
    public const int VolumeNameFieldLength = 8;
    /// <summary>Offset du nombre total de blocs.</summary>
    public const int TotalBlocksOffset = 14;
    /// <summary>Offset du nombre de fichiers.</summary>
    public const int FileCountOffset = 16;
    /// <summary>Offset de la date du volume.</summary>
    public const int VolumeDateOffset = 20;
    /// <summary>Offset du premier bloc d'une entrée.</summary>
    public const int EntryFirstBlockOffset = 0;
    /// <summary>Offset du dernier bloc d'une entrée.</summary>
    public const int EntryLastBlockOffset = 2;
    /// <summary>Offset du type d'une entrée.</summary>
    public const int EntryKindOffset = 4;
    /// <summary>Offset du champ de nom d'une entrée.</summary>
    public const int EntryNameOffset = 6;
    /// <summary>Longueur du champ de nom d'une entrée.</summary>
    public const int EntryNameFieldLength = 16;
    /// <summary>Longueur maximale du nom d'un fichier.</summary>
    public const int MaximumFileNameLength = 15;
    /// <summary>Offset du nombre d'octets du dernier bloc.</summary>
    public const int EntryLastBlockBytesOffset = 22;
    /// <summary>Offset de la date d'une entrée.</summary>
    public const int EntryDateOffset = 24;
    /// <summary>Masque du type de fichier.</summary>
    public const int FileKindMask = 0x0f;

    /// <summary>Indique si une valeur désigne une fin de répertoire courte ou longue.</summary>
    public static bool IsDirectoryEnd(int value) => value is ShortDirectoryEnd or LongDirectoryEnd;

    /// <summary>Retourne le nombre de blocs occupés par le répertoire annoncé.</summary>
    public static int DirectoryBlockCount(int endDirectory) => endDirectory == LongDirectoryEnd ? LongDirectoryBlockCount : ShortDirectoryBlockCount;
}
