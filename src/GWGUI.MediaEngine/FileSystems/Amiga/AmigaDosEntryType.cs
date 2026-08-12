namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Identifie les types secondaires d'entrée AmigaDOS.</summary>
public enum AmigaDosEntryType
{
    /// <summary>Type non interprété.</summary>
    Unknown = 0,
    /// <summary>Répertoire.</summary>
    Directory = AmigaDosLayout.DirectorySecondaryType,
    /// <summary>Fichier.</summary>
    File = AmigaDosLayout.FileSecondaryType,
    /// <summary>Lien dur.</summary>
    HardLink = AmigaDosLayout.HardLinkSecondaryType,
    /// <summary>Lien vers un répertoire.</summary>
    DirectoryLink = AmigaDosLayout.DirectoryLinkSecondaryType,
    /// <summary>Lien vers un fichier.</summary>
    FileLink = AmigaDosLayout.FileLinkSecondaryType
}

/// <summary>Convertit les types secondaires AmigaDOS vers le modèle commun.</summary>
public static class AmigaDosEntryTypeExtensions
{
    /// <summary>Convertit une valeur brute en type AmigaDOS connu.</summary>
    public static AmigaDosEntryType FromRaw(int value) => Enum.IsDefined(typeof(AmigaDosEntryType), value) ? (AmigaDosEntryType)value : AmigaDosEntryType.Unknown;
    /// <summary>Convertit le type AmigaDOS en nature commune.</summary>
    public static FileSystemEntryKind ToCommonKind(this AmigaDosEntryType type) => type switch
    {
        AmigaDosEntryType.Directory => FileSystemEntryKind.Directory,
        AmigaDosEntryType.File => FileSystemEntryKind.File,
        AmigaDosEntryType.HardLink or AmigaDosEntryType.DirectoryLink or AmigaDosEntryType.FileLink => FileSystemEntryKind.Link,
        _ => FileSystemEntryKind.Unknown
    };
}
