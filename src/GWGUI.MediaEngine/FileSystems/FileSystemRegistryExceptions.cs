namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Construit les erreurs de configuration du registre de systèmes de fichiers.</summary>
public static class FileSystemRegistryExceptions
{
    /// <summary>Crée l'erreur signalant un lecteur nul.</summary>
    public static ArgumentException NullReader(int index) => new($"Le lecteur de système de fichiers à l'index {index} est nul.", "readers");
    /// <summary>Crée l'erreur signalant un identifiant vide.</summary>
    public static ArgumentException EmptyReaderId(int index) => new($"Le lecteur de système de fichiers à l'index {index} possède un identifiant vide.", "readers");
    /// <summary>Crée l'erreur signalant un identifiant dupliqué.</summary>
    public static ArgumentException DuplicateReaderId(string id) => new($"Plusieurs lecteurs de systèmes de fichiers possèdent l'identifiant '{id}'.", "readers");
    /// <summary>Crée l'erreur signalant un identifiant demandé absent.</summary>
    public static KeyNotFoundException UnknownReaderId(string id) => new($"Aucun lecteur de système de fichiers ne possède l'identifiant '{id}'.");
}
