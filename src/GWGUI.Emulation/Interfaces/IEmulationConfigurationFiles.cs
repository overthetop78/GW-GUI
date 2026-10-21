using System.Text.Json;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Persistance commune : le module conserve son schéma et fournit les conversions de chemins.</summary>
public interface IEmulationConfigurationFiles
{
    /// <summary>Stocke relativement à la base un chemin situé dans cette base ; conserve un chemin absolu extérieur.</summary>
    /// <param name="path">Chemin à enregistrer ; les valeurs nulles, vides ou blanches sont conservées.</param>
    /// <param name="pathBase">Racine servant au calcul du chemin relatif.</param>
    /// <returns>Chemin de stockage utilisant les conventions existantes, sans lecture ni déplacement du contenu.</returns>
    string? StorePath(string? path, string pathBase);

    /// <summary>Résout un chemin relatif enregistré depuis sa base de stockage.</summary>
    /// <param name="path">Chemin stocké ; les valeurs vides et les chemins entièrement qualifiés sont conservés.</param>
    /// <param name="pathBase">Racine utilisée pour la résolution.</param>
    /// <returns>Chemin utilisable par le module, sans création ni contrôle de présence du contenu.</returns>
    string? ResolvePath(string? path, string pathBase);

    /// <summary>Écrit un document dans un fichier temporaire puis remplace sa destination, avec nettoyage.</summary>
    Task WriteDocumentAsync<T>(string path, T document, JsonSerializerOptions options, CancellationToken cancellationToken);
    /// <summary>Charge les documents acceptés, dans l’ordre commun, puis résout leurs chemins.</summary>
    Task<IReadOnlyList<T>> ReadDocumentsAsync<T>(string directory, string fileName, Func<string, T?> deserialize,
        Func<T, bool> isSupported, Func<T, T> resolve, CancellationToken cancellationToken) where T : class;
    /// <summary>Charge des documents JSON puis les restaure en configurations au moyen des conversions du module.</summary>
    /// <param name="directory">Racine du magasin dont les sous-dossiers contiennent les documents.</param>
    /// <param name="fileName">Nom invariant du document dans chaque sous-dossier.</param>
    /// <param name="deserialize">Conversion de l’élément JSON emprunté en document du module.</param>
    /// <param name="restore">Restauration du document, avec ses contrôles et sa résolution des chemins.</param>
    /// <param name="cancellationToken">Annulation de l’énumération et des lectures.</param>
    /// <returns>Configurations restaurées dans l’ordre ordinal des chemins, sans les documents illisibles ou invalides.</returns>
    /// <remarks>L’élément JSON reste valide pendant deserialize uniquement. L’implémentation possède
    /// et libère le document JSON ; les callbacks restent empruntés et ne doivent pas conserver cet élément.</remarks>
    Task<IReadOnlyList<TConfiguration>> ReadDocumentsAsync<TDocument, TConfiguration>(string directory, string fileName,
        Func<JsonElement, TDocument> deserialize, Func<TDocument, TConfiguration> restore, CancellationToken cancellationToken);
    /// <summary>Exécute une écriture sous le verrou emprunté et le libère dans finally.</summary>
    Task WithSaveGateAsync(SemaphoreSlim gate, Func<Task> write, CancellationToken cancellationToken);
    /// <summary>Supprime un dossier possédé s’il existe.</summary>
    void DeleteDirectoryIfExists(string path, bool recursive);
}
