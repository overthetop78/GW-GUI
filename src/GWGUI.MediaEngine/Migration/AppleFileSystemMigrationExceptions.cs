namespace GWGUI.MediaEngine.Migration;

/// <summary>Construit les erreurs de migration vers les systèmes de fichiers Apple.</summary>
public static class AppleFileSystemMigrationExceptions
{
    /// <summary>Indique que la cible Apple ou son conteneur n'est pas pris en charge.</summary>
    public static InvalidDataException UnsupportedTarget(string formatId, string extension) => new($"The Apple migration target '{formatId}' cannot be written to '{extension}'.");
}
