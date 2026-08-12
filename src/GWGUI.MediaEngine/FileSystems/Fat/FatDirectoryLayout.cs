namespace GWGUI.MediaEngine.FileSystems.Fat;

/// <summary>Définit la disposition d'une entrée de répertoire FAT.</summary>
public static class FatDirectoryLayout
{
    /// <summary>Taille d'une entrée.</summary>
    public const int EntrySize = 32;
    /// <summary>Longueur du nom principal.</summary>
    public const int NameLength = 8;
    /// <summary>Offset de l'extension.</summary>
    public const int ExtensionOffset = NameLength;
    /// <summary>Longueur de l'extension.</summary>
    public const int ExtensionLength = 3;
    /// <summary>Offset des attributs.</summary>
    public const int AttributesOffset = 11;
    /// <summary>Offset de l'heure de modification.</summary>
    public const int ModifiedTimeOffset = 22;
    /// <summary>Offset de la date de modification.</summary>
    public const int ModifiedDateOffset = 24;
    /// <summary>Offset du premier cluster.</summary>
    public const int FirstClusterOffset = 26;
    /// <summary>Offset de la taille du fichier.</summary>
    public const int FileSizeOffset = 28;
    /// <summary>Marqueur de fin du répertoire.</summary>
    public const byte EndMarker = 0x00;
    /// <summary>Marqueur d'une entrée supprimée.</summary>
    public const byte DeletedMarker = 0xe5;
    /// <summary>Combinaison d'attributs identifiant une entrée de nom long.</summary>
    public const FatDirectoryAttributes LongFileName = FatDirectoryAttributes.ReadOnly | FatDirectoryAttributes.Hidden | FatDirectoryAttributes.System | FatDirectoryAttributes.VolumeLabel;
}

/// <summary>Décrit les attributs d'une entrée de répertoire FAT.</summary>
[Flags]
public enum FatDirectoryAttributes : byte
{
    /// <summary>Aucun attribut.</summary>
    None = 0,
    /// <summary>Entrée en lecture seule.</summary>
    ReadOnly = 0x01,
    /// <summary>Entrée masquée.</summary>
    Hidden = 0x02,
    /// <summary>Entrée système.</summary>
    System = 0x04,
    /// <summary>Label de volume.</summary>
    VolumeLabel = 0x08,
    /// <summary>Répertoire.</summary>
    Directory = 0x10,
    /// <summary>Archive.</summary>
    Archive = 0x20
}
