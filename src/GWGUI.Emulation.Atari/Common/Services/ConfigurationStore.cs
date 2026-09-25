using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Atari.Common.Services;

public sealed class ConfigurationStore
{
    private readonly string _directory;
    private readonly string _pathBase;
    private static readonly SemaphoreSlim SaveGate = new(1, 1);
    private int _activeLoads;

    public ConfigurationStore(string directory, string? pathBase = null)
    {
        _directory = Path.GetFullPath(
            string.IsNullOrWhiteSpace(directory) ? throw new ArgumentException(nameof(directory)) : directory);
        _pathBase = Path.GetFullPath(pathBase ?? directory);
    }

    public bool IsLoading => Volatile.Read(ref _activeLoads) > ConfigurationStoreConstants.NoActiveLoads;

    public async Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _activeLoads);
        try
        {
            Directory.CreateDirectory(_directory);
            var configurations = new List<MachineConfiguration>();
            foreach (var path in ConfigurationPaths())
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var json = ConfigurationFileAccessFunctions.ReadAllText(path);
                    var document = JsonConfigurationRecoveryFunctions
                        .DeserializeRemovingInvalidProperties(json,
                            ConfigurationMigrationFunctions.MigrateToCurrent,
                            out var repairedJson);
                    if (!string.Equals(json, repairedJson, StringComparison.Ordinal))
                        await JsonConfigurationRecoveryFunctions.WriteAtomicallyAsync(path, repairedJson,
                            cancellationToken).ConfigureAwait(false);
                    configurations.Add(ConfigurationStoreFunctions.FromDocument(document, _pathBase));
                }
                catch (JsonException) { }
                catch (IOException) { }
                catch (InvalidDataException) { }
                catch (ArgumentException) { }
            }
            return configurations;
        }
        finally
        {
            Interlocked.Decrement(ref _activeLoads);
        }
    }

    public async Task SaveAsync(MachineConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        await SaveGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var document = ConfigurationStoreFunctions.ToDocument(configuration, _pathBase);
            var machineDirectory = Path.Combine(_directory,
                configuration.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
            await ConfigurationStoreFunctions.WriteDocumentAtomicallyAsync(
                Path.Combine(machineDirectory, ConfigurationStoreConstants.MachineFileName), document,
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            SaveGate.Release();
        }
    }

    public void Delete(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException(nameof(id));
        var machineDirectory = Path.Combine(_directory,
            id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        if (Directory.Exists(machineDirectory)) Directory.Delete(machineDirectory, recursive: true);
        var legacyPath = Path.Combine(_directory,
            id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)
            + ConfigurationStoreConstants.LegacyFileExtension);
        if (File.Exists(legacyPath)) File.Delete(legacyPath);
    }

    private IEnumerable<string> ConfigurationPaths() => Directory.EnumerateDirectories(_directory)
        .Select(directory => Path.Combine(directory, ConfigurationStoreConstants.MachineFileName))
        .Concat(Directory.EnumerateFiles(_directory, ConfigurationStoreConstants.JsonSearchPattern))
        .Where(File.Exists)
        .Order(StringComparer.OrdinalIgnoreCase);
}
