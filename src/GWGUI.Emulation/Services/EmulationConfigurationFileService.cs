using System.Text.Json;

namespace GWGUI.Emulation.Services;

/// <summary>Adaptation du contrat de persistance aux opérations existantes ; aucun second algorithme d’écriture.</summary>
internal sealed class EmulationConfigurationFileService : IEmulationConfigurationFiles
{
    public Task WriteDocumentAsync<T>(string path, T document, JsonSerializerOptions options, CancellationToken cancellationToken)
        => ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(path, document, options, cancellationToken);
    public Task<IReadOnlyList<T>> ReadDocumentsAsync<T>(string directory, string fileName, Func<string, T?> deserialize,
        Func<T, bool> isSupported, Func<T, T> resolve, CancellationToken cancellationToken) where T : class
        => ConfigurationFileAccessFunctions.ReadDocumentsAsync(directory, fileName, deserialize, isSupported, resolve, cancellationToken);
    public Task WithSaveGateAsync(SemaphoreSlim gate, Func<Task> write, CancellationToken cancellationToken)
        => ConfigurationFileAccessFunctions.WithSaveGateAsync(gate, write, cancellationToken);
    public void DeleteDirectoryIfExists(string path, bool recursive) => ConfigurationFileAccessFunctions.DeleteDirectoryIfExists(path, recursive);
}
