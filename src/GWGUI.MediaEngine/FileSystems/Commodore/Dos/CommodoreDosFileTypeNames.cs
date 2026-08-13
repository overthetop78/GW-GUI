namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Fournit les libellés associés aux types de fichiers Commodore DOS.</summary>
public static class CommodoreDosFileTypeNames
{
    /// <summary>Retourne le libellé du type de base.</summary>
    public static string GetBaseTypeName(CommodoreDosFileType fileType) => (fileType & CommodoreDosFileType.BaseTypeMask) switch
    {
        CommodoreDosFileType.Seq => "SEQ",
        CommodoreDosFileType.Prg => "PRG",
        CommodoreDosFileType.Usr => "USR",
        CommodoreDosFileType.Rel => "REL",
        CommodoreDosFileType.Cbm => "CBM",
        _ => "DEL"
    };

    /// <summary>Construit le commentaire affichant le type et ses drapeaux.</summary>
    public static string GetComment(CommodoreDosFileType fileType) => GetBaseTypeName(fileType) + (fileType.HasFlag(CommodoreDosFileType.Closed) ? string.Empty : ", open") + (fileType.HasFlag(CommodoreDosFileType.Locked) ? ", locked" : string.Empty);
}
