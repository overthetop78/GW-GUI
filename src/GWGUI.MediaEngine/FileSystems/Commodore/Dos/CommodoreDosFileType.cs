namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Décrit le type et les drapeaux d'une entrée Commodore DOS.</summary>
[Flags]
public enum CommodoreDosFileType : byte
{
    /// <summary>Masque isolant le type de base.</summary>
    BaseTypeMask = 0x07,
    /// <summary>Entrée supprimée.</summary>
    Del = 0,
    /// <summary>Fichier séquentiel.</summary>
    Seq = 1,
    /// <summary>Programme.</summary>
    Prg = 2,
    /// <summary>Fichier utilisateur.</summary>
    Usr = 3,
    /// <summary>Fichier relatif.</summary>
    Rel = 4,
    /// <summary>Fichier de partition CBM.</summary>
    Cbm = 5,
    /// <summary>Entrée verrouillée.</summary>
    Locked = 0x40,
    /// <summary>Fichier correctement fermé.</summary>
    Closed = 0x80
}
