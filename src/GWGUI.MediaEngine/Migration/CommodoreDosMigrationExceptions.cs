namespace GWGUI.MediaEngine.Migration;

/// <summary>Construit les erreurs de migration vers Commodore DOS.</summary>
public static class CommodoreDosMigrationExceptions
{
    /// <summary>Indique une combinaison format/conteneur non prise en charge.</summary>
    public static InvalidDataException UnsupportedTarget(string formatId, string extension) => new($"The Commodore DOS migration target '{formatId}' cannot be written to '{extension}'.");
}
