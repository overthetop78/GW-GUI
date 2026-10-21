using System.Text.Json;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Persistance commune : le module conserve son schéma et fournit les conversions de chemins.</summary>
public interface IEmulationConfigurationFiles
{
    /// <summary>Écrit un document dans un fichier temporaire puis remplace sa destination, avec nettoyage.</summary>
    Task WriteDocumentAsync<T>(string path, T document, JsonSerializerOptions options, CancellationToken cancellationToken);
    /// <summary>Charge les documents acceptés, dans l’ordre commun, puis résout leurs chemins.</summary>
    Task<IReadOnlyList<T>> ReadDocumentsAsync<T>(string directory, string fileName, Func<string, T?> deserialize,
        Func<T, bool> isSupported, Func<T, T> resolve, CancellationToken cancellationToken) where T : class;
    /// <summary>Exécute une écriture sous le verrou emprunté et le libère dans finally.</summary>
    Task WithSaveGateAsync(SemaphoreSlim gate, Func<Task> write, CancellationToken cancellationToken);
    /// <summary>Supprime un dossier possédé s’il existe.</summary>
    void DeleteDirectoryIfExists(string path, bool recursive);
}
