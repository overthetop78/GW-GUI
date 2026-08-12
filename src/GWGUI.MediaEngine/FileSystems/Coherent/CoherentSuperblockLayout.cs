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
}
