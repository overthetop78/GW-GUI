namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Définit les drapeaux d'une entrée de répertoire Atari DOS.</summary>
[Flags]
public enum AtariDosDirectoryFlags : byte
{
    /// <summary>Aucun drapeau.</summary>
    None = 0,
    /// <summary>Entrée active.</summary>
    InUse = AtariDosFileSystemLayout.InUseFlag,
    /// <summary>Entrée supprimée.</summary>
    Deleted = AtariDosFileSystemLayout.DeletedFlag
}
