namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Définit la disposition du home block et des segments de répertoire RT-11.</summary>
public static class Rt11FileSystemLayout
{
    /// <summary>Taille logique d'un bloc RT-11.</summary>
    public const int BlockSize = 512;
    /// <summary>Numéro du home block.</summary>
    public const int HomeBlock = 1;
    /// <summary>Signature système attendue.</summary>
    public const string SystemSignature = "DECRT11";
    /// <summary>Offset du premier segment de répertoire.</summary>
    public const int DirectoryBlockOffset = 468;
    /// <summary>Offset du nom du volume.</summary>
    public const int VolumeNameOffset = 472;
    /// <summary>Longueur du nom du volume.</summary>
    public const int VolumeNameLength = 12;
    /// <summary>Offset de l'identifiant système.</summary>
    public const int SystemIdOffset = 496;
    /// <summary>Longueur de l'identifiant système.</summary>
    public const int SystemIdLength = 12;
    /// <summary>Premier bloc de répertoire admis.</summary>
    public const int MinimumDirectoryBlock = 2;
    /// <summary>Premier bloc de répertoire exclu.</summary>
    public const int MaximumDirectoryBlockExclusive = 1001;
    /// <summary>Nombre de blocs d'un segment.</summary>
    public const int SegmentBlockCount = 2;
    /// <summary>Nombre maximal de segments.</summary>
    public const int MaximumSegmentCount = 31;
    /// <summary>Offset du segment suivant.</summary>
    public const int NextSegmentOffset = 2;
    /// <summary>Offset du nombre d'octets supplémentaires.</summary>
    public const int ExtraBytesOffset = 6;
    /// <summary>Offset du premier bloc de données.</summary>
    public const int DataBlockOffset = 8;
    /// <summary>Offset de la première entrée.</summary>
    public const int EntriesOffset = 10;
    /// <summary>Taille minimale d'une entrée.</summary>
    public const int MinimumEntrySize = 14;
    /// <summary>Taille maximale d'une entrée.</summary>
    public const int MaximumEntrySize = 128;
    /// <summary>Offset du statut d'une entrée.</summary>
    public const int StatusOffset = 0;
    /// <summary>Offset des deux mots du nom.</summary>
    public const int NameOffset = 2;
    /// <summary>Offset du mot d'extension.</summary>
    public const int ExtensionOffset = 6;
    /// <summary>Offset de la longueur en blocs.</summary>
    public const int BlockLengthOffset = 8;
    /// <summary>Offset de la date.</summary>
    public const int DateOffset = 12;
    /// <summary>Attribut brut d'un fichier protégé.</summary>
    public const uint ProtectedAttribute = 1;
    /// <summary>Attribut brut d'un fichier non protégé.</summary>
    public const uint UnprotectedAttribute = 0;
    /// <summary>Description d'un fichier provisoire.</summary>
    public const string TentativeFileDescription = "Fichier RT-11 provisoire";
    /// <summary>Description d'un fichier permanent.</summary>
    public const string PermanentFileDescription = "Fichier RT-11";

    /// <summary>Retourne la description technique correspondant au statut.</summary>
    public static string FileDescription(Rt11DirectoryEntryStatus status) => status.HasFlag(Rt11DirectoryEntryStatus.Tentative) ? TentativeFileDescription : PermanentFileDescription;
    /// <summary>Alphabet RADIX-50 RT-11.</summary>
    /// <summary>Base de l'année RT-11.</summary>
}
