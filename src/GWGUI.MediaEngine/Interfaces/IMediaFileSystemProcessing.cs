namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Expose les opérations partagées de métadonnées et de parcours des fichiers par MediaEngine.</summary>
public interface IMediaFileSystemProcessing
{
    /// <summary>Lit un entier décimal invariant ; retourne zéro lorsque la clé manque ou que sa valeur est invalide.</summary>
    int ReadIntOrDefault(IReadOnlyDictionary<string, string> metadata, string key);

    /// <summary>Énumère les fichiers d'un arbre emprunté dans leur ordre, en conservant le type d'entrée du consommateur.</summary>
    /// <param name="entry">Racine du parcours, elle-même incluse si elle représente un fichier.</param>
    /// <param name="isFile">Prédicat identifiant les fichiers.</param>
    /// <param name="children">Accès aux enfants de chaque entrée ; les collections restent empruntées.</param>
    /// <returns>Parcours différé des fichiers, identique à celui du lecteur de systèmes de fichiers.</returns>
    IEnumerable<TEntry> EnumerateFiles<TEntry>(TEntry entry, Func<TEntry, bool> isFile, Func<TEntry, IReadOnlyList<TEntry>> children);
}
