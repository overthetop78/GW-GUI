using System.Text.Json;

namespace GWGUI.Emulation.Services;

/// <summary>Adaptation du contrat de persistance aux opérations existantes ; aucun second algorithme d’écriture.</summary>
internal sealed class EmulationConfigurationFileService : IEmulationConfigurationFiles
{
    public Task WriteDocumentAsync<TConfiguration>(SemaphoreSlim saveGate, string directory, TConfiguration configuration, Func<TConfiguration, TConfiguration> prepareConfiguration, Func<TConfiguration, Guid> configurationId, Func<TConfiguration, TConfiguration> storePaths, JsonSerializerOptions options, CancellationToken cancellationToken)
        => ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(saveGate, directory, configuration, prepareConfiguration, configurationId, storePaths, options, cancellationToken);
    public Task<IReadOnlyList<TConfiguration>> ReadDocumentsAsync<TDocument, TConfiguration>(string directory, Func<IEnumerable<string>> paths, Func<JsonElement, TDocument> deserialize, Func<TDocument, TConfiguration> restore, CancellationToken cancellationToken)
        => ConfigurationFileAccessFunctions.ReadDocumentsAsync(directory, paths, deserialize, restore, cancellationToken);
    public IEnumerable<string> EnumerateDocumentPaths(string directory, string fileName) => ConfigurationFileAccessFunctions.EnumerateDocumentPaths(directory, fileName);
    public void DeleteIfExists(string path) => ConfigurationFileAccessFunctions.DeleteIfExists(path);

    public string? StorePath(string? path, string pathBase) => ConfigurationFileAccessFunctions.StorePath(path, pathBase);
    public string? ResolvePath(string? path, string pathBase) => ConfigurationFileAccessFunctions.ResolvePath(path, pathBase);
    public Task WriteDocumentAsync<T>(string path, T document, JsonSerializerOptions options, CancellationToken cancellationToken)
        => ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(path, document, options, cancellationToken);
    public Task<IReadOnlyList<T>> ReadDocumentsAsync<T>(string directory, string fileName, Func<string, T?> deserialize,
        Func<T, bool> isSupported, Func<T, T> resolve, CancellationToken cancellationToken) where T : class
        => ConfigurationFileAccessFunctions.ReadDocumentsAsync(directory, fileName, deserialize, isSupported, resolve, cancellationToken);
    public Task WithSaveGateAsync(SemaphoreSlim gate, Func<Task> write, CancellationToken cancellationToken)
        => ConfigurationFileAccessFunctions.WithSaveGateAsync(gate, write, cancellationToken);

    public Task<IReadOnlyList<TConfiguration>> ReadDocumentsAsync<TDocument, TConfiguration>(string directory, string fileName,
        Func<JsonElement, TDocument> deserialize, Func<TDocument, TConfiguration> restore, CancellationToken cancellationToken)
        => ConfigurationFileAccessFunctions.ReadDocumentsAsync(directory,
            () => ConfigurationFileAccessFunctions.EnumerateDocumentPaths(directory, fileName), deserialize, restore, cancellationToken);
    public void DeleteDirectoryIfExists(string path, bool recursive) => ConfigurationFileAccessFunctions.DeleteDirectoryIfExists(path, recursive);
}
