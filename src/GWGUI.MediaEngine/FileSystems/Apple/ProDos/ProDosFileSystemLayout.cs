namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Définit la disposition des volumes, répertoires et fichiers ProDOS.</summary>
internal static class ProDosFileSystemLayout
{
    /// <summary>Taille d'un bloc.</summary>
    public const int BlockSize = 512;
    /// <summary>Bloc du répertoire racine.</summary>
    public const int RootBlock = 2;
    /// <summary>Offset du premier en-tête.</summary>
    public const int HeaderOffset = 4;
    /// <summary>Offset de la longueur d'entrée dans l'en-tête.</summary>
    public const int HeaderEntryLengthOffset = 0x23;
    /// <summary>Nombre de blocs réservés au répertoire racine standard.</summary>
    public const int RootDirectoryBlockCount = 4;
    /// <summary>Premier bloc du bitmap d'allocation standard.</summary>
    public const int DefaultBitmapBlock = 6;
    /// <summary>Offset du type de stockage et de la longueur du nom.</summary>
    public const int StorageAndNameLengthOffset = 0;
    /// <summary>Taille d'une entrée de répertoire.</summary>
    public const int EntrySize = 0x27;
    /// <summary>Offset du nom dans une entrée.</summary>
    public const int NameOffset = 1;
    /// <summary>Longueur maximale du nom.</summary>
    public const int MaximumNameLength = 15;
    /// <summary>Offset du type de fichier.</summary>
    public const int FileTypeOffset = 16;
    /// <summary>Offset du bloc clé.</summary>
    public const int KeyBlockOffset = 17;
    /// <summary>Offset du nombre de blocs utilisés.</summary>
    public const int BlocksUsedOffset = 19;
    /// <summary>Offset de la longueur du fichier.</summary>
    public const int EndOfFileOffset = 21;
    /// <summary>Offset de la date de création.</summary>
    public const int CreatedDateOffset = 24;
    /// <summary>Offset des droits d'accès.</summary>
    public const int AccessOffset = 30;
    /// <summary>Offset de la date de modification.</summary>
    public const int ModifiedDateOffset = 33;
    /// <summary>Offset du pointeur vers l'en-tête du répertoire parent.</summary>
    public const int HeaderPointerOffset = 37;
    /// <summary>Offset absolu du champ réservé d'un en-tête de sous-répertoire.</summary>
    public const int SubdirectoryReservedOffset = HeaderOffset + 16;
    /// <summary>Valeur réservée utilisée par les en-têtes de sous-répertoires récents.</summary>
    public const byte SubdirectoryReservedValue = 0x76;
    /// <summary>Offset absolu de la version d'un sous-répertoire.</summary>
    public const int SubdirectoryVersionOffset = HeaderOffset + 28;
    /// <summary>Version de sous-répertoire compatible GS/OS.</summary>
    public const byte SubdirectoryVersion = 5;
    /// <summary>Droits de lecture, écriture, renommage et suppression d'une entrée déverrouillée.</summary>
    public const byte DefaultAccess = 0xc3;
    /// <summary>Offset absolu du bloc contenant l'entrée parente.</summary>
    public const int SubdirectoryParentBlockOffset = HeaderOffset + 35;
    /// <summary>Offset absolu du numéro d'entrée parente.</summary>
    public const int SubdirectoryParentEntryOffset = HeaderOffset + 37;
    /// <summary>Offset absolu de la longueur de l'entrée parente.</summary>
    public const int SubdirectoryParentEntryLengthOffset = HeaderOffset + 38;
    /// <summary>Nombre d'entrées par bloc de répertoire.</summary>
    public const int EntriesPerDirectoryBlock = 13;
    /// <summary>Offset du bloc précédent.</summary>
    public const int PreviousBlockOffset = 0;
    /// <summary>Offset du pointeur vers le bloc suivant.</summary>
    public const int NextBlockOffset = 2;
    /// <summary>Offset de la première entrée suivant l'en-tête de volume.</summary>
    public const int FirstVolumeEntryOffset = HeaderOffset + EntrySize;
    /// <summary>Offset de la première entrée des blocs suivants.</summary>
    public const int FirstChainedEntryOffset = HeaderOffset;
    /// <summary>Nombre de pointeurs dans un bloc d'index.</summary>
    public const int IndexPointerCount = 256;
    /// <summary>Offset de la partie haute des pointeurs d'index.</summary>
    public const int IndexHighBytesOffset = 256;
    /// <summary>Nombre d'octets dans chaque moitié d'un bloc d'index.</summary>
    public const int IndexHalfLength = 256;
    /// <summary>Offset du bloc bitmap dans l'en-tête du volume.</summary>
    public const int BitmapBlockOffset = HeaderOffset + 35;
    /// <summary>Offset du nombre total de blocs.</summary>
    public const int TotalBlocksOffset = HeaderOffset + 37;
    /// <summary>Nombre de blocs décrits par un bloc bitmap.</summary>
    public const int BlocksPerBitmapBlock = 4096;
    /// <summary>Longueur maximale d'un fichier exprimée sur 24 bits.</summary>
    public const int MaximumFileLength = 0x00ffffff;
    /// <summary>Masque du bit de poids fort d'un octet de bitmap.</summary>
    public const byte BitmapHighBitMask = 0x80;
    /// <summary>Nombre de bits dans un octet.</summary>
    public const int BitsPerByte = 8;
    /// <summary>Profondeur maximale protégeant des cycles de répertoires.</summary>
    public const int MaximumDirectoryDepth = 64;
    /// <summary>Masque de la longueur d'un nom.</summary>
    public const byte NameLengthMask = 0x0f;
    /// <summary>Décalage du type de stockage.</summary>
    public const int StorageTypeShift = 4;
    /// <summary>Type de stockage seedling.</summary>
    public const int SeedlingStorageType = (int)ProDosStorageType.Seedling;
    /// <summary>Type de stockage sapling.</summary>
    public const int SaplingStorageType = (int)ProDosStorageType.Sapling;
    /// <summary>Type de stockage tree.</summary>
    public const int TreeStorageType = (int)ProDosStorageType.Tree;
    /// <summary>Type de stockage d'un sous-répertoire.</summary>
    public const int SubdirectoryStorageType = (int)ProDosStorageType.Subdirectory;
    /// <summary>Type de stockage d'un en-tête de volume.</summary>
    public const int VolumeHeaderStorageType = (int)ProDosStorageType.VolumeHeader;
}
