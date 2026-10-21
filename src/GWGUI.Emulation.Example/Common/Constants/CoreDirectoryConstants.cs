namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Noms des sous-dossiers de données utilisés pour préparer et exécuter les cœurs.</summary>
/// <remarks>Les services composent ces noms avec une racine fournie par l’hôte.
/// Le service qui crée un dossier de session en assure le nettoyage après destruction du cœur ;
/// les sauvegardes persistantes et les fichiers fournis par l’utilisateur gardent leur propriétaire distinct.</remarks>
internal static class CoreDirectoryConstants
{
    /// <summary>Dossier des fichiers système et firmwares préparés pour le cœur.</summary>
    internal const string SystemDirectoryName = "System";
    /// <summary>Dossier de préparation du contenu utilisé par la session.</summary>
    internal const string ContentDirectoryName = "Content";
    /// <summary>Dossier des sauvegardes persistantes du cœur, distinct des fichiers de session.</summary>
    internal const string SavesDirectoryName = "Saves";
    /// <summary>Dossier des ressources de données accompagnant le cœur.</summary>
    internal const string AssetsDirectoryName = "Assets";
    /// <summary>Dossier regroupant les documents persistants de configuration des machines.</summary>
    internal const string ConfigurationsDirectoryName = "Configurations";
    /// <summary>Dossier des mémoires persistantes du cœur, conservées entre les sessions.</summary>
    /// <remarks>Ce dossier de données est distinct du dossier temporaire de session et des fichiers d’état de la machine.</remarks>
    internal const string MemoryDirectoryName = "Memory";
    /// <summary>Dossier regroupant les installations des bibliothèques de cœurs du module.</summary>
    internal const string CoreDirectoryName = "Core";
}

