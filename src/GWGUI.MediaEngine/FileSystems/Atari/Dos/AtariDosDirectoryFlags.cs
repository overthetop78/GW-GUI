namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Définit les drapeaux d'une entrée de répertoire Atari DOS.</summary>
[Flags]
public enum AtariDosDirectoryFlags : byte
{
    /// <summary>Aucun drapeau.</summary>
    None = 0,
    /// <summary>Entrée encore ouverte en écriture.</summary>
    OpenForOutput = AtariDosFileSystemLayout.OpenForOutputFlag,
    /// <summary>Entrée créée par Atari DOS 2.</summary>
    CreatedByDos2 = AtariDosFileSystemLayout.CreatedByDos2Flag,
    /// <summary>Entrée verrouillée.</summary>
    Locked = AtariDosFileSystemLayout.LockedFlag,
    /// <summary>Entrée active.</summary>
    InUse = AtariDosFileSystemLayout.InUseFlag,
    /// <summary>Entrée supprimée.</summary>
    Deleted = AtariDosFileSystemLayout.DeletedFlag
}
