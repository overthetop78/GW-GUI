namespace GWGUI.MediaEngine.FileSystems.Commodore;

/// <summary>Décrit le type et les drapeaux d'une entrée Commodore DOS.</summary>
[Flags]
public enum CommodoreDosFileType : byte
{
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

/// <summary>Fournit les libellés associés aux types de fichiers Commodore DOS.</summary>
public static class CommodoreDosFileTypeNames
{
    /// <summary>Retourne le libellé du type de base.</summary>
    /// <param name="fileType">Type et drapeaux lus dans le répertoire.</param>
    /// <returns>Libellé du type de base.</returns>
    public static string GetBaseTypeName(CommodoreDosFileType fileType) => (fileType & (CommodoreDosFileType)0x07) switch
    {
        CommodoreDosFileType.Seq => "SEQ",
        CommodoreDosFileType.Prg => "PRG",
        CommodoreDosFileType.Usr => "USR",
        CommodoreDosFileType.Rel => "REL",
        CommodoreDosFileType.Cbm => "CBM",
        _ => "DEL"
    };

    /// <summary>Construit le commentaire affichant le type et ses drapeaux.</summary>
    /// <param name="fileType">Type et drapeaux lus dans le répertoire.</param>
    /// <returns>Commentaire décrivant l'entrée.</returns>
    public static string GetComment(CommodoreDosFileType fileType) => GetBaseTypeName(fileType) + (fileType.HasFlag(CommodoreDosFileType.Closed) ? string.Empty : ", open") + (fileType.HasFlag(CommodoreDosFileType.Locked) ? ", locked" : string.Empty);
}
