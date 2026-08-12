namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Identifie les trois variantes connues du catalogue Lisa Office.</summary>
public enum LisaCatalogVersion : ushort
{
    /// <summary>Catalogue tabulaire.</summary>
    Table = 0x000e,
    /// <summary>Catalogue haché.</summary>
    Hash = 0x000f,
    /// <summary>Catalogue B-tree.</summary>
    BTree = 0x0011
}

/// <summary>Fournit les noms techniques des variantes de catalogue Lisa.</summary>
public static class LisaCatalogVersionNames
{
    /// <summary>Retourne le nom technique d'une version connue ou inconnue.</summary>
    public static string Get(ushort version) => (LisaCatalogVersion)version switch { LisaCatalogVersion.Table => "table", LisaCatalogVersion.Hash => "hash", LisaCatalogVersion.BTree => "B-tree", _ => $"inconnue-0x{version:X4}" };
}
