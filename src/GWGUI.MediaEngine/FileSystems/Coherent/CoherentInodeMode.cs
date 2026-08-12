namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Définit les types V7 stockés dans le mode d'un inode COHERENT.</summary>
public enum CoherentInodeMode : ushort
{
    /// <summary>Type inconnu.</summary>
    Unknown = 0,
    /// <summary>Fichier régulier.</summary>
    Regular = 0x8000,
    /// <summary>Répertoire.</summary>
    Directory = 0x4000,
    /// <summary>Périphérique caractère.</summary>
    CharacterDevice = 0x2000,
    /// <summary>Périphérique bloc.</summary>
    BlockDevice = 0x6000,
    /// <summary>Tube nommé.</summary>
    NamedPipe = 0x1000
}

/// <summary>Convertit les modes COHERENT vers le modèle commun.</summary>
public static class CoherentInodeModeExtensions
{
    /// <summary>Extrait le type de l'inode.</summary>
    public static CoherentInodeMode Type(this ushort mode) => (CoherentInodeMode)(mode & CoherentFileSystemLayout.TypeMask);
    /// <summary>Convertit le type en nature commune.</summary>
    public static FileSystemEntryKind ToCommonKind(this CoherentInodeMode mode) => mode switch { CoherentInodeMode.Directory => FileSystemEntryKind.Directory, CoherentInodeMode.Regular => FileSystemEntryKind.File, _ => FileSystemEntryKind.Unknown };
}
