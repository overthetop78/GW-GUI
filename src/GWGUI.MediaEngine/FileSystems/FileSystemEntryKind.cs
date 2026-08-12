namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Indique la nature commune d'une entrée de système de fichiers.</summary>
public enum FileSystemEntryKind
{
    /// <summary>Répertoire contenant éventuellement d'autres entrées.</summary>
    Directory,
    /// <summary>Fichier contenant éventuellement des données.</summary>
    File,
    /// <summary>Lien vers une autre entrée.</summary>
    Link,
    /// <summary>Entrée reconnue dont le type propre au format n'est pas interprété.</summary>
    Unknown
}
