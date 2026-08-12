namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Décrit les champs du superbloc COHERENT utilisés par le moteur.</summary>
internal static class CoherentSuperblockLayout
{
    /// <summary>Taille d'un bloc logique en octets.</summary>
    public const int BlockSize = 512;
    /// <summary>Taille minimale permettant de lire les champs utilisés du superbloc.</summary>
    public const int MinimumImageSize = 1_024;
    /// <summary>Offset de la fin de la zone d'inodes.</summary>
    public const int InodeZoneEndOffset = 512;
    /// <summary>Offset du nombre de blocs du système de fichiers.</summary>
    public const int FileSystemBlockCountOffset = 514;
    /// <summary>Offset de la date de modification.</summary>
    public const int ModifiedTimeOffset = 976;
    /// <summary>Offset du nombre de blocs libres.</summary>
    public const int FreeBlockCountOffset = 980;
    /// <summary>Offset du nom de volume.</summary>
    public const int VolumeNameOffset = 996;
    /// <summary>Offset du nom de pack.</summary>
    public const int PackNameOffset = 1_002;
    /// <summary>Longueur des noms fixes.</summary>
    public const int NameLength = 6;
    /// <summary>Nom de volume par défaut.</summary>
    public const string DefaultVolumeName = "noname";
    /// <summary>Nom de pack par défaut.</summary>
    public const string DefaultPackName = "nopack";
    /// <summary>Marqueur utilisé pour un nom non renseigné.</summary>
    public const string PlaceholderName = "xxxxx";
    /// <summary>Caractère de remplissage du nom de volume.</summary>
    public const char VolumePadding = ' ';
    /// <summary>Caractère de remplissage du nom de pack.</summary>
    public const char PackPadding = '\n';
    /// <summary>Taille d'un inode.</summary>
    public const int InodeSize = 64;
    /// <summary>Numéro du premier inode utilisateur.</summary>
    public const int RootInodeNumber = 2;
    /// <summary>Mode identifiant un répertoire.</summary>
    public const ushort DirectoryMode = 0x4000;
    /// <summary>Masque isolant le type d'un inode.</summary>
    public const ushort TypeMask = 0xf000;
    /// <summary>Masque isolant les droits.</summary>
    public const ushort ProtectionMask = 0x0fff;
    /// <summary>Offset du mode dans un inode.</summary>
    public const int InodeModeOffset = 0;
    /// <summary>Offset de la taille dans un inode.</summary>
    public const int InodeSizeOffset = 8;
    /// <summary>Offset des pointeurs de blocs.</summary>
    public const int InodePointersOffset = 12;
    /// <summary>Taille d'un pointeur de bloc.</summary>
    public const int InodePointerSize = 3;
    /// <summary>Nombre total de pointeurs.</summary>
    public const int InodePointerCount = 13;
    /// <summary>Nombre de pointeurs directs.</summary>
    public const int DirectPointerCount = 10;
    /// <summary>Offset de la date de modification.</summary>
    public const int InodeModifiedOffset = 56;
    /// <summary>Taille d'une entrée de répertoire.</summary>
    public const int DirectoryEntrySize = 16;
    /// <summary>Longueur du nom dans une entrée.</summary>
    public const int DirectoryNameLength = 14;
}
